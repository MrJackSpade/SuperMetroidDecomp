using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's death: drifting to its burial point, the Evirs and sinking below the room.</summary>
[RoomLifetime]
internal sealed class DraygonDeath
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Applies common death and drop behavior after Draygon's special sequence.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
    private readonly DraygonParts _parts;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonDeath(EnemyRoomBinding room, RoomSpriteObjectPool spriteObjects, EnemyDeathsAndDrops deathsAndDrops, DraygonParts parts)
    {
        _room = room;
        _spriteObjects = spriteObjects;
        _deathsAndDrops = deathsAndDrops;
        _parts = parts;
    }

    /// <summary>Moves the dying body toward the burial point and changes its linked eye pose.</summary>
    internal void DriftDyingDraygonToBurialPoint(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        if ((body.FrameCounter & 15) == 0)
        {
            _spriteObjects.SpawnRoomSpriteObject(
                body.Position.Whole + new Offset16<RoomPixels>((short)(state.FacingRight ? 32 : -32), -16),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphics: default);
        }

        // Retail quarters both coordinates before calculating this angle. That is not an
        // algebraic simplification: logical shifts discard low coordinate bits and thereby
        // select slightly different table vectors near the destination.
        ByteAngle movementAngle = EnemyMath.CalculateBearing(new(
            unchecked((short)(64 - (body.Position.X.Whole >> 2))),
            unchecked((short)(120 - (body.Position.Y.Whole >> 2))))).ToTableAngle();
        MoveDraygonAtMovementAngle(body, movementAngle, speed: 1);

        if (!EnemyMath.IsWithinStrictModularDistance(body.Position.X.Whole, DraygonBehavior.DraygonDeathTargetX, 4) ||
            !EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonBehavior.DraygonDeathTargetY, 4))
        {
            return;
        }

        SpawnDraygonDeathEvirs(state);
        state.Function = DraygonAiFunction.DyingSink;
        state.MusicRequest = MusicRequest.SelectTrack(3);
        state.BurialWait = WrappingCountdown.Start(DraygonBehavior.DraygonDeathWaitFramesUs);
        _parts.StartDraygonProgram(body, DraygonProgram.Sleep);

        EnemyProperties deletedPartProperties = body.Properties.With(EnemyProperties.Deleted);
        state.Tail.Properties = deletedPartProperties;
        state.Arms.Properties = deletedPartProperties;
        RoomEnemySlot eye = state.Eye;
        _parts.StartDraygonProgram(
            eye,
            state.FacingRight
                ? DraygonProgram.EyeFacingRightDead
                : DraygonProgram.EyeFacingLeftDead);
        eye.RequireSpeciesState<DraygonEyeState>().Function = DraygonEyeFunction.Inert;
    }

    /// <summary>Advances burial sprite motion while the pre-sink countdown expires.</summary>
    internal void WaitForDraygonBurialEvirs(
        DraygonEnemyState state,
        int frameCounterLow)
    {
        SpawnDyingDraygonSmoke(state, frameCounterLow);
        state.BurialWait = state.BurialWait.Decrement().Remaining;
        if (state.BurialWait.IsZero)
        {
            state.Function = DraygonAiFunction.DyingFinish;
            return;
        }

        MoveDraygonDeathEvirs();
    }

    /// <summary>Sinks the body past the room floor, then persists boss defeat and requests its drop.</summary>
    internal void SinkDraygonBelowTheRoom(
        DraygonEnemyState state,
        int frameCounterLow)
    {
        SpawnDyingDraygonSmoke(state, frameCounterLow);
        MoveDraygonDeathEvirs();
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - DraygonBehavior.DraygonBurialFinishedY)) < 0)
            return;

        _parts.StartDraygonProgram(body, DraygonProgram.Delete);

        // $A0:B917 uses Draygon's body header for sixteen independent drops over the
        // lower half of the arena. Do this before deleting the body so the native event is
        // represented by real pickup actors rather than only a debugger-visible flag.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.DraygonBody,
            count: 16,
            xBase: 128,
            xMask: byte.MaxValue,
            yBase: 352,
            yMask: 63);
        EnemyProperties deletedProperties = body.Properties.With(EnemyProperties.Deleted);
        body.Properties = deletedProperties;
        state.Eye.Properties = deletedProperties;
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        state.ItemDropRequested = true;
        ClearRoomSpriteObjectPool();
    }

    /// <summary>Consumes room RNG and spawns smoke at the dying body every eighth frame.</summary>
    private void SpawnDyingDraygonSmoke(DraygonEnemyState state, int frameCounterLow)
    {
        if ((frameCounterLow & 7) != 0)
            return;

        int random = _room.Random.Generate();
        Position16<RoomPixels> smoke = new((ushort)((random & 127) + 192), (ushort)((SharedRandomState.UpperByte(random) & 63) + 400));
        if (_spriteObjects.SpawnRoomSpriteObject(smoke, RoomSpriteObjectKind.DustCloud, graphics: default) is not null)
            state.DeathSmokeObjectsSpawned++;
    }

    /// <summary>Clears the room sprite pool and spawns six burial Evirs, which take the first six slots in processing order.</summary>
    private void SpawnDraygonDeathEvirs(DraygonEnemyState state)
    {
        ClearRoomSpriteObjectPool();

        // The two loops share one descending position-table index. Each spawn takes the next slot in processing order,
        // so the Evirs occupy the first six slots in movement-table order: three left-facing objects followed by three
        // right-facing objects.
        for (int entry = 5; entry >= 3; entry--)
            SpawnDraygonDeathEvir(entry, RoomSpriteObjectKind.DraygonIntroEvir);
        for (int entry = 2; entry >= 0; entry--)
            SpawnDraygonDeathEvir(entry, RoomSpriteObjectKind.DraygonDeathEvirFacingRight);

        state.DeathEvirsSpawned = true;
    }

    /// <summary>Allocates one burial Evir from its definition-table position and kind.</summary>
    internal void SpawnDraygonDeathEvir(int entry, RoomSpriteObjectKind kind)
    {
        DraygonBurialEvirDefinition definition =
            DraygonBurialEvirs.ForEntry(entry);
        RoomSpriteObjectSlot? sprite = _spriteObjects.SpawnRoomSpriteObject(
            definition.Initial,
            kind,
            graphics: SpriteGraphics.CharacterZeroIn(new EncounterSpriteColouring(BossColourDefinitions.DraygonSprite)));
        if (sprite is null)
        {
            throw new InvalidOperationException(
                "Draygon cleared the sprite-object pool but could not allocate a burial Evir.");
        }
    }

    /// <summary>Advances the six burial Evir sprite slots by their table-defined velocities.</summary>
    internal void MoveDraygonDeathEvirs()
    {
        // The first Evir in processing order moves by the last table entry, and so on down to the first.
        int entry = DraygonBurialEvirDefinitions.EntryCount - 1;
        foreach (RoomSpriteObjectSlot sprite in _spriteObjects.InProcessingOrder.Take(DraygonBurialEvirDefinitions.EntryCount))
        {
            if (!sprite.IsActive || sprite.Kind is not (
                    RoomSpriteObjectKind.DraygonIntroEvir or
                    RoomSpriteObjectKind.DraygonDeathEvirFacingRight))
            {
                throw new InvalidDataException(
                    $"Draygon burial movement expected Evir in sprite slot {sprite.SlotIndex}.");
            }

            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirs.ForEntry(entry);
            sprite.Position = sprite.Position.Advance(DraygonBurialEvirs.Velocity(definition));
            entry--;
        }
    }

    /// <summary>Moves the body along a native sine-table angle at the supplied speed.</summary>
    private static void MoveDraygonAtMovementAngle(RoomEnemySlot body, ByteAngle movementAngle, int speed)
    {
        uint xMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            checked((ushort)speed),
            angleOffset: RotationAngle.QuarterTurn.TableIndex));
        uint yMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            checked((ushort)speed),
            angleOffset: RotationAngle.HalfTurn.TableIndex));
        body.Position = body.Position.WithX(DraygonMovement.AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: EnemyMath.CosineIsNegative(movementAngle)));
        body.Position = body.Position.WithY(DraygonMovement.AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: EnemyMath.SineIsNegative(movementAngle)));
    }

    /// <summary>Releases room sprite slots before allocating the ordered burial Evir set.</summary>
    private void ClearRoomSpriteObjectPool()
    {
        foreach (RoomSpriteObjectSlot sprite in _spriteObjects.Slots)
            sprite.Clear();
    }
}
