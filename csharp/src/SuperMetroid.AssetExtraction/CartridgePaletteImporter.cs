using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Resolves cartridge-backed palette DMA while importing assets. Runtime CGRAM only
/// accepts mutable memory sources; ROM colors must become installed asset bytes first.
/// </summary>
internal static class CartridgePaletteImporter
{
    public static void LoadToCgram(
        SnesCgram destination,
        ISnesAddressSpace bus,
        int sourceAddress,
        int colorCount = SnesCgram.ColorCount,
        int destinationIndex = 0)
    {
        ArgumentNullException.ThrowIfNull(destination);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        if (colorCount < 0 || destinationIndex < 0 || destinationIndex + colorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(colorCount), "CGRAM load must remain within 256 colors.");

        SnesAddress source = SnesAddress.FromBusAddress(sourceAddress);
        for (int color = 0; color < colorCount; color++)
        {
            byte low = cartridge.ReadCartridgeByte((int)source.AddWithinBank(color * 2));
            byte high = cartridge.ReadCartridgeByte((int)source.AddWithinBank(color * 2 + 1));
            destination.SetColor(destinationIndex + color, (ushort)(low | (high << 8)));
        }
    }
}
