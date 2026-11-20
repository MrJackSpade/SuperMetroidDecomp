using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The terrain an item lying in the room shows its looks in (#2558): a tank's own blocks, which its kind pictures, or the
/// block pair an item that shows its own picture took (<see cref="CollectibleBlockPairs"/>), over which its picture shows.
/// </summary>
internal abstract record CollectibleTerrain
{
    /// <summary>Only the two terrains below exist.</summary>
    private CollectibleTerrain()
    {
    }

    /// <summary>A tank's own blocks, which its kind pictures.</summary>
    internal static CollectibleTerrain TankBlocks { get; } = new TankTerrain();

    /// <summary>The block pair <paramref name="pair"/> an item that shows its own picture took.</summary>
    /// <param name="pair">The pair the item took.</param>
    internal static CollectibleTerrain OwnPicture(CollectibleBlockPairs.Pair pair) => new OwnPictureTerrain(pair);

    /// <summary>Whether the item shows its own picture over the terrain it writes.</summary>
    internal abstract bool ShowsOwnPicture { get; }

    /// <summary>The draw of <paramref name="look"/> of an item of <paramref name="kind"/> lying in this terrain.</summary>
    /// <param name="kind">The item's kind.</param>
    /// <param name="look">The look shown.</param>
    internal abstract RoomObjectFrameDraw<CollectibleDraw> VisibleFrame(InWorldCollectibleKind kind, CollectibleLook look);

    /// <summary>A tank's own blocks.</summary>
    private sealed record TankTerrain : CollectibleTerrain
    {
        /// <inheritdoc/>
        internal override bool ShowsOwnPicture => false;

        /// <inheritdoc/>
        internal override RoomObjectFrameDraw<CollectibleDraw> VisibleFrame(InWorldCollectibleKind kind, CollectibleLook look) =>
            CollectibleObjectDraws.TankFrame(kind, look);
    }

    /// <summary>The block pair an item that shows its own picture took.</summary>
    /// <param name="Pair">The pair the item took.</param>
    private sealed record OwnPictureTerrain(CollectibleBlockPairs.Pair Pair) : CollectibleTerrain
    {
        /// <inheritdoc/>
        internal override bool ShowsOwnPicture => true;

        /// <inheritdoc/>
        internal override RoomObjectFrameDraw<CollectibleDraw> VisibleFrame(InWorldCollectibleKind kind, CollectibleLook look) =>
            CollectibleObjectDraws.BlockPairFrame(Pair, look);
    }
}
