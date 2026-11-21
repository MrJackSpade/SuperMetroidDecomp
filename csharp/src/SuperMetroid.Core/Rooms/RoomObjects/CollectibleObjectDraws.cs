using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectCollectibleDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The collectibles' draws (#2034): how each draw's block collides, the terrain a block pair's frame writes, and the frame
/// selection; the Chozo orb's frames are its animation's (<see cref="CollectibleSequenceAnimations"/>). What a frame other than a
/// block pair's shows is its installed picture (#1557).
/// </summary>
internal static class CollectibleObjectDraws
{
    /// <summary>
    /// The collectibles' frames: each one block at the object's block, pictured by the installed collectible frames, but a
    /// block pair's frame, which lies under the item's own picture (#1538) and writes its terrain alone.
    /// </summary>
    internal static RoomObjectDrawFamily<CollectibleDraw> Family { get; } = new CollectibleDrawFamily();

    /// <summary>How the block <paramref name="draw"/> writes collides.</summary>
    /// <exception cref="InvalidOperationException">The draw is no collectible's frame.</exception>
    internal static RoomCollisionType CollisionOf(CollectibleDraw draw) => Collision(GroupOf(draw));

    /// <summary>The looks of an item's own picture, in the order a block pair's draws are matched.</summary>
    private static readonly CollectibleLook[] Looks = [CollectibleLook.Primary, CollectibleLook.Alternate];

    /// <summary>
    /// The terrain a block pair's frame writes, or null when <paramref name="draw"/> is not one: the frame lies under the
    /// item's own picture (#1538), has no installed picture of its own, and writes its pair's block for that look, which is
    /// gameplay terrain (<see cref="CollectiblePairBlock"/>, #2354).
    /// </summary>
    internal static RoomTerrainBlock? BlockPairTerrain(CollectibleDraw draw)
    {
        foreach (CollectibleBlockPairs.Pair pair in CollectibleBlockPairs.InTurn)
        {
            CollectibleLookDraws looks = BlockPairLooks[pair];
            foreach (CollectibleLook look in Looks)
            {
                if (looks.Of(look) == draw)
                    return new RoomTerrainBlock(RoomBlockAppearance.Of(new CollectiblePairBlock(pair, look)),
                        Collision(DrawGroup.OwnPicture));
            }
        }
        return null;
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
        CollectibleDraw.SolidPairPrimaryLook or CollectibleDraw.SolidPairAlternateLook or CollectibleDraw.FirstDoorPairPrimaryLook
            or CollectibleDraw.FirstDoorPairAlternateLook or CollectibleDraw.SecondDoorPairPrimaryLook or CollectibleDraw.SecondDoorPairAlternateLook
            or CollectibleDraw.ThirdDoorPairPrimaryLook or CollectibleDraw.ThirdDoorPairAlternateLook => DrawGroup.OwnPicture,
        CollectibleDraw.ShotBlockIntact or CollectibleDraw.ShotBlockCracking or CollectibleDraw.ShotBlockDust => DrawGroup.ShotReveal,
        _ => throw new InvalidOperationException($"Undefined {nameof(CollectibleDraw)} {draw}."),
    };


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

    /// <summary>The draw of <paramref name="look"/> of the tank kind <paramref name="kind"/>, in its own blocks.</summary>
    /// <exception cref="InvalidDataException">The kind is no tank.</exception>
    internal static RoomObjectFrameDraw<CollectibleDraw> TankFrame(InWorldCollectibleKind kind, CollectibleLook look) =>
        TankLooks.TryGetValue(kind, out CollectibleLookDraws tank)
            ? Family.Of(tank.Of(look))
            : throw new InvalidDataException($"Collectible {kind} is no tank.");

    /// <summary>
    /// The draw of <paramref name="look"/> into the block pair <paramref name="pair"/> an item that shows its own picture
    /// took (<see cref="CollectibleBlockPairs"/>).
    /// </summary>
    internal static RoomObjectFrameDraw<CollectibleDraw> BlockPairFrame(CollectibleBlockPairs.Pair pair, CollectibleLook look) =>
        Family.Of(BlockPairLooks[pair].Of(look));

    /// <summary>The collectibles' family: a block pair's frame writes its terrain alone; every other frame draws as pictured.</summary>
    private sealed class CollectibleDrawFamily() : RoomObjectDrawFamily<CollectibleDraw>("collectible",
        frame => RoomBlockShapes.Block(CollisionOf(frame)))
    {
        /// <inheritdoc/>
        internal override void Draw(RoomLevelData level, Position16<RoomBlocks> origin, IRoomObjectDrawing drawing,
            RoomObjectFrameDraw<CollectibleDraw> draw)
        {
            if (BlockPairTerrain(Ensure.NotNull(draw).Frame) is { } blockPair)
                DrawTerrainAt(level, draw, origin, Offset16<RoomBlocks>.Zero, blockPair);
            else
                base.Draw(level, origin, drawing, draw);
        }
    }
}
