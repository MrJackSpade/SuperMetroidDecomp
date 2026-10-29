using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Which of a graphics set's regions each look shown over what its frames read covers (#1392, #1406, #1463): every look an
/// encounter, the set's own load or the standard sprites show, and each of Mother Brain's corpse rows. Extraction computes
/// it from where each lands; the set's frames read their regions through it.
/// </summary>
public sealed class SetLookCoverage
{
    /// <summary>The regions each shown look covers.</summary>
    private readonly IReadOnlyDictionary<SpriteArtworkSource, LookRegion[]> byLook;

    /// <summary>The regions each of Mother Brain's corpse rows covers, by row.</summary>
    private readonly IReadOnlyList<LookRegion[]> rotRows;

    /// <summary>The set's <paramref name="regions"/> and what covers each.</summary>
    public SetLookCoverage(IReadOnlyList<LookRegion> regions, IReadOnlyDictionary<SpriteArtworkSource, LookRegion[]> byLook,
        IReadOnlyList<LookRegion[]> rotRows)
    {
        Regions = Ensure.NotNull(regions);
        this.byLook = Ensure.NotNull(byLook);
        this.rotRows = Ensure.NotNull(rotRows);
    }

    /// <summary>The coverage of a set whose regions nothing shows over after its room loads.</summary>
    public static SetLookCoverage None { get; } = new([], new Dictionary<SpriteArtworkSource, LookRegion[]>(), []);

    /// <summary>The set's regions, in order.</summary>
    public IReadOnlyList<LookRegion> Regions { get; }

    /// <summary>Whether showing <paramref name="look"/> covers <paramref name="region"/>.</summary>
    public bool Covers(SpriteArtworkSource look, LookRegion region) =>
        byLook.TryGetValue(Ensure.NotNull(look), out LookRegion[]? covered) && covered.Contains(region);

    /// <summary>Whether the set records what <paramref name="look"/> covers: whether showing it under the set is known.</summary>
    public bool Knows(SpriteArtworkSource look) => byLook.ContainsKey(Ensure.NotNull(look));

    /// <summary>The regions Mother Brain's corpse row <paramref name="row"/> covers.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The set covers no such row.</exception>
    public IReadOnlyList<LookRegion> RotRow(int row) => (uint)row < (uint)rotRows.Count ? rotRows[row]
        : throw new ArgumentOutOfRangeException(nameof(row), row, $"The set's coverage holds {rotRows.Count} corpse rows.");

    /// <summary>
    /// What shows over <paramref name="region"/> now: of the looks still visible in <paramref name="shown"/> as
    /// <paramref name="catalog"/> judges them, the newest that covers it, named when it is one of
    /// <paramref name="setLooks"/>, or none while the graphics set's own look, which the room load showed, or the standard
    /// sprites show there.
    /// </summary>
    /// <exception cref="InvalidDataException">A look an encounter of another graphics set showed is still visible.</exception>
    public LookRegionContent Showing(ShownSpriteArtwork shown, IEncounterLookCatalog catalog, IReadOnlyList<SpriteArtworkSource> setLooks,
        LookRegion region)
    {
        Ensure.NotNull(shown);
        Ensure.NotNull(catalog);
        Ensure.NotNull(setLooks);
        Ensure.NotNull(region);
        foreach (SpriteArtworkSource look in shown.VisibleNewestFirst(catalog))
        {
            if (!Knows(look) && catalog.IsEncounterLook(look))
                throw new InvalidDataException($"{look}, shown by an encounter of another graphics set, shows over a frame drawn under this one.");
            if (Covers(look, region))
                return new LookRegionContent(region, setLooks.Contains(look) ? look : null);
        }
        return new LookRegionContent(region, null);
    }
}
