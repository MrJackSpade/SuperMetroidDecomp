using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three atlas-aligned asteroid shapes with independently supplied scene anchors.</summary>
/// <param name="left">Scene anchor for the six parts composing the left asteroid.</param>
/// <param name="right">Scene anchor for the seven parts composing the right asteroid.</param>
/// <param name="lower">Scene anchor for the six parts composing the lower asteroid.</param>
/// <param name="priority">OBJ priority applied to every generated part in this composition.</param>
/// <remarks>
/// The complete original compositions at 8C:909D and 8C:94F7 place three distinct
/// drawings at (-113,-65), (88,-36), and (0,8). Their local geometry follows atlas
/// offsets; their relative scene placement is authored composition. Native BF22
/// initializes one shared origin and BF35 translates the entire group horizontally,
/// with no per-rock trajectory or placement calculation. A numeric case or fitted
/// curve for these six coordinates would only re-encode that drawing arrangement.
/// Retain those independent anchors, while calculating all nineteen local parts.
/// This disposition does not extend to source pixels or other scene placements.
/// </remarks>
internal sealed class CeresLargeAsteroidParts(
    (int X, int Y) left, (int X, int Y) right, (int X, int Y) lower, int priority)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Rebuilds a recognized three-asteroid spritemap from its independently supplied scene anchors.</summary>
    /// <param name="pointer">Native spritemap pointer identifying the under-attack or approach composition.</param>
    /// <param name="supplied">Composition inspected for anchors and returned unchanged if it cannot be represented safely.</param>
    /// <returns>The calculated nineteen-part composition when the pointer and anchors are supported; otherwise <paramref name="supplied"/>.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (pointer is not (CeresLargeAsteroidAtlas.UnderAttack or CeresLargeAsteroidAtlas.Approach)
            || supplied.PartCount != 19) return supplied;
        static (int X, int Y) Anchor(CompiledSpritePart part) => (part.X.SignedOffset, unchecked((sbyte)part.Y));
        var left = Anchor(supplied.Part(5));
        var right = Anchor(supplied.Part(12));
        var lower = Anchor(supplied.Part(18));
        // A valid custom anchor can place the calculated remainder outside signed OBJ X.
        // Keep that independent supplied composition instead of throwing during matching.
        if (left.X > 231 || right.X > 231 || lower.X > 239) return supplied;
        return supplied.CalculateIfMatching(new CeresLargeAsteroidParts(left, right, lower,
            pointer == CeresLargeAsteroidAtlas.UnderAttack ? 3 : 0));
    }
    /// <summary>Gets the number of generated sprite parts across the three asteroid shapes.</summary>
    public int Count => 19;

    /// <summary>Gets one generated sprite part in the original left, right, then lower asteroid ordering.</summary>
    /// <param name="index">Zero-based index from zero through eighteen.</param>
    /// <returns>The atlas tile and positioned OBJ attributes for that part.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the nineteen-part composition.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int column, row, tile;
            bool large = index != 6;
            (int X, int Y) anchor;
            if (index < 6)
            {
                anchor = left;
                column = index < 2 ? 3 : 2 - 2 * ((index - 2) % 2);
                row = index < 2 ? 2 - 2 * index : 2 - 2 * ((index - 2) / 2);
                tile = CeresLargeAsteroidAtlas.Left;
            }
            else if (index < 13)
            {
                anchor = right;
                int piece = index - 6;
                if (piece == 0) { column = 0; row = 4; }
                else if (piece == 1) { column = 1; row = 3; }
                else if (piece == 2) { column = 3; row = 0; }
                else { column = 2 - 2 * ((piece - 3) % 2); row = 2 - 2 * ((piece - 3) / 2); }
                tile = CeresLargeAsteroidAtlas.Right;
            }
            else
            {
                anchor = lower;
                int piece = index - 13;
                column = 2 - 2 * (piece % 2);
                row = 4 - 2 * (piece / 2);
                tile = CeresLargeAsteroidAtlas.Lower;
            }
            return new(SnesSpritemapXWord.Create(anchor.X + 8 * column, large),
                unchecked((byte)(anchor.Y + 8 * row)),
                SnesObjAttributeWord.Create(tile + column + 16 * row, 0, priority, 0), true);
        }
    }
    /// <summary>Enumerates all nineteen generated parts in spritemap order.</summary>
    /// <returns>An enumerator over the left, right, and lower asteroid parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Native spritemap identities and atlas origins used to assemble Ceres's three large asteroids.</summary>
internal static class CeresLargeAsteroidAtlas
{
    /// <summary>8C:909D,station-under-attack large asteroids,also exposed by the legacy discovery catalog.</summary>
    internal const ushort UnderAttack = 0x909d;
    /// <summary>8C:94F7,same three large asteroids at OBJ priority0.</summary>
    internal const ushort Approach = 0x94f7;
    /// <summary>Tile$160,upper-left of the left asteroid atlas patch.</summary>
    internal const int Left = 0x160;
    /// <summary>Tile$169,upper-left of the right asteroid atlas patch.</summary>
    internal const int Right = 0x169;
    /// <summary>Tile$165,upper-left of the lower asteroid atlas patch.</summary>
    internal const int Lower = 0x165;
}
