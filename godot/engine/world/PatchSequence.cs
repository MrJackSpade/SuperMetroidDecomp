using Engine.Content;

namespace Engine.World;

/// <summary>
/// A change to a room's terrain that plays out: named patches of the room's layout applied one after another, each
/// standing for its own number of updates before the next, the last one staying. A wall that crumbles through several
/// pictures before it is gone is one of these. Whoever plays it steps it once an update for as long as it runs.
/// </summary>
/// <remarks>
/// <code>
/// { "delay": 1, "steps": [ { "patch": "wall_cracked", "updates": 4, "sound": "crumble" }, { "patch": "wall_gone" } ] }
/// </code>
/// "delay" is how many updates pass before the first patch; a step's "sound" is asked for as its patch is applied;
/// the last step needs no "updates".
/// </remarks>
public sealed class PatchSequence
{
    private readonly IReadOnlyList<(string Patch, int Updates, string Sound)> _steps;
    private readonly int _delay;
    private int _next;
    private int _wait;

    private PatchSequence(IReadOnlyList<(string Patch, int Updates, string Sound)> steps, int delay)
    {
        _steps = steps;
        _delay = delay;
        _wait = delay;
    }

    /// <summary>The same change, ready to be played from its start: for one played more than once, or kept until it is wanted.</summary>
    public PatchSequence FromStart() => new(_steps, _delay);

    /// <summary>Whether every patch has been applied.</summary>
    public bool IsOver => _next >= _steps.Count;

    /// <summary>Reads a sequence from a data file's group, ready to be played from its start.</summary>
    /// <exception cref="ContentException">It has no steps, or a step that is not the last stands for no updates.</exception>
    public static PatchSequence Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        IReadOnlyList<DataNode> listed = data.List("steps");
        if (listed.Count == 0)
            throw data.Problem("is a change to the terrain with no steps");
        var steps = new List<(string, int, string)>();
        for (int index = 0; index < listed.Count; index++)
        {
            DataNode step = listed[index];
            int updates = step.Has("updates") ? step.Int("updates") : 0;
            if (updates <= 0 && index < listed.Count - 1)
                throw step.Problem("stands for no updates, and is not the last step");
            steps.Add((step.Text("patch"), updates, step.Has("sound") ? step.Text("sound") : ""));
        }

        int delay = data.Has("delay") ? data.Int("delay") : 0;
        if (delay < 0)
            throw data.Problem("begins before it is played");
        return new PatchSequence(steps, delay);
    }

    /// <summary>Plays one update: applies the next patch when the one before it has stood its time.</summary>
    public void Step(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (IsOver)
            return;
        if (_wait > 0 && --_wait > 0)
            return;
        (string patch, int updates, string sound) = _steps[_next++];
        room.Terrain.ApplyPatch(patch);
        if (sound.Length > 0)
            room.Session.Sounds.Request(sound);
        _wait = updates;
    }
}
