using System.Text.Json.Serialization;

namespace NetwrixAuditorMCPServer.Configuration;

public class ClaudeDesktopConfig
{
    [JsonPropertyName("mcpServers")]
    public Dictionary<string, McpServerConfig>? McpServers { get; set; }
}

public class McpServerConfig
{
    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }

    // Can add other fields if they're ever needed
    // [JsonPropertyName("command")]
    // public string? Command { get; set; }
    // [JsonPropertyName("args")]
    // public List<string>? Args { get; set; }
}