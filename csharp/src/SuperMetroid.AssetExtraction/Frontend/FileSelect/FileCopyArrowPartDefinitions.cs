using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD, which the file-select sheet pictures.</summary>
internal static class FileCopyArrowPartDefinitions
{
    /// <summary>Exposes the selected native OAM part order for the four file-copy arrow shapes.</summary>
    internal static ReadOnlySpan<CompiledSpritePart> Parts(int shape) => Shapes[shape];
    /// <summary>Builds one 16-pixel OBJ part in palette three, optionally flipping it vertically for an upward arrow.</summary>
    private static CompiledSpritePart Part(Offset16<SpritePixels> offset, int tile, TileFlip flip) => new(
        SpritePart.Create(Offset16<ScreenPixels>.Of(offset.X, offset.Y), false,
        tile, 1, 3, flip), true);

    /// <summary>Four native sprite orders, including interleaved shaft and arrowhead tiles for short/long and upward/downward arrows.</summary>
    private static readonly CompiledSpritePart[][] Shapes =
    [
        [
            Part(new(2, -16), 0xcf, TileFlip.None),
            Part(new(-6, -16), 0xce, TileFlip.None),
            Part(new(2, -24), 0xbf, TileFlip.None),
            Part(new(-6, -24), 0xbe, TileFlip.None),
            Part(new(-6, 8), 0xb9, TileFlip.None),
            Part(new(-6, 16), 0xc9, TileFlip.None),
            Part(new(2, 16), 0xcb, TileFlip.None),
            Part(new(2, 8), 0xbb, TileFlip.None),
            Part(new(-14, 8), 0xb8, TileFlip.None),
            Part(new(-14, 0), 0xbc, TileFlip.None),
            Part(new(-6, 0), 0xbd, TileFlip.None),
            Part(new(-14, -16), 0xcd, TileFlip.None),
            Part(new(-6, -8), 0xbd, TileFlip.None),
            Part(new(-14, -8), 0xbc, TileFlip.None),
        ],
        [
            Part(new(-14, -16), 0xb8, TileFlip.Vertical),
            Part(new(-14, -8), 0xbc, TileFlip.Vertical),
            Part(new(-6, -8), 0xbd, TileFlip.Vertical),
            Part(new(-6, -24), 0xc9, TileFlip.Vertical),
            Part(new(-6, -16), 0xb9, TileFlip.Vertical),
            Part(new(2, 8), 0xcf, TileFlip.Vertical),
            Part(new(-6, 8), 0xce, TileFlip.Vertical),
            Part(new(2, 16), 0xbf, TileFlip.Vertical),
            Part(new(-6, 16), 0xbe, TileFlip.Vertical),
            Part(new(-14, 8), 0xcd, TileFlip.Vertical),
            Part(new(-6, 0), 0xbd, TileFlip.Vertical),
            Part(new(-14, 0), 0xbc, TileFlip.Vertical),
            Part(new(2, -24), 0xcb, TileFlip.Vertical),
            Part(new(2, -16), 0xbb, TileFlip.Vertical),
        ],
        [
            Part(new(-6, 8), 0xbd, TileFlip.None),
            Part(new(-14, 8), 0xbc, TileFlip.None),
            Part(new(-6, 0), 0xbd, TileFlip.None),
            Part(new(-14, 0), 0xbc, TileFlip.None),
            Part(new(-6, -8), 0xbd, TileFlip.None),
            Part(new(-14, -8), 0xbc, TileFlip.None),
            Part(new(-6, -16), 0xbd, TileFlip.None),
            Part(new(-14, -16), 0xbc, TileFlip.None),
            Part(new(-14, 24), 0xb8, TileFlip.None),
            Part(new(-14, 16), 0xbc, TileFlip.None),
            Part(new(-6, 16), 0xbd, TileFlip.None),
            Part(new(-6, 32), 0xc9, TileFlip.None),
            Part(new(-6, 24), 0xb9, TileFlip.None),
            Part(new(2, -32), 0xcf, TileFlip.None),
            Part(new(-6, -32), 0xce, TileFlip.None),
            Part(new(2, -40), 0xbf, TileFlip.None),
            Part(new(-6, -40), 0xbe, TileFlip.None),
            Part(new(-14, -32), 0xcd, TileFlip.None),
            Part(new(-6, -24), 0xbd, TileFlip.None),
            Part(new(-14, -24), 0xbc, TileFlip.None),
            Part(new(2, 32), 0xcb, TileFlip.None),
            Part(new(2, 24), 0xbb, TileFlip.None),
        ],
        [
            Part(new(-6, -16), 0xbd, TileFlip.Vertical),
            Part(new(-14, -16), 0xbc, TileFlip.Vertical),
            Part(new(-6, -8), 0xbd, TileFlip.Vertical),
            Part(new(-14, -8), 0xbc, TileFlip.Vertical),
            Part(new(-6, 0), 0xbd, TileFlip.Vertical),
            Part(new(-14, 0), 0xbc, TileFlip.Vertical),
            Part(new(-6, 8), 0xbd, TileFlip.Vertical),
            Part(new(-14, 8), 0xbc, TileFlip.Vertical),
            Part(new(-14, -32), 0xb8, TileFlip.Vertical),
            Part(new(-14, -24), 0xbc, TileFlip.Vertical),
            Part(new(-6, -24), 0xbd, TileFlip.Vertical),
            Part(new(-6, -40), 0xc9, TileFlip.Vertical),
            Part(new(-6, -32), 0xb9, TileFlip.Vertical),
            Part(new(2, 24), 0xcf, TileFlip.Vertical),
            Part(new(-6, 24), 0xce, TileFlip.Vertical),
            Part(new(2, 32), 0xbf, TileFlip.Vertical),
            Part(new(-6, 32), 0xbe, TileFlip.Vertical),
            Part(new(-14, 24), 0xcd, TileFlip.Vertical),
            Part(new(-6, 16), 0xbd, TileFlip.Vertical),
            Part(new(-14, 16), 0xbc, TileFlip.Vertical),
            Part(new(2, -40), 0xcb, TileFlip.Vertical),
            Part(new(2, -32), 0xbb, TileFlip.Vertical),
        ],
    ];
}
