using Godot;

namespace Engine.World;

/// <summary>
/// A scripted sequence belonging to one room. It owns its own timeline, and at the right moments tells the room's
/// objects, terrain, effects and camera what to do through their own methods. It holds no behaviour that belongs to
/// something in the room. It starts when it is told to "start", or by itself as the room is set up if its condition
/// holds or the room was entered at the entry point it names.
/// </summary>
public abstract partial class RoomSequence : Node, IPlaced, ITriggerTarget, IRemembered
{
    /// <inheritdoc/>
    public bool MustBeRemembered => false;

    /// <summary>The condition under which the sequence starts by itself as the room is set up; null when it waits to be told.</summary>
    [Export]
    public Condition? StartsWhen { get; set; }

    /// <summary>The entry point whose use starts the sequence as the room is set up; empty for none.</summary>
    [Export]
    public string StartsAtEntry { get; set; } = "";

    /// <summary>The fact recorded when the sequence finishes; empty for none.</summary>
    [Export]
    public string RememberedAs { get; set; } = "";

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>Whether the sequence is playing.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>How many updates the sequence has played since it started.</summary>
    public int Elapsed { get; private set; }

    /// <summary>
    /// What of the room waits while the sequence is playing: nothing, unless the sequence says otherwise, as one that
    /// holds the player still while it moves them does. Play asks as each update begins.
    /// </summary>
    public virtual Stopped Stops => Stopped.Nothing;

    /// <summary>
    /// Whether the sequence plays before the view follows the player, and not after: one that moves the player so
    /// that the view follows the move on the same update says so.
    /// </summary>
    public virtual bool PlaysBeforeView => false;

    private Room? _room;

    /// <summary>The room the sequence is of, from the room's setting up on.</summary>
    protected Room Room => _room ?? throw new InvalidOperationException($"The sequence \"{Name}\" is in no room that has been set up.");

    /// <summary>Puts the sequence at its start, and starts it if its condition says so.</summary>
    internal void Setup(Room room)
    {
        _room = room;
        IsRunning = false;
        Elapsed = 0;
        if ((StartsWhen is not null && StartsWhen.IsTrue(room)) || (StartsAtEntry.Length > 0 && room.Entry?.EntryPoint == StartsAtEntry))
            Start();
    }

    /// <summary>Starts the sequence from the beginning. Starting one that is playing changes nothing.</summary>
    public void Start()
    {
        if (IsRunning)
            return;
        IsRunning = true;
        Elapsed = 0;
        OnStarted();
    }

    /// <summary>
    /// Called as the sequence starts, before its first update is played: as its room is set up, for one that starts
    /// by itself, with the player already in the room. What must hold from the first moment is done here.
    /// </summary>
    protected virtual void OnStarted()
    {
    }

    /// <summary>Plays the sequence's part of the opening of an update, if it is running and has one.</summary>
    internal void OpenUpdate(Room room)
    {
        if (IsRunning && this is IOpeningStep opening)
            opening.StepOpening(room);
    }

    /// <summary>Plays one update of the sequence if it is running.</summary>
    internal void Step(Room room)
    {
        if (!IsRunning)
            return;
        bool finished = Play(room, Elapsed);
        Elapsed++;
        if (!finished)
            return;
        IsRunning = false;
        if (RememberedAs.Length > 0)
            room.Session.Progress.Record(RememberedAs);
    }

    /// <summary>
    /// Plays the update that comes <paramref name="elapsed"/> updates after the sequence started, and returns whether
    /// that was its last.
    /// </summary>
    protected abstract bool Play(Room room, int elapsed);

    /// <summary>Plays the sequence's part of an update on which the room is held still, if it is running.</summary>
    internal void StepHeld(Room room)
    {
        if (IsRunning)
            WhileRoomHeld(room);
    }

    /// <summary>
    /// Called in place of an update of the sequence while the room is held still, as under a player's state that
    /// stops it: the sequence does not move on, and nothing happens. A sequence with something that goes on all the
    /// same, a count kept through held time say, plays that here.
    /// </summary>
    protected virtual void WhileRoomHeld(Room room)
    {
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what == "start")
            Start();
        else
            OnTold(what);
    }

    /// <summary>Called when the sequence is told anything but "start". A sequence that knows other words acts on them here.</summary>
    protected virtual void OnTold(string what) =>
        throw new ArgumentException($"The room sequence \"{Name}\" can be told to \"start\", not \"{what}\".", nameof(what));
}
