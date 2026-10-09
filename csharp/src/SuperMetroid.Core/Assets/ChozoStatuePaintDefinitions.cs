namespace SuperMetroid.Core.Assets;

/// <summary>Which Chozo statue palette at $AA:E31D (Wrecked Ship) or $AA:E35D (Lower Norfair).</summary>
internal enum ChozoStatuePalette
{
    /// <summary>Palette stored for the Wrecked Ship Chozo statue at the first native table entry.</summary>
    WreckedShip,

    /// <summary>Palette stored for the Lower Norfair Chozo statue at the second native table entry.</summary>
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
    /// <summary>Number of colors in each independently authored statue palette half.</summary>
    private const int HalfColors = 16;

    /// <summary>Resolves one RGB5 word from the selected statue's 32-color palette.</summary>
    /// <param name="palette">Statue palette family whose retained paint colors are selected.</param>
    /// <param name="color">Color index from zero through thirty-one across the two palette halves.</param>
    /// <returns>The packed SNES RGB5 color word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color index or palette identity is outside the supported domain.</exception>
    internal static ushort Color(ChozoStatuePalette palette, int color)
    {
        if ((uint)color >= HalfColors * 2) throw new ArgumentOutOfRangeException(nameof(color));
        int local = color % HalfColors;
        bool second = color >= HalfColors;
        if (Shared(local) is ushort shared) return shared;
        return palette switch
        {
            ChozoStatuePalette.WreckedShip => WreckedShip(local, second),
            ChozoStatuePalette.LowerNorfair => LowerNorfair(local, second),
            _ => throw new ArgumentOutOfRangeException(nameof(palette)),
        };
    }

    /// <summary>Returns a color shared by both statues and halves, or <see langword="null"/> when paint is statue-specific.</summary>
    /// <param name="local">Index within one sixteen-color half.</param>
    private static ushort? Shared(int local) => local switch
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

    /// <summary>Resolves the retained Wrecked Ship grey ramp and white/black endpoints.</summary>
    /// <param name="local">Index within one sixteen-color half.</param>
    /// <param name="second">Selects the second half's darker grey ramp and grey endpoint.</param>
    /// <returns>The statue-specific packed RGB5 color for the requested slot.</returns>
    private static ushort WreckedShip(int local, bool second) => local switch
    {
        >= 4 and <= 7 => second
            ? Ramp((16, 16, 16), (4, 4, 4), local - 4, 3)
            : Ramp((25, 25, 25), (6, 6, 6), local - 4, 3),
        14 => second ? Pack(20, 20, 20) : Pack(31, 31, 31),
        15 => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(local)),
    };

    /// <summary>Resolves Lower Norfair's retained skin, gold endpoints, and second-half shade reuse.</summary>
    /// <param name="local">Index within one sixteen-color half.</param>
    /// <param name="second">Selects the second palette half, including its reused or darker colors.</param>
    /// <returns>The statue-specific packed RGB5 color for the requested slot.</returns>
    private static ushort LowerNorfair(int local, bool second)
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

    /// <summary>Interpolates one RGB5 color along an inclusive component-wise ramp.</summary>
    /// <param name="start">RGB5 components at the ramp's first endpoint.</param>
    /// <param name="end">RGB5 components at the ramp's final endpoint.</param>
    /// <param name="step">Zero-based interpolation position.</param>
    /// <param name="steps">Number of intervals between endpoints.</param>
    /// <returns>The interpolated components packed into an SNES color word.</returns>
    private static ushort Ramp((int Red, int Green, int Blue) start, (int Red, int Green, int Blue) end, int step, int steps) =>
        Pack(Nearest(start.Red, end.Red, step, steps), Nearest(start.Green, end.Green, step, steps),
            Nearest(start.Blue, end.Blue, step, steps));

    /// <summary>Interpolates an integer component using nearest-integer rounding along the signed delta.</summary>
    /// <param name="start">Component value at the first endpoint.</param>
    /// <param name="end">Component value at the final endpoint.</param>
    /// <param name="step">Zero-based position along the ramp.</param>
    /// <param name="steps">Number of intervals across the ramp.</param>
    /// <returns>The rounded component at the requested position.</returns>
    private static int Nearest(int start, int end, int step, int steps)
    {
        int delta = end - start;
        return start + Math.Sign(delta) * ((Math.Abs(delta) * step + steps / 2) / steps);
    }

    /// <summary>Packs three five-bit RGB components into the SNES 15-bit color layout.</summary>
    /// <param name="red">Five-bit red component in bits zero through four.</param>
    /// <param name="green">Five-bit green component in bits five through nine.</param>
    /// <param name="blue">Five-bit blue component in bits ten through fourteen.</param>
    /// <returns>The packed RGB15 word.</returns>
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
