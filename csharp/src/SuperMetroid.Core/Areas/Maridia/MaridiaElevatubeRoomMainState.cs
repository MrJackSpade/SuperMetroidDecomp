using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// The Maridia elevatube's room main behaviour and the four door
/// behaviours that initialize or release it. The fraction of the tracked height starts from what the previous
/// room's main left, because the door behaviours do not set it, and is left for the following room's main
/// (<see cref="ElevatubeInheritedFraction"/>).
/// </summary>
[RoomLifetime]
public sealed class MaridiaElevatubeRoomMainState
{
    /// <summary>
    /// The tube's tracked room height (#2016): the room Y its ride moves by the velocity each update, with its fraction.
    /// </summary>
    public FixedCoordinate TrackedY { get; private set; }

    /// <summary>The hand-over the tube's fraction is adopted from and left in for the following room's main.</summary>
    private readonly ElevatubeInheritedFraction _fraction;

    /// <summary>The tube's vertical velocity in room pixels per update, negative upward.</summary>
    public SignedEightEight Velocity { get; private set; }

    /// <summary>The tube's vertical acceleration in room pixels per update per update, negative upward.</summary>
    public SignedEightEight Acceleration { get; private set; }

    /// <summary>
    /// Builds the elevatube's ride for a room whose state runs the elevatube (#2287). The door behaviours initialize the
    /// room-main values this behaviour reads.
    /// </summary>
    /// <param name="fraction">The fraction the previous room's main left, which the tube adopts and leaves its own in.</param>
    public MaridiaElevatubeRoomMainState(ElevatubeInheritedFraction fraction)
    {
        _fraction = Ensure.NotNull(fraction);
        TrackedY = FixedCoordinate.Of(0, fraction.Fraction);
    }

    /// <summary>
    /// Starts the ride from <paramref name="entry"/>, as the tube's entry door does (#2485): the tube's starting height,
    /// speed and acceleration for a ride from that end, Samus held still. The tracked fraction stays as it was.
    /// </summary>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    /// <param name="samus">Samus, whom the ride holds still.</param>
    public void StartRide(ElevatubeEntry entry, SamusState samus)
    {
        Ensure.NotNull(samus);
        MaridiaElevatubeRideStart start = MaridiaElevatubeDefinitions.RideStarts[entry];
        TrackedY = TrackedY.WithWhole(start.StartingY);
        Velocity = start.StartingVelocity;
        Acceleration = start.Acceleration;
        samus.Control.LockStationary();
    }

    /// <summary>
    /// Runs the elevatube once: hold Samus at X 128, move both the tracked height and Samus by the velocity, Samus
    /// through her ordinary vertical collision check, then add the acceleration to the velocity unless that passes the
    /// maximum speed.
    /// </summary>
    internal void Step(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(roomObjects);
        Ensure.NotNull(samus);

        // Samus is held at the tube's centre every update, with no fraction of a pixel.
        samus.Kinematics.Position = samus.Kinematics.Position.WithX(FixedCoordinate.FromWhole(MaridiaElevatubeDefinitions.SamusCenterX));

        SignedSixteenSixteen displacement = Velocity.ToSixteenSixteenDelta();
        TrackedY = TrackedY.MovedBy(displacement);
        _fraction.Leave(TrackedY.Fraction);

        SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter), IncludeSolidEnemies = false });

        // The game's range test is exactly the inclusive range of the maximum speed either way; a candidate outside it is
        // rejected rather than clamped, leaving the preceding velocity intact.
        SignedEightEight candidate = Velocity.Accelerated(Acceleration);
        if (candidate >= -MaridiaElevatubeDefinitions.MaximumSpeed && candidate <= MaridiaElevatubeDefinitions.MaximumSpeed)
            Velocity = candidate;
    }
}
