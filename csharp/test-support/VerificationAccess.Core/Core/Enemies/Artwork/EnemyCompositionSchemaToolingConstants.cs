using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Cartridge values and helpers of <see cref="EnemyCompositionSchemaTooling"/> that only verification reads, and the
/// bosses' BG2 frame writes by frame, which only verification looks up once the import composes the body sheets (#1466).
/// </summary>
internal static class EnemyCompositionSchemaToolingConstants
{
    /// <summary>Crocomire's BG2 frames by bank-$A4 address.</summary>
    extension(CrocomireBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => LookUp(0xa4, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => Find(self.Writes, frame, out writes);
    }
    /// <summary>Draygon's BG2 frames by bank-$A5 address.</summary>
    extension(DraygonBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => LookUp(0xa5, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => Find(self.Writes, frame, out writes);
    }
    /// <summary>Phantoon's BG2 frames by bank-$A7 address.</summary>
    extension(PhantoonBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => LookUp(0xa7, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => Find(self.Writes, frame, out writes);
    }
    /// <summary>Mother Brain's BG2 body frames by bank-$A9 address.</summary>
    extension(MotherBrainBodyBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => LookUp(0xa9, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => Find(self.Writes, frame, out writes);
    }

    /// <summary>A shared BG2 frame catalog's writes by frame.</summary>
    extension(EnemyBg2FrameCatalog self)
    {
        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => Find(self, frame, out writes);
    }

    /// <summary>The writes <paramref name="catalog"/> holds for <paramref name="frame"/>, if any.</summary>
    private static bool Find(EnemyBg2FrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes)
    {
        bool found = catalog.frames.TryGetValue(frame, out EnemyBg2TilemapWrite[]? held);
        writes = held;
        return found;
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
