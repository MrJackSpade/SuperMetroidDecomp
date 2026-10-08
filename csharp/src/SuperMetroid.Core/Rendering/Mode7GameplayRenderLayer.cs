using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Resolved Mode-7 viewport, opaque HUD, and optional bottom Mode-1 BG2/OBJ band.</summary>
/// <remarks>Scanline boundaries are physical screen coordinates; the transform never restarts at the band origin.</remarks>
public sealed record Mode7GameplayRenderLayer : RenderLayer
{
    /// <summary>Gets the native integer Mode 7 transform and overflow policy; sampling uses physical screen coordinates even below the HUD band.</summary>
    public Mode7RenderRegisters Registers { get; }
    /// <summary>Gets the VRAM word base of the unscrolled 32-column BG3 HUD tilemap, whose color-zero pixels remain opaque.</summary>
    public ushort HudTilemapWord { get; }
    /// <summary>Gets the VRAM word base of the HUD's 2-bpp eight-pixel characters, not a physical byte offset.</summary>
    public ushort HudCharacterWord { get; }
    /// <summary>Gets the number of top-of-screen scanlines occupied by the opaque HUD, 0..224 in multiples of eight; zero disables the HUD band.</summary>
    public int HudScanlines { get; }
    /// <summary>Gets the optional bottom Mode-1 BG2/OBJ band; null lets Mode 7 and OBJ continue to the bottom of the viewport.</summary>
    public Mode1FloorBand? Floor { get; }

    /// <summary>Creates an immutable full-screen composition description with explicit HUD, Mode 7, and optional Mode-1 floor boundaries.</summary>
    /// <param name="registers">Value-copied native Mode 7 transform and overflow controls.</param>
    /// <param name="hudTilemapWord">VRAM word address of the HUD BG3 tilemap.</param>
    /// <param name="hudCharacterWord">VRAM word address of the HUD's 2-bpp character data.</param>
    /// <param name="hudScanlines">Top HUD height, from zero through 224 physical scanlines and divisible by eight.</param>
    /// <param name="floor">Optional value-copied lower band; its first scanline must follow the HUD and be below 224, with each map dimension either 32 or 64 tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">The HUD height or supplied floor boundary/map dimensions are invalid.</exception>
    public Mode7GameplayRenderLayer(Mode7RenderRegisters registers, ushort hudTilemapWord,
        ushort hudCharacterWord, int hudScanlines, Mode1FloorBand? floor = null)
    {
        if (hudScanlines < 0 || hudScanlines > SnesPpuLayout.ScreenHeightPixels ||
            hudScanlines % SnesPpuLayout.BackgroundTileSizePixels != 0)
            throw new ArgumentOutOfRangeException(nameof(hudScanlines));
        if (floor is { } f && (f.FirstScanline < hudScanlines || f.FirstScanline >= SnesPpuLayout.ScreenHeightPixels ||
            f.MapWidthTiles is not (32 or 64) || f.MapHeightTiles is not (32 or 64)))
            throw new ArgumentOutOfRangeException(nameof(floor));
        Registers = registers;
        HudTilemapWord = hudTilemapWord;
        HudCharacterWord = hudCharacterWord;
        HudScanlines = hudScanlines;
        Floor = floor;
    }
}

/// <summary>Mode-1 BG2 and OBJ register values after a bottom-of-screen video-mode switch.</summary>
/// <param name="FirstScanline">Inclusive physical screen Y where the lower band begins; sampling does not restart its scroll origin here.</param>
/// <param name="TilemapWord">BG2 tilemap base in VRAM words.</param>
/// <param name="CharacterWord">BG2 4-bpp character base in VRAM words.</param>
/// <param name="HorizontalScroll">Native horizontal BG2 scroll in pixels, retained as its unsigned register representation.</param>
/// <param name="VerticalScroll">Native vertical BG2 scroll in pixels, retained as its unsigned register representation.</param>
/// <param name="MapWidthTiles">BG2 width in eight-pixel cells, 32 or 64 when used by a gameplay layer.</param>
/// <param name="MapHeightTiles">BG2 height in eight-pixel cells, 32 or 64 when used by a gameplay layer.</param>
public readonly record struct Mode1FloorBand(int FirstScanline, ushort TilemapWord, ushort CharacterWord,
    ushort HorizontalScroll, ushort VerticalScroll, int MapWidthTiles, int MapHeightTiles);
