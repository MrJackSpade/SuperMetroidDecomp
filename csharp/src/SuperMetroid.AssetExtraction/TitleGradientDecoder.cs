using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Import-time expansion of the title's native direct-mode HDMA tables.</summary>
public static class TitleGradientDecoder
{
    /// <summary>Expands the selected title gradient's fixed-color and control HDMA tables into one descriptor per scanline.</summary>
    /// <param name="cartridge">Cartridge source containing the native title HDMA tables.</param>
    /// <param name="zoom">Native zoom value whose low-byte bits 4 through 7 select the fixed-color table.</param>
    /// <returns>The decoded title gradient scanlines.</returns>
    public static TitleGradientLine[] Decode(IImportCartridgeSource cartridge, ushort zoom)
    {
        int pointer = TitleGradientRomData.FixedColorPointers + ((zoom & 0xf0) >> 3);
        int table = TitleGradientRomData.FixedColorBank |
            (cartridge.ReadCartridgeByte(pointer) | cartridge.ReadCartridgeByte(pointer + 1) << 8);
        byte[] fixedWrites = Expand(cartridge, table);
        byte[] control = Expand(cartridge, TitleGradientRomData.ControlTable);
        var result = new TitleGradientLine[SnesPpuLayout.ScreenHeightPixels];
        byte red = 0, green = 0, blue = 0;
        for (int line = 0; line < result.Length; line++)
        {
            byte write = fixedWrites[line], component = (byte)(write & 31);
            if ((write & 0x20) != 0) red = component;
            if ((write & 0x40) != 0) green = component;
            if ((write & 0x80) != 0) blue = component;
            result[line] = new(red, green, blue, control[line]);
        }
        return result;
    }

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
