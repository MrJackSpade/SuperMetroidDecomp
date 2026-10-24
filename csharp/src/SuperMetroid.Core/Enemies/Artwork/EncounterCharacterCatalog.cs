using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The uploads each encounter makes over the OBJ characters after its room loads, the regions a graphics set's uploads
/// divide the characters into, and the picture of a frame drawn over them that shows (#1392). The encounters' own module
/// supplies the uploads; the installed sheets carry the catalogue to the enemies drawing from them.
/// </summary>
public sealed class EncounterCharacterCatalog
{
    /// <summary>Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    private readonly IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteCharacterPlacement>> byEnemy;

    /// <summary>Every catalogued upload, once, in a fixed order whose index names it in picture names.</summary>
    private readonly ImmutableArray<SpriteCharacterPlacement> all;

    /// <summary>Catalogues each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    public EncounterCharacterCatalog(IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteCharacterPlacement>> byEnemy)
    {
        this.byEnemy = Ensure.NotNull(byEnemy);
        all = [.. byEnemy.OrderBy(entry => entry.Key).SelectMany(entry => entry.Value).Distinct()];
    }

    /// <summary>The catalogued placement that shows <paramref name="source"/>.</summary>
    /// <exception cref="InvalidDataException">No catalogued encounter shows the source, or several place it differently.</exception>
    public SpriteCharacterPlacement Placement(ObjCharacterSource source)
    {
        Ensure.NotNull(source);
        SpriteCharacterPlacement[] matches = [.. all.Where(upload => upload.Source == source)];
        return matches.Length == 1 ? matches[0]
            : throw new InvalidDataException(matches.Length == 0 ? $"No catalogued encounter shows {source}." : $"{source} is catalogued in {matches.Length} places.");
    }

    /// <summary>The uploads the encounters of graphics set <paramref name="set"/>'s members make, in catalogue order.</summary>
    public IReadOnlyList<SpriteCharacterPlacement> Of(RoomEnemyGraphicsSetDefinition set) =>
        [.. Ensure.NotNull(set).Records.Span.SelectToArray(record => record.DefinitionId).Distinct()
            .Where(byEnemy.ContainsKey).SelectMany(enemy => byEnemy[enemy]).Distinct().OrderBy(all.IndexOf)];

    /// <summary>
    /// The name of the picture of layer <paramref name="layer"/> of <paramref name="frame"/> whose regions each show what
    /// <paramref name="shown"/> pairs with it.
    /// </summary>
    public string PictureName(string frame, int layer, IReadOnlyList<CharacterRegionContent> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        Ensure.AtLeastZero(layer);
        return $"{frame}#{layer}" + string.Concat(Ensure.NotNull(shown).Select(content =>
            $"|{content.Region.Name}={Token(content)}"));
    }

    /// <summary>How a picture's name spells what <paramref name="content"/> shows over its region.</summary>
    private string Token(CharacterRegionContent content)
    {
        if (content.RotStep is int step)
            return $"rot{step}";
        return content.Placement is { } placement ? $"u{all.IndexOf(placement)}" : "set";
    }

    /// <summary>
    /// What shows over <paramref name="region"/> now: the frame an animation the set's encounters paint the region with
    /// shows in <paramref name="frames"/> (#1406); else, of the sources <paramref name="coverage"/> says cover it, the upload
    /// among <paramref name="setUploads"/> shown last, or none while the graphics set's own characters, which the room load
    /// showed, or the standard sprites show there.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// An upload the set's encounters do not make shows over its characters, or an animation shows a frame the set's
    /// encounters do not paint the region with.
    /// </exception>
    public CharacterRegionContent Showing(SpriteCharacters uploads, IReadOnlyList<SpriteCharacterPlacement> setUploads,
        CharacterRegion region, ArtworkFrameSelections frames, SetCharacterCoverage coverage)
    {
        Ensure.NotNull(uploads);
        Ensure.NotNull(setUploads);
        Ensure.NotNull(region);
        Ensure.NotNull(frames);
        Ensure.NotNull(coverage);
        SpriteCharacterPlacement[] over = [.. setUploads.Where(upload => coverage.Covers(upload.Source, region))];
        foreach (SpriteCharacterPlacement upload in over)
        {
            if (upload.Source is not ArtworkFrameCharacters painted || !frames.TryShown(painted.Frame.Animation, out ArtworkFrame shown))
                continue;
            SpriteCharacterPlacement selected = over.FirstOrDefault(candidate => candidate.Source is ArtworkFrameCharacters frame && frame.Frame == shown)
                ?? throw new InvalidDataException($"{shown} shows over region {region.Name}, which no encounter of this graphics set paints with it.");
            return new CharacterRegionContent(region, selected);
        }
        foreach ((ObjCharacterSource source, _) in uploads.NewestFirst)
        {
            if (!coverage.Knows(source) && all.Any(upload => upload.Source == source))
                throw new InvalidDataException($"{source}, uploaded by an encounter of another graphics set, shows over a frame drawn under this one.");
            if (coverage.Covers(source, region))
                return new CharacterRegionContent(region, setUploads.FirstOrDefault(upload => upload.Source == source));
        }
        return new CharacterRegionContent(region, null);
    }
}
