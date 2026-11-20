using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's stations' service (#1554): their setup, the touch that triggers one, the save a station asks the frontend for
/// and the room-entry save lockout. Each station is a typed room object that holds and advances its own state: a save
/// station (<see cref="SaveStationObject"/>), or a map, energy or missile station used through its access arm
/// (<see cref="AccessStationObject"/>, #2484). What a station does in a room-object step joins that step's outcome (#2514).
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
    /// <summary>The map stations the room's population placed, which stay in the room for as long as it is loaded.</summary>
    private readonly List<AccessStationObject> _mapStations = [];
    /// <summary>Blocks repeated save confirmation during the current room entry.</summary>
    private bool SaveStationLockedOut { get; set; }

    /// <summary>
    /// The map station's side of gameplay resume. Ending the map access sequence does
    /// not restore Samus's normal control; leaving the automatically opened map does,
    /// through the station lock's release on unpause (<see cref="SuperMetroid.Core.Samus.SamusControl.ReleaseRefillStationOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (AccessStationObject station in _mapStations)
            station.ResumeAfterMapUnpause();
    }

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
        SaveStationObject station = activation.Station;
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
        SaveStationObject station = activation.Station;
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
        level.Deactivate(place.Block);
    }

    /// <summary>
    /// Sets up the save station the room's population placed, saving at the load station its placement names
    /// (<paramref name="savedAt"/>): its trigger floor and its pod, drawn once.
    /// </summary>
    internal static void SetupSaveStation(RoomLevelData level, RoomObjectPlace place, LoadStation savedAt)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        WriteStationBlock(
            level,
            place.Block,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(SpecialBlockReaction.SaveStationFloor));
        place.Become(new SaveStationObject(savedAt));
    }

    /// <summary>
    /// Sets up the map, energy or missile station the room's population placed, used by <paramref name="rule"/>: its solid
    /// block, its animation and, while the station can still be used, the access blocks either side. Only the map station
    /// has a separate acquired animation: once its area map is held it shows that animation from the start and writes no
    /// access blocks, so the layout's own blocks stay beside it and nothing there serves Samus (#2508). The stations hold
    /// each map station they place, which leaving the map screen returns to idle.
    /// </summary>
    internal void SetupAccessStation(RoomLevelData level, AreaId area, RoomObjectPlace place, StationAccessRule rule)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        Ensure.NotNull(rule);
        WriteStationBlock(level, place.Block, RoomCollisionType.SolidBlock, behavior: null);
        StationAnimation initialAnimation;
        if (rule.AcquiredAnimation is { } acquired && System.HasAreaMap(area))
        {
            initialAnimation = acquired;
        }
        else
        {
            WriteAccessBlock(level, level.GridBlock(place.Block + new Offset16<RoomBlocks>(rule.RightOffset, 0)),
                new StationAccess(rule.Kind, HorizontalDirection.Right));
            WriteAccessBlock(level, level.GridBlock(place.Block + new Offset16<RoomBlocks>(rule.LeftOffset, 0)),
                new StationAccess(rule.Kind, HorizontalDirection.Left));
            initialAnimation = rule.Animation;
        }
        var station = new AccessStationObject(rule, area, initialAnimation);
        place.Become(station);
        if (rule.Reward == StationReward.AreaMap)
            _mapStations.Add(station);
    }

    /// <summary>Marks an in-room access block with station-trigger collision and the reaction of <paramref name="access"/>.</summary>
    private static void WriteAccessBlock(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        StationAccess access)
    {
        if (!level.Size.Contains(block))
        {
            throw new InvalidDataException(
                $"Station access setup targets out-of-room block {block}.");
        }
        WriteStationBlock(
            level,
            block,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(access)));
    }

    /// <summary>
    /// Changes a station block's collision kind, and sets its optional behaviour, keeping its picture and what it shows, as
    /// the stations' setups rewrite only each block's type, or type and parameter (#2502).
    /// </summary>
    private static void WriteStationBlock(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        RoomCollisionType collisionType,
        RoomBlockBehavior? behavior)
    {
        level.SetCollision(block, collisionType);
        if (behavior is { } written)
            level.SetBehavior(block, written);
    }

    /// <summary>
    /// The station collision notice room collision sends with the block's behaviour: a touch of a save station's floor or of
    /// a map, energy or missile station's access block. It checks the entry pose and direction, then arms the resident
    /// station's room-object sequence.
    /// </summary>
    /// <param name="level">The room's level, through whose blocks the step from the access block to its station runs.</param>
    /// <param name="touched">The floor or access block Samus collided with.</param>
    /// <param name="behavior">The touched block's behaviour.</param>
    /// <param name="collisionPose">Samus's pose as room collision reads it.</param>
    /// <param name="axis">The axis Samus moved along.</param>
    /// <param name="movingPositive">Whether Samus moved right or down.</param>
    /// <returns>Whether a station stands where the touched block names it.</returns>
    public bool TryNotifyStationCollision(
        RoomLevelData level,
        Position16<RoomBlocks> touched,
        RoomBlockBehavior behavior,
        SamusCollisionPose collisionPose,
        Axis axis,
        bool movingPositive)
    {
        Ensure.NotNull(level);
        if (behavior.IsSaveStationFloor)
            return TryNotifySaveStationFloor(touched, collisionPose, axis, movingPositive);
        return behavior.TryGetStationAccess(out StationAccess access) &&
            TryNotifyStationAccess(level, touched, access, collisionPose, axis, movingPositive);
    }

    /// <summary>
    /// A touch of a save station's floor, which is the station's own block: a landing Samus standing centred on it asks to
    /// save, unless a save is under way or the room entry's lockout holds.
    /// </summary>
    private bool TryNotifySaveStationFloor(Position16<RoomBlocks> floor, SamusCollisionPose collisionPose, Axis axis, bool movingPositive)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != floor || place.Behaviour is not SaveStationObject station)
                continue;

            // A foot can touch the floor trigger before the body's biased center reaches its column. Accepts only the
            // latter, narrower range. A save station only starts its animation here and checks no line-up.
            if (axis == Axis.Vertical && movingPositive &&
                collisionPose == SamusCollisionPose.StandingNormal &&
                IsSaveStationTriggerCentered(place.Block) &&
                station.SavePhase == SaveStationPhase.Idle && !SaveStationLockedOut)
                station.Trigger();
            return true;
        }
        return false;
    }

    /// <summary>
    /// A touch of a map, energy or missile station's access block, which sits beside the station on its row, as the
    /// station's setup placed it; the station lies back along the row by the same columns. Samus running into the station from the access's side starts its arm, unless a refill station has
    /// nothing to give her.
    /// </summary>
    private bool TryNotifyStationAccess(RoomLevelData level, Position16<RoomBlocks> accessBlock, StationAccess access,
        SamusCollisionPose collisionPose, Axis axis, bool movingPositive)
    {
        StationAccessRule placedBy = StationAccessRuleDefinitions.Rules[access.Station];
        Position16<RoomBlocks> stationBlock = level.GridBlock(accessBlock -
            new Offset16<RoomBlocks>(placedBy.ColumnsToAccessOn(access.Side), 0));

        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != stationBlock || place.Behaviour is not AccessStationObject station)
                continue;

            // Right-side access is entered while moving left into the wall in the ran-into-wall-left pose; left-side
            // access is the mirror in the ran-into-wall-right pose. These are the stations' setup tests before
            // cannon-height alignment.
            bool setupAccepted = axis == Axis.Horizontal && (access.Side == HorizontalDirection.Right
                ? !movingPositive && collisionPose == SamusCollisionPose.RanIntoWallLeft
                : movingPositive && collisionPose == SamusCollisionPose.RanIntoWallRight);
            if (setupAccepted && station.AcceptsTouch)
            {
                // Recharge setup rejects an already-full resource before the station lock takes control. Rejecting it
                // later leaves a locked Samus with no access animation running to release her. Collision itself remains
                // solid.
                if (station.Rule.Reward is StationReward.Refills { Refill: var refill } && refill.IsFull(_samus.Current))
                    return true;
                station.Trigger(accessBlock, access);

                // The arm cannon's line-up is checked as the station is triggered, before the station's animation
                // advances. In G-Mode the station's animation cannot start or finish, but Samus is already held still,
                // so the station leaves her stuck.
                _samus.Current.Control.LockAtStation();
            }
            return true;
        }
        return false;
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
}
