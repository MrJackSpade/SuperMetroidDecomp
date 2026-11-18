using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The shades a Work Robot's pictures are pre-coloured in (#1512): one for each place of the red glow powered robots cycle
/// through, which the cycle's steps showing that place share (#2393). The import colours each from the installed Work Robot
/// glow colours.
/// </summary>
internal static class WorkRobotShades
{
    /// <summary>
    /// The shade of glow place <paramref name="place"/> (#2393), named by the place (#2451), as Kraid's shades are named by
    /// his health band (#2297).
    /// </summary>
    internal static PictureShade Glow(WorkRobotGlowPlace place) => new($"work-robot-glow-{place}", 0);
}
