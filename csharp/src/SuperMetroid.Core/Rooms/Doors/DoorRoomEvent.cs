namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What a door's setup starts in the room it leads into besides changing its screens (#1721): a behaviour object that
/// runs its own work at each point of the room's load it acts at (#2485). It composes what the room shows from its first
/// frame before the room's graphics and actors initialize (<see cref="Compose"/>), and changes the room once its own room
/// objects are placed, before its enemies load (<see cref="Run"/>). <see cref="None"/> does nothing.
/// </summary>
public abstract partial class DoorRoomEvent
{
    /// <summary>Names the event.</summary>
    /// <param name="name">The event's name, as the door setups author it.</param>
    private protected DoorRoomEvent(string name) => Name = Ensure.NotNull(name);

    /// <summary>The event's name, as the door setups author it.</summary>
    public string Name { get; }

    /// <summary>Composes what the event shows from the room's first frame, before the room's graphics and actors initialize.</summary>
    /// <param name="load">The room being loaded.</param>
    internal virtual void Compose(IDoorEventLoad load) => Ensure.NotNull(load);

    /// <summary>Changes the room once its own room objects are placed and before its enemies load.</summary>
    /// <param name="room">The room the door leads into.</param>
    internal virtual void Run(IDoorEventRoom room) => Ensure.NotNull(room);

    /// <summary>Whether the event starts the Wrecked Ship's treadmill, whose setup blocks the room's entrance then holds.</summary>
    internal virtual bool StartsWreckedShipTreadmill => false;

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
    private sealed partial class NoDoorRoomEvent() : DoorRoomEvent(nameof(None));

    /// <summary>The Ceres elevator door's event, which shows the shaft level as a turning floor and starts its turn.</summary>
    private sealed partial class ShaftTurningFloorShown() : DoorRoomEvent(nameof(ShowsTurningFloor))
    {
        /// <inheritdoc/>
        internal override void Compose(IDoorEventLoad load) => Ensure.NotNull(load).ShowShaftLevelAndStartTurn();
    }

    /// <summary>The shaft's exit door's event, which returns the screen to the ordinary room display.</summary>
    private sealed partial class TurningFloorEnded() : DoorRoomEvent(nameof(EndsTurningFloor))
    {
        /// <inheritdoc/>
        /// <remarks>Its door leads out of the shaft, so the room it loads has no shaft's turn to end (#2287).</remarks>
        internal override void Run(IDoorEventRoom room) => Ensure.NotNull(room).ShowOrdinaryDisplay();
    }

    /// <summary>The elevatube's exit doors' event, which only restores Samus's control.</summary>
    private sealed partial class ElevatubeRideEnded() : DoorRoomEvent(nameof(EndsElevatubeRide))
    {
        /// <inheritdoc/>
        /// <remarks>Both exit doors lead out of the tube, so the room they load has no ride to end (#2287).</remarks>
        internal override void Run(IDoorEventRoom room) => Ensure.NotNull(room).ReleaseSamus();
    }

    /// <summary>A Wrecked Ship entrance door's event, which starts its treadmill running one way.</summary>
    /// <param name="direction">The way the treadmill's belt runs.</param>
    private sealed partial class TreadmillStarted(WreckedShipTreadmillDirection direction) : DoorRoomEvent($"StartsTreadmill{direction}")
    {
        /// <inheritdoc/>
        internal override bool StartsWreckedShipTreadmill => true;

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

    /// <summary>An elevatube entry door's event, which starts the ride from its end of the tube.</summary>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    private sealed partial class ElevatubeRideStarted(ElevatubeEntry entry) : DoorRoomEvent($"StartsElevatubeRide{entry}")
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The setup ignores whether the tube's room object found a free place: with every place held, Samus is still placed
        /// for the ride and the elevatube does not appear.
        /// </remarks>
        internal override void Run(IDoorEventRoom room)
        {
            Ensure.NotNull(room).StartElevatubeRide(entry);
            _ = room.TrySpawnElevatube();
        }
    }
}
