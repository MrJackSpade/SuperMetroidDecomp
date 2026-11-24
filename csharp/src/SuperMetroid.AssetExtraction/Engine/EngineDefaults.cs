using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Begins a data file from what its type in the Godot project reads from it, and checks it against the same before it
/// is written.
/// </summary>
internal static partial class EngineDefaults
{
    /// <summary>The Godot type each data file begun here is for.</summary>
    private static readonly ConditionalWeakTable<JsonObject, string> Begun = [];

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
        Begun.AddOrUpdate(data, type);
    }

    /// <summary>
    /// Checks a data file that was begun by <see cref="Fill"/> before it is written: every value its type reads by
    /// name is there and of the kind the type reads it as. The Godot game refuses a file that lacks a value, gives a
    /// fraction where it reads a whole number, or gives nothing where it reads text; this refuses it first, and says
    /// which value. A file not begun by <see cref="Fill"/> is not a type's file and is not checked.
    /// </summary>
    /// <exception cref="InvalidDataException">A value is missing or of the wrong kind.</exception>
    internal static void Check(string file, JsonObject data)
    {
        if (!Begun.TryGetValue(data, out string? type))
            return;
        for (string? named = type; named is not null; named = Bases.GetValueOrDefault(named))
        {
            if (!Own.TryGetValue(named, out (string Name, string Json)[]? values))
                continue;
            foreach ((string name, string json) in values)
            {
                if (!data.TryGetPropertyValue(name, out JsonNode? given))
                    throw new InvalidDataException($"The data file {file} lacks the value {name}, which its type {type} reads.");
                JsonValueKind reads = JsonNode.Parse(json)!.GetValueKind();
                JsonValueKind gives = given?.GetValueKind() ?? JsonValueKind.Null;
                bool fits = reads switch
                {
                    JsonValueKind.Number => gives == JsonValueKind.Number && (IsFraction(json) || !IsFraction(given!.ToJsonString())),
                    JsonValueKind.True or JsonValueKind.False => gives is JsonValueKind.True or JsonValueKind.False,
                    JsonValueKind.String or JsonValueKind.Array => gives == reads,
                    _ => throw new InvalidDataException($"The default of {name} of the type {named} is {json}, which is no kind of value a data file gives."),
                };
                if (!fits)
                {
                    string kind = reads switch
                    {
                        JsonValueKind.Number => IsFraction(json) ? "a number" : "a whole number",
                        JsonValueKind.String => "text",
                        JsonValueKind.Array => "a list",
                        _ => "true or false",
                    };
                    throw new InvalidDataException(
                        $"The data file {file} gives {name} as {given?.ToJsonString() ?? "nothing"}, which its type {type} reads as {kind}.");
                }
            }
        }
    }

    /// <summary>Whether a number is written with a fraction or an exponent, and so is not a whole number as the Godot game reads one.</summary>
    private static bool IsFraction(string number) => number.Contains('.') || number.Contains('e') || number.Contains('E');
}
