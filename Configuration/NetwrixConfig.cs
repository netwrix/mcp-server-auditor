using System.Net;

namespace NetwrixAuditorMCPServer.Configuration;

/// <summary>
/// Configuration for Netwrix Auditor API connection.
/// </summary>
public class NetwrixConfig
{
    /// <summary>
    /// The base URL for the Netwrix Auditor API.
    /// </summary>
    public string ApiUrl { get; set; } = "https://localhost:9699";
    
    /// <summary>
    /// Credentials for authenticating with the Netwrix Auditor API.
    /// </summary>
    public NetworkCredential? Credentials { get; set; }

    public bool IsInternalApi { get; set; } = false;
}
