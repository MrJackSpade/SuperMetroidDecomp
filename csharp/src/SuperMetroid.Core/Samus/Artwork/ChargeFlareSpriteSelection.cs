using static SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which charge-flare sprite each animation index draws.</summary>
public static class ChargeFlareSpriteSelection
{
    /// <summary>
    /// The composition each of the fifty-four native flare-table positions selects: thirty main-flare phases, then six
    /// phases each of right slow, right fast, left slow and left fast sparks. Repeated main-flare phases stay separate
    /// positions.
    /// </summary>
    public static IReadOnlyList<ProjectileSpriteId> Selectors { get; } = [.. Enumerable.Range(0, 54).Select(Selector)];

    /// <summary>True when <paramref name="selector"/> indexes the native $93:A1A1 flare table.</summary>
    public static bool Contains(int selector) => selector < Selectors.Count;

    /// <summary>The composition flare-table position <paramref name="index"/> selects.</summary>
    private static ProjectileSpriteId Selector(int index) =>
        All[index >= 30 ? index - 26 : index < 8 ? index % 2 : index < 16 ? 2 - index % 2 : 2 + index % 2];
}
