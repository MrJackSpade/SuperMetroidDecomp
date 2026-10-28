using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Verification's native view of Kraid's head images: each by the bank-$A7 address the cartridge stores it at.</summary>
internal static class KraidHeadFrameDefinitions
{
    /// <summary>The bank-$A7 address of the head image <paramref name="frame"/> shows.</summary>
    internal static ushort PresentationKey(KraidHeadFrame frame) => KraidHeadArtworkDefinitions.Address(frame.MouthStage);

    /// <summary>Every head image's address, from the closed mouth to the fully open one.</summary>
    internal static IEnumerable<ushort> PresentationKeys => KraidHeadFrame.ByMouthStage.Select(PresentationKey);

    /// <summary>The mouth stage of the head image stored at <paramref name="address"/>.</summary>
    /// <exception cref="InvalidDataException">No head image is stored at <paramref name="address"/>.</exception>
    internal static int MouthStageAt(ushort address)
    {
        foreach (KraidHeadFrame head in KraidHeadFrame.ByMouthStage)
            if (PresentationKey(head) == address)
                return head.MouthStage;
        throw new InvalidDataException($"No Kraid head tilemap is stored at $A7:{address:X4}.");
    }
}

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
