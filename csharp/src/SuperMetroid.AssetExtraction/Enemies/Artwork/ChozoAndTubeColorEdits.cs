using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Compares the supplied n00b-tube crack and Chozo statue colours with their calculated stock paint and keeps the crack
/// seeds and only the colours that differ, the selection the game resolves its colours from.
/// </summary>
internal static class ChozoAndTubeColorEdits
{
    /// <summary>Selects the authored crack seeds and the deviations from calculated stock tube and statue colours.</summary>
    internal static ChozoAndTubeColorSelection Select(RgbColor[] tubeCracks, RgbColor[] wreckedShip, RgbColor[] lowerNorfair)
    {
        RgbColor[] seeds = tubeCracks[..ChozoAndTubeColors.TubeSeedCount];
        return new(seeds, TubeEdits(seeds, tubeCracks),
            StatueEdits(ChozoStatuePalette.WreckedShip, wreckedShip),
            StatueEdits(ChozoStatuePalette.LowerNorfair, lowerNorfair));
    }

    /// <summary>The supplied crack colours after the seeds that differ from the calculated ramp and repeated half.</summary>
    private static Dictionary<ColourCell, RgbColor> TubeEdits(RgbColor[] seeds, RgbColor[] tubeCracks)
    {
        Dictionary<ColourCell, RgbColor> edits = [];
        for (int color = ChozoAndTubeColors.TubeSeedCount; color < tubeCracks.Length; color++)
            if (tubeCracks[color] != ChozoAndTubeColors.TubeColor(seeds, color))
                edits.Add(new ColourCell(0, color), tubeCracks[color]);
        return edits;
    }

    /// <summary>The supplied statue colours that differ from the variant's calculated paint.</summary>
    private static Dictionary<ColourCell, RgbColor> StatueEdits(ChozoStatuePalette palette, RgbColor[] statue)
    {
        Dictionary<ColourCell, RgbColor> edits = [];
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            if (statue[color] != ChozoStatuePaint.Color(palette, color))
                edits.Add(new ColourCell(0, color), statue[color]);
        return edits;
    }
}
