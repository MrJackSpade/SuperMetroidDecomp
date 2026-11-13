namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The gradual acceleration toward a point and horizontally, shared by the Tourian Shitroid and the Mother Brain
/// baby-Metroid cutscene.
/// Each axis accelerates in runs of steps (<see cref="SeekingVelocityRun"/>): a step that carries
/// the velocity across zero makes the run's next step a unit larger, and a step taken while already
/// moving toward the target is a unit larger when the distance to the target wraps around the
/// coordinate space. A reversal starts fresh runs. The distance is the signed distance across the wrapping space, and
/// the velocity stays within a few thousand units of rest (each call clamps it to its axis's limit and a step is at
/// most a room's width), so the clamps compare signed velocities.
/// </summary>
internal static class BabyMetroidGradualAcceleration
{
    /// <summary>Extra speed adjustment while reversing direction toward the target.</summary>
    private const int ReversalStep = 8;
    /// <summary>Horizontal signed-velocity clamp applied after the run: eight pixels an update either way.</summary>
    private static readonly SignedEightEight MaxXSpeed = SignedEightEight.FromPixels(8);
    /// <summary>Vertical signed-velocity clamp, smaller than the horizontal limit: five pixels an update either way.</summary>
    private static readonly SignedEightEight MaxYSpeed = SignedEightEight.FromPixels(5);

    /// <summary>
    /// Returns the new X velocity. <paramref name="isVaguelyOffScreen"/> runs only on the
    /// reversal path, as the original's vaguely-on-screen check does.
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
        int wrongWayOffScreenSpeed,
        Func<bool> isVaguelyOffScreen)
    {
        Ensure.NotNull(isVaguelyOffScreen);
        var run = new SeekingVelocityRun(velocity, position, target);
        int distance = CoordinateAxis.Difference(position, target);
        if (distance == 0)
            return velocity;
        if (distance < 0)
        {
            int step = Step(-distance, divisor);
            if (velocity.IsNegative)
            {
                // Off screen, a reversal first pushes the mover by its off-screen speed, raising by one unit more
                // than the matching lowering does.
                if (isVaguelyOffScreen())
                {
                    run.StartFresh();
                    run.Raise(wrongWayOffScreenSpeed + 1);
                }
                run.StartFresh();
                run.Raise(ReversalStep);
                run.Raise(step);
            }
            run.Raise(step);
            return SignedEightEight.Min(run.Velocity, MaxXSpeed);
        }
        else
        {
            int step = Step(distance, divisor);
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
            return SignedEightEight.Max(run.Velocity, -MaxXSpeed);
        }
    }

    /// <summary>Returns the new Y velocity (the vertical half of gradual acceleration).</summary>
    /// <param name="position">The mover's whole Y coordinate.</param>
    /// <param name="target">The target's whole Y coordinate.</param>
    /// <param name="velocity">The mover's Y velocity.</param>
    /// <param name="divisor">The distance divisor, 1 through 16.</param>
    public static SignedEightEight AccelerateVertically(int position, int target, SignedEightEight velocity, int divisor)
    {
        var run = new SeekingVelocityRun(velocity, position, target);
        int distance = CoordinateAxis.Difference(position, target);
        if (distance == 0)
            return velocity;
        if (distance < 0)
        {
            int step = Step(-distance, divisor);
            if (velocity.IsNegative)
            {
                run.StartFresh();
                run.Raise(ReversalStep);
                run.Raise(step);
            }
            run.Raise(step);
            return SignedEightEight.Min(run.Velocity, MaxYSpeed);
        }
        else
        {
            int step = Step(distance, divisor);
            if (!velocity.IsNegative)
            {
                run.StartFresh();
                run.Lower(ReversalStep);
                run.Lower(step);
            }
            run.Lower(step);
            return SignedEightEight.Max(run.Velocity, -MaxYSpeed);
        }
    }

    /// <summary>Models the distance division, replacing a zero quotient with one.</summary>
    private static int Step(int distance, int divisor)
    {
        if (divisor is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(divisor), "The divisor table holds 16 down to 1.");
        int quotient = distance / divisor;
        return quotient == 0 ? 1 : quotient;
    }
}
