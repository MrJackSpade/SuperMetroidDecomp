using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads the four bank-$AD pointer-selected damage palette pairs once at installation.</summary>
internal static class MotherBrainHealthPaletteExtractor
{
    /// <summary>Follows body and back-leg state pointers, decodes each BGR555 color, and writes the installed JSON document.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        using var json = new MemoryStream();
        MotherBrainHealthPaletteFile.Write(json, new MotherBrainHealthPaletteDocument
        {
            Version = MotherBrainHealthPaletteFile.Version,
            HealthBands = MotherBrainSourceDefinitions.Palettes.HealthTableOrder.Select((band, state) => KeyValuePair.Create(band.Key,
                new MotherBrainBodyColoursDocument
                {
                    Body = ReadState(MotherBrainSourceDefinitions.Palettes.HealthBodyPointerTable, state),
                    BackLegs = ReadState(MotherBrainSourceDefinitions.Palettes.HealthRearLegPointerTable, state),
                })).ToDictionary(StringComparer.Ordinal),
        });
        return json.ToArray();

        PaletteRgb5[] ReadState(int table, int state)
        {
            int pointer = table + state * sizeof(ushort);
            int source = (MotherBrainSourceDefinitions.Palettes.PaletteBank|CartridgeImportSource.Require(bus).ReadCartridgeWord(pointer));
            var colours = new PaletteRgb5[MotherBrainRainbowPaletteGeometry.ColorCount];
            for (int color = 0; color < colours.Length; color++)
            {
                int address = source + color * sizeof(ushort);
                int word = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
                colours[color] = PaletteRgb5.From(RgbColor.FromWordIgnoringTopBit(word));
            }
            return colours;
        }
    }
}
