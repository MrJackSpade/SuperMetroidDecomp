using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD, which the file-select sheet pictures.</summary>
internal static class FileCopyArrowPartDefinitions
{
    /// <summary>The short downward arrow's native sprite order, interleaving shaft and arrowhead tiles.</summary>
    private static readonly CompiledSpritePart[] ShortDown =
        [
            new(SpritePart.Create(new(2, -16), false, 207, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -16), false, 206, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, -24), false, 191, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -24), false, 190, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 8), false, 185, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 16), false, 201, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, 16), false, 203, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, 8), false, 187, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, 8), false, 184, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, 0), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 0), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, -16), false, 205, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -8), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, -8), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
        ];

    /// <summary>The short upward arrow's native sprite order: the short downward arrow's tiles flipped vertically.</summary>
    private static readonly CompiledSpritePart[] ShortUp =
        [
            new(SpritePart.Create(new(-14, -16), false, 184, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, -8), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -8), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -24), false, 201, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -16), false, 185, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, 8), false, 207, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 8), false, 206, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, 16), false, 191, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 16), false, 190, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, 8), false, 205, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 0), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, 0), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, -24), false, 203, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, -16), false, 187, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
        ];

    /// <summary>The long downward arrow's native sprite order, interleaving shaft and arrowhead tiles.</summary>
    private static readonly CompiledSpritePart[] LongDown =
        [
            new(SpritePart.Create(new(-6, 8), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, 8), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 0), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, 0), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -8), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, -8), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -16), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, -16), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, 24), false, 184, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, 16), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 16), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 32), false, 201, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, 24), false, 185, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, -32), false, 207, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -32), false, 206, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, -40), false, 191, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -40), false, 190, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, -32), false, 205, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-6, -24), false, 189, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(-14, -24), false, 188, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, 32), false, 203, 1, ActorDepth.InFront, PictureFlip.None), true),
            new(SpritePart.Create(new(2, 24), false, 187, 1, ActorDepth.InFront, PictureFlip.None), true),
        ];

    /// <summary>The long upward arrow's native sprite order: the long downward arrow's tiles flipped vertically.</summary>
    private static readonly CompiledSpritePart[] LongUp =
        [
            new(SpritePart.Create(new(-6, -16), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, -16), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -8), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, -8), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 0), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, 0), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 8), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, 8), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, -32), false, 184, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, -24), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -24), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -40), false, 201, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, -32), false, 185, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, 24), false, 207, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 24), false, 206, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, 32), false, 191, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 32), false, 190, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, 24), false, 205, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-6, 16), false, 189, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(-14, 16), false, 188, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, -40), false, 203, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
            new(SpritePart.Create(new(2, -32), false, 187, 1, ActorDepth.InFront, PictureFlip.Vertical), true),
        ];

    /// <summary>Each of the four file-copy arrow shapes' native OAM parts, in order: 16-pixel OBJ parts in palette three.</summary>
    internal static IReadOnlyDictionary<FileCopyArrowShape, CompiledSpritePart[]> Parts { get; } = new Dictionary<FileCopyArrowShape, CompiledSpritePart[]>
    {
        [FileCopyArrowShape.ShortDown] = ShortDown,
        [FileCopyArrowShape.ShortUp] = ShortUp,
        [FileCopyArrowShape.LongDown] = LongDown,
        [FileCopyArrowShape.LongUp] = LongUp,
    };
}
