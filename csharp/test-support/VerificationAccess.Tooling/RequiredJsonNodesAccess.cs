using System.Text.Json.Nodes;

namespace SuperMetroid.Tooling;

/// <summary>Verification access to <see cref="RequiredJsonNodes"/> navigation development tools do not use.</summary>
internal static class RequiredJsonNodesAccess
{
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
