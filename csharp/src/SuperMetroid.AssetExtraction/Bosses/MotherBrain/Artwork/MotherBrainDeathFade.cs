using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// One of Mother Brain's death fades, as the import selected it. The body/leg channels fade to black as (initial*(15-frame)+1)/15.
/// The corpse channels interpolate between endpoints, rounded to nearest
/// over seven intervals. These rules match every original intermediate color.
/// Starting colors calculate from health state three; the corpse final palette calculates shared
/// drained shade rules from the three narrowly approved drained-paint anchors.
/// Unmatched edited endpoints and frames remain exact and independent of the health document.
/// </summary>
internal sealed class MotherBrainDeathFade
{
    /// <summary>Edited starting colors, or null when the health palette supplies the stock start.</summary>
    internal readonly RgbColor[]? first;
    /// <summary>Selects the back-leg rather than body starting palette.</summary>
    private readonly MotherBrainPalettePart part;
    /// <summary>Corpse fade endpoint; null selects black for the body and leg fades.</summary>
    internal readonly InstalledMotherBrainRainbowPalette.DrainedBodyColors? last;
    /// <summary>Exact edited frames retained when the calculated stock interpolation differs.</summary>
    internal readonly RgbColor[][]? supplied;
    /// <summary>Number of authored palette stages in this fade.</summary>
    internal int FrameCount { get; }
    /// <summary>Opaque colors in each authored stage.</summary>
    internal int ColorCount { get; }

    /// <summary>Keeps a fade's endpoints, its geometry and, when it is not the calculated interpolation, its stages.</summary>
    /// <param name="first">Edited starting colours, or null when the health palette supplies the stock start.</param>
    /// <param name="last">The drained-body colours the corpse fade ends at, or null for a fade that ends at black.</param>
    /// <param name="supplied">Every supplied stage when one differs from the calculation, otherwise null.</param>
    /// <param name="frameCount">The number of stages.</param>
    /// <param name="colorCount">The opaque colours in each stage.</param>
    /// <param name="part">The palette part whose stock starting colours the fade starts from.</param>
    internal MotherBrainDeathFade(RgbColor[]? first, InstalledMotherBrainRainbowPalette.DrainedBodyColors? last,
        RgbColor[][]? supplied, int frameCount, int colorCount, MotherBrainPalettePart part)
    {
        this.first = first;
        this.last = last;
        this.supplied = supplied;
        FrameCount = frameCount;
        ColorCount = colorCount;
        this.part = part;
    }

    /// <summary>Checks authored frame and color bounds before resolving a fade color.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The stage or color index is outside the authored images.</exception>
    internal RgbColor Checked(int frame, int color, string name) =>
        (uint)frame < FrameCount && (uint)color < ColorCount
            ? Resolve(frame, color)
            : throw new ArgumentOutOfRangeException(nameof(frame),
                $"Mother Brain death {name} frame {frame}, color {color} is outside the authored images.");

    /// <summary>Returns an exact edited color when present, otherwise calculates the stock stage.</summary>
    internal RgbColor Resolve(int frame, int color) =>
        supplied is null ? Calculate(frame, color) : supplied[frame][color];

    /// <summary>Interpolates a channel with the black-fade bias or rounded corpse endpoint.</summary>
    private RgbColor Calculate(int frame, int color)
    {
        int steps = FrameCount - 1;
        RgbColor start = first is null ? MotherBrainHealthPalettePresentation.StockDeathStartColor(part, color) : first[color];
        // Without a supplied endpoint the fade ends at black with a one-unit bias.
        RgbColor end = last is null ? RgbColor.Black : last[color];
        int bias = last is null ? 1 : steps / 2;
        return start.Zip(end, (_, from, to) => (from * (steps - frame) + to * frame + bias) / steps);
    }
}
