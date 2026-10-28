using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Which layers each gameplay line shows while Mother Brain rises through the floor.</summary>
internal static class MotherBrainAscentDisplay
{
    /// <summary>Below the HUD: the foreground, overlay and sprites, then the foreground, background and sprites, then the foreground and overlay on the floor band.</summary>
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
