using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Resolved Mode-7 viewport, opaque HUD, and optional bottom Mode-1 BG2/OBJ band.</summary>
/// <remarks>Scanline boundaries are physical screen coordinates; the transform never restarts at the band origin.</remarks>
public sealed record Mode7GameplayRenderLayer : RenderLayer
{
    public Mode7RenderRegisters Registers { get; }
    public ushort HudTilemapWord { get; }
    public ushort HudCharacterWord { get; }
    public int HudScanlines { get; }
    public Mode1FloorBand? Floor { get; }

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
public readonly record struct Mode1FloorBand(int FirstScanline, ushort TilemapWord, ushort CharacterWord,
    ushort HorizontalScroll, ushort VerticalScroll, int MapWidthTiles, int MapHeightTiles);
