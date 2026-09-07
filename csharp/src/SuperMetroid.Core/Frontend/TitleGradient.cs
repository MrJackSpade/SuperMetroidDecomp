using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Literal five-bit fixed color and CGADSUB control for a physical title scanline.</summary>
public readonly record struct TitleGradientLine(byte Red, byte Green, byte Blue, byte Control);

/// <summary>Expands the two native direct-mode HDMA tables installed by $88:EB58.</summary>
public static class TitleGradient
{
    public static TitleGradientLine[] Decode(ISnesAddressSpace bus, ushort zoom)
    {
        int pointer = TitleGradientRomData.FixedColorPointers + ((zoom & 0xf0) >> 3);
        int table = TitleGradientRomData.FixedColorBank |
            (bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8);
        byte[] fixedWrites = Expand(bus, table);
        byte[] control = Expand(bus, TitleGradientRomData.ControlTable);
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

    private static byte[] Expand(ISnesAddressSpace bus, int cursor)
    {
        var result = new byte[SnesPpuLayout.ScreenHeightPixels];
        int line = 0;
        byte previous = 0;
        while (line < result.Length)
        {
            byte header = bus.ReadByte(cursor++);
            if (header == 0)
            {
                result.AsSpan(line).Fill(previous);
                break;
            }
            int count = (header & 127) == 0 ? 128 : header & 127;
            bool transferEveryLine = (header & 128) != 0;
            if (!transferEveryLine) previous = bus.ReadByte(cursor++);
            for (int tick = 0; tick < count && line < result.Length; tick++)
            {
                if (transferEveryLine) previous = bus.ReadByte(cursor++);
                result[line++] = previous;
            }
        }
        return result;
    }
}
