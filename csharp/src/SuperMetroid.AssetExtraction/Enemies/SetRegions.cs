using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The regions of a graphics set's OBJ characters that change after its room loads, and what each can show (#1392): those
/// its encounters upload over, each showing an upload or the set's own characters, and the rows its rotting corpses upload,
/// showing the rot steps each corpse's uploads can leave them in. The sheet composition pictures frames from them, and the
/// installation checks the pictures a sheet holds against them.
/// </summary>
internal sealed class SetRegions
{
    /// <summary>Each row a rotting corpse uploads, by its region, with the corpse.</summary>
    private readonly Dictionary<CharacterRegion, CorpseRegion> corpseRows;

    /// <summary>The uploads the set's encounters make and the corpses that rot in its rooms.</summary>
    /// <exception cref="InvalidDataException">A corpse uploads a row over characters an encounter or another corpse uploads.</exception>
    internal SetRegions(IReadOnlyList<RuntimeCharacterUpload> uploads, IReadOnlyList<CorpseRotSimulation> corpses)
    {
        Uploads = Ensure.NotNull(uploads);
        IReadOnlyList<CharacterRegion> uploaded = RuntimeCharacterUploadCatalog.Regions(uploads);
        corpseRows = Ensure.NotNull(corpses).SelectMany(corpse => corpse.Rows.Select(row => new CorpseRegion(corpse, row)))
            .ToDictionary(row => new CharacterRegion(row.Row.FirstByte, row.Row.ByteCount));
        foreach (CharacterRegion row in corpseRows.Keys)
        {
            if (uploaded.Concat(corpseRows.Keys.Where(other => other != row)).Any(region => Overlaps(region, row)))
                throw new InvalidDataException($"{corpseRows[row].Corpse.Flesh} uploads a row at table byte {row.FirstByte} over characters another upload shows.");
        }
        Regions = [.. uploaded.Concat(corpseRows.Keys).OrderBy(region => region.FirstByte)];
    }

    /// <summary>The uploads the set's encounters make.</summary>
    internal IReadOnlyList<RuntimeCharacterUpload> Uploads { get; }

    /// <summary>The regions the uploads and the corpses' rows divide the characters into.</summary>
    internal IReadOnlyList<CharacterRegion> Regions { get; }

    /// <summary>
    /// What <paramref name="regions"/> can show together: each upload or the set's own characters in each region an
    /// encounter uploads, by the cartesian product, and each state each corpse's rows can show together, as a corpse
    /// uploads its rows together and rots apart from the others. Only a corpse's own frames show its rot, so unless
    /// <paramref name="showsRot"/> a corpse's rows show only what the room load left there.
    /// </summary>
    internal IEnumerable<IReadOnlyList<CharacterRegionContent>> Contents(IReadOnlyList<CharacterRegion> regions, bool showsRot)
    {
        Ensure.NotNull(regions);
        IEnumerable<Dictionary<CharacterRegion, CharacterRegionContent>> combinations = [[]];
        foreach (CharacterRegion encounter in regions.Where(region => !corpseRows.ContainsKey(region)))
        {
            IReadOnlyList<CharacterRegionContent>[] shown = [.. RuntimeCharacterUploadCatalog.Contents([encounter], Uploads)];
            combinations = [.. combinations.SelectMany(earlier => shown.Select(content => With(earlier, content)))];
        }
        foreach (IGrouping<CorpseRotSimulation, CharacterRegion> corpse in regions.Where(corpseRows.ContainsKey)
                     .GroupBy(region => corpseRows[region].Corpse))
        {
            CharacterRegion[] rows = [.. corpse];
            IReadOnlyList<CharacterRegionContent>[] shown = showsRot
                ? [.. corpse.Key.Shown([.. rows.Select(region => corpseRows[region].Row)])
                    .Select(steps => (IReadOnlyList<CharacterRegionContent>)[.. rows.Select((region, index) => new CharacterRegionContent(region, null, steps[index]))])]
                : [[.. rows.Select(region => new CharacterRegionContent(region, null))]];
            combinations = [.. combinations.SelectMany(earlier => shown.Select(content => With(earlier, content)))];
        }
        return combinations.Select(combination => (IReadOnlyList<CharacterRegionContent>)[.. regions.Select(region => combination[region])]);
    }

    /// <summary>
    /// The names of the pictures of <paramref name="frame"/>: the frame whole once when it has no
    /// <paramref name="layers"/>, else each layer once for each content <see cref="Contents"/> gives its regions.
    /// </summary>
    internal IEnumerable<string> PictureNames(string frame, IReadOnlyList<CharacterRegion[]> layers, bool showsRot) =>
        Ensure.NotNull(layers).Count == 0 ? [frame]
            : layers.SelectMany((regions, layer) => Contents(regions, showsRot)
                .Select(contents => RuntimeCharacterUploadDefinitions.Catalog.PictureName(frame, layer, contents)));

    /// <summary>The bytes <paramref name="content"/>'s corpse row shows after its rot step.</summary>
    /// <exception cref="InvalidOperationException">The content shows no rot step.</exception>
    internal ReadOnlySpan<byte> RotBytes(CharacterRegionContent content)
    {
        Ensure.NotNull(content);
        CorpseRegion corpse = corpseRows[content.Region];
        return corpse.Corpse.RowBytes(corpse.Row, content.RotStep ?? throw new InvalidOperationException(
            $"The region at table byte {content.Region.FirstByte} shows no rot step."));
    }

    /// <summary><paramref name="earlier"/> with <paramref name="contents"/> added.</summary>
    private static Dictionary<CharacterRegion, CharacterRegionContent> With(Dictionary<CharacterRegion, CharacterRegionContent> earlier,
        IReadOnlyList<CharacterRegionContent> contents)
    {
        var combined = new Dictionary<CharacterRegion, CharacterRegionContent>(earlier);
        foreach (CharacterRegionContent content in contents)
            combined.Add(content.Region, content);
        return combined;
    }

    /// <summary>Whether two regions share a byte.</summary>
    private static bool Overlaps(CharacterRegion region, CharacterRegion other) =>
        other.FirstByte < region.FirstByte + region.ByteCount && region.FirstByte < other.FirstByte + other.ByteCount;

    /// <summary>A row a rotting corpse uploads, with the corpse.</summary>
    private sealed record CorpseRegion(CorpseRotSimulation Corpse, CorpseRotRow Row);
}
