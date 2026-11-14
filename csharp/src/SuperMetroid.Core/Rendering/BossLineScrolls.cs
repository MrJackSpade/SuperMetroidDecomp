using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The line scrolls the bosses' published background effects draw as (#1468): Phantoon's wave as a sine wave down the
/// whole screen, Crocomire's melt as a stretch. The bosses publish their effects in their own terms; the display
/// describes them here.
/// </summary>
public static class BossLineScrolls
{
    /// <summary>Phantoon's wave as a sine wave over every screen line.</summary>
    /// <param name="wave">The wave the boss published.</param>
    public static ScrollWave Of(PhantoonWave wave) => new(Ensure.NotNull(wave).BaseScroll, wave.Amplitude, wave.Phase, wave.Period, 0,
        ScreenGeometry.ScreenHeightPixels);

    /// <summary>
    /// Crocomire's melt as a stretch of his body, in the rows of his body's area the screen's top shows (#1514, #1802): the
    /// boss publishes the room rows his body's area's top lies on, and <paramref name="camera"/> shows the area from below
    /// the camera row the screen hides.
    /// </summary>
    /// <param name="melt">The melt the boss published.</param>
    /// <param name="camera">The frame's camera, the room pixel at the screen's top-left.</param>
    public static ScrollStretch Of(CrocomireMeltDistortion melt, Position16<RoomPixels> camera) =>
        new(BodyRowAtScreenTop(Ensure.NotNull(melt).LevelledTop, camera), BodyRowAtScreenTop(melt.Top, camera), melt.FirstLine, melt.EndLine, melt.Slope);

    /// <summary>The row of a body's area whose top lies on room row <paramref name="top"/> that the screen's top shows against <paramref name="camera"/>.</summary>
    private static int BodyRowAtScreenTop(int top, Position16<RoomPixels> camera) =>
        GameplaySceneView.LayerRowAtScreenTop(CoordinateAxis.Wrap(camera.Y - top));
}
