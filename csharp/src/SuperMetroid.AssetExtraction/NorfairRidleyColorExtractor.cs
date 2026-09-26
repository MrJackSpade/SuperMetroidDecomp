using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Copies Ridley's authored RGB5 images while retaining the native reveal pointer order.</summary>
public static class NorfairRidleyColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var reveal = new PaletteRgb5[NorfairRidleyPaletteRomData.RevealRowCount][];
        for (int row = 0; row < reveal.Length; row++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus,
                NorfairRidleyPaletteRomData.RevealSourcePointers + row * sizeof(ushort));
            if (pointer < 0x8000)
                throw new InvalidDataException($"Norfair Ridley reveal row {row} has invalid source ${pointer:X4}.");
            reveal[row] = ReadColors(bus, 0xa60000 | pointer,
                NorfairRidleyPaletteRomData.RevealColorCount);
        }
        ushort terminator = RomDataReader.ReadWordFixedBank(bus,
            NorfairRidleyPaletteRomData.RevealSourcePointers +
            reveal.Length * sizeof(ushort));
        if (terminator != 0)
            throw new InvalidDataException("Norfair Ridley reveal has no zero pointer terminator.");
        return NorfairRidleyColorCatalog.Write(new NorfairRidleyColorDocument
        {
            Version = NorfairRidleyColorFormat.Version,
            Initial = ReadColors(bus, NorfairRidleyPaletteRomData.InitialColors,
                NorfairRidleyPaletteRomData.InitialColorCount),
            Reveal = reveal,
        });
    }

    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int source, int count)
    {
        var colors = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            int address = source + color * sizeof(ushort);
            ushort word = RomDataReader.ReadWordFixedBank(bus, address);
            if ((word & 0x8000) != 0)
                throw new InvalidDataException($"Norfair Ridley color ${address:X6} has bit 15 set.");
            colors[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }
}
