using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The cutscene Baby's fade to black. The Baby's fade displays the separately identified final-health paints at
/// five-sevenths down to zero intensity. Supplied edits remain independent; no latent RGB8 endpoint reconstruction or
/// stored stock sample is needed.
/// </summary>
internal sealed class BabyMetroidFade
{
    /// <summary>Stores the edited frames only when they differ from the final-health fade formula.</summary>
    internal readonly RgbColor[][]? supplied;

    /// <summary>Uses the composition for stock frames and retains any independently authored edits.</summary>
    internal BabyMetroidFade(RgbColor[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
            if (Calculate(frame, color) != frames[frame][color])
            {
                supplied = frames.Select(fadeFrame => fadeFrame.ToArray()).ToArray();
                return;
            }
    }

    /// <summary>
    /// One displayed fade color: <paramref name="paletteIndex"/> one through six and <paramref name="color"/> zero
    /// through thirteen; the exception names <paramref name="paletteIndex"/> for both invalid cases.
    /// </summary>
    internal RgbColor Displayed(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorDefinitions.FadeColorCount
            ? Resolve(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

    /// <summary>Retrieves one displayed color from the edited frames or the composition.</summary>
    internal RgbColor Resolve(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

    /// <summary>Looks up the matching final-health paint before applying fade intensity.</summary>
    internal static RgbColor Endpoint(int color) => BabyMetroidFinalHealthPaint.Color(color);

    /// <summary>Scales each RGB5 channel by the remaining-sevenths intensity.</summary>
    private static RgbColor Calculate(int frame, int color)
    {
        int remaining = BabyMetroidCutsceneColorDefinitions.FadeFrameCount - 1 - frame;
        int intervals = BabyMetroidCutsceneColorDefinitions.FadeFrameCount + 1;
        return Endpoint(color).Map((_, channel) => channel * remaining / intervals);
    }
}
