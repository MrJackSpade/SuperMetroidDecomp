using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive byte-domain equations used by the current room FX reference.</summary>
public enum ExpandedColorMathOperation : byte
{
    /// <summary>Adds the expanded background RGB channels to the existing output channels, clamping each sum to 255 and preserving output alpha.</summary>
    Add,
    /// <summary>Subtracts the expanded background RGB channels from the existing output channels, clamping each difference to zero and preserving output alpha.</summary>
    Subtract
}

/// <summary>Literal BG scroll registers for one physical output scanline.</summary>
/// <param name="X">Horizontal scroll register in pixels, added to the output X coordinate before tilemap wrapping.</param>
/// <param name="Y">Vertical scroll register in pixels, added to the physical output Y coordinate before tilemap wrapping.</param>
public readonly record struct BackgroundLineScroll(ushort X, ushort Y);

/// <summary>Owned 2-bpp BG color-math plane with a physical-line scroll table.</summary>
public sealed record Bg2BppColorMathRenderLayer : RenderLayer
{
    private readonly BackgroundLineScroll[] scrolls;
    /// <summary>The owned 224 register pairs indexed by physical output scanline, including HUD lines rather than starting at the gameplay viewport.</summary>
    public ReadOnlySpan<BackgroundLineScroll> Scrolls => scrolls;
    /// <summary>VRAM word address of the 32-column background tilemap, not a byte address.</summary>
    public ushort TilemapWord { get; }
    /// <summary>VRAM word address of the 2-bpp character data; each 8-by-8 character occupies eight words.</summary>
    public ushort CharacterWord { get; }
    /// <summary>Tilemap height of 32 or 64 eight-pixel rows, determining vertical wrapping; the plane has a fixed width of 32 tiles.</summary>
    public int MapHeightTiles { get; }
    /// <summary>First participating physical output scanline, inclusive; lines before it are untouched, and 224 disables the plane for the entire frame.</summary>
    public int FirstScanline { get; }
    /// <summary>Expanded-byte RGB equation used when this plane is composited as a standalone layer; source-aware gameplay composition instead follows its enclosing color-math controls.</summary>
    public ExpandedColorMathOperation Operation { get; }

    /// <summary>Captures a 2-bpp background color-math plane and copies its physical-line scroll table, validating the map height, first scanline, and operation without retaining caller-owned mutable storage.</summary>
    /// <param name="tilemapWord">Tilemap base address in VRAM words.</param>
    /// <param name="characterWord">2-bpp character base address in VRAM words.</param>
    /// <param name="mapHeightTiles">Tilemap height, either 32 or 64 eight-pixel rows.</param>
    /// <param name="firstScanline">Inclusive first physical scanline from 0 through 224; 224 means no visible lines participate.</param>
    /// <param name="operation">Addition or subtraction for the standalone expanded-byte compositor.</param>
    /// <param name="scrolls">Exactly 224 register pairs, indexed by physical output scanline; the constructor takes an owned copy.</param>
    public Bg2BppColorMathRenderLayer(ushort tilemapWord, ushort characterWord, int mapHeightTiles,
        int firstScanline, ExpandedColorMathOperation operation, ReadOnlySpan<BackgroundLineScroll> scrolls)
    {
        if (mapHeightTiles is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(mapHeightTiles));
        if (firstScanline is < 0 or > SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentOutOfRangeException(nameof(firstScanline));
        if (operation is not (ExpandedColorMathOperation.Add or ExpandedColorMathOperation.Subtract))
            throw new ArgumentOutOfRangeException(nameof(operation));
        if (scrolls.Length != SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("BG color math needs 224 physical-line register pairs.", nameof(scrolls));
        TilemapWord = tilemapWord;
        CharacterWord = characterWord;
        MapHeightTiles = mapHeightTiles;
        FirstScanline = firstScanline;
        Operation = operation;
        this.scrolls = scrolls.ToArray();
    }
}
