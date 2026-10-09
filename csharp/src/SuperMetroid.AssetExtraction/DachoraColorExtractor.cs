using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

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
        ArgumentNullException.ThrowIfNull(bus);
        return DachoraColorCatalog.Write(new DachoraColorDocument
        {
            Version = DachoraColorFormat.Version,
            Normal = Read(DachoraColorRomData.DefaultSource),
            Speed = ReadFrames(DachoraColorRomData.SpeedSource),
            Shine = ReadFrames(DachoraColorRomData.ShineSource),
        });

        PaletteRgb5[][] ReadFrames(int source)
        {
            var frames = new PaletteRgb5[DachoraColorRomData.AnimatedFrameCount][];
            for (int frame = 0; frame < frames.Length; frame++)
                frames[frame] = Read(source + frame * DachoraColorRomData.FrameByteCount);
            return frames;
        }

        PaletteRgb5[] Read(int source)
        {
            var colors = new PaletteRgb5[DachoraColorRomData.ColorsPerFrame];
            for (int color = 0; color < colors.Length; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if ((native & 0x8000) != 0)
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
