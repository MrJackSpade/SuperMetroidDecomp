using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Which main-screen layers each gameplay line shows while Mother Brain rises through the floor.</summary>
internal static class MotherBrainAscentDisplay
{
    /// <summary>$88:E73D selects BG3 for the 32-line HUD, then $15/$13/$05 for the gameplay bands.</summary>
    internal static ScreenLayers[] BuildGameplayLayers()
    {
        var layers = new ScreenLayers[ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels];
        for (int y = ScreenGeometry.GameplayHudHeightPixels; y < ScreenGeometry.ScreenHeightPixels; y++)
            layers[y - ScreenGeometry.GameplayHudHeightPixels] = y < MotherBrainAscentDisplayDefinitions.UpperBandEnd
                ? ScreenLayers.Foreground | ScreenLayers.Overlay | ScreenLayers.Sprites
                : y < MotherBrainAscentDisplayDefinitions.FloorBandStart
                    ? ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites
                    : ScreenLayers.Foreground | ScreenLayers.Overlay;
        return layers;
    }
}
