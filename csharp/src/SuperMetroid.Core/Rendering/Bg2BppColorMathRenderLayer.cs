using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive byte-domain equations used by the current room FX reference.</summary>
public enum ExpandedColorMathOperation : byte { Add, Subtract }

/// <summary>Literal BG scroll registers for one physical output scanline.</summary>
public readonly record struct BackgroundLineScroll(ushort X, ushort Y);

/// <summary>Owned 2-bpp BG color-math plane with a physical-line scroll table.</summary>
public sealed record Bg2BppColorMathRenderLayer : RenderLayer
{
    private readonly BackgroundLineScroll[] scrolls;
    public ReadOnlySpan<BackgroundLineScroll> Scrolls => scrolls;
    public ushort TilemapWord { get; }
    public ushort CharacterWord { get; }
    public int MapHeightTiles { get; }
    public int FirstScanline { get; }
    public ExpandedColorMathOperation Operation { get; }

    public Bg2BppColorMathRenderLayer(ushort tilemapWord, ushort characterWord, int mapHeightTiles,
        int firstScanline, ExpandedColorMathOperation operation, ReadOnlySpan<BackgroundLineScroll> scrolls)
    {
        if (mapHeightTiles is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(mapHeightTiles));
        if (firstScanline < 0 || firstScanline > SnesPpuLayout.ScreenHeightPixels)
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
