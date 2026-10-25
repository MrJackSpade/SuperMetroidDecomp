using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Exports complete Mother Brain death-fade and exploded-door RGB5 images.</summary>
[CpuBusBoundary("codec: ROM Mother Brain death-fade and exploded-door colors → MotherBrainDeathColorCatalog")]
public static class MotherBrainDeathColorExtractor
{
    /// <summary>Reads every Mother Brain body, leg, and corpse death-fade row plus the exploded-door palette, rejects high-bit colors, and serializes RGB5 data.</summary>
    /// <param name="bus">Supported-cartridge address space containing the death-fade and door palette tables.</param>
    /// <returns>UTF-8 JSON bytes for the complete versioned Mother Brain death-color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var body = new PaletteRgb5[MotherBrainDeathDefinitions.BodyFadeFrameCount][];
        var leg = new PaletteRgb5[body.Length][];
        for (int frame = 0; frame < body.Length; frame++)
        {
            int source = MotherBrainSourceDefinitions.Death.BodyFadeColors(frame);
            body[frame] = Read(source, MotherBrainDeathDefinitions.BodyColorCount);
            leg[frame] = Read(source + MotherBrainDeathDefinitions.BodyColorCount *
                sizeof(ushort), MotherBrainDeathDefinitions.BodyColorCount);
        }
        var corpse = new PaletteRgb5[MotherBrainDeathDefinitions.CorpseFadeFrameCount][];
        for (int frame = 0; frame < corpse.Length; frame++)
            corpse[frame] = Read(MotherBrainSourceDefinitions.Death.CorpseFadeColors(frame),
                MotherBrainDeathDefinitions.CorpseColorCount);
        return MotherBrainDeathColorFile.Write(new MotherBrainDeathColorDocument
        {
            Version = MotherBrainDeathColorFormat.Version,
            BodyFade = body,
            LegFade = leg,
            CorpseFade = corpse,
            ExplodedDoor = Read(MotherBrainSourceDefinitions.Death.ExplodedDoorColors,
                MotherBrainDeathDefinitions.BodyColorCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!Bgr555.IsColorWord(native))
                    throw new InvalidDataException(
                        $"Mother Brain death color ${address:X6} has an unrepresentable high bit.");
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
