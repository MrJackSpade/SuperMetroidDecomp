using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Import-time expansion of the title's native direct-mode HDMA tables.</summary>
[CpuBusBoundary("codec: ROM title direct-mode HDMA gradient tables → TitleGradientLine[]")]
public static class TitleGradientDecoder
{
    /// <summary>Expands the selected title gradient's fixed-color and control HDMA tables into one descriptor per scanline.</summary>
    /// <param name="cartridge">Cartridge source containing the native title HDMA tables.</param>
    /// <param name="zoom">Native zoom value whose low-byte bits 4 through 7 select the fixed-color table.</param>
    /// <returns>The decoded title gradient scanlines.</returns>
    public static TitleGradientLine[] Decode(IImportCartridgeSource cartridge, ushort zoom)
    {
        int pointer = FrontendCartridgeTables.FixedColorPointers + (zoom >> 4) % 16 * sizeof(ushort);
        int table = TitleGradientRomData.FixedColorBank |
            cartridge.ReadCartridgeWord(pointer);
        byte[] fixedWrites = Expand(cartridge, table);
        byte[] control = Expand(cartridge, TitleGradientRomData.ControlTable);
        var result = new TitleGradientLine[SnesPpuLayout.ScreenHeightPixels];
        byte red = 0, green = 0, blue = 0;
        for (int line = 0; line < result.Length; line++)
        {
            byte write = fixedWrites[line], component = (byte)(write & 31);
            if ((write & 0x20) != 0) red = component; // cartridge-encoding: allow(Register) - COLDATA red-select bit
            if ((write & 0x40) != 0) green = component; // cartridge-encoding: allow(Register) - COLDATA green-select bit
            if ((write & 0x80) != 0) blue = component; // cartridge-encoding: allow(Register) - COLDATA blue-select bit
            result[line] = new(red, green, blue, (ColorMathControlFlags)control[line]);
        }
        return result;
    }

    /// <summary>Expands one direct-mode HDMA table, retaining the last write after its zero terminator.</summary>
    private static byte[] Expand(IImportCartridgeSource cartridge, int cursor)
    {
        var result = new byte[SnesPpuLayout.ScreenHeightPixels];
        int line = 0;
        byte previous = 0;
        while (line < result.Length)
        {
            byte header = cartridge.ReadCartridgeByte(cursor++);
            if (header == 0)
            {
                result.AsSpan(line).Fill(previous);
                break;
            }
            int count = (header & 127) == 0 ? 128 : header & 127;
            bool transferEveryLine = (header & 128) != 0;
            if (!transferEveryLine) previous = cartridge.ReadCartridgeByte(cursor++);
            for (int tick = 0; tick < count && line < result.Length; tick++)
            {
                if (transferEveryLine) previous = cartridge.ReadCartridgeByte(cursor++);
                result[line++] = previous;
            }
        }
        return result;
    }
}
