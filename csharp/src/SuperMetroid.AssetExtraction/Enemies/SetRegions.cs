using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

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
    internal SetRegions(IReadOnlyList<RuntimeCharacterUpload> uploads, IReadOnlyList<CorpseRotSimulation> corpses)
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
            named.Add(regions[ordinal], new CharacterRegion(ordinal, $"{regions[ordinal].FirstByte:X4}"));
    }

    /// <summary>The uploads the set's encounters make.</summary>
    internal IReadOnlyList<RuntimeCharacterUpload> Uploads { get; }

    /// <summary>The ranges the uploads and the corpses' rows divide the characters into.</summary>
    internal IReadOnlyList<CharacterRange> Regions { get; }

    /// <summary>Each range's region, as Core names it: by its ordinal among the set's ranges, and the name its pictures spell it by.</summary>
    private readonly Dictionary<CharacterRange, CharacterRegion> named = [];

    /// <summary>The region <paramref name="range"/> is, as Core names it.</summary>
    /// <exception cref="InvalidDataException">The range is not one of the set's regions.</exception>
    internal CharacterRegion RegionOf(CharacterRange range) => named.TryGetValue(Ensure.NotNull(range), out CharacterRegion? region) ? region
        : throw new InvalidDataException($"Table bytes {range.FirstByte}+{range.ByteCount} are not a region of the set's characters.");

    /// <summary>The range region <paramref name="region"/> spans.</summary>
    internal CharacterRange RangeOf(CharacterRegion region) => Regions[Ensure.NotNull(region).Ordinal];

    /// <summary>
    /// What covers each of the set's regions (#1406): each of its encounters' uploads, each entry of the set's own load and
    /// the standard sprites, by the bytes each shares with a region, as a room's characters show the last of them over it;
    /// and each of Mother Brain's corpse rows, by the regions it spans.
    /// </summary>
    internal SetCharacterCoverage Coverage(RoomEnemyGraphicsSetDefinition set)
    {
        Ensure.NotNull(set);
        var bySource = new Dictionary<ObjCharacterSource, CharacterRegion[]>();
        void Add(ObjCharacterSource source, int firstByte, int byteCount)
        {
            CharacterRegion[] covered = [.. Regions.Where(range => range.Overlaps(firstByte, byteCount)).Select(RegionOf)];
            bySource[source] = bySource.TryGetValue(source, out CharacterRegion[]? earlier) ? [.. earlier.Union(covered)] : covered;
        }
        foreach (RuntimeCharacterUpload upload in Uploads)
            Add(upload.Source, upload.FirstByte, upload.ByteCount);
        foreach (RoomEnemyGraphicsSetEntry entry in EnemyGraphicsSet.LayOut(CompiledRoomEnemyDefinitions.Instance, set))
            Add(new TransferObjCharacters(entry.Definition.TileDataAddress, entry.TileByteCount),
                EnemyGraphicsSet.EnemyTileAreaFirstByte + entry.StagingOffset, entry.TileByteCount);
        Add(new AssetObjCharacters(VramAssetId.StandardObjectTiles), 0, StandardObjectArtworkFormat.ByteCount);
        CharacterRegion[][] rotRows = corpseRows.Count == 0 ? [] : [.. MotherBrainCorpseArtworkDefinitions.RotRows.Select(row =>
            Regions.Where(range => range.FirstByte >= row.FirstCharacter * CharacterBytes
                && range.FirstByte + range.ByteCount <= row.FirstCharacter * CharacterBytes + row.ByteCount).Select(RegionOf).ToArray())];
        return new SetCharacterCoverage([.. Regions.Select(RegionOf)], bySource, rotRows);
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
    internal IEnumerable<IReadOnlyList<CharacterRegionContent>> Contents(IReadOnlyList<CharacterRange> regions, bool showsRot)
    {
        Ensure.NotNull(regions);
        IEnumerable<Dictionary<CharacterRange, CharacterRegionContent>> combinations = [[]];
        foreach (CharacterRange encounter in regions.Where(region => !corpseRows.ContainsKey(region)))
            combinations = Combine(combinations, Encountered([encounter]));
        foreach (IGrouping<CorpseRotSimulation, CharacterRange> corpse in regions.Where(corpseRows.ContainsKey)
                     .GroupBy(region => corpseRows[region].Corpse))
            combinations = Combine(combinations, Corpse(corpse.Key, [.. corpse], showsRot));
        return combinations.Select(combination => (IReadOnlyList<CharacterRegionContent>)[.. regions.Select(region => combination[region])]);
    }

    /// <summary>
    /// The names of the pictures of <paramref name="frame"/>: the frame whole once when it has no
    /// <paramref name="layers"/>, else each layer once for each content <see cref="Contents"/> gives its regions.
    /// </summary>
    internal IEnumerable<string> PictureNames(string frame, IReadOnlyList<CharacterRange[]> layers, bool showsRot) =>
        Ensure.NotNull(layers).Count == 0 ? [frame]
            : layers.SelectMany((regions, layer) => Contents(regions, showsRot)
                .Select(contents => SuperMetroid.AssetExtraction.Enemies.RuntimeCharacterUploadPlacements.Catalog.PictureName(frame, layer, contents)));

    /// <summary>The bytes <paramref name="content"/>'s region of a corpse row shows after its rot step.</summary>
    /// <exception cref="InvalidOperationException">The content shows no rot step.</exception>
    internal ReadOnlySpan<byte> RotBytes(CharacterRegionContent content)
    {
        Ensure.NotNull(content);
        CharacterRange range = RangeOf(content.Region);
        CorpseRegion corpse = corpseRows[range];
        return corpse.Corpse.RowBytes(corpse.Row, content.RotStep ?? throw new InvalidOperationException(
                $"The region at table byte {range.FirstByte} shows no rot step."))
            .Slice(range.FirstByte - corpse.Row.FirstByte, range.ByteCount);
    }

    /// <summary>What the encounters' uploads can leave <paramref name="regions"/> showing, by the cartesian product.</summary>
    private IEnumerable<IReadOnlyList<CharacterRegionContent>> Encountered(IReadOnlyList<CharacterRange> regions)
    {
        IEnumerable<IReadOnlyList<CharacterRegionContent>> combinations = [[]];
        foreach (CharacterRange range in regions)
        {
            CharacterRegion region = RegionOf(range);
            CharacterRegionContent[] contents = [new(region, null), .. CharacterRange.Over(Uploads, range).Select(upload => new CharacterRegionContent(region, upload))];
            combinations = [.. combinations.SelectMany(earlier => contents.Select(content => (IReadOnlyList<CharacterRegionContent>)[.. earlier, content]))];
        }
        return combinations;
    }

    /// <summary>
    /// What a corpse's <paramref name="regions"/> can show together: for each state its rows can show together, a rot
    /// step in each region of a row it uploaded, and what the encounters left in each of a row it has not.
    /// </summary>
    private IEnumerable<IReadOnlyList<CharacterRegionContent>> Corpse(CorpseRotSimulation corpse, CharacterRange[] regions, bool showsRot)
    {
        CorpseRotRow[] rows = [.. regions.Select(region => corpseRows[region].Row).Distinct()];
        IEnumerable<int?[]> states = showsRot ? corpse.Shown(rows) : [new int?[rows.Length]];
        foreach (int?[] state in states)
        {
            CharacterRange[] unrotted = [.. regions.Where(region => state[Array.IndexOf(rows, corpseRows[region].Row)] is null)];
            foreach (IReadOnlyList<CharacterRegionContent> encountered in Encountered(unrotted))
            {
                yield return [.. regions.Select(region => state[Array.IndexOf(rows, corpseRows[region].Row)] is int step
                    ? new CharacterRegionContent(RegionOf(region), null, step)
                    : encountered.Single(content => content.Region == RegionOf(region)))];
            }
        }
    }

    /// <summary>Every combination of an earlier one and one of <paramref name="contents"/>.</summary>
    private IEnumerable<Dictionary<CharacterRange, CharacterRegionContent>> Combine(
        IEnumerable<Dictionary<CharacterRange, CharacterRegionContent>> combinations, IEnumerable<IReadOnlyList<CharacterRegionContent>> contents)
    {
        IReadOnlyList<CharacterRegionContent>[] shown = [.. contents];
        return [.. combinations.SelectMany(earlier => shown.Select(content =>
        {
            var combined = new Dictionary<CharacterRange, CharacterRegionContent>(earlier);
            foreach (CharacterRegionContent region in content)
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
