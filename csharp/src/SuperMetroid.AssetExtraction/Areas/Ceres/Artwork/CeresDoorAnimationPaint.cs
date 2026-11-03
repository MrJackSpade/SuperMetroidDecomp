using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;

/// <summary>
/// Elevator beacon colors, copied to palette colours 41..46 while the elevator animates.
/// White/rust/red surfaces and three gold facets share normal target paint identities.
/// Mirrored phases use a five-level fade; the brightest gold facets preserve blue
/// while brightening red/green by three, with a four-level green highlight.
/// Only first-seed blue17 and the dimmest middle-amber red14 remain independent
/// paint channels. These and the named brightening/phase policies are selected
/// beacon paint composition, not a claim about pixel art or actor cadence.
/// </summary>
internal sealed class CeresDoorAnimationPaint
{
    /// <summary>Highest value permitted by each five-bit palette channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Channel change between adjacent mirrored beacon fade phases.</summary>
    private const int FadeStep = 5;
    /// <summary>Extra red and green applied to the bright gold facets.</summary>
    private const int GoldBrightening = 3;
    /// <summary>Green boost for the leading gold facet at peak brightness.</summary>
    private const int GoldHighlightGreenBrightening = 4;
    /// <summary>Shared target paint that supplies the six beacon seed colors.</summary>
    private readonly CeresDoorNormalPaint normal;
    /// <summary>Blue channel retained independently for the first seed color.</summary>
    internal readonly int firstSeedBlue;
    /// <summary>Red channel retained independently for the dim middle amber phase.</summary>
    internal readonly int dimMiddleAmberRed;
    /// <summary>Seed colors that differ from the shared normal target paint.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> seedEdits;
    /// <summary>Individual source cells that differ from the calculated mirrored fade.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits;

    /// <summary>Keeps the independent paint channels and per-cell edits of the eight six-color steps of the glow cycle.</summary>
    /// <param name="normal">The shared normal target paint supplying the seed colors.</param>
    /// <param name="firstSeedBlue">The first seed's independent blue.</param>
    /// <param name="dimMiddleAmberRed">The dim middle amber phase's independent red.</param>
    /// <param name="seedEdits">Seed colors the import found differing from the normal target paint.</param>
    /// <param name="edits">Cells the import found differing from the calculated mirrored fade.</param>
    internal CeresDoorAnimationPaint(CeresDoorNormalPaint normal, int firstSeedBlue, int dimMiddleAmberRed,
        Dictionary<ColourCell, RgbColor> seedEdits, Dictionary<ColourCell, RgbColor> edits)
    {
        this.normal = normal;
        this.firstSeedBlue = firstSeedBlue;
        this.dimMiddleAmberRed = dimMiddleAmberRed;
        this.seedEdits = seedEdits;
        this.edits = edits;
    }

    /// <summary>Uses normal target paint for each seed, with the first seed's independent blue channel.</summary>
    internal RgbColor SharedSeed(int color)
    {
        if (color != 0) return normal.ColorAt(8 + color);
        RgbColor normalSeed = normal.ColorAt(8);
        return new(normalSeed.Red, normalSeed.Green, firstSeedBlue);
    }

    /// <summary>Resolves an edited beacon cell or reconstructs its mirrored fade color.</summary>
    internal RgbColor ColorAt(int step, int color)
    {
        if ((uint)step >= 8)
            throw new ArgumentOutOfRangeException(nameof(step));
        if ((uint)color >= 6)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(step, color), out RgbColor edited) ? edited : Calculate(step, color);
    }

    /// <summary>Applies the five-step mirrored fade, peak gold brightening, and dim amber exception.</summary>
    private RgbColor Calculate(int step, int color)
    {
        int phase = Math.Min(step, 7 - step);
        RgbColor seed = seedEdits.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : SharedSeed(color);
        int red = seed.Red, green = seed.Green, blue = seed.Blue;
        if (phase == 0 && color >= 3)
        {
            red = Math.Min(MaximumChannel, red + GoldBrightening);
            green = Math.Min(MaximumChannel, green + (color == 3 ? GoldHighlightGreenBrightening : GoldBrightening));
        }
        else
        {
            int delta = FadeStep * (1 - phase);
            red = Math.Clamp(red + delta, 0, MaximumChannel);
            green = Math.Clamp(green + delta, 0, MaximumChannel);
            blue = Math.Clamp(blue + delta, 0, MaximumChannel);
            if (phase == 3 && color == 4) red = dimMiddleAmberRed;
        }
        return new(red, green, blue);
    }
}
