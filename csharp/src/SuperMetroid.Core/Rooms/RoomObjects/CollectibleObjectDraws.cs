using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectCollectibleDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The terrain and frame selection of each collectible draw.</summary>
internal static class CollectibleObjectDraws
{
    /// <summary>Physical runtime projection without constructing exported artwork names.</summary>
    internal static bool TryGetBlock(RoomObjectDraw request, out RoomTerrainBlock block)
    {
        bool found = TryLocate(request, out Draw draw);
        block = found ? Block(draw) : default;
        return found;
    }

    /// <summary>
    /// Whether <paramref name="request"/> is a block pair's frame, which lies under the item's own picture (#1538): it
    /// writes only its terrain and has no installed picture of its own.
    /// </summary>
    internal static bool LiesUnderOwnPicture(RoomObjectDraw request) =>
        TryLocate(request, out Draw draw) && draw.Group == DrawGroup.OwnPicture;

    /// <summary>The number of frames in a group.</summary>
    internal static int CountOf(DrawGroup group) => group switch
    {
        DrawGroup.Empty => 1,
        DrawGroup.Orb => 3,
        DrawGroup.OrbBurst => 1,
        DrawGroup.Tank => 8,
        DrawGroup.OwnPicture => CollectibleBlockPairs.Count * 2,
        DrawGroup.ShotReveal => 3,
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {group}."),
    };

    /// <summary>Accepts a collectible draw whose frame lies within its group.</summary>
    private static bool TryLocate(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw frame) => (uint)frame.Index < (uint)CountOf(frame.Group), out draw);

    /// <summary>The terrain a draw writes; each group advances one block per frame.</summary>
    private static RoomTerrainBlock Block(Draw draw) => draw.Group switch
    {
        DrawGroup.Empty => RoomTerrainBlocks.BlankAir,
        DrawGroup.Orb => RoomTerrainBlock.Of(OrbFirstBlock + draw.Index, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        DrawGroup.OrbBurst => RoomTerrainBlock.Of(OrbBurstBlock, BlockMirroring.None, RoomCollisionType.SolidBlock),
        DrawGroup.Tank => RoomTerrainBlock.Of(TankFirstBlock + draw.Index, BlockMirroring.None, RoomCollisionType.SpecialBlock),
        DrawGroup.OwnPicture => RoomTerrainBlock.Of(OwnPictureFirstBlock + draw.Index, BlockMirroring.None, RoomCollisionType.SpecialBlock),
        DrawGroup.ShotReveal => RoomTerrainBlock.Of(ShotRevealFirstBlock + draw.Index, BlockMirroring.None, RoomCollisionType.SolidBlock),
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {draw.Group}."),
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
        int animationIndex, int? blockPair)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        int tank = Array.IndexOf(TankKinds, kind);
        if (tank >= 0)
            return RoomObjectDraw.Of(new Draw(DrawGroup.Tank, tank * 2 + animationIndex));
        if (blockPair is not { } pair || pair is < 0 or >= CollectibleBlockPairs.Count)
            throw new InvalidDataException(
                $"Collectible {kind} block pair {blockPair} is not one of the {CollectibleBlockPairs.Count} pairs.");
        return RoomObjectDraw.Of(new Draw(DrawGroup.OwnPicture, pair * 2 + animationIndex));
    }
}
