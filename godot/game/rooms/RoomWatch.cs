using Engine.World;
using Godot;

namespace Game.Rooms;

/// <summary>
/// A watch kept on a room: once what it waits for is true, it records the facts it names and is done. The Metroid
/// rooms of Tourian are remembered as cleared this way, and Zebes wakes when a room's guard has been killed.
/// </summary>
public partial class RoomWatch : RoomSequence
{
    /// <summary>What it waits for.</summary>
    [Export]
    public Condition? Until { get; set; }

    /// <summary>The facts it records, by name.</summary>
    [Export]
    public string[] Records { get; set; } = [];

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        Condition until = Until ?? throw new InvalidOperationException($"The room watch \"{Name}\" waits for nothing.");
        if (!until.IsTrue(room))
            return false;
        foreach (string fact in Records)
            room.Session.Progress.Record(fact);
        return true;
    }
}
