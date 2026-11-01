using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated cinematic shade composition and hurt white blend, with narrowly retained paint policies.</summary>
internal static class SamusHurtColorDefinitions
{
    /// <summary>The intro blue channel is two below red above the darkest shade. This selected tint is narrowly retained.</summary>
    internal const int BlueTintDrop = 2;
    /// <summary>The darkest blue is four. This selected outline endpoint also supplies the blue floor.</summary>
    internal const int OutlineLevel = 4;
    /// <summary>Hurt colours blend five parts white to two parts intro colour. The selected flash contrast ratio is narrowly retained.</summary>
    internal const int WhiteWeight = 5;
    /// <summary>The complementary source weight in the exact seven-part blend; narrowly retained with the white weight.</summary>
    internal const int SourceWeight = 2;

    /// <summary>The selected outline-to-visor shade scale has three equal main intervals and inserted half shades.</summary>
    internal const int MainShadeIntervals = 3;
    /// <summary>Maximum RGB5 channel value; full intensity at the visor end of this selected scale.</summary>
    internal const int FullIntensity = (1 << 5) - 1;
    /// <summary>Sprite transparent-slot black, calculated from zero RGB channels.</summary>
    internal static RgbColor HurtTransparentWord => RgbColor.Black;
    /// <summary>Exact independently editable transparent payload shared with the previously reviewed normal suit palette.</summary>
    internal static readonly RgbColor IntroTransparentWord = new(0, 0, 14);
}
