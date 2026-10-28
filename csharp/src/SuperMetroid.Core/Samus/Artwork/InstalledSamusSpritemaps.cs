using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable visual compositions behind Samus's pose-half table.</summary>
/// <remarks>
/// The 253 pose selectors and 2,096 indexed entries preserve native selection; the selected compositions and their
/// five-byte sprite parts describe appearance; timing and collisions remain in the simulation. An entry with no
/// composition is a native control slot or an omitted half, and draws nothing.
/// </remarks>
public sealed class InstalledSamusSpritemaps
{
    /// <summary>Number of entries in the pose-half table, including repeated compositions and entries with none; not the number of unique compositions or poses.</summary>
    public const int EntryCount = 2096;

    /// <summary>Upper-half pose base indices whose selected values differ from calculated stock values.</summary>
    internal readonly Dictionary<int, ushort> topBases;
    /// <summary>Lower-half pose base indices whose selected values differ from calculated stock values.</summary>
    internal readonly Dictionary<int, ushort> bottomBases;
    /// <summary>Table entries whose selected composition differs from the calculated allocation.</summary>
    internal readonly Dictionary<int, SamusSpritemapId?> entries;
    /// <summary>Validated selected compositions, each at its identity's ordinal.</summary>
    internal readonly SamusSpritemapDefinition[] definitions;

    /// <summary>Validates pose selectors, table entries, and compositions, copying sprite parts and retaining independently edited selections without replacing animation or collision mechanics.</summary>
    /// <param name="topBases">253 upper-half table element indices in native pose order, each below <see cref="EntryCount"/>.</param>
    /// <param name="bottomBases">253 lower-half table element indices in native pose order, each below <see cref="EntryCount"/>.</param>
    /// <param name="entries">2,096 table entries; each names a supplied composition, or none for a slot that draws nothing.</param>
    /// <param name="definitions">Non-null compositions, each identified by its position, with at most 128 non-null-array sprite parts.</param>
    /// <exception cref="InvalidDataException">Selector dimensions or indices, compositions, or entry coverage are invalid.</exception>
    public InstalledSamusSpritemaps(ushort[] topBases, ushort[] bottomBases,
        SamusSpritemapId?[] entries, SamusSpritemapDefinition[] definitions)
    {
        Ensure.NotNull(topBases);
        Ensure.NotNull(bottomBases);
        Ensure.NotNull(entries);
        Ensure.NotNull(definitions);
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
        this.definitions = [.. definitions.Select(definition =>
            definition is not { Parts: { Length: <= 128 } parts }
                ? throw new InvalidDataException("Samus spritemap has an invalid composition.")
                : new SamusSpritemapDefinition((SpritePart[])parts.Clone()))];
        foreach (SamusSpritemapId? entry in entries)
            if (entry is { } composition && (uint)composition.Ordinal >= (uint)this.definitions.Length)
                throw new InvalidDataException($"Samus spritemap {composition.Ordinal} is absent from installed art.");
        this.entries = Enumerable.Range(0, entries.Length)
            .Where(index => SamusSpritemapSharing.SourceIndex(index) != index
                ? entries[index] != entries[SamusSpritemapSharing.SourceIndex(index)]
                : !SamusSpritemapSharing.TryComposition(index, this.definitions.Length, out SamusSpritemapId? calculated) || entries[index] != calculated)
            .ToDictionary(index => index, index => entries[index]);
        foreach (int index in topBases.Concat(bottomBases))
            if (index >= EntryCount)
                throw new InvalidDataException($"Samus spritemap base index {index} is outside the table.");
    }

    /// <summary>SHA-256 of selected pose bases, table entries and ordered composition parts.</summary>
    public string ContentIdentity => field ??= SelectedPresentationHash.Create("samus-spritemaps-v3", content =>
    {
        content.AppendWords("top bases", TopBases);
        content.AppendWords("bottom bases", BottomBases);
        foreach (SamusSpritemapId? entry in Entries)
            content.Append("entry", entry?.Ordinal ?? -1);
        foreach (SamusSpritemapDefinition definition in definitions)
        {
            content.Append("part count", definition.Parts.Length);
            foreach (SpritePart part in definition.Parts)
                part.AppendIdentity(content);
        }
    });

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
            : SamusSpritemapSharing.TryComposition(index, definitions.Length, out SamusSpritemapId? calculated)
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

    /// <summary>Whole frames already built, by composition.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SamusSpritemapId, SpriteFrame> frames = new();

    /// <summary>
    /// The spritemap at <paramref name="index"/> as a whole frame named by its composition, whose parts' characters
    /// are their sheet characters. Returns false only for an entry with no composition, which draws nothing.
    /// </summary>
    public bool TryGetFrame(int index, [NotNullWhen(true)] out SpriteFrame? frame)
    {
        if (Entry(Checked(index)) is not { } composition)
        {
            frame = null;
            return false;
        }
        frame = frames.GetOrAdd(composition, selected => new SpriteFrame($"Samus.{selected.Ordinal}",
            [.. definitions[selected.Ordinal].Parts.Select(part => SpritePartFrames.Part(part, 0))]));
        return true;
    }

    /// <summary>The composition the entry at <paramref name="index"/> names; false only for an entry with none, which draws nothing.</summary>
    public bool TryGetComposition(int index, out SamusSpritemapId composition)
    {
        SamusSpritemapId? entry = Entry(Checked(index));
        composition = entry ?? default;
        return entry is not null;
    }

    /// <summary>Returns false only for an entry with no composition, which draws nothing.</summary>
    public bool TryGet(int index, [NotNullWhen(true)] out SamusSpritemapDefinition? definition)
    {
        definition = Entry(Checked(index)) is { } composition ? definitions[composition.Ordinal] : null;
        return definition is not null;
    }

    /// <summary>Requires <paramref name="index"/> to be a table entry.</summary>
    private static int Checked(int index) => index is < 0 or >= EntryCount
        ? throw new InvalidDataException($"Samus spritemap index {index} is outside extracted art.")
        : index;
}
