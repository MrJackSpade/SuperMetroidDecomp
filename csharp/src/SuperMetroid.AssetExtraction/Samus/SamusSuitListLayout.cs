using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// How the original lays out its per-suit colour lists (#1914): each per-suit cycle, charge and death list holds
/// Power's entries, then Varia's, then Gravity's. The import reads the lists by this layout once; the game names a suit's
/// colours by the suit.
/// </summary>
public static class SamusSuitListLayout
{
    /// <summary>The suits in the order their colour lists follow one another.</summary>
    public static ImmutableArray<SamusSuit> Order { get; } = [SamusSuit.Power, SamusSuit.Varia, SamusSuit.Gravity];

    /// <summary>The suit whose entries lie at <paramref name="place"/> among the per-suit lists.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> is past the last suit's entries.</exception>
    public static SamusSuit AtPlace(int place) =>
        (uint)place < (uint)Order.Length
            ? Order[place]
            : throw new ArgumentOutOfRangeException(nameof(place), place, "No suit's entries lie there.");
}
