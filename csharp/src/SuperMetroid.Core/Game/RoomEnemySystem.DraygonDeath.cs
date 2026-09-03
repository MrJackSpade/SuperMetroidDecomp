namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's frame-driven death and burial program from <c>$A5:9185-$931B</c> and
/// <c>$A5:9FE0-$A0D8</c>. The six Evirs remain real entries in the shared 32-slot sprite
/// object pool; their fractional ROM velocities are not replaced with a host animation.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DraygonDeathTargetX = 0x0100;
    private const ushort DraygonDeathTargetY = 0x01e0;
    private const ushort DraygonBurialFinishedY = 0x0240;
    private const ushort DraygonDeathWaitFramesUs = 0x01a0;
    private const int DraygonDeathEvirSubspeedTable = 0xa5a1af;
    private const int DraygonDeathEvirSpawnPositionTable = 0xa5a1c7;
    private const int DraygonDeathEvirAngleTable = 0xa5a1df;

    private void DriftDyingDraygonToBurialPoint(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        if ((body.FrameCounter & 0x000f) == 0)
        {
            ushort foamX = unchecked((ushort)(
                body.XPosition + (state.FacingRight ? 0x0020 : -0x0020)));
            SpawnRoomSpriteObject(
                foamX,
                unchecked((ushort)(body.YPosition - 0x0010)),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphicsIndex: 0);
        }

        // Retail quarters both coordinates before calculating this angle. That is not an
        // algebraic simplification: logical shifts discard low coordinate bits and thereby
        // select slightly different table vectors near the destination.
        short deltaX = unchecked((short)(0x0040 - (body.XPosition >> 2)));
        short deltaY = unchecked((short)(0x0078 - (body.YPosition >> 2)));
        byte cartridgeAngle = CalculateCartridgeAngle(deltaX, deltaY);
        byte movementAngle = unchecked((byte)(0x40 - cartridgeAngle));
        state.DeathMovementAngle = movementAngle;
        MoveDraygonAtMovementAngle(body, movementAngle, speed: 1);

        if (!IsWithinStrictModularDistance(body.XPosition, DraygonDeathTargetX, 4) ||
            !IsWithinStrictModularDistance(body.YPosition, DraygonDeathTargetY, 4))
        {
            return;
        }

        SpawnDraygonDeathEvirs(state);
        state.Function = DraygonAiFunction.DyingSink;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        state.FunctionTimer = DraygonDeathWaitFramesUs;
        InstallDraygonInstruction(body, DraygonInstructionLists.Ilist_97B9);

        ushort deletedPartProperties = body.Properties.With(EnemyProperties.Deleted);
        state.Tail!.Properties = deletedPartProperties;
        state.Arms!.Properties = deletedPartProperties;
        RoomEnemySlot eye = state.Eye!;
        InstallDraygonInstruction(
            eye,
            state.FacingRight
                ? DraygonInstructionLists.Ilist_9D3E
                : DraygonInstructionLists.Ilist_999C);
        eye.VariableA = 0x804b;
    }

    private void WaitForDraygonBurialEvirs(
        DraygonEnemyState state,
        byte nmiFrameCounter8)
    {
        SpawnDyingDraygonSmoke(state, nmiFrameCounter8);
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if (state.FunctionTimer == 0)
        {
            state.Function = DraygonAiFunction.DyingFinish;
            return;
        }

        MoveDraygonDeathEvirs();
    }

    private void SinkDraygonBelowTheRoom(
        DraygonEnemyState state,
        byte nmiFrameCounter8)
    {
        SpawnDyingDraygonSmoke(state, nmiFrameCounter8);
        MoveDraygonDeathEvirs();
        RoomEnemySlot body = state.Body;
        body.YPosition = unchecked((ushort)(body.YPosition + 1));
        if (unchecked((short)(body.YPosition - DraygonBurialFinishedY)) < 0)
            return;

        InstallDraygonInstruction(body, DraygonInstructionLists.Ilist_98ED);

        // $A0:B917 uses Draygon's body header for sixteen independent drops over the
        // lower half of the arena. Do this before deleting the body so the native event is
        // represented by real pickup actors rather than only a debugger-visible flag.
        SpawnEnemyDropScatter(
            DraygonBodyDefinition,
            count: 16,
            xBase: 128,
            xMask: 0x00ff,
            yBase: 352,
            yMask: 0x3f00);
        ushort deletedProperties = body.Properties.With(EnemyProperties.Deleted);
        body.Properties = deletedProperties;
        state.Eye!.Properties = deletedProperties;
        if (!state.BossDefeatPersisted)
        {
            RequireSetAreaBossDefeated();
            state.BossDefeatPersisted = true;
        }
        state.ItemDropRequested = true;
        ClearRoomSpriteObjectPool();
    }

    private void SpawnDyingDraygonSmoke(DraygonEnemyState state, byte nmiFrameCounter8)
    {
        if ((nmiFrameCounter8 & 7) != 0)
            return;

        ushort random = _nextRandom!();
        ushort x = unchecked((ushort)((random & 0x007f) + 0x00c0));
        ushort y = unchecked((ushort)(((random & 0x3f00) >> 8) + 0x0190));
        if (SpawnRoomSpriteObject(x, y, RoomSpriteObjectKind.DustCloud, graphicsIndex: 0) is not null)
            state.DeathSmokeObjectsSpawned++;
    }

    private void SpawnRandomDyingDraygonObject(
        DraygonEnemyState state,
        RoomSpriteObjectKind kind)
    {
        ushort random = _nextRandom!();
        short xOffset = unchecked((short)((random & 0x007f) - 0x0040));
        short yOffset = unchecked((short)(((random & 0x7f00) >> 8) - 0x0040));
        if (SpawnRoomSpriteObject(
                unchecked((ushort)(state.Body.XPosition + xOffset)),
                unchecked((ushort)(state.Body.YPosition + yOffset)),
                kind,
                graphicsIndex: 0) is not null)
        {
            state.DeathAnimationObjectsSpawned++;
        }
    }

    private void SpawnDraygonDeathEvirs(DraygonEnemyState state)
    {
        ClearRoomSpriteObjectPool();

        // The two native loops share one descending position-table index. Allocation also
        // descends, so these calls deterministically occupy slots 31..26 in movement-table
        // order: three left-facing objects followed by three right-facing objects.
        for (int entry = 5; entry >= 3; entry--)
            SpawnDraygonDeathEvir(entry, RoomSpriteObjectKind.DraygonIntroEvir);
        for (int entry = 2; entry >= 0; entry--)
            SpawnDraygonDeathEvir(entry, RoomSpriteObjectKind.DraygonDeathEvirFacingRight);

        state.DeathEvirsSpawned = true;
    }

    private void SpawnDraygonDeathEvir(int entry, RoomSpriteObjectKind kind)
    {
        int source = DraygonDeathEvirSpawnPositionTable + entry * 4;
        RoomSpriteObjectSlot? sprite = SpawnRoomSpriteObject(
            ReadWord(_bus!, source),
            ReadWord(_bus!, source + 2),
            kind,
            graphicsIndex: EnemyPaletteBits.Palette7);
        if (sprite is null)
        {
            throw new InvalidOperationException(
                "Draygon cleared the sprite-object pool but could not allocate a burial Evir.");
        }
    }

    private void MoveDraygonDeathEvirs()
    {
        for (int entry = 5, slotIndex = RoomSpriteObjectSlotCount - 1;
             entry >= 0;
             entry--, slotIndex--)
        {
            RoomSpriteObjectSlot sprite = _roomSpriteObjects[slotIndex];
            if (!sprite.IsActive || sprite.Kind is not (
                    RoomSpriteObjectKind.DraygonIntroEvir or
                    RoomSpriteObjectKind.DraygonDeathEvirFacingRight))
            {
                throw new InvalidDataException(
                    $"Draygon burial movement expected Evir in sprite slot {slotIndex}.");
            }

            ushort angle = ReadWord(_bus!, DraygonDeathEvirAngleTable + entry * 4);
            ushort xSubspeed = ReadWord(_bus!, DraygonDeathEvirSubspeedTable + entry * 4);
            ushort ySubspeed = ReadWord(_bus!, DraygonDeathEvirSubspeedTable + entry * 4 + 2);
            (sprite.XPosition, sprite.XSubposition) = AddSpriteObjectSubspeed(
                sprite.XPosition,
                sprite.XSubposition,
                xSubspeed,
                add: ((angle + 0x0040) & 0x0080) != 0);
            (sprite.YPosition, sprite.YSubposition) = AddSpriteObjectSubspeed(
                sprite.YPosition,
                sprite.YSubposition,
                ySubspeed,
                add: ((angle + 0x0080) & 0x0080) != 0);
        }
    }

    private void MoveDraygonAtMovementAngle(RoomEnemySlot body, byte movementAngle, ushort speed)
    {
        uint xMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x40));
        uint yMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x80));
        (body.XPosition, body.XSubposition) = AddDraygonAngleMagnitude(
            body.XPosition,
            body.XSubposition,
            xMagnitude,
            subtract: ((movementAngle + 0x40) & 0x80) != 0);
        (body.YPosition, body.YSubposition) = AddDraygonAngleMagnitude(
            body.YPosition,
            body.YSubposition,
            yMagnitude,
            subtract: ((movementAngle + 0x80) & 0x80) != 0);
    }

    private static (ushort Position, ushort Subposition) AddSpriteObjectSubspeed(
        ushort position,
        ushort subposition,
        ushort magnitude,
        bool add)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        fixedPosition = add
            ? unchecked(fixedPosition + magnitude)
            : unchecked(fixedPosition - magnitude);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private void ClearRoomSpriteObjectPool()
    {
        foreach (RoomSpriteObjectSlot sprite in _roomSpriteObjects)
            sprite.Clear();
    }
}
