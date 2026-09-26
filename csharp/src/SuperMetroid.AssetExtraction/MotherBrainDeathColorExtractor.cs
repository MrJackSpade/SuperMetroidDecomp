using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports complete Mother Brain death-fade and exploded-door RGB5 images.</summary>
public static class MotherBrainDeathColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var body = new PaletteRgb5[MotherBrainDeathRomData.BodyFadeFrameCount][];
        var leg = new PaletteRgb5[body.Length][];
        for (int frame = 0; frame < body.Length; frame++)
        {
            int source = MotherBrainDeathRomData.BodyFadeSource(frame);
            body[frame] = Read(source, MotherBrainDeathRomData.BodyColorCount);
            leg[frame] = Read(source + MotherBrainDeathRomData.BodyColorCount *
                sizeof(ushort), MotherBrainDeathRomData.BodyColorCount);
        }
        var corpse = new PaletteRgb5[MotherBrainDeathRomData.CorpseFadeFrameCount][];
        for (int frame = 0; frame < corpse.Length; frame++)
            corpse[frame] = Read(MotherBrainDeathRomData.CorpseFadeSource(frame),
                MotherBrainDeathRomData.CorpseColorCount);
        return MotherBrainDeathColorCatalog.Write(new MotherBrainDeathColorDocument
        {
            Version = MotherBrainDeathColorFormat.Version,
            BodyFade = body,
            LegFade = leg,
            CorpseFade = corpse,
            ExplodedDoor = Read(MotherBrainDeathRomData.DoorPalette,
                MotherBrainDeathRomData.BodyColorCount),
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
