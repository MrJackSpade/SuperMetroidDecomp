namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain's low-health phase-two red hand beam. The body dispatcher, its bank-$A9
/// bytecode, and the recursively emitted bank-$86 actors are kept together because they
/// communicate through the cartridge's seven shared "next beam" words.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainHandBeamBodyInstruction = 0x9a42;
    private const ushort MotherBrainHandBeamWalkInstruction = 0x9852;
    private const ushort MotherBrainHandBeamInstruction = 0xc796;

    /// <summary>Ports the four-entry body dispatcher at <c>$A9:B87D-B8EA</c>.</summary>
    private void RunMotherBrainHandBeamAttack(MotherBrainEnemyState state)
    {
        switch (state.HandBeamPhase)
        {
            case MotherBrainHandBeamPhase.BackUp:
                if (!TryWalkMotherBrainBackwardsForHandBeam(state))
                    return;
                state.LowerNeckMovementIndex = 8;
                state.UpperNeckMovementIndex = 6;
                state.HandBeamPhase = MotherBrainHandBeamPhase.WaitForBombs;
                return;

            case MotherBrainHandBeamPhase.WaitForBombs:
                // Bombs occupy the same eighteen-slot projectile pool. Native waits for
                // every active shell before starting the visually dense recursive beam.
                if (state.BombCounter != 0)
                    return;
                SetMotherBrainInstructionList(
                    state.Body,
                    MotherBrainHandBeamBodyInstruction);
                state.HandBeamPhase = MotherBrainHandBeamPhase.Firing;
                return;

            case MotherBrainHandBeamPhase.Firing:
                // `$A9:B8C8` is intentionally just RTS. Body bytecode at $9A42 owns the
                // charge effects, projectile emission, 240-frame hold, and phase advance.
                return;

            case MotherBrainHandBeamPhase.Finish:
                SetMotherBrainInstructionList(
                    state.Head!,
                    MotherBrainNeutralPhaseTwoHeadInstruction);
                state.LowerNeckMovementIndex = 2;
                state.UpperNeckMovementIndex = 4;
                state.HandBeamPhase = MotherBrainHandBeamPhase.BackUp;
                state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain hand-beam phase {(ushort)state.HandBeamPhase} is " +
                    "outside the four-entry cartridge dispatcher.");
        }
    }

    /// <summary>
    /// Literal <c>MakeMotherBrainWalkBackwards($28, $08)</c>. Offset eight selects the slow
    /// authored gait; the helper's hard floor at X=$30 can report arrival before target $28.
    /// </summary>
    private static bool TryWalkMotherBrainBackwardsForHandBeam(
        MotherBrainEnemyState state)
    {
        ushort targetMinusBody = unchecked((ushort)(0x0028 - state.Body.XPosition));
        if ((targetMinusBody & 0x8000) == 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (unchecked((short)(state.Body.XPosition - 0x0030)) < 0)
            return true;

        SetMotherBrainInstructionList(
            state.Body,
            MotherBrainHandBeamWalkInstruction);
        return false;
    }

    /// <summary>
    /// Ports charging initializer <c>$86:C605</c>. The body record—not the articulated
    /// head—is the source: its extended map puts the hand at (+$40,-$30).
    /// </summary>
    private void SpawnMotherBrainHandBeamCharging(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemyProjectileSlot? beam = AllocateEnemyProjectile();
        if (beam is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
            graphicsIndex: 0x0040);
        beam.Variable0 = 0;
        beam.Variable1 = 0;
        beam.XVelocity = 0;
        beam.YVelocity = 0;
        beam.XSubposition = 0;
        beam.YSubposition = 0;
        beam.GraphicsIndex = 0x0400;
        beam.XPosition = unchecked((ushort)(state.Body.XPosition + 0x0040));
        beam.YPosition = unchecked((ushort)(state.Body.YPosition - 0x0030));

        state.HandBeamNextXPosition = beam.XPosition;
        state.HandBeamNextXSubposition = 0;
        state.HandBeamNextYPosition = beam.YPosition;
        state.HandBeamNextYSubposition = 0;

        short deltaX = unchecked((short)(samus.XPosition - beam.XPosition));
        short deltaY = unchecked((short)(samus.YPosition - beam.YPosition));
        byte angle = unchecked((byte)(
            0x80 - CalculateCartridgeAngle(deltaX, deltaY)));
        state.HandBeamNextAngle = angle;
        state.HandBeamNextXVelocity = MultiplyCartridgeSinCos(0x0c00, angle);
        state.HandBeamNextYVelocity = MultiplyCartridgeSinCos(
            0x0c00,
            unchecked((byte)(angle + 0x40)));

        // The shared definition already supplies $C796, but retaining the literal pointer
        // here documents the initializer/list coupling and catches malformed ROM records in
        // the audit without replacing the cartridge-authored maps or durations.
        beam.InstructionPointer = MotherBrainHandBeamInstruction;
        beam.InstructionTimer = 1;
    }

    /// <summary>
    /// Ports fired-child initializer <c>$86:C684-C75C</c>. Each child first advances the
    /// shared aimed cursor, then receives a randomized secondary displacement used only for
    /// its spawn frame. Surviving actors become stationary animated emitters; out-of-bounds
    /// children turn into the cartridge's large impact dust and earthquake.
    /// </summary>
    private void SpawnMotherBrainHandBeamFired(ushort parentParameter)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain hand-beam bytecode ran without its encounter state.");
        RoomEnemyProjectileSlot? beam = AllocateEnemyProjectile();
        if (beam is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamFired,
            graphicsIndex: 0);
        beam.XPosition = state.HandBeamNextXPosition;
        beam.XSubposition = state.HandBeamNextXSubposition;
        beam.YPosition = state.HandBeamNextYPosition;
        beam.YSubposition = state.HandBeamNextYSubposition;
        beam.XVelocity = state.HandBeamNextXVelocity;
        beam.YVelocity = state.HandBeamNextYVelocity;

        (beam.XPosition, beam.XSubposition) = AddEightBitVelocity(
            beam.XPosition,
            beam.XSubposition,
            beam.XVelocity);
        (beam.YPosition, beam.YSubposition) = AddEightBitVelocity(
            beam.YPosition,
            beam.YSubposition,
            beam.YVelocity);
        state.HandBeamNextXPosition = beam.XPosition;
        state.HandBeamNextXSubposition = beam.XSubposition;
        state.HandBeamNextYPosition = beam.YPosition;
        state.HandBeamNextYSubposition = beam.YSubposition;

        ushort angleRandom = _nextRandom?.Invoke() ?? 0;
        byte scatterAngle = unchecked((byte)(
            state.HandBeamNextAngle + unchecked((byte)angleRandom)));
        ushort speedRandom = _nextRandom?.Invoke() ?? 0;
        ushort scatterSpeed = unchecked((ushort)(speedRandom & 0x0700));
        beam.XVelocity = MultiplyCartridgeSinCos(scatterSpeed, scatterAngle);
        beam.YVelocity = MultiplyCartridgeSinCos(
            scatterSpeed,
            unchecked((byte)(scatterAngle + 0x40)));
        (beam.XPosition, beam.XSubposition) = AddEightBitVelocity(
            beam.XPosition,
            beam.XSubposition,
            beam.XVelocity);
        (beam.YPosition, beam.YSubposition) = AddEightBitVelocity(
            beam.YPosition,
            beam.YSubposition,
            beam.YVelocity);

        if (IsMotherBrainHandBeamOutsideArena(beam))
        {
            ushort impactX = beam.XPosition;
            ushort impactY = beam.YPosition;
            beam.Clear();
            SpawnRoomGraphicsDustExplosion(
                impactX,
                impactY,
                animationIndex: 0x001d);
            state.LastSoundEffectLibrary3 = 0x0013;
            EarthquakeTimer = 0x000a;
            EarthquakeType = 0x0005;
            return;
        }

        beam.Variable0 = unchecked((ushort)((parentParameter + 1) & 3));
        beam.Variable1 = 0;
        beam.XVelocity = 0;
        beam.YVelocity = 0;
        beam.InstructionPointer = MotherBrainHandBeamInstruction;
        beam.InstructionTimer = 1;
    }

    private static bool IsMotherBrainHandBeamOutsideArena(
        RoomEnemyProjectileSlot beam) =>
        unchecked((short)(beam.YPosition - 0x0022)) < 0 ||
        unchecked((short)(beam.YPosition - 0x00ce)) >= 0 ||
        unchecked((short)(beam.XPosition - 0x0002)) < 0 ||
        unchecked((short)(beam.XPosition - 0x00ee)) >= 0;
}
