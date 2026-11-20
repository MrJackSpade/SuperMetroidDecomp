namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How a collectible shows its two looks (#2554): a tank by its kind's own blocks, or an item that shows its own picture by
/// the terrain block pair it took (<see cref="CollectibleBlockPairs"/>).
/// </summary>
internal abstract record CollectiblePictured
{
    /// <summary>Only the ways below are choices.</summary>
    private CollectiblePictured()
    {
    }

    /// <summary>A tank, whose looks are its kind's own blocks.</summary>
    internal sealed record Tank : CollectiblePictured;

    /// <summary>An item that shows its own picture through the block pair <paramref name="Pair"/> it took.</summary>
    /// <param name="Pair">The terrain block pair it writes its frames into.</param>
    internal sealed record OwnPicture(CollectibleBlockPairs.Pair Pair) : CollectiblePictured;
}
