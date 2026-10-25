using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads the four bank-$AD pointer-selected damage palette pairs once at installation.</summary>
[CpuBusBoundary("codec: ROM bank-$AD damage palette pointer pairs → Mother Brain health palettes")]
internal static class MotherBrainHealthPaletteExtractor
{
    /// <summary>Follows body and back-leg state pointers, decodes each BGR555 color, and writes the installed JSON document.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        using var json = new MemoryStream();
        MotherBrainHealthPaletteFile.Write(json, new MotherBrainHealthPaletteDocument
        {
            Version = MotherBrainHealthPaletteFormat.Version,
            Body = ReadTable(MotherBrainSourceDefinitions.Palettes.HealthBodyPointerTable),
            BackLegs = ReadTable(MotherBrainSourceDefinitions.Palettes.HealthRearLegPointerTable),
        });
        return json.ToArray();

        PaletteRgb5[][] ReadTable(int table)
        {
            var states = new PaletteRgb5[MotherBrainHealthPaletteFormat.StateCount][];
            for (int state = 0; state < states.Length; state++)
            {
                int pointer = table + state * sizeof(ushort);
                int source = (MotherBrainSourceDefinitions.Palettes.PaletteBank|CartridgeImportSource.Require(bus).ReadCartridgeWord(pointer));
                states[state] = new PaletteRgb5[MotherBrainRainbowPaletteDefinitions.ColorCount];
                for (int color = 0; color < states[state].Length; color++)
                {
                    int address = source + color * sizeof(ushort);
                    ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
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
