using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetwrixAuditorMCPServer.Configuration;
using NetwrixAuditorMCPServer.Tools;
using Serilog;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // --- Configuration for bootstrap logging ---
        var bootstrapConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        EnsureLogDirectoryExists(bootstrapConfig);

        // Configure static bootstrap logger
        // For single-file deployment, configure Serilog explicitly instead of from configuration
        var logPath = bootstrapConfig["Serilog:WriteTo:0:Args:path"] ?? "logs/log-.log";
        var minimumLevel = bootstrapConfig["Serilog:MinimumLevel:Default"] ?? "Information";
        
        var logLevel = minimumLevel switch
        {
            "Verbose" => Serilog.Events.LogEventLevel.Verbose,
            "Debug" => Serilog.Events.LogEventLevel.Debug,
            "Information" => Serilog.Events.LogEventLevel.Information,
            "Warning" => Serilog.Events.LogEventLevel.Warning,
            "Error" => Serilog.Events.LogEventLevel.Error,
            "Fatal" => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            // Removed console logging for MCP compatibility - stdout must be reserved for JSON-RPC
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        ILogger<Program>? hostLogger = null;
        try
        {
            Log.Information("--- Starting Netwrix Auditor MCP Server Configuration ---");
            Log.Information("Server Version: 1.0.0.0");
            Log.Information("Working Directory: {WorkingDirectory}", Directory.GetCurrentDirectory());
            Log.Information("User: {UserName}@{MachineName}", Environment.UserName, Environment.MachineName);
            Log.Information("Process ID: {ProcessId}", Environment.ProcessId);
            Log.Information("CLR Version: {CLRVersion}", Environment.Version);

            var builder = Host.CreateApplicationBuilder(args);

            // --- Configure Host ---

            // 1. Configuration: Add full configuration sources
            builder.Configuration.SetBasePath(AppContext.BaseDirectory);

            // 2. Logging: Clear default providers and integrate Serilog fully
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, dispose: false); // `dispose: false` because we handle CloseAndFlush manually

            Log.Information("Host builder created. Configuring services...");

            // 3. Services: Register application services
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

            // 4. Load NetwrixConfig using a temporary scope
            NetwrixConfig netwrixConfig;
            using (var tempSp = builder.Services.BuildServiceProvider())
            {
                var cfgSvc = tempSp.GetRequiredService<IConfigurationService>();
                var logger = tempSp.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Loading Netwrix configuration...");
                netwrixConfig = cfgSvc.GetNetwrixConfig();
                logger.LogInformation("Netwrix configuration loaded. API URL: {ApiUrl}, IsInternal: {IsInternal}",
                    string.IsNullOrEmpty(netwrixConfig.ApiUrl) ? "[Not Set]" : netwrixConfig.ApiUrl,
                    netwrixConfig.IsInternalApi);
            }

            // 5. Configure HttpClient
            builder.Services.AddHttpClient("NetwrixClient", (serviceProvider, client) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                if (string.IsNullOrEmpty(netwrixConfig.ApiUrl))
                {
                    const string errorMsg = "Netwrix API URL is not configured in appsettings.json. Cannot configure HttpClient.";
                    logger.LogError(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                try
                {
                    var baseUri = new Uri(netwrixConfig.ApiUrl.TrimEnd('/') + "/netwrix/api/v1/");
                    logger.LogInformation("Setting Netwrix API base address: {BaseUri}", baseUri);
                    client.BaseAddress = baseUri;
                    client.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue("netwrix-auditor-mcp", "1.0"));
                }
                catch (UriFormatException ex)
                {
                    logger.LogError(ex, "Invalid Netwrix API URL format: {ApiUrl}", netwrixConfig.ApiUrl);
                    throw new InvalidOperationException($"Invalid Netwrix API URL format: {netwrixConfig.ApiUrl}", ex);
                }
            })
            .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<HttpClientHandler>>();
                // Use Serilog's static logger here as it's configured early
                Log.Debug("Configuring primary HTTP message handler for NetwrixClient.");

                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    // WARNING: Bypassing server certificate validation is insecure.
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                Log.Warning("Server certificate validation is currently bypassed for NetwrixClient. Ensure this is acceptable for the environment.");

                if (netwrixConfig.Credentials != null && !string.IsNullOrEmpty(netwrixConfig.Credentials.UserName))
                {
                    Log.Information("Configuring HttpClientHandler with network credentials for user: {User}", netwrixConfig.Credentials.UserName);
                    handler.Credentials = netwrixConfig.Credentials;
                    handler.PreAuthenticate = true;
                }
                else
                {
                    Log.Warning("No Netwrix API credentials provided in configuration. HttpClient will operate without authentication.");
                }
                return handler;
            });

            // 6. Configure MCP Server
            Log.Information("Configuring MCP server with STDIO transport. IsInternalApi = {IsInternalApi}", netwrixConfig.IsInternalApi);
            var mcpBuilder = builder.Services
                .AddMcpServer(options =>
                {
                    Log.Debug("MCP Server options configured.");
                })
                .WithStdioServerTransport();

            // Register tools based on configuration
            if (netwrixConfig.IsInternalApi)
            {
                Log.Information("Registering all MCP tools found in the current assembly.");
                mcpBuilder.WithToolsFromAssembly();
            }
            else
            {
                Log.Information("Registering specific MCP tool: {ToolName}", nameof(ActivityRecordsTools));
                mcpBuilder.WithTools<ActivityRecordsTools>();
            }

            // --- Build and Run Host ---
            Log.Information("Building the host...");
            var app = builder.Build();

            // Get logger after host is built
            hostLogger = app.Services.GetService<ILogger<Program>>();
            hostLogger?.LogInformation("Host built successfully. Starting application run...");
            
            Log.Information("=== Netwrix Auditor MCP Server Started Successfully ===");
            Log.Information("Server is ready to accept requests from Claude Desktop");

            await app.RunAsync();

            hostLogger?.LogInformation("Application run loop finished.");
            Log.Information("=== Netwrix Auditor MCP Server Shutting Down ===");
            return 0;
        }
        catch (Exception ex)
        {
            if (hostLogger != null)
            {
                hostLogger.LogCritical(ex, "Application terminated unexpectedly during setup or runtime.");
            }
            else
            {
                Log.Fatal(ex, "Application terminated unexpectedly during setup or runtime.");
            }

            // Log to Console.Error as a last resort if logging itself fails
            Console.Error.WriteLine($"FATAL ERROR: {ex}");
            return 1;
        }
        finally
        {
            if (hostLogger != null)
            {
                 hostLogger.LogInformation("--- Shutting down Netwrix Auditor MCP Server ---");
            }
            else
            {
                 Log.Information("--- Shutting down Netwrix Auditor MCP Server ---");
            }

            await Log.CloseAndFlushAsync();
        }
    }

    private static void EnsureLogDirectoryExists(IConfiguration config)
    {
        try
        {
            var fileSinks = config.GetSection("Serilog:WriteTo").GetChildren()
                .Where(s => string.Equals(s.GetValue<string>("Name"), "File", StringComparison.OrdinalIgnoreCase));

            foreach (var sink in fileSinks)
            {
                var path = sink.GetValue<string>("Args:path");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (!Path.IsPathRooted(path))
                    {
                        path = Path.Combine(AppContext.BaseDirectory, path);
                    }

                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    {
                        Log.Information("Attempting to create log directory: {Directory}", dir);
                        Directory.CreateDirectory(dir);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error ensuring log directory exists.");
            Console.Error.WriteLine($"Error ensuring log directory exists: {ex.Message}");
        }
    }
}