using Engine.Content;

namespace Engine.World;

/// <summary>
/// A change to a room's terrain that plays out: named patches of the room's layout applied one after another, each
/// standing for its own number of updates before the next, the last one staying. A wall that crumbles through several
/// pictures before it is gone is one of these. A room plays it (<see cref="Room.Play"/>), stepping it once an update
/// until it is over. A step may take a patch back, with or without applying one: a change set off in one part of an
/// update that is to show at the terrain's own turn, a wall put up or taken down then, is a sequence of one such
/// step.
/// </summary>
/// <remarks>
/// <code>
/// { "delay": 1, "steps": [ { "patch": "wall_cracked", "updates": 4, "sound": "crumble" }, { "patch": "wall_gone" } ] }
/// { "steps": [ { "reverts": "door_held", "patch": "door_walled" } ] }
/// </code>
/// "delay" is how many updates pass before the first step; a step names a "patch" to apply, a patch it "reverts",
/// which is taken back first, or both; its "sound" is asked for as it is played; the last step needs no "updates".
/// </remarks>
public sealed class PatchSequence
{
    private readonly IReadOnlyList<(string Patch, string Reverts, int Updates, string Sound)> _steps;
    private readonly int _delay;
    private int _next;
    private int _wait;

    private PatchSequence(IReadOnlyList<(string Patch, string Reverts, int Updates, string Sound)> steps, int delay)
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
    /// <exception cref="ContentException">It has no steps, a step that is not the last stands for no updates, or a step names no patch.</exception>
    public static PatchSequence Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        IReadOnlyList<DataNode> listed = data.List("steps");
        if (listed.Count == 0)
            throw data.Problem("is a change to the terrain with no steps");
        var steps = new List<(string, string, int, string)>();
        for (int index = 0; index < listed.Count; index++)
        {
            DataNode step = listed[index];
            int updates = step.Has("updates") ? step.Int("updates") : 0;
            if (updates <= 0 && index < listed.Count - 1)
                throw step.Problem("stands for no updates, and is not the last step");
            string patch = step.Has("patch") ? step.Text("patch") : "", reverts = step.Has("reverts") ? step.Text("reverts") : "";
            if (patch.Length == 0 && reverts.Length == 0)
                throw step.Problem("names no patch to apply and none to take back");
            steps.Add((patch, reverts, updates, step.Has("sound") ? step.Text("sound") : ""));
        }

        int delay = data.Has("delay") ? data.Int("delay") : 0;
        if (delay < 0)
            throw data.Problem("begins before it is played");
        return new PatchSequence(steps, delay);
    }

    /// <summary>Plays one update: plays the next step when the one before it has stood its time, taking back the patch it reverts and then applying its own.</summary>
    internal void Step(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (IsOver)
            return;
        if (_wait > 0 && --_wait > 0)
            return;
        (string patch, string reverts, int updates, string sound) = _steps[_next++];
        if (reverts.Length > 0)
            room.Terrain.RevertPatch(reverts);
        if (patch.Length > 0)
            room.Terrain.ApplyPatch(patch);
        if (sound.Length > 0)
            room.Session.Sounds.Request(sound);
        _wait = updates;
    }
}
