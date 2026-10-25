using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

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
    /// <summary>Caller-selected star positions and tiles retained when calculated attributes exactly match.</summary>
    private readonly StarfieldPoint[] stars;
    /// <summary>One retained star's drawn offset and chosen glint tile, independent of calculated OAM flags.</summary>
    /// <summary>Owns the selected star drawing after its calculated size and attributes have been validated.</summary>
    private EndingExplosionStarfieldParts(StarfieldPoint[] stars) => this.stars = stars;

    /// <summary>Recognizes the star pose and tile family, then substitutes calculated OAM only when every supplied part matches.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        if (shown != EndingExplosionSpriteDefinitions.PoseOf(EndingExplosionSpriteDefinitions.Pose.Stars)) return supplied;
        var stars = new StarfieldPoint[supplied.PartCount];
        for (int index = 0; index < stars.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int tile = part.Attributes.TileNumber;
            if (tile != SnesCharacterGrid.Cell(14, 13) && (tile < SnesCharacterGrid.Cell(15, 8) || tile > SnesCharacterGrid.Cell(15, 12))) return supplied;
            stars[index] = new(new(checked((short)part.X.SignedOffset), (short)part.Y.SignedOffset), tile);
        }
        return supplied.CalculateIfMatching(new EndingExplosionStarfieldParts(stars));
    }

    /// <summary>Number of selected glints retained from the supplied scattered star composition.</summary>
    public int Count => stars.Length;
    /// <summary>Calculates size and attributes from a retained large-glint or small-star tile while preserving its position.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            StarfieldPoint star = stars[index];
            return new(SnesSpritemapXWord.Create(star.Offset.X, star.Tile == SnesCharacterGrid.Cell(14, 13)), SnesSpritemapYByte.Create(star.Offset.Y),
                SnesObjAttributeWord.Create(star.Tile, 0, 0), true);
        }
    }
    /// <summary>Yields selected stars in the supplied OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the supplied star order for non-generic composition consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
