using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the live Shitroid's normal cycle and three initialization targets.</summary>
public static class ShitroidColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var normal = new PaletteRgb5[ShitroidColorRomData.NormalFrameCount][];
        for (int frame = 0; frame < normal.Length; frame++)
            normal[frame] = Read(ShitroidColorRomData.NormalCycle +
                frame * ShitroidColorRomData.NormalColorsPerFrame * sizeof(ushort),
                ShitroidColorRomData.NormalColorsPerFrame);
        return ShitroidColorCatalog.Write(new ShitroidColorDocument
        {
            Version = ShitroidColorFormat.Version,
            Normal = normal,
            Sidehopper = Read(ShitroidColorRomData.SidehopperTarget,
                ShitroidColorRomData.TargetColorCount),
            Shitroid = Read(ShitroidColorRomData.ShitroidTarget,
                ShitroidColorRomData.TargetColorCount),
            DeadSidehopper = Read(ShitroidColorRomData.DeadSidehopperTarget,
                ShitroidColorRomData.TargetColorCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Shitroid color ${address:X6} has an unrepresentable high bit.");
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
