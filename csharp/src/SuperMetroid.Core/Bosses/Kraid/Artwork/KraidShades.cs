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

    /// <summary>Kraid's backdrop colours, as his defeated room shows them.</summary>
    public static PictureShade Backdrop { get; } = new("kraid-backdrop", 0);

    /// <summary>The backdrop after the step of counter value <paramref name="value"/> of its fade from the room's colours into Kraid's.</summary>
    public static PictureShade BackdropFadingInFromRoomColours(int value) => new("kraid-backdrop-fading-in-from-room-colours", value);

    /// <summary>The backdrop after the step of counter value <paramref name="value"/> of its fade from Kraid's colours to black.</summary>
    public static PictureShade BackdropFadingOut(int value) => new("kraid-backdrop-fading-out", value);

    /// <summary>The backdrop after the step of counter value <paramref name="value"/> of its fade from black into Kraid's colours.</summary>
    public static PictureShade BackdropFadingInFromBlack(int value) => new("kraid-backdrop-fading-in-from-black", value);
}
