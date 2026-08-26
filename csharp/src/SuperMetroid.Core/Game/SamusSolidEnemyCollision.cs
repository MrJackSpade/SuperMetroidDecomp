namespace SuperMetroid.Core.Game;

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
    public static SolidEnemyCollisionResult Probe(
        SamusKinematicsState state,
        IReadOnlyList<SolidEnemyCollisionBody> interactiveEnemies,
        SamusCollisionDirection direction,
        ushort distance,
        ushort distanceSubposition)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(interactiveEnemies);

        if ((uint)direction > (uint)SamusCollisionDirection.Down)
            throw new ArgumentOutOfRangeException(nameof(direction));

        // `$A0:A90A-$A0:A9B7` constructs only a whole-pixel target for the broad overlap
        // test. Fractional motion is rounded one extra pixel outward whenever its resulting
        // subposition is nonzero. Although this can look like an off-by-one in C#, it is the
        // exact SEC/SBC or CLC/ADC plus DEC/INC sequence in the ROM.
        (ushort targetX, ushort targetY) = BuildRoundedTarget(
            state, direction, distance, distanceSubposition);

        foreach (SolidEnemyCollisionBody enemy in interactiveEnemies)
        {
            // `$A0:A9D2-$A0:A9E8`: frozen enemies behave as solid regardless of properties;
            // otherwise enemy property bit 15 is the solid-to-Samus flag.
            if (enemy.FreezeTimer == 0 && (enemy.Properties & 0x8000) == 0)
                continue;

            // `$A0:A9EB-$A0:AA30`: the rounded future center must overlap on both axes.
            // Tangency is not overlap here; the native CMP/BCC sequence requires the
            // center distance to be strictly less than the sum of both radii.
            if (!StrictlyOverlaps(targetX, state.XRadius, enemy.XPosition, enemy.XRadius) ||
                !StrictlyOverlaps(targetY, state.YRadius, enemy.YPosition, enemy.YRadius))
            {
                continue;
            }

            // The broad test says the future boxes overlap. The directional test now uses
            // the *current* leading edges to distinguish an approaching gap from an enemy
            // Samus is already embedded in. A negative native/BPL result skips that enemy.
            ushort gap = direction switch
            {
                SamusCollisionDirection.Left => unchecked((ushort)(
                    state.XPosition - state.XRadius - enemy.XPosition - enemy.XRadius)),
                SamusCollisionDirection.Right => unchecked((ushort)(
                    enemy.XPosition - enemy.XRadius - state.XPosition - state.XRadius)),
                SamusCollisionDirection.Up => unchecked((ushort)(
                    state.YPosition - state.YRadius - enemy.YPosition - enemy.YRadius)),
                SamusCollisionDirection.Down => unchecked((ushort)(
                    enemy.YPosition - enemy.YRadius - state.YPosition - state.YRadius)),
                // Probe rejected values outside zero through three above. Keep a throwing
                // arm anyway so this switch stays honest if the public enum later grows.
                _ => throw new InvalidOperationException("Unreachable collision direction."),
            };

            if (gap != 0 && (gap & 0x8000) != 0)
                continue;

            bool wasTouching = gap == 0;
            if (wasTouching)
            {
                // `$A0:AAC8` executes STZ $0AFC for every direction. Yes, that clears
                // Samus's *Y* subposition even for a left/right touch. The disassembly calls
                // this out as an original-game bug; preserving it prevents one-pixel drift
                // differences when repeatedly pressing into a solid or frozen enemy.
                state.YSubposition = 0;
            }

            // Native success returns A=$FFFF, publishes the whole gap in $12, clears $14,
            // and stores the enemy index in $16 plus a direction-specific diagnostic slot.
            return new SolidEnemyCollisionResult(
                Collided: true,
                Distance: gap,
                DistanceSubposition: 0,
                EnemyIndex: enemy.Index,
                WasTouching: wasTouching,
                TargetXPosition: targetX,
                TargetYPosition: targetY);
        }

        // The target is retained in the managed result solely to make probes inspectable in
        // the debugger. The native no-collision return is simply A=0 and has no enemy index.
        return new SolidEnemyCollisionResult(
            Collided: false,
            Distance: distance,
            DistanceSubposition: distanceSubposition,
            EnemyIndex: null,
            WasTouching: false,
            TargetXPosition: targetX,
            TargetYPosition: targetY);
    }

    private static (ushort X, ushort Y) BuildRoundedTarget(
        SamusKinematicsState state,
        SamusCollisionDirection direction,
        ushort distance,
        ushort distanceSubposition)
    {
        ushort targetX = state.XPosition;
        ushort targetY = state.YPosition;

        switch (direction)
        {
            case SamusCollisionDirection.Left:
                targetX = RoundNegative(state.XPosition, state.XSubposition, distance, distanceSubposition);
                break;

            case SamusCollisionDirection.Right:
                targetX = RoundPositive(state.XPosition, state.XSubposition, distance, distanceSubposition);
                break;

            case SamusCollisionDirection.Up:
                targetY = RoundNegative(state.YPosition, state.YSubposition, distance, distanceSubposition);
                break;

            case SamusCollisionDirection.Down:
                targetY = RoundPositive(state.YPosition, state.YSubposition, distance, distanceSubposition);
                break;
        }

        return (targetX, targetY);
    }

    private static ushort RoundNegative(
        ushort position,
        ushort subposition,
        ushort distance,
        ushort distanceSubposition)
    {
        ushort whole = unchecked((ushort)(position - distance));

        // SEC/SBC sets carry when no unsigned borrow occurred. Model that borrow explicitly,
        // because C# discards it when a result is narrowed back to ushort.
        bool borrowed = subposition < distanceSubposition;
        ushort fraction = unchecked((ushort)(subposition - distanceSubposition));
        if (borrowed)
            whole--;

        if (fraction != 0)
            whole--;

        return whole;
    }

    private static ushort RoundPositive(
        ushort position,
        ushort subposition,
        ushort distance,
        ushort distanceSubposition)
    {
        ushort whole = unchecked((ushort)(position + distance));

        // CLC/ADC carries the fractional overflow into the whole word before the native
        // nonzero-fraction INC performs its outward pixel rounding.
        uint fractionSum = (uint)subposition + distanceSubposition;
        ushort fraction = unchecked((ushort)fractionSum);
        if (fractionSum > ushort.MaxValue)
            whole++;

        if (fraction != 0)
            whole++;

        return whole;
    }

    private static bool StrictlyOverlaps(
        ushort firstPosition,
        ushort firstRadius,
        ushort secondPosition,
        ushort secondRadius)
    {
        // `$A0:A9F1` obtains a 16-bit absolute center delta with EOR #$FFFF / INC. This
        // intentionally leaves $8000 unchanged, matching the native two's-complement edge.
        ushort signedDelta = unchecked((ushort)(secondPosition - firstPosition));
        ushort absoluteDelta = (signedDelta & 0x8000) == 0
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

/// <summary>Low two bits consumed by <c>$A0:A8F0</c>'s direction jump table.</summary>
public enum SamusCollisionDirection : ushort
{
    Left = 0,
    Right = 1,
    Up = 2,
    Down = 3,
}

/// <summary>
/// Collision-relevant words for one entry in native <c>InteractiveEnemyIndices</c>.
/// </summary>
public readonly record struct SolidEnemyCollisionBody(
    ushort Index,
    ushort XPosition,
    ushort YPosition,
    ushort XRadius,
    ushort YRadius,
    ushort FreezeTimer,
    ushort Properties);

/// <summary>Observable outputs of one bank-$A0 solid-enemy collision probe.</summary>
public readonly record struct SolidEnemyCollisionResult(
    bool Collided,
    ushort Distance,
    ushort DistanceSubposition,
    ushort? EnemyIndex,
    bool WasTouching,
    ushort TargetXPosition,
    ushort TargetYPosition);
