using System.Text.Json;

namespace eDom.Application.Common.Filtering;

public sealed class FilterGroup
{
    public string? Logic { get; set; }
    public List<FilterCondition> Conditions { get; set; } = [];
    public List<FilterGroup>? Groups { get; set; }
}

public sealed class FilterCondition
{
    public string Field { get; set; } = string.Empty;
    public string Op { get; set; } = string.Empty;
    public string? Type { get; set; }
    public JsonElement? Value { get; set; }
    public List<JsonElement>? Values { get; set; }
}
