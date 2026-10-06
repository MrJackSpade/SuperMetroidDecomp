namespace SuperMetroid.Core.Assets;

/// <summary>Calculated native cinematic shade composition and hurt white blend, with narrowly retained paint policies.</summary>
internal static class SamusHurtColorDefinitions
{
    /// <summary>$9B:A3A2..A3BE: the intro blue channel is two below red above the darkest shade. This selected tint is narrowly retained.</summary>
    internal const int BlueTintDrop = 2;
    /// <summary>$9B:A3A6 and repeated shade: the darkest blue is four. This selected outline endpoint also supplies the blue floor.</summary>
    internal const int OutlineLevel = 4;
    /// <summary>$9B:A382..A39E versus $9B:A3A2..A3BE: hurt blends five parts white to two parts source. The selected flash contrast ratio is narrowly retained.</summary>
    internal const int WhiteWeight = 5;
    /// <summary>$9B:A382..A39E: the complementary source weight in the exact seven-part blend; narrowly retained with the white weight.</summary>
    internal const int SourceWeight = 2;

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

    /// <summary>The selected outline-to-visor shade scale has three equal main intervals and inserted half shades.</summary>
    private const int MainShadeIntervals = 3;
    /// <summary>Maximum RGB5 channel value; full intensity at the visor end of this selected scale.</summary>
    private const int FullIntensity = (1 << 5) - 1;
    /// <summary>$9B:A3A2..A3BE: main gray shades plus half shades; the lowest half deliberately rounds darker.</summary>
    internal static byte DefaultIntroLevel(int index)
    {
        int Main(int step) => OutlineLevel + (FullIntensity - OutlineLevel) * step / MainShadeIntervals;
        int Half(int step, bool roundUp) => (Main(step) + Main(step + 1) + (roundUp ? 1 : 0)) / 2;
        return (byte)(IntroLevelSourceIndex(index) switch
        {
            1 => Main(1),
            2 => Main(2),
            3 => Main(0),
            4 => Main(MainShadeIntervals),
            6 => Half(2, true),
            8 => Half(1, true),
            11 => Half(0, false),
            _ => throw new InvalidOperationException("Unknown monochrome shade role."),
        });
    }

    /// <summary>$9B:A380: OBJ transparent-slot black, calculated from zero RGB channels.</summary>
    internal static ushort HurtTransparentWord => 0;
    /// <summary>$9B:A3A0: exact independently editable transparent payload shared with the previously reviewed normal suit palette.</summary>
    internal const ushort IntroTransparentWord = 14 << 10;

    /// <summary>Equal red/green channels and the bounded blue tint; gray levels calculate from the reviewed selected shade composition.</summary>
    internal static ushort IntroFromLevel(byte red)
    {
        int blue = Math.Max(OutlineLevel, red - BlueTintDrop);
        return (ushort)(red | red << 5 | blue << 10);
    }

    /// <summary>Floor-scaled RGB5 blend toward maximum white; applies only to indices 1..15, excluding the independently supplied zero slot.</summary>
    internal static ushort HurtFromIntro(ushort intro)
    {
        int Blend(int channel) => (SourceWeight * channel + WhiteWeight * FullIntensity) / (SourceWeight + WhiteWeight);
        return (ushort)(Blend(intro & 31) | Blend((intro >> 5) & 31) << 5 | Blend((intro >> 10) & 31) << 10);
    }
}
