using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Literal Mode-1 register inputs for the modeled HUD/gameplay scanline split.</summary>
/// <param name="Bg1X">BG1 horizontal scroll-register value in pixels, before tilemap wrapping.</param>
/// <param name="Bg1Y">BG1 vertical scroll-register value in pixels, without the renderer's first-visible-background-scanline adjustment.</param>
/// <param name="Bg2X">Fallback BG2 horizontal scroll-register value when no per-line replacement is captured.</param>
/// <param name="Bg2Y">Fallback BG2 vertical scroll-register value, without the first-visible-background-scanline adjustment.</param>
/// <param name="Bg2WidthTiles">BG2 tilemap width in 8-pixel cells: 32 or 64.</param>
/// <param name="Bg2HeightTiles">BG2 tilemap height in 8-pixel cells: 32 or 64.</param>
/// <param name="Bg2TilemapWord">BG2 tilemap base in VRAM words, not byte addresses.</param>
/// <param name="Bg1CharacterWord">BG1 four-bit planar character base in VRAM words.</param>
/// <param name="Bg2CharacterWord">BG2 four-bit planar character base in VRAM words.</param>
/// <param name="HudCharacterWord">BG3 two-bit planar HUD character base in VRAM words.</param>
/// <param name="MainScreenLayers">Fallback gameplay TM layer enables, replaced rather than combined with any captured per-line enables.</param>
/// <param name="Bg2FirstScanline">Inclusive physical output scanline at which BG2 drawing starts, intersected with the gameplay region.</param>
/// <param name="Bg2EndScanline">Exclusive physical output scanline at which BG2 drawing ends.</param>
/// <param name="Windows">Captured hardware window endpoint, enable, inversion, and logic registers used to mask individual layers.</param>
/// <param name="MainScreenWindowMask">TMW layer bits selecting which main-screen layers are subject to their configured windows.</param>
/// <param name="Bg2Mosaic">Captured BG2 mosaic sampling configuration, applied by rendering without advancing simulation state.</param>
public readonly record struct OrdinaryGameplayRegisters(
    ushort Bg1X, ushort Bg1Y, ushort Bg2X, ushort Bg2Y,
    int Bg2WidthTiles, int Bg2HeightTiles, ushort Bg2TilemapWord,
    ushort Bg1CharacterWord, ushort Bg2CharacterWord, ushort HudCharacterWord,
    SnesMainScreenLayers MainScreenLayers,
    int Bg2FirstScanline = 32, int Bg2EndScanline = 224,
    SnesWindowRegisters Windows = default,
    SnesMainScreenLayers MainScreenWindowMask = SnesMainScreenLayers.None,
    BackgroundMosaicSampling Bg2Mosaic = default);

/// <summary>
/// Fused backdrop/HUD/Mode-1/OBJ composition. This must be the first operation;
/// subsequent operations may apply color math and overlays. Scroll tables contain
/// literal register values for physical lines 32..223, not pre-offset source Y.
/// </summary>
public sealed record OrdinaryGameplayRenderLayer : RenderLayer
{
    /// <summary>Owned BG2 horizontal register table for the 192 visible gameplay lines.</summary>
    private readonly ushort[] horizontalScrolls;
    /// <summary>Owned BG2 vertical register table for the 192 visible gameplay lines.</summary>
    private readonly ushort[] verticalScrolls;
    /// <summary>Owned per-line main-screen layer replacement table.</summary>
    private readonly ushort[] mainScreenLayersByLine;
    /// <summary>Captured register values for this composition operation; PPU memory and OAM belong to the enclosing render snapshot, not this value.</summary>
    public OrdinaryGameplayRegisters Registers { get; }
    /// <summary>Owned BG2 horizontal register replacements for physical lines 32..223, indexed 0..191; empty means use <see cref="OrdinaryGameplayRegisters.Bg2X"/> on every line.</summary>
    public ReadOnlySpan<ushort> HorizontalScrolls => horizontalScrolls;
    /// <summary>Owned BG2 vertical register replacements for physical lines 32..223, before the renderer's visible-line adjustment; empty means use <see cref="OrdinaryGameplayRegisters.Bg2Y"/> on every line.</summary>
    public ReadOnlySpan<ushort> VerticalScrolls => verticalScrolls;
    /// <summary>Owned TM layer-enable words for physical lines 32..223; nonempty entries replace the fallback gameplay mask, not the separate TMW window mask or fixed HUD composition.</summary>
    public ReadOnlySpan<ushort> MainScreenLayersByLine => mainScreenLayersByLine;

    /// <summary>Captures immutable register inputs and copies optional scanline tables for the first backdrop/HUD/BG/OBJ operation; Mode-1 priority composition precedes later color-math and overlay operations.</summary>
    /// <param name="registers">Captured Mode-1 state with a 32- or 64-cell BG2 tilemap on each axis.</param>
    /// <param name="horizontalScrolls">Empty or exactly 192 BG2 horizontal pixel-register values; index zero corresponds to physical line 32.</param>
    /// <param name="verticalScrolls">Empty or exactly 192 BG2 vertical pixel-register values; these are literal offsets, not precomputed source-row coordinates.</param>
    /// <param name="mainScreenLayersByLine">Empty or exactly 192 TM replacement masks in <see cref="SnesMainScreenLayers"/> encoding.</param>
    /// <exception cref="ArgumentException">BG2 dimensions are unsupported or a nonempty scanline table does not contain exactly 192 values.</exception>
    public OrdinaryGameplayRenderLayer(OrdinaryGameplayRegisters registers,
        ReadOnlySpan<ushort> horizontalScrolls = default, ReadOnlySpan<ushort> verticalScrolls = default,
        ReadOnlySpan<ushort> mainScreenLayersByLine = default)
    {
        if (registers.Bg2WidthTiles is not (32 or 64) || registers.Bg2HeightTiles is not (32 or 64)
            || registers.Bg2WidthTiles * registers.Bg2HeightTiles is not (1024 or 2048 or 4096))
            throw new ArgumentException("Gameplay BG2 requires 32 or 64 tiles on each axis.", nameof(registers));
        int lines = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        if ((!horizontalScrolls.IsEmpty && horizontalScrolls.Length != lines)
            || (!verticalScrolls.IsEmpty && verticalScrolls.Length != lines)
            || (!mainScreenLayersByLine.IsEmpty && mainScreenLayersByLine.Length != lines))
            throw new ArgumentException("Gameplay HDMA tables must contain exactly 192 register values.");
        Registers = registers;
        this.horizontalScrolls = horizontalScrolls.ToArray();
        this.verticalScrolls = verticalScrolls.ToArray();
        this.mainScreenLayersByLine = mainScreenLayersByLine.ToArray();
    }
}
