using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectCollectibleDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each collectible draw's block collides, the terrain a block pair's frame writes, and the frame selection; the Chozo
/// orb's frames are its animation's (<see cref="CollectibleOrbAnimation"/>). What a frame other than a block pair's shows
/// is its installed picture (#1557).
/// </summary>
internal static class CollectibleObjectDraws
{
    /// <summary>How the block <paramref name="request"/> writes collides, when it is a collectible draw.</summary>
    internal static bool TryGetCollision(RoomObjectDraw request, out RoomCollisionType collision)
    {
        bool found = TryLocate(request, out CollectibleDraw draw);
        collision = found ? Collision(GroupOf(draw)) : default;
        return found;
    }

    /// <summary>
    /// The terrain a block pair's frame writes, when <paramref name="request"/> is one: the frame lies under the item's own
    /// picture (#1538), has no installed picture of its own, and writes its block number, which is gameplay terrain
    /// (<see cref="RoomObjectCollectibleDrawDefinitions.OwnPictureFirstBlock"/>).
    /// </summary>
    internal static bool TryGetBlockPairTerrain(RoomObjectDraw request, out RoomTerrainBlock terrain)
    {
        int block = 0;
        bool found = TryLocate(request, out CollectibleDraw draw) && BlockPairBlocks.TryGetValue(draw, out block);
        terrain = found ? RoomTerrainBlock.Of(block, BlockMirroring.None, Collision(DrawGroup.OwnPicture)) : default;
        return found;
    }

    /// <summary>The kind of draw <paramref name="draw"/> is.</summary>
    /// <exception cref="InvalidOperationException">The draw is no collectible's frame.</exception>
    internal static DrawGroup GroupOf(CollectibleDraw draw) => draw switch
    {
        CollectibleDraw.Empty => DrawGroup.Empty,
        CollectibleDraw.ChozoOrbDim or CollectibleDraw.ChozoOrbGlowing or CollectibleDraw.ChozoOrbBright => DrawGroup.Orb,
        CollectibleDraw.ChozoOrbBurst => DrawGroup.OrbBurst,
        CollectibleDraw.EnergyTankBright or CollectibleDraw.EnergyTankDim or CollectibleDraw.MissileTankDim or CollectibleDraw.MissileTankBright
            or CollectibleDraw.SuperMissileTankDim or CollectibleDraw.SuperMissileTankBright or CollectibleDraw.PowerBombTankBright
            or CollectibleDraw.PowerBombTankDim => DrawGroup.Tank,
        CollectibleDraw.FirstBlockPairFirstFrame or CollectibleDraw.FirstBlockPairSecondFrame or CollectibleDraw.SecondBlockPairFirstFrame
            or CollectibleDraw.SecondBlockPairSecondFrame or CollectibleDraw.ThirdBlockPairFirstFrame or CollectibleDraw.ThirdBlockPairSecondFrame
            or CollectibleDraw.FourthBlockPairFirstFrame or CollectibleDraw.FourthBlockPairSecondFrame => DrawGroup.OwnPicture,
        CollectibleDraw.ShotBlockIntact or CollectibleDraw.ShotBlockCracking or CollectibleDraw.ShotBlockDust => DrawGroup.ShotReveal,
        _ => throw new InvalidOperationException($"Undefined {nameof(CollectibleDraw)} {draw}."),
    };

    /// <summary>Accepts a collectible draw that is one of the collectibles' frames.</summary>
    private static bool TryLocate(RoomObjectDraw request, out CollectibleDraw draw) =>
        RoomObjectDraw.TryFrame(request, static (CollectibleDraw frame) => Enum.IsDefined(frame), out draw);

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

    /// <summary>A shot-block reveal shows three consecutive draws; hiding consumes them in reverse.</summary>
    internal static RoomObjectFrameDraw<CollectibleDraw> ShotRevealFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 3)
            throw new InvalidDataException(
                $"Shot-block reveal index {animationIndex} is outside three authored frames.");
        return RoomObjectDraw.Of(ShotRevealFrames[animationIndex]);
    }

    /// <summary>
    /// The visible frame <paramref name="animationIndex"/> of a tank kind, or of the block pair an item that shows its own
    /// picture took (<see cref="CollectibleBlockPairs"/>): two draws each.
    /// </summary>
    internal static RoomObjectFrameDraw<CollectibleDraw> VisibleFrame(InWorldCollectibleKind kind,
        int animationIndex, CollectibleBlockPairs.Pair? blockPair)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        if (TankFrames.TryGetValue(kind, out CollectibleDraw[]? tank))
            return RoomObjectDraw.Of(tank[animationIndex]);
        if (blockPair is not { } pair)
            throw new InvalidDataException($"Collectible {kind} shows its own picture but took no block pair.");
        return BlockPairFrames.TryGetValue(pair, out CollectibleDraw[]? frames)
            ? RoomObjectDraw.Of(frames[animationIndex])
            : throw new InvalidDataException($"Undefined collectible block pair {pair}.");
    }
}
