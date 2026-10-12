using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Literal Mode-1 register inputs for the modeled HUD/gameplay scanline split.</summary>
/// <param name="Bg1Scroll">BG1 scroll-register values in pixels, before tilemap wrapping and without the renderer's first-visible-background-scanline adjustment.</param>
/// <param name="Bg2Scroll">Fallback BG2 scroll-register values when no per-line replacement is captured, without the first-visible-background-scanline adjustment.</param>
/// <param name="Bg2MapSize">BG2 tilemap size in 8-pixel cells: 32 or 64 on each axis.</param>
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
    Position16<RoomPixels> Bg1Scroll, Position16<Bg2Pixels> Bg2Scroll,
    Extent16<TilemapCells> Bg2MapSize, ushort Bg2TilemapWord,
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
    /// <summary>Stores main screen layers by line.</summary>
    private readonly ushort[] mainScreenLayersByLine;
    /// <summary>Captured register values for this composition operation; PPU memory and OAM belong to the enclosing render snapshot, not this value.</summary>
    public OrdinaryGameplayRegisters Registers { get; }
    /// <summary>Owned BG2 HDMA scroll replacements for physical lines 32..223; an inactive axis uses <see cref="OrdinaryGameplayRegisters.Bg2Scroll"/> on every line.</summary>
    public Bg2HdmaScrollTables Bg2LineScrolls { get; }
    /// <summary>Owned TM layer-enable words for physical lines 32..223; nonempty entries replace the fallback gameplay mask, not the separate TMW window mask or fixed HUD composition.</summary>
    public ReadOnlySpan<ushort> MainScreenLayersByLine => mainScreenLayersByLine;

    /// <summary>Captures immutable register inputs and copies optional scanline tables for the first backdrop/HUD/BG/OBJ operation; Mode-1 priority composition precedes later color-math and overlay operations.</summary>
    /// <param name="registers">Captured Mode-1 state with a 32- or 64-cell BG2 tilemap on each axis.</param>
    /// <param name="bg2LineScrolls">Optional BG2 HDMA scroll replacements for physical lines 32..223.</param>
    /// <param name="mainScreenLayersByLine">Empty or exactly 192 TM replacement masks in <see cref="SnesMainScreenLayers"/> encoding.</param>
    /// <exception cref="ArgumentException">BG2 dimensions are unsupported or a nonempty TM table does not contain exactly 192 values.</exception>
    public OrdinaryGameplayRenderLayer(OrdinaryGameplayRegisters registers,
        Bg2HdmaScrollTables bg2LineScrolls = default, ReadOnlySpan<ushort> mainScreenLayersByLine = default)
    {
        if (registers.Bg2MapSize.X is not (32 or 64) || registers.Bg2MapSize.Y is not (32 or 64)
            || registers.Bg2MapSize.X * registers.Bg2MapSize.Y is not (1024 or 2048 or 4096))
            throw new ArgumentException("Gameplay BG2 requires 32 or 64 tiles on each axis.", nameof(registers));
        if (!mainScreenLayersByLine.IsEmpty && mainScreenLayersByLine.Length != Bg2HdmaScrollTables.GameplayLines)
            throw new ArgumentException("Gameplay HDMA tables must contain exactly 192 register values.");
        Registers = registers;
        Bg2LineScrolls = bg2LineScrolls;
        this.mainScreenLayersByLine = mainScreenLayersByLine.ToArray();
    }

    /// <summary>The BG2 scroll registers on gameplay line <paramref name="line"/> (0..191).</summary>
    public Position16<Bg2Pixels> Bg2ScrollAt(int line) => Bg2LineScrolls.At(line, Registers.Bg2Scroll);
}
