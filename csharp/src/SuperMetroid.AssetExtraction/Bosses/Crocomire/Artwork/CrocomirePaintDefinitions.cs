using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;

/// <summary>
/// Selected Crocomire material inks. These choices paint the red
/// hide, bone, masonry, projectile and spike illustrations; they are not samples
/// of movement, damage or health. Shared channels, shade ramps and aliases calculate.
/// No complete stock palette is stored. Independently supplied inks belong to the catalog.
/// </summary>
/// <remarks>
/// Initialization copies the wall/projectile targets, the hurt restoration restores
/// body inks, the death sequence copies spike colors and the wall break copies arm colors.
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
    /// <summary>Copied sprite ink-zero metadata; sprite rendering skips ink zero.</summary>
    internal const int TransparentBlue = 14;
    /// <summary>Saturated red hide highlight with selected warm green and blue channels.</summary>
    internal const int HideHighlightGreen = 15, HideHighlightBlue = 3;
    /// <summary>Selected darker red hide intensities; green falls one level per contour shade.</summary>
    internal const int HideMiddleRed = 21, HideDeepRed = 12, HideOutlineRed = 7;
    /// <summary>Warm-contour green peak and capped half-green blue contribution.</summary>
    internal const int HideGreenPeak = 5, HideBlueCap = 2;
    /// <summary>Lightest selected bone paint, also used by the body teeth.</summary>
    internal const int BoneLightRed = 30, BoneLightGreen = 27, BoneLightBlue = 26;
    /// <summary>Four bone shades use nearest red interpolation to this darkest intensity.</summary>
    internal const int BoneDarkRed = 11;
    /// <summary>Bone green is red minus the warm tint, clipped at its selected darkest green.</summary>
    internal const int BoneGreenTint = 6, BoneGreenFloor = 6;
    /// <summary>Selected blue channels of the four differently warm bone facets.</summary>
    internal const int BoneBlueLight = 21, BoneBlueMiddle = 14, BoneBlueDeep = 8, BoneBlueDark = 3;
    /// <summary>Copied gold accents shared with the original body OBJ palette, outside the eight-word fight field.</summary>
    internal const int BoneGoldLightGreen = 28, BoneGoldMiddleGreen = 17,
        BoneGoldDarkRed = 22, BoneGoldDarkGreen = 11;
    /// <summary>Repeated neutral masonry paint.</summary>
    internal const int WallNeutral = 24;
    /// <summary>Independently chosen masonry face, recess and edge paints.</summary>
    internal static readonly RgbColor WallFace = new(222, 156, 148), WallRecess = new(132, 33, 57), WallEdge = new(74, 24, 41), WallDeepEdge = new(33, 8, 41);
    /// <summary>Selected red-only deepest masonry mark.</summary>
    internal const int WallRedMark = 8;
    /// <summary>Masonry shares hide red/green contour shading but selects violet-blue tint levels.</summary>
    internal const int WallTintLight = 9, WallTintDeep = 7;
    /// <summary>Independent darkest wall paint, including its selected red-channel endpoint.</summary>
    internal static readonly RgbColor WallDark = new(41, 16, 33);
    /// <summary>Full-blue projectile glint with equal selected red and green.</summary>
    internal const int ProjectileGlint = 26;
    /// <summary>Yellow/orange projectile core endpoints; red uses floor gamma-two interpolation and green floor linear interpolation. Blue is absent.</summary>
    internal const int FlameLightRed = 27, FlameLightGreen = 25, FlameDarkRed = 19, FlameDarkGreen = 8;
    /// <summary>Violet projectile rim runs from full blue to its chosen darkest red with a shared blue excess.</summary>
    internal const int VioletDarkRed = 10, VioletBlueTint = 2;
    /// <summary>Green projectile endpoints; floor gamma-two interpolation supplies the middle intensity and blue scales proportionally from its highlight.</summary>
    internal const int GreenDark = 12, GreenBluePeak = 14;
    /// <summary>Linear neutral projectile ramp endpoints.</summary>
    internal const int ProjectileNeutralLight = 20, ProjectileNeutralDark = 8;
    /// <summary>Dim violet outline has half the neutral shadow intensity and this blue excess.</summary>
    internal const int ProjectileOutlineBlueTint = 5;
    /// <summary>Gold spike mounting shades, with a selected middle red level and green endpoint interpolation.</summary>
    internal const int SpikeGoldMiddleRed = 23, SpikeGoldDarkRed = 12,
        SpikeGoldLightGreen = 22, SpikeGoldDarkGreen = 5;
    /// <summary>Four cool steel shades interpolate downward with floor rounding and a shared blue excess.</summary>
    internal const int SteelLight = 19, SteelDark = 3, SteelBlueTint = 3;
    /// <summary>Three additional copied spike-palette accent paints; no visible-role claim is made for this fragment fixture.</summary>
    internal static readonly RgbColor SpikeAccent = new(140, 239, 16), SpikeWarmAccent = new(222, 57, 148), SpikeCoolAccent = new(41, 57, 140);
}
