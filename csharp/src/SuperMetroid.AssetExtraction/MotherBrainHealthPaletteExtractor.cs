using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads the four bank-$AD pointer-selected damage palette pairs once at installation.</summary>
internal static class MotherBrainHealthPaletteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        using var json = new MemoryStream();
        MotherBrainHealthPalettePresentation.Write(json, new MotherBrainHealthPaletteDocument
        {
            Version = MotherBrainHealthPaletteFormat.Version,
            Body = ReadTable(MotherBrainHealthPaletteRomData.BrainTable),
            BackLegs = ReadTable(MotherBrainHealthPaletteRomData.BackLegTable),
        });
        return json.ToArray();

        PaletteRgb5[][] ReadTable(int table)
        {
            var states = new PaletteRgb5[MotherBrainHealthPaletteFormat.StateCount][];
            for (int state = 0; state < states.Length; state++)
            {
                int pointer = table + state * sizeof(ushort);
                int source = MotherBrainRainbowPaletteRomData.SourceBank |
                    bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8;
                states[state] = new PaletteRgb5[MotherBrainRainbowPaletteRomData.ColorCount];
                for (int color = 0; color < states[state].Length; color++)
                {
                    int address = source + color * sizeof(ushort);
                    ushort word = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                    states[state][color] = new PaletteRgb5
                    {
                        Red = word & 31,
                        Green = word >> 5 & 31,
                        Blue = word >> 10 & 31,
                    };
                }
            }
            return states;
        }
    }
}
