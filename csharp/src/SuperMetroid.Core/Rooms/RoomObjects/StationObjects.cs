using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's stations' service (#1554): their setup, the touch that triggers one, the save handshake with the frontend,
/// the room-entry save lockout, and the activation events an update publishes. Each station is a typed room object
/// (<see cref="StationObject"/>) that holds and advances its own state.
/// </summary>
[RoomLifetime]
internal sealed class StationObjects
{

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Applies the station and access-arm draw lists to the room's blocks.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>Locates resident station room objects when collision or frontend confirmation arrives.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Queues extension, retraction, and save-animation sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Connects station activation to room state, room-object drawing, resident slots and sound requests.</summary>
    internal StationObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomObjectDrawing drawing, RoomObjectSlotTable slotTable, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _drawing = drawing;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Messages and frontend actions produced in the current room-object update.</summary>
    private readonly List<StationActivationEvent> _stationActivationEvents = [];
    /// <summary>Blocks repeated save confirmation during the current room entry.</summary>
    internal bool SaveStationLockedOut { get; private set; }

    /// <summary>Frame start: drops the previous pass's activation events.</summary>
    internal void BeginFrame() => _stationActivationEvents.Clear();

    /// <summary>
    /// The map station's side of gameplay resume. Ending the map access sequence does
    /// not restore the normal Samus handlers; leaving the automatically opened map does,
    /// through the station lock's release on unpause (<see cref="SuperMetroid.Core.Samus.SamusControl.ReleaseRefillStationOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
        {
            // Unpause can arrive before the final access-art hold expires. Keep that
            // remaining animation; only a station waiting for the map returns to idle.
            if (slot.Active && slot.Behaviour is StationObject { Kind: StationKind.Map } station &&
                station.OperationPhase == StationOperationPhase.AwaitingMapUnpause)
                station.OperationPhase = StationOperationPhase.Idle;
        }
    }

    /// <summary>Station messages/actions emitted by the most recent room-object handler pass.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivationEvents =>
        _stationActivationEvents;

    /// <summary>
    /// Sets the room-entry lockout used when loading directly onto a save
    /// station. Ordinary destination-room construction clears it by creating a fresh room object
    /// owner; loading a saved game explicitly sets it after the station population is installed.
    /// </summary>
    public void LockSaveStationForCurrentRoomEntry() => SaveStationLockedOut = true;

    /// <summary>
    /// Resumes the sleeping save-station room object after the save message returns its YES/NO result.
    /// The accepted route centres Samus, queues the saving sound and draws the first
    /// animation frame within the same room-object pass. The declined route still installs the
    /// once-per-room-entry lockout. Returns whether the station is saving.
    /// </summary>
    public bool ResolveSaveStationConfirmation(StationActivationEvent activation, bool accepted, RoomLevelData level)
    {
        Ensure.NotNull(level);
        StationObject station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingConfirmation)
        {
            throw new InvalidOperationException(
                $"Save station {activation.SaveStation} returned " +
                $"confirmation while in {station.SavePhase}.");
        }

        if (!accepted)
        {
            station.SavePhase = SaveStationPhase.Idle;
            SaveStationLockedOut = true;
            return false;
        }

        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Confirmed save station has no live Samus owner.");
        samus.Position = samus.Position.WithWrappedWholeX(RoomGridConversions.NearestBlockBoundary(samus.Position.X.Whole));
        samus.FaceForward();
        samus.Control.LockOutPlayer();

        // The accepted route queues the saving sound right after centring Samus, in the pass the message returned into.
        _sounds.Queue(SamusSound.Saving, SoundRequestPriority.Normal);
        station.StartSaveAnimation(_drawing, level, activation.Block);
        return true;
    }

    /// <summary>Completes the save after the saved-game message has closed.</summary>
    public void CompleteSaveStation(StationActivationEvent activation)
    {
        StationObject station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingCompletionMessageClose)
        {
            throw new InvalidOperationException(
                $"Save station {activation.SaveStation} completed " +
                $"save-completed message while in {station.SavePhase}.");
        }

        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Completed save station has no live Samus owner.");
        samus.Control.Unlock();
        station.SavePhase = SaveStationPhase.Idle;
        SaveStationLockedOut = true;
    }

    /// <summary>Requires exactly one resident save station matching a frontend confirmation event.</summary>
    private StationObject FindSaveStation(StationActivationEvent activation)
    {
        StationObject[] matches = _slotTable.FromLastPlace
            .Where(slot => slot.Active && slot.Argument is SaveStationArgument saved && saved.Station == activation.Station)
            .Select(slot => slot.Behaviour as StationObject)
            .OfType<StationObject>()
            .Where(station => station.Kind == StationKind.Save && station.Area == activation.Area)
            .ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException(
                $"Save station {activation.SaveStation} is no longer resident."),
            _ => throw new InvalidDataException(
                $"Multiple resident save stations save at {activation.SaveStation}."),
        };
    }

    /// <summary>
    /// Sets up the station or elevator platform the room's population placed as <paramref name="placed"/>: installs a
    /// station's collision blocks and animation state, or deactivates an elevator platform's block.
    /// </summary>
    /// <returns>False when <paramref name="placed"/> is neither a station nor an elevator platform.</returns>
    internal static bool TrySetupStationOrElevator(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        RoomObjectSlot slot,
        RoomObjectKind placed)
    {
        Ensure.NotNull(slot);
        if (placed == RoomObjectKind.ElevatorPlatform)
        {
            // The deactivating setup leaves a solid block solid and any other block air, keeping its appearance; the
            // platform then plays its loop.
            slot.Become(new ElevatorPlatformObject());
            level.SetTerrain(slot.Block, level.GetCollisionBlock(slot.Block).Terrain.Deactivated());
            return true;
        }
        if (!StationKindDefinitions.Kinds.TryGetValue(placed, out StationKind kind))
            return false;
        SetupStation(level, system, area, slot, kind);
        return true;
    }

    /// <summary>Installs station and access blocks and selects the current animation for saved map state.</summary>
    private static void SetupStation(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        RoomObjectSlot slot,
        StationKind kind)
    {
        RoomTerrainBlock original = level.GetCollisionBlock(slot.Block).Terrain;
        switch (kind)
        {
            case StationKind.Save:
                SetupSaveStation(level, area, slot, original);
                return;
            case StationKind.Map:
                SetupAccessStation(level, system, area, slot, original, StationAccessRuleDefinitions.Map);
                return;
            case StationKind.Energy:
                SetupAccessStation(level, system, area, slot, original, StationAccessRuleDefinitions.Energy);
                return;
            case StationKind.Missile:
                SetupAccessStation(level, system, area, slot, original, StationAccessRuleDefinitions.Missile);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationKind)} {kind}.");
        }
    }

    /// <summary>A save station: its trigger floor and its one save animation.</summary>
    private static void SetupSaveStation(RoomLevelData level, AreaId area, RoomObjectSlot slot, RoomTerrainBlock original)
    {
        WriteStationBlock(
            level,
            slot.Block,
            original,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(StationAccessBehavior.SaveFloor)));
        slot.Become(new StationObject(
            StationKind.Save,
            area,
            StationAnimationList.SaveIdle,
            completedAnimationList: StationAnimationList.SaveIdle,
            animationFrameCount: 1));
    }

    /// <summary>
    /// A map, energy or missile station: its solid block, the access blocks either side, and its animations. Only the map
    /// station has a separate acquired animation, shown from the start once its area map is held.
    /// </summary>
    private static void SetupAccessStation(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        RoomObjectSlot slot,
        RoomTerrainBlock original,
        StationAccessRule rule)
    {
        WriteStationBlock(level, slot.Block, original, RoomCollisionType.SolidBlock, behavior: null);
        WriteAccessBlock(level, level.ObjectBlockStep(slot.Block, new Offset16<RoomBlocks>(rule.RightOffset, 0)), rule.RightBehavior);
        WriteAccessBlock(level, level.ObjectBlockStep(slot.Block, new Offset16<RoomBlocks>(rule.LeftOffset, 0)), rule.LeftBehavior);

        StationAnimationList normalAnimationList = rule.Animation;
        StationAnimationList completedAnimationList = rule.AcquiredAnimation ?? normalAnimationList;
        StationAnimationList initialAnimationList = rule.AcquiredAnimation is not null && system.HasAreaMap(area)
            ? completedAnimationList
            : normalAnimationList;
        slot.Become(new StationObject(
            rule.Kind,
            area,
            initialAnimationList,
            completedAnimationList,
            animationFrameCount: 3));
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

    /// <summary>The station collision notice room collision dispatch sends with the block's behaviour.</summary>
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
        => TryNotifyStationCollision(
            level,
            accessBlock,
            behavior,
            collisionPose,
            axis,
            movingPositive,
            StationSetupGate.Checked);

    /// <summary>Checks the entry pose and direction, then arms a matching resident station's room-object sequence.</summary>
    internal bool TryNotifyStationCollision(
        RoomLevelData level,
        Position16<RoomBlocks> accessBlock,
        RoomBlockBehavior behavior,
        SamusCollisionPose collisionPose,
        Axis axis,
        bool movingPositive,
        StationSetupGate setupGate)
    {
        Ensure.NotNull(level);
        if (!behavior.TryGetStationAccess(out StationAccessBehavior access))
            return false;

        // The access blocks sit beside their station on its row; the step to the station runs through the room's blocks in
        // reading order, as the station's setup placed them.
        int columnsToStation = access switch
        {
            StationAccessBehavior.MapRight => -1,
            StationAccessBehavior.MapLeft => 2,
            StationAccessBehavior.EnergyRight or StationAccessBehavior.MissileRight => -1,
            StationAccessBehavior.EnergyLeft or StationAccessBehavior.MissileLeft => 1,
            StationAccessBehavior.SaveFloor => 0,
            _ => throw new InvalidOperationException($"Undefined station access {access}."),
        };
        Position16<RoomBlocks> stationBlock = level.ObjectBlockStep(accessBlock, new Offset16<RoomBlocks>(columnsToStation, 0));

        foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
        {
            if (!slot.Active || slot.Block != stationBlock || slot.Behaviour is not StationObject station)
                continue;

            bool setupAccepted = setupGate == StationSetupGate.Bypassed || access switch
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
                    IsSaveStationTriggerCentered(slot.Block),
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
                    IRoomSamus samus = _samus?.Current
                        ?? throw new InvalidOperationException("Recharge setup has no live Samus owner.");
                    if (station.Kind == StationKind.Energy
                        ? samus.Health == samus.MaxHealth
                        : samus.Missiles == samus.MaxMissiles)
                    {
                        return true;
                    }
                }
                station.Triggered = true;
                station.AccessBlock = accessBlock;
                station.AccessBehavior = access;

                // The arm-cannon line-up check runs in setup,
                // before the common room-object handler advances the resident access sequence. This
                // distinction is observable in G-Mode: the disabled handler cannot start
                // or finish the animation, but Samus is already movement-locked and the
                // station therefore softlocks. The save trigger only advances its
                // resident sequence and does not run the line-up check at this setup seam.
                if (station.Kind != StationKind.Save)
                {
                    IRoomSamus samus = _samus?.Current
                        ?? throw new InvalidOperationException(
                            $"{station.Kind} station setup has no live Samus owner.");
                    samus.Control.LockAtStation();
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>Applies the game's wrapping coordinate test before queuing any save side effect.</summary>
    private bool IsSaveStationTriggerCentered(Position16<RoomBlocks> block)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Save-station collision requires the room's Samus owner.");
        int probeX = CoordinateAxis.Wrap(samus.Position.X.Whole - SaveStationTriggerGeometry.HorizontalProbeOffset);
        return RoomGridConversions.PixelToBlock(probeX) == block.X;
    }

    /// <summary>Samus, whom <paramref name="owner"/> reads and changes.</summary>
    internal IRoomSamus RequireSamus(string owner) =>
        _samus?.Current ?? throw new InvalidOperationException($"{owner} has no live Samus owner.");

    /// <summary>The game's progression, which <paramref name="owner"/> reads and changes.</summary>
    internal SharedGameState RequireSystem(string owner) =>
        _system ?? throw new InvalidOperationException($"{owner} has no shared game state owner.");

    /// <summary>Publishes a station's message or frontend action for this update.</summary>
    internal void Publish(StationActivationEvent activation) => _stationActivationEvents.Add(activation);
}
