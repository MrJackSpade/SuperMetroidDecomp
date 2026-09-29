namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Resolves known visual-transfer identities against current installed artwork.
/// Native source addresses identify bounded resources; they are not readable ROM locations.
/// This contract exposes neither cartridge bytes nor arbitrary address-space reads.
/// </summary>
public interface IInstalledArtworkTransferSource
{
    /// <summary>Returns false only when the source lies outside this artwork domain.</summary>
    bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data);
}
