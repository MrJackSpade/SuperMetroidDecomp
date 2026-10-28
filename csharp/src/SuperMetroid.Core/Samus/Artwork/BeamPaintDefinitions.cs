using SuperMetroid.Core.Progression;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Artwork;

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
    /// <summary>Highest channel value in a five-bit SNES RGB component.</summary>
    private const int Rgb5Maximum = (1 << 5) - 1;
    /// <summary>$90:C3E1 and corresponding slot0 aliases copy this blue target despite OBJ transparency.</summary>
    private const int ClearTargetBlue = 14;
    /// <summary>Builds a sixteen-color beam material row from shared clear/white/black slots and the selected power, ice, wave, plasma, or Spazer paint.</summary>
    internal static RgbColor Color(SamusBeamCombination selection, int color)
    {
        if (color == 0) return new RgbColor(0, 0, ClearTargetBlue);
        if (color == 1) return new RgbColor(Rgb5Maximum, Rgb5Maximum, Rgb5Maximum);
        if (BeamPaletteDefinitions.IsBlackSlot(selection, color)) return RgbColor.Black;
        SamusBeamCombination source = BeamPaletteDefinitions.ColorSourceSelection(selection, color);
        return source switch
        {
            SamusBeamCombination.Power => Power(color),
            SamusBeamCombination.Ice => Ice(color),
            SamusBeamCombination.Wave => Wave(color),
            SamusBeamCombination.Plasma => Plasma(color),
            SamusBeamCombination.Spazer => Spazer(color),
            SamusBeamCombination.IceWave or
                SamusBeamCombination.SpazerWave or
                SamusBeamCombination.SpazerIce or
                SamusBeamCombination.SpazerIceWave or
                SamusBeamCombination.PlasmaWave or
                SamusBeamCombination.PlasmaIce or
                SamusBeamCombination.PlasmaIceWave or
                SamusBeamCombination.SpazerPlasma or
                SamusBeamCombination.SpazerPlasmaWave or
                SamusBeamCombination.SpazerPlasmaIce or
                SamusBeamCombination.SpazerPlasmaIceWave => throw new InvalidOperationException($"{source} is not a beam palette row."),
            _ => throw new InvalidOperationException($"{source} is not a beam palette row."),
        };
    }

    /// <summary>Combines Power's orange three-step ramp with its separate yellow, red, and dark accent inks.</summary>
    private static RgbColor Power(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2;
            int red = shade switch { 0 => Rgb5Maximum, 1 => 21, _ => 13 };
            int blue = shade switch { 0 => 6, 1 => 7, _ => 4 };
            return new RgbColor(red, 15 - 5 * shade, blue);
        }
        return color switch
        {
            5 => new RgbColor(Rgb5Maximum, Rgb5Maximum, 20),
            6 => new RgbColor(30, 28, 0),
            7 => new RgbColor(Rgb5Maximum, 10, 10),
            8 => new RgbColor(Rgb5Maximum, 6, 6),
            15 => new RgbColor(10, 2, 4),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Builds Ice's blue-white and frost ramps, including the selected extra green step in the final frost targets.</summary>
    private static RgbColor Ice(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2;
            int blue = shade switch { 0 => Rgb5Maximum, 1 => 27, _ => 21 };
            return new RgbColor(0, (22 * (2 - shade) + 7 * shade + 1) / 2, blue);
        }
        if (color is >= 5 and <= 8)
        {
            int shade = color - 5;
            int red = shade switch { 0 => 14, 1 => 10, 2 => 5, _ => 2 };
            int green = shade switch { 0 => 27, 1 => 24, 2 => 19, _ => 17 };
            return new RgbColor(red, green, (Rgb5Maximum * (3 - shade) + 23 * shade) / 3);
        }
        if (color is >= 9 and <= 13)
        {
            int shade = color - 9;
            // Selected extra green step belongs to final two frost targets. It is
            // color composition, not a spatial lighting or hardware rule.
            return new RgbColor(0, 29 - 2 * (shade + (shade >= 3 ? 1 : 0)), Rgb5Maximum - shade);
        }
        return color switch
        {
            14 => new RgbColor(17, 28, Rgb5Maximum),
            15 => new RgbColor(0, 6, 8),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Interpolates Wave's magenta ramp and its brighter violet accents.</summary>
    private static RgbColor Wave(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2;
            int magenta = (Rgb5Maximum * (2 - shade) + 12 * shade + 1) / 2;
            return new RgbColor(magenta, 0, magenta);
        }
        if (color is >= 5 and <= 7) return new RgbColor(Rgb5Maximum, (22 * (7 - color) + 6 * (color - 5)) / 2, Rgb5Maximum);
        int level = color switch { 8 => 26, 15 => 9, _ => throw new ArgumentOutOfRangeException(nameof(color)) };
        return new RgbColor(level, 0, level);
    }

    /// <summary>Interpolates Plasma's green ramp and the brighter green-white accent row.</summary>
    private static RgbColor Plasma(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2, green = shade switch { 0 => Rgb5Maximum, 1 => 20, _ => 12 };
            return new RgbColor(0, green, (14 * (2 - shade) + 5 * shade) / 2);
        }
        if (color is >= 5 and <= 7)
        {
            int level = (26 * (7 - color) + 10 * (color - 5)) / 2;
            return new RgbColor(level, Rgb5Maximum, level);
        }
        return color switch { 8 => new RgbColor(0, Rgb5Maximum, 0), 15 => new RgbColor(0, 9, 2), _ => throw new ArgumentOutOfRangeException(nameof(color)) };
    }

    /// <summary>Reuses Plasma's green ramp channels for Spazer's yellow-green accents and supplies its remaining gold inks.</summary>
    private static RgbColor Spazer(int color)
    {
        if (color is >= 5 and <= 7)
        {
            var plasma = Plasma(color);
            return new RgbColor(plasma.Green, plasma.Green, plasma.Red);
        }
        return color switch
        {
            2 => new RgbColor(Rgb5Maximum, Rgb5Maximum, 0),
            3 => new RgbColor(22, 16, 0),
            4 => new RgbColor(14, 7, 0),
            8 => new RgbColor(27, 27, 0),
            15 => new RgbColor(13, 5, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }
}
