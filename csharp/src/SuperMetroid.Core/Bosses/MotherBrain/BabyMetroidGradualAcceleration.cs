namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The gradual acceleration toward a point and horizontally, shared by the Tourian Baby Metroid and the Mother Brain
/// baby-Metroid cutscene.
/// Each axis accelerates in runs of steps (<see cref="SeekingVelocityRun"/>): a step that carries the velocity across
/// rest makes the run's next step a unit larger, and a step taken while already moving toward a target on the far side
/// of the room's wrap (<see cref="CoordinateAxis"/>) is a unit larger. A reversal starts fresh runs.
/// </summary>
internal static class BabyMetroidGradualAcceleration
{
    /// <summary>Extra speed adjustment while reversing direction toward the target: a thirty-second of a pixel an update.</summary>
    private static readonly SignedEightEight ReversalStep = SignedEightEight.FromPixelFraction(1, 32);

    /// <summary>
    /// How much more an off-screen reversal raises the velocity than its matching lowering: the finest step a speed
    /// takes.
    /// </summary>
    private static readonly SignedEightEight OffScreenRaiseExtra = SignedEightEight.FromPixelFraction(1, 256);
    /// <summary>Horizontal signed-velocity clamp applied after the run: eight pixels an update either way.</summary>
    private static readonly SignedEightEight MaxXSpeed = SignedEightEight.FromPixels(8);
    /// <summary>Vertical signed-velocity clamp, smaller than the horizontal limit: five pixels an update either way.</summary>
    private static readonly SignedEightEight MaxYSpeed = SignedEightEight.FromPixels(5);

    /// <summary>
    /// Returns the new X velocity. <paramref name="isVaguelyOffScreen"/> runs only on the
    /// reversal path.
    /// </summary>
    /// <param name="position">The mover's whole X coordinate.</param>
    /// <param name="target">The target's whole X coordinate.</param>
    /// <param name="velocity">The mover's X velocity.</param>
    /// <param name="divisor">The distance divisor, 1 through 16.</param>
    /// <param name="wrongWayOffScreenSpeed">The extra speed a reversal adds while the mover is off screen.</param>
    /// <param name="isVaguelyOffScreen">Whether the mover is off screen, consulted only when reversing.</param>
    public static SignedEightEight AccelerateHorizontally(
        int position,
        int target,
        SignedEightEight velocity,
        int divisor,
        SignedEightEight wrongWayOffScreenSpeed,
        Func<bool> isVaguelyOffScreen)
    {
        Ensure.NotNull(isVaguelyOffScreen);
        return Accelerate(position, target, velocity, divisor, MaxXSpeed, isVaguelyOffScreen, wrongWayOffScreenSpeed);
    }

    /// <summary>Returns the new Y velocity (the vertical half of gradual acceleration), whose reversals take no off-screen push.</summary>
    /// <param name="position">The mover's whole Y coordinate.</param>
    /// <param name="target">The target's whole Y coordinate.</param>
    /// <param name="velocity">The mover's Y velocity.</param>
    /// <param name="divisor">The distance divisor, 1 through 16.</param>
    public static SignedEightEight AccelerateVertically(int position, int target, SignedEightEight velocity, int divisor) =>
        Accelerate(position, target, velocity, divisor, MaxYSpeed, NeverPushedOffScreen, default);

    /// <summary>
    /// One axis's gradual acceleration: the velocity runs towards the target by the distance's step, a reversal first starting
    /// fresh runs, and is clamped to <paramref name="maxSpeed"/> either way. When <paramref name="isVaguelyOffScreen"/> says
    /// the mover is off screen, a reversal first pushes it by <paramref name="wrongWayOffScreenSpeed"/>, raising by one unit
    /// more than the matching lowering does.
    /// </summary>
    private static SignedEightEight Accelerate(int position, int target, SignedEightEight velocity, int divisor, SignedEightEight maxSpeed,
        Func<bool> isVaguelyOffScreen, SignedEightEight wrongWayOffScreenSpeed)
    {
        var run = new SeekingVelocityRun(velocity, position, target);
        int distance = DistanceAcrossRoomEdge.Apart(position, target);
        if (distance == 0)
            return velocity;
        if (distance < 0)
        {
            SignedEightEight step = Step(-distance, divisor);
            if (velocity.IsNegative)
            {
                if (isVaguelyOffScreen())
                {
                    run.StartFresh();
                    run.Raise(wrongWayOffScreenSpeed + OffScreenRaiseExtra);
                }
                run.StartFresh();
                run.Raise(ReversalStep);
                run.Raise(step);
            }
            run.Raise(step);
            return SignedEightEight.Min(run.Velocity, maxSpeed);
        }
        else
        {
            SignedEightEight step = Step(distance, divisor);
            if (!velocity.IsNegative)
            {
                if (isVaguelyOffScreen())
                {
                    run.StartFresh();
                    run.Lower(wrongWayOffScreenSpeed);
                }
                run.StartFresh();
                run.Lower(ReversalStep);
                run.Lower(step);
            }
            run.Lower(step);
            return SignedEightEight.Max(run.Velocity, -maxSpeed);
        }
    }

    /// <summary>The vertical axis's reversals: never pushed as an off-screen reversal is.</summary>
    private static bool NeverPushedOffScreen() => false;

    /// <summary>
    /// The step for the distance: the distance divided by the divisor, at least one
    /// (<see cref="SeekingVelocityRun.StepFor"/>).
    /// </summary>
    private static SignedEightEight Step(int distance, int divisor)
    {
        if (divisor is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(divisor), "The divisor is 16 down to 1.");
        int quotient = distance / divisor;
        return SeekingVelocityRun.StepFor(quotient == 0 ? 1 : quotient);
    }
}
