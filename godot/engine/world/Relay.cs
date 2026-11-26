using Godot;

namespace Engine.World;

/// <summary>
/// Something placed in a room that turns one word into what the room does about it. Things that only know that
/// something has happened to them (a boss that has woken, a wall that has opened) tell a relay a word, and its rules
/// say what follows: which camera zone opens, which patch of terrain changes, what the music becomes. It holds no
/// behaviour of its own, and a word none of its rules is for is a mistake.
/// </summary>
public partial class Relay : Node, ITriggerTarget, IPlaced
{
    /// <summary>What it does on hearing each word.</summary>
    [Export]
    public Godot.Collections.Array<RelayRule> Rules { get; set; } = [];

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        Room room = FindRoom();
        bool heard = false;
        foreach (RelayRule rule in Rules)
        {
            if (rule.Hears != what)
                continue;
            heard = true;
            if (!rule.Target.IsEmpty)
            {
                (GetNode(rule.Target) as ITriggerTarget
                    ?? throw new InvalidOperationException($"The relay \"{Name}\" is to tell \"{rule.Target}\" \"{rule.Tells}\", but that is nothing that can be told."))
                    .Triggered(rule.Tells);
            }

            if (rule.AppliesPatch.Length > 0)
                room.Terrain.ApplyPatch(rule.AppliesPatch);
            if (rule.RevertsPatch.Length > 0)
                room.Terrain.RevertPatch(rule.RevertsPatch);
            if (rule.ChangesMusic)
                room.Session.Sounds.RequestMusic(rule.Music);
            if (rule.ShakeStrength > 0)
                room.Shake(rule.ShakeStrength, rule.ShakeUpdates);
            if (rule.Records.Length > 0)
                room.Session.Progress.Record(rule.Records);
        }

        if (!heard)
            throw new ArgumentException($"The relay \"{Name}\" has no rule for the word \"{what}\".", nameof(what));
    }

    private Room FindRoom()
    {
        for (Node? node = GetParent(); node is not null; node = node.GetParent())
        {
            if (node is Room room)
                return room;
        }

        throw new InvalidOperationException($"The relay \"{Name}\" is not in a room.");
    }
}
