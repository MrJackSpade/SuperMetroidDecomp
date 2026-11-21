using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>
/// File-copy arrow compositions $82:CDCD/CE15/CE5D/CECD, which the file-select sheet pictures: each upward arrow is its
/// downward arrow mirrored top to bottom.
/// </summary>
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

    /// <summary>The height of each of the arrows' small parts, which an upward arrow's parts are mirrored over.</summary>
    private const int PartHeight = 8;

    /// <summary>The short upward arrow: the short downward arrow's parts mirrored top to bottom.</summary>
    private static readonly CompiledSpritePart[] ShortUp = [.. ShortDown.Select(Upward)];

    /// <summary>The long upward arrow: the long downward arrow's parts mirrored top to bottom.</summary>
    private static readonly CompiledSpritePart[] LongUp = [.. LongDown.Select(Upward)];

    /// <summary>
    /// Each of the four file-copy arrow shapes' parts, in palette three: a downward arrow's in the cartridge's order, an
    /// upward arrow's in its downward arrow's order, which pictures the same arrow since no part covers another (#2596).
    /// </summary>
    internal static IReadOnlyDictionary<FileCopyArrowShape, CompiledSpritePart[]> Parts { get; } = new Dictionary<FileCopyArrowShape, CompiledSpritePart[]>
    {
        [FileCopyArrowShape.ShortDown] = ShortDown,
        [FileCopyArrowShape.ShortUp] = ShortUp,
        [FileCopyArrowShape.LongDown] = LongDown,
        [FileCopyArrowShape.LongUp] = LongUp,
    };

    /// <summary>
    /// A downward arrow's part as the upward arrow shows it (#2557): mirrored top to bottom about the arrow's centre. The
    /// arrows' parts never overlap, so the order the cartridge lists the upward arrow's parts in shows the same picture.
    /// </summary>
    private static CompiledSpritePart Upward(CompiledSpritePart part) => part with { Part = part.Part.MirroredVertically(PartHeight) };
}
