using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Extracts all three bank-$AA Chozo/tube target sprite-palette pairs.</summary>
public static class ChozoAndTubeColorExtractor
{
    /// <summary>Reads the tube-crack, Wrecked Ship, and Lower Norfair native target palettes, rejects high-bit colors, and serializes their RGB5 components.</summary>
    /// <param name="bus">Supported-cartridge address space containing the three bank-$AA palette pairs.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Chozo and tube color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return ChozoAndTubeColorFile.Write(new ChozoAndTubeColorDocument
        {
            Version = ChozoAndTubeColorFormat.Version,
            TubeCracks = Read(EnemySpeciesSourceDefinitions.ChozoAndTube.TubeCracksColors),
            WreckedShip = Read(EnemySpeciesSourceDefinitions.ChozoAndTube.WreckedShipColors),
            LowerNorfair = Read(EnemySpeciesSourceDefinitions.ChozoAndTube.LowerNorfairColors),
        });

        PaletteRgb5[] Read(int source)
        {
            var colors = new PaletteRgb5[ChozoAndTubeColorDefinitions.ColorCount];
            for (int color = 0; color < colors.Length; color++)
            {
                int address = source + color * EnemySpeciesSourceDefinitions.ColorWordBytes;
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(native))
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
