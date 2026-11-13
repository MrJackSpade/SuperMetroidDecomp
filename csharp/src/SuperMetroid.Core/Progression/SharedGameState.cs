using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Progression;

/// <summary>
/// The game state every part of a session shares: the persistent progress a save slot carries
/// (<see cref="PersistentProgress"/>), the saved loading state and item-load count, the session's shared random-number
/// generator, the earthquake rumble's power-on delay, and the held-input repeat that menus use for cursor movement.
/// Lifetime: the session.
/// </summary>
public sealed class SharedGameState
{
    /// <summary>
    /// The startup route the next save keeps. Ordinary gameplay saves the main game; the Spacetime Beam can overwrite it
    /// independently of Samus's equipment and checkpoint.
    /// </summary>
    public SaveLoadingGameState SavedStartupRoute { get; private set; } = SaveLoadingGameState.MainGame;

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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="route"/> is not a defined route.</exception>
    public void LoadSavedStartupRoute(SaveLoadingGameState route) =>
        SavedStartupRoute = Enum.IsDefined(route) ? route : throw new ArgumentOutOfRangeException(nameof(route), route, "Undefined startup route.");

    /// <summary>
    /// Cumulative item room-object initializations, including repeat visits, 0 through 65,535. The count is saved as one save
    /// field and wraps from 65,535 to zero when an item initializes, a wrap the saved value shows; the setter rejects
    /// anything outside the field's range, so the producer that counts must apply that wrap.
    /// </summary>
    public int LoadedItemCount
    {
        get;
        set => field = SaveFieldValue.IsInRange(value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "The loaded-item count is a save field.");
    }

    /// <summary>
    /// Applies the Spacetime Beam's progression overwrite (#1318, #1330, #1474, #1475): every story event, boss, item, door,
    /// station and map the overwrite decides takes the state it gives, and the saved loading state when the overwrite
    /// reaches it.
    /// </summary>
    /// <exception cref="ArgumentException">The overwrite decides more stations than an area numbers.</exception>
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

    /// <summary>
    /// Performs a direct write to the shared RNG seed. Most callers
    /// should use <see cref="NextRandom"/>; this seam exists because a small number of
    /// actors deliberately reseed the global generator as part of their AI.
    /// </summary>
    /// <remarks>
    /// This is not a host-only testing shortcut. Both Sbug activation behaviours reseed the generator
    /// with eleven, so later unrelated random
    /// consumers must observe the replacement seed as well.
    /// </remarks>
    public void SetRandomNumber(int value) => Random.Seed(value);

    /// <summary>Stores the shared RNG state with its two halves exchanged, as the lava and acid FX does.</summary>
    internal void SwapRandomHalves() => Random.SwapHalves();

    /// <summary>
    /// Most recently calculated held input: the previous value of "held but not newly pressed."
    /// </summary>
    public ControllerButton HeldInputPrevious { get; private set; }

    /// <summary>
    /// Held-input countdown. A signed underflow from zero to 65,535
    /// causes the held input to become active.
    /// </summary>
    public WrappingCountdown TimedHeldInputTimer { get; internal set; }

    /// <summary>
    /// Held-input countdown reload value, 0 through <see cref="WrappingCountdown.LongestStart"/>: any value the held-input
    /// countdown can start at (#1627). A value of N requires N + 1 stable update calls after the held-input value changes.
    /// The value is only ever reloaded into <see cref="TimedHeldInputTimer"/>, never counted itself; that countdown keeps
    /// the wrap.
    /// </summary>
    public int TimedHeldInputTimerReset { get; internal set => field = Ensure.BetweenInclusive(0, WrappingCountdown.LongestStart, value); }

    /// <summary>
    /// Filtered input. Bits appear after their buttons have remained
    /// stable long enough for the timer to underflow.
    /// </summary>
    public ControllerButton TimedHeldInput { get; internal set; }

    /// <summary>
    /// Rising-edge bits for <see cref="TimedHeldInput"/>.
    /// </summary>
    public ControllerButton NewlyTimedHeldInput { get; internal set; }

    /// <summary>
    /// Prior active filtered input. The game updates
    /// this only on timer underflow, not on every frame.
    /// </summary>
    public ControllerButton TimedHeldInputPrevious { get; internal set; }

    /// <summary>
    /// Advances the shared RNG seed exactly as the game's generator
    /// does, preserving the two half-by-half products, the carry into the upper half,
    /// and the final addition wrapping past 65535. Every caller observes the updated seed.
    /// </summary>
    /// <returns>The new RNG state as a sample, 0 through 65535, also stored in <see cref="Random"/>.</returns>
    public int NextRandom() => Random.Generate();

    /// <summary>
    /// Turns stable held buttons into delayed input suitable
    /// for repeated cursor movement on the pause map and item screens.
    /// </summary>
    /// <param name="timerReset">Number of stable frames minus one before activation.</param>
    /// <param name="controllerInput">All buttons currently down.</param>
    /// <param name="controllerNewInput">Buttons newly pressed this frame.</param>
    public void UpdateHeldInput(int timerReset, ControllerButton controllerInput, ControllerButton controllerNewInput)
    {
        TimedHeldInputTimerReset = timerReset;

        // Every newly pressed button is cleared from the current input: a fresh press is deliberately not considered
        // "held" until the following frame.
        ControllerButton heldInput = controllerInput & ~controllerNewInput;

        // The comparison happens before the previous sample is replaced, so remember its result first. This ordering is
        // significant when a button changes state.
        bool heldInputIsUnchanged = heldInput == HeldInputPrevious;
        HeldInputPrevious = heldInput;

        if (!heldInputIsUnchanged)
        {
            // Any change, including releasing one member of a multi-button chord, restarts
            // the delay and suppresses filtered input for this update.
            TimedHeldInputTimer = WrappingCountdown.Start(TimedHeldInputTimerReset);
            TimedHeldInput = 0;
        }
        else
        {
            // The decrement wraps below zero to 65,535, and the test reads 32,768 and above as negative.
            // Therefore zero decrements to 65,535 and takes the activation path.
            CountdownStep timer = TimedHeldInputTimer.Decrement();
            TimedHeldInputTimer = timer.Remaining;

            if (timer.IsNonNegative)
            {
                TimedHeldInput = 0;
            }
            else
            {
                // The game pins the expired timer to zero. Continued stable input therefore
                // underflows again on every update, keeping TimedHeldInput active.
                TimedHeldInputTimer = WrappingCountdown.Start(0);
                TimedHeldInputPrevious = TimedHeldInput;
                TimedHeldInput = heldInput;
            }
        }

        // This is the standard rising-edge expression: current & (previous XOR current).
        // It produces a one-update pulse when a delayed button first becomes active.
        NewlyTimedHeldInput = TimedHeldInput & (TimedHeldInputPrevious ^ TimedHeldInput);
    }

    /// <summary>Records <paramref name="bits"/> as defeated in <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flags are not all named.</exception>
    public void SetBossBits(AreaId area, BossBits bits)
    {
        ProvenBossBits.Validate(bits, "Boss-state set mask");
        Progress.SetDefeatedBosses(area, Progress.DefeatedBosses(area) | bits);
    }

    /// <summary>
    /// Forgets <paramref name="bits"/> as defeated in <paramref name="area"/>, as an otherwise unused original
    /// step does; the encounter reset on loading a save uses it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The flags are not all named.</exception>
    public void ClearBossBits(AreaId area, BossBits bits)
    {
        ProvenBossBits.Validate(bits, "Boss-state clear mask");
        Progress.SetDefeatedBosses(area, Progress.DefeatedBosses(area) & ~bits);
    }

    /// <summary>Whether any of <paramref name="bits"/> is defeated in <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flags are not all named.</exception>
    public bool HasAnyBossBits(AreaId area, BossBits bits)
    {
        ProvenBossBits.Validate(bits, "Boss-state query mask");
        return (Progress.DefeatedBosses(area) & bits) != BossBits.None;
    }

    /// <summary>The bosses defeated in <paramref name="area"/>.</summary>
    public BossBits GetBossBits(AreaId area) => Progress.DefeatedBosses(area);

    /// <summary>Marks an event as having occurred.</summary>
    public void SetEvent(StoryEvent eventNumber) => Progress.SetEvent(eventNumber);

    /// <summary>Clears an event.</summary>
    public void ClearEvent(StoryEvent eventNumber) => Progress.ClearEvent(eventNumber);

    /// <summary>Tests an event.</summary>
    public bool HasEvent(StoryEvent eventNumber) => Progress.HasEvent(eventNumber);

    /// <summary>Whether <paramref name="location"/>'s pickup has been collected, as the item room objects test it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="location"/> is not a retail item location.</exception>
    public bool HasCollectedItem(ItemLocation location) => Progress.CollectedItems.Contains(location);

    /// <summary>
    /// Marks one physical pickup as collected, as an item room object does when Samus collects it. This set identifies
    /// locations; it is deliberately independent from Samus's equipment, where multiple tanks of one kind share one meaning.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="location"/> is not a retail item location.</exception>
    public void CollectItem(ItemLocation location) => Progress.CollectedItems.Add(location);

    /// <summary>
    /// Whether <paramref name="door"/> has been opened for good. Caps whose opening never persists are handled by their
    /// room object, which never asks.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="door"/> is not a retail door.</exception>
    public bool HasOpenedDoor(LockedDoor door) => Progress.OpenedDoors.Contains(door);

    /// <summary>
    /// Persists a coloured door after its final valid hit, matching the door cue. Re-entering
    /// the room then converts that coloured cap into its ordinary blue-door counterpart.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="door"/> is not a retail door.</exception>
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
