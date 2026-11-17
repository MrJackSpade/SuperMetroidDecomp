using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>
/// Checks that the Ki-Hunter swoop's typed angles and turning speeds (#2042) reproduce the integer swoop angle and the
/// 16.16 angular velocity they replaced, over every value each can take.
/// </summary>
internal static class KiHunterSwoopTurnVerification
{
    /// <summary>
    /// Compares the swoop definitions with the integers they replaced; every turning speed's whole steps, acceleration
    /// and whole-step cap with the 16.16 arithmetic; and every swoop angle stepped by every whole velocity with the
    /// integer angle's end test, table angle and animation-change comparison.
    /// </summary>
    internal static void VerifyKiHunterSwoopTurn()
    {
        AssertEqual(RotationAngle.FromSteps(128), KiHunterMotionDefinitions.RightSwoopStartAngle, "a rightward swoop starts at angle 128");
        AssertEqual(RotationAngle.FromSteps(255), KiHunterMotionDefinitions.LeftSwoopStartAngle, "a leftward swoop starts at angle 255");
        AssertEqual(RotationAngle.FromSteps(144), KiHunterMotionDefinitions.RightSwoopAnimationChangeAngle, "a rightward swoop changes animation at angle 144");
        AssertEqual(RotationAngle.FromSteps(240), KiHunterMotionDefinitions.LeftSwoopAnimationChangeAngle, "a leftward swoop changes animation at angle 240");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 65536 / 8), Widened(KiHunterMotionDefinitions.SwoopAcceleration),
            "a rightward swoop accelerates by the 16.16 eighth of a step");
        AssertEqual(-SignedSixteenSixteen.FromParts(0, 65536 / 8), Widened(-KiHunterMotionDefinitions.SwoopAcceleration),
            "a leftward swoop accelerates by the negated 16.16 eighth of a step");
        AssertEqual(SignedSixteenSixteen.FromWholePixels(2), Widened(KiHunterMotionDefinitions.SwoopMaximumAngularVelocity),
            "a rightward swoop's whole velocity cap is two");
        AssertEqual(SignedSixteenSixteen.FromWholePixels(-2), Widened(-KiHunterMotionDefinitions.SwoopMaximumAngularVelocity),
            "a leftward swoop's whole velocity cap is minus two");

        string? mismatch = null;
        for (int fractions = short.MinValue; fractions <= short.MaxValue; fractions++)
        {
            AngularSpeed speed = AngularSpeed.FromFractionsOfStep(fractions);
            SignedSixteenSixteen old = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(fractions);
            if (speed.WholeSteps != old.WholePixels)
                mismatch ??= $"speed {fractions}: whole steps {speed.WholeSteps}, 16.16 whole {old.WholePixels}";
            foreach (AngularSpeed acceleration in new[] { KiHunterMotionDefinitions.SwoopAcceleration, -KiHunterMotionDefinitions.SwoopAcceleration })
            {
                int sum = fractions + acceleration.FractionsOfStep;
                if (sum >= short.MinValue && sum <= short.MaxValue && Widened(speed + acceleration) != old + Widened(acceleration))
                    mismatch ??= $"speed {fractions} accelerated by {acceleration} differs from the 16.16 sum";
            }
            SignedSixteenSixteen oldRightCap = old.WholePixels >= 2 ? old.WithWholePixels(2) : old;
            if (Widened(speed.WithWholeStepsLimitedTo(KiHunterMotionDefinitions.SwoopMaximumAngularVelocity)) != oldRightCap)
                mismatch ??= $"speed {fractions} capped rightward differs from the 16.16 whole-word cap";
            SignedSixteenSixteen oldLeftCap = old.WholePixels < -2 ? old.WithWholePixels(-2) : old;
            if (Widened(speed.WithWholeStepsLimitedTo(-KiHunterMotionDefinitions.SwoopMaximumAngularVelocity)) != oldLeftCap)
                mismatch ??= $"speed {fractions} capped leftward differs from the 16.16 whole-word cap";
        }

        for (int angle = RotationAngle.StepsPerTurn / 2; angle < RotationAngle.StepsPerTurn; angle++)
        {
            RotationAngle swoopAngle = RotationAngle.FromSteps(angle);
            if ((angle < 144) != (swoopAngle < KiHunterMotionDefinitions.RightSwoopAnimationChangeAngle) ||
                (angle >= 240) != (swoopAngle >= KiHunterMotionDefinitions.LeftSwoopAnimationChangeAngle))
            {
                mismatch ??= $"angle {angle} compares with the animation-change angles unlike the integer";
            }
            for (int wholeVelocity = -2; wholeVelocity <= 2; wholeVelocity++)
            {
                int oldAngle = angle + wholeVelocity;
                bool oldEnded = wholeVelocity >= 0 ? oldAngle >= RotationAngle.StepsPerTurn : oldAngle < RotationAngle.StepsPerTurn / 2;
                RotationAngle stepped = swoopAngle.TurnedBySteps(wholeVelocity);
                if (oldEnded != (stepped < RotationAngle.HalfTurn))
                    mismatch ??= $"angle {angle} stepped by {wholeVelocity} ends the swoop unlike the integer test";
                if (stepped != RotationAngle.WrappedSteps(oldAngle) ||
                    stepped.ToTrigonometryAngle() != TrigonometryAngle.Wrapped(oldAngle))
                {
                    mismatch ??= $"angle {angle} stepped by {wholeVelocity} lands at {stepped}, not the integer {oldAngle}";
                }
            }
        }

        AssertEqual<string?>(null, mismatch, "every typed swoop speed and angle reproduces the integer arithmetic");
        Console.WriteLine("Ki-Hunter swoop turn: all 65536 turning speeds and every swoop angle and whole velocity match the integer arithmetic.");
    }

    /// <summary>The 16.16 word the swoop's angular velocity was held in, for <paramref name="speed"/>.</summary>
    private static SignedSixteenSixteen Widened(AngularSpeed speed) =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(speed.FractionsOfStep);

    /// <summary>Runs the focused Ki-Hunter swoop turn check.</summary>
    [VerificationCommand("--ki-hunter-swoop-turn")]
    internal static int KiHunterSwoopTurnCommand()
    {
        VerifyKiHunterSwoopTurn();
        return 0;
    }
}
