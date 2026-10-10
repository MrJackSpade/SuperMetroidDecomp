using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Map/resource/save stations and the ordinary elevator-platform room PLM.</summary>
public sealed partial class RoomPlmSystem
{
    private const int StationNormalAnimationOffset = 4;
    private const int MapStationAcquiredAnimationOffset = 20;
    private const ushort StationAccessMovementFrames = 6;
    private const ushort StationAccessExtendedHoldFrames = 0x60;
    private readonly List<StationActivationEvent> _stationActivationEvents = [];
    private bool _saveStationLockedOut;

    /// <summary>
    /// The map station's side of gameplay resume ($82:A2E3). Map access-list deletion does
    /// not restore the normal Samus handlers; leaving the automatically opened map does,
    /// through command $0C (<see cref="SamusState.ReleaseRefillStationLockOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (PlmSlot slot in _slots)
        {
            // Unpause can arrive before the final access-art hold expires. Keep that
            // remaining animation; only a station waiting for the map returns to idle.
            if (slot.Active && slot.Station is { Kind: StationKind.Map } station &&
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
    public SaveStationConfirmationResult ResolveSaveStationConfirmation(
        ISnesAddressSpace bus,
        StationActivationEvent activation,
        bool accepted,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
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

        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException("Confirmed save station has no live Samus owner.");
        samus.XPosition = unchecked((ushort)((samus.XPosition + 8) & 0xfff0));
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.InputLocked = true;

        station.SavePhase = SaveStationPhase.Animating;
        station.AnimationFrame = 0;
        station.SaveAnimationLoopsRemaining =
            SaveStationAnimationDefinitions.SaveAnimationLoops;
        // `$84:AFF4` queues library-one sound $2E right after centring Samus.
        _soundRequests.Add(CreateSoundRequest(SoundEffectLibrary1Sounds.Saving, MaximumQueued: 6));
        _tilemapUpdates.Clear();
        DrawSaveStationFrame(bus, level, streamer, activation.BlockIndex, station,
            layer1XPosition, layer1YPosition, bg1XOffset);
        return new SaveStationConfirmationResult(true, _tilemapUpdates.ToArray());
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

        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException("Completed save station has no live Samus owner.");
        samus.InputLocked = false;
        station.SavePhase = SaveStationPhase.Idle;
        _saveStationLockedOut = true;
    }

    private StationPlmState FindSaveStation(StationActivationEvent activation)
    {
        StationPlmState[] matches = _slots
            .Where(slot => slot.Active && slot.RoomArgument == activation.StationIndex &&
                slot.Station is { Kind: StationKind.Save } station &&
                station.AreaIndex == activation.AreaIndex)
            .Select(slot => slot.Station!)
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

    private static bool TrySetupStationOrElevator(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        AreaId area,
        PlmSlot slot)
    {
        switch (slot.HeaderPointer)
        {
            case RoomPlmHeaders.ElevatorPlatform:
            {
                // Setup_DeactivatePLM clears collision bits 12..14 but deliberately retains
                // bit 15 and the visual payload. The list at $AFB6 is then handled by the
                // ordinary timer/draw interpreter without an elevator-specific animation.
                ushort word = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
                ushort deactivated = unchecked((ushort)(word & 0x8fff));
                level.SetForegroundEntry(slot.BlockIndex, deactivated);
                streamer.SetLevelEntry(slot.BlockIndex, deactivated);
                slot.IsElevatorPlatform = true;
                return true;
            }

            case RoomPlmHeaders.MapStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Map);
                return true;
            case RoomPlmHeaders.EnergyStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Energy);
                return true;
            case RoomPlmHeaders.MissileStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Missile);
                return true;
            case RoomPlmHeaders.SaveStation:
                SetupStation(level, streamer, system, area, slot, StationKind.Save);
                return true;
            default:
                return false;
        }
    }

    private static void SetupStation(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        AreaId area,
        PlmSlot slot,
        StationKind kind)
    {
        ushort original = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
        if (kind == StationKind.Save)
        {
            WriteStationBlock(
                level,
                streamer,
                slot.BlockIndex,
                original,
                11,
                new RoomBlockBehavior((byte)StationAccessBehavior.SaveFloor));
            slot.Station = new StationPlmState(
                kind,
                area,
                slot.InstructionPointer,
                completedAnimationList: slot.InstructionPointer,
                animationFrameCount: 1);
            return;
        }

        WriteStationBlock(level, streamer, slot.BlockIndex, original, 8, behavior: null);
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

        ushort normalAnimationList = unchecked((ushort)(
            slot.InstructionPointer + StationNormalAnimationOffset));
        ushort completedAnimationList = kind switch
        {
            StationKind.Map => unchecked((ushort)(
                slot.InstructionPointer + MapStationAcquiredAnimationOffset)),
            StationKind.Energy or StationKind.Missile => normalAnimationList,
            _ => throw new InvalidOperationException(),
        };
        ushort initialAnimationList = kind == StationKind.Map && system.HasAreaMap(area)
            ? completedAnimationList
            : normalAnimationList;
        slot.Station = new StationPlmState(
            kind,
            area,
            initialAnimationList,
            completedAnimationList,
            animationFrameCount: 3);
    }

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
        ushort original = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
        WriteStationBlock(
            level,
            streamer,
            blockIndex,
            original,
            11,
            new RoomBlockBehavior((byte)behavior));
    }

    private static void WriteStationBlock(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort original,
        int collisionType,
        RoomBlockBehavior? behavior)
    {
        ushort word = unchecked((ushort)((original & 0x0fff) | (collisionType << 12)));
        level.SetForegroundEntry(blockIndex, word);
        streamer.SetLevelEntry(blockIndex, word);
        if (behavior is { } bts)
            level.SetBehavior(blockIndex, bts);
    }

    /// <summary>Typed BTS overload used by room collision dispatch.</summary>
    public bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        byte collisionPose,
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

    private bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        byte collisionPose,
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

        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != parentBlockIndex || slot.Station is null)
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
                    collisionPose == SamusPoseIds.RanIntoWallLeftPose,
                StationAccessBehavior.MapLeft or
                    StationAccessBehavior.EnergyLeft or
                    StationAccessBehavior.MissileLeft =>
                    horizontal && movingPositive &&
                    collisionPose == SamusPoseIds.RanIntoWallRightPose,
                // A foot can touch the floor trigger before the body's biased center
                // reaches its column. Native accepts only the latter, narrower range.
                StationAccessBehavior.SaveFloor => !horizontal && movingPositive &&
                    (collisionPose is SamusPoseIds.FacingRightNormalPose or
                        SamusPoseIds.FacingLeftNormalPose) &&
                    IsSaveStationTriggerCentered(slot.BlockIndex, roomWidthInBlocks),
                _ => throw new InvalidOperationException($"Undefined station access {access}."),
            };
            if (setupAccepted && slot.Station.OperationPhase == StationOperationPhase.Idle &&
                (slot.Station.Kind != StationKind.Save ||
                    (slot.Station.SavePhase == SaveStationPhase.Idle && !_saveStationLockedOut)))
            {
                // Recharge setup rejects an already-full resource before command six
                // takes control. Rejecting it later leaves a locked Samus with no access
                // animation running to release her. Collision itself remains solid.
                if (slot.Station.Kind is StationKind.Energy or StationKind.Missile)
                {
                    SamusState samus = _collectibleSamus?.Invoke()
                        ?? throw new InvalidOperationException("Recharge setup has no live Samus owner.");
                    if (slot.Station.Kind == StationKind.Energy
                        ? samus.Health == samus.MaxHealth
                        : samus.Missiles == samus.MaxMissiles)
                    {
                        return true;
                    }
                }
                slot.Station.Triggered = true;
                slot.Station.AccessBlockIndex = accessBlockIndex;
                slot.Station.AccessBehavior = access;

                // ActivateStationIfSamusArmCannonLinedUp runs command six in setup,
                // before the common PLM handler advances the resident access list. This
                // distinction is observable in G-Mode: the disabled handler cannot start
                // or finish the animation, but Samus is already movement-locked and the
                // station therefore softlocks. Save trigger $B590 only advances its
                // resident list and does not execute command six at this setup seam.
                if (slot.Station.Kind != StationKind.Save)
                {
                    SamusState samus = _collectibleSamus?.Invoke()
                        ?? throw new InvalidOperationException(
                            $"{slot.Station.Kind} station setup has no live Samus owner.");
                    samus.LockIntoRefillStation();
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
        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException("Save-station collision requires the room's Samus owner.");
        ushort probeX = unchecked((ushort)(samus.XPosition - SaveStationTriggerGeometry.HorizontalProbeOffset));
        return (probeX >> SaveStationTriggerGeometry.BlockCoordinateShift) == blockIndex % roomWidthInBlocks;
    }

    private bool TryStepStation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        StationPlmState? station = slot.Station;
        if (station is null)
            return false;

        SamusState samus = _collectibleSamus?.Invoke()
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
                StationKind.Map => !(_collectibleSystem ?? throw new InvalidOperationException(
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
                station.OperationTimer = StationAccessMovementFrames;
                operationStartedThisPass = true;
                _soundRequests.Add(CreateSoundRequest(RoomPlmSounds.StationExtension, MaximumQueued: 6));
                DrawStationAccess(
                    bus,
                    level,
                    streamer,
                    station,
                    extended: false,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset);
            }
        }

        if (station.Kind == StationKind.Save && StepSaveStationAnimation(
                bus,
                level,
                streamer,
                slot,
                station,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset))
        {
            return true;
        }

        if (!operationStartedThisPass &&
            station.OperationPhase is not (StationOperationPhase.Idle or StationOperationPhase.AwaitingMapUnpause))
        {
            station.OperationTimer--;
            if (station.OperationTimer == 0)
            {
                switch (station.OperationPhase)
                {
                    case StationOperationPhase.Extending:
                        DrawStationAccess(
                            bus,
                            level,
                            streamer,
                            station,
                            extended: true,
                            layer1XPosition,
                            layer1YPosition,
                            bg1XOffset);
                        station.OperationPhase = StationOperationPhase.Extended;
                        station.OperationTimer = StationAccessExtendedHoldFrames;
                        break;
                    case StationOperationPhase.Extended:
                        PublishStationActivation(station, slot, samus);
                        // $84:8CC6/$8CE7: the recharge activations run Samus command one
                        // right after their message box, before the access retracts. The
                        // map activation leaves input locked for pause teardown's command.
                        if (station.Kind != StationKind.Map)
                            samus.InputLocked = false;
                        station.OperationPhase = StationOperationPhase.PostActivationHold;
                        station.OperationTimer = StationAccessMovementFrames;
                        break;
                    case StationOperationPhase.PostActivationHold:
                        _soundRequests.Add(CreateSoundRequest(RoomPlmSounds.StationRetraction, MaximumQueued: 6));
                        station.OperationPhase = StationOperationPhase.Retracting;
                        station.OperationTimer = StationAccessMovementFrames;
                        break;
                    case StationOperationPhase.Retracting:
                        DrawStationAccess(
                            bus,
                            level,
                            streamer,
                            station,
                            extended: false,
                            layer1XPosition,
                            layer1YPosition,
                            bg1XOffset);
                        station.OperationPhase = StationOperationPhase.FinalRetractionHold;
                        station.OperationTimer = StationAccessMovementFrames;
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

        station.AnimationTimer--;
        if (station.AnimationTimer != 0)
            return true;

        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationProgramDefinitions.Resolve(
                station.AnimationList, station.AnimationFrame);
        ushort timer = frame.Duration;
        ushort draw = frame.DrawPointer;
        if ((timer & 0x8000) != 0 || timer == 0)
        {
            throw new InvalidDataException(
                $"{station.Kind} station animation $84:{station.AnimationList:X4} " +
                $"frame {station.AnimationFrame} does not begin with a timed draw.");
        }
        DrawPlmInstruction(
            bus,
            level,
            streamer,
            slot.BlockIndex,
            draw,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);
        station.AnimationTimer = timer;
        station.InitialDrawCompleted = true;
        station.AnimationFrame = (station.AnimationFrame + 1) % station.AnimationFrameCount;
        return true;
    }

    private bool StepSaveStationAnimation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        StationPlmState station,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
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

        station.AnimationTimer--;
        if (station.AnimationTimer != 0)
            return true;

        // `$84:B002` runs when the second frame's four frames expire. After the last of
        // the 21 loops it falls through to `$84:B006`, the saved-game message, instead of
        // returning to the first frame. (The loop is counted at the second draw here.)
        if (station.AnimationFrame == 0 && station.SaveAnimationLoopsRemaining == 0)
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
        DrawSaveStationFrame(bus, level, streamer, slot.BlockIndex, station,
            layer1XPosition, layer1YPosition, bg1XOffset);
        return true;
    }

    /// <summary>Draws the next of the two four-frame electricity frames (`$84:AFFA`/`$AFFE`).</summary>
    private void DrawSaveStationFrame(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        StationPlmState station,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        ushort list = station.AnimationFrame == 0
            ? RoomPlmInstructionLists.SaveStationAnimationFirstFrame
            : RoomPlmInstructionLists.SaveStationAnimationSecondFrame;
        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationProgramDefinitions.Resolve(list, 0);
        if (frame.Duration != 4)
        {
            throw new InvalidDataException(
                $"Save-station animation list $84:{list:X4} has timer {frame.Duration}, expected 4.");
        }
        DrawPlmInstruction(
            bus,
            level,
            streamer,
            blockIndex,
            frame.DrawPointer,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);
        station.AnimationTimer = frame.Duration;
        station.AnimationFrame ^= 1;
        if (station.AnimationFrame == 0)
            station.SaveAnimationLoopsRemaining--;
    }

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
                (_collectibleSystem ?? throw new InvalidOperationException(
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
        station.AnimationTimer = 1;
        _stationActivationEvents.Add(new StationActivationEvent(
            station.Kind,
            message,
            station.AreaIndex,
            slot.RoomArgument,
            slot.BlockIndex));
    }

    private void DrawStationAccess(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        StationPlmState station,
        bool extended,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        StationAccessBehavior accessBehavior = station.AccessBehavior
            ?? throw new InvalidDataException("Station access has no BTS owner.");
        StationAccessPlmDefinition definition =
            StationAccessPlmDefinitions.Resolve(accessBehavior);
        ushort drawPointer = definition.DrawPointer(extended);
        DrawPlmInstruction(
            bus,
            level,
            streamer,
            station.AccessBlockIndex,
            drawPointer,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);
    }

    private sealed class StationPlmState(
        StationKind kind,
        AreaId area,
        ushort animationList,
        ushort completedAnimationList,
        int animationFrameCount)
    {
        public StationKind Kind { get; } = kind;
        public AreaId AreaIndex { get; } = area;
        public ushort AnimationList { get; set; } = animationList;
        public ushort CompletedAnimationList { get; } = completedAnimationList;
        public int AnimationFrameCount { get; } = animationFrameCount;
        public int AnimationFrame { get; set; }
        public ushort AnimationTimer { get; set; } = 1;
        public bool InitialDrawCompleted { get; set; }
        public bool Triggered { get; set; }
        public StationOperationPhase OperationPhase { get; set; }
        public ushort OperationTimer { get; set; }
        public int AccessBlockIndex { get; set; } = -1;
        public StationAccessBehavior? AccessBehavior { get; set; }
        public SaveStationPhase SavePhase { get; set; }
        public ushort SaveAnimationLoopsRemaining { get; set; }
    }

    private enum StationOperationPhase : byte
    {
        Idle,
        Extending,
        Extended,
        PostActivationHold,
        Retracting,
        FinalRetractionHold,
        // The access actor has finished; command $0C still owns the Samus release.
        AwaitingMapUnpause,
    }
}

/// <summary>The resident station family that owns access animation, resource changes, and frontend activation events.</summary>
public enum StationKind : byte
{
    /// <summary>Acquires the current area's map and keeps Samus input locked until map-screen unpause releases it.</summary>
    Map,
    /// <summary>Refills Samus health to its current maximum after the access arm's extension hold.</summary>
    Energy,
    /// <summary>Refills ordinary Missiles to their current maximum after the access arm's extension hold.</summary>
    Missile,
    /// <summary>Uses the floor trigger to request confirmation, then runs the save-pod animation and completion-message handshake.</summary>
    Save,
}

/// <summary>
/// A station action/message request published by a PLM handler pass for the gameplay
/// frontend to consume. Save events retain the station identity needed to return
/// confirmation or message-close results to the same resident room owner.
/// </summary>
/// <param name="Kind">The station family whose activation produced this event.</param>
/// <param name="MessageBoxIndex">The native gameplay message identity: map/refill completion, save confirmation, or save completion.</param>
/// <param name="AreaIndex">The room's area, used for map acquisition and save-station identity.</param>
/// <param name="StationIndex">The station PLM's native room argument, used with the area to identify a resident save station.</param>
/// <param name="BlockIndex">The parent station's linear foreground-block index, rather than a side-access trigger's index.</param>
public readonly record struct StationActivationEvent(
    StationKind Kind,
    GameplayMessageId MessageBoxIndex,
    AreaId AreaIndex,
    ushort StationIndex,
    int BlockIndex);

/// <summary>The save-pod coroutine's handshake between resident PLM animation and frontend messages.</summary>
public enum SaveStationPhase : byte
{
    /// <summary>The initial pod draw sleeps until an eligible floor collision; a room-entry lockout can still prevent another request.</summary>
    Idle,
    /// <summary>A floor trigger has published the YES/NO request and waits for the frontend's confirmation result.</summary>
    AwaitingConfirmation,
    /// <summary>Accepted confirmation has centered and locked Samus; alternating four-tick pod frames run for the authored loop count.</summary>
    Animating,
    /// <summary>Animation has published save-completed message $18 and waits for its close callback to unlock input and install the room-entry lockout.</summary>
    AwaitingCompletionMessageClose,
}

/// <summary>A save station's confirmation outcome and its same-pass first-frame draw.</summary>
public readonly record struct SaveStationConfirmationResult(
    bool Saving,
    IReadOnlyList<PlmTilemapUpdate> TilemapUpdates);
