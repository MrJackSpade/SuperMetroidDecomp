using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's BG2 body frame writes by frame and by the cartridge address of the frame, which only verification looks
/// up once the import composes her body sheet (#1466).
/// </summary>
internal static class MotherBrainBodyBg2FrameCatalogAccess
{
    /// <summary>Mother Brain's BG2 body frames by bank-$A9 address.</summary>
    extension(MotherBrainBodyBg2FrameCatalog self)
    {
        /// <summary>The writes of the BG2 frame stored at <paramref name="pointer"/>.</summary>
        internal bool TryGet(ushort pointer, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) =>
            EnemyBg2FrameCatalogAccess.LookUp(0xa9, pointer, self.TryGet, out writes);

        /// <summary>The writes of <paramref name="frame"/>, if the catalog holds it.</summary>
        internal bool TryGet(EnemyFrameId frame, out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) => self.Writes.TryGet(frame, out writes);
    }
}
