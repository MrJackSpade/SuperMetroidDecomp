using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// Shared translation of Maridia elevatube room main <c>$8F:E2B6</c> and the four door
/// callbacks that initialize or release it. The state preserves the cartridge's scratch
/// words so their signed fixed-point behavior remains debugger-visible. The fraction of the
/// tracked position is the shared <c>RoomMainASMVar1</c>, which the door callbacks leave stale.
/// </summary>
[RoomLifetime]
public sealed class MaridiaElevatubeRoomMainState
{
    /// <summary>Whether the selected room state names main routine $8F:E2B6.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The tube's tracked room position at X zero: its whole Y pixels, whose subpixels the shared room-main scratch
    /// carries, wrapping at 16 bits as the cartridge's word does.
    /// </summary>
    public Position16<RoomPixels> TrackedPosition { get; private set; }

    /// <summary>The tube's signed 8.8 vertical velocity in room pixels per update.</summary>
    public SnesSignedEightEight Velocity { get; private set; }

    /// <summary>The tube's signed 8.8 vertical acceleration in room pixels per update per update.</summary>
    public SnesSignedEightEight Acceleration { get; private set; }

    /// <summary>
    /// Builds one room's elevatube owner, selecting whether $E2B6 owns the room. The door callbacks initialize the
    /// room-main words this routine reads.
    /// </summary>
    /// <param name="active">Whether the room state names main routine $8F:E2B6.</param>
    public MaridiaElevatubeRoomMainState(bool active) => IsActive = active;

    /// <summary>Applies door callback $8F:E26C for entry from Oasis to the south.</summary>
    public void SetUpFromSouth(SamusState samus)
    {
        EnsureActive(DoorSetupCode.DoorASM_SetupElevatubeFromSouth);
        Ensure.NotNull(samus);
        // $8F:E272 is STZ $07E3, immediately overwritten; RoomMainASMVar1 stays as it was.
        TrackedPosition = new(0, MaridiaElevatubeDefinitions.SouthStartingY);
        Velocity = MaridiaElevatubeDefinitions.SouthStartingVelocity;
        Acceleration = MaridiaElevatubeDefinitions.SouthAcceleration;
        samus.Control.LockStationary();
    }

    /// <summary>Applies door callback $8F:E291 for entry from Plasma Spark to the north.</summary>
    public void SetUpFromNorth(SamusState samus)
    {
        EnsureActive(DoorSetupCode.DoorASM_SetupElevatubeFromNorth);
        Ensure.NotNull(samus);
        // $8F:E297 is STZ $07E3, immediately overwritten; RoomMainASMVar1 stays as it was.
        TrackedPosition = new(0, MaridiaElevatubeDefinitions.NorthStartingY);
        Velocity = MaridiaElevatubeDefinitions.NorthStartingVelocity;
        Acceleration = MaridiaElevatubeDefinitions.NorthAcceleration;
        samus.Control.LockStationary();
    }

    /// <summary>
    /// Applies Samus command one used by both exit callbacks. Room loading has already
    /// selected a non-elevatube main routine, so no host-side movement state survives.
    /// </summary>
    public void ResetOnExit(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Control.Unlock();
        IsActive = false;
    }

    /// <summary>
    /// Runs one call of $8F:E2B6: pin X to $0080, integrate signed 8.8 velocity into both
    /// the room scratch position and Samus's 16.16 position through the ordinary bank-$94
    /// vertical collision dispatcher, then apply the cartridge's bounded acceleration.
    /// </summary>
    internal BlockMoveResult? Step(

        RoomLevelData level,
        SamusState? samus,
        int frameCounter,
        ISamusRoomPlms plms,
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
        activeSamus.Kinematics.Position = activeSamus.Kinematics.Position.WithX(new SnesFixedPosition(MaridiaElevatubeDefinitions.SamusCenterX, 0));

        int displacement = SnesSixteenSixteen.FromEightEight(Velocity.SignedRawValue);
        uint trackedPosition = SnesSixteenSixteen.Compose(TrackedPosition.Y, scratch.Var1);
        trackedPosition = unchecked(trackedPosition + (uint)displacement);
        TrackedPosition = new(0, SnesFixedPosition.WholeOf(unchecked((int)trackedPosition)));
        scratch.Var1 = unchecked((ushort)trackedPosition);

        BlockMoveResult movement = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, activeSamus.Kinematics, activeSamus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = (frameCounter & 1) == 0, IncludeSolidEnemies = false });

        // The cartridge's biased unsigned range test is exactly the inclusive signed range of the maximum speed either
        // way; a candidate outside it is rejected rather than clamped, leaving the preceding velocity intact. The sum
        // wraps as the cartridge's word does, so an overflowed candidate is judged by its wrapped sign.
        SnesSignedEightEight candidate = new(unchecked((ushort)(Velocity.RawValue + Acceleration.RawValue)));
        if (Math.Abs((int)candidate.SignedRawValue) <= MaridiaElevatubeDefinitions.MaximumSpeed.SignedRawValue)
            Velocity = candidate;

        return movement;
    }

    /// <summary>Rejects a door setup callback when the room has not selected elevatube main $8F:E2B6.</summary>
    private void EnsureActive(DoorSetupCode setupCode)
    {
        if (IsActive)
            return;
        throw new InvalidDataException(
            $"Elevatube setup callback {setupCode} selected a room without the elevatube's room main.");
    }
}
