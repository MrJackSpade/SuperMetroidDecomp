using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>
/// Confirms that the escape's named counter phases, cadences and explosion choice (#1619) read every input exactly as the
/// mask, remainder and threshold expressions they replace did: the timer's place in its period, the typewriter's key
/// click, which explosion choices name an explosion, the room effects' chances of a shake turning diagonal and the
/// cadences of the outdoor and indoor explosions.
/// </summary>
internal static class EscapeCounterPhasesVerification
{
    /// <summary>Compares every frame count, glyph count and sample with the expressions the named rules replace.</summary>
    internal static void VerifyEscapeCounterPhases()
    {
        for (int input = 0; input <= ushort.MaxValue; input++)
        {
            AssertSameAt(input & 127, EscapeTimerCadenceDefinitions.PeriodPlace.Of(input), "escape timer period place", input);
            AssertSameAt(input % 2 == 0, EscapeTypewriterDefinitions.ClickEverySecondGlyph.IsDue(input), "typewriter click", input);
            AssertSameAt((input & 15) < 8,
                ZebesEscapeExplosionDefinitions.Draw.Choose(input) is ZebesEscapeExplosionDraw.Named,
                "explosion choice names an explosion", input);
            AssertSameAt(input < 512, ZebesEscapeDefinitions.LightShakeTurnsDiagonal.IsTakenBy(input), "light shake turns diagonal", input);
            AssertSameAt(input < 384, ZebesEscapeDefinitions.MediumShakeTurnsDiagonal.IsTakenBy(input), "medium shake turns diagonal", input);
            AssertSameAt((input & 1) == 0, ZebesEscapeDefinitions.OutdoorExplosionEverySecondUpdate.IsDue(input), "outdoor explosion", input);
            AssertSameAt((input & 3) == 0, ZebesEscapeDefinitions.IndoorExplosionEveryFourthUpdate.IsDue(input), "indoor explosion", input);
        }

        AssertEqual(EscapeTimerCadenceDefinitions.CentisecondsRemoved.Length, EscapeTimerCadenceDefinitions.PeriodPlace.Outcomes,
            "the cadence authors a step for every place of its period");
        Console.WriteLine("Escape counter phases: every value from 0 through 65535 reads as the expressions they replace.");
    }
}
