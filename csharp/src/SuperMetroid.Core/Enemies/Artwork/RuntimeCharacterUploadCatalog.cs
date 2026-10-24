using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The uploads each encounter makes over the OBJ characters after its room loads, the regions a graphics set's uploads
/// divide the characters into, and the picture of a frame drawn over them that shows (#1392). The encounters' own module
/// supplies the uploads; the installed sheets carry the catalogue to the enemies drawing from them.
/// </summary>
public sealed class RuntimeCharacterUploadCatalog
{
    /// <summary>Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    private readonly IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<RuntimeCharacterUpload>> byEnemy;

    /// <summary>Every catalogued upload, once, in a fixed order whose index names it in picture names.</summary>
    private readonly ImmutableArray<RuntimeCharacterUpload> all;

    /// <summary>Catalogues each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    public RuntimeCharacterUploadCatalog(IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<RuntimeCharacterUpload>> byEnemy)
    {
        this.byEnemy = Ensure.NotNull(byEnemy);
        all = [.. byEnemy.OrderBy(entry => entry.Key).SelectMany(entry => entry.Value).Distinct()];
    }

    /// <summary>The catalogued upload that shows <paramref name="source"/>.</summary>
    /// <exception cref="InvalidDataException">No catalogued encounter shows the source, or several place it differently.</exception>
    public RuntimeCharacterUpload Upload(ObjCharacterSource source)
    {
        Ensure.NotNull(source);
        RuntimeCharacterUpload[] matches = [.. all.Where(upload => upload.Source == source)];
        return matches.Length == 1 ? matches[0]
            : throw new InvalidDataException(matches.Length == 0 ? $"No catalogued encounter shows {source}." : $"{source} is catalogued in {matches.Length} places.");
    }

    /// <summary>The uploads the encounters of graphics set <paramref name="set"/>'s members make, in catalogue order.</summary>
    public IReadOnlyList<RuntimeCharacterUpload> Of(RoomEnemyGraphicsSetDefinition set) =>
        [.. Ensure.NotNull(set).Records.Span.SelectToArray(record => record.DefinitionId).Distinct()
            .Where(byEnemy.ContainsKey).SelectMany(enemy => byEnemy[enemy]).Distinct().OrderBy(all.IndexOf)];

    /// <summary>The uploads among <paramref name="uploads"/> that show over <paramref name="region"/> when made.</summary>
    public static IEnumerable<RuntimeCharacterUpload> Over(IEnumerable<RuntimeCharacterUpload> uploads, CharacterRegion region)
    {
        Ensure.NotNull(region);
        return Ensure.NotNull(uploads).Where(upload =>
            upload.FirstByte <= region.FirstByte && region.FirstByte + region.ByteCount <= upload.FirstByte + upload.ByteCount);
    }

    /// <summary>
    /// Every combination of what <paramref name="regions"/> can show, in a fixed order: in each region, the graphics set's
    /// own characters or each of <paramref name="uploads"/> that shows over it.
    /// </summary>
    public static IEnumerable<IReadOnlyList<CharacterRegionContent>> Contents(IReadOnlyList<CharacterRegion> regions,
        IReadOnlyList<RuntimeCharacterUpload> uploads)
    {
        Ensure.NotNull(uploads);
        IEnumerable<IReadOnlyList<CharacterRegionContent>> combinations = [[]];
        foreach (CharacterRegion region in Ensure.NotNull(regions))
        {
            CharacterRegionContent[] contents = [new(region, null), .. Over(uploads, region).Select(upload => new CharacterRegionContent(region, upload))];
            combinations = [.. combinations.SelectMany(earlier => contents.Select(content => (IReadOnlyList<CharacterRegionContent>)[.. earlier, content]))];
        }
        return combinations;
    }

    /// <summary>
    /// The name of the picture of layer <paramref name="layer"/> of <paramref name="frame"/> whose regions each show what
    /// <paramref name="shown"/> pairs with it.
    /// </summary>
    public string PictureName(string frame, int layer, IReadOnlyList<CharacterRegionContent> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        Ensure.AtLeastZero(layer);
        return $"{frame}#{layer}" + string.Concat(Ensure.NotNull(shown).Select(content =>
            $"|{content.Region.FirstByte:X4}={Token(content)}"));
    }

    /// <summary>How a picture's name spells what <paramref name="content"/> shows over its region.</summary>
    private string Token(CharacterRegionContent content)
    {
        if (content.RotStep is int step)
            return $"rot{step}";
        return content.Upload is { } upload ? $"u{all.IndexOf(upload)}" : "set";
    }

    /// <summary>
    /// What shows over <paramref name="region"/> now: the frame an animation the set's encounters paint the region with
    /// shows in <paramref name="frames"/> (#1406); else the upload among <paramref name="setUploads"/> made last over it, or
    /// none while the graphics set's own characters, which the room load uploaded, show there.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// An upload the set's encounters do not make shows over the region, or an animation shows a frame the set's encounters
    /// do not paint the region with.
    /// </exception>
    public CharacterRegionContent Showing(ObjCharacterUploads uploads, IReadOnlyList<RuntimeCharacterUpload> setUploads,
        CharacterRegion region, ArtworkFrameSelections frames)
    {
        Ensure.NotNull(uploads);
        Ensure.NotNull(setUploads);
        Ensure.NotNull(region);
        Ensure.NotNull(frames);
        RuntimeCharacterUpload[] over = [.. Over(setUploads, region)];
        foreach (RuntimeCharacterUpload upload in over)
        {
            if (upload.Source is not ArtworkFrameCharacters painted || !frames.TryShown(painted.Frame.Animation, out ArtworkFrame shown))
                continue;
            RuntimeCharacterUpload selected = over.FirstOrDefault(candidate => candidate.Source is ArtworkFrameCharacters frame && frame.Frame == shown)
                ?? throw new InvalidDataException($"{shown} shows over table byte {region.FirstByte}, which no encounter of this graphics set paints with it.");
            return new CharacterRegionContent(region, selected);
        }
        if (uploads.LastOver(region.FirstByte, region.ByteCount) is not { } last)
            return new CharacterRegionContent(region, null);
        RuntimeCharacterUpload? made = setUploads.FirstOrDefault(upload =>
            upload.Source == last.Source && upload.FirstByte == last.FirstByte && upload.ByteCount == last.ByteCount);
        if (made is null && all.Any(upload => upload.Source == last.Source && upload.FirstByte == last.FirstByte))
            throw new InvalidDataException(
                $"{last.Source}, uploaded at table byte {last.FirstByte} by an encounter of another graphics set, shows over a frame drawn under this one.");
        return new CharacterRegionContent(region, made);
    }
}
