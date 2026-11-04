using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The uploads each encounter makes over the OBJ characters after its room loads (#1392, #1463). The import knows where
/// each lands; it installs that knowledge as Core's <see cref="EncounterLooks"/>, which the game names pictures and
/// judges overlap from without running import code (#1508).
/// </summary>
/// <param name="byEnemy">Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</param>
/// <param name="standing">Where the looks shown apart from encounters and graphics sets land: the standard sprites.</param>
internal sealed class EncounterCharacterCatalog(IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> byEnemy,
    IReadOnlyList<SpriteArtworkPlacement> standing)
{
    /// <summary>Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    private readonly IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> uploadsByEnemy = Ensure.NotNull(byEnemy);

    /// <summary>Every catalogued upload, once, in enemy order, whose index names it in picture names.</summary>
    private readonly ImmutableArray<SpriteArtworkPlacement> all = [.. byEnemy.OrderBy(entry => entry.Key).SelectMany(entry => entry.Value).Distinct()];

    /// <summary>Every catalogued upload, once, in catalogue order: what the encounters of every graphics set show.</summary>
    internal IReadOnlyList<SpriteArtworkPlacement> All => all;

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

    /// <summary>
    /// The catalogue as Core installs it: every encounter look where it lands in catalogue order, each enemy's encounter's
    /// looks, the standing looks and where each graphics-set member's tiles land.
    /// </summary>
    internal EncounterLooks Looks => looks.Value;

    /// <summary>The installed catalogue, built once.</summary>
    private readonly Lazy<EncounterLooks> looks = new(() => new EncounterLooks(
        [.. byEnemy.OrderBy(entry => entry.Key).SelectMany(entry => entry.Value).Distinct().Select(Installed)],
        byEnemy.ToDictionary(entry => entry.Key, entry => (IReadOnlyList<SpriteArtworkSource>)[.. entry.Value.Select(upload => upload.Source)]),
        [.. standing.Select(Installed)], member => Installed(EncounterCharacterPlacements.Placed(member))));

    /// <summary><paramref name="placement"/> as Core holds it.</summary>
    private static PlacedLook Installed(SpriteArtworkPlacement placement) =>
        new(placement.Source, placement.FirstCharacter, placement.CharacterCount);
}
