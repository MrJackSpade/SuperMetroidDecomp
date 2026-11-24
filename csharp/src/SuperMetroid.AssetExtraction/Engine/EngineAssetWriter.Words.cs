using System.Text.Json.Nodes;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The game's words. Whatever writes something that shows words gives them here under the name the Godot game asks
/// for them by, and they are written together as the text file of each language.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The languages the Godot game's options offer. The old game's second language changes a few pictures and none of these words.</summary>
    private static readonly string[] Languages = ["english", "japanese"];

    /// <summary>The words given so far, by name.</summary>
    private readonly SortedDictionary<string, string> _words = new(StringComparer.Ordinal);

    /// <summary>Gives the words the Godot game asks for by <paramref name="name"/>, and gives the name back.</summary>
    /// <exception cref="InvalidOperationException">Other words were already given under the name.</exception>
    private string Words(string name, string words)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(words);
        if (_words.TryGetValue(name, out string? given) && given != words)
            throw new InvalidOperationException($"The words named {name} are given twice, as \"{given}\" and as \"{words}\".");
        _words[name] = words;
        return name;
    }

    /// <summary>Writes the words given, once for each language.</summary>
    private void WriteWords()
    {
        foreach (string language in Languages)
        {
            var data = new JsonObject();
            foreach ((string name, string words) in _words)
                data[name] = words;
            _tree.WriteData($"text/{language}.json", data);
        }
    }
}
