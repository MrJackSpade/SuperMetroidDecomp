using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which pre-coloured body picture each entry of Samus's pose-half table selects.</summary>
/// <remarks>
/// The 253 pose selectors and 2,096 indexed entries preserve native selection: a pose's top or bottom base plus its
/// animation frame names an entry, and the entry names a composition, or none for a native control slot or an
/// omitted half, which draws nothing. Each composition is one picture on the body sheets, which shows the top or
/// bottom definition record selected for the frame when the picture reads that half (#1463). The compositions' parts
/// are the import pipeline's, which composes the sheets from them; timing and collisions remain in the simulation.
/// </remarks>
public abstract class InstalledSamusSpritemaps
{
    /// <summary>Number of entries in the pose-half table, including repeated compositions and entries with none; not the number of unique compositions or poses.</summary>
    public const int EntryCount = 2096;

    /// <summary>Upper-half pose base indices whose selected values differ from calculated stock values.</summary>
    internal readonly Dictionary<int, ushort> topBases;
    /// <summary>Lower-half pose base indices whose selected values differ from calculated stock values.</summary>
    internal readonly Dictionary<int, ushort> bottomBases;
    /// <summary>Table entries whose selected composition differs from the calculated allocation.</summary>
    internal readonly Dictionary<int, SamusSpritemapId?> entries;
    /// <summary>The body halves each composition's picture shows, at its identity's ordinal.</summary>
    private readonly SamusBodyHalvesShown[] halves;

    /// <summary>Validates pose selectors and table entries, retaining independently edited selections without replacing animation or collision mechanics.</summary>
    /// <param name="topBases">253 upper-half table element indices in native pose order, each below <see cref="EntryCount"/>.</param>
    /// <param name="bottomBases">253 lower-half table element indices in native pose order, each below <see cref="EntryCount"/>.</param>
    /// <param name="entries">2,096 table entries; each names a supplied composition, or none for a slot that draws nothing.</param>
    /// <param name="halves">The body halves each composition's picture shows, each at its composition's ordinal.</param>
    /// <exception cref="InvalidDataException">Selector dimensions or indices, or entry coverage, are invalid.</exception>
    protected InstalledSamusSpritemaps(ushort[] topBases, ushort[] bottomBases,
        SamusSpritemapId?[] entries, SamusBodyHalvesShown[] halves)
    {
        Ensure.NotNull(topBases);
        Ensure.NotNull(bottomBases);
        Ensure.NotNull(entries);
        Ensure.NotNull(halves);
        if (topBases.Length != InstalledSamusBodyArtwork.PoseCount ||
            bottomBases.Length != InstalledSamusBodyArtwork.PoseCount ||
            entries.Length != EntryCount)
            throw new InvalidDataException("Samus spritemap selector tables have an invalid length.");
        this.topBases = Enumerable.Range(0, topBases.Length)
            .Where(pose => topBases[pose] != SamusSpritemapPoseBases.TopBase(SamusPoseNumbering.FromNumber(pose)))
            .ToDictionary(pose => pose, pose => topBases[pose]);
        this.bottomBases = Enumerable.Range(0, bottomBases.Length)
            .Where(pose => bottomBases[pose] != SamusSpritemapPoseBases.BottomBase(SamusPoseNumbering.FromNumber(pose)))
            .ToDictionary(pose => pose, pose => bottomBases[pose]);
        this.halves = (SamusBodyHalvesShown[])halves.Clone();
        foreach (SamusSpritemapId? entry in entries)
            if (entry is { } composition && (uint)composition.Ordinal >= (uint)this.halves.Length)
                throw new InvalidDataException($"Samus spritemap {composition.Ordinal} is absent from installed art.");
        this.entries = Enumerable.Range(0, entries.Length)
            .Where(index => SamusSpritemapSharing.SourceIndex(index) != index
                ? entries[index] != entries[SamusSpritemapSharing.SourceIndex(index)]
                : !SamusSpritemapSharing.TryComposition(index, this.halves.Length, out SamusSpritemapId? calculated) || entries[index] != calculated)
            .ToDictionary(index => index, index => entries[index]);
        foreach (int index in topBases.Concat(bottomBases))
            if (index >= EntryCount)
                throw new InvalidDataException($"Samus spritemap base index {index} is outside the table.");
    }

    /// <summary>SHA-256 of the selected pose bases, table entries and compositions.</summary>
    public abstract string ContentIdentity { get; }

    /// <summary>Number of installed compositions.</summary>
    public int CompositionCount => halves.Length;

    /// <summary>Materializes all 253 upper-half element-index bases in pose order; callers add the animation frame to obtain a pointer-table index.</summary>
    public ReadOnlySpan<ushort> TopBases => topBasesTable ??= Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount).Select(pose => TopBase(SamusPoseNumbering.FromNumber(pose))).ToArray();
    /// <summary>The materialized <see cref="TopBases"/>; the installed tables never change after construction.</summary>
    private ushort[]? topBasesTable;
    /// <summary>Materializes all 253 lower-half element-index bases in pose order, including poses sharing the upper/lower composition.</summary>
    public ReadOnlySpan<ushort> BottomBases => bottomBasesTable ??= Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount).Select(pose => BottomBase(SamusPoseNumbering.FromNumber(pose))).ToArray();
    /// <summary>The materialized <see cref="BottomBases"/>; the installed tables never change after construction.</summary>
    private ushort[]? bottomBasesTable;
    /// <summary>Materializes the complete table in native order, preserving repeated compositions and entries with none.</summary>
    public ReadOnlySpan<SamusSpritemapId?> Entries => entriesTable ??= Enumerable.Range(0, EntryCount).Select(Entry).ToArray();
    /// <summary>The materialized <see cref="Entries"/>; the installed tables never change after construction.</summary>
    private SamusSpritemapId?[]? entriesTable;
    /// <summary>Resolves a table entry through an override, shared source entry, or calculated stock composition.</summary>
    private SamusSpritemapId? Entry(int index) => entries.TryGetValue(index, out SamusSpritemapId? value)
        ? value : SamusSpritemapSharing.SourceIndex(index) != index
            ? Entry(SamusSpritemapSharing.SourceIndex(index))
            : SamusSpritemapSharing.TryComposition(index, halves.Length, out SamusSpritemapId? calculated)
                ? calculated : throw new InvalidDataException("Installed OAM allocation no longer supplies its selected composition.");
    /// <summary>Resolves the installed upper-half base corresponding to native $92:9263, using calculated stock selection unless independently edited.</summary>
    /// <param name="pose">Native Samus pose identity, $00..$FC.</param>
    /// <returns>A pointer-table element index, not a bank address or byte offset; add the current animation frame before composition lookup.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pose is outside the 253 supported identities.</exception>
    public ushort TopBase(SamusPoseId pose)
    {
        if (!SamusPoseNumbering.HasRecord(pose)) throw new ArgumentOutOfRangeException(nameof(pose));
        int number = SamusPoseNumbering.Number(pose);
        return topBases.ContainsKey(number) ? topBases[number] : SamusSpritemapPoseBases.TopBase(pose);
    }
    /// <summary>Resolves the installed lower-half base corresponding to native $92:945D, independently of whether the current draw policy displays that half.</summary>
    /// <param name="pose">Native Samus pose identity, $00..$FC.</param>
    /// <returns>A pointer-table element index to which the current animation frame is added.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pose is outside the 253 supported identities.</exception>
    public ushort BottomBase(SamusPoseId pose)
    {
        if (!SamusPoseNumbering.HasRecord(pose)) throw new ArgumentOutOfRangeException(nameof(pose));
        int number = SamusPoseNumbering.Number(pose);
        return bottomBases.ContainsKey(number) ? bottomBases[number] : SamusSpritemapPoseBases.BottomBase(pose);
    }

    /// <summary>The composition the entry at <paramref name="index"/> names; false only for an entry with none, which draws nothing.</summary>
    public bool TryGetComposition(int index, out SamusSpritemapId composition)
    {
        SamusSpritemapId? entry = Entry(Checked(index));
        composition = entry ?? default;
        return entry is not null;
    }

    /// <summary>The body halves <paramref name="composition"/>'s picture shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No such composition is installed.</exception>
    public SamusBodyHalvesShown HalvesShown(SamusSpritemapId composition) => (uint)composition.Ordinal < (uint)halves.Length
        ? halves[composition.Ordinal]
        : throw new ArgumentOutOfRangeException(nameof(composition), composition, "No such Samus composition is installed.");

    /// <summary>
    /// The pre-coloured body picture the entry at <paramref name="index"/> shows with <paramref name="records"/>
    /// selected: its composition with the selected record of each half the picture shows. False only for an entry with
    /// no composition, which draws nothing.
    /// </summary>
    public bool TryGetFrameKey(int index, SamusBodyRecords records, out SamusBodyFrameKey key)
    {
        if (!TryGetComposition(index, out SamusSpritemapId composition))
        {
            key = default;
            return false;
        }
        SamusBodyHalvesShown shown = halves[composition.Ordinal];
        key = new SamusBodyFrameKey(composition, shown.Top ? records.Top : null, shown.Bottom ? records.Bottom : null);
        return true;
    }

    /// <summary>Requires <paramref name="index"/> to be a table entry.</summary>
    private static int Checked(int index) => index is < 0 or >= EntryCount
        ? throw new InvalidDataException($"Samus spritemap index {index} is outside extracted art.")
        : index;
}
