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

    /// <summary>The number of frames in a group.</summary>
    internal static int CountOf(DrawGroup group) => group switch
    {
        DrawGroup.Empty => 1,
        DrawGroup.Orb => 3,
        DrawGroup.OrbBurst => 1,
        DrawGroup.Tank => 8,
        DrawGroup.Dynamic => 8,
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
        DrawGroup.Dynamic => RoomTerrainBlock.Of(DynamicFirstBlock + draw.Index, BlockMirroring.None, RoomCollisionType.SpecialBlock),
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

    /// <summary>Two draws per tank kind or allocated dynamic graphics slot.</summary>
    internal static RoomObjectFrameDraw<Draw> VisibleFrame(InWorldCollectibleKind kind,
        int animationIndex, int graphicsSlot)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        int tank = Array.IndexOf(TankKinds, kind);
        if (tank >= 0)
            return RoomObjectDraw.Of(new Draw(DrawGroup.Tank, tank * 2 + animationIndex));
        if ((uint)graphicsSlot >= 4)
            throw new InvalidDataException(
                $"Dynamic collectible graphics slot {graphicsSlot} is outside four allocations.");
        return RoomObjectDraw.Of(new Draw(DrawGroup.Dynamic, graphicsSlot * 2 + animationIndex));
    }
}
