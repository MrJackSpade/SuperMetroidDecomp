namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Which of a graphics set's regions each thing shown over its characters covers (#1392, #1406): every source an
/// encounter, the set's own load or the standard sprites show, and each of Mother Brain's corpse rows. Extraction
/// computes it from where each lands; the set's frames read their regions through it.
/// </summary>
public sealed class SetCharacterCoverage
{
    /// <summary>The regions each shown source covers.</summary>
    private readonly IReadOnlyDictionary<ObjCharacterSource, CharacterRegion[]> bySource;

    /// <summary>The regions each of Mother Brain's corpse rows covers, by row.</summary>
    private readonly IReadOnlyList<CharacterRegion[]> rotRows;

    /// <summary>The set's <paramref name="regions"/> and what covers each.</summary>
    public SetCharacterCoverage(IReadOnlyList<CharacterRegion> regions, IReadOnlyDictionary<ObjCharacterSource, CharacterRegion[]> bySource,
        IReadOnlyList<CharacterRegion[]> rotRows)
    {
        Regions = Ensure.NotNull(regions);
        this.bySource = Ensure.NotNull(bySource);
        this.rotRows = Ensure.NotNull(rotRows);
    }

    /// <summary>The coverage of a set whose characters nothing shows over after its room loads.</summary>
    public static SetCharacterCoverage None { get; } = new([], new Dictionary<ObjCharacterSource, CharacterRegion[]>(), []);

    /// <summary>The set's regions, in order.</summary>
    public IReadOnlyList<CharacterRegion> Regions { get; }

    /// <summary>Whether showing <paramref name="source"/> covers <paramref name="region"/>.</summary>
    public bool Covers(ObjCharacterSource source, CharacterRegion region) =>
        bySource.TryGetValue(Ensure.NotNull(source), out CharacterRegion[]? covered) && covered.Contains(region);

    /// <summary>Whether the set records what <paramref name="source"/> covers: whether showing it over the set's characters is known.</summary>
    public bool Knows(ObjCharacterSource source) => bySource.ContainsKey(Ensure.NotNull(source));

    /// <summary>The regions Mother Brain's corpse row <paramref name="row"/> covers.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The set covers no such row.</exception>
    public IReadOnlyList<CharacterRegion> RotRow(int row) => (uint)row < (uint)rotRows.Count ? rotRows[row]
        : throw new ArgumentOutOfRangeException(nameof(row), row, $"The set's coverage holds {rotRows.Count} corpse rows.");
}
