using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.GoldenTorizoHealthPaintDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Paints Golden Torizo's health bands between its low-health and golden endpoints, front and rear.</summary>
internal static class GoldenTorizoHealthPaint
{
    /// <summary>Checks all eight palette bands and sixteen colors against this front or rear paint composition.</summary>
    internal static bool Matches(RgbColor[][] rows, bool rear)
    {
        for (int frame = 0; frame <= Intervals; frame++)
        for (int color = 0; color < 16; color++)
            if (rows[frame][color] != Color(frame, color, rear)) return false;
        return true;
    }

    /// <summary>Interpolates a color between low-health and golden endpoints for one palette band.</summary>
    internal static RgbColor Color(int frame, int color, bool rear)
    {
        if ((uint)frame > Intervals || (uint)color >= 16) throw new ArgumentOutOfRangeException(nameof(frame));
        if (color == 0) return !rear && frame == Intervals ? RgbColor.Black : TransparentPayload;
        RgbColor first = Endpoint(GoldenTorizoHealthEndpoint.Low, color, rear), last = Endpoint(GoldenTorizoHealthEndpoint.Gold, color, rear);
        return first.Zip(last, (_, from, to) => (from * (Intervals - frame) + to * frame + Intervals / 2) / Intervals);
    }

    /// <summary>Selects endpoint paint and applies the rear-facing shade rules where requested.</summary>
    private static RgbColor Endpoint(GoldenTorizoHealthEndpoint endpoint, int color, bool rear)
    {
        RgbColor front = endpoint == GoldenTorizoHealthEndpoint.Gold ? Gold(color) : Low(color);
        if (!rear) return front;
        if (endpoint != GoldenTorizoHealthEndpoint.Gold && color == 15) return front;
        if (endpoint != GoldenTorizoHealthEndpoint.Gold && color is >= 12 and <= 14)
            return Adjust(Low(color - 3), RearShade, 0, 0);
        RgbColor result = color is >= 9 and <= 11
            ? Adjust(front, -RearShade, -RearCoreShade, -RearCoreShade)
            : Adjust(front, -RearShade, -RearShade, -RearShade);
        if (endpoint != GoldenTorizoHealthEndpoint.Gold && color == 7) result = new(result.Red, result.Green, LowRearPlateBlue);
        if (endpoint == GoldenTorizoHealthEndpoint.Gold && color == 9) result = new(result.Red, Math.Max(0, front.Green - RearShade), result.Blue);
        return result;
    }

    /// <summary>Builds one low-health red armor, purple core, eye, or contour color.</summary>
    private static RgbColor Low(int color)
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

    /// <summary>Builds one golden armor, cyan core, eye, or contour color.</summary>
    private static RgbColor Gold(int color)
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
            4 => RgbColor.Black,
            5 => Pack(GoldPlateLightRed, GoldPlateLightRed - GoldLightGreenDrop, GoldPlateLightBlue),
            6 => Pack(GoldPlateMiddleRed, GoldPlateMiddleRed - GoldShadeGreenDrop, 0),
            7 => Pack((GoldPlateMiddleRed + GoldPlateDarkRed + 1) / 2,
                (GoldPlateMiddleRed + GoldPlateDarkRed - 2 * GoldShadeGreenDrop + 1) / 2, 0),
            8 => Pack(GoldPlateDarkRed, GoldPlateDarkRed - GoldShadeGreenDrop, 0),
            15 => Pack(GoldContourRed, GoldContourGreen, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Applies per-channel lighting offsets with RGB5 saturation.</summary>
    private static RgbColor Adjust(RgbColor color, int red, int green, int blue) =>
        RgbColor.Saturating(color.Red + red, color.Green + green, color.Blue + blue);
    /// <summary>Packs the selected red, green, and blue levels into a palette color.</summary>
    private static RgbColor Pack(int red, int green, int blue) => new(red, green, blue);
}
