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
    /// Crocomire's melt as a stretch of his body layer, in the rows the screen's top shows (#1514): the boss publishes
    /// his body camera's rows, and the screen shows the layer from below the camera row it hides.
    /// </summary>
    /// <param name="melt">The melt the boss published.</param>
    public static ScrollStretch Of(CrocomireMeltDistortion melt) => new(GameplaySceneView.LayerRowAtScreenTop(Ensure.NotNull(melt).ScrollAbove),
        GameplaySceneView.LayerRowAtScreenTop(melt.BaseScroll), melt.FirstLine, melt.EndLine, melt.Slope);
}
