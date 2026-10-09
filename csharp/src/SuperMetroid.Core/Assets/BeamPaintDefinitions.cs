using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Five stock beam material palettes at $90:C3E1-C480. $90:ACCD-ACEF copies
/// the selected sixteen colors to OBJ palette6; charge/impact sprites and frozen
/// enemy components also use this palette. The reviewed exact material channel
/// choices and shade memberships are selected paint composition; generating them
/// without those inputs invents different hues. Shared ramps/channels calculate.
/// Copied clear/black target metadata is explicit, not inferred from visibility.
/// No pixels, cadence, freeze logic or selection precedence are exempted.
/// </summary>
internal static class BeamPaintDefinitions
{
    /// <summary>Largest channel value representable by a five-bit SNES RGB component.</summary>
    private const int Rgb5Maximum = (1 << 5) - 1;
    /// <summary>$90:C3E1 and corresponding slot0 aliases copy this blue target despite OBJ transparency.</summary>
    private const int ClearTargetBlue = 14;
    /// <summary>Builds one packed SNES RGB555 color for a beam-palette selection and color index.</summary>
    /// <param name="selection">Combined beam selection used to choose the authored material palette.</param>
    /// <param name="color">Color slot within the sixteen-color OBJ palette.</param>
    /// <returns>The packed 15-bit color word, including the explicit clear-target and black-slot values.</returns>
    internal static ushort Color(int selection, int color)
    {
        if (color == 0) return ClearTargetBlue << 10;
        if (color == 1) return (ushort)(Rgb5Maximum | Rgb5Maximum << 5 | Rgb5Maximum << 10);
        if (BeamPaletteDefinitions.IsBlackSlot(selection, color)) return 0;
        int source = BeamPaletteDefinitions.ColorSourceSelection(selection, color);
        (int red, int green, int blue) = source switch
        {
            (int)SamusBeamFlags.Ice => Ice(color),
            (int)SamusBeamFlags.Wave => Wave(color),
            (int)SamusBeamFlags.Plasma => Plasma(color),
            (int)SamusBeamFlags.Spazer => Spazer(color),
            _ => Power(color),
        };
        return (ushort)(red | green << 5 | blue << 10);
    }

    /// <summary>Returns the RGB555 channels for the stock power-beam material at one palette slot.</summary>
    /// <param name="color">Color slot whose authored channel values are requested.</param>
    /// <returns>Five-bit red, green, and blue channel values.</returns>
    private static (int Red, int Green, int Blue) Power(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2;
            int red = shade switch { 0 => Rgb5Maximum, 1 => 21, _ => 13 };
            int blue = shade switch { 0 => 6, 1 => 7, _ => 4 };
            return (red, 15 - 5 * shade, blue);
        }
        return color switch
        {
            5 => (Rgb5Maximum, Rgb5Maximum, 20),
            6 => (30, 28, 0),
            7 => (Rgb5Maximum, 10, 10),
            8 => (Rgb5Maximum, 6, 6),
            15 => (10, 2, 4),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Returns the frost-blue material channels for an ice-beam palette slot.</summary>
    /// <param name="color">Color slot whose authored channel values are requested.</param>
    /// <returns>Five-bit red, green, and blue channel values.</returns>
    private static (int Red, int Green, int Blue) Ice(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2;
            int blue = shade switch { 0 => Rgb5Maximum, 1 => 27, _ => 21 };
            return (0, (22 * (2 - shade) + 7 * shade + 1) / 2, blue);
        }
        if (color is >= 5 and <= 8)
        {
            int shade = color - 5;
            int red = shade switch { 0 => 14, 1 => 10, 2 => 5, _ => 2 };
            int green = shade switch { 0 => 27, 1 => 24, 2 => 19, _ => 17 };
            return (red, green, (Rgb5Maximum * (3 - shade) + 23 * shade) / 3);
        }
        if (color is >= 9 and <= 13)
        {
            int shade = color - 9;
            // Selected extra green step belongs to final two frost targets. It is
            // color composition, not a spatial lighting or hardware rule.
            return (0, 29 - 2 * (shade + (shade >= 3 ? 1 : 0)), Rgb5Maximum - shade);
        }
        return color switch
        {
            14 => (17, 28, Rgb5Maximum),
            15 => (0, 6, 8),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Returns the magenta material channels for a wave-beam palette slot.</summary>
    /// <param name="color">Color slot whose authored channel values are requested.</param>
    /// <returns>Five-bit red, green, and blue channel values.</returns>
    private static (int Red, int Green, int Blue) Wave(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2;
            int magenta = (Rgb5Maximum * (2 - shade) + 12 * shade + 1) / 2;
            return (magenta, 0, magenta);
        }
        if (color is >= 5 and <= 7) return (Rgb5Maximum, (22 * (7 - color) + 6 * (color - 5)) / 2, Rgb5Maximum);
        int level = color switch { 8 => 26, 15 => 9, _ => throw new ArgumentOutOfRangeException(nameof(color)) };
        return (level, 0, level);
    }

    /// <summary>Returns the green material channels for a plasma-beam palette slot.</summary>
    /// <param name="color">Color slot whose authored channel values are requested.</param>
    /// <returns>Five-bit red, green, and blue channel values.</returns>
    private static (int Red, int Green, int Blue) Plasma(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2, green = shade switch { 0 => Rgb5Maximum, 1 => 20, _ => 12 };
            return (0, green, (14 * (2 - shade) + 5 * shade) / 2);
        }
        if (color is >= 5 and <= 7)
        {
            int level = (26 * (7 - color) + 10 * (color - 5)) / 2;
            return (level, Rgb5Maximum, level);
        }
        return color switch { 8 => (0, Rgb5Maximum, 0), 15 => (0, 9, 2), _ => throw new ArgumentOutOfRangeException(nameof(color)) };
    }

    /// <summary>Returns the gold material channels for a spazer-beam palette slot.</summary>
    /// <param name="color">Color slot whose authored channel values are requested.</param>
    /// <returns>Five-bit red, green, and blue channel values.</returns>
    private static (int Red, int Green, int Blue) Spazer(int color)
    {
        if (color is >= 5 and <= 7)
        {
            var plasma = Plasma(color);
            return (plasma.Green, plasma.Green, plasma.Red);
        }
        return color switch
        {
            2 => (Rgb5Maximum, Rgb5Maximum, 0),
            3 => (22, 16, 0),
            4 => (14, 7, 0),
            8 => (27, 27, 0),
            15 => (13, 5, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }
}
