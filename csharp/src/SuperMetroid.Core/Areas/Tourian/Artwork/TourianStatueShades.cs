using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// The shades the Tourian entrance statues' pictures are pre-coloured in (#1512): the statue scene's entrance colours,
/// the eye glow each released boss lights, and each boss statue's blacked-out eyes and grey once released. The import
/// colours them from the installed Tourian statue colours, on the statue's sprites and, for the statues drawn in the
/// room's background, on those pictures.
/// </summary>
public static class TourianStatueShades
{
    /// <summary>The statue scene's own colours, which its initialization installs.</summary>
    public static PictureShade Entrance { get; } = new("tourian-statue-entrance", 0);

    /// <summary>The eye glow Phantoon's statue lights once released.</summary>
    public static PictureShade PhantoonEyeGlow { get; } = new("tourian-statue-phantoon-eye-glow", 0);

    /// <summary>The eye glow Ridley's statue lights once released.</summary>
    public static PictureShade RidleyEyeGlow { get; } = new("tourian-statue-ridley-eye-glow", 0);

    /// <summary>The eye glow Draygon's statue lights once released.</summary>
    public static PictureShade DraygonEyeGlow { get; } = new("tourian-statue-draygon-eye-glow", 0);

    /// <summary>The eye glow Kraid's statue lights once released.</summary>
    public static PictureShade KraidEyeGlow { get; } = new("tourian-statue-kraid-eye-glow", 0);

    /// <summary>The eye glow <paramref name="statue"/> lights once released (#1747).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="statue"/> is not one boss's statue.</exception>
    public static PictureShade EyeGlow(TourianGreyStatues statue) => statue switch
    {
        TourianGreyStatues.Phantoon => PhantoonEyeGlow,
        TourianGreyStatues.Ridley => RidleyEyeGlow,
        TourianGreyStatues.Draygon => DraygonEyeGlow,
        TourianGreyStatues.Kraid => KraidEyeGlow,
        TourianGreyStatues.None => throw new ArgumentOutOfRangeException(nameof(statue), statue, "No statue lights an eye glow."),
        _ => throw new ArgumentOutOfRangeException(nameof(statue), statue, "Only one boss's statue lights an eye glow."),
    };

    /// <summary>Phantoon's statue with its eyes blacked out.</summary>
    public static PictureShade PhantoonCleared { get; } = new("tourian-statue-phantoon-cleared", 0);

    /// <summary>Phantoon's statue greyed.</summary>
    public static PictureShade PhantoonGrey { get; } = new("tourian-statue-phantoon-grey", 0);

    /// <summary>Ridley's statue with its eyes blacked out.</summary>
    public static PictureShade RidleyCleared { get; } = new("tourian-statue-ridley-cleared", 0);

    /// <summary>Ridley's statue greyed.</summary>
    public static PictureShade RidleyGrey { get; } = new("tourian-statue-ridley-grey", 0);

    /// <summary>Kraid's statue with its eyes blacked out.</summary>
    public static PictureShade KraidCleared { get; } = new("tourian-statue-kraid-cleared", 0);

    /// <summary>Kraid's statue greyed.</summary>
    public static PictureShade KraidGrey { get; } = new("tourian-statue-kraid-grey", 0);

    /// <summary>Draygon's statue with its eyes blacked out.</summary>
    public static PictureShade DraygonCleared { get; } = new("tourian-statue-draygon-cleared", 0);

    /// <summary>Draygon's statue greyed.</summary>
    public static PictureShade DraygonGrey { get; } = new("tourian-statue-draygon-grey", 0);

    /// <summary>The looks of the statues drawn in the room's background, Kraid's and Draygon's, which the room's pictures show.</summary>
    public static IReadOnlySet<PictureShade> RoomPictureLooks { get; } = new HashSet<PictureShade> { KraidCleared, KraidGrey, DraygonCleared, DraygonGrey };
}
