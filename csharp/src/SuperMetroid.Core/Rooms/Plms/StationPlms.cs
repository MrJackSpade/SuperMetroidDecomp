using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Save, refill, map and recharge station PLMs: activation, lockout, collision notification and their draws.</summary>
internal sealed class StationPlms : IPlmFamilySource
{
    /// <summary>Registers the Station family's step.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.Station, (slot, context) =>
            TryStepStation(context.Level, context.Streamer, slot, context.View) ? PlmStepOutcome.Consumed : PlmStepOutcome.Shared);
    }

    /// <summary>Provides the live Samus and persistent area state used by station activation.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Applies the station and access-arm draw lists to the room tilemap.</summary>
    private readonly PlmBlockDrawing _drawing;
    /// <summary>Locates resident station PLMs when collision or frontend confirmation arrives.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Queues extension, retraction, and save-animation sounds.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Connects station activation to room state, PLM drawing, resident slots, and sound requests.</summary>
    internal StationPlms(PlmRoomBindings bindings, PlmBlockDrawing drawing, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _drawing = drawing;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Native instruction-list offset for a station's ordinary animation.</summary>
    private const int StationNormalAnimationOffset = 4;
    /// <summary>Native instruction-list offset for the map station's acquired animation.</summary>
    private const int MapStationAcquiredAnimationOffset = 20;
    /// <summary>Frames spent extending or retracting the station access arm.</summary>
    private const ushort StationAccessMovementFrames = 6;
    /// <summary>Native hold before a fully extended arm publishes station activation.</summary>
    private const ushort StationAccessExtendedHoldFrames = 96;
    /// <summary>Messages and frontend actions produced in the current PLM handler pass.</summary>
    internal readonly List<StationActivationEvent> _stationActivationEvents = [];
    /// <summary>Blocks repeated save confirmation during the current room entry.</summary>
    internal bool _saveStationLockedOut;

    /// <summary>
    /// The map station's side of gameplay resume ($82:A2E3). Map access-list deletion does
    /// not restore the normal Samus handlers; leaving the automatically opened map does,
    /// through command $0C (<see cref="SuperMetroid.Core.Samus.SamusScriptLocks.ReleaseRefillStationLockOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (PlmSlot slot in _slotTable._slots)
        {
            // Unpause can arrive before the final access-art hold expires. Keep that
            // remaining animation; only a station waiting for the map returns to idle.
            if (slot.Active && slot.Family is StationPlmState { Kind: StationKind.Map } station &&
                station.OperationPhase == StationOperationPhase.AwaitingMapUnpause)
                station.OperationPhase = StationOperationPhase.Idle;
        }
    }

    /// <summary>Station messages/actions emitted by the most recent PLM handler pass.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivationEvents =>
        _stationActivationEvents;

    /// <summary>
    /// Sets WRAM <c>$1E75</c>'s room-entry lockout used when loading directly onto a save
    /// station. Ordinary destination-room construction clears it by creating a fresh PLM
    /// owner; an SRAM load explicitly sets it after the station population is installed.
    /// </summary>
    public void LockSaveStationForCurrentRoomEntry() => _saveStationLockedOut = true;

    /// <summary>
    /// Resumes the sleeping save-station PLM after bank $85 returns its YES/NO result.
    /// The accepted route executes `$84:8CF1` and continues through `$AFF2-$AFFA` within
    /// the same PLM pass: it centres Samus, queues the saving sound and draws the first
    /// animation frame. The declined route jumps to `$B008`, which still installs the
    /// once-per-room-entry lockout. Returns the first frame's tilemap updates.
    /// </summary>
    public SaveStationConfirmationResult ResolveSaveStationConfirmation(StationActivationEvent activation, bool accepted, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmView view)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(streamer);
        StationPlmState station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingConfirmation)
        {
            throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} returned " +
                $"confirmation while in {station.SavePhase}.");
        }

        if (!accepted)
        {
            station.SavePhase = SaveStationPhase.Idle;
            _saveStationLockedOut = true;
            return new SaveStationConfirmationResult(false, []);
        }

        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException("Confirmed save station has no live Samus owner.");
        // Rounds Samus to the nearest block boundary.
        samus.Position = samus.Position.WithWholeX(unchecked((ushort)(((samus.Position.X.Whole + 8) >> 4) << 4)));
        samus.PoseMetadata.ApplyForwardFacingPoseSetup();
        samus.InputLocked = true;

        station.SavePhase = SaveStationPhase.Animating;
        station.AnimationFrame = 0;
        station.SaveAnimationLoopsRemaining =
            NativeCountdown.Start(SaveStationAnimationDefinitions.SaveAnimationLoops);
        // `$84:AFF4` queues library-one sound $2E right after centring Samus.
        _sounds._soundRequests.Add(_sounds.CreateSoundRequest(SoundEffectLibrary1Sounds.Saving, MaximumQueued: 6));
        _drawing._tilemapUpdates.Clear();
        DrawSaveStationFrame(level, streamer, activation.BlockIndex, station, view);
        return new SaveStationConfirmationResult(true, _drawing._tilemapUpdates.ToArray());
    }

    /// <summary>Completes `$84:B030` after message $18 has closed.</summary>
    public void CompleteSaveStation(StationActivationEvent activation)
    {
        StationPlmState station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingCompletionMessageClose)
        {
            throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} completed " +
                $"message $18 while in {station.SavePhase}.");
        }

        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException("Completed save station has no live Samus owner.");
        samus.InputLocked = false;
        station.SavePhase = SaveStationPhase.Idle;
        _saveStationLockedOut = true;
    }

    /// <summary>Requires exactly one resident save PLM matching a frontend confirmation event.</summary>
    private StationPlmState FindSaveStation(StationActivationEvent activation)
    {
        StationPlmState[] matches = _slotTable._slots
            .Where(slot => slot.Active && slot.RoomArgument == activation.StationIndex)
            .Select(slot => slot.Family as StationPlmState)
            .OfType<StationPlmState>()
            .Where(station => station.Kind == StationKind.Save && station.AreaIndex == activation.AreaIndex)
            .ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} is no longer resident."),
            _ => throw new InvalidDataException(
                $"Multiple resident save stations share {activation.AreaIndex}:{activation.StationIndex}."),
        };
    }

    /// <summary>Installs station collision blocks and animation state, or deactivates an elevator platform's collision bits.</summary>
    internal static bool TrySetupStationOrElevator(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        AreaId area,
        PlmSlot slot)
    {
        if (slot.HeaderPointer is not (PlmHeaderId.ElevatorPlatform or PlmHeaderId.MapStation or
            PlmHeaderId.EnergyStation or PlmHeaderId.MissileStation or PlmHeaderId.SaveStation))
            return false;
        switch (slot.HeaderPointer)
        {
            case PlmHeaderId.ElevatorPlatform:
            {
                // Setup_DeactivatePLM clears collision bits 12..14 but deliberately retains
                // bit 15 and the visual payload; the platform then runs its idle loop.
                slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.ElevatorPlatform));
                ushort word = level.GetCollisionBlockByIndex(slot.BlockIndex).PackedWord.Raw;
                ushort deactivated = new RoomLevelWord(word).WithPlmDeactivation().Raw;
                level.SetForegroundEntry(slot.BlockIndex, deactivated);
                streamer.SetLevelEntry(slot.BlockIndex, deactivated);
                return true;
            }

            case PlmHeaderId.MapStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Map);
                return true;
            case PlmHeaderId.EnergyStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Energy);
                return true;
            case PlmHeaderId.MissileStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Missile);
                return true;
            case PlmHeaderId.SaveStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Save);
                return true;
            default:
                throw new InvalidOperationException($"Undefined station header {slot.HeaderPointer}.");
        }
    }

    /// <summary>Installs station and access BTS blocks and selects the current animation for saved map state.</summary>
    private static void SetupStation(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        AreaId area,
        PlmSlot slot,
        StationKind kind)
    {
        ushort original = level.GetCollisionBlockByIndex(slot.BlockIndex).PackedWord.Raw;
        if (kind == StationKind.Save)
        {
            WriteStationBlock(
                level,
                streamer,
                slot.BlockIndex,
                original,
                RoomCollisionType.SpecialBlock,
                new RoomBlockBehavior((byte)StationAccessBehavior.SaveFloor));
            StationAnimationList saveList = ClosedNativeWords.Decode<StationAnimationList>(
                RoomPlmHeaderDefinitions.Get(slot.HeaderPointer).InitialInstruction, "save-station animation list");
            slot.Family = new StationPlmState(
                kind,
                area,
                saveList,
                completedAnimationList: saveList,
                animationFrameCount: 1);
            return;
        }

        WriteStationBlock(level, streamer, slot.BlockIndex, original, RoomCollisionType.SolidBlock, behavior: null);
        (int rightOffset, int leftOffset, StationAccessBehavior rightBts,
            StationAccessBehavior leftBts) = kind switch
        {
            StationKind.Map => (1, -2, StationAccessBehavior.MapRight,
                StationAccessBehavior.MapLeft),
            StationKind.Energy => (1, -1, StationAccessBehavior.EnergyRight,
                StationAccessBehavior.EnergyLeft),
            StationKind.Missile => (1, -1, StationAccessBehavior.MissileRight,
                StationAccessBehavior.MissileLeft),
            _ => throw new InvalidOperationException("Save stations use their trigger setup."),
        };
        WriteAccessBlock(level, streamer, slot.BlockIndex + rightOffset, rightBts);
        WriteAccessBlock(level, streamer, slot.BlockIndex + leftOffset, leftBts);

        StationAnimationList normalAnimationList = ClosedNativeWords.Decode<StationAnimationList>(
            unchecked((ushort)(RoomPlmHeaderDefinitions.Get(slot.HeaderPointer).InitialInstruction + StationNormalAnimationOffset)),
            "station animation list");
        StationAnimationList completedAnimationList = kind switch
        {
            StationKind.Map => ClosedNativeWords.Decode<StationAnimationList>(
                unchecked((ushort)(RoomPlmHeaderDefinitions.Get(slot.HeaderPointer).InitialInstruction + MapStationAcquiredAnimationOffset)),
                "map-station acquired animation list"),
            StationKind.Energy or StationKind.Missile => normalAnimationList,
            _ => throw new InvalidOperationException(),
        };
        StationAnimationList initialAnimationList = kind == StationKind.Map && system.HasAreaMap(area)
            ? completedAnimationList
            : normalAnimationList;
        slot.Family = new StationPlmState(
            kind,
            area,
            initialAnimationList,
            completedAnimationList,
            animationFrameCount: 3);
    }

    /// <summary>Marks an in-room access block with station-trigger collision and its directional BTS.</summary>
    private static void WriteAccessBlock(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        StationAccessBehavior behavior)
    {
        if ((uint)blockIndex >= (uint)level.ForegroundEntries.Length)
        {
            throw new InvalidDataException(
                $"Station access setup targets out-of-room block index {blockIndex}.");
        }
        ushort original = level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw;
        WriteStationBlock(
            level,
            streamer,
            blockIndex,
            original,
            RoomCollisionType.SpecialBlock,
            new RoomBlockBehavior((byte)behavior));
    }

    /// <summary>Changes a station block's collision nibble while preserving its visual word and optional BTS.</summary>
    private static void WriteStationBlock(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort original,
        RoomCollisionType collisionType,
        RoomBlockBehavior? behavior)
    {
        ushort word = new RoomLevelWord(original).WithCollisionType(collisionType).Raw;
        level.SetForegroundEntry(blockIndex, word);
        streamer.SetLevelEntry(blockIndex, word);
        if (behavior is { } bts)
            level.SetBehavior(blockIndex, bts);
    }

    /// <summary>Typed BTS overload used by room collision dispatch.</summary>
    public bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        SamusPoseId collisionPose,
        bool horizontal,
        bool movingPositive,
        int roomWidthInBlocks)
        => TryNotifyStationCollision(
            accessBlockIndex,
            behavior,
            collisionPose,
            horizontal,
            movingPositive,
            roomWidthInBlocks,
            bypassSetupGate: false);

    /// <summary>Checks the entry pose and direction, then arms a matching resident station's PLM sequence.</summary>
    internal bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        SamusPoseId collisionPose,
        bool horizontal,
        bool movingPositive,
        int roomWidthInBlocks,
        bool bypassSetupGate)
    {
        if (!behavior.TryGetStationAccess(out StationAccessBehavior access))
            return false;

        int parentBlockIndex = access switch
        {
            StationAccessBehavior.MapRight => accessBlockIndex - 1,
            StationAccessBehavior.MapLeft => accessBlockIndex + 2,
            StationAccessBehavior.EnergyRight or StationAccessBehavior.MissileRight =>
                accessBlockIndex - 1,
            StationAccessBehavior.EnergyLeft or StationAccessBehavior.MissileLeft =>
                accessBlockIndex + 1,
            StationAccessBehavior.SaveFloor => accessBlockIndex,
            _ => throw new InvalidOperationException($"Undefined station access {access}."),
        };

        foreach (PlmSlot slot in _slotTable._slots)
        {
            if (!slot.Active || slot.BlockIndex != parentBlockIndex || slot.Family is not StationPlmState station)
                continue;

            bool setupAccepted = bypassSetupGate || access switch
            {
                // Right-side access is entered while moving left in pose $8A; left-side
                // access is the mirror in pose $89. These are the exact B1C8/B1F0 and
                // B26D-B300 setup predicates before cannon-height alignment.
                StationAccessBehavior.MapRight or
                    StationAccessBehavior.EnergyRight or
                    StationAccessBehavior.MissileRight =>
                    horizontal && !movingPositive &&
                    collisionPose == SamusPoseId.RanIntoWallLeftPose,
                StationAccessBehavior.MapLeft or
                    StationAccessBehavior.EnergyLeft or
                    StationAccessBehavior.MissileLeft =>
                    horizontal && movingPositive &&
                    collisionPose == SamusPoseId.RanIntoWallRightPose,
                // A foot can touch the floor trigger before the body's biased center
                // reaches its column. Native accepts only the latter, narrower range.
                StationAccessBehavior.SaveFloor => !horizontal && movingPositive &&
                    (collisionPose is SamusPoseId.FacingRightNormalPose or
                        SamusPoseId.FacingLeftNormalPose) &&
                    IsSaveStationTriggerCentered(slot.BlockIndex, roomWidthInBlocks),
                _ => throw new InvalidOperationException($"Undefined station access {access}."),
            };
            if (setupAccepted && station.OperationPhase == StationOperationPhase.Idle &&
                (station.Kind != StationKind.Save ||
                    (station.SavePhase == SaveStationPhase.Idle && !_saveStationLockedOut)))
            {
                // Recharge setup rejects an already-full resource before command six
                // takes control. Rejecting it later leaves a locked Samus with no access
                // animation running to release her. Collision itself remains solid.
                if (station.Kind is StationKind.Energy or StationKind.Missile)
                {
                    SamusState samus = _bindings.Samus?.Current
                        ?? throw new InvalidOperationException("Recharge setup has no live Samus owner.");
                    if (station.Kind == StationKind.Energy
                        ? samus.Health == samus.MaxHealth
                        : samus.Missiles == samus.MaxMissiles)
                    {
                        return true;
                    }
                }
                station.Triggered = true;
                station.AccessBlockIndex = accessBlockIndex;
                station.AccessBehavior = access;

                // ActivateStationIfSamusArmCannonLinedUp runs command six in setup,
                // before the common PLM handler advances the resident access list. This
                // distinction is observable in G-Mode: the disabled handler cannot start
                // or finish the animation, but Samus is already movement-locked and the
                // station therefore softlocks. Save trigger $B590 only advances its
                // resident list and does not execute command six at this setup seam.
                if (station.Kind != StationKind.Save)
                {
                    SamusState samus = _bindings.Samus?.Current
                        ?? throw new InvalidOperationException(
                            $"{station.Kind} station setup has no live Samus owner.");
                    samus.ScriptLocks.LockIntoRefillStation();
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>Applies the cartridge's wrapping coordinate test before queuing any save side effect.</summary>
    private bool IsSaveStationTriggerCentered(int blockIndex, int roomWidthInBlocks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException("Save-station collision requires the room's Samus owner.");
        ushort probeX = unchecked((ushort)(samus.Position.X.Whole - SaveStationTriggerGeometry.HorizontalProbeOffset));
        return (probeX >> SaveStationTriggerGeometry.BlockCoordinateShift) == blockIndex % roomWidthInBlocks;
    }

    /// <summary>Advances station draw timers, access-arm phases, and activation messages during the PLM pass.</summary>
    internal bool TryStepStation(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view)
    {
        StationPlmState? station = slot.Family as StationPlmState;
        if (station is null)
            return false;

        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException(
                $"{station.Kind} station has no live Samus owner.");

        // The access list's first draw instruction installs its six-frame timer on the
        // triggering pass itself; the common PLM handler only decrements on later passes.
        bool operationStartedThisPass = false;
        if (station.Triggered)
        {
            station.Triggered = false;
            bool canActivate = station.Kind switch
            {
                StationKind.Map => !(_bindings.System ?? throw new InvalidOperationException(
                    "Map station has no bank-$80 state owner.")).HasAreaMap(station.AreaIndex),
                StationKind.Energy => samus.Health != samus.MaxHealth,
                StationKind.Missile => samus.Missiles != samus.MaxMissiles,
                StationKind.Save => true,
                _ => throw new InvalidDataException($"Unknown station kind {station.Kind}."),
            };
            if (!canActivate && station.Kind is StationKind.Energy or StationKind.Missile)
            {
                // The access-list full-resource branch runs command one before deleting
                // itself. A refill between setup and this pass must release control too.
                samus.InputLocked = false;
                station.AccessBlockIndex = -1;
                station.AccessBehavior = null;
            }
            if (canActivate && station.Kind == StationKind.Save)
            {
                station.SavePhase = SaveStationPhase.AwaitingConfirmation;
                PublishStationActivation(station, slot, samus);
            }
            else if (canActivate)
            {
                // Access lists draw their first extension frame for six ticks, hold the
                // fully inserted arm for $60, then execute the activation opcode. Command
                // six locked Samus in setup; this pass does not lock her again, so one an
                // intervening unpause freed ($90:F2A2) stays free.
                station.OperationPhase = StationOperationPhase.Extending;
                station.OperationTimer = NativeCountdown.Start(StationAccessMovementFrames);
                operationStartedThisPass = true;
                _sounds._soundRequests.Add(_sounds.CreateSoundRequest(RoomPlmSounds.StationExtension, MaximumQueued: 6));
                DrawStationAccess(level, streamer, station, extended: false, view: view);
            }
        }

        if (station.Kind == StationKind.Save && StepSaveStationAnimation(level, streamer, slot, station, view))
        {
            return true;
        }

        if (!operationStartedThisPass &&
            station.OperationPhase is not (StationOperationPhase.Idle or StationOperationPhase.AwaitingMapUnpause))
        {
            station.OperationTimer = station.OperationTimer.Decrement().Remaining;
            if (station.OperationTimer.IsZero)
            {
                switch (station.OperationPhase)
                {
                    case StationOperationPhase.Extending:
                        DrawStationAccess(level, streamer, station, extended: true, view: view);
                        station.OperationPhase = StationOperationPhase.Extended;
                        station.OperationTimer = NativeCountdown.Start(StationAccessExtendedHoldFrames);
                        break;
                    case StationOperationPhase.Extended:
                        PublishStationActivation(station, slot, samus);
                        // $84:8CC6/$8CE7: the recharge activations run Samus command one
                        // right after their message box, before the access retracts. The
                        // map activation leaves input locked for pause teardown's command.
                        if (station.Kind != StationKind.Map)
                            samus.InputLocked = false;
                        station.OperationPhase = StationOperationPhase.PostActivationHold;
                        station.OperationTimer = NativeCountdown.Start(StationAccessMovementFrames);
                        break;
                    case StationOperationPhase.PostActivationHold:
                        _sounds._soundRequests.Add(_sounds.CreateSoundRequest(RoomPlmSounds.StationRetraction, MaximumQueued: 6));
                        station.OperationPhase = StationOperationPhase.Retracting;
                        station.OperationTimer = NativeCountdown.Start(StationAccessMovementFrames);
                        break;
                    case StationOperationPhase.Retracting:
                        DrawStationAccess(level, streamer, station, extended: false, view: view);
                        station.OperationPhase = StationOperationPhase.FinalRetractionHold;
                        station.OperationTimer = NativeCountdown.Start(StationAccessMovementFrames);
                        break;
                    case StationOperationPhase.FinalRetractionHold:
                        station.OperationPhase = station.Kind == StationKind.Map && samus.InputLocked
                            ? StationOperationPhase.AwaitingMapUnpause
                            : StationOperationPhase.Idle;
                        station.AccessBlockIndex = -1;
                        station.AccessBehavior = null;
                        // Every access list only deletes itself here. Recharges released
                        // input at activation; map pause teardown's command $0C owns its release.
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Unknown station operation phase {station.OperationPhase}.");
                }
            }
        }

        // Save station $AFE8 draws once and then sleeps until its BTS-$4D trigger advances
        // it. Its confirmation/save coroutine is published above for the frontend owner.
        if (station.Kind == StationKind.Save && station.InitialDrawCompleted)
            return true;

        station.AnimationTimer = station.AnimationTimer.Decrement().Remaining;
        if (!station.AnimationTimer.IsZero)
            return true;

        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationProgramDefinitions.Resolve(
                station.AnimationList, station.AnimationFrame);
        NativeCountdown timer = NativeCountdown.FromRaw(frame.Duration);
        ushort draw = frame.DrawPointer;
        if (timer.IsNegative || timer.IsZero)
        {
            throw new InvalidDataException(
                $"{station.Kind} station animation $84:{(int)station.AnimationList:X4} " +
                $"frame {station.AnimationFrame} does not begin with a timed draw.");
        }
        _drawing.DrawPlmInstruction(level, streamer, slot.BlockIndex, draw, view);
        station.AnimationTimer = timer;
        station.InitialDrawCompleted = true;
        station.AnimationFrame = (station.AnimationFrame + 1) % station.AnimationFrameCount;
        return true;
    }

    /// <summary>Runs the accepted save's two-frame loop until the saved-game message can be published.</summary>
    private bool StepSaveStationAnimation(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, StationPlmState station, PlmView view)
    {
        if (station.SavePhase is SaveStationPhase.Idle or
            SaveStationPhase.AwaitingConfirmation)
        {
            return false;
        }
        if (station.SavePhase == SaveStationPhase.AwaitingCompletionMessageClose)
            return true;
        if (station.SavePhase != SaveStationPhase.Animating)
        {
            throw new InvalidDataException(
                $"Save station has unknown phase {station.SavePhase}.");
        }

        station.AnimationTimer = station.AnimationTimer.Decrement().Remaining;
        if (!station.AnimationTimer.IsZero)
            return true;

        // `$84:B002` runs when the second frame's four frames expire. After the last of
        // the 21 loops it falls through to `$84:B006`, the saved-game message, instead of
        // returning to the first frame. (The loop is counted at the second draw here.)
        if (station.AnimationFrame == 0 && station.SaveAnimationLoopsRemaining.IsZero)
        {
            station.SavePhase = SaveStationPhase.AwaitingCompletionMessageClose;
            _stationActivationEvents.Add(new StationActivationEvent(
                StationKind.Save,
                GameplayMessageIds.SaveCompleted,
                station.AreaIndex,
                slot.RoomArgument,
                slot.BlockIndex));
            return true;
        }
        DrawSaveStationFrame(level, streamer, slot.BlockIndex, station, view);
        return true;
    }

    /// <summary>Draws the next of the two four-frame electricity frames (`$84:AFFA`/`$AFFE`).</summary>
    private void DrawSaveStationFrame(RoomLevelData level, BackgroundTilemapStreamer streamer, int blockIndex, StationPlmState station, PlmView view)
    {
        StationAnimationList list = station.AnimationFrame == 0
            ? StationAnimationList.SaveFirstFrame
            : StationAnimationList.SaveSecondFrame;
        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationProgramDefinitions.Resolve(list, 0);
        if (frame.Duration != 4)
        {
            throw new InvalidDataException(
                $"Save-station animation list $84:{(int)list:X4} has timer {frame.Duration}, expected 4.");
        }
        _drawing.DrawPlmInstruction(level, streamer, blockIndex, frame.DrawPointer, view);
        station.AnimationTimer = NativeCountdown.FromRaw(frame.Duration);
        station.AnimationFrame ^= 1;
        if (station.AnimationFrame == 0)
            station.SaveAnimationLoopsRemaining = station.SaveAnimationLoopsRemaining.Decrement().Remaining;
    }

    /// <summary>Applies map or refill effects and publishes the station's frontend message.</summary>
    private void PublishStationActivation(
        StationPlmState station,
        PlmSlot slot,
        SamusState samus)
    {
        GameplayMessageId message = station.Kind switch
        {
            StationKind.Map => GameplayMessageIds.MapDataAccessCompleted,
            StationKind.Energy => GameplayMessageIds.EnergyRechargeCompleted,
            StationKind.Missile => GameplayMessageIds.MissileRechargeCompleted,
            StationKind.Save => GameplayMessageIds.SaveConfirmation,
            _ => throw new InvalidDataException($"Unknown station kind {station.Kind}."),
        };

        switch (station.Kind)
        {
            case StationKind.Map:
                (_bindings.System ?? throw new InvalidOperationException(
                    "Map station has no bank-$80 state owner."))
                    .SetAreaMapAcquired(station.AreaIndex);
                station.AnimationList = station.CompletedAnimationList;
                break;
            case StationKind.Energy:
                samus.Health = samus.MaxHealth;
                station.AnimationList = station.CompletedAnimationList;
                break;
            case StationKind.Missile:
                samus.Missiles = samus.MaxMissiles;
                station.AnimationList = station.CompletedAnimationList;
                break;
            case StationKind.Save:
                break;
        }

        station.AnimationFrame = 0;
        station.AnimationTimer = NativeCountdown.Start(1);
        _stationActivationEvents.Add(new StationActivationEvent(
            station.Kind,
            message,
            station.AreaIndex,
            slot.RoomArgument,
            slot.BlockIndex));
    }

    /// <summary>Draws the station access arm at its captured collision block in the short or extended pose.</summary>
    private void DrawStationAccess(RoomLevelData level, BackgroundTilemapStreamer streamer, StationPlmState station, bool extended, PlmView view)
    {
        StationAccessBehavior accessBehavior = station.AccessBehavior
            ?? throw new InvalidDataException("Station access has no BTS owner.");
        StationAccessPlmDefinition definition =
            StationAccessPlmDefinitions.Resolve(accessBehavior);
        ushort drawPointer = definition.DrawPointer(extended);
        _drawing.DrawPlmInstruction(level, streamer, station.AccessBlockIndex, drawPointer, view);
    }

    /// <summary>Resident PLM state for station appearance, access-arm timing, and save confirmation.</summary>
    internal sealed class StationPlmState(
        StationKind kind,
        AreaId area,
        StationAnimationList animationList,
        StationAnimationList completedAnimationList,
        int animationFrameCount) : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.Station;

        /// <summary>Determines whether activation opens the map, refills a resource, or starts a save.</summary>
        public StationKind Kind { get; } = kind;
        /// <summary>Area whose map-acquired or save-station state is affected.</summary>
        public AreaId AreaIndex { get; } = area;
        /// <summary>Animation currently used by the resident station PLM.</summary>
        public StationAnimationList AnimationList { get; set; } = animationList;
        /// <summary>Animation selected after the station has completed its first activation.</summary>
        public StationAnimationList CompletedAnimationList { get; } = completedAnimationList;
        /// <summary>Number of timed draws in the current normal animation loop.</summary>
        public int AnimationFrameCount { get; } = animationFrameCount;
        /// <summary>Next frame index in the resident station's draw list.</summary>
        public int AnimationFrame { get; set; }
        /// <summary>Ticks remaining before the next station draw.</summary>
        public NativeCountdown AnimationTimer { get; set; } = NativeCountdown.Start(1);
        /// <summary>Keeps the save trigger asleep after its one initial draw.</summary>
        public bool InitialDrawCompleted { get; set; }
        /// <summary>Records accepted BTS collision for the next PLM handler pass.</summary>
        public bool Triggered { get; set; }
        /// <summary>Current extension, hold, retraction, or map-unpause phase.</summary>
        public StationOperationPhase OperationPhase { get; set; }
        /// <summary>Ticks remaining in the current access-arm phase.</summary>
        public NativeCountdown OperationTimer { get; set; }
        /// <summary>Room block index where the activated access arm must be drawn.</summary>
        public int AccessBlockIndex { get; set; } = -1;
        /// <summary>Directional BTS type identifying the selected access draw list.</summary>
        public StationAccessBehavior? AccessBehavior { get; set; }
        /// <summary>Tracks frontend confirmation and save animation separately from arm motion.</summary>
        public SaveStationPhase SavePhase { get; set; }
        /// <summary>Remaining two-frame save animation loops before the completion message.</summary>
        public NativeCountdown SaveAnimationLoopsRemaining { get; set; }
    }

    /// <summary>Phases of the non-save station's access-arm PLM list.</summary>
    internal enum StationOperationPhase : byte
    {
        /// <summary>No access sequence is in progress.</summary>
        Idle,
        /// <summary>The first access draw is waiting to extend.</summary>
        Extending,
        /// <summary>The extended arm is holding before station activation.</summary>
        Extended,
        /// <summary>Station activation finished; the arm has not begun retraction.</summary>
        PostActivationHold,
        /// <summary>The arm is retracting after its sound request.</summary>
        Retracting,
        /// <summary>The short arm pose is holding before the access list ends.</summary>
        FinalRetractionHold,
        /// <summary>The access actor has finished; command <c>$0C</c> still owns Samus release.</summary>
        AwaitingMapUnpause,
    }
}
