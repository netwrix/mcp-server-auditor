namespace NetwrixAuditorMCPServer.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Defines a working hours filter.
/// </summary>
public class WorkingHoursFilter
{
    /// <summary>
    /// Start of working hours interval.
    /// Format: "HH:MM:SSZ" (UTC) or with timezone offset "+HH:MM" / "-HH:MM".
    /// </summary>
    [JsonPropertyName("From")]
    [Required(ErrorMessage = "The 'From' field for WorkingHours is required.")]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// End of working hours interval.
    /// Format: "HH:MM:SSZ" (UTC) or with timezone offset "+HH:MM" / "-HH:MM".
    /// </summary>
    [JsonPropertyName("To")]
    [Required(ErrorMessage = "The 'To' field for WorkingHours is required.")]
    public string To { get; set; } = string.Empty;
}