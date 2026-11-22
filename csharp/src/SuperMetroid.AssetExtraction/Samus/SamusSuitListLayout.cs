using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// How the original lays out its per-suit colour lists (#1914): each per-suit cycle, charge and death list holds
/// Power's entries, then Varia's, then Gravity's. The import reads the lists by this layout once; the game names a suit's
/// colours by the suit.
/// </summary>
public static partial class SamusSuitListLayout
{
    /// <summary>The suits in the order their colour lists follow one another.</summary>
    public static ImmutableArray<SamusSuit> Order { get; } = [SamusSuit.Power, SamusSuit.Varia, SamusSuit.Gravity];

    /// <summary>
    /// Each suit's entries of a per-suit list, keyed by the suit's name for the installed document (#2565): the entries the
    /// list holds at the suit's place in <see cref="Order"/>.
    /// </summary>
    /// <param name="entries">The list's entries at a suit's place.</param>
    internal static Dictionary<string, T> BySuit<T>(Func<int, T> entries) =>
        Order.Select((suit, place) => KeyValuePair.Create(SamusSuitKeys.Installed.Of(suit), entries(place))).ToDictionary(StringComparer.Ordinal);
}
