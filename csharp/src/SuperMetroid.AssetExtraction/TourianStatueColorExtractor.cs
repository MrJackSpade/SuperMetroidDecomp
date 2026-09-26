using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only the authored palette images, not statue unlock conditions or animation programs.</summary>
public static class TourianStatueColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var eye = new PaletteRgb5[TourianStatuePaletteRomData.EyeRowCount][];
        for (int row = 0; row < eye.Length; row++)
            eye[row] = Read(bus,
                TourianStatuePaletteRomData.EyeColors + row *
                    TourianStatuePaletteRomData.EyeColorCount * sizeof(ushort),
                TourianStatuePaletteRomData.EyeColorCount);
        return TourianStatueColorCatalog.Write(new TourianStatueColorDocument
        {
            Version = TourianStatueColorFormat.Version,
            Base = Read(bus, TourianStatuePaletteRomData.BaseColors,
                TourianStatuePaletteRomData.BaseColorCount),
            Statue = Read(bus, TourianStatuePaletteRomData.StatueColors,
                TourianStatuePaletteRomData.StatueColorCount),
            Eye = eye,
            Grey = Read(bus, TourianStatuePaletteRomData.GreyColors,
                TourianStatuePaletteRomData.GreyColorCount),
        });
    }

    private static PaletteRgb5[] Read(ISnesAddressSpace bus, int source, int count)
    {
        var result = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            int address = source + color * sizeof(ushort);
            ushort word = RomDataReader.ReadWordFixedBank(bus, address);
            if ((word & 0x8000) != 0)
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
