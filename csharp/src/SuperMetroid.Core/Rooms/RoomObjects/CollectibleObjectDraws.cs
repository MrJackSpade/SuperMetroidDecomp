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
        foreach ((CollectibleDraw frame, RoomTerrainBlock terrain) in BlockPairFrames)
        {
            if (frame == draw)
                return terrain;
        }
        return null;
    }

    /// <summary>
    /// Every block pair's frame, in the pairs' turn and then look order, with the terrain it writes (#2554): the frames
    /// under the item's own picture, each carrying its pair's block.
    /// </summary>
    internal static IEnumerable<(CollectibleDraw Draw, RoomTerrainBlock Terrain)> BlockPairFrames =>
        CollectibleBlockPairs.InTurn.SelectMany(pair => Looks.Select(look => (BlockPairLooks[pair].Of(look),
            new RoomTerrainBlock(RoomBlockAppearance.Of(new CollectiblePairBlock(pair, look)), Collision(DrawGroup.OwnPicture)))));

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
        CollectibleDraw.FirstBlockPairPrimaryLook or CollectibleDraw.FirstBlockPairAlternateLook or CollectibleDraw.SecondBlockPairPrimaryLook
            or CollectibleDraw.SecondBlockPairAlternateLook or CollectibleDraw.ThirdBlockPairPrimaryLook or CollectibleDraw.ThirdBlockPairAlternateLook
            or CollectibleDraw.FourthBlockPairPrimaryLook or CollectibleDraw.FourthBlockPairAlternateLook => DrawGroup.OwnPicture,
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

    /// <summary>
    /// The draw of <paramref name="look"/> of a tank kind, or of the block pair an item that shows its own picture took
    /// (<see cref="CollectibleBlockPairs"/>).
    /// </summary>
    internal static RoomObjectFrameDraw<CollectibleDraw> VisibleFrame(InWorldCollectibleKind kind,
        CollectibleLook look, CollectibleBlockPairs.Pair? blockPair)
    {
        if (TankLooks.TryGetValue(kind, out CollectibleLookDraws tank))
            return Family.Of(tank.Of(look));
        if (blockPair is not { } pair)
            throw new InvalidDataException($"Collectible {kind} shows its own picture but took no block pair.");
        return BlockPairLooks.TryGetValue(pair, out CollectibleLookDraws looks)
            ? Family.Of(looks.Of(look))
            : throw new InvalidDataException($"Undefined collectible block pair {pair}.");
    }

    /// <summary>The collectibles' family: a block pair's frame writes its terrain alone; every other frame draws as pictured.</summary>
    private sealed class CollectibleDrawFamily() : RoomObjectDrawFamily<CollectibleDraw>("collectible",
        frame => RoomBlockShapes.Block(CollisionOf(frame)),
        (drawing, draw) => drawing.Installed<InstalledRoomObjectCollectibleVisuals>().Image(draw))
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
