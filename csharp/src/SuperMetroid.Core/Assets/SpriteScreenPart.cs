namespace SuperMetroid.Core.Assets;

/// <summary>One sprite-draw part placed on the screen, as every backend draws it.</summary>
/// <param name="Position">Top-left corner relative to the screen's top-left; may lie off screen.</param>
/// <param name="Size">Width and height in pixels.</param>
/// <param name="Character">First character in the draw's artwork.</param>
/// <param name="FlipX">Whether the pixels are mirrored horizontally.</param>
/// <param name="FlipY">Whether the pixels are mirrored vertically.</param>
/// <param name="Palette">OBJ palette row 0..7, taken from a validated <see cref="SpriteFramePart"/> or <see cref="SpriteDraw"/>.</param>
/// <param name="Priority">Priority against the backgrounds 0..3, taken from a validated part or draw.</param>
public readonly record struct SpriteScreenPart(Offset16<ScreenPixels> Position, Extent16<ScreenPixels> Size, int Character,
    bool FlipX, bool FlipY, int Palette, int Priority)
{
    /// <summary>The artwork character holding a pixel of this part, from its position inside the part's rectangle.</summary>
    /// <param name="cell">Pixel inside the part, before flipping.</param>
    /// <param name="pixel">The pixel's position inside the returned character.</param>
    public int CharacterAt(Position16<ScreenPixels> cell, out Position16<CharacterPixels> pixel)
    {
        int sourceX = FlipX ? Size.X - 1 - cell.X : cell.X;
        int sourceY = FlipY ? Size.Y - 1 - cell.Y : cell.Y;
        pixel = new((ushort)(sourceX % SpriteArtwork.CharacterPixels), (ushort)(sourceY % SpriteArtwork.CharacterPixels));
        return Character + sourceY / SpriteArtwork.CharacterPixels * SpriteArtwork.CharactersPerRow
            + sourceX / SpriteArtwork.CharacterPixels;
    }
}
