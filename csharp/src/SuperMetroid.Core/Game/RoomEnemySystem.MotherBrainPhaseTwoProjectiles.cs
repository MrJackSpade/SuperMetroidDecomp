namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical bank-$86 actors emitted by Mother Brain's phase-two head bytecode. Keeping
/// these in the room's shared eighteen-slot pool preserves allocation failure, same-frame
/// projectile processing, collision, and OAM ordering with the existing room turrets.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private static ReadOnlySpan<short> MotherBrainDroolXOffsets =>
        [6, 14, 8, 10, 11, 12];

    private static ReadOnlySpan<short> MotherBrainDroolYOffsets =>
        [20, 18, 23, 19, 25, 18];

    /// <summary>
    /// Ports head opcode <c>$A9:9E29</c> and initializer <c>$86:C2F3</c>. The shared
    /// allocator deliberately runs after the twelve room turrets and all lingering drool,
    /// so pool saturation drops the ring exactly as it does on hardware.
    /// </summary>
    private void SpawnMotherBrainOnionRing(
        MotherBrainEnemyState state,
        byte angle)
    {
        RoomEnemyProjectileSlot? ring = AllocateEnemyProjectile();
        if (ring is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            ring,
            RoomEnemyProjectileKind.MotherBrainOnionRing,
            graphicsIndex: 0x0400);
        ring.Variable0 = 8;
        ring.Variable1 = 0;
        ring.DirectionParameter = angle;
        ring.XVelocity = MultiplyCartridgeSinCos(0x0450, angle);
        ring.YVelocity = MultiplyCartridgeSinCos(
            0x0450,
            unchecked((byte)(angle + 0x40)));

        // `$C2F3` falls through into `$C320`, making the mouth position observable before
        // the projectile scheduler performs the first of eight delayed attachment frames.
        PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Ports head opcode <c>$A9:9B3C</c> and initializer <c>$86:C843</c>.</summary>
    private void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        ushort parameter = unchecked((ushort)(state.DroolProjectileParameter + 1));
        if (parameter >= 6)
            parameter = 0;
        state.DroolProjectileParameter = parameter;

        RoomEnemyProjectileSlot? drool = AllocateEnemyProjectile();
        if (drool is null)
            return;

        RoomEnemyProjectileKind kind = state.NeckAngleDelta < 0x0080
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        InitializeEnemyProjectileFromDefinition(drool, kind, graphicsIndex: 0);
        drool.Variable0 = parameter;

        // SpawnEnemyProjectile runs initializer C843 immediately. It falls through into
        // the attached pre-instruction, so debugger-visible coordinates are valid before
        // the later global projectile pass repeats the same attachment calculation.
        RunMotherBrainAttachedDroolPreInstruction(drool);
    }

    /// <summary>Ports head opcode <c>$A9:9B6D</c> and initializer <c>$86:CA6A</c>.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? breath = AllocateEnemyProjectile();
        if (breath is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
            graphicsIndex: 0);
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain purple breath requires the linked head record.");
        breath.XPosition = unchecked((ushort)(head.XPosition + 6));
        breath.YPosition = unchecked((ushort)(head.YPosition + 0x0010));
    }

    /// <summary>Exact six-position mouth attachment table at <c>$86:C86E-C885</c>.</summary>
    private void RunMotherBrainAttachedDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain drool ran without its multipart encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain drool requires the linked head record.");
        int parameter = drool.Variable0;
        if ((uint)parameter >= 6)
        {
            throw new InvalidDataException(
                $"Mother Brain drool offset parameter {parameter} is outside 0..5.");
        }

        drool.XPosition = unchecked((ushort)(
            head.XPosition + MotherBrainDroolXOffsets[parameter]));
        drool.YPosition = unchecked((ushort)(
            head.YPosition + MotherBrainDroolYOffsets[parameter]));
        drool.XVelocity = 0;
        drool.YVelocity = 0;
    }

    /// <summary>
    /// Ports <c>$86:C886-C8AF</c>. The native actor uses signed 8.8 velocity, adds twelve
    /// units of gravity per frame, and switches to its four-map splash at world Y $D7.
    /// </summary>
    private static void RunMotherBrainFallingDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        drool.YVelocity = unchecked((ushort)(drool.YVelocity + 0x000c));
        (drool.YPosition, drool.YSubposition) = AddEightBitVelocity(
            drool.YPosition,
            drool.YSubposition,
            drool.YVelocity);
        if (drool.YPosition < 0x00d7)
            return;

        drool.YPosition = unchecked((ushort)(drool.YPosition - 4));
        drool.InstructionPointer = 0xc8e1;
        drool.InstructionTimer = 1;
    }

    /// <summary>
    /// Ports <c>$86:C335-C431</c>. A live Baby slot is tested before Samus and the room,
    /// matching the private projectile's cross-enemy collision order during phase three.
    /// </summary>
    private void RunMotherBrainOnionRingPreInstruction(
        RoomEnemyProjectileSlot ring,
        SamusState? samus,
        ushort cameraX)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain onion ring ran without its multipart encounter state.");

        if (ring.Variable0 != 0)
        {
            ring.Variable0 = unchecked((ushort)(ring.Variable0 - 1));
            PinMotherBrainOnionRingToMouth(ring, state);
            return;
        }

        (ring.XPosition, ring.XSubposition) = AddEightBitVelocity(
            ring.XPosition,
            ring.XSubposition,
            ring.XVelocity);
        (ring.YPosition, ring.YSubposition) = AddEightBitVelocity(
            ring.YPosition,
            ring.YSubposition,
            ring.YVelocity);

        BabyMetroidCutsceneState? baby = state.BabyMetroid;
        if (baby is { IsDeleted: false })
        {
            // `$C3B1-$C3C8` deletes every later ring as soon as the still-registered Baby
            // reaches zero health. Native clears only the projectile ID on this branch.
            if (baby.Health == 0)
            {
                ring.Kind = RoomEnemyProjectileKind.None;
                return;
            }

            if (MotherBrainOnionRingOverlapsBaby(ring, baby))
            {
                state.PendingBabyCryCount = unchecked((ushort)(state.PendingBabyCryCount + 1));
                _ = baby.ApplyMotherBrainOnionRingHit();
                ushort ringX = ring.XPosition;
                ushort ringY = ring.YPosition;
                ExplodeMotherBrainOnionRing(ring, state, ringX, ringY);
                return;
            }
        }

        // This private collision path intentionally ignores Samus's current invincibility
        // timer. It writes a fresh $60 only after applying suit-divided damage.
        if (samus is not null && MotherBrainOnionRingOverlapsSamus(ring, samus))
        {
            ushort ringX = ring.XPosition;
            ushort ringY = ring.YPosition;
            ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? (ushort)(0x0050 >> 2)
                : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? (ushort)(0x0050 >> 1)
                    : (ushort)0x0050;
            samus.Health = samus.Health <= damage
                ? (ushort)0
                : unchecked((ushort)(samus.Health - damage));
            samus.InvincibilityTimer = 0x0060;
            samus.KnockbackTimer = 5;
            samus.KnockbackXDirection = unchecked((short)(samus.XPosition - ringX)) >= 0
                ? (ushort)1
                : (ushort)0;
            ExplodeMotherBrainOnionRing(ring, state, ringX, ringY);
            return;
        }

        short signedY = unchecked((short)ring.YPosition);
        short signedX = unchecked((short)ring.XPosition);
        short screenX = unchecked((short)(ring.XPosition - cameraX));
        if (signedY < 0x20 || ring.YPosition >= 0x00d8 || signedX < 0 ||
            screenX < 0 || screenX >= 0x00f8)
        {
            EarthquakeTimer = 10;
            EarthquakeType = 5;
            ExplodeMotherBrainOnionRing(
                ring,
                state,
                ring.XPosition,
                ring.YPosition);
        }
    }

    private static void PinMotherBrainOnionRingToMouth(
        RoomEnemyProjectileSlot ring,
        MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain onion ring requires the linked head record.");
        ring.XPosition = unchecked((ushort)(head.XPosition + 0x000a));
        ring.YPosition = unchecked((ushort)(head.YPosition + 0x0010));
    }

    private static bool MotherBrainOnionRingOverlapsSamus(
        RoomEnemyProjectileSlot ring,
        SamusState samus) =>
        WrappedMagnitude(unchecked((ushort)(ring.XPosition - samus.XPosition))) <
            ring.XRadius + samus.Kinematics.XRadius &&
        WrappedMagnitude(unchecked((ushort)(ring.YPosition - samus.YPosition))) <
            ring.YRadius + samus.Kinematics.YRadius;

    private static bool MotherBrainOnionRingOverlapsBaby(
        RoomEnemyProjectileSlot ring,
        BabyMetroidCutsceneState baby) =>
        WrappedMagnitude(unchecked((ushort)(ring.XPosition - baby.XPosition))) <
            ring.XRadius + BabyMetroidCutsceneState.XHitboxRadius &&
        WrappedMagnitude(unchecked((ushort)(ring.YPosition - baby.YPosition))) <
            ring.YRadius + BabyMetroidCutsceneState.YHitboxRadius;

    private void ExplodeMotherBrainOnionRing(
        RoomEnemyProjectileSlot ring,
        MotherBrainEnemyState state,
        ushort x,
        ushort y)
    {
        // `$C410` clears the source before allocating parameter-three room dust. If the
        // allocator reuses this physical slot, the outer projectile scheduler will advance
        // the newborn dust list during the same pass, matching EprojRunOne's original X.
        ring.Clear();
        SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 3);
        state.LastSoundEffectLibrary3 = 0x0013;
    }
}
