using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>
/// The shades a Blue Brinstar face block's pictures are pre-coloured in (#1512): one for each level of the room-wide glow its
/// eyes cycle through once Samus has the Morph Ball, which the cycle's steps showing that level share (#2477). The import
/// colours each from the installed face-block glow colours.
/// </summary>
internal static class FaceBlockShades
{
    /// <summary>
    /// The shade of glow level <paramref name="level"/> (#2477), named by the level, as the Work Robots' glow shades are named
    /// by place (#2451).
    /// </summary>
    internal static PictureShade Glow(FaceBlockGlowLevel level) => new($"face-block-glow-{level}", 0);
}
