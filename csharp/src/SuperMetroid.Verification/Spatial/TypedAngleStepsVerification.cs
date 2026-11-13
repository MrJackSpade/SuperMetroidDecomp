using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #1684 typed angle operations against the angle arithmetic they replace: the nearest of equally spaced
/// directions and the grapple's swing direction against the rounded, wrapped bucket of 32, the shorter-way step
/// difference against the signed byte difference, and the middle of a step against the step with half a step's
/// fraction.
/// </summary>
internal static class TypedAngleStepsVerification
{
    /// <summary>Runs every typed angle-step check.</summary>
    internal static void VerifyTypedAngleSteps()
    {
        for (int units = 0; units < RotationAngle.StepsPerTurn; units++)
        {
            var angle = new TrigonometryAngle(units);
            int bucket = ((units + 4) >> 3) & 31;
            AssertEqual(bucket, angle.NearestDirection(32), $"angle {units} nearest of 32 directions");
            AssertEqual(GrappleSwingDirection.FromNumber(bucket), GrappleSwingDirection.Nearest(angle), $"angle {units} grapple swing direction");
            AssertEqual(units, angle.NearestDirection(RotationAngle.StepsPerTurn), $"angle {units} nearest of a turn's steps");
            AssertEqual(0, angle.NearestDirection(1), $"angle {units} nearest of one direction");
            AssertEqual(units is >= 64 and < 192 ? 1 : 0, angle.NearestDirection(2), $"angle {units} nearest of two directions");
            AssertEqual(RotationAngle.FromSteps(units, 128), RotationAngle.MiddleOfStep(angle), $"middle of step {units}");
            for (int origin = 0; origin < RotationAngle.StepsPerTurn; origin++)
            {
                int expected = unchecked((sbyte)(units - origin));
                if (angle.StepsFrom(new TrigonometryAngle(origin)) != expected)
                    throw new InvalidDataException($"Angle {units} lies {angle.StepsFrom(new TrigonometryAngle(origin))} steps from {origin}, not {expected}.");
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(() => TrigonometryAngle.Zero.NearestDirection(0), "a turn holds at least one direction");
        AssertThrows<ArgumentOutOfRangeException>(() => TrigonometryAngle.Zero.NearestDirection(RotationAngle.StepsPerTurn + 1), "a turn holds at most a direction per step");
        AssertThrows<ArgumentOutOfRangeException>(() => GrappleSwingDirection.FromNumber(GrappleSwingDirection.Count), "a swing direction past the turn is rejected");
        AssertEqual(GrappleSwingDirection.Count, GrappleSwingDirection.All.Count, "every swing direction is listed once");
        for (int number = 0; number < GrappleSwingDirection.Count; number++)
            AssertEqual(number, GrappleSwingDirection.All[number].Number, $"swing direction {number} lies in turn order");
        Console.WriteLine("  Typed angle steps: nearest directions, swing directions, step differences and step middles equal the arithmetic they replace.");
    }
}
