using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

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
internal sealed class CeresSmallAsteroidParts((int X, int Y)[] anchors) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Rebuilds the recognized Ceres small-asteroid composition from its authored anchors.</summary>
    /// <param name="pointer">Sprite-definition pointer identifying the composition to specialize.</param>
    /// <param name="supplied">Composition parsed from the source spritemap.</param>
    /// <returns>The compiled small-asteroid parts for the matching composition, or <paramref name="supplied"/> when it does not match.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer != CeresFlightSpriteDefinitions.SmallAsteroids || supplied.PartCount != 16) return supplied;
        (int X, int Y) Anchor(int index)
        {
            var part = supplied.Part(index);
            return (part.X.SignedOffset, unchecked((sbyte)part.Y));
        }
        (int X, int Y)[] anchors = [Anchor(1), Anchor(3), Anchor(5), Anchor(6), Anchor(10), Anchor(12), Anchor(13)];
        // Horizontal and corner shapes extend eight pixels beyond their anchor.
        if (anchors[2].X > 247 || anchors[3].X > 247 || anchors[5].X > 247 || anchors[6].X > 247) return supplied;
        return supplied.CalculateIfMatching(new CeresSmallAsteroidParts(anchors));
    }
    /// <summary>Gets the number of compiled sprite parts in the seven-rock composition.</summary>
    public int Count => 16;

    /// <summary>Gets a compiled sprite part by its position in the assembled composition.</summary>
    /// <param name="index">Zero-based part index, from zero through <see cref="Count"/> minus one.</param>
    /// <returns>The tile, placement, and reflection data for the requested part.</returns>
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
                tile = CeresSmallAsteroidAtlas.Vertical + 16 * (1 - piece);
            }
            else if (group is 2 or 5)
            {
                x = 8 * (1 - piece); y = 0;
                tile = CeresSmallAsteroidAtlas.Horizontal + 1 - piece;
            }
            else
            {
                large = piece != 0;
                x = piece == 1 ? 8 : 0;
                y = piece == 2 ? 8 : 0;
                tile = CeresSmallAsteroidAtlas.Corner + x / 8 + 16 * (y / 8);
                // Reflection is relative to the small anchor cell, so large parts
                // need an additional eight-pixel displacement.
                if (flip) y = -y - (large ? 8 : 0);
            }
            return new(SnesSpritemapXWord.Create(anchors[group].X + x, large), unchecked((byte)(anchors[group].Y + y)),
                SnesObjAttributeWord.Create(tile, 0, 0, flip ? SnesTileFlipFlags.Vertical : 0), true);
        }
    }
    /// <summary>Enumerates all compiled parts in spritemap order.</summary>
    /// <returns>An enumerator over the composition's sixteen sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
/// <summary>Names the Tile16 atlas cells reused to assemble the Ceres small-asteroid shapes.</summary>
internal static class CeresSmallAsteroidAtlas
{
    /// <summary>Tile16E, top of the overlapping vertical asteroid pair in8C:90FE.</summary>
    internal const int Vertical = 0x16e;
    /// <summary>Tile1A0, left of the overlapping horizontal asteroid pair.</summary>
    internal const int Horizontal = 0x1a0;
    /// <summary>Tile19D, small corner cell adjoining two large asteroid parts.</summary>
    internal const int Corner = 0x19d;
}
