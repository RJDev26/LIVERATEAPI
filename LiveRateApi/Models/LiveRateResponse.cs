using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiveRateApi.Models;

public sealed record LiveRateResponse(
    [property: JsonPropertyName("IsSuccess")] bool IsSuccess,
    [property: JsonPropertyName("LastUpdatedDateTime")] DateTimeOffset? LastUpdatedDateTime,
    [property: JsonPropertyName("Data")] IReadOnlyList<JsonElement> Data,
    [property: JsonPropertyName("Message"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Message = null);
