using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ModelContextProtocol.Utils.Json;
using NetwrixAuditorMCPServer.Models;
using NetwrixAuditorMCPServer.Tools.Definitions;

namespace NetwrixAuditorMCPServer.Tools;

[McpServerToolType]
public class ActivityRecordsTools
{
    private readonly ILogger<ActivityRecordsTools> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ActivityRecordsTools(ILogger<ActivityRecordsTools> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public const string Format = "json";

    [McpServerTool, Description("Searches for specific activity audit records in Netwrix Auditor based on provided filter criteria. " +
        "Use this tool when the user wants to find records matching some conditions, such as actions performed by a particular user ('who'), " +
        "changes to a specific object ('what' or 'object_name'), actions on a specific location ('where'), " +
        "or events within a defined time range ('when'). You can combine multiple filters. " +
        "The response may contain a `continuation_mark` if older records exist beyond the initial batch. " +
        "If there're 100 or more Activity Records present it's considered that filter is too general and needs to be more specific")]
    public async Task<string> SearchActivityRecords(
        [Description( ActivityRecordToolsDefinitions.FilterListDescription)]
        ActivityRecordSearchFilter filter,

        [Description("Optional continuation mark from a previous search")]
        string continuationMark = "",

        [Description("Maximum number of records to retrieve (default: 100)")]
        int count = 100)
    {
        _logger.LogInformation("=== SearchActivityRecords tool called ===");
        _logger.LogInformation("Parameters: Count={Count}, ContinuationMark={ContinuationMark}", 
            count, string.IsNullOrEmpty(continuationMark) ? "<empty>" : continuationMark);
        _logger.LogInformation("Filter: {Filter}", JsonSerializer.Serialize(filter));
        
        if (count <= 0)
        {
            _logger.LogWarning("Incorrect parameter `count`: {Count}. Should be greater than 0.", count);
            return "Error: Count should be greater than 0.";
        }

        if (count > 1000)
        {
            _logger.LogWarning("Incorrect parameter `count`: {Count}. Should be greater no than 1000.", count);
            return "Error: Count should be no greater than 1000.";
        }

        var client = _httpClientFactory.CreateClient("NetwrixClient");

        try
        {
            _logger.LogInformation("Searching for activity records. Count: {Count}, HasContinuationMark: {HasContinuationMark}", 
                count, !string.IsNullOrEmpty(continuationMark));
            object payloadToSend;
            if (string.IsNullOrEmpty(continuationMark))
            {
                payloadToSend = filter;
            }
            else
            {
                payloadToSend = new Dictionary<string, object>
                {
                    { "ContinuationMark", continuationMark },
                    { "filterlist", filter.FilterList }
                };
            }

            var requestContent = JsonSerializer.Serialize(payloadToSend, McpJsonUtilities.DefaultOptions);
            _logger.LogDebug("Serialized JSON request: {RequestContent}", requestContent);

            HttpContent content = new StringContent(requestContent, Encoding.UTF8, "application/json");

            var url = "activity_records/search";
            var queryParams = new Dictionary<string, string>
            {
                { "format", Format },
                { "count", count.ToString() }
            };

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            }

            _logger.LogDebug("Request to URL: {RequestUrl}", url);
            _logger.LogInformation("Sending POST request to: {BaseAddress}{Url}", client.BaseAddress, url);
            _logger.LogInformation("Request headers: {Headers}", string.Join(", ", client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
            
            var response = await client.PostAsync(url, content);
            _logger.LogInformation("Response status: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully found activity records.");
                return await FormatActivityRecordsResponse(responseContent);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to find activity records: {StatusCode} - {ReasonPhrase}\n{ErrorContent}",
                response.StatusCode, response.ReasonPhrase, errorContent);

            return $"Error searching for activity records: {response.ReasonPhrase} (Status: {response.StatusCode}). See server logs for details.";
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON error while searching for activity records.");
            return $"Error processing filter data. Check filter format.";
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "Network error while searching for activity records");
            return $"Network error while accessing Netwrix Auditor API: {httpEx.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while searching for activity records");
            return $"Internal server error. {ex}";
        }
    }

    [McpServerTool, Description("Retrieves a batch of random activity audit records from Netwrix Auditor. " +
        "Only use this tool for for sanity check to see if there're any audit records in the connected Netwrix Auditor instance." +
        "Use it with count = 1 in case other tools failed to retrieve any records or continuation mark. ")]
    public async Task<string> RetrieveActivityRecords(
        [Description("Maximum number of records to retrieve (default: 100, max: 100)")] 
        int count = 1)
    {
        _logger.LogInformation("=== RetrieveActivityRecords tool called ===");
        _logger.LogInformation("Parameters: Count={Count}", count);
        
        if (count <= 0)
        {
            _logger.LogWarning("Invalid count parameter provided: {Count}", count);
            return "Error: Count must be greater than 0.";
        }

        var client = _httpClientFactory.CreateClient("NetwrixClient");

        try
        {
            _logger.LogInformation("Retrieving activity records. Count: {Count}", count);

            string url = "activity_records/enum";
            var queryParams = new Dictionary<string, string>
            {
                { "format", Format },
                { "count", count.ToString() }
            };

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            }

            _logger.LogInformation("Sending GET request to: {BaseAddress}{Url}", client.BaseAddress, url);
            _logger.LogInformation("Request headers: {Headers}", string.Join(", ", client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
            
            var response = await client.GetAsync(url);
            _logger.LogInformation("Response status: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
            
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Successfully retrieved activity records");

                return await FormatActivityRecordsResponse(responseContent);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to retrieve activity records: {StatusCode}\n{ErrorContent}",
                    response.StatusCode, errorContent);

                return $"Error retrieving activity records: {response.StatusCode}\n{errorContent}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while retrieving activity records");
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool, Description("Retrieves the subsequent batch of activity audit records for a query previously " +
        "initiated by `RetrieveActivityRecords` or `SearchActivityRecords` or `RetrieveNextActivityRecords`. " +
        "**Only use this tool if the previous response from one of those tools contained a `continuation_mark`**. " +
        "Provide that exact `continuation_mark` to this tool to get the next page of results for the *original* query.")]
    public async Task<string> RetrieveNextActivityRecords(
        [Description("Continuation mark from the previous response. This is required and must be exactly as provided in the previous response.")]
        string continuationMark,

        [Description("Maximum number of records to retrieve (default: 100)")]
        int count = 100)
    {
        _logger.LogInformation("=== RetrieveNextActivityRecords tool called ===");
        _logger.LogInformation("Parameters: Count={Count}, ContinuationMark={ContinuationMark}", 
            count, string.IsNullOrEmpty(continuationMark) ? "<empty>" : continuationMark);
        
        if (string.IsNullOrWhiteSpace(continuationMark))
        {
            _logger.LogWarning("Missing continuation mark");
            return "Error: Continuation mark is required.";
        }

        if (count <= 0)
        {
            _logger.LogWarning("Invalid count parameter provided: {Count}", count);
            return "Error: Count must be greater than 0.";
        }

        var client = _httpClientFactory.CreateClient("NetwrixClient");

        try
        {
            _logger.LogInformation("Retrieving next activity records. Count: {Count}, ContinuationMark: {ContinuationMark}", count, continuationMark);

            var url = $"activity_records/enum";
            var queryParams = new Dictionary<string, string>
            {
                { "format", Format },
                { "count", count.ToString() }
            };

            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            }

            var requestContent = JsonSerializer.Serialize(continuationMark);
            HttpContent content = new StringContent(requestContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending POST request to: {BaseAddress}{Url}", client.BaseAddress, url);
            _logger.LogInformation("Request headers: {Headers}", string.Join(", ", client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
            
            var response = await client.PostAsync(url, content);
            _logger.LogInformation("Response status: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return await FormatActivityRecordsResponse(responseContent);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve next activity records: {StatusCode}\n{ErrorContent}",
                response.StatusCode, errorContent);
            return $"Error retrieving next activity records: {response.StatusCode}\n{errorContent}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while retrieving next activity records");
            return $"Error: {ex.Message}";
        }
    }

    private async Task<string> FormatActivityRecordsResponse(string responseContent)
    {
        var result = new StringBuilder();
        var continuationMark = string.Empty;

        try
        {
            var response = JsonSerializer.Deserialize<JsonElement>(responseContent);
            if (response.TryGetProperty("ContinuationMark", out var mark))
            {
                continuationMark = mark.GetString();
            }

            var activityRecords = new List<JsonElement>();
            if (response.TryGetProperty("ActivityRecordList", out var list) && list.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in list.EnumerateArray())
                {
                    activityRecords.Add(item);
                }
            }

            if (activityRecords.Count == 0)
            {
                return "No activity records found.";
            }

            result.AppendLine($"Found {activityRecords.Count} activity records.");

            foreach (var record in activityRecords)
            {
                result.AppendLine("\n----- Activity Record -----");

                foreach (var prop in record.EnumerateObject())
                {
                    if (prop.Name == "DetailList" || prop.Name == "MonitoringPlan" || prop.Name == "Item")
                    {
                        continue; // These will be handled separately
                    }

                    result.AppendLine($"{prop.Name}: {prop.Value}");
                }

                if (record.TryGetProperty("MonitoringPlan", out var monitoringPlan) &&
                    monitoringPlan.ValueKind == JsonValueKind.Object)
                {
                    result.AppendLine("\nMonitoring Plan:");
                    foreach (var prop in monitoringPlan.EnumerateObject())
                    {
                        result.AppendLine($"  {prop.Name}: {prop.Value}");
                    }
                }

                if (record.TryGetProperty("Item", out var item) &&
                    item.ValueKind == JsonValueKind.Object)
                {
                    result.AppendLine("\nItem:");
                    foreach (var prop in item.EnumerateObject())
                    {
                        result.AppendLine($"  {prop.Name}: {prop.Value}");
                    }
                }

                if (record.TryGetProperty("DetailList", out var detailList) &&
                    detailList.ValueKind == JsonValueKind.Array)
                {
                    result.AppendLine("\nDetails:");
                    foreach (var detail in detailList.EnumerateArray())
                    {
                        result.AppendLine("  Detail:");
                        foreach (var prop in detail.EnumerateObject())
                        {
                            result.AppendLine($"    {prop.Name}: {prop.Value}");
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(continuationMark))
            {
                result.AppendLine("\nNo more records available (end of data).");
            }
            else
            {
                result.AppendLine($"\nContinuation Mark for next page: {continuationMark}");
                result.AppendLine("Use this mark with RetrieveNextActivityRecords to get the next batch of records.");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting response");
            return $"Error formatting response: {ex.Message}\n\nRaw response:\n{responseContent}";
        }
    }
}