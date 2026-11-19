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
    /// <summary>
    /// Phantoon's wave as a sine wave over every screen line (#1858, #2031): his body ripples about where it is placed, and
    /// <paramref name="camera"/> shows it from there.
    /// </summary>
    /// <remarks>
    /// The wave's base scroll is the camera's signed distance from the placement (#2308), as the body's shown scroll is
    /// (<see cref="Enemies.BossBodies.BossBodyPlacement.Shown"/>): negative while his body lies right of the camera's corner.
    /// </remarks>
    /// <param name="wave">The wave the boss published.</param>
    /// <param name="camera">The frame's camera, the room pixel at the screen's top-left.</param>
    /// <param name="placement">Where his body is placed, the room pixel each of its frames lies against.</param>
    public static ScrollWave Of(PhantoonWave wave, Position16<RoomPixels> camera, Position16<RoomPixels> placement) =>
        new(CameraPastRoomEdge.CameraPast(camera.X, placement.X), Ensure.NotNull(wave).Amplitude, wave.Phase, wave.Period, 0,
            ScreenGeometry.ScreenHeightPixels);

    /// <summary>
    /// Crocomire's melt as a stretch of his body, in the rows of his body the screen's top shows (#1514, #1802, #2031): the
    /// boss publishes the room rows his body lies on, each frame's rows reckoned from there, and <paramref name="camera"/>
    /// shows his body from the camera's row.
    /// </summary>
    /// <param name="melt">The melt the boss published.</param>
    /// <param name="camera">The frame's camera, the room pixel at the screen's top-left.</param>
    public static ScrollStretch Of(CrocomireMeltDistortion melt, Position16<RoomPixels> camera) =>
        new(BodyRowAtScreenTop(Ensure.NotNull(melt).LevelledTop, camera), BodyRowAtScreenTop(melt.Top, camera), melt.FirstLine, melt.EndLine, melt.Slope);

    /// <summary>
    /// The row of a body placed on room row <paramref name="top"/> that the screen's top shows against
    /// <paramref name="camera"/>, the camera's row (#2573), a signed row (#2308): negative while the body's row lies below
    /// the screen's top.
    /// </summary>
    private static int BodyRowAtScreenTop(int top, Position16<RoomPixels> camera) =>
        CameraPastRoomEdge.CameraPast(camera.Y, top);
}
