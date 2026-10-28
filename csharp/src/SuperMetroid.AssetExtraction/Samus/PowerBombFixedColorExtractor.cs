using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports native RGB5 colors without exporting radius or HDMA mechanics.</summary>
public static class PowerBombFixedColorExtractor
{
    /// <summary>Imports the sixteen pre-explosion and thirty-two explosion fixed-color triplets used by Power Bomb effects.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the native three-byte fixed-color sequences.</param>
    /// <returns>New UTF-8 JSON bytes containing ordered RGB5 colors, with each native component masked to 0..31.</returns>
    /// <remarks>Radius-based selection, HDMA geometry, and shared Crystal Flash or Ceres explosion behavior remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return PowerBombFixedColorFile.Write(new PowerBombFixedColorDocument
        {
            Version = PowerBombFixedColorFormat.Version,
            PreExplosion = Read(bus, PowerBombFixedColorSequence.PreExplosion),
            Explosion = Read(bus, PowerBombFixedColorSequence.Explosion),
        });
    }

    /// <summary>Decodes one fixed-color sequence from native three-byte RGB triplets into five-bit channels.</summary>
    private static PaletteRgb5[] Read(ISnesAddressSpace bus, PowerBombFixedColorSequence sequence)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            PowerBombColorSources.Start(sequence),
            PowerBombFixedColorFormat.Count(sequence) * SamusSourceDefinitions.PowerBombBytesPerColor);
        var colors = new PaletteRgb5[PowerBombFixedColorFormat.Count(sequence)];
        for (int index = 0; index < colors.Length; index++)
        {
            int offset = index * SamusSourceDefinitions.PowerBombBytesPerColor;
            colors[index] = new PaletteRgb5
            {
                Red = source[offset] & SamusPaletteDefinitions.PowerBomb.ComponentMask,
                Green = source[offset + 1] & SamusPaletteDefinitions.PowerBomb.ComponentMask,
                Blue = source[offset + 2] & SamusPaletteDefinitions.PowerBomb.ComponentMask,
            };
        }
        return colors;
    }
}
