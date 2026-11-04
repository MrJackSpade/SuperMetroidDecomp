using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The artwork sequences encounters play in the middle of others (#1513).</summary>
internal static class EncounterArtworkInterludes
{
    /// <summary>
    /// Every interlude: Mother Brain's corpse rots once her last corpse page has loaded, and has rotted away before her
    /// escape timer loads, as her death runs its stages one after another.
    /// </summary>
    internal static IReadOnlyList<EncounterArtworkInterlude> All { get; } =
    [
        new(EncounterArtworkSequence.MotherBrainPages, LastCorpsePage, EncounterArtworkSequence.MotherBrainCorpseRot),
    ];

    /// <summary>The step of Mother Brain's sequence that loads her corpse's last page.</summary>
    private static int LastCorpsePage =>
        MotherBrainSpritePageDefinitions.Sequence.ToList().FindLastIndex(page => page.Sheet == MotherBrainSpritePageSheet.Corpse) is var step and >= 0
            ? step : throw new InvalidDataException("Mother Brain's artwork sequence loads no corpse page.");
}
