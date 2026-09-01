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

    private readonly List<StationActivationEvent> _stationActivationEvents = [];

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
            entry.slot.Station.Triggered))
        .ToArray();

    /// <summary>Every live elevator-platform PLM in native physical-slot order.</summary>
    public IReadOnlyList<RoomPlmSlotSnapshot> ElevatorPlatforms => PopulationSlots
        .Where(snapshot => _slots[snapshot.NativeSlotIndex].IsElevatorPlatform)
        .ToArray();

    private bool TrySetupStationOrElevator(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        byte areaIndex,
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
                SetupStation(level, streamer, system, areaIndex, slot, StationKind.Map);
                return true;
            case EnergyStationHeader:
                SetupStation(level, streamer, system, areaIndex, slot, StationKind.Energy);
                return true;
            case MissileStationHeader:
                SetupStation(level, streamer, system, areaIndex, slot, StationKind.Missile);
                return true;
            case SaveStationHeader:
                SetupStation(level, streamer, system, areaIndex, slot, StationKind.Save);
                return true;
            default:
                return false;
        }
    }

    private static void SetupStation(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        Bank80SystemState system,
        byte areaIndex,
        PlmSlot slot,
        StationKind kind)
    {
        ushort original = level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord;
        if (kind == StationKind.Save)
        {
            WriteStationBlock(level, streamer, slot.BlockIndex, original, 11, 0x4d);
            slot.Station = new StationPlmState(
                kind,
                areaIndex,
                slot.InstructionPointer,
                completedAnimationList: slot.InstructionPointer,
                animationFrameCount: 1);
            return;
        }

        WriteStationBlock(level, streamer, slot.BlockIndex, original, 8, behavior: null);
        (int rightOffset, int leftOffset, byte rightBts, byte leftBts) = kind switch
        {
            StationKind.Map => (1, -2, (byte)0x47, (byte)0x48),
            StationKind.Energy => (1, -1, (byte)0x49, (byte)0x4a),
            StationKind.Missile => (1, -1, (byte)0x4b, (byte)0x4c),
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
        ushort initialAnimationList = kind == StationKind.Map && system.HasAreaMap(areaIndex)
            ? completedAnimationList
            : normalAnimationList;
        slot.Station = new StationPlmState(
            kind,
            areaIndex,
            initialAnimationList,
            completedAnimationList,
            animationFrameCount: 3);
    }

    private static void WriteAccessBlock(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        byte behavior)
    {
        if ((uint)blockIndex >= (uint)level.ForegroundEntries.Length)
        {
            throw new InvalidDataException(
                $"Station access setup targets out-of-room block index {blockIndex}.");
        }
        ushort original = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
        WriteStationBlock(level, streamer, blockIndex, original, 11, behavior);
    }

    private static void WriteStationBlock(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort original,
        int collisionType,
        byte? behavior)
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
            behavior,
            collisionPose,
            horizontal,
            movingPositive,
            bypassSetupGate: false);

    private bool TryNotifyStationCollision(
        int accessBlockIndex,
        byte behavior,
        byte collisionPose,
        bool horizontal,
        bool movingPositive,
        bool bypassSetupGate)
    {
        if (behavior is < 0x47 or > 0x4d)
            return false;

        int parentBlockIndex = behavior switch
        {
            0x47 => accessBlockIndex - 1,
            0x48 => accessBlockIndex + 2,
            0x49 or 0x4b => accessBlockIndex - 1,
            0x4a or 0x4c => accessBlockIndex + 1,
            0x4d => accessBlockIndex,
            _ => int.MinValue,
        };

        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != parentBlockIndex || slot.Station is null)
                continue;

            bool setupAccepted = bypassSetupGate || behavior switch
            {
                // Right-side access is entered while moving left in pose $8A; left-side
                // access is the mirror in pose $89. These are the exact B1C8/B1F0 and
                // B26D-B300 setup predicates before cannon-height alignment.
                0x47 or 0x49 or 0x4b =>
                    horizontal && !movingPositive &&
                    collisionPose == SamusState.RanIntoWallLeftPose,
                0x48 or 0x4a or 0x4c =>
                    horizontal && movingPositive &&
                    collisionPose == SamusState.RanIntoWallRightPose,
                // Save trigger B590 accepts a downward floor probe only while standing.
                0x4d => !horizontal && movingPositive &&
                    collisionPose is SamusState.FacingRightNormalPose or
                        SamusState.FacingLeftNormalPose,
                _ => false,
            };
            if (setupAccepted && slot.Station.OperationPhase == StationOperationPhase.Idle)
            {
                slot.Station.Triggered = true;
                slot.Station.AccessBlockIndex = accessBlockIndex;
                slot.Station.AccessBehavior = behavior;
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
                _soundRequests.Add(new PlmSoundRequest(2, 0x37, MaximumQueued: 6));
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
                        _soundRequests.Add(new PlmSoundRequest(2, 0x38, MaximumQueued: 6));
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
                        station.AccessBehavior = 0;
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

    private void PublishStationActivation(
        StationPlmState station,
        PlmSlot slot,
        SamusState samus)
    {
        int message = station.Kind switch
        {
            StationKind.Map => 0x14,
            StationKind.Energy => 0x15,
            StationKind.Missile => 0x16,
            StationKind.Save => 0x17,
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
            slot.RoomArgument));
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
            0x47 => 0xb6d7,
            0x48 => 0xb6db,
            0x49 => 0xb6e3,
            0x4a => 0xb6e7,
            0x4b => 0xb6ef,
            0x4c => 0xb6f3,
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
        byte areaIndex,
        ushort animationList,
        ushort completedAnimationList,
        int animationFrameCount)
    {
        public StationKind Kind { get; } = kind;
        public byte AreaIndex { get; } = areaIndex;
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
        public byte AccessBehavior { get; set; }
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
    byte AreaIndex,
    ushort StationIndex);

public readonly record struct StationPlmSnapshot(
    int NativeSlotIndex,
    int BlockIndex,
    ushort RoomArgument,
    StationKind Kind,
    bool Triggered);
