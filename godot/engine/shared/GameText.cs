using Engine.Content;

namespace Engine.Shared;

/// <summary>
/// The game's words: every message, menu label, narration line and credit, each under a name, in one language.
/// Nothing in the game writes its own words; it asks for them by name.
/// </summary>
/// <remarks><code>{ "missile_tank": "MISSILE TANK", "save_prompt": "Would you like to save?" }</code></remarks>
public sealed class GameText
{
    private readonly string _file;
    private readonly Dictionary<string, string> _texts;

    private GameText(string file, Dictionary<string, string> texts)
    {
        _file = file;
        _texts = texts;
    }

    /// <summary>Reads the words in a language's data file.</summary>
    public static GameText Read(DataNode data) =>
        new(data.File, data.Entries().ToDictionary(entry => entry.Key, entry => entry.Value.AsText(), StringComparer.Ordinal));

    /// <summary>The words under <paramref name="name"/>.</summary>
    /// <exception cref="ContentException">The file has no words under that name.</exception>
    public string Get(string name) =>
        _texts.TryGetValue(name, out string? text) ? text : throw new ContentException(_file, $"has no words named \"{name}\".");
}
