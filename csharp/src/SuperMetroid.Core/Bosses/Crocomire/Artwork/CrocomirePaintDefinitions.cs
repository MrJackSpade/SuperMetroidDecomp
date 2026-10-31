using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Selected Crocomire material inks at $A4:B89D-B93C. These choices paint the red
/// hide, bone, masonry, projectile and spike illustrations; they are not samples
/// of movement, damage or health. Shared channels, shade ramps and aliases calculate.
/// No complete stock palette is stored. Independently supplied inks belong to the catalog.
/// </summary>
/// <remarks>
/// Native $A4:8A99-8AA9 copies the wall/projectile targets, $8CEF-8CF8 restores
/// body inks, $9870-9879 copies spike colors and $999E-99A8 copies arm colors.
/// These consumers install selected pen colors without deriving them from a
/// physical state. The residual named hues/intensities and material assignments
/// are the illustration content itself: replacing them with a generated color
/// rule would invent different paint. This narrow content boundary also accounts
/// for uniformly copied accent inks not selected by the representative poses;
/// absence from those poses is not an exemption. All demonstrated interpolation,
/// channel equality, tint, neutral, saturation and alias relations calculate.
/// Movement, hurt-flash cadence, collision, pixels and unrelated palettes are excluded.
/// </remarks>
internal static class CrocomirePaintDefinitions
{
    /// <summary>$A4:B8BD/B8DD/B8FD/B91D: copied OBJ ink-zero metadata $3800; OBJ rendering skips ink zero.</summary>
    internal const int TransparentBlue = 14;
    /// <summary>$A4:B8A1: saturated red hide highlight with selected warm green and blue channels.</summary>
    internal const int HideHighlightGreen = 15, HideHighlightBlue = 3;
    /// <summary>$A4:B8A5-B8A9: selected darker red hide intensities; green falls one level per contour shade.</summary>
    internal const int HideMiddleRed = 21, HideDeepRed = 12, HideOutlineRed = 7;
    /// <summary>$A4:B8A3-B8A9: warm-contour green peak and capped half-green blue contribution.</summary>
    internal const int HideGreenPeak = 5, HideBlueCap = 2;
    /// <summary>$A4:B8AB/B901: lightest selected bone paint, also used by the body teeth.</summary>
    internal const int BoneLightRed = 30, BoneLightGreen = 27, BoneLightBlue = 26;
    /// <summary>$A4:B903-B909: four bone shades use nearest red interpolation to this darkest intensity.</summary>
    internal const int BoneDarkRed = 11;
    /// <summary>$A4:B903-B909: bone green is red minus the warm tint, clipped at its selected darkest green.</summary>
    internal const int BoneGreenTint = 6, BoneGreenFloor = 6;
    /// <summary>$A4:B903-B909: selected blue channels of the four differently warm bone facets.</summary>
    internal const int BoneBlueLight = 21, BoneBlueMiddle = 14, BoneBlueDeep = 8, BoneBlueDark = 3;
    /// <summary>$A4:B915-B919: copied gold accents shared with the original body OBJ palette, outside the eight-word fight field.</summary>
    internal const int BoneGoldLightGreen = 28, BoneGoldMiddleGreen = 17,
        BoneGoldDarkRed = 22, BoneGoldDarkGreen = 11;
    /// <summary>$A4:B8C1-B8C9: repeated neutral masonry paint.</summary>
    internal const int WallNeutral = 24;
    /// <summary>$A4:B8CB-B8D3: independently chosen masonry face, recess and edge paints.</summary>
    internal static readonly RgbColor WallFace = new(27, 19, 18), WallRecess = new(16, 4, 7), WallEdge = new(9, 3, 5), WallDeepEdge = new(4, 1, 5);
    /// <summary>$A4:B8D3: selected red-only deepest masonry mark.</summary>
    internal const int WallRedMark = 8;
    /// <summary>$A4:B8D5-B8D9: masonry shares hide red/green contour shading but selects violet-blue tint levels.</summary>
    internal const int WallTintLight = 9, WallTintDeep = 7;
    /// <summary>$A4:B8DB: independent darkest wall paint, including its selected red-channel endpoint.</summary>
    internal static readonly RgbColor WallDark = new(5, 2, 4);
    /// <summary>$A4:B8DF: full-blue projectile glint with equal selected red and green.</summary>
    internal const int ProjectileGlint = 26;
    /// <summary>$A4:B8E1-B8E5: yellow/orange projectile core endpoints; red uses floor gamma-two interpolation and green floor linear interpolation. Blue is absent.</summary>
    internal const int FlameLightRed = 27, FlameLightGreen = 25, FlameDarkRed = 19, FlameDarkGreen = 8;
    /// <summary>$A4:B8E7-B8EB: violet projectile rim runs from full blue to its chosen darkest red with a shared blue excess.</summary>
    internal const int VioletDarkRed = 10, VioletBlueTint = 2;
    /// <summary>$A4:B8ED-B8F1: green projectile endpoints; floor gamma-two interpolation supplies the middle intensity and blue scales proportionally from its highlight.</summary>
    internal const int GreenDark = 12, GreenBluePeak = 14;
    /// <summary>$A4:B8F3-B8F7: linear neutral projectile ramp endpoints.</summary>
    internal const int ProjectileNeutralLight = 20, ProjectileNeutralDark = 8;
    /// <summary>$A4:B8F9: dim violet outline has half the neutral shadow intensity and this blue excess.</summary>
    internal const int ProjectileOutlineBlueTint = 5;
    /// <summary>$A4:B91F-B923: gold spike mounting shades, with a selected middle red level and green endpoint interpolation.</summary>
    internal const int SpikeGoldMiddleRed = 23, SpikeGoldDarkRed = 12,
        SpikeGoldLightGreen = 22, SpikeGoldDarkGreen = 5;
    /// <summary>$A4:B925-B92B: four cool steel shades interpolate downward with floor rounding and a shared blue excess.</summary>
    internal const int SteelLight = 19, SteelDark = 3, SteelBlueTint = 3;
    /// <summary>$A4:B92D/B92F/B937: three additional copied spike-palette accent paints; no visible-role claim is made for this fragment fixture.</summary>
    internal static readonly RgbColor SpikeAccent = new(17, 29, 2), SpikeWarmAccent = new(27, 7, 18), SpikeCoolAccent = new(5, 7, 17);
}
