using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only the authored palette images, not statue unlock conditions or animation programs.</summary>
public static class TourianStatueColorExtractor
{
    /// <summary>Exports Tourian entrance base/statue palettes, the four boss-specific eye-glow rows, and the grey-transition target band.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for base $AA:D785, statue $AA:D765, eyes $86:B91E, and grey colors $87:839C.</param>
    /// <returns>A new UTF-8 JSON buffer containing two complete sixteen-color rows, four four-color eye rows, and eight grey colors, expressed as RGB5 channels 0..31.</returns>
    /// <remarks>Eye row order is Phantoon, Ridley, Draygon, Kraid. Complete rows include color zero; the grey band's destination remains an animated-tile operand. Boss unlock conditions, soul/eye state, palette destinations, and timing are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A source color sets bit 15.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var eye = new PaletteRgb5[TourianStatueColorDefinitions.EyeRowCount][];
        for (int row = 0; row < eye.Length; row++)
            eye[row] = Read(bus,
                EnemySourceDefinitions.TourianStatueEyeColors + row *
                    TourianStatueColorDefinitions.EyeColorCount * sizeof(ushort),
                TourianStatueColorDefinitions.EyeColorCount);
        return TourianStatueColorFile.Write(new TourianStatueColorDocument
        {
            Version = TourianStatueColorFormat.Version,
            Base = Read(bus, EnemySourceDefinitions.TourianStatueBaseColors,
                TourianStatueColorDefinitions.BaseColorCount),
            Statue = Read(bus, EnemySourceDefinitions.TourianStatueColors,
                TourianStatueColorDefinitions.StatueColorCount),
            Eye = eye,
            Grey = Read(bus, EnemySourceDefinitions.TourianStatueGreyColors,
                TourianStatueColorDefinitions.GreyColorCount),
        });
    }

    /// <summary>Reads fixed-bank BGR555 words into RGB5 channels and rejects invalid high bits.</summary>
    private static PaletteRgb5[] Read(ISnesAddressSpace bus, int source, int count)
    {
        var result = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            int address = source + color * sizeof(ushort);
            ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            if (!RgbColor.IsColorWord(word))
                throw new InvalidDataException($"Tourian statue palette word ${address:X6} has bit 15 set.");
            result[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return result;
    }
}
