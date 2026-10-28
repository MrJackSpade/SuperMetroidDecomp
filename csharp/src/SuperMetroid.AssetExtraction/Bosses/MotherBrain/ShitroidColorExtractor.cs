using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Exports the live Shitroid's normal cycle and three initialization targets.</summary>
public static class ShitroidColorExtractor
{
    /// <summary>Exports the live Shitroid encounter's eight four-color innard-pulse images and three complete initialization/transition target palettes.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the normal cycle at $A9:F6D1 and dead-sidehopper, sidehopper, and Shitroid targets at $A9:F8A6/F8C6/F8E6.</param>
    /// <returns>A new UTF-8 JSON buffer with eight four-color Normal rows and three named sixteen-color targets, including transparent-slot payloads; RGB5 channels are 0..31.</returns>
    /// <remarks>The four-color pulse is distinct from complete target rows and later Baby Metroid cutscene artwork. Palette-update cadence, cries, encounter state, and target-color transition mechanics are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A source color sets bit 15, which the RGB5 document cannot represent.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var normal = new PaletteRgb5[ShitroidColorDefinitions.NormalFrameCount][];
        for (int frame = 0; frame < normal.Length; frame++)
            normal[frame] = Read(MotherBrainSourceDefinitions.Shitroid.NormalCycle +
                frame * ShitroidColorDefinitions.NormalColorsPerFrame * MotherBrainSourceDefinitions.ColorWordBytes,
                ShitroidColorDefinitions.NormalColorsPerFrame);
        return ShitroidColorFile.Write(new ShitroidColorDocument
        {
            Version = ShitroidColorFormat.Version,
            Normal = normal,
            Sidehopper = Read(MotherBrainSourceDefinitions.Shitroid.SidehopperTarget,
                ShitroidColorDefinitions.TargetColorCount),
            Shitroid = Read(MotherBrainSourceDefinitions.Shitroid.ShitroidTarget,
                ShitroidColorDefinitions.TargetColorCount),
            DeadSidehopper = Read(MotherBrainSourceDefinitions.Shitroid.DeadSidehopperTarget,
                ShitroidColorDefinitions.TargetColorCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(native))
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
