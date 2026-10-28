using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// $84:8032/8132 Golden Torizo body/belly health palettes interpolate RGB5 endpoints
/// to nearest over seven intervals. Transparent slot zero holds its initial word until
/// the last band. $A8:E7CC face-block glow mirrors a three-step rise; its fourth color
/// has a separate resting color and two-step active rise. Endpoint paint and unmatched
/// independently supplied rows remain data. $A9:EBCC-EC7C Sidehopper draining interpolates
/// actual RGB5 endpoints to nearest over five intervals; EC8C is separate corpse paint.
/// </summary>
internal sealed class EnemyAuxiliaryPaletteRows
{
    /// <summary>Initial per-color interpolation endpoints for non-stock derived rows.</summary>
    internal readonly RgbColor[] first;
    /// <summary>Final per-color interpolation endpoints for non-stock derived rows.</summary>
    internal readonly RgbColor[] last;
    /// <summary>Exact supplied rows when their colors do not match a known native progression.</summary>
    internal readonly RgbColor[][]? supplied;
    /// <summary>Whether non-stock rows mirror a face-block glow rise and fall.</summary>
    private readonly bool faceGlow;
    /// <summary>Stock paint recognition and Torizo rear-palette selection flags.</summary>
    private readonly bool stockTorizo, rearTorizo, stockSidehopperDrain, stockFaceGlow;
    /// <summary>The face-block accent color used for the active portion of its mirrored glow.</summary>
    private readonly RgbColor glowAccentStart;
    /// <summary>The independently authored terminal sidehopper corpse row.</summary>
    internal readonly RgbColor[]? corpse;

    /// <summary>Number of authored stages or health bands in this family.</summary>
    internal int FrameCount { get; }
    /// <summary>Colors supplied per row, excluding sidehopper's native transparent slot zero.</summary>
    internal int ColorCount { get; }
    /// <summary>Accepts a calculated paint progression only when every supplied RGB5 word matches it.</summary>
    internal EnemyAuxiliaryPaletteRows(RgbColor[][] rows, AuxiliaryPaint paint)
    {
        bool healthGradient = paint is AuxiliaryPaint.TorizoFront or AuxiliaryPaint.TorizoRear;
        bool faceGlow = paint == AuxiliaryPaint.FaceGlow;
        bool sidehopperDrain = paint == AuxiliaryPaint.SidehopperDrain;
        bool rearTorizo = paint == AuxiliaryPaint.TorizoRear;
        FrameCount = rows.Length;
        ColorCount = rows[0].Length;
        if (healthGradient && GoldenTorizoHealthPaint.Matches(rows, rearTorizo))
        {
            stockTorizo = true; this.rearTorizo = rearTorizo; first = []; last = []; return;
        }
        if (faceGlow && FaceBlockGlowPaint.Matches(rows))
        {
            stockFaceGlow = true; first = []; last = []; return;
        }
        if (sidehopperDrain && SidehopperDrainPaint.Matches(rows))
        {
            stockSidehopperDrain = true; first = []; last = []; return;
        }
        first = rows[0];
        last = rows[faceGlow ? 3 : sidehopperDrain ? rows.Length - 2 : rows.Length - 1];
        corpse = sidehopperDrain ? rows[^1] : null;
        this.faceGlow = faceGlow;
        glowAccentStart = faceGlow ? rows[1][3] : RgbColor.Black;
        if (!healthGradient && !faceGlow && !sidehopperDrain) { supplied = rows; return; }
        for (int frame = 0; frame < FrameCount; frame++)
            for (int color = 0; color < ColorCount; color++)
                if (Calculate(frame, color) != rows[frame][color]) { supplied = rows; return; }

    }
    /// <summary>Uses exact edited paint where retained; otherwise derives the native color relationship.</summary>
    internal RgbColor Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];
    /// <summary>Interpolates RGB5 health/drain endpoints or mirrors the face-block glow, with stock palettes using their own paint definitions.</summary>
    private RgbColor Calculate(int frame, int color)
    {
        if (stockTorizo) return GoldenTorizoHealthPaint.Color(frame, color, rearTorizo);
        if (stockFaceGlow) return FaceBlockGlowPaint.Color(frame, color);
        if (stockSidehopperDrain) return SidehopperDrainPaint.Color(frame, color);
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
