using Engine.Content;

namespace Engine.World.Objects;

/// <summary>A permanent upgrade: what it grants, the message shown for it, and its picture.</summary>
/// <remarks>
/// <code>
/// { "name": "missile tank", "grants_equipment": "", "expands": "missiles", "expands_by": 5,
///   "message": "missile_tank", "picture": "items/missile_tank.png" }
/// </code>
/// With the capacity it raises it gives as much more of the amount, unless "adds" says how much it gives instead or
/// "fills" has it fill the amount to its new capacity. "switches_off" lists equipment switched off as it is
/// collected: what cannot be used together with what it grants.
/// </remarks>
public sealed class ItemType
{
    /// <summary>The item's name.</summary>
    public string Name { get; private init; } = "";

    /// <summary>The equipment it gives, by name; empty when it gives none.</summary>
    public string GrantsEquipment { get; private init; } = "";

    /// <summary>The inventory amount whose capacity it raises, by name; empty when it raises none.</summary>
    public string Expands { get; private init; } = "";

    /// <summary>How much it raises that capacity by.</summary>
    public int ExpandsBy { get; private init; }

    /// <summary>How much of that amount it gives along with the capacity.</summary>
    public int Adds { get; private init; }

    /// <summary>Whether it fills that amount to its new capacity.</summary>
    public bool Fills { get; private init; }

    /// <summary>The equipment switched off as it is collected, by name.</summary>
    public IReadOnlyList<string> SwitchesOff { get; private init; } = [];

    /// <summary>The name of the message shown when it is collected; empty for none.</summary>
    public string Message { get; private init; } = "";

    /// <summary>Its picture, as a path within the assets tree; empty for none.</summary>
    public string Picture { get; private init; } = "";

    /// <summary>The data file of the timeline that shows it, in place of its one picture; empty for none.</summary>
    public string Timeline { get; private init; } = "";

    /// <summary>The music played while its message shows, by name; empty for none.</summary>
    public string Fanfare { get; private init; } = "";

    /// <summary>How many updates its message stays up before it can be dismissed.</summary>
    public int MessageHoldUpdates { get; private init; }

    /// <summary>The sound of it being collected, by name; empty for none.</summary>
    public string Sound { get; private init; } = "";

    /// <summary>Reads an item type from its data file.</summary>
    public static ItemType Read(DataNode data)
    {
        var type = new ItemType
        {
            Name = data.Text("name"),
            GrantsEquipment = data.Has("grants_equipment") ? data.Text("grants_equipment") : "",
            Expands = data.Has("expands") ? data.Text("expands") : "",
            ExpandsBy = data.Has("expands_by") ? data.Int("expands_by") : 0,
            Adds = data.Has("adds") ? data.Int("adds") : data.Has("expands_by") ? data.Int("expands_by") : 0,
            Fills = data.Has("fills") && data.Flag("fills"),
            SwitchesOff = data.Has("switches_off") ? [.. data.List("switches_off").Select(equipment => equipment.AsText())] : [],
            Message = data.Has("message") ? data.Text("message") : "",
            Picture = data.Has("picture") ? data.Text("picture") : "",
            Timeline = data.Has("timeline") ? data.Text("timeline") : "",
            Fanfare = data.Has("fanfare") ? data.Text("fanfare") : "",
            MessageHoldUpdates = data.Has("message_hold_updates") ? data.Int("message_hold_updates") : 0,
            Sound = data.Has("sound") ? data.Text("sound") : "",
        };
        if (type.GrantsEquipment.Length == 0 && type.Expands.Length == 0)
            throw data.Problem("is an item that grants nothing");
        if (type.ExpandsBy < 0)
            throw data.Get("expands_by").Problem("is negative");
        if (type.Adds < 0 || type.Adds > type.ExpandsBy)
            throw data.Problem("is an item that gives less than nothing, or more than the capacity it raises");
        return type;
    }
}
