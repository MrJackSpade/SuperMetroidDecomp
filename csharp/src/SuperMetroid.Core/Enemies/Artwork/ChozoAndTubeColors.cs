using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed n00b-tube crack and Chozo statue colors: how each resolves from its edit or its calculated stock paint,
/// how each installs, and the identity of all of them.
/// </summary>
internal static class ChozoAndTubeColors
{
    /// <summary>The authored crack seed inks the rest of the tube image is calculated from.</summary>
    internal const int TubeSeedCount = 8;

    /// <summary>A tube-crack ink: its edit, or the calculated stock color.</summary>
    internal static RgbColor TubeCrack(ChozoAndTubeColorCatalog colors, int color)
    {
        if ((uint)color >= ChozoAndTubeColorDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.selection.TubeColorEdits.TryGetValue(new ColourCell(0, color), out RgbColor edited)
            ? edited
            : TubeColor(colors.selection.TubeColorSeeds, color);
    }

    /// <summary>Two equal16-color halves; colors8..15 linearly shade RGB5(31,27,29) to(0,0,1).</summary>
    internal static RgbColor TubeColor(RgbColor[] seeds, int color)
    {
        int local = color % 16;
        if (local < TubeSeedCount) return seeds[local];
        int step = local - TubeSeedCount;
        int red = 31 - (31 * step + 3) / 7;
        int green = 27 - (27 * step + 3) / 7;
        int blue = 29 - 4 * step;
        return new RgbColor(red, green, blue);
    }

    /// <summary>Resolves a statue ink from an authored edit or the selected variant's calculated paint.</summary>
    internal static RgbColor Statue(ChozoAndTubeColorCatalog colors, ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= ChozoAndTubeColorDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        Dictionary<ColourCell, RgbColor> edits = palette == ChozoStatuePalette.WreckedShip ? colors.selection.WreckedShipEdits : colors.selection.LowerNorfairEdits;
        return edits.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : ChozoStatuePaint.Color(palette, color);
    }

    /// <summary>Writes the tube-crack image's 32 inks.</summary>
    internal static void ApplyTubeCracks(ChozoAndTubeColorCatalog colors, ColourRunSequence destination)
    {
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            destination.Write(color, colors.ResolveTubeCracks(color));
    }

    /// <summary>Writes the selected statue's 32 inks into the shared boss OBJ palette range.</summary>
    internal static void ApplyStatue(ChozoAndTubeColorCatalog colors, ColourRunSequence destination, ChozoStatuePalette palette)
    {
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            destination.Write(color, Statue(colors, palette, color));
    }
}
