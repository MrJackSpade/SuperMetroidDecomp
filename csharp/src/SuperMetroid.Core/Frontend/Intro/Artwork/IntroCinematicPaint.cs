using System.Buffers.Binary;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Frontend.Intro.Artwork.IntroCinematicPaintDefinitions;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Paints the intro (<see cref="IntroCinematicPaintDefinitions"/>): every duplicated slot and described shade relationship of
/// the intro palettes, calculated from their reviewed materials and endpoints.
/// </summary>
internal static class IntroCinematicPaint
{
    /// <summary>Checks every colour of both of the intro screen's owners against the calculated set and ink painting.</summary>
    internal static bool Matches(ScreenColours colours)
    {
        Ensure.NotNull(colours);
        for (int set = 0; set < ColourSets.SetCount; set++)
        for (int ink = 0; ink < ColourSets.SetColours; ink++)
        {
            if (colours.BackgroundSet(set)[ink] != Background(set, ink) || colours.SpriteSet(set)[ink] != Sprite(set, ink))
                return false;
        }
        return true;
    }

    /// <summary>Selects a font, portrait, sepia scene or fill colour for one colour of an intro background set.</summary>
    internal static RgbColor Background(int set, int ink)
    {
        Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set);
        Ensure.BetweenInclusive(0, ColourSets.SetColours - 1, ink);
        if (set == 0) return Font(ink);
        if (ink == 0) return RgbColor.Black;
        return set switch
        {
            1 => ink switch { 1 or 4 => Neutral(31), 2 or 14 => RgbColor.Black, 3 or 6 => Neutral(TextGray), _ => Neutral(Fill) },
            2 => ink <= 8 ? Sepia(ink) : ink == 15 ? RgbColor.Black : Neutral(Fill),
            3 => Portrait(ink),
            4 or 5 or 6 => Neutral(Fill),
            7 => Scene(ink),
            _ => throw new ArgumentOutOfRangeException(nameof(set)),
        };
    }

    /// <summary>Selects a neutral-cycle, object, crossfade or Mother Brain colour for one colour of an intro sprite set.</summary>
    internal static RgbColor Sprite(int set, int ink)
    {
        Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set);
        Ensure.BetweenInclusive(0, ColourSets.SetColours - 1, ink);
        if (ink == 0) return ObjectZero;
        return set switch
        {
            0 => Neutral(31 - (ink - 1) % WhiteCycleLength * WhiteCycleStep),
            1 => ObjectOne(ink),
            2 or 3 => Neutral(Fill),
            4 => ObjectFour(ink),
            5 => ObjectFive(ink),
            6 => Crossfade(ink),
            7 => MotherBrain(ink),
            _ => throw new ArgumentOutOfRangeException(nameof(set)),
        };
    }

    /// <summary>Builds the four-step font ink groups from green highlights, neutral fill, and depth-varying shadows.</summary>
    private static RgbColor Font(int ink)
    {
        int group = ink / 4;
        return (ink % 4) switch
        {
            0 when group == 0 => RgbColor.Black,
            0 => FontZero(group - 1),
            1 => Pack(0, 31, 0),
            2 => Neutral(Fill),
            _ => Pack(0, FontShadow(group + 1), 0),
        };
    }
    /// <summary>Interpolates a green font shadow in squared intensity between full green and the authored dark endpoint.</summary>
    private static int FontShadow(int depth)
    {
        if (depth == 4) return FontShadowDark;
        double intensity = (Math.Sqrt(31) * (4 - depth) + Math.Sqrt(FontShadowDark) * depth) / 4;
        return (int)Math.Ceiling(intensity * intensity);
    }
    /// <summary>Calculates the hue of a font group's zero-ink entry from green gamma shading and proportional blue.</summary>
    private static RgbColor FontZero(int shade)
    {
        int green = Gamma(31, FontZeroGreenDark, shade, 2);
        return Pack(0, green, FontZeroBlue * green / 31);
    }
    /// <summary>Derives six sepia levels and two darker outlines with a fixed blue deficit.</summary>
    private static RgbColor Sepia(int ink)
    {
        int level = ink <= 6 ? (31 * (6 - ink) + SepiaDark * (ink - 1) + SepiaIntervals - 1) / SepiaIntervals
            : SepiaDark - (ink - 6) * SepiaOutlineStep;
        return Pack(level, level, Math.Max(0, level - SepiaBlueDeficit));
    }
    /// <summary>Raises red and green on a sepia ink while preserving its blue channel.</summary>
    private static RgbColor Warm(int ink, int tint)
    {
        RgbColor color = Sepia(ink);
        return Pack(Math.Min(31, (color.Red) + tint), Math.Min(31, (color.Green) + tint), color.Blue);
    }
    /// <summary>Darkens the shared Mother Brain plate highlight by one RGB5 level for intro scene objects.</summary>
    private static RgbColor Plate()
    {
        RgbColor color = MotherBrainHealthPaintDefinitions.PlateHighlight;
        return Pack((color.Red) - PlateShade, (color.Green) - PlateShade, (color.Blue) - PlateShade);
    }
    /// <summary>Selects authored neutral, sepia, plate, and warm materials for the intro scene row.</summary>
    private static RgbColor Scene(int ink) => ink switch
    {
        1 => Neutral(SceneHighlight), 2 => Sepia(4), 3 => Pack(SceneDark, SceneDark, SceneDarkBlue),
        4 => Neutral(Contour), 5 or 9 => Plate(), 6 => Warm(5, WarmTint), 7 => Neutral(MiddleNeutral),
        8 => Neutral(LowNeutral), 10 => Warm(5, StrongWarmTint), 11 => Warm(6, StrongWarmTint),
        12 => Sepia(7), 13 or 14 => Sepia(1), _ => RgbColor.Black,
    };
    /// <summary>Maps the first object palette's repeated plate and warm materials plus its dark accent.</summary>
    private static RgbColor ObjectOne(int ink) => ink switch
    {
        1 or 5 or 10 or 13 or 15 => Plate(), 2 or 6 or 11 => Warm(5, WarmTint),
        3 or 7 or 12 => Warm(6, WarmTint), 4 or 8 => Sepia(7),
        9 => Pack(ObjectAccentRed, 0, ObjectAccentBlue), 14 => Sepia(1), _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    /// <summary>Maps the fourth object palette to repeated sepia steps and contour ink.</summary>
    private static RgbColor ObjectFour(int ink) => ink switch
    {
        1 or 5 or 14 => Sepia(5), 2 or 7 => Sepia(3), 3 => Neutral(Contour), 4 => Sepia(1), 6 => Sepia(2),
        8 or 9 or 10 or 12 => Sepia(4), 11 or 13 or 15 => Sepia(6), _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    /// <summary>Repeats a three-material warm and sepia cycle across the fifth object palette.</summary>
    private static RgbColor ObjectFive(int ink) => ink switch
    {
        1 or 15 => Sepia(1), 14 => Neutral(Contour),
        _ => ((ink - 2) % 3) switch { 0 => Warm(4, StrongWarmTint), 1 => Warm(5, WarmTint), _ => Sepia(6) },
    };
    /// <summary>Combines sepia scene inks with the four-step blue fade and two color accents.</summary>
    private static RgbColor Crossfade(int ink) => ink switch
    {
        <= 8 => Sepia(ink), <= 12 => Pack(0, 0, (31 * (12 - ink) + CrossfadeBlueDark * (ink - 9)) / 3),
        13 => Pack(0, 31, 0), 14 => Pack(0, CrossfadeAccentGreen, CrossfadeAccentBlue), _ => RgbColor.Black,
    };
    /// <summary>Reuses scene sepia and plate materials for the Mother Brain intro object row.</summary>
    private static RgbColor MotherBrain(int ink) => ink switch
    {
        1 or 13 or 14 => Sepia(1), 2 => Sepia(4), 3 => Neutral(Contour), 4 => Neutral(DeepContour),
        5 => MotherBrainHealthPaintDefinitions.PlateHighlight, 6 => Warm(5, WarmTint), 7 => Neutral(PlateNeutral),
        8 => Neutral(PlateDarkNeutral), 9 => Plate(), 10 => Warm(5, StrongWarmTint), 11 => Warm(6, StrongWarmTint),
        12 => Sepia(7), _ => RgbColor.Black,
    };
    /// <summary>Paints Samus's helmet, cheek, and visor shade groups with authored contour and glint colors.</summary>
    private static RgbColor Portrait(int ink)
    {
        if (ink is 9 or 14 or 15)
        {
            int shade = ink == 9 ? 0 : ink == 14 ? 1 : 2;
            int red = Gamma(HelmetRedLight, HelmetRedDark, shade, 2), range = HelmetRedLight - HelmetRedDark;
            return Pack(red, (HelmetGreen * (red - HelmetRedDark) + range - 1) / range, HelmetBlue);
        }
        if (ink is 7 or 8 or 5)
        {
            int shade = ink == 7 ? 0 : ink == 8 ? 1 : 2;
            return Pack(CheekRed - shade * CheekShade, CheekGreen - shade * CheekShade, CheekBlue - shade * CheekShade);
        }
        if (ink is 11 or 13 or 10)
        {
            int shade = ink == 11 ? 0 : ink == 13 ? 1 : 2;
            return Pack(0, Gamma(VisorGreenLight, VisorGreenDark, shade, 2), (VisorBlueLight * (2 - shade) + VisorBlueDark * shade) / 2);
        }
        return ink switch { 1 => Pack(0, 0, PortraitOutlineBlue), 2 => Shoulder, 3 => RgbColor.Black,
            4 => Pack(0, VisorGlintGreen, VisorGlintBlue), 6 => PortraitContour, 12 => PortraitDeep,
            _ => throw new ArgumentOutOfRangeException(nameof(ink)) };
    }
    /// <summary>Interpolates a palette channel in square-root intensity and rounds back to RGB5.</summary>
    private static int Gamma(int first, int last, int shade, int intervals)
    {
        double intensity = (Math.Sqrt(first) * (intervals - shade) + Math.Sqrt(last) * shade) / intervals;
        return (int)Math.Floor(intensity * intensity + 0.5);
    }
    /// <summary>Uses one RGB5 intensity across all three channels for neutral paint.</summary>
    private static RgbColor Neutral(int value) => Pack(value, value, value);
    /// <summary>Converts three calculated five-bit channels into a CGRAM color.</summary>
    private static RgbColor Pack(int red, int green, int blue) => new RgbColor(red, green, blue);
}
