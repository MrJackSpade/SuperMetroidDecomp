using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The Golden Torizo body/belly health palettes interpolate RGB5 endpoints
/// to nearest over seven intervals. Transparent slot zero holds its initial word until
/// the last band. The face-block glow mirrors a three-step rise; its fourth color
/// has a separate resting color and two-step active rise. Endpoint paint and unmatched
/// independently supplied frames remain data. Sidehopper draining interpolates
/// actual RGB5 endpoints to nearest over five intervals; the last frame is separate corpse paint.
/// </summary>
internal sealed class EnemyAuxiliaryColourFrames
{
    /// <summary>Initial per-color interpolation endpoints for non-stock derived frames.</summary>
    internal readonly RgbColor[] first;
    /// <summary>Final per-color interpolation endpoints for non-stock derived frames.</summary>
    internal readonly RgbColor[] last;
    /// <summary>Exact supplied frames when their colors do not match a known progression.</summary>
    internal readonly RgbColor[][]? supplied;
    /// <summary>The calculated paint progression the family follows.</summary>
    private readonly AuxiliaryPaint paint;
    /// <summary>Whether the frames are the progression's own stock paint rather than an interpolation of supplied endpoints.</summary>
    private readonly bool stock;
    /// <summary>The face-block accent color used for the active portion of its mirrored glow.</summary>
    private readonly RgbColor glowAccentStart;
    /// <summary>The independently authored terminal sidehopper corpse frame.</summary>
    internal readonly RgbColor[]? corpse;

    /// <summary>Number of authored stages or health bands in this family.</summary>
    internal int FrameCount { get; }
    /// <summary>Colors supplied per frame, excluding sidehopper's transparent slot zero.</summary>
    internal int ColorCount { get; }
    /// <summary>Keeps a family's frames as the import recognized them.</summary>
    /// <param name="paint">The calculated paint progression the family follows.</param>
    /// <param name="stock">Whether the frames are the progression's own stock paint.</param>
    /// <param name="frameCount">Number of authored stages or health bands.</param>
    /// <param name="colorCount">Colors supplied per frame.</param>
    /// <param name="first">Initial interpolation endpoints; empty for stock paint.</param>
    /// <param name="last">Final interpolation endpoints; empty for stock paint.</param>
    /// <param name="corpse">The sidehopper's terminal corpse frame, or null for the other families and for stock paint.</param>
    /// <param name="glowAccentStart">The face-block accent colour the active glow starts from; black for other families.</param>
    /// <param name="supplied">Every supplied frame when they follow no calculated progression, otherwise null.</param>
    internal EnemyAuxiliaryColourFrames(AuxiliaryPaint paint, bool stock, int frameCount, int colorCount, RgbColor[] first,
        RgbColor[] last, RgbColor[]? corpse, RgbColor glowAccentStart, RgbColor[][]? supplied)
    {
        this.paint = paint;
        this.stock = stock;
        FrameCount = frameCount;
        ColorCount = colorCount;
        this.first = first;
        this.last = last;
        this.corpse = corpse;
        this.glowAccentStart = glowAccentStart;
        this.supplied = supplied;
    }
    /// <summary>Uses exact edited paint where retained; otherwise derives the color relationship.</summary>
    internal RgbColor Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];
    /// <summary>Interpolates RGB5 health/drain endpoints or mirrors the face-block glow, with stock palettes using their own paint definitions.</summary>
    private RgbColor Calculate(int frame, int color)
    {
        if (stock)
            return paint switch
            {
                AuxiliaryPaint.TorizoFront => GoldenTorizoHealthPaint.Color(frame, color, rear: false),
                AuxiliaryPaint.TorizoRear => GoldenTorizoHealthPaint.Color(frame, color, rear: true),
                AuxiliaryPaint.FaceGlow => FaceBlockGlowPaint.Color(frame, color),
                AuxiliaryPaint.SidehopperDrain => SidehopperDrainPaint.Color(frame, color),
                _ => throw new InvalidOperationException($"Auxiliary paint {paint} has no stock progression."),
            };
        bool faceGlow = paint == AuxiliaryPaint.FaceGlow;
        if (corpse is not null && frame == FrameCount - 1) return corpse[color];
        int steps = faceGlow ? 3 : FrameCount - (corpse is null ? 1 : 2);
        if (faceGlow) frame = Math.Min(frame, FrameCount - 1 - frame);
        else if (corpse is null && color == 0) return frame == steps ? last[color] : first[color];
        RgbColor start = first[color];
        if (faceGlow && color == 3)
        {
            if (frame == 0) return start;
            start = glowAccentStart;
            frame--; steps--;
        }
        return start.Zip(last[color], (_, a, b) => ((a * (steps - frame)
                + b * frame + steps / 2) / steps));
    }
}
