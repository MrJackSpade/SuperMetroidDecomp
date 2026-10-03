using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Native main-screen layer selection while Mother Brain rises through the floor.</summary>
internal static class MotherBrainAscentDisplayDefinitions
{
    /// <summary>$88:E73D, HDMATable_MotherBrainRising_MainScreenLayers: first gameplay band ends after physical line 55.</summary>
    private const int UpperBandEnd = 56;
    /// <summary>$88:E745, the final eight-line floor band starts at physical line 216.</summary>
    private const int FloorBandStart = 216;

    /// <summary>$88:E73D selects BG3 for the 32-line HUD, then $15/$13/$05 for the gameplay bands.</summary>
    internal static ushort[] BuildGameplayLayers()
    {
        var layers = new ushort[SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels];
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < SnesPpuLayout.ScreenHeightPixels; y++)
            layers[y - SnesPpuLayout.GameplayHudHeightPixels] = (ushort)(y < UpperBandEnd
                ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg3 | SnesMainScreenLayers.Obj
                : y < FloorBandStart
                    ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj
                    : SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg3);
        return layers;
    }
}
