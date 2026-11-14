using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD, which the file-select sheet pictures.</summary>
internal static class FileCopyArrowPartDefinitions
{
    /// <summary>Exposes the selected native OAM part order for one of the four file-copy arrow shapes.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is not an arrow shape.</exception>
    internal static ReadOnlySpan<CompiledSpritePart> Parts(FileCopyArrowShape shape) => shape switch
    {
        FileCopyArrowShape.ShortDown => ShortDown,
        FileCopyArrowShape.ShortUp => ShortUp,
        FileCopyArrowShape.LongDown => LongDown,
        FileCopyArrowShape.LongUp => LongUp,
        _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Undefined file-copy arrow shape."),
    };
    /// <summary>Builds one 16-pixel OBJ part in palette three, optionally flipping it vertically for an upward arrow.</summary>
    private static CompiledSpritePart Part(Offset16<SpritePixels> offset, int tile, PictureFlip flip) => new(
        SpritePart.Create(Offset16<ScreenPixels>.Of(offset.X, offset.Y), false,
        tile, 1, 3, flip), true);

    /// <summary>The short downward arrow's native sprite order, interleaving shaft and arrowhead tiles.</summary>
    private static readonly CompiledSpritePart[] ShortDown =
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
        ];

    /// <summary>The short upward arrow's native sprite order: the short downward arrow's tiles flipped vertically.</summary>
    private static readonly CompiledSpritePart[] ShortUp =
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
        ];

    /// <summary>The long downward arrow's native sprite order, interleaving shaft and arrowhead tiles.</summary>
    private static readonly CompiledSpritePart[] LongDown =
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
        ];

    /// <summary>The long upward arrow's native sprite order: the long downward arrow's tiles flipped vertically.</summary>
    private static readonly CompiledSpritePart[] LongUp =
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
        ];
}
