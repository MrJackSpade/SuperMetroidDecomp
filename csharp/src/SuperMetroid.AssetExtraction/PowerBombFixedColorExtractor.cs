using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports native RGB5 colors without exporting radius or HDMA mechanics.</summary>
public static class PowerBombFixedColorExtractor
{
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

    private static PaletteRgb5[] Read(ISnesAddressSpace bus, PowerBombFixedColorSequence sequence)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus,
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
