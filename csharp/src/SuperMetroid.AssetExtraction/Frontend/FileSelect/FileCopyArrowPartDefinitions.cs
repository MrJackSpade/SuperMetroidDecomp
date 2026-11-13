using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD, which the file-select sheet pictures.</summary>
internal static class FileCopyArrowPartDefinitions
{
    /// <summary>The file-copy arrow shapes: short and long, downward and upward.</summary>
    internal const int ShapeCount = 4;

    /// <summary>Exposes the selected native OAM part order for the four file-copy arrow shapes.</summary>
    internal static ReadOnlySpan<CompiledSpritePart> Parts(int shape) => Shapes[shape];
    /// <summary>Builds one 16-pixel OBJ part in palette three, optionally flipping it vertically for an upward arrow.</summary>
    private static CompiledSpritePart Part(Offset16<SpritePixels> offset, int tile, PictureFlip flip) => new(
        SpritePart.Create(Offset16<ScreenPixels>.Of(offset.X, offset.Y), false,
        tile, 1, 3, flip), true);

    /// <summary>Four native sprite orders, including interleaved shaft and arrowhead tiles for short/long and upward/downward arrows.</summary>
    private static readonly CompiledSpritePart[][] Shapes =
    [
        [
            Part(new(2, -16), 207, PictureFlip.None),
            Part(new(-6, -16), 206, PictureFlip.None),
            Part(new(2, -24), 191, PictureFlip.None),
            Part(new(-6, -24), 190, PictureFlip.None),
            Part(new(-6, 8), 185, PictureFlip.None),
            Part(new(-6, 16), 201, PictureFlip.None),
            Part(new(2, 16), 203, PictureFlip.None),
            Part(new(2, 8), 187, PictureFlip.None),
            Part(new(-14, 8), 184, PictureFlip.None),
            Part(new(-14, 0), 188, PictureFlip.None),
            Part(new(-6, 0), 189, PictureFlip.None),
            Part(new(-14, -16), 205, PictureFlip.None),
            Part(new(-6, -8), 189, PictureFlip.None),
            Part(new(-14, -8), 188, PictureFlip.None),
        ],
        [
            Part(new(-14, -16), 184, PictureFlip.Vertical),
            Part(new(-14, -8), 188, PictureFlip.Vertical),
            Part(new(-6, -8), 189, PictureFlip.Vertical),
            Part(new(-6, -24), 201, PictureFlip.Vertical),
            Part(new(-6, -16), 185, PictureFlip.Vertical),
            Part(new(2, 8), 207, PictureFlip.Vertical),
            Part(new(-6, 8), 206, PictureFlip.Vertical),
            Part(new(2, 16), 191, PictureFlip.Vertical),
            Part(new(-6, 16), 190, PictureFlip.Vertical),
            Part(new(-14, 8), 205, PictureFlip.Vertical),
            Part(new(-6, 0), 189, PictureFlip.Vertical),
            Part(new(-14, 0), 188, PictureFlip.Vertical),
            Part(new(2, -24), 203, PictureFlip.Vertical),
            Part(new(2, -16), 187, PictureFlip.Vertical),
        ],
        [
            Part(new(-6, 8), 189, PictureFlip.None),
            Part(new(-14, 8), 188, PictureFlip.None),
            Part(new(-6, 0), 189, PictureFlip.None),
            Part(new(-14, 0), 188, PictureFlip.None),
            Part(new(-6, -8), 189, PictureFlip.None),
            Part(new(-14, -8), 188, PictureFlip.None),
            Part(new(-6, -16), 189, PictureFlip.None),
            Part(new(-14, -16), 188, PictureFlip.None),
            Part(new(-14, 24), 184, PictureFlip.None),
            Part(new(-14, 16), 188, PictureFlip.None),
            Part(new(-6, 16), 189, PictureFlip.None),
            Part(new(-6, 32), 201, PictureFlip.None),
            Part(new(-6, 24), 185, PictureFlip.None),
            Part(new(2, -32), 207, PictureFlip.None),
            Part(new(-6, -32), 206, PictureFlip.None),
            Part(new(2, -40), 191, PictureFlip.None),
            Part(new(-6, -40), 190, PictureFlip.None),
            Part(new(-14, -32), 205, PictureFlip.None),
            Part(new(-6, -24), 189, PictureFlip.None),
            Part(new(-14, -24), 188, PictureFlip.None),
            Part(new(2, 32), 203, PictureFlip.None),
            Part(new(2, 24), 187, PictureFlip.None),
        ],
        [
            Part(new(-6, -16), 189, PictureFlip.Vertical),
            Part(new(-14, -16), 188, PictureFlip.Vertical),
            Part(new(-6, -8), 189, PictureFlip.Vertical),
            Part(new(-14, -8), 188, PictureFlip.Vertical),
            Part(new(-6, 0), 189, PictureFlip.Vertical),
            Part(new(-14, 0), 188, PictureFlip.Vertical),
            Part(new(-6, 8), 189, PictureFlip.Vertical),
            Part(new(-14, 8), 188, PictureFlip.Vertical),
            Part(new(-14, -32), 184, PictureFlip.Vertical),
            Part(new(-14, -24), 188, PictureFlip.Vertical),
            Part(new(-6, -24), 189, PictureFlip.Vertical),
            Part(new(-6, -40), 201, PictureFlip.Vertical),
            Part(new(-6, -32), 185, PictureFlip.Vertical),
            Part(new(2, 24), 207, PictureFlip.Vertical),
            Part(new(-6, 24), 206, PictureFlip.Vertical),
            Part(new(2, 32), 191, PictureFlip.Vertical),
            Part(new(-6, 32), 190, PictureFlip.Vertical),
            Part(new(-14, 24), 205, PictureFlip.Vertical),
            Part(new(-6, 16), 189, PictureFlip.Vertical),
            Part(new(-14, 16), 188, PictureFlip.Vertical),
            Part(new(2, -40), 203, PictureFlip.Vertical),
            Part(new(2, -32), 187, PictureFlip.Vertical),
        ],
    ];
}
