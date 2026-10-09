using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

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
        ArgumentNullException.ThrowIfNull(bus);
        return PowerBombFixedColorCatalog.Write(new PowerBombFixedColorDocument
        {
            Version = PowerBombFixedColorFormat.Version,
            PreExplosion = Read(bus, PowerBombFixedColorSequence.PreExplosion),
            Explosion = Read(bus, PowerBombFixedColorSequence.Explosion),
        });
    }

    /// <summary>Reads one power-bomb phase's byte-separated RGB component sequence.</summary>
    /// <param name="bus">Import address space containing the fixed-color bytes.</param>
    /// <param name="sequence">Pre-explosion or explosion sequence to select.</param>
    /// <returns>The authored RGB5 colors in playback order.</returns>
    private static PaletteRgb5[] Read(ISnesAddressSpace bus, PowerBombFixedColorSequence sequence)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            PowerBombFixedColorFormat.SourceAddress(sequence),
            PowerBombFixedColorFormat.Count(sequence) * SamusPaletteRomData.PowerBomb.BytesPerColor);
        var colors = new PaletteRgb5[PowerBombFixedColorFormat.Count(sequence)];
        for (int index = 0; index < colors.Length; index++)
        {
            int offset = index * SamusPaletteRomData.PowerBomb.BytesPerColor;
            colors[index] = new PaletteRgb5
            {
                Red = source[offset] & SamusPaletteRomData.PowerBomb.ComponentMask,
                Green = source[offset + 1] & SamusPaletteRomData.PowerBomb.ComponentMask,
                Blue = source[offset + 2] & SamusPaletteRomData.PowerBomb.ComponentMask,
            };
        }
        return colors;
    }
}
