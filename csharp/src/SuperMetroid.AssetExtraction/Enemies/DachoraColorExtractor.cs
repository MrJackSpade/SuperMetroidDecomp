using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Extracts Dachora's contiguous default, speed and shine RGB5 frames.</summary>
public static class DachoraColorExtractor
{
    /// <summary>Imports Dachora's default palette and ordered speed and shine frames as editable RGB5 colors.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the native Dachora palette tables.</param>
    /// <returns>New UTF-8 JSON bytes with the fixed frame dimensions and red, green, and blue channels in the range 0..31.</returns>
    /// <remarks>Frame selection and animation cadence remain runtime behavior, not editable palette data.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A native color uses bit 15, which the RGB5 document cannot represent.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return DachoraColorFile.Write(new DachoraColorDocument
        {
            Version = DachoraColorFormat.Version,
            Normal = Read(EnemySpeciesSourceDefinitions.Dachora.DefaultColors),
            Speed = ReadFrames(EnemySpeciesSourceDefinitions.Dachora.SpeedColors),
            Shine = ReadFrames(EnemySpeciesSourceDefinitions.Dachora.ShineColors),
        });

        PaletteRgb5[][] ReadFrames(int source)
        {
            var frames = new PaletteRgb5[DachoraColorDefinitions.AnimatedFrameCount][];
            for (int frame = 0; frame < frames.Length; frame++)
                frames[frame] = Read(source + frame * EnemySpeciesSourceDefinitions.Dachora.ImageBytes);
            return frames;
        }

        PaletteRgb5[] Read(int source)
        {
            var colors = new PaletteRgb5[DachoraColorDefinitions.ColorsPerFrame];
            for (int color = 0; color < colors.Length; color++)
            {
                int address = source + color * EnemySpeciesSourceDefinitions.ColorWordBytes;
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(native))
                    throw new InvalidDataException(
                        $"Dachora color ${address:X6} has an unrepresentable high bit.");
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
