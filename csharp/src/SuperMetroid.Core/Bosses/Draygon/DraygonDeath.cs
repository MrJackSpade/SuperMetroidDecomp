using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's death: drifting to its burial point, the Evirs and sinking below the room.</summary>
[RoomLifetime]
internal sealed class DraygonDeath
{
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Applies common death and drop behavior after Draygon's special sequence.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    private readonly DraygonParts _parts;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonDeath(EnemyRoomBinding room, RoomEffectActors effectActors, EnemyDeathsAndDrops deathsAndDrops, DraygonParts parts)
    {
        _room = room;
        _effectActors = effectActors;
        _deathsAndDrops = deathsAndDrops;
        _parts = parts;
    }

    /// <summary>The direction a dying Draygon at <paramref name="body"/> drifts along, aimed across the burial drift's coarse grid.</summary>
    private static TrigonometryAngle BurialDriftAngle(Position16<RoomPixels> body) =>
        EnemyMath.CalculateAimAngle(new(
            DraygonArenaDefinitions.DeathTargetX / DraygonArenaDefinitions.BurialAimCellPixels - body.X / DraygonArenaDefinitions.BurialAimCellPixels,
            DraygonArenaDefinitions.DeathTargetY / DraygonArenaDefinitions.BurialAimCellPixels - body.Y / DraygonArenaDefinitions.BurialAimCellPixels));

    /// <summary>Moves the dying body toward the burial point and changes its linked eye pose.</summary>
    internal void DriftDyingDraygonToBurialPoint(DraygonEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        if (DraygonCadenceDefinitions.DyingFoam.IsDue(body.FrameCounter))
        {
            _effectActors.SpawnRoomEffectActor(
                body.Position.Whole + DraygonParts.TowardFacing(DraygonArenaDefinitions.MouthFacingRight, state.Facing),
                RoomEffectActorKind.DraygonFoam,
                graphics: SpriteGraphics.SharedStandard);
        }

        DraygonMovement.MoveDraygonAtAngle(body, BurialDriftAngle(body.Position.Whole), DraygonArenaDefinitions.BurialDriftSpeed);

        // The burial point and its tolerance lie inside the arena, so a place within the tolerance is so plainly.
        if (Math.Abs(body.Position.X.Whole - DraygonArenaDefinitions.DeathTargetX) >= DraygonArenaDefinitions.BurialPointTolerance ||
            Math.Abs(body.Position.Y.Whole - DraygonArenaDefinitions.DeathTargetY) >= DraygonArenaDefinitions.BurialPointTolerance)
        {
            return;
        }

        SpawnDraygonDeathEvirs(state);
        state.Phase = DraygonPhase.DyingWaitForEvirs;
        _parts.RequestDraygonMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
        state.BurialWait.LastFor(DraygonArenaDefinitions.BurialWaitUpdates);
        state.BodyPart.StartAnimation(DraygonSegment.Rest);

        // The tail and arms are deleted (#2298).
        DeleteDraygonPart(state.Tail);
        DeleteDraygonPart(state.Arms);
        state.EyePart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonSegment.EyeFacingRightDead
                : DraygonSegment.EyeFacingLeftDead);
        state.EyeTracking.Action = DraygonEyeAction.Inert;
    }

    /// <summary>
    /// Deletes one of his parts as he dies (#2298): a deleted part neither draws, collides nor runs, so its other
    /// properties are left as they are.
    /// </summary>
    private static void DeleteDraygonPart(RoomEnemyPlace part) => part.Presence = part.Presence.With(EnemyPresence.Deleted);

    /// <summary>Moves the burial Evirs through his wait at his burial point, and starts his sinking on its last update.</summary>
    internal void WaitForDraygonBurialEvirs(
        DraygonEnemyState state,
        int frameCounter)
    {
        SpawnDyingDraygonSmoke(frameCounter);
        if (state.BurialWait.StepRunsOut())
        {
            state.Phase = DraygonPhase.DyingFinish;
            return;
        }

        MoveDraygonDeathEvirs(state);
    }

    /// <summary>Sinks the body past the room floor, then persists boss defeat and requests its drop.</summary>
    internal void SinkDraygonBelowTheRoom(
        DraygonEnemyState state,
        int frameCounter,
        SamusState samus)
    {
        SpawnDyingDraygonSmoke(frameCounter);
        MoveDraygonDeathEvirs(state);
        RoomEnemyPlace body = state.Body;
        // He sinks a pixel an update from the burial point, inside the arena, to the burial depth, so his height stays in the room.
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + 1);
        if (body.Position.Y.Whole < DraygonArenaDefinitions.BurialFinishedY)
            return;

        state.BodyPart.StartAnimation(DraygonSegment.Delete);

        // The death drops use Draygon's body definition for his independent death drops over the
        // lower half of the arena. The drops spawn before the body is deleted.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.DraygonBody,
            count: DraygonArenaDefinitions.DeathDropCount,
            xBase: DraygonArenaDefinitions.DeathDropOrigin.X,
            xScatter: EnemyDropScatterDefinitions.WideArenaHorizontalScatter,
            yBase: DraygonArenaDefinitions.DeathDropOrigin.Y,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter,
            samus: samus);
        // The body and the eye are deleted (#2298).
        DeleteDraygonPart(body);
        DeleteDraygonPart(state.Eye);
        // The sink runs once: this update deletes his body, whose step runs it.
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);
        _effectActors.EndAll();
    }

    /// <summary>Consumes room RNG and spawns smoke at the dying body at its smoke cadence.</summary>
    private void SpawnDyingDraygonSmoke(int frameCounter)
    {
        if (!DraygonCadenceDefinitions.DyingSmoke.IsDue(frameCounter))
            return;

        int random = _room.Random.Generate();
        Position16<RoomPixels> smoke = DraygonArenaDefinitions.DeathSmokeOrigin + new Offset16<RoomPixels>(DraygonRandomDefinitions.DeathSmokeX.Of(random), DraygonRandomDefinitions.DeathSmokeY.Of(random));
        _effectActors.SpawnRoomEffectActor(smoke, RoomEffectActorKind.BigDustCloud, graphics: SpriteGraphics.SharedStandard);
    }

    /// <summary>Ends every room effect actor and spawns six burial Evirs, each carrying the definition that moves it.</summary>
    private void SpawnDraygonDeathEvirs(DraygonEnemyState state)
    {
        _effectActors.EndAll();

        // Three left-facing Evirs on the right of the arc, then three right-facing ones on its left.
        foreach (DraygonBurialEvirDefinition evir in DraygonBurialEvirDefinitions.InSpawnOrder)
            SpawnDraygonDeathEvir(state, evir);
    }

    /// <summary>
    /// Allocates one burial Evir where its definition places it, drawn as its definition's kind in the Evirs' own colours
    /// (#1544).
    /// </summary>
    private void SpawnDraygonDeathEvir(DraygonEnemyState state, DraygonBurialEvirDefinition definition)
    {
        Ensure.NotNull(state);
        RoomEffectActor sprite = _effectActors.SpawnRoomEffectActor(
            Position16<RoomPixels>.Zero + definition.Start,
            definition.Kind,
            graphics: SpriteGraphics.SharedIn(DraygonColours.Evirs));
        state.BurialEvirs.Add(new DraygonBurialEvir(sprite, definition));
    }

    /// <summary>
    /// How far a burial Evir drifts each update: just under a pixel back along its heading toward the arc's centre, the
    /// reverse of a pixel an update along the heading (<see cref="EnemyMath.VelocityAlong"/>). An Evir left of the centre
    /// drifts right, and every one drifts down.
    /// </summary>
    private static FixedVector<RoomPixels> BurialDrift(DraygonBurialEvirDefinition evir) =>
        -EnemyMath.VelocityAlong(evir.Heading, 1);

    /// <summary>
    /// Drifts the six burial Evirs, in the order they spawned, by their definitions' headings: each Evir the burial spawned,
    /// as the burial holds it.
    /// </summary>
    private static void MoveDraygonDeathEvirs(DraygonEnemyState state)
    {
        foreach (DraygonBurialEvir evir in Ensure.NotNull(state).BurialEvirs)
            evir.Sprite.Position = evir.Sprite.Position.Advance(BurialDrift(evir.Definition));
    }
}
