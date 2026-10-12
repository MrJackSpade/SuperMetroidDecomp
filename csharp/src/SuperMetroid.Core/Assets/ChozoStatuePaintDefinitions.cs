using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Which Chozo statue palette at $AA:E31D (Wrecked Ship) or $AA:E35D (Lower Norfair).</summary>
internal enum ChozoStatuePalette
{
    WreckedShip,
    LowerNorfair,
}

/// <summary>
/// Stock colors for both 32-color Chozo statue palettes. Each has two sixteen-color halves.
/// Shared structure calculates: colors 0..3 and 8..13 are common to all four halves, with
/// 8..11 a nearest-integer gold ramp; Wrecked Ship colors 4..7 are nearest-integer grey ramps
/// in both halves; Lower Norfair's second-half colors 4..6 repeat its first-half 5..7, one
/// shade down. Retained as authored paint: the skin and gold endpoints, colors 14/15, Lower
/// Norfair's first-half 4..7 and second-half color 7.
/// </summary>
internal static class ChozoStatuePaintDefinitions
{
    private const int HalfColors = 16;

    internal static Bgr555 Color(ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= HalfColors * 2) throw new ArgumentOutOfRangeException(nameof(color));
        int local = color % HalfColors;
        bool second = color >= HalfColors;
        if (Shared(local) is Bgr555 shared) return shared;
        return palette switch
        {
            ChozoStatuePalette.WreckedShip => WreckedShip(local, second),
            ChozoStatuePalette.LowerNorfair => LowerNorfair(local, second),
            _ => throw new ArgumentOutOfRangeException(nameof(palette)),
        };
    }

    private static Bgr555? Shared(int local) => local switch
    {
        0 => Pack(0, 0, 14),
        1 => Pack(31, 25, 24),
        2 => Pack(31, 20, 18),
        3 => Pack(31, 14, 11),
        >= 8 and <= 11 => Ramp((31, 31, 9), (6, 6, 0), local - 8, 3),
        12 => Pack(31, 31, 15),
        13 => Pack(25, 25, 10),
        _ => null,
    };

    private static Bgr555 WreckedShip(int local, bool second) => local switch
    {
        >= 4 and <= 7 => second
            ? Ramp((16, 16, 16), (4, 4, 4), local - 4, 3)
            : Ramp((25, 25, 25), (6, 6, 6), local - 4, 3),
        14 => second ? Pack(20, 20, 20) : Pack(31, 31, 31),
        15 => Bgr555.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(local)),
    };

    private static Bgr555 LowerNorfair(int local, bool second)
    {
        if (second && local is >= 4 and <= 6) return LowerNorfair(local + 1, false);
        return (local, second) switch
        {
            (4, false) => Pack(28, 27, 11),
            (5, false) => Pack(21, 20, 8),
            (6, false) => Pack(13, 12, 4),
            (7, false) => Pack(8, 7, 2),
            (7, true) => Pack(5, 4, 0),
            (14, false) => Pack(31, 30, 28),
            (14, true) => Pack(20, 20, 20),
            (15, false) => Pack(3, 2, 0),
            (15, true) => Pack(1, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(local)),
        };
    }

    private static Bgr555 Ramp((int Red, int Green, int Blue) start, (int Red, int Green, int Blue) end, int step, int steps) =>
        Pack(Nearest(start.Red, end.Red, step, steps), Nearest(start.Green, end.Green, step, steps),
            Nearest(start.Blue, end.Blue, step, steps));

    private static int Nearest(int start, int end, int step, int steps)
    {
        int delta = end - start;
        return start + Math.Sign(delta) * ((Math.Abs(delta) * step + steps / 2) / steps);
    }

    private static Bgr555 Pack(int red, int green, int blue) => new Bgr555(red, green, blue);
}
