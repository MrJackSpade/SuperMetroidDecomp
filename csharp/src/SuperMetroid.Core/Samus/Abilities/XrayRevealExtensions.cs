using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The X-ray scope's extension traversal; deliberately not the collision extension resolver.</summary>
public static class XrayRevealExtensions
{
    /// <summary>
    /// Returns the replacement metatile, or null to retain copied BG1 art. Unlike collision,
    /// X-ray extensions reveal only a terminal scroll-trigger block, not arbitrary linked terrain.
    /// </summary>
    public static int? Resolve(RoomLevelData level, int blockIndex,
        InstalledXrayReveals? visuals = null)
    {
        Ensure.NotNull(level);
        RoomCollisionBlock block = level.GetPlmCollisionBlockByIndex(blockIndex);
        if (block.CollisionType is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension))
            throw new ArgumentException("X-ray extension traversal requires an extension block.", nameof(blockIndex));
        if (block.Behavior == RoomBlockBehaviorValues.None) return null;
        int x = blockIndex % level.Size.X;
        int y = blockIndex / level.Size.X;
        bool vertical = block.CollisionType == RoomCollisionType.VerticalExtension;
        // Both entry points read the initial behaviour as a signed offset. Later reads are asymmetric: only the
        // horizontal-extension branch reads it signed again; the vertical branch reads the unsigned value.
        int step = block.Behavior.ExtensionOffset;
        var visited = new HashSet<XrayRevealVisit>();
        while (true)
        {
            if (!visited.Add(new(Position16<RoomBlocks>.Of(x, y), vertical, step)))
                throw new InvalidDataException($"Cyclic X-ray extension chain from block {blockIndex}.");
            int coordinate = CoordinateAxis.Wrap((vertical ? y : x) + step);
            if (CoordinateAxis.AsSigned(coordinate) < 0) return XrayRevealDefinitions.BlankMetatile;
            if (vertical) y = coordinate; else x = coordinate;
            // The block lookup multiplies only Y's low byte by the room width; X is then added, so it may legitimately
            // spill into the next row.
            block = level.GetPlmCollisionBlockByIndex(CoordinateAxis.Wrap(WordWrap.LowByte(y) * level.Size.X + x));
            step = block.Behavior.Value;
            if (block.CollisionType == RoomCollisionType.VerticalExtension)
            {
                vertical = true;
                continue;
            }
            if (block.CollisionType == RoomCollisionType.HorizontalExtension)
            {
                // A positive horizontal offset reached from the vertical loop stays vertical. A negative horizontal
                // offset alone switches that loop to X.
                if (block.Behavior.ExtensionOffset < 0) { step = block.Behavior.ExtensionOffset; vertical = false; }
                continue;
            }
            if (block.CollisionType != RoomCollisionType.SpecialAir) return null;
            XrayRevealDefinition? reveal = visuals is not null
                ? visuals.Apply(block.CollisionType, block.Behavior)
                : XrayRevealTable.Find(block.CollisionType, block.Behavior);
            return reveal?.TopLeft;
        }
    }
}
