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
    /// <summary>Input position and tile selections retained for the native starfield's calculated OAM attributes.</summary>
    private readonly Star[] stars;

    /// <summary>One star's authored signed X offset, screen Y byte, and selected source tile number.</summary>
    /// <param name="X">Signed horizontal offset retained from the supplied sprite part.</param>
    /// <param name="Y">Vertical screen coordinate retained from the supplied sprite part.</param>
    /// <param name="Tile">Source tile number selecting the large glint or one of the small star tiles.</param>
    private readonly record struct Star(int X, byte Y, int Tile);

    /// <summary>Stores the positions and tile identities used to rebuild the starfield's hardware attributes on demand.</summary>
    /// <param name="stars">Validated star inputs in their supplied composition order.</param>
    private EndingExplosionStarfieldParts(Star[] stars) => this.stars = stars;

    /// <summary>Uses calculated OAM parts only for the star pose when every supplied tile belongs to its native star family.</summary>
    /// <param name="pointer">Spritemap pointer identifying the composition's pose.</param>
    /// <param name="supplied">Existing composition whose positions and source tiles are retained if it matches.</param>
    /// <returns>The supplied composition for other poses or tile families; otherwise, a composition backed by calculated star parts.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Stars)) return supplied;
        var stars = new Star[supplied.PartCount];
        for (int index = 0; index < stars.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int tile = part.Attributes.TileNumber;
            if (tile != 0xed && tile is not (>= 0xf8 and <= 0xfc)) return supplied;
            stars[index] = new(part.X.SignedOffset, part.Y, tile);
        }
        return supplied.CalculateIfMatching(new EndingExplosionStarfieldParts(stars));
    }

    /// <summary>Number of retained star inputs, matching the source composition's part count.</summary>
    public int Count => stars.Length;

    /// <summary>Rebuilds the star's tile-size, flip, palette, and priority attributes from its retained source tile.</summary>
    /// <param name="index">Zero-based star position in the supplied composition.</param>
    /// <returns>A compiled sprite part with the retained position and calculated native attributes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index does not identify a retained star.</exception>
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
    /// <summary>Enumerates calculated parts in the same order as the supplied star composition.</summary>
    /// <returns>An enumerator over each retained star converted to its native OAM part.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
