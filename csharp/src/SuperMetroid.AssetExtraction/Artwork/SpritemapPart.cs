using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One five-byte spritemap record as the cartridge stores it: an X word, a Y byte and an attribute word. The import
/// pipeline decodes it into the game's <see cref="SpritePart"/> once; nothing past extraction reads the record.
/// </summary>
/// <param name="X">The encoded X offset and size selector.</param>
/// <param name="Y">The encoded signed Y offset.</param>
/// <param name="Attributes">The tile number and sprite attributes.</param>
public readonly record struct SpritemapPart(SnesSpritemapXWord X, SnesSpritemapYByte Y, SnesObjAttributeWord Attributes)
{
    /// <summary>The piece the record describes: its signed offsets, size, character, palette row, priority and mirroring.</summary>
    public SpritePart ToPart() =>
        new(Offset16<ScreenPixels>.Of(X.SignedOffset, Y.SignedOffset), X.IsLarge, Attributes.TileNumber, Attributes.FlipFlags,
            Attributes.PaletteIndex, Attributes.Priority);

    /// <summary>The record that stores <paramref name="part"/>.</summary>
    public static SpritemapPart From(SpritePart part) =>
        new(SnesSpritemapXWord.Create(part.Offset.X, part.Large), SnesSpritemapYByte.Create(part.Offset.Y), SnesObjAttributeWord.From(part));
}
