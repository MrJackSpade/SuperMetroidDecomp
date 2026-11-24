using System.Text.Json.Nodes;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The sets the cartridge's music comes in, of which one is loaded at a time: the set loaded at power-on, and each
/// set's theme, the piece a room plays when it asks for "the theme" of whatever set is loaded.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The name of a set of music, as the Godot game's rooms and data name it.</summary>
    internal static string MusicSetName(MusicDataSet set) => EngineSheets.FileName(set.ToString());

    /// <summary>Writes the set loaded at power-on and every set's theme by the set's name.</summary>
    private void WriteMusicSets()
    {
        var themes = new JsonObject();
        foreach (MusicDataSet set in Enum.GetValues<MusicDataSet>())
            themes[MusicSetName(set)] = MusicName(AreaThemeDefinitions.Themes[set]);

        _tree.WriteData("audio/music_sets.json", new JsonObject
        {
            ["loaded_at_start"] = MusicSetName(AudioQueueDefinitions.PowerOnDataSet),
            ["themes"] = themes,
        });
    }
}
