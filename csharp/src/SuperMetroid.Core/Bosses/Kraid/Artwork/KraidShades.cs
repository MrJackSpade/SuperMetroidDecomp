using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The shades the import pre-colours Kraid's room pictures in for his backdrop's fades (#1512), one per step taken from
/// none through the completing step: the fade the room's backdrop takes as he grows, and the fades out to black and back
/// in as he dies.
/// </summary>
public static class KraidShades
{
    /// <summary>The shades of each backdrop fade: one per step taken, from none through all fourteen.</summary>
    public const int BackdropFadeShades = 15;

    /// <summary>Kraid's backdrop colours, as his defeated room shows them.</summary>
    public static PictureShade Backdrop { get; } = new("kraid-backdrop", 0);

    /// <summary>The backdrop after <paramref name="steps"/> steps of its fade from the room's colours into Kraid's.</summary>
    public static PictureShade BackdropFadingInFromRoomColours(int steps) => new("kraid-backdrop-fading-in-from-room-colours", steps);

    /// <summary>The backdrop after <paramref name="steps"/> steps of its fade from Kraid's colours to black.</summary>
    public static PictureShade BackdropFadingOut(int steps) => new("kraid-backdrop-fading-out", steps);

    /// <summary>The backdrop after <paramref name="steps"/> steps of its fade from black into Kraid's colours.</summary>
    public static PictureShade BackdropFadingInFromBlack(int steps) => new("kraid-backdrop-fading-in-from-black", steps);
}
