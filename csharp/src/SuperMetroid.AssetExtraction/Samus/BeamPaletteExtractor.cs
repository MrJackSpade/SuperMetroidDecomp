using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts ordinary beam color selections from the supported cartridge, not charge animation rules.</summary>
[CpuBusBoundary("codec: ROM beam palette pointer and color tables → beam palette selections")]
public static class BeamPaletteExtractor
{
    /// <summary>Follows each legal native beam-selection palette pointer and serializes its complete RGB5 color row without charge-animation rules.</summary>
    /// <param name="bus">Supported-cartridge address space containing the beam palette pointer table and color words.</param>
    /// <returns>UTF-8 JSON bytes keyed by the compiled beam-selection identities.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var palettes = new Dictionary<string, PaletteRgb5[]>();
        for (int index = 0; index < BeamTileAtlasDefinitions.SelectionCount; index++)
        {
            SamusBeamCombination selection = SamusBeamCombinations.FromTableIndex(index);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.PalettePointers.Value + selection.TableIndex * 2);
            var colors = new PaletteRgb5[BeamPaletteDefinitions.ColorCount];
            for (int i = 0; i < colors.Length; i++)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.MovementBank.Value | (pointer + i * 2));
                colors[i] = PaletteRgb5.From(Bgr555.FromWord(word));
            }
            palettes.Add(BeamPaletteDefinitions.Key(selection), colors);
        }
        return BeamPaletteFile.Write(new() { Version = BeamPaletteDefinitions.Version, Palettes = palettes });
    }
}
