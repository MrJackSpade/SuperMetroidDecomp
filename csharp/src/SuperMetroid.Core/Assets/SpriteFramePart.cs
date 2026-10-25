namespace SuperMetroid.Core.Assets;

/// <summary>One rectangular piece of a whole sprite frame, placed relative to the frame's origin.</summary>
/// <param name="Offset">Top-left corner relative to the frame origin, before any whole-frame flip.</param>
/// <param name="Size">Width and height in pixels, each 8, 16, 32 or 64.</param>
/// <param name="Character">First character in the bound artwork; larger parts read the sixteen-wide grid from it.</param>
/// <param name="Flip">The part's own mirroring, combined with the draw's whole-frame flip.</param>
/// <param name="Palette">The part's OBJ palette row 0..7, or null to use the draw's palette; <see cref="SpriteFrame"/>
/// rejects a row outside that range.</param>
/// <param name="Priority">The part's priority against the backgrounds 0..3, or null to use the draw's priority;
/// <see cref="SpriteFrame"/> rejects a priority outside that range.</param>
public readonly record struct SpriteFramePart(
    Offset16<ScreenPixels> Offset,
    Extent16<ScreenPixels> Size,
    int Character,
    SnesTileFlipFlags Flip,
    int? Palette,
    int? Priority)
{
    /// <summary>The highest character index this part reads in its artwork.</summary>
    public int LastCharacter =>
        Character + (Size.Y / SpriteArtwork.CharacterPixels - 1) * SpriteArtwork.CharactersPerRow + Size.X / SpriteArtwork.CharacterPixels - 1;

    /// <summary>The artwork character holding a pixel of this part, from its position inside the part's rectangle.</summary>
    /// <param name="cell">Pixel inside the part, before the part's own mirroring.</param>
    /// <param name="pixel">The pixel's position inside the returned character.</param>
    public int CharacterAt(Position16<ScreenPixels> cell, out Position16<CharacterPixels> pixel)
    {
        int sourceX = (Flip & SnesTileFlipFlags.Horizontal) != SnesTileFlipFlags.None ? Size.X - 1 - cell.X : cell.X;
        int sourceY = (Flip & SnesTileFlipFlags.Vertical) != SnesTileFlipFlags.None ? Size.Y - 1 - cell.Y : cell.Y;
        pixel = new((ushort)(sourceX % SpriteArtwork.CharacterPixels), (ushort)(sourceY % SpriteArtwork.CharacterPixels));
        return Character + sourceY / SpriteArtwork.CharacterPixels * SpriteArtwork.CharactersPerRow
            + sourceX / SpriteArtwork.CharacterPixels;
    }
}
