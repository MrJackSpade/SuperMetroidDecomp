
namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The VRAM word each ending upload lands at (#1406): the cartridge's layout, which only extraction's replay of the
/// ending screen needs.
/// </summary>
internal static class EndingVramDestinations
{
    /// <summary>The VRAM word post-shot part <paramref name="transfer"/> lands at.</summary>
    /// <exception cref="InvalidOperationException">The part names no defined source.</exception>
    internal static ushort PostShotWordOf(EndingPostShotUploadDefinition transfer) => transfer.Source switch
    {
        EndingPostShotUploadSource.Subtitle => EndingVramDestinationDefinitions.SubtitleWord,
        EndingPostShotUploadSource.LogoTiles => checked((ushort)(EndingVramDestinationDefinitions.FirstLogoTileWord +
            transfer.Offset / sizeof(ushort))),
        EndingPostShotUploadSource.LogoMap => EndingVramDestinationDefinitions.LogoMapWord,
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
    };
}
