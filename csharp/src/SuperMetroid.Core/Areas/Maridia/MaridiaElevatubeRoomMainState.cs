using SuperMetroid.Core.Rooms;
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
    /// <summary>
    /// The tube's tracked room height (#2016): the room Y its ride moves by the velocity each update, with its fraction.
    /// </summary>
    public FixedCoordinate TrackedY { get; private set; }

    /// <summary>The tube's vertical velocity in room pixels per update, negative upward.</summary>
    public SignedEightEight Velocity { get; private set; }

    /// <summary>The tube's vertical acceleration in room pixels per update per update, negative upward.</summary>
    public SignedEightEight Acceleration { get; private set; }

    /// <summary>
    /// Builds the elevatube's ride for a room whose state runs the elevatube (#2287). The door behaviours initialize the
    /// room-main values this behaviour reads.
    /// </summary>
    /// <param name="carryover">The room-main value the previous room left, whose fraction the tube adopts.</param>
    public MaridiaElevatubeRoomMainState(RoomMainCarryover carryover)
    {
        TrackedY = FixedCoordinate.Of(0, Ensure.NotNull(carryover).Fraction);
    }

    /// <summary>Applies the door behaviour for entry from Oasis to the south.</summary>
    public void SetUpFromSouth(SamusState samus)
    {
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
        Ensure.NotNull(samus);
        // Entry from the north leaves the tracked fraction as it was.
        TrackedY = TrackedY.WithWhole(MaridiaElevatubeDefinitions.NorthStartingY);
        Velocity = MaridiaElevatubeDefinitions.NorthStartingVelocity;
        Acceleration = MaridiaElevatubeDefinitions.NorthAcceleration;
        samus.Control.LockStationary();
    }

    /// <summary>
    /// Runs the elevatube once: hold Samus at X 128, move both the tracked height and Samus by the velocity, Samus
    /// through her ordinary vertical collision check, then add the acceleration to the velocity unless that passes the
    /// maximum speed.
    /// </summary>
    internal BlockMoveResult Step(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects,
        RoomMainCarryover carryover)
    {
        Ensure.NotNull(carryover);
        Ensure.NotNull(level);
        Ensure.NotNull(roomObjects);
        Ensure.NotNull(samus);

        // Samus is held at the tube's centre every update, with no fraction of a pixel.
        samus.Kinematics.Position = samus.Kinematics.Position.WithX(new FixedCoordinate(MaridiaElevatubeDefinitions.SamusCenterX, 0));

        SignedSixteenSixteen displacement = Velocity.ToSixteenSixteenDelta();
        TrackedY = TrackedY.MovedBy(displacement);
        carryover.LeaveFraction(TrackedY.Fraction);

        BlockMoveResult movement = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false });

        // The game's range test is exactly the inclusive range of the maximum speed either way; a candidate outside it is
        // rejected rather than clamped, leaving the preceding velocity intact.
        SignedEightEight candidate = Velocity.Accelerated(Acceleration);
        if (candidate >= -MaridiaElevatubeDefinitions.MaximumSpeed && candidate <= MaridiaElevatubeDefinitions.MaximumSpeed)
            Velocity = candidate;

        return movement;
    }
}
