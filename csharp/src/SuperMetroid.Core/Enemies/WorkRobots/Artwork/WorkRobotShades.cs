using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The shades a Work Robot's pictures are pre-coloured in (#1512): one for each place of the red glow powered robots cycle
/// through, which the cycle's steps showing that place share (#2393). The import colours each from the installed Work Robot
/// glow colours.
/// </summary>
internal static partial class WorkRobotShades
{
    /// <summary>
    /// The installed name of each glow place's shade (#2511), written out so renaming a place's identity never changes a name
    /// on disk.
    /// </summary>
    private static readonly FrozenDictionary<WorkRobotGlowPlace, string> GlowKeys =
        new Dictionary<WorkRobotGlowPlace, string>
        {
            [WorkRobotGlowPlace.AtFirstEnd] = "work-robot-glow-AtFirstEnd",
            [WorkRobotGlowPlace.NearFirstEnd] = "work-robot-glow-NearFirstEnd",
            [WorkRobotGlowPlace.NearFarEnd] = "work-robot-glow-NearFarEnd",
            [WorkRobotGlowPlace.AtFarEnd] = "work-robot-glow-AtFarEnd",
        }.ToFrozenDictionary();

    /// <summary>
    /// The shade of glow place <paramref name="place"/> (#2393), named by the place (#2451), as Kraid's shades are named by
    /// his health band (#2297).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The place has no installed name.</exception>
    internal static PictureShade Glow(WorkRobotGlowPlace place) => new(Ensure.Listed(GlowKeys, place), 0);
}
