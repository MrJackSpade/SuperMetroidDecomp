using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Converts authored OBJ parts (a spritemap X/size word, a signed Y byte and an attribute word) into whole-frame
/// parts. Small parts are 8x8 and large ones 16x16, the gameplay OBSEL sizes. A part's character is its OBJ tile
/// number minus the bound artwork's first character, so artwork holds only the characters its frames use.
/// </summary>
public static class ObjPartFrames
{
    /// <summary>Edge of a small gameplay OBJ (OBSEL size mode 0).</summary>
    private const byte SmallObjectPixels = 8;
    /// <summary>Edge of a large gameplay OBJ (OBSEL size mode 0).</summary>
    private const byte LargeObjectPixels = 16;

    /// <summary>Single small-OBJ frames, shared because their content is only the attribute word and base.</summary>
    private static readonly ConcurrentDictionary<(ushort Attributes, int FirstCharacter), SpriteFrame> SmallObjects = new();

    /// <summary>One authored part as a whole-frame part.</summary>
    /// <param name="x">Spritemap X word: signed nine-bit offset and the large-size bit.</param>
    /// <param name="y">Signed Y offset byte.</param>
    /// <param name="attributes">Tile, palette, priority and flips.</param>
    /// <param name="firstCharacter">OBJ tile number of the bound artwork's character 0.</param>
    /// <param name="inheritPalette">Whether the part takes the draw's palette instead of its own.</param>
    public static SpriteFramePart Part(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, int firstCharacter,
        bool inheritPalette = false)
    {
        byte size = x.IsLarge ? LargeObjectPixels : SmallObjectPixels;
        return new(new Offset16<ScreenPixels>(checked((short)x.SignedOffset), checked((short)y.SignedOffset)),
            new Extent16<ScreenPixels>(size, size), attributes.TileNumber - firstCharacter, attributes.FlipFlags,
            inheritPalette ? null : attributes.PaletteIndex, attributes.Priority);
    }

    /// <summary>A frame of one small OBJ at the origin, as the cartridge's raw small-sprite writes place it.</summary>
    /// <param name="attributes">Tile, palette, priority and flips.</param>
    /// <param name="firstCharacter">OBJ tile number of the bound artwork's character 0.</param>
    public static SpriteFrame SmallObject(SnesObjAttributeWord attributes, int firstCharacter) =>
        SmallObjects.GetOrAdd((attributes.Raw, firstCharacter), key => new SpriteFrame($"Obj.{key.Attributes:X4}",
            [Part(new SnesSpritemapXWord(0), new SnesSpritemapYByte(0), new SnesObjAttributeWord(key.Attributes), key.FirstCharacter)]));
}
