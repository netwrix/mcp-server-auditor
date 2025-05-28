namespace NetwrixAuditorMCPServer.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Root object for activity audit records search query.
/// </summary>
public class ActivityRecordSearchFilter
{
    /// <summary>
    /// Contains a set of filtering criteria.
    /// </summary>
    [JsonPropertyName("FilterList")]
    [Required(ErrorMessage = "The 'FilterList' object is required.")]
    public FilterListDefinition FilterList { get; set; } = new FilterListDefinition();
}