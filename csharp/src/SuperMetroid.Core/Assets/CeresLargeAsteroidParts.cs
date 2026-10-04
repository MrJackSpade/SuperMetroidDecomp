using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three atlas-aligned asteroid shapes with independently supplied scene anchors.</summary>
internal sealed class CeresLargeAsteroidParts(
    (int X, int Y) left, (int X, int Y) right, (int X, int Y) lower, int priority)
    : IReadOnlyList<CompiledSpritePart>
{
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
    public int Count => 19;
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
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

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
