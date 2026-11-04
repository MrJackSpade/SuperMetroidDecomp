using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The shades the import pre-colours Kraid's room pictures in for his backdrop's fades (#1512), one per value of the
/// shared counter a step can take, each showing the colours after the steps from zero through it: the fade the room's
/// backdrop takes as he grows, and the fades out to black and back in as he dies.
/// </summary>
public static class KraidShades
{
    /// <summary>The shades of each backdrop fade: one per counter value its steps take, zero through thirteen.</summary>
    public const int BackdropFadeShades = 14;

    /// <summary>
    /// Kraid's colours for health band <paramref name="band"/>, zero through seven, or the hurt flash's for minus one: his
    /// body's on layer 2 and his sprite parts' (#1512).
    /// </summary>
    public static PictureShade Health(int band) => new("kraid-health", band + 1);

    /// <summary>His body's colours as he dies, with his arm drawn into them.</summary>
    public static PictureShade DeathArm { get; } = new("kraid-death-arm", 0);

    /// <summary>The colours his ceiling rocks and spat rocks draw in, which his initialization loads.</summary>
    public static PictureShade Rocks { get; } = new("kraid-rocks", 0);

    /// <summary>Kraid's backdrop colours, as his defeated room shows them.</summary>
    public static PictureShade Backdrop { get; } = new("kraid-backdrop", 0);

    /// <summary>The backdrop after the step of counter value <paramref name="value"/> of its fade from the room's colours into Kraid's.</summary>
    public static PictureShade BackdropFadingInFromRoomColours(int value) => new("kraid-backdrop-fading-in-from-room-colours", value);

    /// <summary>The backdrop after the step of counter value <paramref name="value"/> of its fade from Kraid's colours to black.</summary>
    public static PictureShade BackdropFadingOut(int value) => new("kraid-backdrop-fading-out", value);

    /// <summary>
    /// The backdrop after the step of counter value <paramref name="value"/> of its fade to black from the room's colours,
    /// which began at counter value <paramref name="first"/>: Kraid died before his growth faded the backdrop in.
    /// </summary>
    public static PictureShade BackdropFadingOutFromRoomColours(int first, int value) =>
        new("kraid-backdrop-fading-out-from-room-colours", Pair(first, value));

    /// <summary>
    /// The backdrop after the step of counter value <paramref name="value"/> of its fade to black from the colours step
    /// <paramref name="lastFadeIn"/> of his growth fade showed: Kraid died during that fade, which the fade to black
    /// continues the shared counter of.
    /// </summary>
    public static PictureShade BackdropFadingOutFromFadingIn(int lastFadeIn, int value) =>
        new("kraid-backdrop-fading-out-from-fading-in", Pair(lastFadeIn, value));

    /// <summary>One shade step naming two counter values of a fade.</summary>
    private static int Pair(int first, int value) => first * BackdropFadeShades + value;

    /// <summary>The backdrop after the step of counter value <paramref name="value"/> of its fade from black into Kraid's colours.</summary>
    public static PictureShade BackdropFadingInFromBlack(int value) => new("kraid-backdrop-fading-in-from-black", value);
}
