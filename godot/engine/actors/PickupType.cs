using Engine.Content;

namespace Engine.Actors;

/// <summary>A kind of drop: which inventory amount it adds to, how much, how long it lies there, and how it looks.</summary>
/// <remarks><code>{ "name": "small energy", "adds_to": "energy", "quantity": 5, "lasts_updates": 400, "size": 8, "picture": "pickups/energy.png" }</code></remarks>
public sealed class PickupType
{
    /// <summary>The drop's name.</summary>
    public string Name { get; private init; } = "";

    /// <summary>The inventory amount it adds to, by name.</summary>
    public string AddsTo { get; private init; } = "";

    /// <summary>How much it adds.</summary>
    public int Quantity { get; private init; }

    /// <summary>How many updates it lies there before it is gone.</summary>
    public int LastsUpdates { get; private init; }

    /// <summary>Its width and height, in pixels.</summary>
    public int Size { get; private init; }

    /// <summary>Its picture, as a path within the assets tree; empty for none.</summary>
    public string Picture { get; private init; } = "";

    /// <summary>Reads a pickup type from its data file.</summary>
    public static PickupType Read(DataNode data)
    {
        var type = new PickupType
        {
            Name = data.Text("name"),
            AddsTo = data.Text("adds_to"),
            Quantity = data.Int("quantity"),
            LastsUpdates = data.Int("lasts_updates"),
            Size = data.Has("size") ? data.Int("size") : 8,
            Picture = data.Has("picture") ? data.Text("picture") : "",
        };
        if (type.Quantity <= 0 || type.LastsUpdates <= 0 || type.Size <= 0)
            throw data.Problem("gives a quantity, a lifetime or a size that is not positive");
        return type;
    }
}
