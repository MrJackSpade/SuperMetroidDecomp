using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The X-ray scope's extension traversal; deliberately not the collision extension resolver.</summary>
public static class XrayRevealExtensions
{
    /// <summary>
    /// Returns the replacement block, or null to retain copied foreground art. Unlike collision,
    /// X-ray extensions reveal only a terminal scroll-trigger block, not arbitrary linked terrain.
    /// </summary>
    /// <param name="level">The room whose extension chain is followed.</param>
    /// <param name="start">The extension block the chain starts at, in the room's columns.</param>
    /// <param name="visuals">The installed reveal blocks, which a terminal special-air block or a backward exit from the room shows.</param>
    public static int? Resolve(RoomLevelData level, Position16<RoomBlocks> start, InstalledXrayReveals visuals)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(visuals);
        RoomCollisionBlock block = level.GetObjectCollisionBlock(start);
        if (block.CollisionType is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension))
            throw new ArgumentException("X-ray extension traversal requires an extension block.", nameof(start));
        if (block.Behavior.EndsExtension) return null;
        int x = start.X;
        int y = start.Y;
        bool vertical = block.CollisionType == RoomCollisionType.VerticalExtension;
        // Both entry points read the initial offset signed. Later reads are asymmetric: only the horizontal-extension
        // branch reads it signed again; the vertical branch reads a backward offset as a forward step of its distance
        // short of a full offset span.
        int step = block.Behavior.ExtensionOffset;
        var visited = new HashSet<XrayRevealVisit>();
        while (true)
        {
            if (!visited.Add(new(Position16<RoomBlocks>.Of(x, y), vertical, step)))
                throw new InvalidDataException($"Cyclic X-ray extension chain from block {start}.");
            int coordinate = CoordinateAxis.Wrap((vertical ? y : x) + step);
            if (CoordinateAxis.AsSigned(coordinate) < 0) return visuals.ExtensionExitBlock;
            if (vertical) y = coordinate; else x = coordinate;
            // The block lookup takes Y's row within the scan cycle; X is then added, so it may legitimately spill into
            // the next row.
            block = level.GetObjectCollisionBlock(BlockScanCycle.RevealLookupBlock(Position16<RoomBlocks>.Of(x, y), level.Size.X));
            if (block.CollisionType == RoomCollisionType.VerticalExtension)
            {
                step = ForwardStep(block.Behavior.ExtensionOffset);
                vertical = true;
                continue;
            }
            if (block.CollisionType == RoomCollisionType.HorizontalExtension)
            {
                // A positive horizontal offset reached from the vertical loop stays vertical. A negative horizontal
                // offset alone switches that loop to X.
                step = block.Behavior.ExtensionOffset;
                if (step < 0) vertical = false;
                continue;
            }
            if (block.CollisionType != RoomCollisionType.SpecialAir) return null;
            return visuals.Apply(block.CollisionType, block.Behavior)?.TopLeft;
        }
    }

    /// <summary>
    /// The step the vertical loop takes for an extension offset: a backward offset becomes a forward step of the span of
    /// offsets less its distance.
    /// </summary>
    private static int ForwardStep(int offset) =>
        offset < 0 ? offset + (RoomBlockBehavior.MostForwardExtension - RoomBlockBehavior.MostBackwardExtension + 1) : offset;
}
