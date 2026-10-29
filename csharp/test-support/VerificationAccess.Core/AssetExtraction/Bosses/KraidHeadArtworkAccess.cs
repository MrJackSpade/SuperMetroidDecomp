using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Kraid's installed head images by cartridge address.</summary>
internal static class KraidHeadArtworkAccess
{
    /// <summary>Head images by address.</summary>
    extension(KraidBackgroundArtwork artwork)
    {
        /// <summary>
        /// The installed head image stored at bank-$A7 address <paramref name="address"/>, each tile encoded through the
        /// codec as the cartridge's tilemap entry.
        /// </summary>
        internal ReadOnlySpan<ushort> HeadWords(ushort address) => BackgroundTileWordBytes.Words(
            artwork.HeadTiles(KraidHeadFrame.ByMouthStage[KraidHeadFrameDefinitions.MouthStageAt(address)]));
    }

    /// <summary>Installed head file names by address.</summary>
    extension(KraidBackgroundArtworkFormat)
    {
        /// <summary>The installed JSON file of the head tilemap stored at <paramref name="address"/>.</summary>
        internal static string HeadFileNameAt(ushort address) =>
            KraidBackgroundArtworkFormat.HeadFileName(KraidHeadFrameDefinitions.MouthStageAt(address));
    }
}
