using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Exports the nine palette rows selected by the compiled Ceres getaway zoom curve.</summary>
public static class CeresRidleyMode7ColorExtractor
{
    /// <summary>Reads the nine fixed-stride native palette rows selected by the compiled getaway zoom curve and serializes their RGB5 components.</summary>
    /// <param name="bus">Supported-cartridge address space containing the Ceres Ridley Mode 7 zoom colors.</param>
    /// <returns>UTF-8 JSON bytes for the versioned zoom-color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var rows = new PaletteRgb5[CeresRidleyPaletteDefinitions.GetawayZoomStepCount][];
        for (int row = 0; row < rows.Length; row++)
        {
            byte[] bytes = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RidleySourceDefinitions.Ceres.GetawayZoomColors +
                row * RidleySourceDefinitions.Ceres.GetawayZoomRowBytes,
                CeresRidleyPaletteDefinitions.GetawayZoomColorCount * sizeof(ushort));
            rows[row] = new PaletteRgb5[CeresRidleyPaletteDefinitions.GetawayZoomColorCount];
            for (int color = 0; color < rows[row].Length; color++)
            {
                int word = LittleEndianBytes.Word(bytes[color * 2], bytes[color * 2 + 1]);
                rows[row][color] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
        }
        return CeresRidleyMode7ColorFile.Write(new CeresRidleyMode7ColorDocument
        {
            Version = CeresRidleyFloorColorFormat.Version,
            ZoomRows = rows,
        });
    }
}
