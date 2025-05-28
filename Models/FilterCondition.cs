namespace NetwrixAuditorMCPServer.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a single filter condition consisting of an operator and a value.
/// Expected JSON format: { "OperatorName": "Value" }, for example { "Contains": "some text" }.
/// </summary>
public class FilterCondition
{
    /// <summary>
    /// Dynamically captures the "Operator": "Value" pair.
    /// Dictionary key is the operator name (e.g., "Contains", "Equals").
    /// Dictionary value is the value for filtering.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> OperatorAndValue { get; set; } = new();

    /// <summary>
    /// Constructor for convenient creation of a condition with the default operator ("Contains").
    /// </summary>
    /// <param name="containsValue">Value for the Contains operator.</param>
    public FilterCondition(string containsValue)
    {
        // According to documentation, Contains is often the default operator.
        OperatorAndValue["Contains"] = containsValue;
    }

    /// <summary>
    /// Constructor for creating a condition with an explicit operator.
    /// </summary>
    /// <param name="operatorName">Operator name (e.g., "Equals", "DoesNotContain"). 
    /// Must match the list of supported operators for the field.</param>
    /// <param name="value">Value for filtering.</param>
    /// <exception cref="ArgumentException">If the operator name is empty.</exception>
    public FilterCondition(string operatorName, object value)
    {
        if (string.IsNullOrWhiteSpace(operatorName))
            throw new ArgumentException("Operator name cannot be empty.", nameof(operatorName));
        OperatorAndValue[operatorName] = value;
    }

    /// <summary>
    /// Parameterless constructor required for deserialization.
    /// </summary>
    public FilterCondition() { }
}