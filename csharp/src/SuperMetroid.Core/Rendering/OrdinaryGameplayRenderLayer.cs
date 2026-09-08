using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Literal Mode-1 register inputs for the modeled HUD/gameplay scanline split.</summary>
public readonly record struct OrdinaryGameplayRegisters(
    ushort Bg1X, ushort Bg1Y, ushort Bg2X, ushort Bg2Y,
    int Bg2WidthTiles, int Bg2HeightTiles, ushort Bg2TilemapWord,
    ushort Bg1CharacterWord, ushort Bg2CharacterWord, ushort HudCharacterWord,
    SnesMainScreenLayers MainScreenLayers,
    int Bg2FirstScanline = 32, int Bg2EndScanline = 224);

/// <summary>
/// Fused backdrop/HUD/Mode-1/OBJ composition. This must be the first operation;
/// subsequent operations may apply color math and overlays. Scroll tables contain
/// literal register values for physical lines 32..223, not pre-offset source Y.
/// </summary>
public sealed record OrdinaryGameplayRenderLayer : RenderLayer
{
    private readonly ushort[] horizontalScrolls;
    private readonly ushort[] verticalScrolls;
    public OrdinaryGameplayRegisters Registers { get; }
    public ReadOnlySpan<ushort> HorizontalScrolls => horizontalScrolls;
    public ReadOnlySpan<ushort> VerticalScrolls => verticalScrolls;

    public OrdinaryGameplayRenderLayer(OrdinaryGameplayRegisters registers,
        ReadOnlySpan<ushort> horizontalScrolls = default, ReadOnlySpan<ushort> verticalScrolls = default)
    {
        if (registers.Bg2WidthTiles is not (32 or 64) || registers.Bg2HeightTiles is not (32 or 64)
            || registers.Bg2WidthTiles * registers.Bg2HeightTiles is not (2048 or 4096))
            throw new ArgumentException("Gameplay BG2 requires 64x32, 32x64 or 64x64 tiles.", nameof(registers));
        int lines = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        if ((!horizontalScrolls.IsEmpty && horizontalScrolls.Length != lines)
            || (!verticalScrolls.IsEmpty && verticalScrolls.Length != lines))
            throw new ArgumentException("Gameplay HDMA tables must contain exactly 192 register values.");
        Registers = registers;
        this.horizontalScrolls = horizontalScrolls.ToArray();
        this.verticalScrolls = verticalScrolls.ToArray();
    }
}
