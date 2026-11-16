using SuperMetroid.Core.Bosses.Torizo;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #2032: Torizo's drool and eye-beam headings and its drool, eye-beam and thrown Super Missile speeds are named
/// definitions; each must give exactly what the inline expression it replaces gave, over every spread and heading.
/// </summary>
internal static class TorizoNamedHeadingsVerification
{
    /// <summary>Compares the named headings and velocities with the replaced expressions.</summary>
    internal static void VerifyTorizoNamedHeadings()
    {
        foreach (bool facingRight in new[] { false, true })
        {
            for (int spread = 0; spread < 16; spread++)
            {
                HorizontalDirection facing = facingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
                AssertSameAt(new TrigonometryAngle((facingRight ? 32 : 224) + spread - 8),
                    TorizoAttacks.DroolHeading(facing, spread), $"drool heading facing right {facingRight}", spread);
                AssertSameAt(new TrigonometryAngle(spread - 8 + 96 + (facingRight ? 0 : 64)),
                    TorizoAttacks.EyeBeamHeading(facing, spread), $"eye-beam heading facing right {facingRight}", spread);
            }
        }

        for (int steps = 0; steps < 256; steps++)
        {
            var heading = new TrigonometryAngle(steps);
            AssertSameAt(Replaced(1, heading), TorizoAttacks.VelocityAlong(TorizoMotionDefinitions.DroolLaunchSpeed, heading), "drool velocity", steps);
            AssertSameAt(Replaced(8, heading), TorizoAttacks.VelocityAlong(TorizoMotionDefinitions.EyeBeamSpeed, heading), "eye-beam velocity", steps);
            AssertSameAt(Replaced(4, heading), TorizoAttacks.VelocityAlong(TorizoMotionDefinitions.ThrownSuperMissileSpeed, heading),
                "thrown Super Missile velocity", steps);
        }

        Console.WriteLine("Torizo named headings: drool and eye-beam headings and the three launch speeds give what their inline expressions gave.");
    }

    /// <summary>The replaced expression: the signed sine and the sine a quarter turn back, each times the pixels.</summary>
    private static EightEightVector<RoomPixels> Replaced(int pixels, TrigonometryAngle heading) =>
        new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(pixels * Trigonometry.SignedSine(heading)),
            SignedEightEight.FromTwoHundredFiftySixthsOfPixel(pixels * Trigonometry.SignedSine(heading - TrigonometryAngle.QuarterTurn)));
}
