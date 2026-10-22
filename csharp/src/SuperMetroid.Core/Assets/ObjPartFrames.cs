using System.Collections.Concurrent;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Converts authored OBJ parts (a spritemap X/size word, a signed Y byte and an attribute word) into whole-frame
/// parts, sized by their screen's OBJ size selection. A part's character is its OBJ tile number minus the bound
/// artwork's first character, so artwork holds only the characters its frames use.
/// </summary>
public static class ObjPartFrames
{
    /// <summary>OBSEL size mode 0: the 8x8 and 16x16 objects gameplay and the menus draw.</summary>
    public static ScreenObjectSizes StandardSizes { get; } = new(new(8, 8), new(16, 16));

    /// <summary>Single small-OBJ frames, shared because their content is only the attribute word and base.</summary>
    private static readonly ConcurrentDictionary<SmallObjectKey, SpriteFrame> SmallObjects = new();

    /// <summary>One authored part, in its own palette, as a whole-frame part of a screen drawing the <see cref="StandardSizes"/>.</summary>
    /// <param name="x">Spritemap X word: signed nine-bit offset and the large-size bit.</param>
    /// <param name="y">Signed Y offset byte.</param>
    /// <param name="attributes">Tile, palette, priority and flips.</param>
    /// <param name="firstCharacter">OBJ tile number of the bound artwork's character 0.</param>
    public static SpriteFramePart Part(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, int firstCharacter) =>
        Part(x, y, attributes, firstCharacter, StandardSizes);

    /// <summary>One authored part as a whole-frame part of a screen whose objects draw at <paramref name="sizes"/>.</summary>
    /// <param name="x">Spritemap X word: signed nine-bit offset and the large-size bit.</param>
    /// <param name="y">Signed Y offset byte.</param>
    /// <param name="attributes">Tile, palette, priority and flips.</param>
    /// <param name="firstCharacter">OBJ tile number of the bound artwork's character 0.</param>
    /// <param name="sizes">The screen's small and large object sizes.</param>
    /// <param name="inheritPalette">Whether the part takes the draw's palette instead of its own.</param>
    public static SpriteFramePart Part(SnesSpritemapXWord x, SnesSpritemapYByte y, SnesObjAttributeWord attributes, int firstCharacter,
        ScreenObjectSizes sizes, bool inheritPalette = false) =>
        new(new Offset16<ScreenPixels>(checked((short)x.SignedOffset), checked((short)y.SignedOffset)),
            x.IsLarge ? sizes.Large : sizes.Small, attributes.TileNumber - firstCharacter, attributes.FlipFlags,
            inheritPalette ? null : attributes.PaletteIndex, attributes.Priority);

    /// <summary>A frame of one small OBJ at the origin, as the cartridge's raw small-sprite writes place it.</summary>
    /// <param name="attributes">Tile, palette, priority and flips.</param>
    /// <param name="firstCharacter">OBJ tile number of the bound artwork's character 0.</param>
    public static SpriteFrame SmallObject(SnesObjAttributeWord attributes, int firstCharacter) =>
        SmallObjects.GetOrAdd(new SmallObjectKey(attributes.Raw, firstCharacter), key => new SpriteFrame($"Obj.{key.Attributes:X4}",
            [Part(new SnesSpritemapXWord(0), new SnesSpritemapYByte(0), new SnesObjAttributeWord(checked((ushort)key.Attributes)), key.FirstCharacter)]));
}
