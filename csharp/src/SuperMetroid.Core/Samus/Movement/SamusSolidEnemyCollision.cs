using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Literal translation of Samus-versus-solid-enemy collision detection at
/// <c>$A0:A8F0-$A0:AB18</c>.
/// </summary>
/// <remarks>
/// This is intentionally a probe, not an enemy simulation. The caller supplies the current
/// interactive-enemy list in the same order as native WRAM <c>$18A6</c>. That order matters:
/// the original routine returns the first colliding entry, not the geometrically nearest one.
/// All positions and arithmetic remain 16-bit so wraparound, signed branch tests, and pixel
/// rounding agree with the 65C816 rather than with host-language geometry conventions.
/// </remarks>
public static class SamusSolidEnemyCollision
{
    /// <summary>
    /// Runs one native bank-$A0 directional probe without moving Samus.
    /// </summary>
    /// <param name="state">Samus's current whole/subpixel position and collision radii.</param>
    /// <param name="interactiveEnemies">
    /// Active interactive enemies in native list order. Non-solid, unfrozen entries remain in
    /// the list because the native loop encounters and rejects them individually.
    /// </param>
    /// <param name="direction">Low two bits of native collision movement direction.</param>
    /// <param name="distance">Whole-pixel half of native scratch pair <c>$12.$14</c>.</param>
    /// <param name="distanceSubposition">Fractional half of native scratch pair <c>$12.$14</c>.</param>
    /// <returns>The first qualifying body's current leading-edge gap, or the requested whole distance with no enemy index when no collision is found.</returns>
    /// <remarks>No whole position or enemy body is changed. Exact current-edge contact clears <paramref name="state"/>'s Y subposition even for horizontal probes. The distance pair is an unsigned 16.16 magnitude; direction supplies its sign.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> or <paramref name="interactiveEnemies"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not one of the four directional values 0..3; NonDirectionalProbe is not accepted here.</exception>
    public static SolidEnemyCollisionResult Probe(
        SamusKinematicsState state,
        IReadOnlyList<SolidEnemyCollisionBody> interactiveEnemies,
        SamusCollisionDirection direction,
        ushort distance,
        ushort distanceSubposition)
    {
        Ensure.NotNull(state);
        Ensure.NotNull(interactiveEnemies);

        if ((uint)direction > (uint)SamusCollisionDirection.Down)
            throw new ArgumentOutOfRangeException(nameof(direction));

        // `$A0:A90A-$A0:A9B7` constructs only a whole-pixel target for the broad overlap
        // test. Fractional motion is rounded one extra pixel outward whenever its resulting
        // subposition is nonzero, and two pixels whenever the subposition carries or
        // borrows. Although this can look like an off-by-one in C#, it is the exact SEC/SBC
        // or CLC/ADC plus DEC/INC sequence in the ROM, whose BEQ reads the flags of the last
        // instruction that ran.
        (ushort targetX, ushort targetY) = BuildRoundedTarget(
            state, direction, distance, distanceSubposition);

        foreach (SolidEnemyCollisionBody enemy in interactiveEnemies)
        {
            if (!enemy.Solid)
                continue;

            // `$A0:A9EB-$A0:AA30`: the rounded future center must overlap on both axes.
            // Tangency is not overlap here; the native CMP/BCC sequence requires the
            // center distance to be strictly less than the sum of both radii.
            if (!StrictlyOverlaps(targetX, state.Radius.X, enemy.Position.X, enemy.Radius.X) ||
                !StrictlyOverlaps(targetY, state.Radius.Y, enemy.Position.Y, enemy.Radius.Y))
            {
                continue;
            }

            // The broad test says the future boxes overlap. The directional test now uses
            // the *current* leading edges to distinguish an approaching gap from an enemy
            // Samus is already embedded in. A negative native/BPL result skips that enemy.
            ushort gap = direction switch
            {
                SamusCollisionDirection.Left => unchecked((ushort)(
                    state.Position.X.Whole - state.Radius.X - enemy.Position.X - enemy.Radius.X)),
                SamusCollisionDirection.Right => unchecked((ushort)(
                    enemy.Position.X - enemy.Radius.X - state.Position.X.Whole - state.Radius.X)),
                SamusCollisionDirection.Up => unchecked((ushort)(
                    state.Position.Y.Whole - state.Radius.Y - enemy.Position.Y - enemy.Radius.Y)),
                SamusCollisionDirection.Down => unchecked((ushort)(
                    enemy.Position.Y - enemy.Radius.Y - state.Position.Y.Whole - state.Radius.Y)),
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
                // `$A0:AAC8` executes STZ $0AFC for every direction. Yes, that clears
                // Samus's *Y* subposition even for a left/right touch. The disassembly calls
                // this out as an original-game bug; preserving it prevents one-pixel drift
                // differences when repeatedly pressing into a solid or frozen enemy.
                state.Position = state.Position.WithFractionY(0);
            }

            // Native success returns A=$FFFF, publishes the whole gap in $12, clears $14,
            // and stores the enemy index in $16 plus a direction-specific diagnostic slot.
            return new SolidEnemyCollisionResult(
                Collided: true,
                Distance: gap,
                EnemySlot: enemy.Slot,
                EnemyIndex: enemy.Index,
                WasTouching: wasTouching);
        }

        // The target is retained in the managed result solely to make probes inspectable in
        // the debugger. The native no-collision return is simply A=0 and has no enemy index.
        return new SolidEnemyCollisionResult(
            Collided: false,
            Distance: distance,
            EnemySlot: null,
            EnemyIndex: null,
            WasTouching: false);
    }

    /// <summary>Applies the directional 16.16 movement to the relevant axis and rounds its fractional target outward for the native broad-phase test.</summary>
    private static Position16<RoomPixels> BuildRoundedTarget(
        SamusKinematicsState state,
        SamusCollisionDirection direction,
        ushort distance,
        ushort distanceSubposition)
    {
        ushort targetX = state.Position.X.Whole;
        ushort targetY = state.Position.Y.Whole;

        switch (direction)
        {
            case SamusCollisionDirection.Left:
                targetX = RoundNegative(state.Position.X, new SnesFixedPosition(distance, distanceSubposition));
                break;

            case SamusCollisionDirection.Right:
                targetX = RoundPositive(state.Position.X, new SnesFixedPosition(distance, distanceSubposition));
                break;

            case SamusCollisionDirection.Up:
                targetY = RoundNegative(state.Position.Y, new SnesFixedPosition(distance, distanceSubposition));
                break;

            case SamusCollisionDirection.Down:
                targetY = RoundPositive(state.Position.Y, new SnesFixedPosition(distance, distanceSubposition));
                break;
            // The pose-observation probe targets Samus's current position.
            case SamusCollisionDirection.NonDirectionalProbe:
                break;
            default:
                throw new InvalidOperationException($"Undefined SamusCollisionDirection {direction}.");
        }

        return new Position16<RoomPixels>(targetX, targetY);
    }

    /// <summary>Models native subtraction, including fractional borrow and the extra outward decrement selected by the assembly's zero flag.</summary>
    private static ushort RoundNegative(SnesFixedPosition target, SnesFixedPosition reach)
    {
        ushort position = target.Whole, subposition = target.Fraction;
        ushort distance = reach.Whole, distanceSubposition = reach.Fraction;
        ushort whole = unchecked((ushort)(position - distance));

        // SEC/SBC sets carry when no unsigned borrow occurred. Model that borrow explicitly,
        // because C# discards it when a result is narrowed back to ushort.
        bool borrowed = subposition < distanceSubposition;
        ushort fraction = unchecked((ushort)(subposition - distanceSubposition));
        // `$A0:A931/$A980`: after a borrow the DEC sets Z from the whole target, so the BEQ
        // tests that word rather than the fraction and a second DEC follows unless it is 0.
        if (borrowed)
        {
            whole--;
            if (whole != 0)
                whole--;
        }
        else if (fraction != 0)
        {
            whole--;
        }

        return whole;
    }

    /// <summary>Models native addition, including fractional carry and the extra outward increment selected by the assembly's zero flag.</summary>
    private static ushort RoundPositive(SnesFixedPosition target, SnesFixedPosition reach)
    {
        ushort position = target.Whole, subposition = target.Fraction;
        ushort distance = reach.Whole, distanceSubposition = reach.Fraction;
        ushort whole = unchecked((ushort)(position + distance));

        // CLC/ADC carries the fractional overflow into the whole word before the native
        // nonzero-fraction INC performs its outward pixel rounding.
        uint fractionSum = (uint)subposition + distanceSubposition;
        ushort fraction = unchecked((ushort)fractionSum);
        // `$A0:A959/$A9A7`: after a carry the INC sets Z from the whole target, so the BEQ
        // tests that word rather than the fraction and a second INC follows unless it is 0.
        if (fractionSum > ushort.MaxValue)
        {
            whole++;
            if (whole != 0)
                whole++;
        }
        else if (fraction != 0)
        {
            whole++;
        }

        return whole;
    }

    /// <summary>Compares wrapped 16-bit center distance against both radii using the native unsigned borrow and strict comparison rules.</summary>
    private static bool StrictlyOverlaps(
        ushort firstPosition,
        ushort firstRadius,
        ushort secondPosition,
        ushort secondRadius)
    {
        // `$A0:A9F1` obtains a 16-bit absolute center delta with EOR #$FFFF / INC. This
        // intentionally leaves $8000 unchanged, matching the native two's-complement edge.
        ushort signedDelta = unchecked((ushort)(secondPosition - firstPosition));
        ushort absoluteDelta = unchecked((short)signedDelta) >= 0
            ? signedDelta
            : unchecked((ushort)(~signedDelta + 1));

        // The assembly subtracts the enemy radius first. Borrow means overlap immediately;
        // otherwise BCC after CMP accepts only a remainder smaller than Samus's radius.
        if (absoluteDelta < secondRadius)
            return true;

        ushort remainder = unchecked((ushort)(absoluteDelta - secondRadius));
        return remainder < firstRadius;
    }
}
