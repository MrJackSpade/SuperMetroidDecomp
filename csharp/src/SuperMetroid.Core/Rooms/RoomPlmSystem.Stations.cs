using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Map/resource/save stations and the ordinary elevator-platform room PLM.</summary>
public sealed partial class RoomPlmSystem
{
    private const ushort MapStationHeader = 0xb6d3;
    private const ushort EnergyStationHeader = 0xb6df;
    private const ushort MissileStationHeader = 0xb6eb;
    private const ushort ElevatorPlatformHeader = 0xb70b;
    private const ushort SaveStationHeader = 0xb76f;
    private const int StationNormalAnimationOffset = 4;
    private const int MapStationAcquiredAnimationOffset = 20;
    private const ushort StationAccessMovementFrames = 6;
    private const ushort StationAccessExtendedHoldFrames = 0x60;
    private const ushort SaveAnimationFirstFrameList = 0xaffa;
    private const ushort SaveAnimationSecondFrameList = 0xaffe;
    private const int SaveAnimationLoopCountAddress = 0x84aff9;

    private readonly List<StationActivationEvent> _stationActivationEvents = [];
    private bool _saveStationLockedOut;

    /// <summary>Station messages/actions emitted by the most recent PLM handler pass.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivationEvents =>
        _stationActivationEvents;

    /// <summary>Every resident cartridge station in native physical-slot order.</summary>
    public IReadOnlyList<StationPlmSnapshot> Stations => _slots
        .Select((slot, index) => (slot, index))
        .Where(entry => entry.slot.Active && entry.slot.Station is not null)
        .OrderByDescending(entry => entry.index)
        .Select(entry => new StationPlmSnapshot(
            entry.index,
            entry.slot.BlockIndex,
            entry.slot.RoomArgument,
            entry.slot.Station!.Kind,
            entry.slot.Station.Triggered,
            entry.slot.Station.SavePhase,
            _saveStationLockedOut))
        .ToArray();

    /// <summary>
    /// Sets WRAM <c>$1E75</c>'s room-entry lockout used when loading directly onto a save
    /// station. Ordinary destination-room construction clears it by creating a fresh PLM
    /// owner; an SRAM load explicitly sets it after the station population is installed.
    /// </summary>
    public void LockSaveStationForCurrentRoomEntry() => _saveStationLockedOut = true;

    /// <summary>
    /// Resumes the sleeping save-station PLM after bank $85 returns its YES/NO result.
    /// The accepted route executes `$84:8CF1` and enters `$AFF2`; the declined route jumps
    /// to `$B008`, which still installs the once-per-room-entry lockout.
    /// </summary>
    public bool ResolveSaveStationConfirmation(
        ISnesAddressSpace bus,
        StationActivationEvent activation,
        bool accepted)
    {
        ArgumentNullException.ThrowIfNull(bus);
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
            return false;
        }

        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException("Confirmed save station has no live Samus owner.");
        samus.XPosition = unchecked((ushort)((samus.XPosition + 8) & 0xfff0));
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.InputLocked = true;

        station.SavePhase = SaveStationPhase.Animating;
        station.AnimationFrame = 0;
        station.AnimationTimer = 1;
        station.SaveAnimationLoopsRemaining = bus.ReadByte(SaveAnimationLoopCountAddress);
        if (station.SaveAnimationLoopsRemaining == 0)
        {
            throw new InvalidDataException(
                "Save-station animation loop count at $84:AFF9 is zero.");
        }
        station.SaveStartSoundPending = true;
        return true;
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

    /// <summary>Every live elevator-platform PLM in native physical-slot order.</summary>
    public IReadOnlyList<RoomPlmSlotSnapshot> ElevatorPlatforms => PopulationSlots
        .Where(snapshot => _slots[snapshot.NativeSlotIndex].IsElevatorPlatform)
        .ToArray();

    private static bool TrySetupStationOrElevator(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        AreaId area,
        PlmSlot slot)
    {
        switch (slot.HeaderPointer)
        {
            case ElevatorPlatformHeader:
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

            case MapStationHeader:
                SetupStation(level, streamer, system, area, slot, StationKind.Map);
                return true;
            case EnergyStationHeader:
                SetupStation(level, streamer, system, area, slot, StationKind.Energy);
                return true;
            case MissileStationHeader:
                SetupStation(level, streamer, system, area, slot, StationKind.Missile);
                return true;
            case SaveStationHeader:
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

    /// <summary>
    /// Publishes contact with BTS $47-$4D to its resident station. This is the same generic
    /// type-$B collision seam used by item and scroll PLMs; no room identity participates.
    /// </summary>
    public bool TryNotifyStationTouch(int accessBlockIndex, byte behavior)
        => TryNotifyStationTouch(accessBlockIndex, new RoomBlockBehavior(behavior));

    /// <summary>Typed BTS overload used by room collision dispatch.</summary>
    public bool TryNotifyStationTouch(int accessBlockIndex, RoomBlockBehavior behavior)
        => TryNotifyStationCollision(
            accessBlockIndex,
            behavior,
            collisionPose: byte.MaxValue,
            horizontal: true,
            movingPositive: true,
            bypassSetupGate: true);

    /// <summary>
    /// Runs the cartridge access setup gate for an ordinary movement collision. A rejected
    /// pose/direction still returns true when the resident parent exists because the type-B
    /// access block remains solid; only its activation side effect is conditional.
    /// </summary>
    public bool TryNotifyStationCollision(
        int accessBlockIndex,
        byte behavior,
        byte collisionPose,
        bool horizontal,
        bool movingPositive)
        => TryNotifyStationCollision(
            accessBlockIndex,
            new RoomBlockBehavior(behavior),
            collisionPose,
            horizontal,
            movingPositive);

    /// <summary>Typed BTS overload used by room collision dispatch.</summary>
    public bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        byte collisionPose,
        bool horizontal,
        bool movingPositive)
        => TryNotifyStationCollision(
            accessBlockIndex,
            behavior,
            collisionPose,
            horizontal,
            movingPositive,
            bypassSetupGate: false);

    private bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        byte collisionPose,
        bool horizontal,
        bool movingPositive,
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
            _ => int.MinValue,
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
                // Save trigger B590 accepts a downward floor probe only while standing.
                StationAccessBehavior.SaveFloor => !horizontal && movingPositive &&
                    collisionPose is SamusPoseIds.FacingRightNormalPose or
                        SamusPoseIds.FacingLeftNormalPose,
                _ => false,
            };
            if (setupAccepted && slot.Station.OperationPhase == StationOperationPhase.Idle &&
                (slot.Station.Kind != StationKind.Save ||
                    (slot.Station.SavePhase == SaveStationPhase.Idle && !_saveStationLockedOut)))
            {
                slot.Station.Triggered = true;
                slot.Station.AccessBlockIndex = accessBlockIndex;
                slot.Station.AccessBehavior = access;
            }
            return true;
        }
        return false;
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

        if (station.Triggered)
        {
            station.Triggered = false;
            bool canActivate = station.Kind switch
            {
                StationKind.Map => !(_collectibleSystem ?? throw new InvalidOperationException(
                    "Map station has no bank-$80 state owner.")).HasAreaMap(station.AreaIndex),
                StationKind.Energy => samus.Health < samus.MaxHealth,
                StationKind.Missile => samus.Missiles < samus.MaxMissiles,
                StationKind.Save => true,
                _ => throw new InvalidDataException($"Unknown station kind {station.Kind}."),
            };
            if (canActivate && station.Kind == StationKind.Save)
            {
                station.SavePhase = SaveStationPhase.AwaitingConfirmation;
                PublishStationActivation(station, slot, samus);
            }
            else if (canActivate)
            {
                // Access lists draw their first extension frame for six ticks, hold the
                // fully inserted arm for $60, then execute the activation opcode. Movement
                // command six owns Samus for that entire synchronous sequence.
                station.OperationPhase = StationOperationPhase.Extending;
                station.OperationTimer = StationAccessMovementFrames;
                samus.InputLocked = true;
                _soundRequests.Add(new PlmSoundRequest(SoundEffectLibrary.Library2, 0x37, MaximumQueued: 6));
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

        if (station.OperationPhase != StationOperationPhase.Idle)
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
                        station.OperationPhase = StationOperationPhase.PostActivationHold;
                        station.OperationTimer = StationAccessMovementFrames;
                        break;
                    case StationOperationPhase.PostActivationHold:
                        _soundRequests.Add(new PlmSoundRequest(SoundEffectLibrary.Library2, 0x38, MaximumQueued: 6));
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
                        station.OperationPhase = StationOperationPhase.Idle;
                        station.AccessBlockIndex = -1;
                        station.AccessBehavior = null;
                        samus.InputLocked = false;
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

        ushort timer = ReadBank84Word(
            bus,
            unchecked((ushort)(station.AnimationList + station.AnimationFrame * 4)));
        ushort draw = ReadBank84Word(
            bus,
            unchecked((ushort)(station.AnimationList + station.AnimationFrame * 4 + 2)));
        if ((timer & 0x8000) != 0 || timer == 0)
        {
            throw new InvalidDataException(
                $"{station.Kind} station animation $84:{station.AnimationList:X4} " +
                $"frame {station.AnimationFrame} does not begin with a timed draw.");
        }
        DrawRomInstruction(
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

        if (station.SaveStartSoundPending)
        {
            // `$84:AFF4` queues sound $2E in library one immediately after centering Samus.
            _soundRequests.Add(new PlmSoundRequest(SoundEffectLibrary.Library1, 0x2e, MaximumQueued: 6));
            station.SaveStartSoundPending = false;
        }

        station.AnimationTimer--;
        if (station.AnimationTimer != 0)
            return true;

        ushort list = station.AnimationFrame == 0
            ? SaveAnimationFirstFrameList
            : SaveAnimationSecondFrameList;
        ushort timer = ReadBank84Word(bus, list);
        ushort draw = ReadBank84Word(bus, unchecked((ushort)(list + 2)));
        if (timer != 4)
        {
            throw new InvalidDataException(
                $"Save-station animation list $84:{list:X4} has timer {timer}, expected 4.");
        }
        DrawRomInstruction(
            bus,
            level,
            streamer,
            slot.BlockIndex,
            draw,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);
        station.AnimationTimer = timer;
        station.AnimationFrame ^= 1;
        if (station.AnimationFrame == 0)
        {
            station.SaveAnimationLoopsRemaining--;
            if (station.SaveAnimationLoopsRemaining == 0)
            {
                station.SavePhase = SaveStationPhase.AwaitingCompletionMessageClose;
                _stationActivationEvents.Add(new StationActivationEvent(
                    StationKind.Save,
                    GameplayMessageIds.SaveCompleted,
                    station.AreaIndex,
                    slot.RoomArgument,
                    slot.BlockIndex));
            }
        }
        return true;
    }

    private void PublishStationActivation(
        StationPlmState station,
        PlmSlot slot,
        SamusState samus)
    {
        int message = station.Kind switch
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
        ushort accessHeader = station.AccessBehavior switch
        {
            StationAccessBehavior.MapRight => 0xb6d7,
            StationAccessBehavior.MapLeft => 0xb6db,
            StationAccessBehavior.EnergyRight => 0xb6e3,
            StationAccessBehavior.EnergyLeft => 0xb6e7,
            StationAccessBehavior.MissileRight => 0xb6ef,
            StationAccessBehavior.MissileLeft => 0xb6f3,
            _ => throw new InvalidDataException(
                $"Station access has invalid BTS ${station.AccessBehavior:X2}."),
        };
        ushort instructionList = ReadBank84Word(
            bus,
            unchecked((ushort)(accessHeader + 2)));
        int firstDrawOffset = station.Kind == StationKind.Map ? 5 : 9;
        int drawOffset = firstDrawOffset + (extended ? 4 : 0);
        ushort drawPointer = ReadBank84Word(
            bus,
            unchecked((ushort)(instructionList + drawOffset)));
        DrawRomInstruction(
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
        public bool SaveStartSoundPending { get; set; }
    }

    private enum StationOperationPhase : byte
    {
        Idle,
        Extending,
        Extended,
        PostActivationHold,
        Retracting,
        FinalRetractionHold,
    }
}

public enum StationKind : byte
{
    Map,
    Energy,
    Missile,
    Save,
}

public readonly record struct StationActivationEvent(
    StationKind Kind,
    int MessageBoxIndex,
    AreaId AreaIndex,
    ushort StationIndex,
    int BlockIndex);

public readonly record struct StationPlmSnapshot(
    int NativeSlotIndex,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered,
    SaveStationPhase SavePhase,
    bool SaveStationLockedOut);

public enum SaveStationPhase : byte
{
    Idle,
    AwaitingConfirmation,
    Animating,
    AwaitingCompletionMessageClose,
}
