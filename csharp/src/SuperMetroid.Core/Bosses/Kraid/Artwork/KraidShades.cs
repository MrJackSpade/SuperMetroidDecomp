using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The shades the import pre-colours Kraid's pictures in (#1512): his health bands, his death colours and rocks, and what
/// his room's backdrop shows through its fades, each fade's colours named by the colours it started from and the shared
/// counter's values at its steps.
/// </summary>
public static class KraidShades
{
    /// <summary>The shades of each backdrop fade: one per counter value its steps take, zero through thirteen.</summary>
    public const int BackdropFadeShades = 14;

    /// <summary>
    /// Kraid's colours for <paramref name="band"/>, a health band or the hurt flash: his body's on the background and his
    /// sprite parts' (#1512). The shade's step is the band's place among his health colours' groups.
    /// </summary>
    public static PictureShade Health(KraidColourBand band) => new("kraid-health", band.ColourGroup);

    /// <summary>His body's colours as he dies, with his arm drawn into them.</summary>
    public static PictureShade DeathArm { get; } = new("kraid-death-arm", 0);

    /// <summary>The colours his ceiling rocks and spat rocks draw in, which his initialization loads.</summary>
    public static PictureShade Rocks { get; } = new("kraid-rocks", 0);

    /// <summary>Kraid's backdrop colours, as his defeated room shows them.</summary>
    public static PictureShade Backdrop { get; } = new("kraid-backdrop", 0);

    /// <summary>The backdrop black, as his death fade leaves it.</summary>
    public static PictureShade BackdropBlack { get; } = new("kraid-backdrop-black", 0);

    /// <summary>
    /// The shade of what Kraid's backdrop shows: his colours, black, or a fade's colours, which the fade names by the
    /// chain of fades that led to them and the shared counter's first and latest value in each.
    /// </summary>
    /// <param name="shown">What the backdrop shows, other than the room's own colours.</param>
    /// <exception cref="ArgumentException">The backdrop shows the room's own colours, its pictures' standard colouring.</exception>
    internal static PictureShade BackdropShowing(KraidBackdropShown shown) => shown switch
    {
        KraidBackdropShown.Settled { Colours: KraidBackdropSettledColours.KraidColours } => Backdrop,
        KraidBackdropShown.Settled { Colours: KraidBackdropSettledColours.Black } => BackdropBlack,
        KraidBackdropShown.Settled { Colours: KraidBackdropSettledColours.RoomColours } => throw new ArgumentException(
            "The room's own backdrop colours are its pictures' standard colouring, which no shade names.", nameof(shown)),
        KraidBackdropShown.Fading fading => new($"kraid-backdrop-from-{ChainName(fading)}", ChainStep(fading)),
        _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined Kraid backdrop colours."),
    };

    /// <summary>The colours a chain of fades started from, then each fade's goal in turn.</summary>
    private static string ChainName(KraidBackdropShown shown) => shown switch
    {
        KraidBackdropShown.Settled settled => ColoursName(settled.Colours),
        KraidBackdropShown.Fading fading => $"{ChainName(fading.From)}-to-{ColoursName(fading.Goal)}",
        _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined Kraid backdrop colours."),
    };

    /// <summary>The counter values each fade of a chain stepped at, one bit per value, as the bit fields of one step.</summary>
    /// <exception cref="OverflowException">The chain holds more fades than one step can name.</exception>
    private static int ChainStep(KraidBackdropShown shown) => shown switch
    {
        KraidBackdropShown.Settled => 0,
        KraidBackdropShown.Fading fading => checked((ChainStep(fading.From) * (1 << BackdropFadeShades)) + fading.Steps.Taken),
        _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined Kraid backdrop colours."),
    };

    /// <summary>The name of settled backdrop colours in a shade's name.</summary>
    private static string ColoursName(KraidBackdropSettledColours colours) => colours switch
    {
        KraidBackdropSettledColours.RoomColours => "room-colours",
        KraidBackdropSettledColours.KraidColours => "kraid-colours",
        KraidBackdropSettledColours.Black => "black",
        _ => throw new ArgumentOutOfRangeException(nameof(colours), colours, "Undefined Kraid backdrop colours."),
    };
}
