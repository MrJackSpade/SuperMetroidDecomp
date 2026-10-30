using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The regions of a graphics set's OBJ characters that change after its room loads, and what each can show (#1392): those
/// its encounters upload over, each showing an upload or the set's own characters, and the rows its rotting corpses upload,
/// showing the rot steps each corpse's uploads can leave them in or, before a corpse's first, what the encounters left
/// there. The sheet composition pictures frames from them, and the installation checks the pictures a sheet holds against them.
/// </summary>
internal sealed class SetRegions
{
    /// <summary>Each region a rotting corpse uploads a row over, with the corpse and the row.</summary>
    private readonly Dictionary<CharacterRange, CorpseRegion> corpseRows = [];

    /// <summary>
    /// The uploads the set's encounters make and the corpses that rot in its rooms: the regions both divide the characters
    /// into, a corpse's row being one region or several where encounters upload over parts of it.
    /// </summary>
    /// <exception cref="InvalidDataException">Two corpses upload rows over the same characters.</exception>
    internal SetRegions(IReadOnlyList<SpriteArtworkPlacement> uploads, IReadOnlyList<CorpseRotSimulation> corpses)
    {
        Uploads = Ensure.NotNull(uploads);
        CorpseRegion[] rows = [.. Ensure.NotNull(corpses).SelectMany(corpse => corpse.Rows.Select(row => new CorpseRegion(corpse, row)))];
        foreach (CorpseRegion row in rows)
        {
            if (rows.Any(other => !ReferenceEquals(other, row) && Overlaps(other.Row, row.Row)))
                throw new InvalidDataException($"{row.Corpse.Flesh} uploads a row at table byte {row.Row.FirstByte} over another corpse's row.");
        }
        int[] bounds = [.. uploads.SelectMany(upload => new[] { upload.FirstByte, upload.FirstByte + upload.ByteCount })
            .Concat(rows.SelectMany(row => new[] { row.Row.FirstByte, row.Row.FirstByte + row.Row.ByteCount })).Distinct().Order()];
        var regions = new List<CharacterRange>();
        for (int index = 0; index + 1 < bounds.Length; index++)
        {
            var region = new CharacterRange(bounds[index], bounds[index + 1] - bounds[index]);
            CorpseRegion? row = rows.FirstOrDefault(row => row.Row.FirstByte <= region.FirstByte
                && region.FirstByte + region.ByteCount <= row.Row.FirstByte + row.Row.ByteCount);
            if (row is not null)
                corpseRows.Add(region, row);
            if (row is not null || CharacterRange.Over(uploads, region).Any())
                regions.Add(region);
        }
        Regions = regions;
        for (int ordinal = 0; ordinal < regions.Count; ordinal++)
            named.Add(regions[ordinal], new LookRegion(ordinal, $"{regions[ordinal].FirstByte:X4}"));
    }

    /// <summary>The uploads the set's encounters make.</summary>
    internal IReadOnlyList<SpriteArtworkPlacement> Uploads { get; }

    /// <summary>The ranges the uploads and the corpses' rows divide the characters into.</summary>
    internal IReadOnlyList<CharacterRange> Regions { get; }

    /// <summary>Each range's region, as Core names it: by its ordinal among the set's ranges, and the name its pictures spell it by.</summary>
    private readonly Dictionary<CharacterRange, LookRegion> named = [];

    /// <summary>The region <paramref name="range"/> is, as Core names it.</summary>
    /// <exception cref="InvalidDataException">The range is not one of the set's regions.</exception>
    internal LookRegion RegionOf(CharacterRange range) => named.TryGetValue(Ensure.NotNull(range), out LookRegion? region) ? region
        : throw new InvalidDataException($"Table bytes {range.FirstByte}+{range.ByteCount} are not a region of the set's characters.");

    /// <summary>The range region <paramref name="region"/> spans.</summary>
    internal CharacterRange RangeOf(LookRegion region) => Regions[Ensure.NotNull(region).Ordinal];

    /// <summary>
    /// What covers each of the set's regions (#1406): each of its encounters' uploads, each entry of the set's own load and
    /// the standard sprites, by the bytes each shares with a region, as a room's characters show the last of them over it;
    /// and each of Mother Brain's corpse rows, by the regions it spans.
    /// </summary>
    internal SetLookCoverage Coverage(RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(set);
        var bySource = new Dictionary<SpriteArtworkSource, LookRegion[]>();
        void Add(SpriteArtworkSource source, int firstByte, int byteCount)
        {
            LookRegion[] covered = [.. Regions.Where(range => range.Overlaps(firstByte, byteCount)).Select(RegionOf)];
            bySource[source] = bySource.TryGetValue(source, out LookRegion[]? earlier) ? [.. earlier.Union(covered)] : covered;
        }
        foreach (SpriteArtworkPlacement upload in Uploads)
            Add(upload.Source, upload.FirstByte, upload.ByteCount);
        foreach (RoomEnemyGraphicsSetEntry entry in EnemyGraphicsSet.LayOut(CompiledRoomEnemyDefinitions.Instance, set))
            Add(new GraphicsSetTileCharacters(entry),
                EncounterCharacterPlacementDefinitions.EnemyTileAreaFirstCharacter * SpriteCharacterTableLayout.CharacterByteCount + entry.StagingOffset, entry.TileByteCount);
        Add(new InstalledArtworkCharacters(InstalledArtworkId.StandardObjectTiles), 0, StandardObjectArtworkFormat.ByteCount);
        LookRegion[][] rotRows = corpseRows.Count == 0 ? [] : [.. MotherBrainCorpseArtworkDefinitions.RotRows.Select(row =>
            Regions.Where(range => range.FirstByte >= row.FirstCharacter * CharacterBytes
                && range.FirstByte + range.ByteCount <= row.FirstCharacter * CharacterBytes + row.ByteCount).Select(RegionOf).ToArray())];
        return new SetLookCoverage([.. Regions.Select(RegionOf)], bySource, rotRows);
    }

    /// <summary>The bytes one four-bit character holds.</summary>
    private const int CharacterBytes = 32;

    /// <summary>
    /// What <paramref name="regions"/> can show together: each upload or the set's own characters in each region an
    /// encounter uploads, by the cartesian product, and each state each corpse's rows can show together, as a corpse
    /// uploads its rows together and rots apart from the others; a corpse's row it has not yet uploaded shows what the
    /// encounters left there. Only a corpse's own frames show its rot, so unless <paramref name="showsRot"/> a corpse's
    /// rows show only what the encounters left there.
    /// </summary>
    internal IEnumerable<IReadOnlyList<LookRegionContent>> Contents(IReadOnlyList<CharacterRange> regions, bool showsRot)
    {
        Ensure.NotNull(regions);
        IEnumerable<Dictionary<CharacterRange, LookRegionContent>> combinations = [[]];
        foreach (CharacterRange encounter in regions.Where(region => !corpseRows.ContainsKey(region)))
            combinations = Combine(combinations, Encountered([encounter]));
        foreach (IGrouping<CorpseRotSimulation, CharacterRange> corpse in regions.Where(corpseRows.ContainsKey)
                     .GroupBy(region => corpseRows[region].Corpse))
            combinations = Combine(combinations, Corpse(corpse.Key, [.. corpse], showsRot));
        // A finished corpse's region replaced by an upload shows what an unrotted region with that upload shows, so each
        // combination is kept once.
        return combinations.Select(combination => (IReadOnlyList<LookRegionContent>)[.. regions.Select(region => combination[region])])
            .Distinct(SameContents.Instance);
    }

    /// <summary>Compares two combinations region by region.</summary>
    private sealed class SameContents : IEqualityComparer<IReadOnlyList<LookRegionContent>>
    {
        /// <summary>The one comparer.</summary>
        internal static SameContents Instance { get; } = new();

        /// <inheritdoc/>
        public bool Equals(IReadOnlyList<LookRegionContent>? x, IReadOnlyList<LookRegionContent>? y) =>
            ReferenceEquals(x, y) || x is not null && y is not null && x.SequenceEqual(y);

        /// <inheritdoc/>
        public int GetHashCode(IReadOnlyList<LookRegionContent> obj)
        {
            var hash = new HashCode();
            foreach (LookRegionContent content in Ensure.NotNull(obj))
                hash.Add(content);
            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// The names of the pictures of <paramref name="frame"/>: the frame whole once when it has no
    /// <paramref name="layers"/>, else each layer once for each content <see cref="Contents"/> gives its regions.
    /// </summary>
    internal IEnumerable<string> PictureNames(string frame, IReadOnlyList<CharacterRange[]> layers, bool showsRot) =>
        Ensure.NotNull(layers).Count == 0 ? [frame]
            : layers.SelectMany((regions, layer) => Contents(regions, showsRot)
                .Select(contents => SuperMetroid.AssetExtraction.Enemies.EncounterCharacterPlacements.Catalog.PictureName(frame, layer, contents)));

    /// <summary>The bytes <paramref name="content"/>'s region of a corpse row shows after its rot step.</summary>
    /// <exception cref="InvalidOperationException">The content shows no rot step.</exception>
    internal ReadOnlySpan<byte> RotBytes(LookRegionContent content)
    {
        Ensure.NotNull(content);
        CharacterRange range = RangeOf(content.Region);
        CorpseRegion corpse = corpseRows[range];
        return corpse.Corpse.RowBytes(corpse.Row, content.RotStep ?? throw new InvalidOperationException(
                $"The region at table byte {range.FirstByte} shows no rot step."))
            .Slice(range.FirstByte - corpse.Row.FirstByte, range.ByteCount);
    }

    /// <summary>What the encounters' uploads can leave <paramref name="regions"/> showing, by the cartesian product.</summary>
    private IEnumerable<IReadOnlyList<LookRegionContent>> Encountered(IReadOnlyList<CharacterRange> regions)
    {
        IEnumerable<IReadOnlyList<LookRegionContent>> combinations = [[]];
        foreach (CharacterRange range in regions)
        {
            LookRegion region = RegionOf(range);
            LookRegionContent[] contents = [new(region, null), .. CharacterRange.Over(Uploads, range).Select(upload => new LookRegionContent(region, upload.Source))];
            combinations = [.. combinations.SelectMany(earlier => contents.Select(content => (IReadOnlyList<LookRegionContent>)[.. earlier, content]))];
        }
        return combinations;
    }

    /// <summary>
    /// What a corpse's <paramref name="regions"/> can show together: for each state its rows can show together, a rot
    /// step in each region of a row it uploaded, and what the encounters left in each of a row it has not. Once the corpse
    /// has finished rotting no rot step uploads again, so an encounter's later upload may replace any rotted region (#1435).
    /// </summary>
    private IEnumerable<IReadOnlyList<LookRegionContent>> Corpse(CorpseRotSimulation corpse, CharacterRange[] regions, bool showsRot)
    {
        CorpseRotRow[] rows = [.. regions.Select(region => corpseRows[region].Row).Distinct()];
        IEnumerable<int?[]> states = showsRot ? corpse.Shown(rows) : [new int?[rows.Length]];
        int?[] finished = corpse.Finished(rows);
        foreach (int?[] state in states)
        {
            if (state.Any(step => step is not null) && state.SequenceEqual(finished))
            {
                foreach (IReadOnlyList<LookRegionContent> shown in AfterRot(regions, region => state[Array.IndexOf(rows, corpseRows[region].Row)]))
                    yield return shown;
                continue;
            }
            CharacterRange[] unrotted = [.. regions.Where(region => state[Array.IndexOf(rows, corpseRows[region].Row)] is null)];
            foreach (IReadOnlyList<LookRegionContent> encountered in Encountered(unrotted))
            {
                yield return [.. regions.Select(region => state[Array.IndexOf(rows, corpseRows[region].Row)] is int step
                    ? new LookRegionContent(RegionOf(region), null, step)
                    : encountered.Single(content => content.Region == RegionOf(region)))];
            }
        }
    }

    /// <summary>
    /// What a finished corpse's <paramref name="regions"/> can show: each rotted region its final step or any upload made
    /// over it since, each region it never rotted what the encounters left there.
    /// </summary>
    private IEnumerable<IReadOnlyList<LookRegionContent>> AfterRot(CharacterRange[] regions, Func<CharacterRange, int?> finalStep)
    {
        IEnumerable<IReadOnlyList<LookRegionContent>> combinations = [[]];
        foreach (CharacterRange range in regions)
        {
            LookRegion region = RegionOf(range);
            IEnumerable<LookRegionContent> uploaded = CharacterRange.Over(Uploads, range).Select(upload => new LookRegionContent(region, upload.Source));
            LookRegionContent[] contents = finalStep(range) is int step
                ? [new(region, null, step), .. uploaded]
                : [new(region, null), .. uploaded];
            combinations = [.. combinations.SelectMany(earlier => contents.Select(content => (IReadOnlyList<LookRegionContent>)[.. earlier, content]))];
        }
        return combinations;
    }

    /// <summary>Every combination of an earlier one and one of <paramref name="contents"/>.</summary>
    private IEnumerable<Dictionary<CharacterRange, LookRegionContent>> Combine(
        IEnumerable<Dictionary<CharacterRange, LookRegionContent>> combinations, IEnumerable<IReadOnlyList<LookRegionContent>> contents)
    {
        IReadOnlyList<LookRegionContent>[] shown = [.. contents];
        return [.. combinations.SelectMany(earlier => shown.Select(content =>
        {
            var combined = new Dictionary<CharacterRange, LookRegionContent>(earlier);
            foreach (LookRegionContent region in content)
                combined.Add(RangeOf(region.Region), region);
            return combined;
        }))];
    }

    /// <summary>Whether two rows share a byte.</summary>
    private static bool Overlaps(CorpseRotRow row, CorpseRotRow other) =>
        other.FirstByte < row.FirstByte + row.ByteCount && row.FirstByte < other.FirstByte + other.ByteCount;

    /// <summary>A row a rotting corpse uploads, with the corpse.</summary>
    private sealed record CorpseRegion(CorpseRotSimulation Corpse, CorpseRotRow Row);
}
