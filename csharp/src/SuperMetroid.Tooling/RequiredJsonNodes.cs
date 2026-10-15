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

}
