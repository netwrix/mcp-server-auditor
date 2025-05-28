namespace NetwrixAuditorMCPServer.Models;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Defines a set of filtering criteria for searching activity audit records.
/// </summary>
public class FilterListDefinition
{
    /// <summary>
    /// Filter by activity record ID.
    /// Max length: 49.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("RID")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Rid { get; set; }

    /// <summary>
    /// Filter by user who performed the action.
    /// Max length: 255.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith, InGroup, NotInGroup.
    /// </summary>
    [JsonPropertyName("Who")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Who { get; set; }

    /// <summary>
    /// Filter by resource where the action was performed (server, domain, etc.).
    /// Max length: 255.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("Where")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Where { get; set; }

    /// <summary>
    /// Filter by object type changed (e.g., "user").
    /// Max length: 255.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("ObjectType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? ObjectType { get; set; }

    /// <summary>
    /// Filter by specific object name changed (e.g., "NewPolicy").
    /// Max length: 1073741822.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("What")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? What { get; set; }

    /// <summary>
    /// Filter by data source (e.g., "Active Directory").
    /// Max length: 1073741822.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("DataSource")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? DataSource { get; set; }

    /// <summary>
    /// Filter by monitoring plan name.
    /// Max length: 255.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("MonitoringPlan")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? MonitoringPlan { get; set; }

    /// <summary>
    /// Filter by specific monitoring item with type in brackets.
    /// Max length: 1073741822.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("Item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Item { get; set; }

    /// <summary>
    /// Filter by workstation from which the action was performed.
    /// Max length: 1073741822.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("Workstation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Workstation { get; set; }

    /// <summary>
    /// Filter by "Detail" field content.
    /// Max length: 1073741822.
    /// Supported operators: Contains (default), Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("Detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Detail { get; set; }

    /// <summary>
    /// Filter by "Before" value in details.
    /// Max length: 536870911.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("Before")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Before { get; set; }

    /// <summary>
    /// Filter by "After" value in details.
    /// Max length: 536870911.
    /// Supported operators: Contains (default), DoesNotContain, Equals, NotEqualTo, StartsWith, EndsWith.
    /// </summary>
    [JsonPropertyName("After")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? After { get; set; }

    /// <summary>
    /// Filter by action type (e.g., "Added", "Modified").
    /// Supported operators: Equals (default), NotEqualTo.
    /// </summary>
    [JsonPropertyName("Action")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FilterCondition>? Action { get; set; } // Using FilterCondition, but remember the limited operators

    /// <summary>
    /// Filter by time range. Can contain multiple WhenFilter conditions.
    /// Supported operators: Equals (default), NotEqualTo - applied to the range itself.
    /// </summary>
    [JsonPropertyName("When")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<WhenFilter>? When { get; set; } // Array, as in the example {"When": [{"LastSevenDays": ""}]}

    /// <summary>
    /// Filter by working hours.
    /// Supported operators: Equals (default), NotEqualTo - applied to the interval itself.
    /// </summary>
    [JsonPropertyName("WorkingHours")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WorkingHoursFilter? WorkingHours { get; set; }
}