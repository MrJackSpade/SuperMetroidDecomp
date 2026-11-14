namespace SuperMetroid.Core.Progression;

/// <summary>
/// The progress a save keeps (#1475): the story events that have happened, how many Zebetites have been destroyed, each
/// area's defeated bosses, the item locations collected, the doors opened for good, the
/// save stations and elevators used, the area maps acquired and each area's explored map cells. Each set is keyed by its
/// own identity, and the Zebetite count is a count (#1527). Lifetime: the
/// session's <see cref="SharedGameState"/> owns the live progress; every save slot and save snapshot owns a detached copy.
/// </summary>
public sealed class PersistentProgress
{
    /// <summary>The Zebetites barring the way to Mother Brain, destroyed one after another.</summary>
    public const int ZebetiteCount = 4;

    /// <summary>Each area's defeated bosses.</summary>
    private readonly Dictionary<AreaId, DefeatedBosses> _defeatedBosses = AreaIds.All.ToDictionary(area => area, _ => DefeatedBosses.None);

    /// <summary>Whether each area's map has been acquired.</summary>
    private readonly Dictionary<AreaId, bool> _acquiredMaps = AreaIds.All.ToDictionary(area => area, _ => false);

    /// <summary>Each area's explored map cells.</summary>
    private readonly Dictionary<AreaId, ExploredMapCells> _explored = AreaIds.All.ToDictionary(area => area, _ => new ExploredMapCells());

    /// <summary>Every story event, in the order the game lists them.</summary>
    private static readonly StoryEvent[] StoryEvents = Enum.GetValues<StoryEvent>();

    /// <summary>The story events that have happened.</summary>
    public ProgressSet<StoryEvent> Events { get; } = new(StoryEvents);

    /// <summary>
    /// How many Zebetites Samus has destroyed, 0 through <see cref="ZebetiteCount"/>. The Zebetites read it when they
    /// spawn, to place the next one standing or to stay away once all are destroyed, and each one's destruction advances it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative or more than <see cref="ZebetiteCount"/>.</exception>
    public int ZebetitesDestroyed
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ZebetiteCount, value);
    }

    /// <summary>The item locations whose pickup has been collected.</summary>
    public ProgressSet<ItemLocation> CollectedItems { get; } = new(ItemLocationDefinitions.All);

    /// <summary>The doors opened for good.</summary>
    public ProgressSet<LockedDoor> OpenedDoors { get; } = new(LockedDoorDefinitions.All);

    /// <summary>The save stations and elevators Samus has used, which label the saved-game map's areas.</summary>
    public ProgressSet<LoadStation> UsedStations { get; } = new(LoadStationDefinitions.Usable);

    /// <summary>Whether <paramref name="storyEvent"/> has happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="storyEvent"/> is not a defined story event.</exception>
    public bool HasEvent(StoryEvent storyEvent) => Events.Contains(storyEvent);

    /// <summary>Records that <paramref name="storyEvent"/> has happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="storyEvent"/> is not a defined story event.</exception>
    public void SetEvent(StoryEvent storyEvent) => Events.Add(storyEvent);

    /// <summary>Records that <paramref name="storyEvent"/> has not happened.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="storyEvent"/> is not a defined story event.</exception>
    public void ClearEvent(StoryEvent storyEvent) => Events.Remove(storyEvent);

    /// <summary>The bosses defeated in <paramref name="area"/>.</summary>
    public DefeatedBosses BossesDefeatedIn(AreaId area) => _defeatedBosses[AreaIds.Require(area)];

    /// <summary>Replaces the bosses defeated in <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bosses"/> names a boss the game does not name.</exception>
    public void SetDefeatedBosses(AreaId area, DefeatedBosses bosses)
    {
        ProvenDefeatedBosses.Validate(bosses, $"{area}'s defeated bosses");
        _defeatedBosses[AreaIds.Require(area)] = bosses;
    }

    /// <summary>Whether <paramref name="area"/>'s map has been acquired.</summary>
    public bool HasAreaMap(AreaId area) => _acquiredMaps[AreaIds.Require(area)];

    /// <summary>Records whether <paramref name="area"/>'s map has been acquired.</summary>
    /// <param name="area">The area.</param>
    /// <param name="acquired">Whether its map has been acquired.</param>
    public void SetAreaMap(AreaId area, bool acquired) => _acquiredMaps[AreaIds.Require(area)] = acquired;

    /// <summary>Whether <paramref name="cell"/> of <paramref name="area"/>'s map has been explored.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the area map.</exception>
    public bool IsExplored(AreaId area, Position16<MapTiles> cell) => _explored[AreaIds.Require(area)].Contains(cell);

    /// <summary>Records whether <paramref name="cell"/> of <paramref name="area"/>'s map has been explored.</summary>
    /// <param name="area">The area.</param>
    /// <param name="cell">The map cell.</param>
    /// <param name="explored">Whether the cell has been explored.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the area map.</exception>
    public void SetExplored(AreaId area, Position16<MapTiles> cell, bool explored) =>
        _explored[AreaIds.Require(area)].Set(cell, explored);

    /// <summary>The explored cells of <paramref name="area"/>'s map, row by row from the top, each row from the left.</summary>
    public IEnumerable<Position16<MapTiles>> ExploredCells(AreaId area) => _explored[AreaIds.Require(area)].Cells;

    /// <summary>
    /// Applies the Spacetime Beam's progression overwrite: every story event, boss, item, door, station and map it decides,
    /// and the Zebetite count when it decides it, takes the state it gives, and everything else keeps its state.
    /// </summary>
    public void Overwrite(ProgressionOverwrite overwrite)
    {
        Ensure.NotNull(overwrite);
        Events.Overwrite(overwrite.Events);
        if (overwrite.ZebetitesDestroyed is int zebetitesDestroyed)
            ZebetitesDestroyed = zebetitesDestroyed;
        foreach (KeyValuePair<AreaId, DefeatedBosses> bosses in overwrite.AreaBosses)
            SetDefeatedBosses(bosses.Key, bosses.Value);
        CollectedItems.Overwrite(overwrite.CollectedItems);
        OpenedDoors.Overwrite(overwrite.OpenedDoors);
        UsedStations.Overwrite(overwrite.UsedStations);
        foreach (KeyValuePair<AreaId, bool> map in overwrite.AreaMaps)
            SetAreaMap(map.Key, map.Value);
    }

    /// <summary>Makes this progress match <paramref name="source"/>.</summary>
    public void CopyFrom(PersistentProgress source)
    {
        Ensure.NotNull(source);
        Events.CopyFrom(source.Events);
        ZebetitesDestroyed = source.ZebetitesDestroyed;
        CollectedItems.CopyFrom(source.CollectedItems);
        OpenedDoors.CopyFrom(source.OpenedDoors);
        UsedStations.CopyFrom(source.UsedStations);
        foreach (AreaId area in AreaIds.All)
        {
            _defeatedBosses[area] = source._defeatedBosses[area];
            _explored[area].CopyFrom(source._explored[area]);
            _acquiredMaps[area] = source._acquiredMaps[area];
        }
    }

    /// <summary>A detached copy of this progress.</summary>
    public PersistentProgress Copy()
    {
        var copy = new PersistentProgress();
        copy.CopyFrom(this);
        return copy;
    }
}
