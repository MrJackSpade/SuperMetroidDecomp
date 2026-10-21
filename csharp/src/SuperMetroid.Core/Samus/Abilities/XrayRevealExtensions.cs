using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Native X-ray extension traversal at $91:CE79-CF34; deliberately not the collision extension resolver.</summary>
public static class XrayRevealExtensions
{
    /// <summary>
    /// Returns the replacement metatile, or null to retain copied BG1 art. Unlike collision,
    /// X-ray extensions reveal only a terminal scroll-trigger block, not arbitrary linked terrain.
    /// </summary>
    public static ushort? Resolve(RoomLevelData level, int blockIndex,
        XrayRevealVisualCatalog? visuals = null)
    {
        Ensure.NotNull(level);
        RoomCollisionBlock block = level.GetPlmCollisionBlockByIndex(blockIndex);
        if (block.CollisionType is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension))
            throw new ArgumentException("X-ray extension traversal requires an extension block.", nameof(blockIndex));
        if (block.Bts.Value == 0) return null;
        ushort x = (ushort)(blockIndex % level.Size.X);
        ushort y = (ushort)(blockIndex / level.Size.X);
        bool vertical = block.CollisionType == RoomCollisionType.VerticalExtension;
        // Both entry points sign-extend the initial BTS. Later reads are asymmetric:
        // only the horizontal-extension branch repeats that sign extension.
        int step = unchecked((sbyte)block.Bts.Value);
        var visited = new HashSet<(ushort X, ushort Y, bool Vertical, int Step)>();
        while (true)
        {
            if (!visited.Add((x, y, vertical, step)))
                throw new InvalidDataException($"Cyclic X-ray extension chain from block {blockIndex}.");
            ushort coordinate = unchecked((ushort)((vertical ? y : x) + step));
            if (unchecked((short)coordinate) < 0) return XrayRevealTableWords.BlankMetatile;
            if (vertical) y = coordinate; else x = coordinate;
            // GetBlockTypeAndBTS uses the hardware's byte-wide Y multiplication;
            // X is then added as a word, so it may legitimately spill into the next row.
            block = level.GetPlmCollisionBlockByIndex(unchecked((ushort)((byte)y * level.Size.X + x)));
            step = block.Bts.Value;
            if (block.CollisionType == RoomCollisionType.VerticalExtension)
            {
                vertical = true;
                continue;
            }
            if (block.CollisionType == RoomCollisionType.HorizontalExtension)
            {
                // A positive horizontal BTS reached from the vertical loop stays vertical
                // ($91:CEB2). Negative horizontal BTS alone switches that loop to X.
                if (unchecked((sbyte)step) < 0) { step = unchecked((sbyte)step); vertical = false; }
                continue;
            }
            if (block.CollisionType != RoomCollisionType.SpecialAir) return null;
            XrayRevealDefinition? reveal = visuals is not null
                ? visuals.Apply(block.CollisionType, block.Bts.Value)
                : XrayRevealTable.Find(block.CollisionType, block.Bts.Value);
            return reveal?.TopLeft;
        }
    }
}
