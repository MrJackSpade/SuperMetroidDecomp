namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// <c>GradduallyAccelerateTowardsPoint</c> ($A9:F46B) and <c>GraduallyAccelerateHorizontally</c>
/// ($A9:F4E6), shared by the Tourian Shitroid and the Mother Brain baby-Metroid cutscene.
/// Each axis accelerates in runs of steps (<see cref="SeekingVelocityRun"/>): a step that carries
/// the velocity across zero makes the run's next step a unit larger, and a step taken while already
/// moving toward the target is a unit larger when the distance to the target wraps around the
/// coordinate space. A reversal starts fresh runs. Positions, targets, distances and velocity words
/// all lie in 0 through 65535.
/// </summary>
internal static class BabyMetroidGradualAcceleration
{
    /// <summary>Extra native speed adjustment while reversing direction toward the target.</summary>
    private const int ReversalStep = 8;
    /// <summary>Horizontal signed-velocity clamp applied after the run.</summary>
    private const int MaxXSpeed = 2048;
    /// <summary>Vertical signed-velocity clamp, smaller than the horizontal limit.</summary>
    private const int MaxYSpeed = 1280;

    /// <summary>
    /// Returns the new X velocity. <paramref name="isVaguelyOffScreen"/> runs only on the
    /// reversal path, as <c>CheckIfEnemyIsVagulyOnScreen</c> does natively.
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
        int distance = Wrap(position - target);
        if (distance == 0)
            return velocity;
        if (IsNegative(distance))
        {
            int step = Step(Negate(distance), divisor);
            if (IsNegative(velocity.RawValue))
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
            int speed = run.Velocity.RawValue;
            return Velocity(IsNegative(Wrap(speed - MaxXSpeed)) ? speed : MaxXSpeed);
        }
        else
        {
            int step = Step(distance, divisor);
            if (!IsNegative(velocity.RawValue))
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
            int speed = run.Velocity.RawValue;
            int limit = Negate(MaxXSpeed);
            return Velocity(IsNegative(Wrap(speed - limit)) ? limit : speed);
        }
    }

    /// <summary>Returns the new Y velocity (the vertical half of $A9:F46B).</summary>
    /// <param name="position">The mover's whole Y coordinate.</param>
    /// <param name="target">The target's whole Y coordinate.</param>
    /// <param name="velocity">The mover's Y velocity.</param>
    /// <param name="divisor">The distance divisor, 1 through 16.</param>
    public static SignedEightEight AccelerateVertically(int position, int target, SignedEightEight velocity, int divisor)
    {
        var run = new SeekingVelocityRun(velocity, position, target);
        int distance = Wrap(position - target);
        if (distance == 0)
            return velocity;
        if (IsNegative(distance))
        {
            int step = Step(Negate(distance), divisor);
            if (IsNegative(velocity.RawValue))
            {
                run.StartFresh();
                run.Raise(ReversalStep);
                run.Raise(step);
            }
            run.Raise(step);
            int speed = run.Velocity.RawValue;
            return Velocity(IsNegative(Wrap(speed - MaxYSpeed)) ? speed : MaxYSpeed);
        }
        else
        {
            int step = Step(distance, divisor);
            if (!IsNegative(velocity.RawValue))
            {
                run.StartFresh();
                run.Lower(ReversalStep);
                run.Lower(step);
            }
            run.Lower(step);
            int speed = run.Velocity.RawValue;
            int limit = Negate(MaxYSpeed);
            return Velocity(IsNegative(Wrap(speed - limit)) ? limit : speed);
        }
    }

    /// <summary>Models $4204/$4206 distance division, replacing a zero quotient with one.</summary>
    private static int Step(int distance, int divisor)
    {
        if (divisor is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(divisor), "The divisor table holds $10..$01.");
        int quotient = distance / divisor;
        return quotient == 0 ? 1 : quotient;
    }

    /// <summary>The two's-complement negation of a word, as a word.</summary>
    private static int Negate(int word) => Wrap(-word);

    /// <summary>A difference of words reduced to the word the run compares.</summary>
    private static int Wrap(int value) => unchecked((ushort)value);

    /// <summary>A velocity word as the velocity it encodes.</summary>
    private static SignedEightEight Velocity(int word) => new(checked((ushort)word));

    /// <summary>Tests the word sign bit before choosing an acceleration or reversal branch.</summary>
    private static bool IsNegative(int word) => unchecked((short)word) < 0;
}
