using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts all three bank-$AA Chozo/tube target sprite-palette pairs.</summary>
public static class ChozoAndTubeColorExtractor
{
    /// <summary>Reads the tube-crack, Wrecked Ship, and Lower Norfair native target palettes, rejects high-bit colors, and serializes their RGB5 components.</summary>
    /// <param name="bus">Supported-cartridge address space containing the three bank-$AA palette pairs.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Chozo and tube color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ChozoAndTubeColorCatalog.Write(new ChozoAndTubeColorDocument
        {
            Version = ChozoAndTubeColorFormat.Version,
            TubeCracks = Read(ChozoAndTubeColorRomData.TubeCracksSource),
            WreckedShip = Read(ChozoAndTubeColorRomData.WreckedShipSource),
            LowerNorfair = Read(ChozoAndTubeColorRomData.LowerNorfairSource),
        });

        PaletteRgb5[] Read(int source)
        {
            var colors = new PaletteRgb5[ChozoAndTubeColorRomData.ColorCount];
            for (int color = 0; color < colors.Length; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Chozo/tube color ${address:X6} has an unrepresentable high bit.");
                colors[color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
            return colors;
        }
    }
}
