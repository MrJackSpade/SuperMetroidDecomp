using System.Text.Json.Nodes;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Begins a data file from what its type in the Godot project reads from it.</summary>
internal static partial class EngineDefaults
{
    /// <summary>
    /// Gives <paramref name="data"/> every value the Godot type <paramref name="type"/> reads by name, its own and
    /// those of the types it builds on, each at its default. Values already there are left as they are.
    /// </summary>
    /// <exception cref="InvalidDataException">The Godot project has no such type that reads values.</exception>
    internal static void Fill(JsonObject data, string type)
    {
        bool known = false;
        for (string? named = type; named is not null; named = Bases.GetValueOrDefault(named))
        {
            if (!Own.TryGetValue(named, out (string Name, string Json)[]? values))
                continue;
            known = true;
            foreach ((string name, string json) in values)
            {
                if (!data.ContainsKey(name))
                    data[name] = JsonNode.Parse(json);
            }
        }

        if (!known && !Bases.ContainsKey(type))
            throw new InvalidDataException($"The Godot project has no type named {type} that reads values from a data file.");
    }
}
