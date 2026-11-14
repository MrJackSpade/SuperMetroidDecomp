using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's death: drifting to its burial point, the Evirs and sinking below the room.</summary>
[RoomLifetime]
internal sealed class DraygonDeath
{
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Applies common death and drop behavior after Draygon's special sequence.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    private readonly DraygonParts _parts;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonDeath(EnemyRoomBinding room, RoomSpriteObjects spriteObjects, EnemyDeathsAndDrops deathsAndDrops, DraygonParts parts)
    {
        _room = room;
        _spriteObjects = spriteObjects;
        _deathsAndDrops = deathsAndDrops;
        _parts = parts;
    }

    /// <summary>
    /// The pixels along each axis of one cell of the coarse grid the burial drift aims across. Draygon aims from his cell to
    /// the burial point's, so each coordinate's place within its cell is discarded and near the burial point the drift takes
    /// slightly different directions than an aim across whole pixels would.
    /// </summary>
    private const int BurialAimCellPixels = 4;

    /// <summary>The bearing a dying Draygon at <paramref name="body"/> drifts along, aimed across the burial drift's coarse grid.</summary>
    internal static TrigonometryAngle BurialDriftAngle(Position16<RoomPixels> body) =>
        EnemyMath.CalculateBearing(new(
            CoordinateAxis.Difference(DraygonBehavior.DraygonDeathTargetX / BurialAimCellPixels, body.X / BurialAimCellPixels),
            CoordinateAxis.Difference(DraygonBehavior.DraygonDeathTargetY / BurialAimCellPixels, body.Y / BurialAimCellPixels))).ToTrigonometryAngle();

    /// <summary>Moves the dying body toward the burial point and changes its linked eye pose.</summary>
    internal void DriftDyingDraygonToBurialPoint(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        if (DraygonCadenceDefinitions.DyingFoamEverySixteenthUpdate.IsDue(body.FrameCounter))
        {
            _spriteObjects.SpawnRoomSpriteObject(
                body.Position.Whole + new Offset16<RoomPixels>(CoordinateAxis.AsSigned(state.FacingRight ? 32 : -32), -16),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphics: default);
        }

        MoveDraygonAtMovementAngle(body, BurialDriftAngle(body.Position.Whole), speed: 1);

        if (!EnemyMath.IsWithinStrictModularDistance(body.Position.X.Whole, DraygonBehavior.DraygonDeathTargetX, 4) ||
            !EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonBehavior.DraygonDeathTargetY, 4))
        {
            return;
        }

        SpawnDraygonDeathEvirs(state);
        state.Phase = DraygonPhase.DyingSink;
        _parts.RequestDraygonMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
        state.BurialWait = DraygonBehavior.DraygonDeathWaitFramesUs;
        _parts.StartDraygonAnimation(body, DraygonSegment.Sleep);

        // The tail and arms take the body's properties, deleted.
        EnemyPresence deletedPartPresence = body.Presence.With(EnemyPresence.Deleted);
        state.Tail.Contact = body.Contact;
        state.Tail.Processing = body.Processing;
        state.Tail.Presence = deletedPartPresence;
        state.Arms.Contact = body.Contact;
        state.Arms.Processing = body.Processing;
        state.Arms.Presence = deletedPartPresence;
        RoomEnemySlot eye = state.Eye;
        _parts.StartDraygonAnimation(
            eye,
            state.FacingRight
                ? DraygonSegment.EyeFacingRightDead
                : DraygonSegment.EyeFacingLeftDead);
        eye.RequireSpeciesState<DraygonEyeState>().Action = DraygonEyeAction.Inert;
    }

    /// <summary>Advances burial sprite motion while the pre-sink countdown expires.</summary>
    internal void WaitForDraygonBurialEvirs(
        DraygonEnemyState state,
        int frameCounter)
    {
        SpawnDyingDraygonSmoke(frameCounter);
        if (--state.BurialWait == 0)
        {
            state.Phase = DraygonPhase.DyingFinish;
            return;
        }

        MoveDraygonDeathEvirs(state);
    }

    /// <summary>Sinks the body past the room floor, then persists boss defeat and requests its drop.</summary>
    internal void SinkDraygonBelowTheRoom(
        DraygonEnemyState state,
        int frameCounter)
    {
        SpawnDyingDraygonSmoke(frameCounter);
        MoveDraygonDeathEvirs(state);
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
        if (CoordinateAxis.Difference(body.Position.Y.Whole, DraygonBehavior.DraygonBurialFinishedY) < 0)
            return;

        _parts.StartDraygonAnimation(body, DraygonSegment.Delete);

        // The death drops use Draygon's body definition for sixteen independent drops over the
        // lower half of the arena. The drops spawn before the body is deleted.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.DraygonBody,
            count: 16,
            xBase: 128,
            xScatter: EnemyDropScatterDefinitions.WideArenaHorizontalScatter,
            yBase: 352,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        // The body is deleted, and the eye takes the body's properties.
        body.Presence = body.Presence.With(EnemyPresence.Deleted);
        state.Eye.Contact = body.Contact;
        state.Eye.Processing = body.Processing;
        state.Eye.Presence = body.Presence;
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        _spriteObjects.EndAll();
    }

    /// <summary>Consumes room RNG and spawns smoke at the dying body every eighth frame.</summary>
    private void SpawnDyingDraygonSmoke(int frameCounter)
    {
        if (!DraygonCadenceDefinitions.DyingSmokeEveryEighthUpdate.IsDue(frameCounter))
            return;

        int random = _room.Random.Generate();
        Position16<RoomPixels> smoke = new(DraygonRandomDefinitions.DeathSmokeX.Of(random) + 192, DraygonRandomDefinitions.DeathSmokeY.Of(random) + 400);
        _spriteObjects.SpawnRoomSpriteObject(smoke, RoomSpriteObjectKind.DustCloud, graphics: default);
    }

    /// <summary>Ends every room sprite object and spawns six burial Evirs, each carrying the definition that moves it.</summary>
    private void SpawnDraygonDeathEvirs(DraygonEnemyState state)
    {
        _spriteObjects.EndAll();

        // Three left-facing Evirs on the right of the circle, then three right-facing ones on its left.
        foreach (DraygonBurialEvirDefinition evir in DraygonBurialEvirs.InSpawnOrder)
            SpawnDraygonDeathEvir(state, evir);
    }

    /// <summary>
    /// Allocates one burial Evir where its definition places it, drawn as its definition's kind in the Evirs' own colours
    /// (#1544).
    /// </summary>
    internal void SpawnDraygonDeathEvir(DraygonEnemyState state, DraygonBurialEvirDefinition definition)
    {
        Ensure.NotNull(state);
        RoomSpriteObject sprite = _spriteObjects.SpawnRoomSpriteObject(
            definition.Initial,
            definition.Kind,
            graphics: SpriteGraphics.SharedIn(DraygonColours.Evirs));
        state.BurialEvirs.Add(new DraygonBurialEvir(sprite, definition));
    }

    /// <summary>Advances the six burial Evirs, in the order they spawned, by their definitions' velocities.</summary>
    internal static void MoveDraygonDeathEvirs(DraygonEnemyState state)
    {
        foreach (DraygonBurialEvir evir in Ensure.NotNull(state).BurialEvirs)
        {
            RoomSpriteObject sprite = evir.Sprite;
            if (!sprite.IsActive || sprite.Kind is not (
                    RoomSpriteObjectKind.DraygonIntroEvir or
                    RoomSpriteObjectKind.DraygonDeathEvirFacingRight))
            {
                throw new InvalidDataException(
                    $"Draygon burial movement expected the burial Evir at angle {evir.Definition.Angle} to be live.");
            }

            sprite.Position = sprite.Position.Advance(DraygonBurialEvirs.Velocity(evir.Definition));
        }
    }

    /// <summary>Moves the body along a sine-table angle at the supplied speed.</summary>
    private static void MoveDraygonAtMovementAngle(RoomEnemySlot body, TrigonometryAngle movementAngle, int speed)
    {
        UnsignedSixteenSixteen xMagnitude = DraygonMovement.AngleMagnitude(movementAngle, speed, RotationAngle.QuarterTurn);
        UnsignedSixteenSixteen yMagnitude = DraygonMovement.AngleMagnitude(movementAngle, speed, RotationAngle.HalfTurn);
        body.Position = body.Position.WithX(DraygonMovement.AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: EnemyMath.CosineIsNegative(movementAngle)));
        body.Position = body.Position.WithY(DraygonMovement.AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: EnemyMath.SineIsNegative(movementAngle)));
    }
}
