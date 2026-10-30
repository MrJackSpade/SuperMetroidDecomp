using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmCollectibleDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The terrain and artwork name of each collectible draw.</summary>
internal static class CollectiblePlmDraws
{
    /// <summary>Physical runtime projection without constructing exported artwork names.</summary>
    internal static bool TryGetBlock(PlmDraw request, out RoomTerrainBlock block)
    {
        bool found = TryLocate(request, out Draw draw);
        block = found ? Block(draw) : default;
        return found;
    }

    /// <summary>The number of frames in a group.</summary>
    private static int CountOf(DrawGroup group) => group switch
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
    private static bool TryLocate(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw frame) => (uint)frame.Index < (uint)CountOf(frame.Group), out draw);

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

    /// <summary>The editable artwork identifier of a draw.</summary>
    internal static string VisualId(Draw draw) => draw.Group switch
    {
        DrawGroup.Empty => "empty",
        DrawGroup.Orb => $"chozo-orb-{draw.Index}",
        DrawGroup.OrbBurst => "chozo-orb-burst",
        DrawGroup.Tank => $"{TankKindName(draw.Index / 2)}-tank-{draw.Index % 2}",
        DrawGroup.Dynamic => $"dynamic-slot-{draw.Index / 2}-frame-{draw.Index % 2}",
        DrawGroup.ShotReveal => $"shot-reveal-{draw.Index}",
        _ => throw new InvalidOperationException($"Undefined {nameof(DrawGroup)} {draw.Group}."),
    };

    /// <summary>Gets the stable visual ID stem for one of the four tank pickup kinds.</summary>
    private static string TankKindName(int kind) => kind switch
    {
        0 => "energy",
        1 => "missile",
        2 => "super-missile",
        _ => "power-bomb",
    };

    /// <summary>Resolves an exact artwork identifier to its compiled collectible draw frame.</summary>
    internal static bool TryGetById(string id, out RoomPlmCollectibleDrawFrame frame)
    {
        foreach (var candidate in All)
        {
            if (!string.Equals(candidate.Id, id, StringComparison.Ordinal)) continue;
            frame = candidate;
            return true;
        }
        frame = default;
        return false;
    }

    /// <summary>The orb cycles forward through its three draws and back through the middle draw.</summary>
    internal static PlmFrameDraw<Draw> OrbFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 4)
            throw new InvalidDataException(
                $"Chozo orb animation index {animationIndex} is outside four authored phases.");
        return PlmDraw.Of(new Draw(DrawGroup.Orb, 2 - Math.Abs(animationIndex - 2)));
    }

    /// <summary>A shot-block reveal shows three consecutive draws; hiding consumes them in reverse.</summary>
    internal static PlmFrameDraw<Draw> ShotRevealFrame(int animationIndex)
    {
        if ((uint)animationIndex >= 3)
            throw new InvalidDataException(
                $"Shot-block reveal index {animationIndex} is outside three authored frames.");
        return PlmDraw.Of(new Draw(DrawGroup.ShotReveal, animationIndex));
    }

    /// <summary>Two draws per tank kind or allocated dynamic graphics slot.</summary>
    internal static PlmFrameDraw<Draw> VisibleFrame(InWorldCollectibleKind kind,
        int animationIndex, int graphicsSlot)
    {
        if ((uint)animationIndex >= 2)
            throw new InvalidDataException(
                $"Collectible animation index {animationIndex} is outside two authored frames.");
        int tank = Array.IndexOf(TankKinds, kind);
        if (tank >= 0)
            return PlmDraw.Of(new Draw(DrawGroup.Tank, tank * 2 + animationIndex));
        if ((uint)graphicsSlot >= 4)
            throw new InvalidDataException(
                $"Dynamic collectible graphics slot {graphicsSlot} is outside four allocations.");
        return PlmDraw.Of(new Draw(DrawGroup.Dynamic, graphicsSlot * 2 + animationIndex));
    }

    /// <summary>Enumerates all 24 collectible one-cell draws in their published order.</summary>
    internal static IEnumerable<RoomPlmCollectibleDrawFrame> All
    {
        get
        {
            foreach (DrawGroup group in Enum.GetValues<DrawGroup>())
            {
                for (int index = 0; index < CountOf(group); index++)
                {
                    var draw = new Draw(group, index);
                    yield return new(draw, Block(draw), VisualId(draw));
                }
            }
        }
    }
}
