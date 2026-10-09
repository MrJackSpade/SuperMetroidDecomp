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
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(interactiveEnemies);

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
            // `$A0:A9D2-$A0:A9E8`: frozen enemies behave as solid regardless of properties;
            // otherwise enemy property bit 15 is the solid-to-Samus flag.
            if (enemy.FreezeTimer == 0 &&
                !enemy.Properties.HasAny(EnemyProperties.SolidToSamus))
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
                EnemyIndex: enemy.Index,
                WasTouching: wasTouching);
        }

        // The target is retained in the managed result solely to make probes inspectable in
        // the debugger. The native no-collision return is simply A=0 and has no enemy index.
        return new SolidEnemyCollisionResult(
            Collided: false,
            Distance: distance,
            EnemyIndex: null,
            WasTouching: false);
    }

    /// <summary>Builds the whole-pixel future center used by the broad overlap check along the requested axis.</summary>
    /// <param name="state">Current position and subpixel coordinates from which movement begins.</param>
    /// <param name="direction">Axis and sign of the directional probe.</param>
    /// <param name="distance">Whole-pixel portion of the movement magnitude.</param>
    /// <param name="distanceSubposition">Fractional portion of the movement magnitude.</param>
    /// <returns>A target whose probed coordinate uses the native outward pixel rounding rules.</returns>
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

    /// <summary>Subtracts a 16.16 distance and applies the native borrow-sensitive outward rounding.</summary>
    /// <param name="position">Starting whole-pixel coordinate.</param>
    /// <param name="subposition">Starting fractional coordinate.</param>
    /// <param name="distance">Whole-pixel amount to subtract.</param>
    /// <param name="distanceSubposition">Fractional amount to subtract.</param>
    /// <returns>The wrapped 16-bit whole-pixel coordinate used by the broad collision test.</returns>
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

    /// <summary>Adds a 16.16 distance and applies the native carry-sensitive outward rounding.</summary>
    /// <param name="position">Starting whole-pixel coordinate.</param>
    /// <param name="subposition">Starting fractional coordinate.</param>
    /// <param name="distance">Whole-pixel amount to add.</param>
    /// <param name="distanceSubposition">Fractional amount to add.</param>
    /// <returns>The wrapped 16-bit whole-pixel coordinate used by the broad collision test.</returns>
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

    /// <summary>Tests axis-aligned collision extents using the native strict center-distance comparison.</summary>
    /// <param name="firstPosition">Center coordinate of the first body.</param>
    /// <param name="firstRadius">Half-extent of the first body on this axis.</param>
    /// <param name="secondPosition">Center coordinate of the second body.</param>
    /// <param name="secondRadius">Half-extent of the second body on this axis.</param>
    /// <returns><see langword="true"/> only when the extents overlap; exact tangency is excluded.</returns>
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
    /// <summary>Native jump-table value 0 at $A0:A911: probes toward decreasing room X using the current left edge.</summary>
    Left = 0,
    /// <summary>Native jump-table value 1 at $A0:A913: probes toward increasing room X using the current right edge.</summary>
    Right = 1,
    /// <summary>Native jump-table value 2 at $A0:A915: probes toward decreasing room Y using the current top edge.</summary>
    Up = 2,
    /// <summary>Native jump-table value 3 at $A0:A917: probes toward increasing room Y using the current bottom edge.</summary>
    Down = 3,
    /// <summary>Bank-$94:96E3 pose-observation direction $F; not an actual downward contact.</summary>
    NonDirectionalProbe = 0x0f,
}

/// <summary>
/// Collision-relevant words for one entry in native <c>InteractiveEnemyIndices</c>.
/// </summary>
/// <param name="Index">Native enemy-slot byte index, ordinarily slot number times $40; not this body's ordinal in the interactive list.</param>
/// <param name="XPosition">Whole-pixel room X of the collision-box center, retained as a wrapping 16-bit word.</param>
/// <param name="YPosition">Whole-pixel room Y of the collision-box center, increasing downward.</param>
/// <param name="XRadius">Horizontal collision half-extent in pixels.</param>
/// <param name="YRadius">Vertical collision half-extent in pixels.</param>
/// <param name="FreezeTimer">Native frozen countdown word; any nonzero value makes the body solid regardless of its properties.</param>
/// <param name="Properties">Raw enemy property word; bit $8000 makes an unfrozen body solid to Samus. Other property bits are not filtered by this probe.</param>
public readonly record struct SolidEnemyCollisionBody(
    ushort Index,
    ushort XPosition,
    ushort YPosition,
    ushort XRadius,
    ushort YRadius,
    ushort FreezeTimer,
    ushort Properties);

/// <summary>Observable outputs of one bank-$A0 solid-enemy collision probe.</summary>
/// <param name="Collided">True when the rounded future box overlaps a qualifying body and the current directional gap is not negative.</param>
/// <param name="Distance">Unsigned whole-pixel current-edge gap on collision, or the caller's requested whole distance on failure; no fractional distance is returned.</param>
/// <param name="EnemyIndex">Selected native enemy-slot byte index on collision, or null on failure.</param>
/// <param name="WasTouching">True for a successful zero-gap contact, which clears Samus's Y subposition; false for positive gaps and failed probes.</param>
public readonly record struct SolidEnemyCollisionResult(
    bool Collided,
    ushort Distance,
    ushort? EnemyIndex,
    bool WasTouching);
