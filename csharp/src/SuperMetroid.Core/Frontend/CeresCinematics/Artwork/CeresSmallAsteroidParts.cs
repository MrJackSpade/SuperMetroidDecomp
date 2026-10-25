using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Seven small asteroids assembled from three reusable overlapping shapes.
/// The original90FE anchors and copy orientations are authored scene placement.</summary>
/// <remarks>
/// The complete native composition scatters seven copies of three rock drawings.
/// BF76/BF89 initializes one shared origin and translates it horizontally; CC4F
/// repeatedly displays the same composition. No individual trajectory or rotation
/// determines the relative positions or chosen vertical reflections. Inspection of
/// the original assembled art confirms these choices vary a fixed scene drawing.
/// A fitted ordinal curve or numerical case list would recite that arrangement.
/// Retain only those anchors and copy orientations under the nonsense exception;
/// local shape geometry is calculated, and source pixels remain a separate review.
/// </remarks>
internal sealed class CeresSmallAsteroidParts(Offset16<SpritePixels>[] anchors) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Calculates repeated rock shapes only for the matching sixteen-part Ceres asteroid composition.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        if (shown != CeresFlightSpriteDefinitions.SmallAsteroids || supplied.PartCount != 16) return supplied;
        Offset16<SpritePixels> Anchor(int index)
        {
            var part = supplied.Part(index);
            return new Offset16<SpritePixels>(checked((short)part.Part.Offset.X), checked((short)part.Part.Offset.Y));
        }
        Offset16<SpritePixels>[] anchors = [Anchor(1), Anchor(3), Anchor(5), Anchor(6), Anchor(10), Anchor(12), Anchor(13)];
        // Horizontal and corner shapes extend eight pixels beyond their anchor.
        if (anchors[2].X > 247 || anchors[3].X > 247 || anchors[5].X > 247 || anchors[6].X > 247) return supplied;
        return supplied.CalculateIfMatching(new CeresSmallAsteroidParts(anchors));
    }
    /// <summary>Counts the parts in seven small asteroid copies.</summary>
    public int Count => 16;
    /// <summary>Calculates one asteroid part from its preserved anchor and one of three reusable shapes.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int group, piece;
            if (index < 4) { group = index / 2; piece = index % 2; }
            else if (index < 6) { group = 2; piece = index - 4; }
            else if (index < 9) { group = 3; piece = index - 6; }
            else if (index < 11) { group = 4; piece = index - 9; }
            else if (index < 13) { group = 5; piece = index - 11; }
            else { group = 6; piece = index - 13; }
            bool flip = group is 0 or 4 or 6;
            int x = 0, y, tile;
            bool large = true;
            if (group is 0 or 1 or 4)
            {
                y = 8 * (1 - piece) * (flip ? -1 : 1);
                tile = CeresSmallAsteroidCellDefinitions.Vertical + 16 * (1 - piece);
            }
            else if (group is 2 or 5)
            {
                x = 8 * (1 - piece); y = 0;
                tile = CeresSmallAsteroidCellDefinitions.Horizontal + 1 - piece;
            }
            else
            {
                large = piece != 0;
                x = piece == 1 ? 8 : 0;
                y = piece == 2 ? 8 : 0;
                tile = CeresSmallAsteroidCellDefinitions.Corner + x / 8 + 16 * (y / 8);
                // Reflection is relative to the small anchor cell, so large parts
                // need an additional eight-pixel displacement.
                if (flip) y = -y - (large ? 8 : 0);
            }
            return new(SpritePart.Create(anchors[group].X + x, anchors[group].Y + y, large,
                tile, 0, 0, flip ? TileFlip.Vertical : 0), true);
        }
    }
    /// <summary>Enumerates asteroid parts in their original OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same asteroid order through the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
