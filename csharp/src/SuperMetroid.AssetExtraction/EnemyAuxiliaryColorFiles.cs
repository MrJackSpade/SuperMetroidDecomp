using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Import-only conversion of the four bounded enemy color animations.</summary>
public static class EnemyAuxiliaryColorFiles
{
    public static byte[] Extract(ISnesAddressSpace source)
    {
        var palettes = new Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]>();
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        foreach (EnemyAuxiliaryPaletteDefinition definition in EnemyAuxiliaryColorDefinitions.All)
        {
            var rows = new PaletteRgb5[definition.FrameCount][];
            for (int frame = 0; frame < rows.Length; frame++)
            {
                rows[frame] = new PaletteRgb5[definition.ColorCount];
                for (int color = 0; color < rows[frame].Length; color++)
                {
                    ushort word = RomDataReader.ReadWordFixedBank(cartridge, definition.SourceAddress +
                        (frame * definition.NativeFrameStrideColors + color) * sizeof(ushort));
                    rows[frame][color] = new PaletteRgb5
                    {
                        Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31,
                    };
                }
            }
            palettes.Add(definition.Id, rows);
        }
        return EnemyAuxiliaryColorCatalog.Write(new EnemyAuxiliaryColorDocument
        {
            Version = EnemyAuxiliaryColorFormat.Version, Palettes = palettes,
        });
    }
}
