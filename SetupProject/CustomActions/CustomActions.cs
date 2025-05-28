using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WixToolset.Dtf.WindowsInstaller;

namespace NetwrixAuditorMCPServer.Installer
{
    public class CustomActions
    {
        private static string ProtectPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return password;
                
            try
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] protectedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.LocalMachine);
                return Convert.ToBase64String(protectedBytes);
            }
            catch
            {
                // If protection fails, return original password
                return password;
            }
        }
        [CustomAction]
        public static ActionResult CheckClaudeDesktopConfig(Session session)
        {
            try
            {
                session.Log("=== Starting Claude Desktop detection ===");
                
                // First check for Claude.exe to see if Claude Desktop is installed
                bool claudeExeFound = false;
                
                // Check AnthropicClaude directory
                string anthropicClaudeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AnthropicClaude");
                if (Directory.Exists(anthropicClaudeDir))
                {
                    session.Log($"Found AnthropicClaude directory at: {anthropicClaudeDir}");
                    
                    // Check for claude.exe directly in AnthropicClaude
                    string directClaudePath = Path.Combine(anthropicClaudeDir, "claude.exe");
                    if (File.Exists(directClaudePath))
                    {
                        claudeExeFound = true;
                        session.Log($"✓ Found claude.exe at: {directClaudePath}");
                    }
                    else
                    {
                        // Check in app-* subdirectories
                        var appDirs = Directory.GetDirectories(anthropicClaudeDir, "app-*");
                        foreach (var appDir in appDirs)
                        {
                            string claudePath = Path.Combine(appDir, "claude.exe");
                            session.Log($"Checking for claude.exe at: {claudePath}");
                            if (File.Exists(claudePath))
                            {
                                claudeExeFound = true;
                                session.Log($"✓ Found claude.exe at: {claudePath}");
                                break;
                            }
                        }
                    }
                }
                
                // Also check other possible locations
                if (!claudeExeFound)
                {
                    string[] claudeExePaths = new string[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "claude-desktop", "Claude.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Claude", "Claude.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Claude", "Claude.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Claude", "Claude.exe")
                    };
                    
                    foreach (var exePath in claudeExePaths)
                    {
                        session.Log($"Checking for Claude.exe at: {exePath}");
                        if (File.Exists(exePath))
                        {
                            claudeExeFound = true;
                            session.Log($"✓ Found Claude.exe at: {exePath}");
                            break;
                        }
                    }
                }
                
                // Check multiple possible locations for Claude Desktop config
                string[] possiblePaths = new string[]
                {
                    // User's AppData/Roaming
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Claude", "claude_desktop_config.json"),
                    // User's AppData/Local
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Claude", "claude_desktop_config.json"),
                    // User's home directory
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude", "claude_desktop_config.json")
                };
                
                string configPath = null;
                bool configExists = false;
                foreach (var path in possiblePaths)
                {
                    session.Log($"Checking for Claude config at: {path}");
                    if (File.Exists(path))
                    {
                        configPath = path;
                        configExists = true;
                        session.Log($"✓ Found Claude config at: {path}");
                        break;
                    }
                }
                
                if (configPath == null)
                {
                    // Use default path if none found
                    configPath = possiblePaths[0];
                    session.Log($"No config found, using default path: {configPath}");
                }
                
                // Consider Claude installed if either exe or config exists
                bool claudeInstalled = claudeExeFound || configExists;
                
                session["CLAUDE_CONFIG_EXISTS"] = claudeInstalled ? "1" : "0";
                session["CLAUDE_CONFIG_PATH"] = configPath;
                
                // Set the status text property
                if (claudeInstalled)
                {
                    session["CLAUDE_STATUS_TEXT"] = "✓ Claude Desktop: Installed";
                }
                else
                {
                    session["CLAUDE_STATUS_TEXT"] = "⚠ Claude Desktop: NOT FOUND\r\n   Install Claude Desktop for automatic configuration";
                }
                
                session.Log($"=== Claude Desktop detection complete ===");
                session.Log($"Claude installed: {claudeInstalled} (exe: {claudeExeFound}, config: {configExists})");
                session.Log($"CLAUDE_CONFIG_EXISTS property set to: {session["CLAUDE_CONFIG_EXISTS"]}");
                session.Log($"CLAUDE_STATUS_TEXT property set to: {session["CLAUDE_STATUS_TEXT"]}");
                
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log($"Error checking Claude Desktop config: {ex.Message}");
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult CheckNetwrixAuditorService(Session session)
        {
            try
            {
                session.Log("=== Starting Netwrix Auditor detection ===");
                bool serviceExists = false;
                bool directoryExists = false;
                
                try
                {
                    // Check for Netwrix Auditor service
                    session.Log("Checking for Netwrix Auditor service...");
                    ServiceController[] services = ServiceController.GetServices();
                    session.Log($"Found {services.Length} services to check");
                    
                    foreach (var service in services)
                    {
                        if (service.ServiceName.Contains("Netwrix") || service.ServiceName == "NwWebAPISvc")
                        {
                            session.Log($"Found potential Netwrix service: {service.ServiceName}, Status: {service.Status}");
                            if (service.ServiceName == "NwWebAPISvc")
                            {
                                serviceExists = true;
                                session.Log($"✓ Found NwWebAPISvc service!");
                                break;
                            }
                        }
                    }
                    
                    if (!serviceExists)
                    {
                        session.Log("NwWebAPISvc service not found");
                    }
                }
                catch (Exception ex)
                {
                    session.Log($"Exception checking for Netwrix service: {ex.Message}");
                    serviceExists = false;
                }
                
                // Also check if Netwrix Auditor is installed by looking for its directory
                session.Log("Checking for Netwrix Auditor directories...");
                string[] possiblePaths = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Netwrix Auditor"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Netwrix Auditor"),
                    @"C:\Program Files\Netwrix Auditor",
                    @"C:\Program Files (x86)\Netwrix Auditor",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Netwrix"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Netwrix")
                };
                
                foreach (var path in possiblePaths)
                {
                    session.Log($"Checking directory: {path}");
                    if (Directory.Exists(path))
                    {
                        directoryExists = true;
                        session.Log($"✓ Found Netwrix directory at: {path}");
                        
                        // Check for specific Netwrix executables
                        string[] exeFiles = { "Netwrix.exe", "NwWebAPI.exe", "NwAuditor.exe" };
                        foreach (var exe in exeFiles)
                        {
                            string exePath = Path.Combine(path, exe);
                            if (File.Exists(exePath))
                            {
                                session.Log($"✓ Found {exe} at: {exePath}");
                            }
                        }
                        break;
                    }
                }
                
                // Consider Netwrix installed if either service or directory exists
                bool netwrixInstalled = serviceExists || directoryExists;
                
                session["NETWRIX_AUDITOR_INSTALLED"] = netwrixInstalled ? "1" : "0";
                
                // Set the status text property
                if (netwrixInstalled)
                {
                    session["NETWRIX_STATUS_TEXT"] = "✓ Netwrix Auditor: Installed";
                }
                else
                {
                    session["NETWRIX_STATUS_TEXT"] = "⚠ Netwrix Auditor: NOT FOUND\r\n   Install Netwrix Auditor for MCP server to work";
                }
                
                session.Log($"=== Netwrix Auditor detection complete ===");
                session.Log($"Netwrix installed: {netwrixInstalled} (service: {serviceExists}, directory: {directoryExists})");
                session.Log($"NETWRIX_AUDITOR_INSTALLED property set to: {session["NETWRIX_AUDITOR_INSTALLED"]}");
                session.Log($"NETWRIX_STATUS_TEXT property set to: {session["NETWRIX_STATUS_TEXT"]}");
                
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log($"Error checking Netwrix Auditor service: {ex.Message}");
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult UpdateClaudeDesktopConfig(Session session)
        {
            try
            {
                // In deferred custom actions, we need to get data from CustomActionData
                string customActionData = session.CustomActionData.ToString();
                var dataValues = new Dictionary<string, string>();
                
                foreach (var pair in customActionData.Split(';'))
                {
                    if (!string.IsNullOrEmpty(pair))
                    {
                        var keyValue = pair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            dataValues[keyValue[0]] = keyValue[1];
                        }
                    }
                }
                
                string configPath = dataValues.ContainsKey("CLAUDE_CONFIG_PATH") ? dataValues["CLAUDE_CONFIG_PATH"] : "";
                string installDir = dataValues.ContainsKey("INSTALLFOLDER") ? dataValues["INSTALLFOLDER"] : "";
                string apiUrl = dataValues.ContainsKey("NETWRIX_API_URL") ? dataValues["NETWRIX_API_URL"] : "";
                string apiUsername = dataValues.ContainsKey("NETWRIX_API_USERNAME") ? dataValues["NETWRIX_API_USERNAME"] : "";
                string apiPassword = dataValues.ContainsKey("NETWRIX_API_PASSWORD") ? dataValues["NETWRIX_API_PASSWORD"] : "";
                // string internalApi = dataValues.ContainsKey("NETWRIX_INTERNAL_API") ? dataValues["NETWRIX_INTERNAL_API"] : "false";
                
                // Load existing config or create new one
                JObject config;
                if (File.Exists(configPath))
                {
                    string configContent = File.ReadAllText(configPath);
                    config = JObject.Parse(configContent);
                }
                else
                {
                    config = new JObject();
                }
                
                // Ensure mcpServers exists
                if (config["mcpServers"] == null)
                {
                    config["mcpServers"] = new JObject();
                }
                
                // Create the directory if it doesn't exist
                string configDir = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                    session.Log($"Created directory: {configDir}");
                }
                
                JObject mcpServers = config["mcpServers"] as JObject;
                
                // Add our MCP server configuration
                JObject serverConfig = new JObject
                {
                    ["command"] = Path.Combine(installDir, "NetwrixAuditorMCPServer.exe"),
                    ["args"] = new JArray(),
                    ["env"] = new JObject
                    {
                        ["NETWRIX_API_URL"] = apiUrl,
                        ["NETWRIX_API_USERNAME"] = apiUsername,
                        ["NETWRIX_API_PASSWORD"] = ProtectPassword(apiPassword),
                        ["NETWRIX_API_PASSWORD_PROTECTED"] = "true"
                        // ["NETWRIX_INTERNAL_API"] = internalApi
                    }
                };
                
                mcpServers["netwrix-auditor-local"] = serverConfig;
                
                // Save the updated config
                string updatedConfig = config.ToString(Formatting.Indented);
                File.WriteAllText(configPath, updatedConfig);
                
                session.Log("Claude Desktop config updated successfully");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log($"Error updating Claude Desktop config: {ex.Message}");
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult RestartClaudeDesktop(Session session)
        {
            try
            {
                // Kill Claude Desktop process if running
                var processes = System.Diagnostics.Process.GetProcessesByName("Claude");
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                    catch { }
                }
                
                // Try to find and start Claude Desktop
                string claudePath = null;
                
                // First check AnthropicClaude directory
                string anthropicClaudeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AnthropicClaude");
                if (Directory.Exists(anthropicClaudeDir))
                {
                    // Check for claude.exe directly
                    string directPath = Path.Combine(anthropicClaudeDir, "claude.exe");
                    if (File.Exists(directPath))
                    {
                        claudePath = directPath;
                    }
                    else
                    {
                        // Check in app-* subdirectories (get the latest version)
                        var appDirs = Directory.GetDirectories(anthropicClaudeDir, "app-*");
                        if (appDirs.Length > 0)
                        {
                            // Sort to get the latest version
                            Array.Sort(appDirs);
                            Array.Reverse(appDirs);
                            
                            foreach (var appDir in appDirs)
                            {
                                string appClaudePath = Path.Combine(appDir, "claude.exe");
                                if (File.Exists(appClaudePath))
                                {
                                    claudePath = appClaudePath;
                                    break;
                                }
                            }
                        }
                    }
                }
                
                // If not found, check other locations
                if (claudePath == null)
                {
                    string[] possibleClaudePaths = new string[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "claude-desktop", "Claude.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Claude", "Claude.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Claude", "Claude.exe")
                    };
                    
                    foreach (var path in possibleClaudePaths)
                    {
                        if (File.Exists(path))
                        {
                            claudePath = path;
                            break;
                        }
                    }
                }
                
                if (claudePath != null)
                {
                    System.Diagnostics.Process.Start(claudePath);
                    session.Log($"Claude Desktop restarted successfully from: {claudePath}");
                }
                else
                {
                    session.Log("Claude Desktop executable not found in any expected location");
                }
                
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log($"Error restarting Claude Desktop: {ex.Message}");
                return ActionResult.Success; // Return success even if restart fails
            }
        }
    }
}