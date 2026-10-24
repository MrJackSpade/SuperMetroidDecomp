using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Verification access to <see cref="EscapeTimerTileAtlas"/> members production does not use.</summary>
internal static class EscapeTimerTileAtlasAccess
{
    /// <summary>Exposes <see cref="EscapeTimerTileAtlas"/> state that only verification reads.</summary>
    extension(EscapeTimerTileAtlas self)
    {
        /// <summary>Shows one native page in the OBJ character table for the Mother Brain sequence's synchronous transfer owner.</summary>
        [PpuLayoutBoundary("codec: a matched native escape-timer transfer record's encoded VRAM destination")]
        public bool TryRecordNativeTransfer(ObjCharacterUploads uploads, int sourceAddress, ushort byteCount,
            ushort destinationWord)
        {
            Ensure.NotNull(uploads);
            if (!EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, out EscapeTimerTilePage page) ||
                ObjCharacterUploads.TableByte(VramDmaDestination.FromNative(destinationWord)) != EscapeTimerTilePages.FirstCharacterOf(page) * 32)
                return false;

            VramAssetId asset = EscapeTimerTilePages.AssetOf(page);
            if (self.Resolve(asset).Length != byteCount)
                throw new InvalidDataException($"Escape timer {page} page no longer matches its native transfer record.");
            uploads.RecordUpload(VramDmaDestination.FromNative(destinationWord), byteCount, new AssetObjCharacters(asset));
            return true;
        }
    }
}
