using Godot;

namespace Engine.World;

/// <summary>
/// A scripted sequence belonging to one room. It owns its own timeline, and at the right moments tells the room's
/// objects, terrain, effects and camera what to do through their own methods. It holds no behaviour that belongs to
/// something in the room. It can be told to "start".
/// </summary>
public abstract partial class RoomSequence : Node, IPlaced, ITriggerTarget, IRemembered
{
    /// <inheritdoc/>
    public bool MustBeRemembered => false;

    /// <summary>The condition under which the sequence starts by itself as the room is set up; null when it waits to be told.</summary>
    [Export]
    public Condition? StartsWhen { get; set; }

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

    /// <summary>Puts the sequence at its start, and starts it if its condition says so.</summary>
    internal void Setup(Room room)
    {
        IsRunning = false;
        Elapsed = 0;
        if (StartsWhen is not null && StartsWhen.IsTrue(room))
            Start();
    }

    /// <summary>Starts the sequence from the beginning. Starting one that is playing changes nothing.</summary>
    public void Start()
    {
        if (IsRunning)
            return;
        IsRunning = true;
        Elapsed = 0;
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

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what != "start")
            throw new ArgumentException($"A room sequence can be told to \"start\", not \"{what}\".", nameof(what));
        Start();
    }
}
