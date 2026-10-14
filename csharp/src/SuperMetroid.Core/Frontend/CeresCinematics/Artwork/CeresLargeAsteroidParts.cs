using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Three atlas-aligned asteroid shapes with independently supplied scene anchors.</summary>
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
    /// <summary>Reconstructs native asteroid atlas parts only when a supplied composition matches the nineteen-part layout.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        // Shared OAM-record pointer space: other compositions belong to other owners.
        if (!Enum.IsDefined((CeresLargeAsteroidComposition)pointer) || supplied.PartCount != 19) return supplied;
        var composition = (CeresLargeAsteroidComposition)pointer;
        static (int X, int Y) Anchor(CompiledSpritePart part) => (part.X.SignedOffset, unchecked((sbyte)part.Y));
        var left = Anchor(supplied.Part(5));
        var right = Anchor(supplied.Part(12));
        var lower = Anchor(supplied.Part(18));
        // A valid custom anchor can place the calculated remainder outside signed OBJ X.
        // Keep that independent supplied composition instead of throwing during matching.
        if (left.X > 231 || right.X > 231 || lower.X > 239) return supplied;
        return supplied.CalculateIfMatching(new CeresLargeAsteroidParts(left, right, lower,
            composition switch
            {
                CeresLargeAsteroidComposition.UnderAttack => 3,
                CeresLargeAsteroidComposition.Approach => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(CeresLargeAsteroidComposition)} {(int)composition}."),
            }));
    }
    /// <summary>Six left, seven right, and six lower asteroid OAM parts.</summary>
    public int Count => 19;
    /// <summary>Places an indexed part relative to its independently supplied rock anchor.</summary>
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
    /// <summary>Emits the three rocks' nineteen parts in native OAM record order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Uses the same native part order for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
