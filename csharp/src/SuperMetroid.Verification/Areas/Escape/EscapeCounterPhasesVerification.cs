using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>
/// Confirms that the escape's named counter phases, cadences and explosion choice (#1619) read every input exactly as the
/// mask, remainder and threshold expressions they replace did: the timer's place in its period, the typewriter's key
/// click and which explosion choices name an explosion.
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
            AssertSameAt(ZebesEscapeDefinitions.ExplosionChoice.Of(input) < 8,
                ZebesEscapeDefinitions.ExplosionChoice.Of(input) < ZebesEscapeExplosionDefinitions.Choices.Length,
                "explosion choice names an explosion", input);
        }

        AssertEqual(EscapeTimerCadenceDefinitions.PeriodFrames, EscapeTimerCadenceDefinitions.PeriodPlace.Outcomes, "the period place spans the countdown's period");
        Console.WriteLine("Escape counter phases: every value from 0 through 65535 reads as the expressions they replace.");
    }
}
