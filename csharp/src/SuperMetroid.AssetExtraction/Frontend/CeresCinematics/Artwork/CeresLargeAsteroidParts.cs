using SuperMetroid.Core.Frontend.Intro;
using System.Collections;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

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
    Offset16<SpritePixels> left, Offset16<SpritePixels> right, Offset16<SpritePixels> lower, int priority)
    : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Reconstructs native asteroid atlas parts only when a supplied composition matches the nineteen-part layout.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        // Other poses belong to other owners.
        CeresLargeAsteroidComposition composition;
        if (shown == CeresDestructionSpritePoses.BackdropPose(CeresDestructionBackdrop.LargeAsteroids))
            composition = CeresLargeAsteroidComposition.UnderAttack;
        else if (shown == CeresFlightSpriteDefinitions.LargeAsteroids)
            composition = CeresLargeAsteroidComposition.Approach;
        else
            return supplied;
        if (supplied.PartCount != 19) return supplied;
        static Offset16<SpritePixels> Anchor(CompiledSpritePart part) => new Offset16<SpritePixels>(part.Part.Offset.X, part.Part.Offset.Y);
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
                _ => throw new InvalidOperationException($"Undefined {nameof(CeresLargeAsteroidComposition)} {composition}."),
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
            Offset16<SpritePixels> anchor;
            if (index < 6)
            {
                anchor = left;
                column = index < 2 ? 3 : 2 - 2 * ((index - 2) % 2);
                row = index < 2 ? 2 - 2 * index : 2 - 2 * ((index - 2) / 2);
                tile = CeresLargeAsteroidCellDefinitions.Left;
            }
            else if (index < 13)
            {
                anchor = right;
                int piece = index - 6;
                if (piece == 0) { column = 0; row = 4; }
                else if (piece == 1) { column = 1; row = 3; }
                else if (piece == 2) { column = 3; row = 0; }
                else { column = 2 - 2 * ((piece - 3) % 2); row = 2 - 2 * ((piece - 3) / 2); }
                tile = CeresLargeAsteroidCellDefinitions.Right;
            }
            else
            {
                anchor = lower;
                int piece = index - 13;
                column = 2 - 2 * (piece % 2);
                row = 4 - 2 * (piece / 2);
                tile = CeresLargeAsteroidCellDefinitions.Lower;
            }
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(anchor.X + 8 * column, anchor.Y + 8 * row), large,
                tile + column + 16 * row, 0, priority, 0), true);
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
