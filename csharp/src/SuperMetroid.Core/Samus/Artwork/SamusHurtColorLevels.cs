using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.SamusHurtColorDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The intro and hurt colour levels each step selects.</summary>
internal static class SamusHurtColorLevels
{
    /// <summary>$9B:A3A2..A3BE: named pen-role aliases in the selected monochrome shade composition.</summary>
    internal static int IntroLevelSourceIndex(int index) => index switch
    {
        1 or 5 or 14 => 1, // Armor/cannon/helmet shadow.
        2 or 7 => 2, // Armor/cannon highlight.
        3 => 3, // Outline.
        4 => 4, // Visor.
        6 => 6, // Bright specular points.
        8 or 9 or 10 or 12 => 8, // Middle material shade.
        11 or 13 or 15 => 11, // Deep material shade.
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>$9B:A3A2..A3BE: main gray shades plus half shades; the lowest half deliberately rounds darker.</summary>
    internal static int DefaultIntroLevel(int index)
    {
        int Main(int step) => OutlineLevel + (FullIntensity - OutlineLevel) * step / MainShadeIntervals;
        int Half(int step, int rounding) => (Main(step) + Main(step + 1) + rounding) / 2;
        return WordWrap.LowByte(IntroLevelSourceIndex(index) switch
        {
            1 => Main(1),
            2 => Main(2),
            3 => Main(0),
            4 => Main(MainShadeIntervals),
            6 => Half(2, rounding: 1),
            8 => Half(1, rounding: 1),
            11 => Half(0, rounding: 0),
            _ => throw new InvalidOperationException("Unknown monochrome shade role."),
        });
    }

    /// <summary>Equal red/green channels and the bounded blue tint; gray levels calculate from the reviewed selected shade composition.</summary>
    internal static RgbColor IntroFromLevel(int red)
    {
        int blue = Math.Max(OutlineLevel, red - BlueTintDrop);
        return new RgbColor(red, red, blue);
    }

    /// <summary>Floor-scaled RGB5 blend toward maximum white; applies only to indices 1..15, excluding the independently supplied zero slot.</summary>
    internal static RgbColor HurtFromIntro(RgbColor intro)
    {
        int Blend(int channel) => (SourceWeight * channel + WhiteWeight * FullIntensity) / (SourceWeight + WhiteWeight);
        return intro.Map((_, channel) => Blend(channel));
    }
}
