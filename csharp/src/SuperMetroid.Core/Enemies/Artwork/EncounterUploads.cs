using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The encounters' uploads over the OBJ characters: their catalogue order, which a graphics set's encounters make, what
/// shows over a region now, and how a picture's name spells it (#1392).
/// </summary>
internal static class EncounterUploads
{
    /// <summary>Every upload in <paramref name="byEnemy"/>, once, in enemy order.</summary>
    internal static ImmutableArray<SpriteArtworkPlacement> InCatalogueOrder(
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> byEnemy) =>
        [.. byEnemy.OrderBy(entry => entry.Key).SelectMany(entry => entry.Value).Distinct()];

    /// <summary>The catalogued placement that shows <paramref name="source"/>.</summary>
    /// <exception cref="InvalidDataException">No catalogued encounter shows the source, or several place it differently.</exception>
    internal static SpriteArtworkPlacement Placement(EncounterCharacterCatalog catalog, SpriteArtworkSource source)
    {
        Ensure.NotNull(source);
        SpriteArtworkPlacement[] matches = [.. catalog.all.Where(upload => upload.Source == source)];
        return matches.Length == 1 ? matches[0]
            : throw new InvalidDataException(matches.Length == 0 ? $"No catalogued encounter shows {source}." : $"{source} is catalogued in {matches.Length} places.");
    }

    /// <summary>The uploads the encounters of graphics set <paramref name="set"/>'s members make, in catalogue order.</summary>
    internal static IReadOnlyList<SpriteArtworkPlacement> Of(EncounterCharacterCatalog catalog, RoomEnemyGraphicsSetDefinition set) =>
        [.. Ensure.NotNull(set).Records.Span.SelectToArray(record => record.DefinitionId).Distinct()
            .Where(catalog.uploadsByEnemy.ContainsKey).SelectMany(enemy => catalog.uploadsByEnemy[enemy]).Distinct().OrderBy(catalog.all.IndexOf)];

    /// <summary>
    /// The name of the picture of layer <paramref name="layer"/> of <paramref name="frame"/> whose regions each show what
    /// <paramref name="shown"/> pairs with it.
    /// </summary>
    internal static string PictureName(EncounterCharacterCatalog catalog, string frame, int layer, IReadOnlyList<CharacterRegionContent> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        Ensure.AtLeastZero(layer);
        return $"{frame}#{layer}" + string.Concat(Ensure.NotNull(shown).Select(content =>
            $"|{content.Region.Name}={Token(catalog, content)}"));
    }

    /// <summary>How a picture's name spells what <paramref name="content"/> shows over its region.</summary>
    private static string Token(EncounterCharacterCatalog catalog, CharacterRegionContent content)
    {
        if (content.RotStep is int step)
            return $"rot{step}";
        return content.Placement is { } placement ? $"u{catalog.all.IndexOf(placement)}" : "set";
    }

    /// <summary>What shows over <paramref name="region"/> now, as <see cref="EncounterCharacterCatalog.Showing"/> describes.</summary>
    /// <exception cref="InvalidDataException">
    /// An upload the set's encounters do not make shows over its characters, or an animation shows a frame the set's
    /// encounters do not paint the region with.
    /// </exception>
    internal static CharacterRegionContent Showing(EncounterCharacterCatalog catalog, SpriteArtworkPlacements uploads,
        IReadOnlyList<SpriteArtworkPlacement> setUploads, CharacterRegion region, ArtworkFrameSelections frames,
        SetCharacterCoverage coverage)
    {
        Ensure.NotNull(uploads);
        Ensure.NotNull(setUploads);
        Ensure.NotNull(region);
        Ensure.NotNull(frames);
        Ensure.NotNull(coverage);
        SpriteArtworkPlacement[] over = [.. setUploads.Where(upload => coverage.Covers(upload.Source, region))];
        foreach (SpriteArtworkPlacement upload in over)
        {
            if (upload.Source is not ArtworkFrameCharacters painted || !frames.TryShown(painted.Frame.Animation, out ArtworkFrame shown))
                continue;
            SpriteArtworkPlacement selected = over.FirstOrDefault(candidate => candidate.Source is ArtworkFrameCharacters frame && frame.Frame == shown)
                ?? throw new InvalidDataException($"{shown} shows over region {region.Name}, which no encounter of this graphics set paints with it.");
            return new CharacterRegionContent(region, selected);
        }
        foreach (SpriteArtworkSource source in uploads.NewestFirst)
        {
            if (!coverage.Knows(source) && catalog.all.Any(upload => upload.Source == source))
                throw new InvalidDataException($"{source}, uploaded by an encounter of another graphics set, shows over a frame drawn under this one.");
            if (coverage.Covers(source, region))
                return new CharacterRegionContent(region, setUploads.FirstOrDefault(upload => upload.Source == source));
        }
        return new CharacterRegionContent(region, null);
    }
}
