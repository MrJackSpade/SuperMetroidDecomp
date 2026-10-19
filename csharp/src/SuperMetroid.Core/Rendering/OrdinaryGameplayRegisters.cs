namespace SuperMetroid.Core.Rendering;

/// <summary>Literal Mode-1 register inputs for the modeled HUD/gameplay scanline split.</summary>
/// <param name="Bg1Scroll">BG1 scroll-register values in pixels, before tilemap wrapping and without the renderer's first-visible-background-scanline adjustment.</param>
/// <param name="Bg2Scroll">Fallback BG2 scroll-register values when no per-line replacement is captured, without the first-visible-background-scanline adjustment.</param>
/// <param name="Bg2MapSize">BG2 tilemap size in 8-pixel cells: 32 or 64 on each axis.</param>
/// <param name="Bg2TilemapWord">BG2 tilemap base VRAM word address.</param>
/// <param name="Bg1CharacterWord">BG1 four-bit planar character base VRAM word address.</param>
/// <param name="Bg2CharacterWord">BG2 four-bit planar character base VRAM word address.</param>
/// <param name="HudCharacterWord">BG3 two-bit planar HUD character base VRAM word address.</param>
/// <param name="MainScreenLayers">Fallback gameplay TM layer enables, replaced rather than combined with any captured per-line enables.</param>
/// <param name="Bg2FirstScanline">Inclusive physical output scanline at which BG2 drawing starts, intersected with the gameplay region.</param>
/// <param name="Bg2EndScanline">Exclusive physical output scanline at which BG2 drawing ends.</param>
/// <param name="Windows">Captured hardware window endpoint, enable, inversion, and logic registers used to mask individual layers.</param>
/// <param name="MainScreenWindowMask">TMW layer bits selecting which main-screen layers are subject to their configured windows.</param>
/// <param name="Bg2Mosaic">Captured BG2 mosaic sampling configuration, applied by rendering without advancing simulation state.</param>
public readonly record struct OrdinaryGameplayRegisters(
    Position16<RoomPixels> Bg1Scroll, Position16<Bg2Pixels> Bg2Scroll,
    Extent16<TilemapCells> Bg2MapSize, int Bg2TilemapWord,
    int Bg1CharacterWord, int Bg2CharacterWord, int HudCharacterWord,
    SnesMainScreenLayers MainScreenLayers,
    int Bg2FirstScanline = 32, int Bg2EndScanline = 224,
    SnesWindowRegisters Windows = default,
    SnesMainScreenLayers MainScreenWindowMask = SnesMainScreenLayers.None,
    BackgroundMosaicSampling Bg2Mosaic = default)
{
    /// <summary>BG2 tilemap base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int Bg2TilemapWord
    {
        get => bg2TilemapWord;
        init => bg2TilemapWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="Bg2TilemapWord"/>.</summary>
    private readonly int bg2TilemapWord = VramWordRange.Require(Bg2TilemapWord);

    /// <summary>BG1 four-bit planar character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int Bg1CharacterWord
    {
        get => bg1CharacterWord;
        init => bg1CharacterWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="Bg1CharacterWord"/>.</summary>
    private readonly int bg1CharacterWord = VramWordRange.Require(Bg1CharacterWord);

    /// <summary>BG2 four-bit planar character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int Bg2CharacterWord
    {
        get => bg2CharacterWord;
        init => bg2CharacterWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="Bg2CharacterWord"/>.</summary>
    private readonly int bg2CharacterWord = VramWordRange.Require(Bg2CharacterWord);

    /// <summary>BG3 two-bit planar HUD character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int HudCharacterWord
    {
        get => hudCharacterWord;
        init => hudCharacterWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="HudCharacterWord"/>.</summary>
    private readonly int hudCharacterWord = VramWordRange.Require(HudCharacterWord);
}
