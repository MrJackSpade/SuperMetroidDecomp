namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>How a picture's name spells the values of the appearance inputs that do not name a collectible or a frame (#1466).</summary>
public static class RoomAppearanceValues
{
    /// <summary>
    /// An input that leaves what the room loaded showing: a collectible slot no collectible shows in, Kraid's backdrop not
    /// yet shown, an animation that has selected no frame, an encounter look not shown.
    /// </summary>
    public const string Loaded = "-";

    /// <summary>An input that shows its artwork: Kraid's backdrop or an encounter's look.</summary>
    public const string Shown = "shown";
}
