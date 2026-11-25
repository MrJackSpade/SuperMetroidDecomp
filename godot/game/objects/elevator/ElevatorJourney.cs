using Engine.Core;
using Engine.Shared;

namespace Game;

/// <summary>
/// The elevator ride under way. It outlasts the room it begins in: the elevator of the room being left starts it, the
/// door between the rooms turns its leaving into an arrival, and the elevator of the room reached ends it. It also
/// keeps whether an elevator's door is armed, which the door's passage reads.
/// </summary>
/// <remarks>
/// The cartridge keeps one ride for the whole game, so every elevator of a room reads and moves by the same one.
/// </remarks>
public sealed class ElevatorJourney : ISessionService
{
    /// <summary>Where a ride stands.</summary>
    public enum Stage
    {
        /// <summary>No ride.</summary>
        None,

        /// <summary>Leaving the room it began in.</summary>
        Leaving,

        /// <summary>Brought through the door: the elevator of the room reached has yet to take its first turn.</summary>
        Arriving,

        /// <summary>Coming to rest in the room reached.</summary>
        Returning,
    }

    private bool _stood;
    private PixelPosition? _setDownAt;

    /// <summary>Where the ride stands.</summary>
    public Stage Status { get; private set; }

    /// <summary>Whether the ride goes down; kept from the last ride once it is over.</summary>
    public bool GoesDown { get; private set; }

    /// <summary>
    /// Whether an elevator's door is armed: Samus stood at it, or a ride was brought through a door. A door entered
    /// while it is armed is an elevator's.
    /// </summary>
    public bool DoorArmed { get; private set; }

    /// <summary>Whether the ride moves Samus by her own step down each update: while it leaves, and all through a ride down.</summary>
    public bool MovesSamus => Status == Stage.Leaving || (Status != Stage.None && GoesDown);

    /// <summary>The session's ride.</summary>
    public static ElevatorJourney Of(GameSession session) => session.Service<ElevatorJourney>();

    /// <summary>Samus stood at an elevator's door as she moved: the elevator hears of it on its next turn.</summary>
    public void NoteStand() => _stood = true;

    /// <summary>An elevator takes its turn in play: a stand made since the last arms the door.</summary>
    public void TakeStand()
    {
        if (_stood)
            DoorArmed = true;
        _stood = false;
    }

    /// <summary>Disarms the door and leaves the ride as it stands, as resuming from the pause does.</summary>
    public void Disarm() => DoorArmed = false;

    /// <summary>A ride begins.</summary>
    public void Begin(bool goesDown) => (Status, GoesDown) = (Stage.Leaving, goesDown);

    /// <summary>
    /// A door's passage sets up the room it leads to: a ride that was leaving arrives there, and the door is armed.
    /// </summary>
    public void PassDoor()
    {
        if (Status != Stage.Leaving)
            return;
        (Status, DoorArmed, _setDownAt) = (Stage.Arriving, true, null);
    }

    /// <summary>The elevator of the room reached has put Samus at <paramref name="place"/> as its room was set up.</summary>
    public void SetDownAt(PixelPosition place) => _setDownAt = place;

    /// <summary>Where the arriving elevator put Samus as its room was set up, handed over once; nothing when no ride arrives.</summary>
    public PixelPosition? TakeSetDownPlace()
    {
        PixelPosition? place = Status == Stage.Arriving ? _setDownAt : null;
        _setDownAt = null;
        return place;
    }

    /// <summary>The arriving elevator takes its first turn and starts back to where it rests.</summary>
    public void BeginReturn() => Status = Stage.Returning;

    /// <summary>The ride is over, and the door disarmed.</summary>
    public void End() => (Status, DoorArmed, _setDownAt) = (Stage.None, false, null);

    /// <inheritdoc/>
    public void BeginAgain() => (Status, GoesDown, DoorArmed, _stood, _setDownAt) = (Stage.None, false, false, false, null);

    /// <inheritdoc/>
    public void Step(GameSession session)
    {
    }

    /// <inheritdoc/>
    public bool WantsToTakeOver => false;

    /// <inheritdoc/>
    public GameMode? TakesOver(PlayingMode playing) => null;
}
