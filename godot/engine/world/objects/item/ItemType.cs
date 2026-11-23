using Engine.Content;

namespace Engine.World.Objects;

/// <summary>A permanent upgrade: what it grants, the message shown for it, and its picture.</summary>
/// <remarks>
/// <code>
/// { "name": "missile tank", "grants_equipment": "", "expands": "missiles", "expands_by": 5,
///   "message": "missile_tank", "picture": "items/missile_tank.png" }
/// </code>
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

    /// <summary>The name of the message shown when it is collected; empty for none.</summary>
    public string Message { get; private init; } = "";

    /// <summary>Its picture, as a path within the assets tree; empty for none.</summary>
    public string Picture { get; private init; } = "";

    /// <summary>Reads an item type from its data file.</summary>
    public static ItemType Read(DataNode data)
    {
        var type = new ItemType
        {
            Name = data.Text("name"),
            GrantsEquipment = data.Has("grants_equipment") ? data.Text("grants_equipment") : "",
            Expands = data.Has("expands") ? data.Text("expands") : "",
            ExpandsBy = data.Has("expands_by") ? data.Int("expands_by") : 0,
            Message = data.Has("message") ? data.Text("message") : "",
            Picture = data.Has("picture") ? data.Text("picture") : "",
        };
        if (type.GrantsEquipment.Length == 0 && type.Expands.Length == 0)
            throw data.Problem("is an item that grants nothing");
        if (type.ExpandsBy < 0)
            throw data.Get("expands_by").Problem("is negative");
        return type;
    }
}
