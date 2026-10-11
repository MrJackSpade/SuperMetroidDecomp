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

    private void DriftDyingDraygonToBurialPoint(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        if ((body.FrameCounter & 0x000f) == 0)
        {
            ushort foamX = unchecked((ushort)(
                body.Position.X.Whole + (state.FacingRight ? 0x0020 : -0x0020)));
            SpawnRoomSpriteObject(
                new(foamX, unchecked((ushort)(body.Position.Y.Whole - 0x0010))),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphicsIndex: 0);
        }

        // Retail quarters both coordinates before calculating this angle. That is not an
        // algebraic simplification: logical shifts discard low coordinate bits and thereby
        // select slightly different table vectors near the destination.
        short deltaX = unchecked((short)(0x0040 - (body.Position.X.Whole >> 2)));
        short deltaY = unchecked((short)(0x0078 - (body.Position.Y.Whole >> 2)));
        byte cartridgeAngle = CalculateCartridgeAngle(new(deltaX, deltaY));
        byte movementAngle = unchecked((byte)(0x40 - cartridgeAngle));
        state.DeathMovementAngle = movementAngle;
        MoveDraygonAtMovementAngle(body, movementAngle, speed: 1);

        if (!IsWithinStrictModularDistance(body.Position.X.Whole, DraygonDeathTargetX, 4) ||
            !IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonDeathTargetY, 4))
        {
            return;
        }

        SpawnDraygonDeathEvirs(state);
        state.Function = DraygonAiFunction.DyingSink;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        state.FunctionTimer = DraygonDeathWaitFramesUs;
        InstallDraygonInstruction(body, DraygonInstructionProgramDefinitions.Sleep);

        ushort deletedPartProperties = body.Properties.With(EnemyProperties.Deleted);
        state.Tail!.Properties = deletedPartProperties;
        state.Arms!.Properties = deletedPartProperties;
        RoomEnemySlot eye = state.Eye!;
        InstallDraygonInstruction(
            eye,
            state.FacingRight
                ? DraygonInstructionProgramDefinitions.EyeFacingRightDead
                : DraygonInstructionProgramDefinitions.EyeFacingLeftDead);
        eye.VariableA = (ushort)DraygonEyeFunction.Inert;
    }

    private void WaitForDraygonBurialEvirs(
        DraygonEnemyState state,
        byte frameCounterLow)
    {
        SpawnDyingDraygonSmoke(state, frameCounterLow);
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
        byte frameCounterLow)
    {
        SpawnDyingDraygonSmoke(state, frameCounterLow);
        MoveDraygonDeathEvirs();
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - DraygonBurialFinishedY)) < 0)
            return;

        InstallDraygonInstruction(body, DraygonInstructionProgramDefinitions.Delete);

        // $A0:B917 uses Draygon's body header for sixteen independent drops over the
        // lower half of the arena. Do this before deleting the body so the native event is
        // represented by real pickup actors rather than only a debugger-visible flag.
        SpawnEnemyDropScatter(
            EnemyDefinitionId.DraygonBody,
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

    private void SpawnDyingDraygonSmoke(DraygonEnemyState state, byte frameCounterLow)
    {
        if ((frameCounterLow & 7) != 0)
            return;

        ushort random = _nextRandom!();
        ushort x = unchecked((ushort)((random & 0x007f) + 0x00c0));
        ushort y = unchecked((ushort)(((random & 0x3f00) >> 8) + 0x0190));
        if (SpawnRoomSpriteObject(new(x, y), RoomSpriteObjectKind.DustCloud, graphicsIndex: 0) is not null)
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
                new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(state.Body.Position.Y.Whole + yOffset))),
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
        DraygonBurialEvirDefinition definition =
            DraygonBurialEvirDefinitions.ForEntry(entry);
        RoomSpriteObjectSlot? sprite = SpawnRoomSpriteObject(
            new(definition.InitialX, definition.InitialY),
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

            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            sprite.Position = sprite.Position.WithX(AddSpriteObjectSubspeed(
                sprite.Position.X,
                definition.XSubspeed,
                add: ((definition.Angle + 0x0040) & 0x0080) != 0));
            sprite.Position = sprite.Position.WithY(AddSpriteObjectSubspeed(
                sprite.Position.Y,
                definition.YSubspeed,
                add: ((definition.Angle + 0x0080) & 0x0080) != 0));
        }
    }

    private static void MoveDraygonAtMovementAngle(RoomEnemySlot body, byte movementAngle, ushort speed)
    {
        uint xMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x40));
        uint yMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x80));
        body.Position = body.Position.WithX(AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: ((movementAngle + 0x40) & 0x80) != 0));
        body.Position = body.Position.WithY(AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: ((movementAngle + 0x80) & 0x80) != 0));
    }

    private static SnesFixedPosition AddSpriteObjectSubspeed(
        SnesFixedPosition position,
        ushort magnitude,
        bool add)
    {
        uint fixedPosition = position.RawValue;
        fixedPosition = add
            ? unchecked(fixedPosition + magnitude)
            : unchecked(fixedPosition - magnitude);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }

    private void ClearRoomSpriteObjectPool()
    {
        foreach (RoomSpriteObjectSlot sprite in _roomSpriteObjects)
            sprite.Clear();
    }
}
