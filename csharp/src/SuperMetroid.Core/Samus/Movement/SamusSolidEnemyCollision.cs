using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Literal translation of Samus-versus-solid-enemy collision detection.
/// </summary>
/// <remarks>
/// This is intentionally a probe, not an enemy simulation. The caller supplies the current
/// interactive-enemy list in slot order. That order matters:
/// the original routine returns the first colliding entry, not the geometrically nearest one.
/// All positions and arithmetic remain 16-bit so wraparound, signed branch tests, and pixel
/// rounding agree with the original rather than with conventional geometry.
/// </remarks>
public static class SamusSolidEnemyCollision
{
    /// <summary>
    /// Runs one directional probe without moving Samus.
    /// </summary>
    /// <param name="state">Samus's current whole/subpixel position and collision radii.</param>
    /// <param name="interactiveEnemies">
    /// Active interactive enemies in slot order. Non-solid, unfrozen entries remain in
    /// the list because the loop encounters and rejects them individually.
    /// </param>
    /// <param name="direction">Low two bits of collision movement direction.</param>
    /// <param name="distance">Whole pixels of the requested collision distance.</param>
    /// <param name="distanceSubposition">Fraction of the requested collision distance.</param>
    /// <returns>The first qualifying body's current leading-edge gap, or the requested whole distance with no enemy when no collision is found.</returns>
    /// <remarks>No whole position or enemy body is changed. Exact current-edge contact clears <paramref name="state"/>'s Y subposition even for horizontal probes. The distance pair is an unsigned 16.16 magnitude; direction supplies its sign.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> or <paramref name="interactiveEnemies"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not one of the four directional values 0..3; NonDirectionalProbe is not accepted here.</exception>
    public static SolidEnemyCollisionResult Probe(
        SamusKinematicsState state,
        IReadOnlyList<SolidEnemyCollisionBody> interactiveEnemies,
        SamusCollisionDirection direction,
        int distance,
        int distanceSubposition)
    {
        Ensure.NotNull(state);
        Ensure.NotNull(interactiveEnemies);

        if (direction is not (SamusCollisionDirection.Left or SamusCollisionDirection.Right or
            SamusCollisionDirection.Up or SamusCollisionDirection.Down))
            throw new ArgumentOutOfRangeException(nameof(direction));

        // Constructs only a whole-pixel target for the broad overlap
        // test. Fractional motion is rounded one extra pixel outward whenever its resulting
        // subposition is nonzero, and two pixels whenever the subposition carries or
        // borrows. Although this can look like an off-by-one in C#, it is the game's exact subtract or add
        // followed by a decrement or increment, whose zero test reads the result of the last step.
        Position16<RoomPixels> target = BuildRoundedTarget(
            state, direction, FixedCoordinate.Of(distance, distanceSubposition));

        foreach (SolidEnemyCollisionBody enemy in interactiveEnemies)
        {
            if (!enemy.Solid)
                continue;

            // The rounded future center must overlap on both axes.
            // Tangency is not overlap here; the unsigned comparison requires the
            // center distance to be strictly less than the sum of both radii.
            if (!StrictlyOverlaps(target.X, state.Radius.X, enemy.Position.X, enemy.Radius.X) ||
                !StrictlyOverlaps(target.Y, state.Radius.Y, enemy.Position.Y, enemy.Radius.Y))
            {
                continue;
            }

            // The broad test says the future boxes overlap. The directional test now uses
            // the *current* leading edges to distinguish an approaching gap from an enemy
            // Samus is already embedded in. A negative result skips that enemy.
            int gap = direction switch
            {
                SamusCollisionDirection.Left => CoordinateAxis.Wrap(
                    state.Position.X.Whole - state.Radius.X - enemy.Position.X - enemy.Radius.X),
                SamusCollisionDirection.Right => CoordinateAxis.Wrap(
                    enemy.Position.X - enemy.Radius.X - state.Position.X.Whole - state.Radius.X),
                SamusCollisionDirection.Up => CoordinateAxis.Wrap(
                    state.Position.Y.Whole - state.Radius.Y - enemy.Position.Y - enemy.Radius.Y),
                SamusCollisionDirection.Down => CoordinateAxis.Wrap(
                    enemy.Position.Y - enemy.Radius.Y - state.Position.Y.Whole - state.Radius.Y),
                // Probe rejected values outside zero through three above. Keep a throwing
                // arm anyway so this switch stays honest if the public enum later grows.
                SamusCollisionDirection.NonDirectionalProbe => throw new InvalidOperationException("Unreachable collision direction."),
                _ => throw new InvalidOperationException("Unreachable collision direction."),
            };

            if (gap != 0 && (gap & 1 << 15) != 0)
                continue;

            bool wasTouching = gap == 0;
            if (wasTouching)
            {
                // Clears Y subposition for every direction. Yes, that clears
                // Samus's *Y* subposition even for a left/right touch. This is an
                // original-game bug; preserving it prevents one-pixel drift
                // differences when repeatedly pressing into a solid or frozen enemy.
                state.Position = state.Position.WithFractionY(0);
            }

            // Success publishes the whole gap and the enemy met; the caller records it for its direction.
            return new SolidEnemyCollisionResult(
                Collided: true,
                Distance: gap,
                EnemySlot: enemy.Slot,
                WasTouching: wasTouching);
        }

        // No collision names no enemy and returns the requested whole distance.
        return new SolidEnemyCollisionResult(
            Collided: false,
            Distance: distance,
            EnemySlot: null,
            WasTouching: false);
    }

    /// <summary>Applies the directional 16.16 movement to the relevant axis and rounds its fractional target outward for the broad-phase test.</summary>
    private static Position16<RoomPixels> BuildRoundedTarget(
        SamusKinematicsState state,
        SamusCollisionDirection direction,
        FixedCoordinate reach)
    {
        Position16<RoomPixels> target = state.Position.Whole;
        return direction switch
        {
            SamusCollisionDirection.Left => target.WithX(RoundNegative(state.Position.X, reach)),
            SamusCollisionDirection.Right => target.WithX(RoundPositive(state.Position.X, reach)),
            SamusCollisionDirection.Up => target.WithY(RoundNegative(state.Position.Y, reach)),
            SamusCollisionDirection.Down => target.WithY(RoundPositive(state.Position.Y, reach)),
            // The pose-observation probe targets Samus's current position.
            SamusCollisionDirection.NonDirectionalProbe => target,
            _ => throw new InvalidOperationException($"Undefined SamusCollisionDirection {direction}."),
        };
    }

    /// <summary>
    /// Models the game's subtraction, including fractional borrow and the extra outward decrement selected by the
    /// zero flag. The whole target is a room coordinate word: each decrement wraps at sixteen bits through the
    /// compatibility counter, so a target at zero continues from 65535 exactly as the game's decrement does.
    /// </summary>
    private static int RoundNegative(FixedCoordinate target, FixedCoordinate reach)
    {
        int whole = CoordinateAxis.Wrap(target.Whole - reach.Whole);

        // The subtraction may borrow from the whole part. Model that borrow explicitly,
        // because the subtraction below keeps only the fraction word.
        bool borrowed = target.Fraction < reach.Fraction;
        int fraction = CoordinateAxis.Wrap(target.Fraction - reach.Fraction);
        // After a borrow the decrement's zero test reads the whole target, so it
        // tests that word rather than the fraction and a second decrement follows unless it is 0.
        if (borrowed)
        {
            whole = WrappingCounter.DecrementWord(whole);
            if (whole != 0)
                whole = WrappingCounter.DecrementWord(whole);
        }
        else if (fraction != 0)
        {
            whole = WrappingCounter.DecrementWord(whole);
        }

        return whole;
    }

    /// <summary>
    /// Models the game's addition, including fractional carry and the extra outward increment selected by the
    /// zero flag. Each increment wraps the whole coordinate word at sixteen bits through the compatibility counter, so
    /// 65535 continues at zero exactly as the game's increment does.
    /// </summary>
    private static int RoundPositive(FixedCoordinate target, FixedCoordinate reach)
    {
        int whole = CoordinateAxis.Wrap(target.Whole + reach.Whole);

        // The add carries the fractional overflow into the whole word before the
        // nonzero-fraction increment performs its outward pixel rounding.
        int fractionSum = target.Fraction + reach.Fraction;
        // After an overflow the increment's zero test reads the whole target, so it
        // tests that word rather than the fraction and a second increment follows unless it is 0.
        if (fractionSum > ushort.MaxValue)
        {
            whole = WrappingCounter.IncrementWord(whole);
            if (whole != 0)
                whole = WrappingCounter.IncrementWord(whole);
        }
        else if (fractionSum != 0)
        {
            whole = WrappingCounter.IncrementWord(whole);
        }

        return whole;
    }

    /// <summary>Compares wrapped 16-bit center distance against both radii using the unsigned borrow and strict comparison rules.</summary>
    private static bool StrictlyOverlaps(
        int firstPosition,
        int firstRadius,
        int secondPosition,
        int secondRadius)
    {
        // Obtains a 16-bit absolute center delta by negating it. This
        // intentionally leaves 32768 unchanged, matching the two's-complement edge.
        int absoluteDelta = CoordinateAxis.Distance(secondPosition, firstPosition);

        // The assembly subtracts the enemy radius first. Borrow means overlap immediately;
        // otherwise an unsigned comparison accepts only a remainder smaller than Samus's radius.
        if (absoluteDelta < secondRadius)
            return true;

        int remainder = CoordinateAxis.Wrap(absoluteDelta - secondRadius);
        return remainder < firstRadius;
    }
}
