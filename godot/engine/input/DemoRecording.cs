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
/// An action counts as newly pressed on the first update it is held after one it was not. A stretch whose recording
/// says otherwise gives "pressed", the actions newly pressed on its first update: one pressed again without having
/// been let go, or one held that was never pressed. A recording that gives "ends": "holding" goes on holding its last
/// stretch for good once it has been played, and is never over.
/// </remarks>
public sealed class DemoRecording
{
    /// <summary>The scene of the room the demo plays in.</summary>
    public string Room { get; private init; } = "";

    /// <summary>The entry point in that room where the player starts.</summary>
    public string EntryPoint { get; private init; } = "";

    /// <summary>The actions held in each update of the demo, in order.</summary>
    public IReadOnlyList<IReadOnlyList<GameAction>> Updates { get; private init; } = [];

    /// <summary>
    /// The actions newly pressed in each update of the demo, where the recording says; null for an update whose
    /// presses are those of its actions that were not held the update before.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<GameAction>?> Pressed { get; private init; } = [];

    /// <summary>Whether the demo goes on holding its last update's actions once it has been played.</summary>
    public bool HoldsLast { get; private init; }

    /// <summary>Reads a demo from its data file.</summary>
    public static DemoRecording Read(DataNode data)
    {
        var updates = new List<IReadOnlyList<GameAction>>();
        var pressed = new List<IReadOnlyList<GameAction>?>();
        foreach (DataNode stretch in data.List("inputs"))
        {
            IReadOnlyList<GameAction> held = stretch.List("held").Select(action => new GameAction(action.AsText())).ToList();
            IReadOnlyList<GameAction>? first = stretch.Has("pressed")
                ? stretch.List("pressed").Select(action => new GameAction(action.AsText())).ToList()
                : null;
            int count = stretch.Int("updates");
            if (count <= 0)
                throw stretch.Get("updates").Problem("must be a positive number of updates");
            for (int update = 0; update < count; update++)
            {
                updates.Add(held);
                // What a stretch says is pressed is pressed as it begins; nothing is pressed while it goes on.
                pressed.Add(first is null ? null : update == 0 ? first : []);
            }
        }

        bool holdsLast = false;
        if (data.Has("ends"))
        {
            holdsLast = data.Text("ends") switch
            {
                "holding" => true,
                "stopping" => false,
                _ => throw data.Get("ends").Problem("must be \"holding\" or \"stopping\""),
            };
        }

        if (holdsLast && updates.Count == 0)
            throw data.Problem("ends holding its last stretch, and has none");
        return new DemoRecording
        {
            Room = data.Text("room"), EntryPoint = data.Text("entry_point"), Updates = updates, Pressed = pressed, HoldsLast = holdsLast,
        };
    }
}
