using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// The Maridia elevatube's room main behaviour and the four door
/// behaviours that initialize or release it. The fraction of the tracked position starts from what the previous
/// room's main left (<see cref="RoomMainCarryover"/>), because the door behaviours do not set it, and is left for the
/// following room's main.
/// </summary>
[RoomLifetime]
public sealed class MaridiaElevatubeRoomMainState
{
    /// <summary>Whether the selected room state runs the elevatube.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The tube's tracked room position at X zero: its whole Y pixels, wrapping as the game's coordinate does.
    /// </summary>
    public Position16<RoomPixels> TrackedPosition { get; private set; }

    /// <summary>The fraction of the tracked position's Y, 0 through 65535.</summary>
    public int TrackedFraction { get; private set; }

    /// <summary>The tube's signed 8.8 vertical velocity in room pixels per update.</summary>
    public SignedEightEight Velocity { get; private set; }

    /// <summary>The tube's signed 8.8 vertical acceleration in room pixels per update per update.</summary>
    public SignedEightEight Acceleration { get; private set; }

    /// <summary>
    /// Builds one room's elevatube owner, selecting whether the elevatube owns the room. The door behaviours initialize the
    /// room-main values this behaviour reads.
    /// </summary>
    /// <param name="active">Whether the room state runs the elevatube.</param>
    /// <param name="carryover">The room-main value the previous room left, whose fraction the tube adopts.</param>
    public MaridiaElevatubeRoomMainState(bool active, RoomMainCarryover carryover)
    {
        IsActive = active;
        TrackedFraction = Ensure.NotNull(carryover).Fraction;
    }

    /// <summary>Applies the door behaviour for entry from Oasis to the south.</summary>
    public void SetUpFromSouth(SamusState samus)
    {
        EnsureActive(DoorSetupCode.ToiletStartsElevatubeRideFromSouth);
        Ensure.NotNull(samus);
        // Entry from the south leaves the tracked fraction as it was.
        TrackedPosition = new(0, MaridiaElevatubeDefinitions.SouthStartingY);
        Velocity = MaridiaElevatubeDefinitions.SouthStartingVelocity;
        Acceleration = MaridiaElevatubeDefinitions.SouthAcceleration;
        samus.Control.LockStationary();
    }

    /// <summary>Applies the door behaviour for entry from Plasma Spark to the north.</summary>
    public void SetUpFromNorth(SamusState samus)
    {
        EnsureActive(DoorSetupCode.ToiletStartsElevatubeRideFromNorth);
        Ensure.NotNull(samus);
        // Entry from the north leaves the tracked fraction as it was.
        TrackedPosition = new(0, MaridiaElevatubeDefinitions.NorthStartingY);
        Velocity = MaridiaElevatubeDefinitions.NorthStartingVelocity;
        Acceleration = MaridiaElevatubeDefinitions.NorthAcceleration;
        samus.Control.LockStationary();
    }

    /// <summary>
    /// Restores Samus's control, as both exit behaviours do. Room loading has already
    /// selected a non-elevatube main behaviour, so no elevatube movement state survives.
    /// </summary>
    public void ResetOnExit(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Control.Unlock();
        IsActive = false;
    }

    /// <summary>
    /// Runs the elevatube once: pin X to 128, integrate signed 8.8 velocity into both
    /// the tracked position and Samus's 16.16 position through Samus's ordinary
    /// vertical collision dispatcher, then apply the game's bounded acceleration.
    /// </summary>
    internal BlockMoveResult? Step(

        RoomLevelData level,
        SamusState? samus,
        int frameCounter,
        ISamusRoomObjects roomObjects,
        RoomMainCarryover carryover)
    {
        if (!IsActive)
            return null;

        Ensure.NotNull(carryover);
        Ensure.NotNull(level);
        Ensure.NotNull(roomObjects);
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Maridia elevatube room main requires an active Samus state.");

        // Stores the whole and fractional X separately every frame.
        activeSamus.Kinematics.Position = activeSamus.Kinematics.Position.WithX(new FixedCoordinate(MaridiaElevatubeDefinitions.SamusCenterX, 0));

        SignedSixteenSixteen displacement = Velocity.ToSixteenSixteenDelta();
        FixedCoordinate tracked = new FixedCoordinate(TrackedPosition.Y, TrackedFraction)
            .MovedBy(displacement);
        TrackedPosition = new(0, tracked.Whole);
        TrackedFraction = tracked.Fraction;
        carryover.LeaveFraction(TrackedFraction);

        BlockMoveResult movement = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, activeSamus.Kinematics, activeSamus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false });

        // The game's biased unsigned range test is exactly the inclusive signed range of the maximum speed either
        // way; a candidate outside it is rejected rather than clamped, leaving the preceding velocity intact. The sum
        // wraps as the game's coordinate does, so an overflowed candidate is judged by its wrapped sign.
        SignedEightEight candidate = Velocity + Acceleration;
        if (candidate >= -MaridiaElevatubeDefinitions.MaximumSpeed && candidate <= MaridiaElevatubeDefinitions.MaximumSpeed)
            Velocity = candidate;

        return movement;
    }

    /// <summary>Rejects a door's setup behaviour when the room has not selected the elevatube.</summary>
    private void EnsureActive(DoorSetupCode setupCode)
    {
        if (IsActive)
            return;
        throw new InvalidDataException(
            $"Door setup {setupCode} drives the elevatube in a room whose room behaviour is not the elevatube.");
    }
}
