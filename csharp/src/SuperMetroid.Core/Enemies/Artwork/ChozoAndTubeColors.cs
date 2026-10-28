using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Chozo;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed n00b-tube crack and Chozo statue colors: which supplied colors are edits of their calculated stock
/// paint, how each resolves and installs, and the identity of all of them.
/// </summary>
internal static class ChozoAndTubeColors
{
    /// <summary>The authored crack seed inks the rest of the tube image is calculated from.</summary>
    private const int TubeSeedCount = 8;

    /// <summary>Hashes the tube cracks, then the Wrecked Ship and Lower Norfair statues.</summary>
    internal static string ContentIdentity(ChozoAndTubeColorCatalog colors) =>
        SelectedPresentationHash.Create("ChozoAndTubeColorCatalog-v1", content =>
        {
            Span<RgbColor> tube = stackalloc RgbColor[ChozoAndTubeColorDefinitions.ColorCount];
            for (int color = 0; color < tube.Length; color++) tube[color] = colors.ResolveTubeCracks(color);
            content.AppendColors("tubeCracks", tube);
            content.AppendColors("wreckedShip", Statue(colors, ChozoStatuePalette.WreckedShip));
            content.AppendColors("lowerNorfair", Statue(colors, ChozoStatuePalette.LowerNorfair));
        });

    /// <summary>The eight crack seed colors, which are authored paint.</summary>
    internal static RgbColor[] TubeSeeds(RgbColor[] tubeCracks) => tubeCracks[..TubeSeedCount];

    /// <summary>The supplied crack colors after the seeds that differ from the calculated ramp and repeated half.</summary>
    internal static Dictionary<ColourCell, RgbColor> TubeEdits(RgbColor[] tubeCracks)
    {
        RgbColor[] seeds = TubeSeeds(tubeCracks);
        Dictionary<ColourCell, RgbColor> edits = [];
        for (int color = TubeSeedCount; color < tubeCracks.Length; color++)
            if (tubeCracks[color] != TubeColor(seeds, color))
                edits.Add(new ColourCell(0, color), tubeCracks[color]);
        return edits;
    }

    /// <summary>The supplied statue colors that differ from the variant's calculated paint.</summary>
    internal static Dictionary<ColourCell, RgbColor> StatueEdits(ChozoStatuePalette palette, RgbColor[] statue)
    {
        Dictionary<ColourCell, RgbColor> edits = [];
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            if (statue[color] != ChozoStatuePaint.Color(palette, color))
                edits.Add(new ColourCell(0, color), statue[color]);
        return edits;
    }

    /// <summary>A tube-crack ink: its edit, or the calculated stock color.</summary>
    internal static RgbColor TubeCrack(ChozoAndTubeColorCatalog colors, int color)
    {
        if ((uint)color >= ChozoAndTubeColorDefinitions.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.tubeColorEdits.TryGetValue(new ColourCell(0, color), out RgbColor edited)
            ? edited
            : TubeColor(colors.tubeColorSeeds, color);
    }

    /// <summary>$AA:E2DD: two equal16-color halves; colors8..15 linearly shade RGB5(31,27,29) to(0,0,1).</summary>
    private static RgbColor TubeColor(RgbColor[] seeds, int color)
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
        Dictionary<ColourCell, RgbColor> edits = palette == ChozoStatuePalette.WreckedShip ? colors.wreckedShipEdits : colors.lowerNorfairEdits;
        return edits.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : ChozoStatuePaint.Color(palette, color);
    }

    /// <summary>Expands a statue variant into its ordered 32-color CGRAM image for identity hashing.</summary>
    private static RgbColor[] Statue(ChozoAndTubeColorCatalog colors, ChozoStatuePalette palette) =>
        Enumerable.Range(0, ChozoAndTubeColorDefinitions.ColorCount).Select(color => Statue(colors, palette, color)).ToArray();

    /// <summary>Writes the tube-crack image's 32 inks.</summary>
    internal static void ApplyTubeCracks(ChozoAndTubeColorCatalog colors, ColourRun destination)
    {
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            destination.Write(color, colors.ResolveTubeCracks(color));
    }

    /// <summary>Writes the selected statue's 32 inks into the shared boss OBJ palette range.</summary>
    internal static void ApplyStatue(ChozoAndTubeColorCatalog colors, ColourRun destination, ChozoStatuePalette palette)
    {
        for (int color = 0; color < ChozoAndTubeColorDefinitions.ColorCount; color++)
            destination.Write(color, Statue(colors, palette, color));
    }
}
