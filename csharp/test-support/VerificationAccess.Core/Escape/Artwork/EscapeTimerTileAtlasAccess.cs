using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Verification access to <see cref="EscapeTimerTileAtlas"/> members production does not use.</summary>
internal static class EscapeTimerTileAtlasAccess
{
    /// <summary>Exposes <see cref="EscapeTimerTileAtlas"/> state that only verification reads.</summary>
    extension(EscapeTimerTileAtlas self)
    {
        /// <summary>Shows one native page in the OBJ character table for the Mother Brain sequence's synchronous transfer owner.</summary>
        public bool TryRecordNativeTransfer(SpriteArtworkPlacements uploads, int sourceAddress, ushort byteCount,
            ushort destinationWord)
        {
            Ensure.NotNull(uploads);
            if (!EscapeTimerTilePagesConstants.TryMatch(sourceAddress, byteCount, out EscapeTimerTilePage page) ||
                SpriteArtworkPlacements.TableByte(VramDmaDestination.FromNative(destinationWord)) != EscapeTimerTilePages.FirstCharacterOf(page) * 32)
                return false;

            InstalledArtworkId asset = EscapeTimerTilePages.AssetOf(page);
            if (self.Resolve(asset).Length != byteCount)
                throw new InvalidDataException($"Escape timer {page} page no longer matches its native transfer record.");
            uploads.RecordUpload(VramDmaDestination.FromNative(destinationWord), byteCount, new InstalledArtworkCharacters(asset));
            return true;
        }
        /// <summary>Restored native queue descriptors resolve the same two installed pages as typed uploads.</summary>
        public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            if (EscapeTimerTilePagesConstants.TryMatch(sourceAddress, byteCount, out EscapeTimerTilePage page))
            {
                data = self.Resolve(EscapeTimerTilePages.AssetOf(page));
                return true;
            }
            data = default;
            return false;
        }
    }
}
