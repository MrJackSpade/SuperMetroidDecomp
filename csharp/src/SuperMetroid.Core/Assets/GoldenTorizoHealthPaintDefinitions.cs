namespace SuperMetroid.Core.Assets;

/// <summary>Exact $84:8032-8231 Golden Torizo health paint composition. Reviewed categorical endpoint
/// channels and material lighting specify this painted transition; health thresholds, mechanics, timing and pixels are excluded.</summary>
internal static class GoldenTorizoHealthPaintDefinitions
{
    /// <summary>$84:8034-8042 / $AA:8709-8717: eight low-health armor red levels.</summary>
    private const int LowHighlight = 26, LowEdge = 18, LowRecess = 7, LowOutline = 3,
        LowPlateLight = 21, LowPlateMiddle = 16, LowPlateShade = 11, LowPlateDark = 8;
    /// <summary>$84:8034-8042: low armor green and blue deficits; highlight shares green, middle blue selects13.</summary>
    private const int LowGreenDrop = 5, LowBlueDrop = 2, LowMiddleBlue = 13;
    /// <summary>$84:8044-8048: chosen purple core paints.</summary>
    private const ushort LowCoreLight = 0x6f7f, LowCoreMiddle = 0x51f8, LowCoreDark = 0x410e;
    /// <summary>$84:804A-804E: warm eye red shade step and two selected green levels; dark green halves middle.</summary>
    private const int LowEyeRedStep = 5, LowEyeGreenLight = 24, LowEyeGreenMiddle = 14;
    /// <summary>$84:8050: neutral low-health contour intensity.</summary>
    private const int LowNeutral = 3;
    /// <summary>$84:8114-8122 / $AA:8789-8797: golden armor channel choices.</summary>
    private const int GoldHighlightRed = 30, GoldHighlightBlue = 18, GoldRecessRed = 8,
        GoldPlateLightRed = 26, GoldPlateLightBlue = 5, GoldPlateMiddleRed = 22, GoldPlateDarkRed = 13,
        GoldLightGreenDrop = 1, GoldShadeGreenDrop = 3;
    /// <summary>$84:8116: independent golden edge paint.</summary>
    private const ushort GoldEdge = 0x06b9;
    /// <summary>$84:8124-812E: clipped cyan core/eye shade policies, not a historical generator claim.</summary>
    private const int CyanGreenHeadroom = 2, CoreGreenStep = 8, CoreBlueLight = 28,
        CoreBlueStep = 9, EyeGreenStep = 12, EyeBlueShade = 22;
    /// <summary>$84:8130: golden contour's red/green; blue is zero.</summary>
    private const int GoldContourRed = 3, GoldContourGreen = 2;
    /// <summary>$84:8134-8150/8214-8230: rear material lighting reductions and the selected low rear dark-facet blue channel.</summary>
    private const int RearShade = 5, RearCoreShade = 10, LowRearPlateBlue = 3;
    /// <summary>$84:8032/8112/8132/8212: copied transparent word1000, except final front band0000.</summary>
    private const ushort TransparentPayload = 0x1000;
    /// <summary>$84:8005-8016: eight native health-selected palette bands; interpolation spans seven intervals.</summary>
    private const int Intervals = 7;

    internal static bool Matches(ushort[][] rows, bool rear)
    {
        for (int frame = 0; frame <= Intervals; frame++)
        for (int color = 0; color < 16; color++)
            if (rows[frame][color] != Color(frame, color, rear)) return false;
        return true;
    }

    internal static ushort Color(int frame, int color, bool rear)
    {
        if ((uint)frame > Intervals || (uint)color >= 16) throw new ArgumentOutOfRangeException(nameof(frame));
        if (color == 0) return !rear && frame == Intervals ? (ushort)0 : TransparentPayload;
        ushort first = Endpoint(false, color, rear), last = Endpoint(true, color, rear);
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((first >> shift & 31) * (Intervals - frame) +
                (last >> shift & 31) * frame + Intervals / 2) / Intervals) << shift;
        return (ushort)result;
    }

    private static ushort Endpoint(bool gold, int color, bool rear)
    {
        ushort front = gold ? Gold(color) : Low(color);
        if (!rear) return front;
        if (!gold && color == 15) return front;
        if (!gold && color is >= 12 and <= 14)
            return Adjust(Low(color - 3), RearShade, 0, 0);
        ushort result = color is >= 9 and <= 11
            ? Adjust(front, -RearShade, -RearCoreShade, -RearCoreShade)
            : Adjust(front, -RearShade, -RearShade, -RearShade);
        if (!gold && color == 7) result = (ushort)((result & 0x03ff) | LowRearPlateBlue << 10);
        if (gold && color == 9) result = (ushort)((result & 0x7c1f) | Math.Max(0, (front >> 5 & 31) - RearShade) << 5);
        return result;
    }

    private static ushort Low(int color)
    {
        if (color <= 8)
        {
            int red = color switch
            {
                1 => LowHighlight, 2 => LowEdge, 3 => LowRecess, 4 => LowOutline,
                5 => LowPlateLight, 6 => LowPlateMiddle, 7 => LowPlateShade, 8 => LowPlateDark,
                _ => throw new ArgumentOutOfRangeException(nameof(color)),
            };
            int green = Math.Max(0, red - LowGreenDrop);
            return Pack(red, green, color == 1 ? green : color == 6 ? LowMiddleBlue : Math.Max(0, red - LowBlueDrop));
        }
        return color switch
        {
            9 => LowCoreLight, 10 => LowCoreMiddle, 11 => LowCoreDark,
            12 => Pack(31, LowEyeGreenLight, 0),
            13 => Pack(31 - LowEyeRedStep, LowEyeGreenMiddle, 0),
            14 => Pack(31 - 2 * LowEyeRedStep, LowEyeGreenMiddle / 2, 0),
            15 => Pack(LowNeutral, LowNeutral, LowNeutral),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    private static ushort Gold(int color)
    {
        if (color is >= 9 and <= 11)
        {
            int shade = color - 9;
            return Pack(0, Math.Min(31, 31 + CyanGreenHeadroom - CoreGreenStep * shade), CoreBlueLight - CoreBlueStep * shade);
        }
        if (color is >= 12 and <= 14)
            return Pack(0, Math.Min(31, 31 + CyanGreenHeadroom - EyeGreenStep * (color - 12)), color == 12 ? 31 : EyeBlueShade);
        return color switch
        {
            1 => Pack(GoldHighlightRed, GoldHighlightRed - GoldLightGreenDrop, GoldHighlightBlue),
            2 => GoldEdge,
            3 => Pack(GoldRecessRed, GoldRecessRed - GoldShadeGreenDrop, 0),
            4 => 0,
            5 => Pack(GoldPlateLightRed, GoldPlateLightRed - GoldLightGreenDrop, GoldPlateLightBlue),
            6 => Pack(GoldPlateMiddleRed, GoldPlateMiddleRed - GoldShadeGreenDrop, 0),
            7 => Pack((GoldPlateMiddleRed + GoldPlateDarkRed + 1) / 2,
                (GoldPlateMiddleRed + GoldPlateDarkRed - 2 * GoldShadeGreenDrop + 1) / 2, 0),
            8 => Pack(GoldPlateDarkRed, GoldPlateDarkRed - GoldShadeGreenDrop, 0),
            15 => Pack(GoldContourRed, GoldContourGreen, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    private static ushort Adjust(ushort color, int red, int green, int blue) =>
        Pack(Math.Clamp((color & 31) + red, 0, 31), Math.Clamp((color >> 5 & 31) + green, 0, 31), Math.Clamp((color >> 10 & 31) + blue, 0, 31));
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
