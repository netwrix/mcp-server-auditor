using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NetwrixAuditorMCPServer.Configuration;

public interface IConfigurationService
{
    NetwrixConfig GetNetwrixConfig();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;

    private static readonly string ClaudeConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Claude",
        "claude_desktop_config.json"
    );

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    private string UnprotectPassword(string protectedPassword)
    {
        if (string.IsNullOrEmpty(protectedPassword))
            return protectedPassword;
            
        try
        {
            byte[] protectedBytes = Convert.FromBase64String(protectedPassword);
            byte[] passwordBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(passwordBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to unprotect password, assuming it's not protected");
            // If unprotection fails, assume password is not protected
            return protectedPassword;
        }
    }

    /// <summary>
    /// Gets Netwrix API configuration with the following priority:
    /// 1. Values (URL, Credentials, InternalAPI) from claude_desktop_config.json (env section netwrixAuditor).
    /// 2. Values from appsettings.json (via IConfiguration).
    /// Environment variables are NOT used.
    /// </summary>
    public NetwrixConfig GetNetwrixConfig()
    {
        _logger.LogInformation("=== Starting Netwrix Configuration Loading ===");
        
        var config = new NetwrixConfig();
        NetworkCredential? claudeCredentials = null;
        string? claudeApiUrl = null;
        bool? claudeInternalApiFlag = null;
        
        // Try different possible server names
        string[] possibleServerNames = { "netwrix-auditor-local", "netwrixAuditor", "netwrix-auditor" };

        // 1. Try to load from claude_desktop_config.json
        _logger.LogInformation("Claude config path: {Path}", ClaudeConfigPath);
        _logger.LogInformation("File exists: {Exists}", File.Exists(ClaudeConfigPath));
        try
        {
            var claudeDesktopConfig = TryLoadClaudeConfig();
            if (claudeDesktopConfig?.McpServers != null)
            {
                _logger.LogInformation("Found {Count} MCP servers in configuration", claudeDesktopConfig.McpServers.Count);
                foreach (var server in claudeDesktopConfig.McpServers)
                {
                    _logger.LogInformation("  - Server: {ServerName}", server.Key);
                }
                
                McpServerConfig? netwrixServerConfig = null;
                string? foundServerName = null;
                
                _logger.LogInformation("Looking for Netwrix server in: {ServerNames}", string.Join(", ", possibleServerNames));
                
                foreach (var serverName in possibleServerNames)
                {
                    if (claudeDesktopConfig.McpServers.TryGetValue(serverName, out netwrixServerConfig))
                    {
                        foundServerName = serverName;
                        _logger.LogInformation("✓ Found server configuration under name: {ServerName}", serverName);
                        break;
                    }
                    else
                    {
                        _logger.LogInformation("✗ Server name '{ServerName}' not found", serverName);
                    }
                }
                
                if (netwrixServerConfig?.Env != null)
            {
                _logger.LogInformation("Found '{ServerName}' section in '{ClaudeConfigPath}'. Attempting to extract env variables.", foundServerName, ClaudeConfigPath);
                var env = netwrixServerConfig.Env;
                if (env.TryGetValue("NETWRIX_API_URL", out var url) && !string.IsNullOrWhiteSpace(url))
                {
                    claudeApiUrl = url;
                    _logger.LogInformation("Found NETWRIX_API_URL in env: {Url}", claudeApiUrl);
                }

                env.TryGetValue("NETWRIX_API_USERNAME", out var username);
                env.TryGetValue("NETWRIX_API_PASSWORD", out var password);
                
                // Check if password is protected
                if (!string.IsNullOrEmpty(password))
                {
                    env.TryGetValue("NETWRIX_API_PASSWORD_PROTECTED", out var isProtected);
                    if (isProtected == "true")
                    {
                        _logger.LogInformation("Password is protected, decrypting...");
                        password = UnprotectPassword(password);
                    }
                }
                
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    claudeCredentials = new NetworkCredential(username, password);
                    _logger.LogInformation("Found NETWRIX_API_USERNAME and NETWRIX_API_PASSWORD in env for user: {Username}", username);
                }
                else if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Only NETWRIX_API_USERNAME or NETWRIX_API_PASSWORD found in env, but not both. Credentials will not be used from this source.");
                }

                // Check for NETWRIX_INTERNAL_API
                if (env.TryGetValue("NETWRIX_INTERNAL_API", out var internalApiValueStr))
                {
                    // Empty string means false, any other value means true
                    claudeInternalApiFlag = !string.IsNullOrEmpty(internalApiValueStr);
                    _logger.LogInformation("Found NETWRIX_INTERNAL_API flag in env: {FlagValue} (raw: '{RawValue}')", 
                        claudeInternalApiFlag, internalApiValueStr);
                }
            }
            }
            else
            {
                _logger.LogInformation("No matching server configuration found in mcpServers section of '{ClaudeConfigPath}'. Tried: {ServerNames}", 
                    ClaudeConfigPath, string.Join(", ", possibleServerNames));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading or parsing '{ClaudeConfigPath}' file", ClaudeConfigPath);
        }

        // 2. Load base configuration from appsettings.json (IConfiguration)
        _logger.LogDebug("Loading Netwrix configuration from IConfiguration (appsettings.json, etc.).");

        _configuration.GetSection("Netwrix").Bind(config);
        _logger.LogInformation("Values from IConfiguration: ApiUrl='{ApiUrl}', IsInternalApi='{IsInternalApi}', Credentials User='{Username}'",
            config.ApiUrl, config.IsInternalApi, config.Credentials?.UserName ?? "<null>");

        // 3. Apply values from Claude over values from IConfiguration
        bool claudeOverride = false;
        if (claudeApiUrl != null)
        {
            if (config.ApiUrl != claudeApiUrl)
            {
                _logger.LogInformation("ApiUrl from '{ClaudeConfigPath}' ({ClaudeUrl}) overrides value from IConfiguration ({ConfigUrl}).", ClaudeConfigPath, claudeApiUrl, config.ApiUrl);
                config.ApiUrl = claudeApiUrl;
                claudeOverride = true;
            }
        }
        if (claudeCredentials != null)
        {
            if (config.Credentials?.UserName != claudeCredentials.UserName)
            {
                _logger.LogInformation("Credentials from '{ClaudeConfigPath}' (user: {ClaudeUser}) override value from IConfiguration (user: {ConfigUser}).",
                    ClaudeConfigPath, claudeCredentials.UserName, config.Credentials?.UserName ?? "<null>");
                config.Credentials = claudeCredentials;
                claudeOverride = true;
            }
            else if (config.Credentials == null)
            {
                _logger.LogInformation("Credentials from '{ClaudeConfigPath}' (user: {ClaudeUser}) set (were absent in IConfiguration).", ClaudeConfigPath, claudeCredentials.UserName);
                config.Credentials = claudeCredentials;
                claudeOverride = true;
            }
        }
        if (claudeInternalApiFlag.HasValue)
        {
            if (config.IsInternalApi != claudeInternalApiFlag.Value)
            {
                _logger.LogInformation("InternalAPI flag from '{ClaudeConfigPath}' ({ClaudeFlag}) overrides value from IConfiguration ({ConfigFlag}).", ClaudeConfigPath, claudeInternalApiFlag.Value, config.IsInternalApi);
                config.IsInternalApi = claudeInternalApiFlag.Value;
                claudeOverride = true;
            }
        }

        if (!claudeOverride)
        {
            _logger.LogInformation("Values from '{ClaudeConfigPath}' not found or do not differ from IConfiguration values.", ClaudeConfigPath);
        }

        // 4. Final checks
        if (config.Credentials == null)
        {
            _logger.LogWarning("Final configuration: Credentials for Netwrix API not found. Authentication may fail.");
        }

        if (string.IsNullOrEmpty(config.ApiUrl))
        {
            _logger.LogWarning("Final configuration: Netwrix API URL not found. API operation is impossible.");
        }

        _logger.LogInformation("Final configuration: ApiUrl='{ApiUrl}', IsInternalApi='{IsInternalApi}', Credentials User='{Username}'",
           config.ApiUrl, config.IsInternalApi, config.Credentials?.UserName ?? "<null>");

        return config;
    }

    /// <summary>
    /// Attempts to load and deserialize configuration from claude_desktop_config.json file.
    /// </summary>
    private ClaudeDesktopConfig? TryLoadClaudeConfig()
    {
        if (!File.Exists(ClaudeConfigPath))
        {
            _logger.LogInformation("Configuration file '{ClaudeConfigPath}' not found.", ClaudeConfigPath);
            return null;
        }

        _logger.LogInformation("Reading configuration file '{ClaudeConfigPath}'", ClaudeConfigPath);
        try
        {
            var json = File.ReadAllText(ClaudeConfigPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Just in case, if mcpServers/env key case might change
            };
            return JsonSerializer.Deserialize<ClaudeDesktopConfig>(json, options);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON deserialization error from file '{ClaudeConfigPath}'", ClaudeConfigPath);
            return null;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "Error reading file '{ClaudeConfigPath}'", ClaudeConfigPath);
            return null;
        }
        catch (UnauthorizedAccessException authEx)
        {
            _logger.LogError(authEx, "Missing access rights to read file '{ClaudeConfigPath}'", ClaudeConfigPath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing file '{ClaudeConfigPath}'", ClaudeConfigPath);
            return null;
        }
    }
}