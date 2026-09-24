using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the nine palette rows selected by the compiled Ceres getaway zoom curve.</summary>
public static class CeresRidleyMode7ColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var rows = new PaletteRgb5[CeresRidleyPaletteRomData.Mode7ZoomRowCount][];
        for (int row = 0; row < rows.Length; row++)
        {
            byte[] bytes = RomDataReader.ReadFixedBank(bus,
                CeresRidleyPaletteRomData.Mode7ZoomColors +
                row * CeresRidleyPaletteRomData.Mode7ZoomRowByteStride,
                CeresRidleyPaletteRomData.Mode7ZoomColorCount * sizeof(ushort));
            rows[row] = new PaletteRgb5[CeresRidleyPaletteRomData.Mode7ZoomColorCount];
            for (int color = 0; color < rows[row].Length; color++)
            {
                ushort word = (ushort)(bytes[color * 2] | bytes[color * 2 + 1] << 8);
                rows[row][color] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
        }
        return CeresRidleyMode7ColorCatalog.Write(new CeresRidleyMode7ColorDocument
        {
            Version = CeresRidleyMode7ColorFormat.Version,
            ZoomRows = rows,
        });
    }
}
