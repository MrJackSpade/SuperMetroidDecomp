namespace SuperMetroid.Core.Assets;

/// <summary>Exact $84:8032-8231 Golden Torizo health paint composition. Reviewed categorical endpoint
/// channels and material lighting specify this painted transition; health thresholds, mechanics, timing and pixels are excluded.</summary>
internal static class GoldenTorizoHealthPaintDefinitions
{
    /// <summary>$84:8034-8042 / $AA:8709-8717: eight low-health armor red levels.</summary>
    private const int LowHighlight = 26, LowEdge = 18, LowRecess = 7, LowOutline = 3,
        LowPlateLight = 21, LowPlateDark = 8;
    /// <summary>$84:803C-8042: selected five-level armor shade step, clipped at the dark facet.</summary>
    private const int LowPlateShadeStep = 5;
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

    /// <summary>Compares all eight 16-color palette bands with the authored Golden Torizo paint composition.</summary>
    /// <param name="rows">Palette bands ordered from low health through full health.</param>
    /// <param name="rear">Selects the rear-material channel treatment when true.</param>
    /// <returns>True only when every supplied color matches the composition.</returns>
    internal static bool Matches(ushort[][] rows, bool rear)
    {
        for (int frame = 0; frame <= Intervals; frame++)
        for (int color = 0; color < 16; color++)
            if (rows[frame][color] != Color(frame, color, rear)) return false;
        return true;
    }

    /// <summary>Interpolates one BGR555 color between the low-health and golden endpoint palettes.</summary>
    /// <param name="frame">Palette band index from 0 through <see cref="Intervals"/>.</param>
    /// <param name="color">Color slot within the 16-entry palette.</param>
    /// <param name="rear">Applies the rear-material paint policy when true.</param>
    /// <returns>The rounded, channel-wise interpolated BGR555 color.</returns>
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

    /// <summary>Builds one endpoint color, applying rear-material shading and selected channel corrections as needed.</summary>
    /// <param name="gold">Selects the golden-health endpoint when true, otherwise the low-health endpoint.</param>
    /// <param name="color">Palette slot whose endpoint paint is requested.</param>
    /// <param name="rear">Selects rear-material lighting adjustments.</param>
    /// <returns>The endpoint's BGR555 color.</returns>
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

    /// <summary>Returns the hand-authored low-health paint for one palette slot.</summary>
    /// <param name="color">Palette slot in the 16-entry Golden Torizo palette.</param>
    /// <returns>The low-health endpoint color in BGR555 format.</returns>
    private static ushort Low(int color)
    {
        if (color <= 8)
        {
            int red = color switch
            {
                1 => LowHighlight, 2 => LowEdge, 3 => LowRecess, 4 => LowOutline,
                >= 5 and <= 8 => Math.Max(LowPlateDark, LowPlateLight - LowPlateShadeStep * (color - 5)),
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

    /// <summary>Returns the hand-authored golden armor, cyan core, eye, or contour paint for one palette slot.</summary>
    /// <param name="color">Palette slot in the 16-entry Golden Torizo palette.</param>
    /// <returns>The golden-health endpoint color in BGR555 format.</returns>
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

    /// <summary>Adds signed channel deltas while clipping each BGR555 component to its representable five-bit range.</summary>
    /// <param name="color">Base BGR555 color.</param>
    /// <param name="red">Signed red-channel adjustment.</param>
    /// <param name="green">Signed green-channel adjustment.</param>
    /// <param name="blue">Signed blue-channel adjustment.</param>
    /// <returns>The adjusted BGR555 color.</returns>
    private static ushort Adjust(ushort color, int red, int green, int blue) =>
        Pack(Math.Clamp((color & 31) + red, 0, 31), Math.Clamp((color >> 5 & 31) + green, 0, 31), Math.Clamp((color >> 10 & 31) + blue, 0, 31));

    /// <summary>Packs three five-bit RGB channels into the SNES BGR555 word layout.</summary>
    /// <param name="red">Red component, stored in bits 0 through 4.</param>
    /// <param name="green">Green component, stored in bits 5 through 9.</param>
    /// <param name="blue">Blue component, stored in bits 10 through 14.</param>
    /// <returns>The packed BGR555 color word.</returns>
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
