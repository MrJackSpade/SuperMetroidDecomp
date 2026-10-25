using System.Buffers.Binary;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Exact $8C:E3E9-E5E8 intro paint composition. Reviewed categorical material/endpoints and policies
/// specify this exact painting; all duplicated slots and described shade relationships calculate. Pixel, timing and gameplay scope is excluded.</summary>
internal static class IntroCinematicPaintDefinitions
{
    /// <summary>$8C:E3ED and flat rows4/5/6/10/11: selected repeated neutral fill intensity.</summary>
    private const int Fill = 24;
    /// <summary>$8C:E40F/E415: selected text gray.</summary>
    private const int TextGray = 10;
    /// <summary>$8C:E435-E43A: main sepia dark level, two final outline steps and blue deficit.</summary>
    private const int SepiaDark = 8, SepiaOutlineStep = 2, SepiaBlueDeficit = 2;
    /// <summary>$8C:E42B-E434: six main sepia levels including both endpoints.</summary>
    private const int SepiaIntervals = 5;
    /// <summary>$8C:E4D5/E4DD/E58D: selected warmer material red/green offsets.</summary>
    private const int WarmTint = 1, StrongWarmTint = 2;
    /// <summary>$8C:E4D1/E4D9/E50B/E5DB: plate highlight is one RGB5 level below the shared5739 paint.</summary>
    private const int PlateShade = 1;
    /// <summary>$8C:E4CB-E4D8/E5CF-E5D8: selected neutral and near-neutral material levels.</summary>
    private const int SceneHighlight = 29, SceneDark = 7, SceneDarkBlue = 6,
        Contour = 4, MiddleNeutral = 9, LowNeutral = 6, DeepContour = 2, PlateNeutral = 8, PlateDarkNeutral = 5;
    /// <summary>$8C:E3EF/E3F7/E3FF/E407: four ceiling gamma2 shadows from maximum green to this endpoint.</summary>
    private const int FontShadowDark = 11;
    /// <summary>$8C:E3F1/E3F9/E401: zero-ink tint endpoints; green gamma2 and blue hue scaling derive the middle.</summary>
    private const int FontZeroGreenDark = 12, FontZeroBlue = 14;
    /// <summary>$8C:E4E9 and every object-row zero: copied transparent compatibility word.</summary>
    private static readonly RgbColor ObjectZero = RgbColor.FromWord(0x3800);
    /// <summary>$8C:E4EB-E508: repeating four-level neutral cycle, with one-level brightness steps.</summary>
    internal const int WhiteCycleLength = 4, WhiteCycleStep = 1;
    /// <summary>$8C:E51B: selected dark red object accent.</summary>
    private const int ObjectAccentRed = 4, ObjectAccentBlue = 1;
    /// <summary>$8C:E5C1/E5C5: crossfade blue endpoint and green accent channels.</summary>
    private const int CrossfadeBlueDark = 4, CrossfadeAccentGreen = 14, CrossfadeAccentBlue = 5;
    /// <summary>$8C:E5BB-E5C2: equal decrement derived from full blue and the selected dark endpoint over three intervals.</summary>
    internal const int CrossfadeBlueStep = (31 - CrossfadeBlueDark) / 3;
    /// <summary>$8C:E45B/E465/E467: helmet red endpoints and hue; middle red follows gamma2.</summary>
    private const int HelmetRedLight = 24, HelmetRedDark = 5, HelmetGreen = 5, HelmetBlue = 5;
    /// <summary>$8C:E457/E459/E453: cyan cheek light and fixed channel shadow decrement.</summary>
    private const int CheekRed = 13, CheekGreen = 20, CheekBlue = 21, CheekShade = 6;
    /// <summary>$8C:E45F/E463/E45D: central visor green gamma2 endpoints and linear blue endpoints.</summary>
    private const int VisorGreenLight = 10, VisorGreenDark = 2, VisorBlueLight = 12, VisorBlueDark = 6;
    /// <summary>$8C:E451: bright visor's green/blue, with zero red.</summary>
    private const int VisorGlintGreen = 28, VisorGlintBlue = 22;
    /// <summary>$8C:E44D: shoulder/edge paint.</summary>
    private static readonly RgbColor Shoulder = RgbColor.FromWord(0x15aa);
    /// <summary>$8C:E455: dark cyan contour.</summary>
    private static readonly RgbColor PortraitContour = RgbColor.FromWord(0x14a2);
    /// <summary>$8C:E461: deep cool silhouette paint.</summary>
    private static readonly RgbColor PortraitDeep = RgbColor.FromWord(0x1c42);
    /// <summary>$8C:E44B: blue-only outline intensity.</summary>
    private const int PortraitOutlineBlue = 5;

    /// <summary>Checks all 256 intro CGRAM words against the calculated row and ink painting.</summary>
    internal static bool Matches(ReadOnlySpan<byte> bytes)
    {
        for (int index = 0; index < 256; index++)
            if (RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(index * 2, 2))) != Color(index / 16, index % 16)) return false;
        return true;
    }

    /// <summary>Selects a font, portrait, sepia scene, object, or crossfade color for one intro palette cell.</summary>
    internal static RgbColor Color(int row, int ink)
    {
        if ((uint)row >= 16 || (uint)ink >= 16) throw new ArgumentOutOfRangeException(nameof(row));
        if (row == 0) return Font(ink);
        if (ink == 0) return row >= 8 ? ObjectZero : RgbColor.Black;
        return row switch
        {
            1 => ink switch { 1 or 4 => Neutral(31), 2 or 14 => RgbColor.Black, 3 or 6 => Neutral(TextGray), _ => Neutral(Fill) },
            2 => ink <= 8 ? Sepia(ink) : ink == 15 ? RgbColor.Black : Neutral(Fill),
            3 => Portrait(ink),
            4 or 5 or 6 or 10 or 11 => Neutral(Fill),
            7 => Scene(ink),
            8 => Neutral(31 - (ink - 1) % WhiteCycleLength * WhiteCycleStep),
            9 => ObjectOne(ink),
            12 => ObjectFour(ink),
            13 => ObjectFive(ink),
            14 => Crossfade(ink),
            15 => MotherBrain(ink),
            _ => throw new ArgumentOutOfRangeException(nameof(row)),
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
