using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Recognizes each supplied auxiliary enemy colour family as its stock paint, as an interpolation between its supplied
/// endpoints, or as independently edited frames, the form the game resolves its colours from.
/// </summary>
internal static class EnemyAuxiliaryColorFrameRecognition
{
    /// <summary>Recognizes the face-block glow, sidehopper drain and Golden Torizo body and belly families.</summary>
    internal static Dictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryColourFrames> Recognize(
        Dictionary<EnemyAuxiliaryPalette, RgbColor[][]> frames) =>
        new()
        {
            [EnemyAuxiliaryPalette.FaceBlock] = Frames(frames[EnemyAuxiliaryPalette.FaceBlock], AuxiliaryPaint.FaceGlow),
            [EnemyAuxiliaryPalette.DeadSidehopper] = Frames(frames[EnemyAuxiliaryPalette.DeadSidehopper], AuxiliaryPaint.SidehopperDrain),
            [EnemyAuxiliaryPalette.GoldenTorizoBody] = Frames(frames[EnemyAuxiliaryPalette.GoldenTorizoBody], AuxiliaryPaint.TorizoFront),
            [EnemyAuxiliaryPalette.GoldenTorizoBelly] = Frames(frames[EnemyAuxiliaryPalette.GoldenTorizoBelly], AuxiliaryPaint.TorizoRear),
        };

    /// <summary>Accepts a calculated paint progression only when every supplied RGB5 word matches it.</summary>
    private static EnemyAuxiliaryColourFrames Frames(RgbColor[][] frames, AuxiliaryPaint paint)
    {
        int frameCount = frames.Length;
        int colorCount = frames[0].Length;
        if (MatchesStock(frames, paint))
            return new(paint, stock: true, frameCount, colorCount, [], [], corpse: null, RgbColor.Black, supplied: null);
        bool faceGlow = paint == AuxiliaryPaint.FaceGlow;
        bool sidehopperDrain = paint == AuxiliaryPaint.SidehopperDrain;
        RgbColor[] last = frames[faceGlow ? 3 : sidehopperDrain ? frameCount - 2 : frameCount - 1];
        RgbColor[]? corpse = sidehopperDrain ? frames[^1] : null;
        RgbColor glowAccentStart = faceGlow ? frames[1][3] : RgbColor.Black;
        var calculated = new EnemyAuxiliaryColourFrames(paint, stock: false, frameCount, colorCount, frames[0], last, corpse,
            glowAccentStart, supplied: null);
        for (int frame = 0; frame < frameCount; frame++)
            for (int color = 0; color < colorCount; color++)
                if (calculated.Color(frame, color) != frames[frame][color])
                    return new(paint, stock: false, frameCount, colorCount, frames[0], last, corpse, glowAccentStart, frames);
        return calculated;
    }

    /// <summary>Whether every supplied frame is the progression's own stock paint.</summary>
    private static bool MatchesStock(RgbColor[][] frames, AuxiliaryPaint paint) => paint switch
    {
        AuxiliaryPaint.TorizoFront => MatchesTorizo(frames, rear: false),
        AuxiliaryPaint.TorizoRear => MatchesTorizo(frames, rear: true),
        AuxiliaryPaint.FaceGlow => MatchesFaceGlow(frames),
        AuxiliaryPaint.SidehopperDrain => MatchesSidehopperDrain(frames),
        _ => throw new ArgumentOutOfRangeException(nameof(paint), paint, "Unknown auxiliary paint progression."),
    };

    /// <summary>Checks all eight palette bands and sixteen colors against the front or rear Golden Torizo paint.</summary>
    private static bool MatchesTorizo(RgbColor[][] frames, bool rear)
    {
        for (int frame = 0; frame <= GoldenTorizoHealthPaintDefinitions.Intervals; frame++)
        for (int color = 0; color < 16; color++)
            if (frames[frame][color] != GoldenTorizoHealthPaint.Color(frame, color, rear)) return false;
        return true;
    }

    /// <summary>Accepts the supplied eight-frame, four-ink glow only when it matches the reviewed rise-and-fall calculation.</summary>
    private static bool MatchesFaceGlow(RgbColor[][] frames)
    {
        for (int frame = 0; frame < 2 * (FaceBlockGlowPaintDefinitions.PeakDepth + 1); frame++)
        for (int color = 0; color < 4; color++)
            if (frames[frame][color] != FaceBlockGlowPaint.Color(frame, color)) return false;
        return true;
    }

    /// <summary>Checks all seven 15-color drain images against the reviewed interpolation and corpse prefix.</summary>
    private static bool MatchesSidehopperDrain(RgbColor[][] frames)
    {
        for (int frame = 0; frame <= SidehopperDrainPaintDefinitions.CorpseFrame; frame++)
        for (int color = 0; color < 15; color++)
            if (frames[frame][color] != SidehopperDrainPaint.Color(frame, color)) return false;
        return true;
    }
}
