using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using static SuperMetroid.AssetExtraction.Samus.Artwork.BeamPaintDefinitions;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>The colour each slot of each stock beam material palette is painted.</summary>
internal static class BeamPaint
{
    /// <summary>Builds a sixteen-color beam colour set from shared clear/white/black slots and the selected power, ice, wave, plasma, or Spazer paint.</summary>
    internal static RgbColor Color(SamusBeamCombination selection, int color)
    {
        if (color == 0) return new FadeLevelColour(0, 0, ClearTargetBlue).ToColour();
        if (color == 1) return new FadeLevelColour(Rgb5Maximum, Rgb5Maximum, Rgb5Maximum).ToColour();
        if (BeamPaletteSources.IsBlackSlot(selection, color)) return RgbColor.Black;
        SamusBeamCombination source = BeamPaletteSources.ColorSourceSelection(selection, color);
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
                SamusBeamCombination.SpazerPlasmaIceWave => throw new InvalidOperationException($"{source} is not a single beam's paint."),
            _ => throw new InvalidOperationException($"{source} is not a single beam's paint."),
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
            return new FadeLevelColour(red, 15 - 5 * shade, blue).ToColour();
        }
        return color switch
        {
            5 => new FadeLevelColour(Rgb5Maximum, Rgb5Maximum, 20).ToColour(),
            6 => new RgbColor(247, 231, 0),
            7 => new FadeLevelColour(Rgb5Maximum, 10, 10).ToColour(),
            8 => new FadeLevelColour(Rgb5Maximum, 6, 6).ToColour(),
            15 => new RgbColor(82, 16, 33),
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
            return new FadeLevelColour(0, (22 * (2 - shade) + 7 * shade + 1) / 2, blue).ToColour();
        }
        if (color is >= 5 and <= 8)
        {
            int shade = color - 5;
            int red = shade switch { 0 => 14, 1 => 10, 2 => 5, _ => 2 };
            int green = shade switch { 0 => 27, 1 => 24, 2 => 19, _ => 17 };
            return new FadeLevelColour(red, green, (Rgb5Maximum * (3 - shade) + 23 * shade) / 3).ToColour();
        }
        if (color is >= 9 and <= 13)
        {
            int shade = color - 9;
            // Selected extra green step belongs to final two frost targets. It is
            // color composition, not a spatial lighting or hardware rule.
            return new FadeLevelColour(0, 29 - 2 * (shade + (shade >= 3 ? 1 : 0)), Rgb5Maximum - shade).ToColour();
        }
        return color switch
        {
            14 => new FadeLevelColour(17, 28, Rgb5Maximum).ToColour(),
            15 => new RgbColor(0, 49, 66),
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
            return new FadeLevelColour(magenta, 0, magenta).ToColour();
        }
        if (color is >= 5 and <= 7) return new FadeLevelColour(Rgb5Maximum, (22 * (7 - color) + 6 * (color - 5)) / 2, Rgb5Maximum).ToColour();
        int level = color switch { 8 => 26, 15 => 9, _ => throw new ArgumentOutOfRangeException(nameof(color)) };
        return new FadeLevelColour(level, 0, level).ToColour();
    }

    /// <summary>Interpolates Plasma's green ramp and the brighter green-white accent colours.</summary>
    private static RgbColor Plasma(int color)
    {
        if (color is >= 2 and <= 4)
        {
            int shade = color - 2, green = shade switch { 0 => Rgb5Maximum, 1 => 20, _ => 12 };
            return new FadeLevelColour(0, green, (14 * (2 - shade) + 5 * shade) / 2).ToColour();
        }
        if (color is >= 5 and <= 7)
        {
            int level = (26 * (7 - color) + 10 * (color - 5)) / 2;
            return new FadeLevelColour(level, Rgb5Maximum, level).ToColour();
        }
        return color switch { 8 => new FadeLevelColour(0, Rgb5Maximum, 0).ToColour(), 15 => new RgbColor(0, 74, 16), _ => throw new ArgumentOutOfRangeException(nameof(color)) };
    }

    /// <summary>Reuses Plasma's green ramp channels for Spazer's yellow-green accents and supplies its remaining gold inks.</summary>
    private static RgbColor Spazer(int color)
    {
        if (color is >= 5 and <= 7)
        {
            var plasma = Plasma(color);
            return new FadeLevelColour(plasma.FadeLevels.Green, plasma.FadeLevels.Green, plasma.FadeLevels.Red).ToColour();
        }
        return color switch
        {
            2 => new FadeLevelColour(Rgb5Maximum, Rgb5Maximum, 0).ToColour(),
            3 => new RgbColor(181, 132, 0),
            4 => new RgbColor(115, 57, 0),
            8 => new RgbColor(222, 222, 0),
            15 => new RgbColor(107, 41, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }
}
