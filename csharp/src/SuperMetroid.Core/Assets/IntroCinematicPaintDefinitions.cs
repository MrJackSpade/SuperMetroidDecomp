using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

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
    private const ushort ObjectZero = 0x3800;
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
    private const ushort Shoulder = 0x15aa;
    /// <summary>$8C:E455: dark cyan contour.</summary>
    private const ushort PortraitContour = 0x14a2;
    /// <summary>$8C:E461: deep cool silhouette paint.</summary>
    private const ushort PortraitDeep = 0x1c42;
    /// <summary>$8C:E44B: blue-only outline intensity.</summary>
    private const int PortraitOutlineBlue = 5;

    internal static bool Matches(ReadOnlySpan<byte> bytes)
    {
        for (int index = 0; index < 256; index++)
            if (BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(index * 2, 2)) != Color(index / 16, index % 16)) return false;
        return true;
    }

    internal static ushort Color(int row, int ink)
    {
        if ((uint)row >= 16 || (uint)ink >= 16) throw new ArgumentOutOfRangeException(nameof(row));
        if (row == 0) return Font(ink);
        if (ink == 0) return row >= 8 ? ObjectZero : (ushort)0;
        return row switch
        {
            1 => ink switch { 1 or 4 => Neutral(31), 2 or 14 => 0, 3 or 6 => Neutral(TextGray), _ => Neutral(Fill) },
            2 => ink <= 8 ? Sepia(ink) : ink == 15 ? (ushort)0 : Neutral(Fill),
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

    private static ushort Font(int ink)
    {
        int group = ink / 4;
        return (ink % 4) switch
        {
            0 when group == 0 => 0,
            0 => FontZero(group - 1),
            1 => Pack(0, 31, 0),
            2 => Neutral(Fill),
            _ => Pack(0, FontShadow(group + 1), 0),
        };
    }
    private static int FontShadow(int depth)
    {
        if (depth == 4) return FontShadowDark;
        double intensity = (Math.Sqrt(31) * (4 - depth) + Math.Sqrt(FontShadowDark) * depth) / 4;
        return (int)Math.Ceiling(intensity * intensity);
    }
    private static ushort FontZero(int shade)
    {
        int green = Gamma(31, FontZeroGreenDark, shade, 2);
        return Pack(0, green, FontZeroBlue * green / 31);
    }
    private static ushort Sepia(int ink)
    {
        int level = ink <= 6 ? (31 * (6 - ink) + SepiaDark * (ink - 1) + SepiaIntervals - 1) / SepiaIntervals
            : SepiaDark - (ink - 6) * SepiaOutlineStep;
        return Pack(level, level, Math.Max(0, level - SepiaBlueDeficit));
    }
    private static ushort Warm(int ink, int tint)
    {
        ushort color = Sepia(ink);
        return Pack(Math.Min(31, (color & 31) + tint), Math.Min(31, (color >> 5 & 31) + tint), color >> 10);
    }
    private static ushort Plate()
    {
        ushort color = MotherBrainHealthPaintDefinitions.PlateHighlight;
        return Pack((color & 31) - PlateShade, (color >> 5 & 31) - PlateShade, (color >> 10) - PlateShade);
    }
    private static ushort Scene(int ink) => ink switch
    {
        1 => Neutral(SceneHighlight), 2 => Sepia(4), 3 => Pack(SceneDark, SceneDark, SceneDarkBlue),
        4 => Neutral(Contour), 5 or 9 => Plate(), 6 => Warm(5, WarmTint), 7 => Neutral(MiddleNeutral),
        8 => Neutral(LowNeutral), 10 => Warm(5, StrongWarmTint), 11 => Warm(6, StrongWarmTint),
        12 => Sepia(7), 13 or 14 => Sepia(1), _ => 0,
    };
    private static ushort ObjectOne(int ink) => ink switch
    {
        1 or 5 or 10 or 13 or 15 => Plate(), 2 or 6 or 11 => Warm(5, WarmTint),
        3 or 7 or 12 => Warm(6, WarmTint), 4 or 8 => Sepia(7),
        9 => Pack(ObjectAccentRed, 0, ObjectAccentBlue), 14 => Sepia(1), _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    private static ushort ObjectFour(int ink) => ink switch
    {
        1 or 5 or 14 => Sepia(5), 2 or 7 => Sepia(3), 3 => Neutral(Contour), 4 => Sepia(1), 6 => Sepia(2),
        8 or 9 or 10 or 12 => Sepia(4), 11 or 13 or 15 => Sepia(6), _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    private static ushort ObjectFive(int ink) => ink switch
    {
        1 or 15 => Sepia(1), 14 => Neutral(Contour),
        _ => ((ink - 2) % 3) switch { 0 => Warm(4, StrongWarmTint), 1 => Warm(5, WarmTint), _ => Sepia(6) },
    };
    private static ushort Crossfade(int ink) => ink switch
    {
        <= 8 => Sepia(ink), <= 12 => Pack(0, 0, (31 * (12 - ink) + CrossfadeBlueDark * (ink - 9)) / 3),
        13 => Pack(0, 31, 0), 14 => Pack(0, CrossfadeAccentGreen, CrossfadeAccentBlue), _ => 0,
    };
    private static ushort MotherBrain(int ink) => ink switch
    {
        1 or 13 or 14 => Sepia(1), 2 => Sepia(4), 3 => Neutral(Contour), 4 => Neutral(DeepContour),
        5 => MotherBrainHealthPaintDefinitions.PlateHighlight, 6 => Warm(5, WarmTint), 7 => Neutral(PlateNeutral),
        8 => Neutral(PlateDarkNeutral), 9 => Plate(), 10 => Warm(5, StrongWarmTint), 11 => Warm(6, StrongWarmTint),
        12 => Sepia(7), _ => 0,
    };
    private static ushort Portrait(int ink)
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
        return ink switch { 1 => Pack(0, 0, PortraitOutlineBlue), 2 => Shoulder, 3 => 0,
            4 => Pack(0, VisorGlintGreen, VisorGlintBlue), 6 => PortraitContour, 12 => PortraitDeep,
            _ => throw new ArgumentOutOfRangeException(nameof(ink)) };
    }
    private static int Gamma(int first, int last, int shade, int intervals)
    {
        double intensity = (Math.Sqrt(first) * (intervals - shade) + Math.Sqrt(last) * shade) / intervals;
        return (int)Math.Floor(intensity * intensity + 0.5);
    }
    private static ushort Neutral(int value) => Pack(value, value, value);
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
