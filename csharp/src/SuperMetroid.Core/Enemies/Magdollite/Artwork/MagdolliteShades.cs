using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The shades a Magdollite's pictures are pre-coloured in (#1512): each frame of the room-wide lava glow every
/// Magdollite, and the lava it throws, cycles through. The import colours each from the installed Magdollite glow colours.
/// </summary>
internal static class MagdolliteShades
{
    /// <summary>The effect name of the Magdollites' glow.</summary>
    internal const string GlowEffect = "magdollite-glow";

    /// <summary>The glow's frames, in cycle order.</summary>
    internal const int GlowFrames = 4;

    /// <summary>The shade of glow frame <paramref name="frame"/>.</summary>
    internal static PictureShade Glow(int frame) => new(GlowEffect, Ensure.BetweenInclusive(0, GlowFrames - 1, frame));
}
