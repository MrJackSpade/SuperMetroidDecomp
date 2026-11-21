namespace SuperMetroid.Core.Progression;

/// <summary>
/// The game state every part of a session shares: the persistent progress a save slot carries
/// (<see cref="PersistentProgress"/>), the saved startup route, the session's shared random-number
/// generator and the earthquake rumble's power-on delay.
/// Lifetime: the session.
/// </summary>
public sealed partial class SharedGameState
{
    /// <summary>
    /// The startup route the next save keeps. Ordinary gameplay saves the main game; the Spacetime Beam can overwrite it
    /// independently of Samus's equipment and checkpoint.
    /// </summary>
    public SaveStartupRoute SavedStartupRoute { get; private set; } = SaveStartupRoute.MainGame;

    /// <summary>
    /// Creates the shared state over the session's random-number generator, for a session whose power-on adds no rumble
    /// delay and whose explored maps are its own.
    /// </summary>
    /// <param name="random">The session's random-number generator, which this state draws from and advances.</param>
    public SharedGameState(SharedRandom random)
        : this(random, PowerOnEarthquakeSoundDelay.None, StoredExploredMapLoads.PowerOn())
    {
    }

    /// <summary>Creates the shared state over the session's random-number generator, rumble delay and stored explored maps.</summary>
    /// <param name="random">The session's random-number generator, which this state draws from and advances.</param>
    /// <param name="earthquakeSoundDelay">The frame the save file found at power-on adds to every earthquake rumble's delay.</param>
    /// <param name="storedExploredMaps">The session's live and stored explored maps, which outlive each runtime.</param>
    public SharedGameState(SharedRandom random, PowerOnEarthquakeSoundDelay earthquakeSoundDelay, StoredExploredMaps<AreaId> storedExploredMaps)
    {
        Random = Ensure.NotNull(random);
        EarthquakeSoundDelay = Ensure.NotNull(earthquakeSoundDelay);
        StoredExploredMaps = Ensure.NotNull(storedExploredMaps);
    }

    /// <summary>
    /// The session's live and stored explored maps, which a camera scroll check past the room's scroll cells reads; the
    /// live map is marked with every cell this state marks explored.
    /// </summary>
    public StoredExploredMaps<AreaId> StoredExploredMaps { get; }

    /// <summary>The session's persistent progress: events, bosses, items, doors, stations, maps and exploration.</summary>
    public PersistentProgress Progress { get; } = new();

    /// <summary>The frame the save file found at power-on adds to every rising-liquid and Tourian-statue earthquake rumble's delay.</summary>
    public PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; }

    /// <summary>Restores the saved startup route from one save slot or a checkpoint.</summary>
    public void LoadSavedStartupRoute(SaveStartupRoute route) =>
        SavedStartupRoute = route;

    /// <summary>
    /// Applies the Spacetime Beam's progression overwrite (#1318, #1330, #1474, #1475): every story event, boss, item, door,
    /// station and map the overwrite decides takes the state it gives, and the saved loading state when the overwrite
    /// reaches it.
    /// </summary>
    internal void Overwrite(ProgressionOverwrite overwrite)
    {
        Ensure.NotNull(overwrite);
        Progress.Overwrite(overwrite);
        if (overwrite.SavedStartupRoute is { } route)
            LoadSavedStartupRoute(route);
    }

    /// <summary>The session's shared random-number generator.</summary>
    public SharedRandom Random { get; }

    /// <summary>The current shared RNG state as a sample, 0 through 65535, without advancing it.</summary>
    public int RandomNumber => Random.Read();

    /// <summary>Stirs the shared random number, as the lava and acid waves do
    /// (<see cref="SharedRandom.StirForLiquidWave"/>).</summary>
    internal void StirRandomForLiquidWave() => Random.StirForLiquidWave();

    /// <summary>
    /// Draws the shared random sequence's next value (<see cref="SharedRandom.Generate"/>); every later draw follows on
    /// from it.
    /// </summary>
    /// <returns>The value drawn, 0 through 65535, which <see cref="Random"/> now holds.</returns>
    public int NextRandom() => Random.Generate();

    /// <summary>Records <paramref name="bosses"/> as defeated in <paramref name="area"/>.</summary>
    public void RecordBossesDefeated(AreaId area, DefeatedBosses bosses) =>
        Progress.SetDefeatedBosses(area, Progress.BossesDefeatedIn(area) | bosses);

    /// <summary>
    /// Forgets <paramref name="bosses"/> as defeated in <paramref name="area"/>; the encounter reset on loading a save
    /// uses it.
    /// </summary>
    public void ForgetBossesDefeated(AreaId area, DefeatedBosses bosses) =>
        Progress.SetDefeatedBosses(area, Progress.BossesDefeatedIn(area) & ~bosses);

    /// <summary>Whether any of <paramref name="bosses"/> is defeated in <paramref name="area"/>.</summary>
    public bool HasDefeatedAnyOf(AreaId area, DefeatedBosses bosses) =>
        (Progress.BossesDefeatedIn(area) & bosses) != DefeatedBosses.None;

    /// <summary>The bosses defeated in <paramref name="area"/>.</summary>
    public DefeatedBosses BossesDefeatedIn(AreaId area) => Progress.BossesDefeatedIn(area);

    /// <summary>Marks <paramref name="storyEvent"/> as having happened.</summary>
    public void SetEvent(StoryEvent storyEvent) => Progress.SetEvent(storyEvent);

    /// <summary>Forgets that <paramref name="storyEvent"/> happened.</summary>
    public void ClearEvent(StoryEvent storyEvent) => Progress.ClearEvent(storyEvent);

    /// <summary>Whether <paramref name="storyEvent"/> has happened.</summary>
    public bool HasEvent(StoryEvent storyEvent) => Progress.HasEvent(storyEvent);

    /// <summary>Whether <paramref name="location"/>'s pickup has been collected, as the item room objects test it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="location"/> is not an item location.</exception>
    public bool HasCollectedItem(ItemLocation location) => Progress.CollectedItems.Contains(location);

    /// <summary>
    /// Marks one physical pickup as collected, as an item room object does when Samus collects it. This set identifies
    /// locations; it is deliberately independent from Samus's equipment, where multiple tanks of one kind share one meaning.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="location"/> is not an item location.</exception>
    public void CollectItem(ItemLocation location) => Progress.CollectedItems.Add(location);

    /// <summary>
    /// Whether <paramref name="door"/> has been opened for good. Caps whose opening never persists are handled by their
    /// room object, which never asks.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="door"/> is not one of the game's doors.</exception>
    public bool HasOpenedDoor(LockedDoor door) => Progress.OpenedDoors.Contains(door);

    /// <summary>
    /// Persists a coloured door after its final valid hit, matching the door cue. Re-entering
    /// the room then converts that coloured cap into its ordinary blue-door counterpart.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="door"/> is not one of the game's doors.</exception>
    public void OpenDoor(LockedDoor door) => Progress.OpenedDoors.Add(door);

    /// <summary>Marks a cell of <paramref name="area"/>'s map as explored, as Samus's map-exploration update does.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell is outside the area map.</exception>
    public void MarkExploredMapTile(AreaId area, Position16<MapTiles> cell)
    {
        Progress.SetExplored(area, cell, explored: true);
        // The game marks one live map whatever area's map it holds; the stored maps receive it when it is stored.
        StoredExploredMaps.MarkLiveMapCell(cell);
    }

    /// <summary>Whether a cell of <paramref name="area"/>'s map has been visited.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell is outside the area map.</exception>
    public bool IsMapTileExplored(AreaId area, Position16<MapTiles> cell) => Progress.IsExplored(area, cell);

    /// <summary>Marks <paramref name="station"/> used.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="station"/> is not a station Samus uses.</exception>
    public void MarkSaveStationUsed(LoadStation station) => Progress.UsedStations.Add(station);

    /// <summary>Returns whether <paramref name="area"/> has downloaded its map-station data.</summary>
    public bool HasAreaMap(AreaId area) => Progress.HasAreaMap(area);

    /// <summary>Marks <paramref name="area"/>'s map as acquired.</summary>
    public void SetAreaMapAcquired(AreaId area) => Progress.SetAreaMap(area, acquired: true);
}
