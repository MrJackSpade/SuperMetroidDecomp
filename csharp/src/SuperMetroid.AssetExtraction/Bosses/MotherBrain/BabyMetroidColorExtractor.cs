using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Exports the live Baby Metroid's normal cycle and three initialization targets.</summary>
public static partial class BabyMetroidColorExtractor
{
    /// <summary>Exports the live Baby Metroid encounter's eight four-color innard-pulse images and three complete initialization/transition target palettes.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the normal cycle at $A9:F6D1 and dead-sidehopper, sidehopper, and Baby Metroid targets at $A9:F8A6/F8C6/F8E6.</param>
    /// <returns>A new UTF-8 JSON buffer with eight four-color Normal rows and three named sixteen-color targets, including transparent-slot payloads; RGB5 channels are 0..31.</returns>
    /// <remarks>The four-color pulse is distinct from complete target rows and later Baby Metroid cutscene artwork. Palette-update cadence, cries, encounter state, and target-color transition mechanics are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A source color sets bit 15, which the RGB5 document cannot represent.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var normal = new PaletteRgb5[BabyMetroidColorDefinitions.NormalFrameCount][];
        for (int frame = 0; frame < normal.Length; frame++)
            normal[frame] = Read(MotherBrainSourceDefinitions.BabyMetroidEnemy.NormalCycle +
                frame * BabyMetroidColorDefinitions.NormalColorsPerFrame * MotherBrainSourceDefinitions.ColorWordBytes,
                BabyMetroidColorDefinitions.NormalColorsPerFrame);
        return BabyMetroidColorFile.Write(new BabyMetroidColorDocument
        {
            Version = BabyMetroidColorFormat.Version,
            Normal = normal,
            Sidehopper = Read(MotherBrainSourceDefinitions.BabyMetroidEnemy.SidehopperTarget,
                BabyMetroidColorDefinitions.TargetColorCount),
            BabyMetroid = Read(MotherBrainSourceDefinitions.BabyMetroidEnemy.BabyMetroidTarget,
                BabyMetroidColorDefinitions.TargetColorCount),
            DeadSidehopper = Read(MotherBrainSourceDefinitions.BabyMetroidEnemy.DeadSidehopperTarget,
                BabyMetroidColorDefinitions.TargetColorCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                int native = RomDataReader.ReadWordFixedBank(bus, address);
                if (!RgbColor.IsColorWord(checked((ushort)native)))
                    throw new InvalidDataException(
                        $"Shitroid color ${address:X6} has an unrepresentable high bit.");
                colors[color] = PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(native));
            }
            return colors;
        }
    }
}
