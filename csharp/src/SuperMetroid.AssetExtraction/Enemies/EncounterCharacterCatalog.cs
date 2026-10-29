using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The uploads each encounter makes over the OBJ characters after its room loads, and the picture of a frame drawn over
/// them that shows (#1392, #1463). Core names the looks; this catalogue knows where each lands, which looks replace which,
/// and how a picture's name spells what each region shows.
/// </summary>
/// <param name="byEnemy">Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</param>
internal sealed class EncounterCharacterCatalog(IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> byEnemy)
    : IEncounterLookCatalog
{
    /// <summary>Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    private readonly IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> uploadsByEnemy = Ensure.NotNull(byEnemy);

    /// <summary>Every catalogued upload, once, in enemy order, whose index names it in picture names.</summary>
    private readonly ImmutableArray<SpriteArtworkPlacement> all = [.. byEnemy.OrderBy(entry => entry.Key).SelectMany(entry => entry.Value).Distinct()];

    /// <summary>The catalogued placement that shows <paramref name="source"/>.</summary>
    /// <exception cref="InvalidDataException">No catalogued encounter shows the source, or several place it differently.</exception>
    internal SpriteArtworkPlacement Placement(SpriteArtworkSource source)
    {
        Ensure.NotNull(source);
        SpriteArtworkPlacement[] matches = [.. all.Where(upload => upload.Source == source)];
        return matches.Length == 1 ? matches[0]
            : throw new InvalidDataException(matches.Length == 0 ? $"No catalogued encounter shows {source}." : $"{source} is catalogued in {matches.Length} places.");
    }

    /// <summary>The catalogued look whose name is <paramref name="name"/>, as an appearance input spells it (#1466).</summary>
    /// <exception cref="InvalidDataException">No catalogued encounter shows a look of that name, or several do.</exception>
    internal SpriteArtworkSource LookNamed(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SpriteArtworkSource[] matches = [.. all.Select(upload => upload.Source).Distinct().Where(source => $"{source}" == name)];
        return matches.Length == 1 ? matches[0]
            : throw new InvalidDataException(matches.Length == 0 ? $"No catalogued encounter shows a look named {name}." : $"{matches.Length} catalogued looks are named {name}.");
    }

    /// <summary>The uploads the encounters of graphics set <paramref name="set"/>'s members make, in catalogue order.</summary>
    internal IReadOnlyList<SpriteArtworkPlacement> Of(RoomEnemyGraphicsSetDefinition set) =>
        [.. Ensure.NotNull(set).Records.Span.SelectToArray(record => record.DefinitionId).Distinct()
            .Where(uploadsByEnemy.ContainsKey).SelectMany(enemy => uploadsByEnemy[enemy]).Distinct().OrderBy(all.IndexOf)];

    /// <inheritdoc/>
    public IReadOnlyList<SpriteArtworkSource> EncounterLooks(RoomEnemyGraphicsSetDefinition set) => [.. Of(set).Select(upload => upload.Source)];

    /// <inheritdoc/>
    public bool IsEncounterLook(SpriteArtworkSource look) => all.Any(upload => upload.Source == Ensure.NotNull(look));

    /// <inheritdoc/>
    public string PictureName(string frame, int layer, IReadOnlyList<LookRegionContent> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        Ensure.AtLeastZero(layer);
        return $"{frame}#{layer}" + string.Concat(Ensure.NotNull(shown).Select(content => $"|{content.Region.Name}={Token(content)}"));
    }

    /// <summary>How a picture's name spells what <paramref name="content"/> shows over its region.</summary>
    private string Token(LookRegionContent content)
    {
        if (content.RotStep is int step)
            return $"rot{step}";
        return content.Look is { } look ? $"u{all.IndexOf(Placement(look))}" : "set";
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The cartridge shows one of the looks nowhere among the sprite characters.</exception>
    public bool Hides(SpriteArtworkSource later, SpriteArtworkSource earlier)
    {
        SpriteArtworkPlacement hidden = EncounterCharacterPlacements.Placed(earlier);
        return EncounterCharacterPlacements.Placed(later).Covers(hidden.FirstCharacter, hidden.CharacterCount);
    }
}
