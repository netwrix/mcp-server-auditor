namespace NetwrixAuditorMCPServer.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Defines a time filter for activity records search.
/// Can specify either a predefined period (e.g., Today) or a date range (From/To).
/// </summary>
public class WhenFilter
{
    [JsonPropertyName("Today")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Today { get; set; }

    [JsonPropertyName("Yesterday")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Yesterday { get; set; }

    [JsonPropertyName("LastSevenDays")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastSevenDays { get; set; }

    [JsonPropertyName("LastThirtyDays")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastThirtyDays { get; set; }

    /// <summary>
    /// Start date and time of the range.
    /// Format: "YYYY-MM-DDTHH:MM:SSZ" (UTC) or with timezone offset "+HH:MM" / "-HH:MM".
    /// </summary>
    [JsonPropertyName("From")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? From { get; set; }

    /// <summary>
    /// End date and time of the range.
    /// Format: "YYYY-MM-DDTHH:MM:SSZ" (UTC) or with timezone offset "+HH:MM" / "-HH:MM".
    /// </summary>
    [JsonPropertyName("To")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? To { get; set; }
}
