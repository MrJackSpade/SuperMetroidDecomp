using Engine.Content;

namespace Engine.Input;

/// <summary>
/// A recorded run of inputs, such as a title-screen demo: the room it plays in, where the player starts, and the
/// actions held for each stretch of updates.
/// </summary>
/// <remarks>
/// <code>
/// { "room": "res://game/rooms/demo.tscn", "entry_point": "Start",
///   "inputs": [ { "held": ["right"], "updates": 60 }, { "held": ["right", "jump"], "updates": 20 }, { "held": [], "updates": 30 } ] }
/// </code>
/// </remarks>
public sealed class DemoRecording
{
    /// <summary>The scene of the room the demo plays in.</summary>
    public string Room { get; private init; } = "";

    /// <summary>The entry point in that room where the player starts.</summary>
    public string EntryPoint { get; private init; } = "";

    /// <summary>The actions held in each update of the demo, in order.</summary>
    public IReadOnlyList<IReadOnlyList<GameAction>> Updates { get; private init; } = [];

    /// <summary>Reads a demo from its data file.</summary>
    public static DemoRecording Read(DataNode data)
    {
        var updates = new List<IReadOnlyList<GameAction>>();
        foreach (DataNode stretch in data.List("inputs"))
        {
            IReadOnlyList<GameAction> held = stretch.List("held").Select(action => new GameAction(action.AsText())).ToList();
            int count = stretch.Int("updates");
            if (count <= 0)
                throw stretch.Get("updates").Problem("must be a positive number of updates");
            for (int update = 0; update < count; update++)
                updates.Add(held);
        }

        return new DemoRecording { Room = data.Text("room"), EntryPoint = data.Text("entry_point"), Updates = updates };
    }
}
