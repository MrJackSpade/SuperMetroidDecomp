using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's stations' service (#1554): their setup, the touch that triggers one, the save a station asks the frontend for,
/// the room-entry save lockout, and the activation events an update publishes. Each station is a typed room object
/// (<see cref="StationObject"/>) that holds and advances its own state.
/// </summary>
[RoomLifetime]
internal sealed class StationObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>Applies the station and access-arm draws to the room's blocks.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>The room's places, searched for the station a touch of an access block reaches.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>Queues extension, retraction, and save-animation sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Connects station activation to room state, room-object drawing, the room-object places and sound requests.</summary>
    internal StationObjects(SharedGameState system, IRoomSamusAccess samus, IRoomObjectDrawing drawing, RoomObjectPlaceList places, RoomObjectSoundQueue sounds)
    {
        System = system;
        _samus = samus;
        _drawing = drawing;
        _places = places;
        _sounds = sounds;
    }
    /// <summary>Messages and frontend actions produced in the current room-object update.</summary>
    private readonly List<StationActivationEvent> _stationActivationEvents = [];
    /// <summary>The map stations the room's population placed, which stay in the room for as long as it is loaded.</summary>
    private readonly List<StationObject> _mapStations = [];
    /// <summary>Blocks repeated save confirmation during the current room entry.</summary>
    internal bool SaveStationLockedOut { get; private set; }

    /// <summary>Frame start: drops the previous pass's activation events.</summary>
    internal void BeginFrame() => _stationActivationEvents.Clear();

    /// <summary>
    /// The map station's side of gameplay resume. Ending the map access sequence does
    /// not restore Samus's normal control; leaving the automatically opened map does,
    /// through the station lock's release on unpause (<see cref="SuperMetroid.Core.Samus.SamusControl.ReleaseRefillStationOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (StationObject station in _mapStations)
            station.ResumeAfterMapUnpause();
    }

    /// <summary>Station messages/actions emitted by the most recent room-object step.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivationEvents =>
        _stationActivationEvents;

    /// <summary>
    /// Sets the room-entry lockout used when loading directly onto a save
    /// station. Ordinary destination-room construction clears it by creating a fresh room object
    /// owner; loading a saved game explicitly sets it after the station population is installed.
    /// </summary>
    public void LockSaveStationForCurrentRoomEntry() => SaveStationLockedOut = true;

    /// <summary>
    /// Resumes the resting save-station room object after the save message returns its YES/NO result.
    /// The accepted route centres Samus, queues the saving sound and draws the first
    /// animation frame within the same room-object pass. The declined route still installs the
    /// once-per-room-entry lockout. Returns whether the station is saving.
    /// </summary>
    public bool ResolveSaveStationConfirmation(SaveStationActivation activation, SaveChoice choice, RoomLevelData level)
    {
        Ensure.NotNull(level);
        StationObject station = activation.Station;
        if (station.SavePhase != SaveStationPhase.AwaitingConfirmation)
        {
            throw new InvalidOperationException(
                $"Save station {activation.SavesAt} returned " +
                $"confirmation while in {station.SavePhase}.");
        }

        if (choice == SaveChoice.No)
        {
            station.EndSave();
            SaveStationLockedOut = true;
            return false;
        }

        IRoomSamus samus = _samus.Current;
        samus.Position = samus.Position.WithWholeX(RoomGridConversions.NearestBlockBoundary(samus.Position.X.Whole));
        samus.FaceForward();
        samus.Control.LockOutPlayer();

        // The accepted route queues the saving sound right after centring Samus, in the pass the message returned into.
        _sounds.Queue(SamusSound.Saving, SoundRequestPriority.Normal);
        station.StartSaveAnimation(_drawing, level, activation.Block);
        return true;
    }

    /// <summary>Completes the save after the saved-game message has closed.</summary>
    public void CompleteSaveStation(SaveStationActivation activation)
    {
        StationObject station = activation.Station;
        if (station.SavePhase != SaveStationPhase.AwaitingCompletionMessageClose)
        {
            throw new InvalidOperationException(
                $"Save station {activation.SavesAt} completed " +
                $"save-completed message while in {station.SavePhase}.");
        }

        IRoomSamus samus = _samus.Current;
        samus.Control.Unlock();
        station.EndSave();
        SaveStationLockedOut = true;
    }

    /// <summary>
    /// Sets up the elevator platform the room's population placed: the deactivating setup leaves a solid block solid and
    /// any other block air, keeping its appearance; the platform then plays its loop.
    /// </summary>
    internal static void SetupElevatorPlatform(RoomLevelData level, RoomObjectPlace place)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        place.Become(new ElevatorPlatformObject());
        level.SetTerrain(place.Block, level.GetCollisionBlock(place.Block).Terrain.Deactivated());
    }

    /// <summary>
    /// Sets up the save station the room's population placed, saving at the load station its placement names
    /// (<paramref name="savedAt"/>): its trigger floor and its one save animation.
    /// </summary>
    internal static void SetupSaveStation(RoomLevelData level, AreaId area, RoomObjectPlace place, LoadStation savedAt)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        WriteStationBlock(
            level,
            place.Block,
            level.GetCollisionBlock(place.Block).Terrain,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(StationAccessBehavior.SaveFloor)));
        place.Become(new StationObject(
            StationKind.Save,
            area,
            StationAnimation.SaveIdle,
            completedAnimation: StationAnimation.SaveIdle,
            savedAt));
    }

    /// <summary>
    /// Sets up the map, energy or missile station the room's population placed, used by <paramref name="rule"/>: its solid
    /// block, the access blocks either side, and its animations. Only the map station has a separate acquired animation,
    /// shown from the start once its area map is held. The stations hold each map station they place, which leaving the
    /// map screen returns to idle.
    /// </summary>
    internal void SetupAccessStation(RoomLevelData level, AreaId area, RoomObjectPlace place, StationAccessRule rule)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        Ensure.NotNull(rule);
        WriteStationBlock(level, place.Block, level.GetCollisionBlock(place.Block).Terrain, RoomCollisionType.SolidBlock, behavior: null);
        WriteAccessBlock(level, level.ObjectBlockStep(place.Block, new Offset16<RoomBlocks>(rule.RightOffset, 0)), rule.RightBehavior);
        WriteAccessBlock(level, level.ObjectBlockStep(place.Block, new Offset16<RoomBlocks>(rule.LeftOffset, 0)), rule.LeftBehavior);

        StationAnimation normalAnimation = rule.Animation;
        StationAnimation completedAnimation = rule.AcquiredAnimation ?? normalAnimation;
        StationAnimation initialAnimation = rule.AcquiredAnimation is not null && System.HasAreaMap(area)
            ? completedAnimation
            : normalAnimation;
        var station = new StationObject(
            rule.Kind,
            area,
            initialAnimation,
            completedAnimation,
            savedAt: null);
        place.Become(station);
        if (rule.Kind == StationKind.Map)
            _mapStations.Add(station);
    }

    /// <summary>Marks an in-room access block with station-trigger collision and its directional behaviour.</summary>
    private static void WriteAccessBlock(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        StationAccessBehavior behavior)
    {
        if (!level.Size.Contains(block))
        {
            throw new InvalidDataException(
                $"Station access setup targets out-of-room block {block}.");
        }
        WriteStationBlock(
            level,
            block,
            level.GetCollisionBlock(block).Terrain,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(behavior)));
    }

    /// <summary>Changes a station block's collision kind while preserving its appearance, and sets its optional behaviour.</summary>
    private static void WriteStationBlock(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        RoomTerrainBlock original,
        RoomCollisionType collisionType,
        RoomBlockBehavior? behavior)
    {
        level.SetTerrain(block, original.WithCollision(collisionType));
        if (behavior is { } written)
            level.SetBehavior(block, written);
    }

    /// <summary>
    /// The station collision notice room collision sends with the block's behaviour: checks the entry pose and direction,
    /// then arms a matching resident station's room-object sequence.
    /// </summary>
    /// <param name="level">The room's level, through whose blocks the step from the access block to its station runs.</param>
    /// <param name="accessBlock">The access block Samus collided with.</param>
    /// <param name="behavior">The access block's behaviour.</param>
    /// <param name="collisionPose">Samus's pose as room collision reads it.</param>
    /// <param name="axis">The axis Samus moved along.</param>
    /// <param name="movingPositive">Whether Samus moved right or down.</param>
    public bool TryNotifyStationCollision(
        RoomLevelData level,
        Position16<RoomBlocks> accessBlock,
        RoomBlockBehavior behavior,
        SamusCollisionPose collisionPose,
        Axis axis,
        bool movingPositive)
    {
        Ensure.NotNull(level);
        if (!behavior.TryGetStationAccess(out StationAccessBehavior access))
            return false;

        // The save station's floor access is the station's own block. A map, energy or missile station's access blocks sit
        // beside it on its row, where its setup stepped through the room's blocks in reading order to place them; the step
        // back to the station reverses that.
        int columnsToStation = access == StationAccessBehavior.SaveFloor ? 0 : ColumnsToStationFrom(access);
        Position16<RoomBlocks> stationBlock = level.ObjectBlockStep(accessBlock, new Offset16<RoomBlocks>(columnsToStation, 0));

        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != stationBlock || place.Behaviour is not StationObject station)
                continue;

            bool setupAccepted = access switch
            {
                // Right-side access is entered while moving left into the wall in the ran-into-wall-left pose;
                // left-side access is the mirror in the ran-into-wall-right pose. These are the stations' setup tests
                // before cannon-height alignment.
                StationAccessBehavior.MapRight or
                    StationAccessBehavior.EnergyRight or
                    StationAccessBehavior.MissileRight =>
                    axis == Axis.Horizontal && !movingPositive &&
                    collisionPose == SamusCollisionPose.RanIntoWallLeft,
                StationAccessBehavior.MapLeft or
                    StationAccessBehavior.EnergyLeft or
                    StationAccessBehavior.MissileLeft =>
                    axis == Axis.Horizontal && movingPositive &&
                    collisionPose == SamusCollisionPose.RanIntoWallRight,
                // A foot can touch the floor trigger before the body's biased center
                // reaches its column. Accepts only the latter, narrower range.
                StationAccessBehavior.SaveFloor => axis == Axis.Vertical && movingPositive &&
                    collisionPose == SamusCollisionPose.StandingNormal &&
                    IsSaveStationTriggerCentered(place.Block),
                _ => throw new InvalidOperationException($"Undefined station access {access}."),
            };
            if (setupAccepted && station.OperationPhase == StationOperationPhase.Idle &&
                (station.Kind != StationKind.Save ||
                    (station.SavePhase == SaveStationPhase.Idle && !SaveStationLockedOut)))
            {
                // Recharge setup rejects an already-full resource before the station lock
                // takes control. Rejecting it later leaves a locked Samus with no access
                // animation running to release her. Collision itself remains solid.
                if (station.Kind is StationKind.Energy or StationKind.Missile)
                {
                    IRoomSamus samus = _samus.Current;
                    if (station.Kind == StationKind.Energy
                        ? samus.Health == samus.MaxHealth
                        : samus.Missiles == samus.MaxMissiles)
                    {
                        return true;
                    }
                }
                station.Trigger(accessBlock, access);

                // The arm cannon's line-up is checked as the station is triggered, before the station's animation
                // advances. In G-Mode the station's animation cannot start or finish, but Samus is already held still,
                // so the station leaves her stuck. A save station only starts its animation here and checks no line-up.
                if (station.Kind != StationKind.Save)
                {
                    IRoomSamus samus = _samus.Current;
                    samus.Control.LockAtStation();
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>The columns from a map, energy or missile station's access block back to the station, by the rule that placed it.</summary>
    /// <exception cref="InvalidOperationException">No station's rule places <paramref name="access"/>.</exception>
    private static int ColumnsToStationFrom(StationAccessBehavior access)
    {
        foreach (StationAccessRule rule in StationAccessRuleDefinitions.All)
        {
            if (rule.ColumnsToStationFrom(access) is { } columns)
                return columns;
        }
        throw new InvalidOperationException($"No station's access rule places access {access}.");
    }

    /// <summary>
    /// Whether Samus stands centred on the save station's floor, tested before any save side effect queues: the point
    /// <see cref="SaveStationTriggerGeometry.HorizontalProbeOffset"/> pixels left of her lies in the block's column. A
    /// Samus nearer the room's left edge than that has no such point in the room.
    /// </summary>
    private bool IsSaveStationTriggerCentered(Position16<RoomBlocks> block)
    {
        IRoomSamus samus = _samus.Current;
        int samusX = samus.Position.X.Whole;
        return samusX >= SaveStationTriggerGeometry.HorizontalProbeOffset &&
            RoomGridConversions.PixelToBlock(samusX - SaveStationTriggerGeometry.HorizontalProbeOffset) == block.X;
    }

    /// <summary>Samus, whom the stations read and change.</summary>
    internal IRoomSamus Samus => _samus.Current;

    /// <summary>The game's progression, which the stations read and change.</summary>
    internal SharedGameState System { get; }

    /// <summary>Publishes a station's message or frontend action for this update.</summary>
    internal void Publish(StationActivationEvent activation) => _stationActivationEvents.Add(activation);
}
