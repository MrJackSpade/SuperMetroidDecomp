using System.Collections.Frozen;
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
    /// The installed name of each glow level's shade (#2511), written out so renaming a level's identity never changes a name
    /// on disk.
    /// </summary>
    private static readonly FrozenDictionary<FaceBlockGlowLevel, string> GlowKeys =
        new Dictionary<FaceBlockGlowLevel, string>
        {
            [FaceBlockGlowLevel.Dimmest] = "face-block-glow-Dimmest",
            [FaceBlockGlowLevel.Dim] = "face-block-glow-Dim",
            [FaceBlockGlowLevel.Bright] = "face-block-glow-Bright",
            [FaceBlockGlowLevel.Brightest] = "face-block-glow-Brightest",
        }.ToFrozenDictionary();

    /// <summary>
    /// The shade of glow level <paramref name="level"/> (#2477), named by the level, as the Work Robots' glow shades are named
    /// by place (#2451).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The level has no installed name.</exception>
    internal static PictureShade Glow(FaceBlockGlowLevel level) => new(Ensure.Listed(GlowKeys, level), 0);
}
