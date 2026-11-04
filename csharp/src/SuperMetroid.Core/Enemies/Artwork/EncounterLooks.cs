using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The looks enemy encounters show after their rooms load, where each look lands among the sprite characters, and the
/// pictures of frames drawn over them (#1392, #1406, #1463, #1508). The import works out where each look lands and installs
/// it here; the game names pictures and judges which look hides which from this data alone.
/// </summary>
public sealed class EncounterLooks : ISpriteArtworkOverlap
{
    /// <summary>The encounter looks in catalogue order, whose place names each in picture names.</summary>
    private readonly ImmutableArray<PlacedLook> catalogue;

    /// <summary>Each catalogued look's place in <see cref="catalogue"/>; a look catalogued at several places has none.</summary>
    private readonly Dictionary<SpriteArtworkSource, int?> places = [];

    /// <summary>The places in <see cref="catalogue"/> of each enemy's encounter's looks, by the enemy whose set loads it.</summary>
    private readonly Dictionary<EnemyDefinitionId, ImmutableArray<int>> placesByEnemy = [];

    /// <summary>Where each look lands that is not a graphics-set member's: the encounter looks and the standard sprites.</summary>
    private readonly Dictionary<SpriteArtworkSource, PlacedLook> placed = [];

    /// <summary>Where a graphics-set member's tiles land, which the import alone lays out (#1513).</summary>
    private readonly Func<GraphicsSetTileCharacters, PlacedLook> memberLanding;

    /// <summary>The catalogue the import installs.</summary>
    /// <param name="encounterLooks">The encounter looks, each where it lands, in catalogue order.</param>
    /// <param name="looksByEnemy">Each enemy's encounter's looks, by the enemy whose graphics set loads the encounter.</param>
    /// <param name="standingLooks">The looks shown apart from encounters and graphics sets, each where it lands.</param>
    /// <param name="memberLanding">Where a graphics-set member's tiles land.</param>
    /// <exception cref="InvalidDataException">An enemy's look is not catalogued, or a look lands at two places.</exception>
    internal EncounterLooks(IReadOnlyList<PlacedLook> encounterLooks,
        IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkSource>> looksByEnemy,
        IReadOnlyList<PlacedLook> standingLooks, Func<GraphicsSetTileCharacters, PlacedLook> memberLanding)
    {
        catalogue = [.. Ensure.NotNull(encounterLooks)];
        for (int place = 0; place < catalogue.Length; place++)
        {
            SpriteArtworkSource look = catalogue[place].Look;
            places[look] = places.ContainsKey(look) ? null : place;
            Place(catalogue[place]);
        }
        foreach ((EnemyDefinitionId enemy, IReadOnlyList<SpriteArtworkSource> looks) in Ensure.NotNull(looksByEnemy))
            placesByEnemy[enemy] = [.. looks.Select(look => places.TryGetValue(look, out int? place) && place is int known ? known
                : throw new InvalidDataException($"{enemy}'s encounter shows {look}, which is not catalogued at one place."))];
        foreach (PlacedLook standing in Ensure.NotNull(standingLooks))
            Place(standing);
        this.memberLanding = Ensure.NotNull(memberLanding);

        void Place(PlacedLook look)
        {
            if (placed.TryGetValue(look.Look, out PlacedLook? earlier) && earlier != look)
                throw new InvalidDataException($"{look.Look} lands at two places among the sprite characters.");
            placed[look.Look] = look;
        }
    }

    /// <summary>The looks the encounters of graphics set <paramref name="set"/>'s members show, in catalogue order.</summary>
    public IReadOnlyList<SpriteArtworkSource> Of(RoomEnemyGraphicsSetDefinition set) =>
        [.. Ensure.NotNull(set).Records.ToArray().Select(record => record.DefinitionId).Distinct()
            .Where(placesByEnemy.ContainsKey).SelectMany(enemy => placesByEnemy[enemy]).Distinct().Order()
            .Select(place => catalogue[place].Look)];

    /// <summary>Whether some catalogued encounter shows <paramref name="look"/>.</summary>
    public bool IsEncounterLook(SpriteArtworkSource look) => places.ContainsKey(Ensure.NotNull(look));

    /// <summary>
    /// The name of the picture of layer <paramref name="layer"/> of <paramref name="frame"/> whose regions each show what
    /// <paramref name="shown"/> pairs with it.
    /// </summary>
    /// <exception cref="InvalidDataException">A region shows a look no encounter catalogues at one place.</exception>
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
        if (content.Look is not { } look)
            return "set";
        return places.TryGetValue(look, out int? place) && place is int known ? $"u{known}"
            : throw new InvalidDataException(places.ContainsKey(look) ? $"{look} is catalogued in several places." : $"No catalogued encounter shows {look}.");
    }

    /// <inheritdoc/>
    public bool Hides(SpriteArtworkSource later, SpriteArtworkSource earlier) => Landing(later).Covers(Landing(earlier));

    /// <summary>Where <paramref name="look"/> lands: a graphics-set member's tiles where the import lays them out, any other look where the catalogue places it.</summary>
    /// <exception cref="InvalidDataException">The catalogue places no such look.</exception>
    private PlacedLook Landing(SpriteArtworkSource look) => Ensure.NotNull(look) switch
    {
        GraphicsSetTileCharacters member => memberLanding(member),
        _ => placed.TryGetValue(look, out PlacedLook? known) ? known
            : throw new InvalidDataException($"The installed catalogue places no {look} among the sprite characters."),
    };
}
