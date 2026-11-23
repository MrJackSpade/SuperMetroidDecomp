using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The shades a Magdollite's pictures are pre-coloured in (#1512): each frame of the room-wide lava glow every
/// Magdollite, and the lava it throws, cycles through. The import colours each from the installed Magdollite glow colours.
/// </summary>
internal static partial class MagdolliteShades
{
    /// <summary>
    /// The installed name of each glow frame's shade (#2511), written out so renaming a frame's identity never changes a name
    /// on disk.
    /// </summary>
    private static readonly FrozenDictionary<MagdolliteGlow, string> GlowKeys =
        new Dictionary<MagdolliteGlow, string>
        {
            [MagdolliteGlow.OnBrightestColour] = "magdollite-glow-OnBrightestColour",
            [MagdolliteGlow.OnDarkestColour] = "magdollite-glow-OnDarkestColour",
            [MagdolliteGlow.OnSecondDarkestColour] = "magdollite-glow-OnSecondDarkestColour",
            [MagdolliteGlow.OnSecondBrightestColour] = "magdollite-glow-OnSecondBrightestColour",
        }.ToFrozenDictionary();

    /// <summary>
    /// The shade of glow frame <paramref name="glow"/>, named by the frame it shows (#2474), as the Work Robot's shades are
    /// named by the place of its glow (#2451).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame has no installed name.</exception>
    internal static PictureShade Glow(MagdolliteGlow glow) => new(GlowKeys[glow], 0);
}
