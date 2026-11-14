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
    /// <remarks>
    /// Every link's offset steps forward or back by its distance, along a row for a horizontal link and down or up a column
    /// for a vertical one. A chain whose next step leaves the room reveals nothing (#1792): it neither runs on into another
    /// row nor shows a block of its own.
    /// </remarks>
    /// <param name="level">The room whose extension chain is followed.</param>
    /// <param name="start">The extension block the chain starts at, in the room's columns.</param>
    /// <param name="visuals">The installed reveal blocks, which a terminal special-air block shows.</param>
    public static int? Resolve(RoomLevelData level, Position16<RoomBlocks> start, InstalledXrayReveals visuals)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(visuals);
        RoomCollisionBlock block = level.GetObjectCollisionBlock(start);
        if (block.CollisionType is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension))
            throw new ArgumentException("X-ray extension traversal requires an extension block.", nameof(start));
        if (block.Behavior.EndsExtension) return null;
        Position16<RoomBlocks> reached = start;
        bool vertical = block.CollisionType == RoomCollisionType.VerticalExtension;
        int step = block.Behavior.ExtensionOffset;
        var visited = new HashSet<XrayRevealVisit>();
        while (true)
        {
            if (!visited.Add(new(reached, vertical, step)))
                throw new InvalidDataException($"Cyclic X-ray extension chain from block {start}.");
            if (XrayRevealMap.RoomBlock(level, vertical ? reached.X : reached.X + step,
                    vertical ? reached.Y + step : reached.Y) is not { } next)
                return null;
            reached = next;
            block = level.GetObjectCollisionBlock(reached);
            if (block.CollisionType == RoomCollisionType.VerticalExtension)
            {
                step = block.Behavior.ExtensionOffset;
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
            return visuals.Apply(block.CollisionType, block.Behavior)?.Blocks?.TopLeft;
        }
    }
}
