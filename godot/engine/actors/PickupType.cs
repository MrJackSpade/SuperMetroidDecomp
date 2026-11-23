using Engine.Content;

namespace Engine.Actors;

/// <summary>A kind of drop: which inventory amount it adds to, how much, how long it lies there, and how it looks.</summary>
/// <remarks>
/// <code>
/// { "name": "small energy", "adds_to": "energy", "then_to": "reserve_energy", "quantity": 5, "lasts_updates": 400,
///   "size": 8, "picture": "pickups/energy.png", "timeline": "pickups/energy.json", "sound": "pickup_collected" }
/// </code>
/// </remarks>
public sealed class PickupType
{
    /// <summary>The drop's name.</summary>
    public string Name { get; private init; } = "";

    /// <summary>The inventory amount it adds to, by name.</summary>
    public string AddsTo { get; private init; } = "";

    /// <summary>The inventory amount that takes what the first cannot hold, by name; empty when the rest is lost.</summary>
    public string ThenTo { get; private init; } = "";

    /// <summary>How much it adds.</summary>
    public int Quantity { get; private init; }

    /// <summary>How many updates it lies there before it is gone.</summary>
    public int LastsUpdates { get; private init; }

    /// <summary>Its width and height, in pixels.</summary>
    public int Size { get; private init; }

    /// <summary>Its picture, as a path within the assets tree; empty for none.</summary>
    public string Picture { get; private init; } = "";

    /// <summary>
    /// The data file of the timeline it is shown by, when it is animated; empty for one shown by its picture alone.
    /// A lone picture is centred on the pickup; a timeline's pictures lie where the timeline's offset puts them.
    /// </summary>
    public string Timeline { get; private init; } = "";

    /// <summary>The sound asked for as it is collected, by name; empty for none.</summary>
    public string Sound { get; private init; } = "";

    /// <summary>Whether a player carrying <paramref name="inventory"/> has a use for it: room for some of what it gives.</summary>
    public bool IsUsefulTo(Inventory inventory) =>
        !inventory.IsFull(AddsTo) || (ThenTo.Length > 0 && !inventory.IsFull(ThenTo));

    /// <summary>Gives the pickup to a player carrying <paramref name="inventory"/>.</summary>
    public void GiveTo(Inventory inventory)
    {
        int added = inventory.Add(AddsTo, Quantity);
        if (ThenTo.Length > 0 && added < Quantity)
            inventory.Add(ThenTo, Quantity - added);
    }

    /// <summary>Reads a pickup type from its data file.</summary>
    public static PickupType Read(DataNode data)
    {
        var type = new PickupType
        {
            Name = data.Text("name"),
            AddsTo = data.Text("adds_to"),
            ThenTo = data.Has("then_to") ? data.Text("then_to") : "",
            Quantity = data.Int("quantity"),
            LastsUpdates = data.Int("lasts_updates"),
            Size = data.Has("size") ? data.Int("size") : 8,
            Picture = data.Has("picture") ? data.Text("picture") : "",
            Timeline = data.Has("timeline") ? data.Text("timeline") : "",
            Sound = data.Has("sound") ? data.Text("sound") : "",
        };
        if (type.Quantity <= 0 || type.LastsUpdates <= 0 || type.Size <= 0)
            throw data.Problem("gives a quantity, a lifetime or a size that is not positive");
        return type;
    }
}
