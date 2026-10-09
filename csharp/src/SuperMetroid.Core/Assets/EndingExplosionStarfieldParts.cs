using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A28B starfield: one large glint tile and five small star tiles.
/// Size follows the selected tile family; all stars are unflipped, priority zero,
/// with actor-inherited palette. Supplied positions and tile choices remain inputs.</summary>
/// <remarks>The original indexed artwork is a particular scattered star drawing.
/// Native consumers translate the whole composition; no per-star depth, orbit or
/// random-state input determines its positions or selected glints. Retain those
/// choices under #1165's nonsense exception: an ordinal fit would recite the drawing.
/// This does not exempt the calculated attributes, tile pixels or actor motion.</remarks>
internal sealed class EndingExplosionStarfieldParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly Star[] stars;
    private readonly record struct Star(int X, byte Y, int Tile);
    private EndingExplosionStarfieldParts(Star[] stars) => this.stars = stars;

    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Stars)) return supplied;
        var stars = new Star[supplied.PartCount];
        for (int index = 0; index < stars.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int tile = part.Attributes.TileNumber;
            if (tile is not 0xed and not (>= 0xf8 and <= 0xfc)) return supplied;
            stars[index] = new(part.X.SignedOffset, part.Y, tile);
        }
        return supplied.CalculateIfMatching(new EndingExplosionStarfieldParts(stars));
    }

    public int Count => stars.Length;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            Star star = stars[index];
            return new(SnesSpritemapXWord.Create(star.X, star.Tile == 0xed), star.Y,
                SnesObjAttributeWord.Create(star.Tile, 0, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
