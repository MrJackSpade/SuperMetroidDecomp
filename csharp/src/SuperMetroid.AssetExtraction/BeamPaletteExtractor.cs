using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts ordinary beam color selections from the supported cartridge, not charge animation rules.</summary>
public static class BeamPaletteExtractor
{
    /// <summary>Follows each legal native beam-selection palette pointer and serializes its complete RGB5 color row without charge-animation rules.</summary>
    /// <param name="bus">Supported-cartridge address space containing the beam palette pointer table and color words.</param>
    /// <returns>UTF-8 JSON bytes keyed by the compiled beam-selection identities.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var palettes = new Dictionary<string, PaletteRgb5[]>();
        for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Beams.PalettePointers + selection * 2);
            var colors = new PaletteRgb5[BeamPaletteDefinitions.ColorCount];
            for (int i = 0; i < colors.Length; i++)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Banks.Movement | (pointer + i * 2));
                colors[i] = new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }
            palettes.Add(BeamPaletteDefinitions.Key(selection), colors);
        }
        return BeamPaletteCatalog.Write(new() { Version = BeamPaletteDefinitions.Version, Palettes = palettes });
    }
}
