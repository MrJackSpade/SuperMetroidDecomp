using System.Collections.Concurrent;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Converts authored sprite pieces into whole-frame parts, sized by their screen's sprite sizes. A part's character is
/// its sheet character minus the bound artwork's first character, so artwork holds only the characters its frames use.
/// </summary>
public static class SpritePartFrames
{
    /// <summary>The standard sprite sizes: the 8x8 and 16x16 sprites gameplay and the menus draw.</summary>
    public static ScreenObjectSizes StandardSizes { get; } = new(new(8, 8), new(16, 16));

    /// <summary>Single small-piece frames, shared because their content is only the piece and the artwork's first character.</summary>
    private static readonly ConcurrentDictionary<SmallObjectKey, SpriteFrame> SmallObjects = new();

    /// <summary>One authored piece, in its own palette, as a whole-frame part of a screen drawing the <see cref="StandardSizes"/>.</summary>
    /// <param name="part">The piece.</param>
    /// <param name="firstCharacter">Sheet character of the bound artwork's character 0.</param>
    public static SpriteFramePart Part(SpritePart part, int firstCharacter) =>
        part.ToFramePart(firstCharacter, StandardSizes);

    /// <summary>A frame of one piece drawn alone at the origin.</summary>
    /// <param name="part">The piece; its offset and size are ignored, and it draws small at the origin.</param>
    /// <param name="firstCharacter">Sheet character of the bound artwork's character 0.</param>
    public static SpriteFrame SmallObject(SpritePart part, int firstCharacter)
    {
        SpritePart alone = part with { Offset = Offset16<ScreenPixels>.Zero, Large = false };
        return SmallObjects.GetOrAdd(new SmallObjectKey(alone, firstCharacter), key => new SpriteFrame(
            $"Piece.{key.Part.Character:X3}.{key.Part.Palette}.{key.Part.Priority}.{(key.Part.FlipsHorizontally ? "h" : "")}{(key.Part.FlipsVertically ? "v" : "")}",
            [Part(key.Part, key.FirstCharacter)]));
    }
}
