using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// One of Mother Brain's death fades. $AD:EA0A body/leg channels fade to black as (initial*(15-frame)+1)/15.
/// $AD:F119 corpse channels interpolate between endpoints, rounded to nearest
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
    internal readonly MotherBrainRainbowPalettePresentation.DrainedBodyColors? last;
    /// <summary>Exact edited frames retained when the calculated stock interpolation differs.</summary>
    internal readonly RgbColor[][]? supplied;
    /// <summary>Number of authored palette stages in this fade.</summary>
    internal int FrameCount { get; }
    /// <summary>Opaque colors in each authored stage.</summary>
    internal int ColorCount { get; }

    /// <summary>Detects whether the supplied stages follow stock interpolation or need exact storage.</summary>
    internal MotherBrainDeathFade(RgbColor[][] frames, MotherBrainFadeTarget target, MotherBrainPalettePart part = MotherBrainPalettePart.Body)
    {
        this.part = part;
        ColorCount = frames[0].Length;
        for (int color = 0; color < ColorCount; color++)
            if (frames[0][color] != MotherBrainHealthPalettePresentation.StockDeathStartColor(part, color))
            { first = frames[0]; break; }
        last = target == MotherBrainFadeTarget.Black ? null : new MotherBrainRainbowPalettePresentation.DrainedBodyColors(frames[^1]);
        FrameCount = frames.Length;
        for (int frame = 0; frame < FrameCount; frame++)
        for (int color = 0; color < ColorCount; color++)
        {
            if (frames[frame][color] != Calculate(frame, color))
            {
                supplied = frames;
                return;
            }
        }
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

    /// <summary>Interpolates a channel with the native black-fade bias or rounded corpse endpoint.</summary>
    private RgbColor Calculate(int frame, int color)
    {
        int steps = FrameCount - 1;
        RgbColor start = first is null ? MotherBrainHealthPalettePresentation.StockDeathStartColor(part, color) : first[color];
        // Without a supplied endpoint the fade ends at black with a one-unit bias.
        RgbColor end = last is null ? RgbColor.Black : last[color];
        int bias = last is null ? 1 : steps / 2;
        return start.Zip(end, (_, from, to) => (from * (steps - frame) + to * frame + bias) / steps);
    }

    /// <summary>Appends resolved stage colors to the identity independent of storage representation.</summary>
    internal void AppendIdentity(SelectedPresentationHash content, string label)
    {
        content.Append(label, FrameCount);
        Span<RgbColor> row = stackalloc RgbColor[ColorCount];
        for (int frame = 0; frame < FrameCount; frame++)
        {
            for (int color = 0; color < ColorCount; color++)
                row[color] = Resolve(frame, color);
            content.AppendColors("row", row);
        }
    }
}
