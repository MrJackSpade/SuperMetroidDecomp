using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The bosses' BG2 frame writes by frame, which only verification looks up once the import composes the body sheets
/// (#1466).
/// </summary>
internal static class EnemyBg2FrameCatalogAccess
{
    /// <summary>A shared BG2 frame catalog's writes by frame.</summary>
    extension(EnemyBg2FrameCatalog self)
    {
        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes)
        {
            bool found = self.frames.TryGetValue(frame, out EnemyBg2TilemapWrite[]? held);
            writes = held;
            return found;
        }
    }

    /// <summary>A catalog lookup by frame.</summary>
    internal delegate bool FrameLookup(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes);

    /// <summary>Looks up the frame <paramref name="bank"/> stores at <paramref name="pointer"/>, if any.</summary>
    internal static bool LookUp(byte bank, ushort pointer, FrameLookup lookup, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes)
    {
        if (EnemyFrameCartridgeLookupTooling.TryFrameAt(bank, pointer) is { } frame)
            return lookup(frame, out writes);
        writes = default;
        return false;
    }
}
