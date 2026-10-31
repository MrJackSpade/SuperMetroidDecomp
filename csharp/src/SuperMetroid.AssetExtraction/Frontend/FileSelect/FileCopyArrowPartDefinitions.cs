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
            Part(new(2, -16), 207, TileFlip.None),
            Part(new(-6, -16), 206, TileFlip.None),
            Part(new(2, -24), 191, TileFlip.None),
            Part(new(-6, -24), 190, TileFlip.None),
            Part(new(-6, 8), 185, TileFlip.None),
            Part(new(-6, 16), 201, TileFlip.None),
            Part(new(2, 16), 203, TileFlip.None),
            Part(new(2, 8), 187, TileFlip.None),
            Part(new(-14, 8), 184, TileFlip.None),
            Part(new(-14, 0), 188, TileFlip.None),
            Part(new(-6, 0), 189, TileFlip.None),
            Part(new(-14, -16), 205, TileFlip.None),
            Part(new(-6, -8), 189, TileFlip.None),
            Part(new(-14, -8), 188, TileFlip.None),
        ],
        [
            Part(new(-14, -16), 184, TileFlip.Vertical),
            Part(new(-14, -8), 188, TileFlip.Vertical),
            Part(new(-6, -8), 189, TileFlip.Vertical),
            Part(new(-6, -24), 201, TileFlip.Vertical),
            Part(new(-6, -16), 185, TileFlip.Vertical),
            Part(new(2, 8), 207, TileFlip.Vertical),
            Part(new(-6, 8), 206, TileFlip.Vertical),
            Part(new(2, 16), 191, TileFlip.Vertical),
            Part(new(-6, 16), 190, TileFlip.Vertical),
            Part(new(-14, 8), 205, TileFlip.Vertical),
            Part(new(-6, 0), 189, TileFlip.Vertical),
            Part(new(-14, 0), 188, TileFlip.Vertical),
            Part(new(2, -24), 203, TileFlip.Vertical),
            Part(new(2, -16), 187, TileFlip.Vertical),
        ],
        [
            Part(new(-6, 8), 189, TileFlip.None),
            Part(new(-14, 8), 188, TileFlip.None),
            Part(new(-6, 0), 189, TileFlip.None),
            Part(new(-14, 0), 188, TileFlip.None),
            Part(new(-6, -8), 189, TileFlip.None),
            Part(new(-14, -8), 188, TileFlip.None),
            Part(new(-6, -16), 189, TileFlip.None),
            Part(new(-14, -16), 188, TileFlip.None),
            Part(new(-14, 24), 184, TileFlip.None),
            Part(new(-14, 16), 188, TileFlip.None),
            Part(new(-6, 16), 189, TileFlip.None),
            Part(new(-6, 32), 201, TileFlip.None),
            Part(new(-6, 24), 185, TileFlip.None),
            Part(new(2, -32), 207, TileFlip.None),
            Part(new(-6, -32), 206, TileFlip.None),
            Part(new(2, -40), 191, TileFlip.None),
            Part(new(-6, -40), 190, TileFlip.None),
            Part(new(-14, -32), 205, TileFlip.None),
            Part(new(-6, -24), 189, TileFlip.None),
            Part(new(-14, -24), 188, TileFlip.None),
            Part(new(2, 32), 203, TileFlip.None),
            Part(new(2, 24), 187, TileFlip.None),
        ],
        [
            Part(new(-6, -16), 189, TileFlip.Vertical),
            Part(new(-14, -16), 188, TileFlip.Vertical),
            Part(new(-6, -8), 189, TileFlip.Vertical),
            Part(new(-14, -8), 188, TileFlip.Vertical),
            Part(new(-6, 0), 189, TileFlip.Vertical),
            Part(new(-14, 0), 188, TileFlip.Vertical),
            Part(new(-6, 8), 189, TileFlip.Vertical),
            Part(new(-14, 8), 188, TileFlip.Vertical),
            Part(new(-14, -32), 184, TileFlip.Vertical),
            Part(new(-14, -24), 188, TileFlip.Vertical),
            Part(new(-6, -24), 189, TileFlip.Vertical),
            Part(new(-6, -40), 201, TileFlip.Vertical),
            Part(new(-6, -32), 185, TileFlip.Vertical),
            Part(new(2, 24), 207, TileFlip.Vertical),
            Part(new(-6, 24), 206, TileFlip.Vertical),
            Part(new(2, 32), 191, TileFlip.Vertical),
            Part(new(-6, 32), 190, TileFlip.Vertical),
            Part(new(-14, 24), 205, TileFlip.Vertical),
            Part(new(-6, 16), 189, TileFlip.Vertical),
            Part(new(-14, 16), 188, TileFlip.Vertical),
            Part(new(2, -40), 203, TileFlip.Vertical),
            Part(new(2, -32), 187, TileFlip.Vertical),
        ],
    ];
}
