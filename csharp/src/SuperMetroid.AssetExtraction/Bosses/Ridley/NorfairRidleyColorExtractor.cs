using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Copies Ridley's authored RGB5 images while retaining the native reveal pointer order.</summary>
[CpuBusBoundary("codec: ROM bank-$A6 Norfair Ridley initial and reveal colors → NorfairRidleyColorCatalog")]
public static class NorfairRidleyColorExtractor
{
    /// <summary>Reads Ridley's initial palette and zero-terminated native reveal-pointer sequence, preserving authored row order and rejecting invalid pointers or high-bit colors.</summary>
    /// <param name="bus">Supported-cartridge address space containing the Norfair Ridley palette data and reveal pointer table.</param>
    /// <returns>UTF-8 JSON bytes for the versioned Norfair Ridley color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var reveal = new PaletteRgb5[NorfairRidleyPaletteRomData.RevealRowCount][];
        for (int row = 0; row < reveal.Length; row++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                NorfairRidleyPaletteRomData.RevealSourcePointers + row * sizeof(ushort));
            if (pointer < SnesBusLayout.RomWindowStart)
                throw new InvalidDataException($"Norfair Ridley reveal row {row} has invalid source ${pointer:X4}.");
            reveal[row] = ReadColors(bus, CartridgeBankWindow.At(EnemyCartridgeTables.RidleyBank, pointer),
                NorfairRidleyPaletteRomData.RevealColorCount);
        }
        ushort terminator = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
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

    /// <summary>Decodes contiguous SNES BGR555 words into RGB5 channels and rejects bit fifteen, which is outside palette color data.</summary>
    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int source, int count)
    {
        var colors = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            int address = source + color * sizeof(ushort);
            ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            if (!Bgr555.IsColorWord(word))
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
