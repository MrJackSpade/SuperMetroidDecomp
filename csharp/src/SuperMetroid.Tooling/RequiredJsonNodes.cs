using System.Text.Json.Nodes;

namespace SuperMetroid.Tooling;

/// <summary>
/// Navigation of JSON documents whose shape a fixture or asset contract fixes (#623). The System.Text.Json indexers
/// return null for an absent property, an out-of-range index or a JSON <c>null</c>; these fail at the missing path
/// instead, so a contract violation names the exact node.
/// </summary>
public static class RequiredJsonNodes
{
    /// <summary>The value of <paramref name="property"/>, which the document must hold and not set to null.</summary>
    public static JsonNode Required(this JsonNode node, string property) => node[property] ??
        throw new KeyNotFoundException($"JSON {node.GetPath()} has no non-null '{property}'.");

    /// <summary>The element at <paramref name="index"/>, which the array must hold and not set to null.</summary>
    public static JsonNode Required(this JsonNode node, int index) => node[index] ??
        throw new KeyNotFoundException($"JSON {node.GetPath()} has no non-null element {index}.");

    /// <summary>The array's elements, each of which the contract requires to be non-null.</summary>
    public static IEnumerable<JsonNode> RequiredElements(this JsonArray array)
    {
        for (int index = 0; index < array.Count; index++)
            yield return array.Required(index);
    }
}
