namespace SuperMetroid.Core.Rendering;

/// <summary>Native main-screen layer selection while Mother Brain rises through the floor.</summary>
internal static class MotherBrainAscentDisplayDefinitions
{
    /// <summary>$88:E73D, HDMATable_MotherBrainRising_MainScreenLayers: first gameplay band ends after physical line 55.</summary>
    private const int UpperBandEnd = 56;
    /// <summary>$88:E745, the final eight-line floor band starts at physical line 216.</summary>
    private const int FloorBandStart = 216;

    /// <summary>$88:E73D selects BG3 for the 32-line HUD, then $15/$13/$05 for the gameplay bands.</summary>
    internal static ScreenLayers[] BuildGameplayLayers()
    {
        var layers = new ScreenLayers[SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels];
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < SnesPpuLayout.ScreenHeightPixels; y++)
            layers[y - SnesPpuLayout.GameplayHudHeightPixels] = y < UpperBandEnd
                ? ScreenLayers.Bg1 | ScreenLayers.Bg3 | ScreenLayers.Sprites
                : y < FloorBandStart
                    ? ScreenLayers.Bg1 | ScreenLayers.Bg2 | ScreenLayers.Sprites
                    : ScreenLayers.Bg1 | ScreenLayers.Bg3;
        return layers;
    }
}
