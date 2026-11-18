using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The shades a Magdollite's pictures are pre-coloured in (#1512): each frame of the room-wide lava glow every
/// Magdollite, and the lava it throws, cycles through. The import colours each from the installed Magdollite glow colours.
/// </summary>
internal static class MagdolliteShades
{
    /// <summary>The glow's frames, in cycle order: the installed glow colours hold one row for each.</summary>
    internal const int GlowFrames = 4;

    /// <summary>
    /// The shade of glow frame <paramref name="glow"/>, named by the frame it shows (#2474), as the Work Robot's shades are
    /// named by the place of its glow (#2451).
    /// </summary>
    internal static PictureShade Glow(MagdolliteGlow glow) => new($"magdollite-glow-{glow}", 0);
}
