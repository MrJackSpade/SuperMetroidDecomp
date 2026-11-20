namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What a door's setup starts in the room it leads into besides changing its screens (#1721): a behaviour object that
/// runs its own work at each point of the room's load it acts at (#2485). It composes what the room shows from its first
/// frame before the room's graphics and actors initialize (<see cref="Compose"/>), and changes the room once its own room
/// objects are placed, before its enemies load (<see cref="Run"/>). <see cref="None"/> does nothing. It replaces the
/// room-event enumeration every owner switched over or tested.
/// </summary>
public abstract class DoorRoomEvent
{
    /// <summary>Names the event.</summary>
    /// <param name="name">The event's name, as the door setups author it.</param>
    private protected DoorRoomEvent(string name) => Name = Ensure.NotNull(name);

    /// <summary>The event's name, as the door setups author it.</summary>
    public string Name { get; }

    /// <summary>
    /// Composes what the event shows from the room's first frame, before the room's graphics and actors initialize, and
    /// returns the event that changes the room once its own room objects are placed (<see cref="Run"/>): this one, or the
    /// one over what its composition built (#2554).
    /// </summary>
    /// <param name="load">The room being loaded.</param>
    internal virtual DoorRoomEvent Compose(IDoorEventLoad load)
    {
        Ensure.NotNull(load);
        return this;
    }

    /// <summary>Changes the room once its own room objects are placed and before its enemies load.</summary>
    /// <param name="room">The room the door leads into.</param>
    internal virtual void Run(IDoorEventRoom room) => Ensure.NotNull(room);

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>Nothing: the setup only changes screens, or does nothing at all.</summary>
    public static DoorRoomEvent None { get; } = new NoDoorRoomEvent();

    /// <summary>
    /// The Ceres elevator shaft is shown as a turning floor, level about the shaft's rotation centre, and its turn starts
    /// there: the door installs that view itself before the room's graphics and actors initialize.
    /// </summary>
    public static DoorRoomEvent ShowsTurningFloor { get; } = new ShaftTurningFloorShown();

    /// <summary>The screen returns from the turning floor to the ordinary room display.</summary>
    public static DoorRoomEvent EndsTurningFloor { get; } = new TurningFloorEnded();

    /// <summary>The elevatube ride ends: Samus is released.</summary>
    public static DoorRoomEvent EndsElevatubeRide { get; } = new ElevatubeRideEnded();

    /// <summary>
    /// The Wrecked Ship entrance's treadmill starts moving <paramref name="direction"/> and the treadmill object of the
    /// entrance it carries Samus from spawns.
    /// </summary>
    /// <param name="direction">The way the treadmill's belt runs.</param>
    public static DoorRoomEvent StartsTreadmill(WreckedShipTreadmillDirection direction) => new TreadmillStarted(direction);

    /// <summary>
    /// The elevatube ride starts from <paramref name="entry"/>: Samus is held still and the tube takes the starting height,
    /// speed and acceleration of a ride from that end, and the tube's room object spawns.
    /// </summary>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    public static DoorRoomEvent StartsElevatubeRide(ElevatubeEntry entry) => new ElevatubeRideStarted(entry);

    /// <summary>The event that does nothing.</summary>
    private sealed class NoDoorRoomEvent() : DoorRoomEvent(nameof(None));

    /// <summary>The Ceres elevator door's event, which shows the shaft level as a turning floor and starts its turn.</summary>
    private sealed class ShaftTurningFloorShown() : DoorRoomEvent(nameof(ShowsTurningFloor))
    {
        /// <inheritdoc/>
        internal override DoorRoomEvent Compose(IDoorEventLoad load)
        {
            Ensure.NotNull(load).ShowShaftLevelAndStartTurn();
            return this;
        }
    }

    /// <summary>The shaft's exit door's event, which returns the screen to the ordinary room display.</summary>
    private sealed class TurningFloorEnded() : DoorRoomEvent(nameof(EndsTurningFloor))
    {
        /// <inheritdoc/>
        /// <remarks>Its door leads out of the shaft, so the room it loads has no shaft's turn to end (#2287).</remarks>
        internal override void Run(IDoorEventRoom room) => Ensure.NotNull(room).ShowOrdinaryDisplay();
    }

    /// <summary>The elevatube's exit doors' event, which only restores Samus's control.</summary>
    private sealed class ElevatubeRideEnded() : DoorRoomEvent(nameof(EndsElevatubeRide))
    {
        /// <inheritdoc/>
        /// <remarks>Both exit doors lead out of the tube, so the room they load has no ride to end (#2287).</remarks>
        internal override void Run(IDoorEventRoom room) => Ensure.NotNull(room).ReleaseSamus();
    }

    /// <summary>A Wrecked Ship entrance door's event, which starts its treadmill running one way.</summary>
    /// <param name="direction">The way the treadmill's belt runs.</param>
    private sealed class TreadmillStarted(WreckedShipTreadmillDirection direction) : DoorRoomEvent($"StartsTreadmill{direction}")
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The setup ignores whether the treadmill object found a free room-object place: with every place held, the
        /// treadmill's blocks still animate and the entrance treadmill does not appear.
        /// </remarks>
        internal override void Run(IDoorEventRoom room)
        {
            Ensure.NotNull(room).StartTreadmillAnimation(direction);
            _ = room.TrySpawnEntranceTreadmill(direction);
        }
    }

    /// <summary>
    /// An elevatube entry door's event, which composes the tube's ride, which the room's main runs, and starts it from its end
    /// of the tube once the room's objects are placed (#2554).
    /// </summary>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    private sealed class ElevatubeRideStarted(ElevatubeEntry entry) : DoorRoomEvent($"StartsElevatubeRide{entry}")
    {
        /// <inheritdoc/>
        internal override DoorRoomEvent Compose(IDoorEventLoad load) => new ElevatubeRideRun(Name, entry, Ensure.NotNull(load).ComposeElevatubeRide());
    }

    /// <summary>The elevatube ride a tube's entry door composed, which it starts from its end once the room's objects are placed.</summary>
    /// <param name="name">The event's name.</param>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    /// <param name="ride">The tube's ride.</param>
    private sealed class ElevatubeRideRun(string name, ElevatubeEntry entry, IElevatubeRide ride) : DoorRoomEvent(name)
    {
        /// <summary>The tube's ride.</summary>
        private readonly IElevatubeRide _ride = Ensure.NotNull(ride);

        /// <inheritdoc/>
        /// <remarks>
        /// The tube takes the starting height, speed and acceleration of a ride from its end, then Samus is held still. The
        /// setup ignores whether the tube's room object found a free place: with all forty held, Samus is still placed for
        /// the ride and the elevatube does not appear.
        /// </remarks>
        internal override void Run(IDoorEventRoom room)
        {
            _ride.Start(entry);
            Ensure.NotNull(room).HoldSamusStill();
            _ = room.TrySpawnElevatube();
        }
    }
}
