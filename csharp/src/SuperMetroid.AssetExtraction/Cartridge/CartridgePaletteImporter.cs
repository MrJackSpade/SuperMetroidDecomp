using SuperMetroid.Core.Assets;
using SuperMetroid.Core;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// Resolves cartridge-backed palette DMA while importing assets. Runtime CGRAM only
/// accepts mutable memory sources; ROM colors must become installed asset bytes first.
/// </summary>
public static class CartridgePaletteImporter
{
    /// <summary>Copies native little-endian color words from a cartridge, WRAM, or SRAM DMA source into a bounded CGRAM range during asset import.</summary>
    /// <param name="destination">Mutable CGRAM receiving decoded BGR555 words.</param>
    /// <param name="bus">Import address space; WRAM or SRAM sources additionally require mutable-memory access.</param>
    /// <param name="sourceAddress">Initial SNES bus address, advanced within its bank for each source byte.</param>
    /// <param name="colorCount">Number of consecutive color words to copy.</param>
    /// <param name="destinationIndex">First zero-based CGRAM color slot to replace.</param>
    public static void LoadToCgram(
        IPaletteColors destination,
        ISnesAddressSpace bus,
        int sourceAddress,
        int colorCount = SnesCgram.ColorCount,
        int destinationIndex = 0)
    {
        Ensure.NotNull(destination);
        Ensure.NotNull(bus);
        if (colorCount < 0 || destinationIndex < 0 || destinationIndex + colorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(colorCount), "CGRAM load must remain within 256 colors.");

        SnesAddress source = SnesAddress.FromBusAddress(sourceAddress);
        byte ReadImportSource(SnesAddress address) => SnesDmaSourceMap.Classify(address) switch
        {
            SnesDmaSourceKind.Cartridge => CartridgeImportSource.Require(bus).ReadCartridgeByte((int)address),
            SnesDmaSourceKind.WorkRam => (bus as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Import palette transfer requires WRAM.")).ReadWorkRamByte((int)address),
            SnesDmaSourceKind.Unmapped => throw new InvalidOperationException($"Import palette source {address} is unmapped."),
            _ => throw new InvalidOperationException($"Import palette source {address} is unmapped."),
        };
        for (int color = 0; color < colorCount; color++)
        {
            // A DMA to CGRAM stores the fifteen color bits of each transferred word, low byte first.
            destination.SetColor(destinationIndex + color, RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(
                ReadImportSource(source.AddWithinBank(color * 2)), ReadImportSource(source.AddWithinBank(color * 2 + 1)))));
        }
    }
}
