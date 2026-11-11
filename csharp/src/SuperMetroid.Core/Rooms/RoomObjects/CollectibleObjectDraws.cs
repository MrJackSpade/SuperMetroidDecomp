using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectCollectibleDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each collectible draw's block collides, the terrain a block pair's frame writes, and the frame selection. What a
/// frame other than a block pair's shows is its installed picture (#1557).
/// </summary>
internal static class CollectibleObjectDraws
{
    /// <summary>How the block <paramref name="request"/> writes collides, when it is a collectible draw.</summary>
    internal static bool TryGetCollision(RoomObjectDraw request, out RoomCollisionType collision)
    {
        bool found = TryLocate(request, out Draw draw);
        collision = found ? Collision(draw.Group) : default;
        return found;
    }

    /// <summary>
    /// The terrain a block pair's frame writes, when <paramref name="request"/> is one: the frame lies under the item's own
    /// picture (#1538), has no installed picture of its own, and writes its block number, which is gameplay terrain
    /// (<see cref="RoomObjectCollectibleDrawDefinitions.OwnPictureFirstBlock"/>).
    /// </summary>
    internal static bool TryGetBlockPairTerrain(RoomObjectDraw request, out RoomTerrainBlock terrain)
    {
        bool found = TryLocate(request, out Draw draw) && draw.Group == DrawGroup.OwnPicture;
        terrain = found ? RoomTerrainBlock.Of(OwnPictureFirstBlock + draw.Index, BlockMirroring.None, Collision(DrawGroup.OwnPicture)) : default;
        return found;
    }

    /// <summary>The number of frames in a group.</summary>
    internal static int CountOf(DrawGroup group) => group switch
    {
        DrawGroup.Empty => 1,
        DrawGroup.Orb => 3,
        DrawGroup.OrbBurst => 1,
        DrawGroup.Tank => 8,
        DrawGroup.OwnPicture => CollectibleBlockPairs.InTurn.Count * 2,
        DrawGroup.ShotReveal => 3,
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {group}."),
    };

    /// <summary>Accepts a collectible draw whose frame lies within its group.</summary>
    private static bool TryLocate(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw frame) => (uint)frame.Index < (uint)CountOf(frame.Group), out draw);

    /// <summary>How the block a draw of <paramref name="group"/> writes collides.</summary>
    private static RoomCollisionType Collision(DrawGroup group) => group switch
    {
        DrawGroup.Empty => RoomCollisionType.Air,
        DrawGroup.Orb => RoomCollisionType.ShootableBlock,
        DrawGroup.OrbBurst => RoomCollisionType.SolidBlock,
        DrawGroup.Tank => RoomCollisionType.SpecialBlock,
        DrawGroup.OwnPicture => RoomCollisionType.SpecialBlock,
        DrawGroup.ShotReveal => RoomCollisionType.SolidBlock,
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {group}."),
    };

    /// <summary>The orb cycles forward through its three draws and back through the middle draw.</summary>
    internal static RoomObjectFrameDraw<Draw> OrbFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 4)
            throw new InvalidDataException(
                $"Chozo orb animation index {animationIndex} is outside four authored phases.");
        return RoomObjectDraw.Of(new Draw(DrawGroup.Orb, 2 - Math.Abs(animationIndex - 2)));
    }

    /// <summary>A shot-block reveal shows three consecutive draws; hiding consumes them in reverse.</summary>
    internal static RoomObjectFrameDraw<Draw> ShotRevealFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 3)
            throw new InvalidDataException(
                $"Shot-block reveal index {animationIndex} is outside three authored frames.");
        return RoomObjectDraw.Of(new Draw(DrawGroup.ShotReveal, animationIndex));
    }

    /// <summary>
    /// The visible frame <paramref name="animationIndex"/> of a tank kind, or of the block pair an item that shows its own
    /// picture took (<see cref="CollectibleBlockPairs"/>): two draws each.
    /// </summary>
    internal static RoomObjectFrameDraw<Draw> VisibleFrame(InWorldCollectibleKind kind,
        int animationIndex, CollectibleBlockPairs.Pair? blockPair)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        int tank = Array.IndexOf(TankKinds, kind);
        if (tank >= 0)
            return RoomObjectDraw.Of(new Draw(DrawGroup.Tank, tank * 2 + animationIndex));
        if (blockPair is not { } pair)
            throw new InvalidDataException($"Collectible {kind} shows its own picture but took no block pair.");
        return RoomObjectDraw.Of(new Draw(DrawGroup.OwnPicture, FirstOwnPictureFrame(pair) + animationIndex));
    }

    /// <summary>
    /// The first of the two frames under the item's own picture that <paramref name="pair"/>'s collectible writes: each
    /// pair's two frames follow the previous pair's, in the turn the room's collectibles take the pairs in.
    /// </summary>
    private static int FirstOwnPictureFrame(CollectibleBlockPairs.Pair pair) => pair switch
    {
        CollectibleBlockPairs.Pair.First => 0,
        CollectibleBlockPairs.Pair.Second => 2,
        CollectibleBlockPairs.Pair.Third => 4,
        CollectibleBlockPairs.Pair.Fourth => 6,
        _ => throw new InvalidDataException($"Undefined collectible block pair {pair}."),
    };
}
