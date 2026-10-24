using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Typed view of the pause screen's composed memory image (#630, #1358).</summary>
[PpuLayoutBoundary("rendering view: pause screen VRAM")]
[NumberedIdentities("BG1, BG2 and BG3 are the PPU's distinct background layers, each with its own pause-screen role and size: the 64x32 map or equipment page, the 32x32 frame and the 32x64 HUD plane. The number names the hardware layer, not a position.")]
internal readonly struct PauseVram(SnesVram vram)
{
    /// <summary>The shared map characters.</summary>
    public VramRegion MapCharacters => new(vram, PauseVramLayout.MapCharacters);

    /// <summary>The pause interface characters.</summary>
    public VramRegion InterfaceCharacters => new(vram, PauseVramLayout.InterfaceCharacters);

    /// <summary>The map-object OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, PauseVramLayout.ObjectCharacters);

    /// <summary>The map/equipment BG1 tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, PauseVramLayout.Bg1Tilemap);

    /// <summary>The pause frame's BG2 tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, PauseVramLayout.Bg2Tilemap);

    /// <summary>The BG3 tilemap: the retained gameplay HUD rows over the cleared FX rows.</summary>
    public VramRegion Bg3Tilemap => new(vram, PauseVramLayout.Bg3Tilemap);
}
