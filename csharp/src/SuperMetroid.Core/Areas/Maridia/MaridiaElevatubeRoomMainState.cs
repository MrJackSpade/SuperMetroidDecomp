using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// The Maridia elevatube's room main behaviour and the four door
/// behaviours that initialize or release it. The fraction of the tracked height starts from what the previous
/// room's main left (<see cref="RoomMainCarryover"/>), because the door behaviours do not set it, and is left for the
/// following room's main.
/// </summary>
[RoomLifetime]
public sealed class MaridiaElevatubeRoomMainState
{
    /// <summary>Whether the selected room state runs the elevatube.</summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The tube's tracked room height (#2016): the room Y its ride moves by the velocity each update, with its fraction,
    /// wrapping as the game's coordinate does.
    /// </summary>
    public FixedCoordinate TrackedY { get; private set; }

    /// <summary>The tube's signed 1/256-pixel vertical velocity in room pixels per update.</summary>
    public SignedEightEight Velocity { get; private set; }

    /// <summary>The tube's signed 1/256-pixel vertical acceleration in room pixels per update per update.</summary>
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
        TrackedY = FixedCoordinate.Of(0, Ensure.NotNull(carryover).Fraction);
    }

    /// <summary>Applies the door behaviour for entry from Oasis to the south.</summary>
    public void SetUpFromSouth(SamusState samus)
    {
        EnsureActive(DoorRoomEvent.StartsElevatubeRideFromSouth);
        Ensure.NotNull(samus);
        // Entry from the south leaves the tracked fraction as it was.
        TrackedY = TrackedY.WithWhole(MaridiaElevatubeDefinitions.SouthStartingY);
        Velocity = MaridiaElevatubeDefinitions.SouthStartingVelocity;
        Acceleration = MaridiaElevatubeDefinitions.SouthAcceleration;
        samus.Control.LockStationary();
    }

    /// <summary>Applies the door behaviour for entry from Plasma Spark to the north.</summary>
    public void SetUpFromNorth(SamusState samus)
    {
        EnsureActive(DoorRoomEvent.StartsElevatubeRideFromNorth);
        Ensure.NotNull(samus);
        // Entry from the north leaves the tracked fraction as it was.
        TrackedY = TrackedY.WithWhole(MaridiaElevatubeDefinitions.NorthStartingY);
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
    /// Runs the elevatube once: pin X to 128, integrate signed 1/256-pixel velocity into both
    /// the tracked height and Samus's 1/65536-pixel position through Samus's ordinary
    /// vertical collision check, then apply the game's bounded acceleration.
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
        TrackedY = TrackedY.MovedBy(displacement);
        carryover.LeaveFraction(TrackedY.Fraction);

        BlockMoveResult movement = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, activeSamus.Kinematics, activeSamus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false });

        // The game's range test is exactly the inclusive range of the maximum speed either
        // way; a candidate outside it is rejected rather than clamped, leaving the preceding velocity intact. The sum
        // wraps as the game's coordinate does, so an overflowed candidate is judged by whether its wrapped value reads as negative.
        SignedEightEight candidate = Velocity + Acceleration;
        if (candidate >= -MaridiaElevatubeDefinitions.MaximumSpeed && candidate <= MaridiaElevatubeDefinitions.MaximumSpeed)
            Velocity = candidate;

        return movement;
    }

    /// <summary>Rejects a door's setup behaviour when the room has not selected the elevatube.</summary>
    private void EnsureActive(DoorRoomEvent roomEvent)
    {
        if (IsActive)
            return;
        throw new InvalidDataException(
            $"Door setup event {roomEvent} drives the elevatube in a room whose room behaviour is not the elevatube.");
    }
}
