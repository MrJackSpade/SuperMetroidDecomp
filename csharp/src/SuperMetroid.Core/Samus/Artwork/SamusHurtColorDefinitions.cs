using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Artwork;

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

    /// <summary>The selected outline-to-visor shade scale has three equal main intervals and inserted half shades.</summary>
    internal const int MainShadeIntervals = 3;
    /// <summary>Maximum RGB5 channel value; full intensity at the visor end of this selected scale.</summary>
    internal const int FullIntensity = (1 << 5) - 1;
    /// <summary>$9B:A380: OBJ transparent-slot black, calculated from zero RGB channels.</summary>
    internal static RgbColor HurtTransparentWord => RgbColor.Black;
    /// <summary>$9B:A3A0: exact independently editable transparent payload shared with the previously reviewed normal suit palette.</summary>
    internal static readonly RgbColor IntroTransparentWord = new(0, 0, 14);
}
