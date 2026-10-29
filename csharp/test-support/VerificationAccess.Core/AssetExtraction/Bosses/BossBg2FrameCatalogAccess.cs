using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Crocomire's, Draygon's and Phantoon's BG2 frame writes by frame and by the cartridge address of the frame, which only
/// verification looks up once the import composes the body sheets (#1466).
/// </summary>
internal static class BossBg2FrameCatalogAccess
{
    /// <summary>Crocomire's BG2 frames by bank-$A4 address.</summary>
    extension(CrocomireBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
            EnemyBg2FrameCatalogAccess.LookUp(0xa4, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => self.Writes.TryGet(frame, out writes);
    }

    /// <summary>Draygon's BG2 frames by bank-$A5 address.</summary>
    extension(DraygonBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
            EnemyBg2FrameCatalogAccess.LookUp(0xa5, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => self.Writes.TryGet(frame, out writes);
    }

    /// <summary>Phantoon's BG2 frames by bank-$A7 address.</summary>
    extension(PhantoonBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
            EnemyBg2FrameCatalogAccess.LookUp(0xa7, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => self.Writes.TryGet(frame, out writes);
    }
}
