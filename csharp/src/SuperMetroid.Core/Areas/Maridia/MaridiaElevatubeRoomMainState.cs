using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// Shared translation of Maridia elevatube room main <c>$8F:E2B6</c> and the four door
/// callbacks that initialize or release it. The state preserves the cartridge's scratch
/// words so their signed fixed-point behavior remains debugger-visible. The fraction of the
/// tracked position is the shared <c>RoomMainASMVar1</c>, which the door callbacks leave stale.
/// </summary>
public sealed class MaridiaElevatubeRoomMainState
{
    /// <summary>Whether the selected room state names main routine $8F:E2B6.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Native RoomMainASMVar2: whole-pixel half of the tube's tracked position.</summary>
    public ushort Position { get; private set; }

    /// <summary>Native RoomMainASMVar3: signed 8.8 vertical velocity.</summary>
    public ushort Velocity { get; private set; }

    /// <summary>Native RoomMainASMVar4: signed 8.8 acceleration.</summary>
    public ushort Acceleration { get; private set; }

    /// <summary>
    /// Selects whether $E2B6 owns this room. Room loading leaves the room-main words as they
    /// were; the door callbacks initialize the ones this routine reads.
    /// </summary>
    public void Reset(bool active) => IsActive = active;

    /// <summary>Applies door callback $8F:E26C for entry from Oasis to the south.</summary>
    public void SetUpFromSouth(SamusState samus)
    {
        EnsureActive(DoorSetupCode.DoorASM_SetupElevatubeFromSouth);
        Ensure.NotNull(samus);
        // $8F:E272 is STZ $07E3, immediately overwritten; RoomMainASMVar1 stays as it was.
        Position = MaridiaElevatubeRomData.SouthStartingPosition;
        Velocity = MaridiaElevatubeRomData.SouthStartingVelocity;
        Acceleration = MaridiaElevatubeRomData.SouthAcceleration;
        samus.ScriptLocks.SetStationaryScriptControlLock(true);
    }

    /// <summary>Applies door callback $8F:E291 for entry from Plasma Spark to the north.</summary>
    public void SetUpFromNorth(SamusState samus)
    {
        EnsureActive(DoorSetupCode.DoorASM_SetupElevatubeFromNorth);
        Ensure.NotNull(samus);
        // $8F:E297 is STZ $07E3, immediately overwritten; RoomMainASMVar1 stays as it was.
        Position = MaridiaElevatubeRomData.NorthStartingPosition;
        Velocity = MaridiaElevatubeRomData.NorthStartingVelocity;
        Acceleration = MaridiaElevatubeRomData.NorthAcceleration;
        samus.ScriptLocks.SetStationaryScriptControlLock(true);
    }

    /// <summary>
    /// Applies Samus command one used by both exit callbacks. Room loading has already
    /// selected a non-elevatube main routine, so no host-side movement state survives.
    /// </summary>
    public void ResetOnExit(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.ScriptLocks.SetStationaryScriptControlLock(false);
        IsActive = false;
    }

    /// <summary>
    /// Runs one call of $8F:E2B6: pin X to $0080, integrate signed 8.8 velocity into both
    /// the room scratch position and Samus's 16.16 position through the ordinary bank-$94
    /// vertical collision dispatcher, then apply the cartridge's bounded acceleration.
    /// </summary>
    public BlockMoveResult? Step(

        RoomLevelData level,
        SamusState? samus,
        int nmiFrameCounter,
        RoomPlmSystem plms,
        RoomMainScratchState scratch)
    {
        if (!IsActive)
            return null;

        Ensure.NotNull(scratch);
        Ensure.NotNull(level);
        Ensure.NotNull(plms);
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Maridia elevatube room main requires an active Samus state.");

        // Native stores the whole and fractional X words separately every frame.
        activeSamus.Kinematics.Position = activeSamus.Kinematics.Position.WithX(new SnesFixedPosition(MaridiaElevatubeRomData.SamusCenterX, 0));

        int displacement = SnesSixteenSixteen.FromEightEight((short)Velocity);
        uint trackedPosition = SnesSixteenSixteen.Compose(Position, scratch.Var1);
        trackedPosition = unchecked(trackedPosition + (uint)displacement);
        Position = SnesFixedPosition.WholeOf(unchecked((int)trackedPosition));
        scratch.Var1 = unchecked((ushort)trackedPosition);

        BlockMoveResult movement = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, activeSamus.Kinematics, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = (nmiFrameCounter & 1) == 0, IncludeSolidEnemies = false });

        // The assembly performs an unsigned range test after biasing by $0E20. This is
        // precisely the inclusive signed range -$0E20..+$0E20; a candidate outside it is
        // rejected rather than clamped, leaving the preceding velocity intact.
        ushort candidate = unchecked((ushort)(Velocity + Acceleration));
        ushort biased = unchecked((ushort)(candidate + MaridiaElevatubeRomData.MaximumSpeed));
        if (biased < MaridiaElevatubeRomData.SpeedRangeExclusiveEnd)
            Velocity = candidate;

        return movement;
    }

    /// <summary>Rejects a door setup callback when the room has not selected elevatube main $8F:E2B6.</summary>
    private void EnsureActive(DoorSetupCode setupCode)
    {
        if (IsActive)
            return;
        throw new InvalidDataException(
            $"Elevatube setup callback $8F:{(int)setupCode:X4} selected a room without main $8F:E2B6.");
    }
}
