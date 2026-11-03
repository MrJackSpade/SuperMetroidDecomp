using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>
/// The shades a Blue Brinstar face block's pictures are pre-coloured in (#1512): each frame of the room-wide glow its
/// eyes cycle through once Samus has the Morph Ball. The import colours each from the installed face-block glow colours.
/// </summary>
internal static class FaceBlockShades
{
    /// <summary>The effect name of the face blocks' glow.</summary>
    internal const string GlowEffect = "face-block-glow";

    /// <summary>The glow's frames, in cycle order.</summary>
    internal const int GlowFrames = 8;

    /// <summary>The shade of glow frame <paramref name="frame"/>.</summary>
    internal static PictureShade Glow(int frame) => new(GlowEffect, Ensure.BetweenInclusive(0, GlowFrames - 1, frame));
}
