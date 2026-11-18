using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One image of a collectible's animations (<see cref="CollectibleSequenceAnimations"/>): a draw at the item's block, or
/// one of the item's own two visible looks (#2480), which the item's kind and block pair draw.
/// </summary>
/// <param name="Draw">The draw; null for a look.</param>
/// <param name="Look">The look; null for a draw.</param>
internal readonly record struct CollectibleImage(RoomObjectDraw? Draw, CollectibleLook? Look)
{
    /// <summary>The image that draws <paramref name="draw"/>.</summary>
    internal static CollectibleImage Drawn(RoomObjectDraw draw) => new(Ensure.NotNull(draw), null);

    /// <summary>The image of the item's own <paramref name="look"/>.</summary>
    internal static CollectibleImage Visible(CollectibleLook look) => new(null, look);
}
