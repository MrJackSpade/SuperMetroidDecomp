using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>
/// The shades a Work Robot's pictures are pre-coloured in (#1512): each step of the red glow powered robots cycle
/// through. The import colours each from the installed Work Robot glow colours.
/// </summary>
internal static class WorkRobotShades
{
    /// <summary>The effect name of the powered robots' glow.</summary>
    internal const string GlowEffect = "work-robot-glow";

    /// <summary>The shade of glow step <paramref name="step"/>.</summary>
    internal static PictureShade Glow(int step) =>
        new(GlowEffect, Ensure.BetweenInclusive(0, WorkRobotPaletteTimingDefinitions.StepCount - 1, step));
}
