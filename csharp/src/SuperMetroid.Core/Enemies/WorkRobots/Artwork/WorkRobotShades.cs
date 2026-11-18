using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The shades a Work Robot's pictures are pre-coloured in (#1512): one for each place of the red glow powered robots cycle
/// through, which the cycle's steps showing that place share (#2393). The import colours each from the installed Work Robot
/// glow colours.
/// </summary>
internal static class WorkRobotShades
{
    /// <summary>The effect name of the powered robots' glow.</summary>
    internal const string GlowEffect = "work-robot-glow";

    /// <summary>The shade of glow place <paramref name="place"/>, keyed by its place among the glow's places (#2393).</summary>
    internal static PictureShade Glow(WorkRobotGlowPlace place) => new(GlowEffect, WorkRobotGlowAnimations.ShadeStep(place));
}
