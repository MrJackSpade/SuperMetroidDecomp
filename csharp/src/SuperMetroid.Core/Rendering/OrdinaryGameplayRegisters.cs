namespace SuperMetroid.Core.Rendering;

/// <summary>Literal Mode-1 register inputs for the modeled HUD/gameplay scanline split.</summary>
/// <param name="Bg1Scroll">BG1 scroll-register values in pixels, before tilemap wrapping and without the renderer's first-visible-background-scanline adjustment.</param>
/// <param name="Bg2Scroll">Fallback BG2 scroll-register values when no per-line replacement is captured, without the first-visible-background-scanline adjustment.</param>
/// <param name="Bg2MapSize">BG2 tilemap size in 8-pixel cells: 32 or 64 on each axis.</param>
/// <param name="MainScreenLayers">Fallback gameplay TM layer enables, replaced rather than combined with any captured per-line enables.</param>
/// <param name="Bg2FirstScanline">Inclusive physical output scanline at which BG2 drawing starts, intersected with the gameplay region.</param>
/// <param name="Bg2EndScanline">Exclusive physical output scanline at which BG2 drawing ends.</param>
/// <param name="Windows">Captured hardware window endpoint, enable, inversion, and logic registers used to mask individual layers.</param>
/// <param name="MainScreenWindowMask">TMW layer bits selecting which main-screen layers are subject to their configured windows.</param>
/// <param name="Bg2Mosaic">Captured BG2 mosaic sampling configuration, applied by rendering without advancing simulation state.</param>
public readonly record struct OrdinaryGameplayRegisters(
    Position16<RoomPixels> Bg1Scroll, Position16<Bg2Pixels> Bg2Scroll,
    Extent16<TilemapCells> Bg2MapSize,
    SnesMainScreenLayers MainScreenLayers,
    int Bg2FirstScanline = 32, int Bg2EndScanline = 224,
    SnesWindowRegisters Windows = default,
    SnesMainScreenLayers MainScreenWindowMask = SnesMainScreenLayers.None,
    BackgroundMosaicSampling Bg2Mosaic = default);
