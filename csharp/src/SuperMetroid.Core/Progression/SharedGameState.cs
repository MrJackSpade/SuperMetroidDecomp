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
    /// Saved frontend dispatcher value, 0 through 65,535: the save field's range. Ordinary gameplay persists state five;
    /// SpaceTime Beam can overwrite this independently of Samus equipment and checkpoint, so any value a save field holds
    /// is possible. It is only copied, never counted or compared by sign.
    /// </summary>
    public int SavedLoadingGameState
    {
        get;
        private set => field = SaveFieldValue.IsInRange(value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "The saved loading game state is a save field.");
    } = SaveLoadingGameStateCodec.Word(SaveLoadingGameState.MainGame);

    /// <summary>Creates the shared state over its own random-number generator, with the game's power-on seed.</summary>
    public SharedGameState()
        : this(new SharedRandom())
    {
    }

    /// <summary>Creates the shared state over the session's random-number generator, for a session whose power-on adds no rumble delay.</summary>
    /// <param name="random">The session's random-number word, which this state reads and advances.</param>
    public SharedGameState(SharedRandom random)
        : this(random, PowerOnEarthquakeSoundDelay.None)
    {
    }

    /// <summary>Creates the shared state over the session's random-number generator and its power-on rumble delay.</summary>
    /// <param name="random">The session's random-number word, which this state reads and advances.</param>
    /// <param name="earthquakeSoundDelay">The frame the save file found at power-on adds to every earthquake rumble's delay.</param>
    public SharedGameState(SharedRandom random, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        Random = Ensure.NotNull(random);
        EarthquakeSoundDelay = Ensure.NotNull(earthquakeSoundDelay);
    }

    /// <summary>The session's persistent progress: events, bosses, orbs, items, doors, stations, maps and exploration.</summary>
    public PersistentProgress Progress { get; } = new();

    /// <summary>The frame the save file found at power-on adds to every rising-liquid and Tourian-statue earthquake rumble's delay.</summary>
    public PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; }

    /// <summary>Restores the saved loading game state from one checksummed save slot or a checkpoint.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside a save field's range.</exception>
    public void LoadSavedLoadingGameState(int value) => SavedLoadingGameState = value;

    /// <summary>
    /// Cumulative item PLM initializations, including repeat visits, 0 through 65,535. The count is saved as one save
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
    /// Applies SpaceTime's progression overwrite (#1318, #1330, #1474, #1475): every story event, boss, orb, item, door,
    /// station and map the overwrite decides takes the state it gives, and the saved loading state when the overwrite
    /// reaches it.
    /// </summary>
    /// <exception cref="ArgumentException">The overwrite decides more flags than a range numbers.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The loading state is not a save field.</exception>
    internal void Overwrite(ProgressionOverwrite overwrite)
    {
        Ensure.NotNull(overwrite);
        Progress.Overwrite(overwrite);
        if (overwrite.SavedLoadingGameState is { } loadingState)
            SavedLoadingGameState = loadingState;
    }

    /// <summary>The session's shared random-number generator.</summary>
    public SharedRandom Random { get; }

    /// <summary>The current shared RNG state as a sample, 0 through 65535, without advancing it.</summary>
    public int RandomNumber => Random.Read();

    /// <summary>
    /// Performs a direct write to the shared RNG word at WRAM <c>$05E5</c>. Most callers
    /// should use <see cref="NextRandom"/>; this seam exists because a small number of
    /// cartridge actors deliberately reseed the global generator as part of their AI.
    /// </summary>
    /// <remarks>
    /// This is not a host-only testing shortcut. Sbug activation functions $A3:A315 and
    /// $A3:A325 both execute <c>STA $05E5</c> with <c>$000B</c>, so later unrelated random
    /// consumers must observe the replacement seed as well.
    /// </remarks>
    public void SetRandomNumber(int value) => Random.Seed(value);

    /// <summary>Stores the shared RNG state with its two bytes exchanged, as the lava and acid FX routine does.</summary>
    internal void SwapRandomBytes() => Random.SwapBytes();

    /// <summary>
    /// Most recently calculated held input, corresponding to WRAM <c>$05D9</c>.
    /// Despite the C decompilation's name <c>joypad_released_keys</c>, the assembly names
    /// and behavior show that this is the previous value of "held but not newly pressed."
    /// </summary>
    public ControllerButton HeldInputPrevious { get; private set; }

    /// <summary>
    /// Countdown at WRAM <c>$05DB</c>. A signed underflow from zero to <c>$FFFF</c>
    /// causes the held input to become active.
    /// </summary>
    public WrappingCountdown TimedHeldInputTimer { get; internal set; }

    /// <summary>
    /// Reload value at WRAM <c>$05DD</c>, 0 through 65,535: any value the held-input countdown can start at. A value of
    /// N requires N + 1 stable update calls after the held-input value changes. The value is only ever reloaded into
    /// <see cref="TimedHeldInputTimer"/>, never counted itself; that countdown keeps the wrap.
    /// </summary>
    public int TimedHeldInputTimerReset { get; internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>
    /// Filtered input at WRAM <c>$05DF</c>. Bits appear after their buttons have remained
    /// stable long enough for the timer to underflow.
    /// </summary>
    public ControllerButton TimedHeldInput { get; internal set; }

    /// <summary>
    /// Rising-edge bits for <see cref="TimedHeldInput"/>, stored at WRAM <c>$05E1</c>.
    /// </summary>
    public ControllerButton NewlyTimedHeldInput { get; internal set; }

    /// <summary>
    /// Prior active filtered input at WRAM <c>$05E3</c>. The original routine updates
    /// this only on timer underflow, not on every frame.
    /// </summary>
    public ControllerButton TimedHeldInputPrevious { get; internal set; }

    /// <summary>
    /// Advances the shared RNG word at <c>$7E:05E5</c> exactly as <c>$80:8111</c>
    /// does, preserving the two byte-multiply results, high-byte addition carry,
    /// and final 16-bit wrapping addition. Every caller observes the updated seed.
    /// </summary>
    /// <returns>The new RNG state as a sample, 0 through 65535, also stored in <see cref="Random"/>.</returns>
    public int NextRandom() => Random.Generate();

    /// <summary>
    /// Ports <c>$80:8146</c>, which turns stable held buttons into delayed input suitable
    /// for repeated cursor movement on the pause map and item screens.
    /// </summary>
    /// <param name="timerReset">Number of stable frames minus one before activation.</param>
    /// <param name="controllerInput">All buttons currently down (WRAM <c>$8B</c>).</param>
    /// <param name="controllerNewInput">Buttons newly pressed this frame (WRAM <c>$8F</c>).</param>
    public void UpdateHeldInput(int timerReset, ControllerButton controllerInput, ControllerButton controllerNewInput)
    {
        TimedHeldInputTimerReset = timerReset;

        // TRB in the original clears every newly-pressed bit from the current input.
        // A fresh press is deliberately not considered "held" until the following frame.
        ControllerButton heldInput = controllerInput & ~controllerNewInput;

        // CMP happens before STA, so remember the comparison result before replacing the
        // previous sample. This ordering is significant when a button changes state.
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
            // DEC is an unsigned 16-bit wrap, but BPL interprets bit 15 as the sign bit.
            // Therefore zero decrements to $FFFF and takes the activation path.
            CountdownStep timer = TimedHeldInputTimer.Decrement();
            TimedHeldInputTimer = timer.Remaining;

            if (timer.IsNonNegative)
            {
                TimedHeldInput = 0;
            }
            else
            {
                // The ROM pins the expired timer to zero. Continued stable input therefore
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

    /// <summary>Records <paramref name="bits"/> as defeated in <paramref name="area"/>, equivalent to <c>$80:81A6</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flags are not all named.</exception>
    public void SetBossBits(AreaId area, BossBits bits)
    {
        ProvenBossBits.Validate(bits, "Boss-state set mask");
        Progress.SetDefeatedBosses(area, Progress.DefeatedBosses(area) | bits);
    }

    /// <summary>
    /// Forgets <paramref name="bits"/> as defeated in <paramref name="area"/>, equivalent to the unused original routine at
    /// <c>$80:81C0</c>; the tester's encounter reset uses it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The flags are not all named.</exception>
    public void ClearBossBits(AreaId area, BossBits bits)
    {
        ProvenBossBits.Validate(bits, "Boss-state clear mask");
        Progress.SetDefeatedBosses(area, Progress.DefeatedBosses(area) & ~bits);
    }

    /// <summary>Whether any of <paramref name="bits"/> is defeated in <paramref name="area"/>, matching <c>$80:81DC</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The flags are not all named.</exception>
    public bool HasAnyBossBits(AreaId area, BossBits bits)
    {
        ProvenBossBits.Validate(bits, "Boss-state query mask");
        return (Progress.DefeatedBosses(area) & bits) != BossBits.None;
    }

    /// <summary>The bosses defeated in <paramref name="area"/>.</summary>
    public BossBits GetBossBits(AreaId area) => Progress.DefeatedBosses(area);

    /// <summary>Marks an event as having occurred, matching <c>$80:81FA</c>.</summary>
    public void SetEvent(EventNumber eventNumber) => Progress.SetEvent(eventNumber);

    /// <summary>Clears an event, matching <c>$80:8212</c>.</summary>
    public void ClearEvent(EventNumber eventNumber) => Progress.ClearEvent(eventNumber);

    /// <summary>Tests an event, matching <c>$80:8233</c>.</summary>
    public bool HasEvent(EventNumber eventNumber) => Progress.HasEvent(eventNumber);

    /// <summary>
    /// Whether the item location a nonnegative room-PLM argument numbers has been collected, matching the shared
    /// <c>PrepareBitAccess</c> path used by instruction <c>$84:887C</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside the room-data range.</exception>
    public bool HasCollectedItemBit(int bitIndex) => Progress.CollectedItems.Contains(bitIndex);

    /// <summary>
    /// Marks one physical pickup as collected, matching PLM instruction <c>$84:8899</c>. This set identifies locations;
    /// it is deliberately independent from Samus's equipment, where multiple tanks of one kind share one meaning.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside the room-data range.</exception>
    public void SetCollectedItemBit(int bitIndex) => Progress.CollectedItems.Add(bitIndex);

    /// <summary>
    /// Whether the door a colored-door PLM argument numbers has been opened for good. Negative room arguments are
    /// handled by the PLM owner, because the room data uses them for caps that never persist.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside the room-data range.</exception>
    public bool HasOpenedDoorBit(int bitIndex) => Progress.OpenedDoors.Contains(bitIndex);

    /// <summary>
    /// Persists a colored door after its final valid hit, matching the door instruction at <c>$84:8A91</c>. Re-entering
    /// the room then converts that colored cap into its ordinary blue-door counterpart.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside the room-data range.</exception>
    public void SetOpenedDoorBit(int bitIndex) => Progress.OpenedDoors.Add(bitIndex);

    /// <summary>
    /// The doors opened for good among the <paramref name="count"/> doors numbered from <paramref name="firstDoor"/>,
    /// ascending; numbers past the room-data range are never open.
    /// </summary>
    internal IReadOnlyList<int> OpenedDoorsFrom(int firstDoor, int count) =>
        [.. Enumerable.Range(Ensure.AtLeastZero(firstDoor), Ensure.AtLeastZero(count))
            .Where(door => door < Progress.OpenedDoors.Capacity && Progress.OpenedDoors.Contains(door))];

    /// <summary>Marks a cell of <paramref name="area"/>'s map as explored, matching <c>$90:A8A6/$90:A91B</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell is outside the area map.</exception>
    public void MarkExploredMapTile(AreaId area, Position16<MapTiles> cell) => Progress.SetExplored(area, cell, explored: true);

    /// <summary>Whether a cell of <paramref name="area"/>'s map has been visited.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cell is outside the area map.</exception>
    public bool IsMapTileExplored(AreaId area, Position16<MapTiles> cell) => Progress.IsExplored(area, cell);

    /// <summary>Marks save station <paramref name="stationBitIndex"/> of <paramref name="area"/> used.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The station number is outside the area's stations.</exception>
    public void MarkSaveStationUsed(AreaId area, int stationBitIndex) => Progress.UsedStations(area).Add(stationBitIndex);

    /// <summary>Returns whether <paramref name="area"/> has downloaded its map-station data.</summary>
    public bool HasAreaMap(AreaId area) => Progress.HasAreaMap(area);

    /// <summary>Marks <paramref name="area"/>'s map as acquired.</summary>
    public void SetAreaMapAcquired(AreaId area) => Progress.SetAreaMap(area, acquired: true);
}
