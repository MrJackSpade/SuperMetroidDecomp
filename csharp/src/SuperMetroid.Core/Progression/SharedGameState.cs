using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Progression;

/// <summary>
/// The game state every part of a session shares: the persistent progress a save slot carries (story events, boss
/// state, Chozo orbs, collected items, opened doors, explored map, used stations, acquired maps), the session's shared
/// random-number generator, and the held-input repeat that menus use for cursor movement. Lifetime: the session.
/// </summary>
/// <remarks>
/// The progress tables keep the cartridge's allocation sizes and bit layout, because save slots round-trip them exactly
/// and SpaceTime's progression overwrite (<see cref="WriteProgressionSliceByte"/>) addresses them byte by byte.
/// </remarks>
public sealed class SharedGameState
{
    /// <summary>
    /// Number of event bytes at SRAM mirror <c>$7E:D820-$7E:D827</c>.
    /// Eight bytes provide event numbers <c>$00-$3F</c>.
    /// </summary>
    public const int EventByteCount = 8;

    /// <summary>
    /// Number of area boss bytes at SRAM mirror <c>$7E:D828-$7E:D82F</c>.
    /// The room format uses area indices as byte offsets into this table.
    /// </summary>
    public const int AreaCount = 8;

    /// <summary>
    /// Number of bytes in the persistent opened-Chozo-orb table at
    /// <c>$7E:D830-$7E:D86F</c>. The same room PLM argument selects a bit here and in
    /// the item table, but the two events are deliberately independent: an orb can be
    /// broken without its exposed item having been collected yet.
    /// </summary>
    public const int RoomChozoBitByteCount = 64;

    /// <summary>
    /// Number of bytes in the persistent picked-up-item table at
    /// <c>$7E:D870-$7E:D8AF</c>. Room PLM arguments are bit indices into this table.
    /// </summary>
    public const int ItemBitByteCount = 64;

    /// <summary>
    /// Number of bytes in the persistent opened-door table at
    /// <c>$7E:D8B0-$7E:D8EF</c>. A colored-door PLM's nonnegative room argument is a bit
    /// index into this table; the bit is set only when the door finishes accepting the
    /// cartridge-authored number and kind of projectile hits.
    /// </summary>
    public const int DoorBitByteCount = 64;

    /// <summary>
    /// Number of 256-byte explored-map planes retained by WRAM
    /// <c>$7E:CD52-$7E:D451</c>. Areas zero through five are packed into SRAM; area six is
    /// Ceres and remains a live-only seventh plane.
    /// </summary>
    public const int ExploredMapAreaCount = 7;

    /// <summary>Bytes in one native 64-by-32 one-bit explored-map plane.</summary>
    public const int ExploredMapBytesPerArea = 256;

    /// <summary>Bytes at <c>$7E:D8F8-$7E:D907</c> for used save/elevator markers.</summary>
    public const int UsedSaveStationByteCount = 16;

    /// <summary>
    /// Bytes at <c>$7E:D908-$7E:D913</c>. Only the area-indexed leading bytes are currently
    /// consumed, but preserving the complete SRAM allocation avoids destroying adjacent
    /// cartridge state written by hacks or untranslated routines.
    /// </summary>
    public const int MapStationByteCount = 12;

    /// <summary>Eight persistent story-event bytes mirrored from $7E:D820.</summary>
    /// <remarks>Keep these arrays private. Returning writable arrays would let callers bypass the same masking
    /// semantics that the ROM routines enforce and would make watch-window corruption extremely difficult to
    /// trace.</remarks>
    private readonly byte[] _events = new byte[EventByteCount];
    /// <summary>Each area's boss state, including any bit whose meaning is not yet proven.</summary>
    private readonly BossBits[] _bossBitsByArea = new BossBits[AreaCount];
    /// <summary>Persistent broken-Chozo-orb bits, independent of item collection.</summary>
    internal readonly byte[] RoomChozoBits = new byte[RoomChozoBitByteCount];
    /// <summary>Persistent bits keyed by physical item PLM arguments.</summary>
    private readonly byte[] _collectedItemBits = new byte[ItemBitByteCount];
    /// <summary>Persistent colored-door bits recorded after their accepted hit sequence.</summary>
    private readonly byte[] _openedDoorBits = new byte[DoorBitByteCount];
    /// <summary>Seven 256-byte explored-map planes, including live-only Ceres.</summary>
    private readonly byte[] _exploredMapTiles =
        new byte[ExploredMapAreaCount * ExploredMapBytesPerArea];
    /// <summary>Native save and elevator marker allocation preserved for SRAM packing.</summary>
    private readonly byte[] _usedSaveStationsAndElevators = new byte[UsedSaveStationByteCount];
    /// <summary>Map-station acquisition bytes and their retained allocation padding.</summary>
    private readonly byte[] _mapStations = new byte[MapStationByteCount];

    /// <summary>
    /// Saved frontend dispatcher value at $7E:D914, 0 through 65,535: the save field's range. Ordinary gameplay persists
    /// state five; SpaceTime Beam can overwrite this independently of Samus equipment and checkpoint, so any value a save
    /// field holds is possible. It is only copied, never counted or compared by sign.
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

    /// <summary>Creates the shared state over the session's random-number generator.</summary>
    /// <param name="random">The session's random-number word, which this state reads and advances.</param>
    public SharedGameState(SharedRandom random) => Random = Ensure.NotNull(random);

    /// <summary>
    /// Carry flag inside every <c>MainGameLoop</c> iteration ($82:8948). The loop brackets
    /// each pass with <c>PHP</c>/<c>PLP</c>, so it keeps the carry left by boot's
    /// <c>DetermineNumberOfDemoSets</c> ($80:8261) when $80:856E jumps to the loop.
    /// Code that adds or subtracts without first setting carry observes it.
    /// </summary>
    public bool MainGameLoopCarry { get; set; }

    /// <summary>Restores the saved loading game state from one checksummed save slot or a checkpoint.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside a save field's range.</exception>
    public void LoadSavedLoadingGameState(int value) => SavedLoadingGameState = value;

    /// <summary>
    /// $7E:D91A; cumulative item PLM initializations, including repeat visits, 0 through 65,535. The count is saved as
    /// one save field and wraps from 65,535 to zero when an item initializes, a wrap the saved value shows; the setter
    /// rejects anything outside the field's range, so the producer that counts must apply that wrap.
    /// </summary>
    public int LoadedItemCount
    {
        get;
        set => field = SaveFieldValue.IsInRange(value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "The loaded-item count is a save field.");
    }

    /// <summary>Whether <paramref name="offset"/> from the event bitset lies in the persistent-progress slice.</summary>
    internal static bool ProgressionSliceHolds(int offset) => offset is >= 0 and < ProgressionSliceLayout.ByteCount;

    /// <summary>
    /// Stores one raw byte at <paramref name="offset"/> from the event bitset into the persistent-progress slice
    /// (<c>$7E:D820-$D91B</c>), in the native layout. SpaceTime's malformed palette copy overwrites progression this way
    /// (#1318, #1330); every written bit survives. The padding after the opened-door bits is not state (owner decision
    /// on #1330), so a store there changes nothing.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> lies outside the slice.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a byte, 0 through 255.</exception>
    internal void WriteProgressionSliceByte(int offset, int value)
    {
        if (!ProgressionSliceHolds(offset))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "The offset lies outside the progression slice.");
        Ensure.BetweenInclusive(0, byte.MaxValue, value);
        if (offset < ProgressionSliceLayout.BossBitsOffset)
            _events[offset - ProgressionSliceLayout.EventsOffset] = (byte)value;
        else if (offset < ProgressionSliceLayout.RoomChozoBitsOffset)
            _bossBitsByArea[offset - ProgressionSliceLayout.BossBitsOffset] = BossBitsCodec.FromSaveByte((byte)value);
        else if (offset < ProgressionSliceLayout.CollectedItemBitsOffset)
            RoomChozoBits[offset - ProgressionSliceLayout.RoomChozoBitsOffset] = (byte)value;
        else if (offset < ProgressionSliceLayout.OpenedDoorBitsOffset)
            _collectedItemBits[offset - ProgressionSliceLayout.CollectedItemBitsOffset] = (byte)value;
        else if (offset < ProgressionSliceLayout.PaddingOffset)
            _openedDoorBits[offset - ProgressionSliceLayout.OpenedDoorBitsOffset] = (byte)value;
        else if (offset < ProgressionSliceLayout.UsedSaveStationsOffset)
            return;
        else if (offset < ProgressionSliceLayout.MapStationsOffset)
            _usedSaveStationsAndElevators[offset - ProgressionSliceLayout.UsedSaveStationsOffset] = (byte)value;
        else if (offset < ProgressionSliceLayout.LoadingGameStateOffset)
            _mapStations[offset - ProgressionSliceLayout.MapStationsOffset] = (byte)value;
        else
        {
            int word = SaveFieldValue.Encode(SavedLoadingGameState, nameof(SavedLoadingGameState));
            SavedLoadingGameState = offset == ProgressionSliceLayout.LoadingGameStateOffset
                ? SnesLittleEndian.Word((byte)value, SnesLittleEndian.HighByte(word))
                : SnesLittleEndian.Word(SnesLittleEndian.LowByte(word), (byte)value);
        }
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

    /// <summary>
    /// Sets one or more boss-state bits for an area, equivalent to <c>$80:81A6</c>.
    /// </summary>
    public void SetBossBits(int areaIndex, BossBits bits)
    {
        ValidateAreaIndex(areaIndex);
        BossBitMasks.Validate(bits, "Boss-state set mask");
        _bossBitsByArea[areaIndex] |= bits;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public void SetBossBits(AreaId areaIndex, BossBits bits) =>
        SetBossBits(AreaIds.ToIndex(areaIndex), bits);

    /// <summary>
    /// Clears one or more boss-state bits for an area, equivalent to the unused original
    /// routine at <c>$80:81C0</c>. Keeping it is useful for debugger experiments.
    /// </summary>
    public void ClearBossBits(int areaIndex, BossBits bits)
    {
        ValidateAreaIndex(areaIndex);
        BossBitMasks.Validate(bits, "Boss-state clear mask");
        _bossBitsByArea[areaIndex] &= ~bits;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public void ClearBossBits(AreaId areaIndex, BossBits bits) =>
        ClearBossBits(AreaIds.ToIndex(areaIndex), bits);

    /// <summary>
    /// Returns whether any requested boss bit is set, matching <c>$80:81DC</c>.
    /// </summary>
    public bool HasAnyBossBits(int areaIndex, BossBits bits)
    {
        ValidateAreaIndex(areaIndex);
        BossBitMasks.Validate(bits, "Boss-state query mask");
        return (_bossBitsByArea[areaIndex] & bits) != BossBits.None;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public bool HasAnyBossBits(AreaId areaIndex, BossBits bits) =>
        HasAnyBossBits(AreaIds.ToIndex(areaIndex), bits);

    /// <summary>
    /// Exposes an area's raw SRAM-mirror byte for save-state inspection and diagnostics.
    /// Mutations still go through the methods above.
    /// </summary>
    public byte GetBossBitsRaw(int areaIndex)
    {
        ValidateAreaIndex(areaIndex);
        return _bossBitsByArea[areaIndex].ToSaveByte();
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public byte GetBossBitsRaw(AreaId areaIndex) => GetBossBitsRaw(AreaIds.ToIndex(areaIndex));

    /// <summary>
    /// Returns the named portion of an area's live boss state for gameplay consumers. Raw
    /// save serialization deliberately continues to use <see cref="GetBossBitsRaw(int)"/>.
    /// </summary>
    public BossBits GetBossBits(int areaIndex)
    {
        ValidateAreaIndex(areaIndex);
        BossBits bits = _bossBitsByArea[areaIndex];
        BossBitMasks.Validate(bits, $"Area {areaIndex} boss state");
        return bits;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public BossBits GetBossBits(AreaId areaIndex) => GetBossBits(AreaIds.ToIndex(areaIndex));

    /// <summary>Restores all eight native area-boss bytes from a save-slot payload.</summary>
    public void LoadBossBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, AreaCount);

        for (int area = 0; area < AreaCount; area++)
            _bossBitsByArea[area] = BossBitsCodec.FromSaveByte(bytes[area]);
    }

    /// <summary>
    /// Marks an event as having occurred, matching <c>$80:81FA</c>.
    /// </summary>
    public void SetEvent(EventNumber eventNumber)
    {
        SetEventRaw(ResolveNamedEvent(eventNumber));
    }

    /// <summary>
    /// Marks an event bit whose retail identity is not yet proven. Cartridge bytecode is
    /// allowed to use this deliberately raw boundary; ordinary game logic must use
    /// <see cref="EventNumber"/> so an unexplained number cannot acquire accidental meaning.
    /// </summary>
    public void SetEventRaw(int eventNumber)
    {
        (int byteIndex, int bitMask) = ResolveEventBit(eventNumber);
        _events[byteIndex] |= checked((byte)bitMask);
    }

    /// <summary>
    /// Clears an event, matching <c>$80:8212</c>.
    /// </summary>
    public void ClearEvent(EventNumber eventNumber)
    {
        ClearEventRaw(ResolveNamedEvent(eventNumber));
    }

    /// <summary>Clears an allocated but intentionally unnamed cartridge event bit.</summary>
    public void ClearEventRaw(int eventNumber)
    {
        (int byteIndex, int bitMask) = ResolveEventBit(eventNumber);
        _events[byteIndex] &= unchecked((byte)~bitMask);
    }

    /// <summary>
    /// Tests an event, matching <c>$80:8233</c>.
    /// </summary>
    public bool HasEvent(EventNumber eventNumber)
    {
        return HasEventRaw(ResolveNamedEvent(eventNumber));
    }

    /// <summary>Tests an allocated but intentionally unnamed cartridge event bit.</summary>
    public bool HasEventRaw(int eventNumber)
    {
        (int byteIndex, int bitMask) = ResolveEventBit(eventNumber);
        return (_events[byteIndex] & bitMask) != 0;
    }

    /// <summary>
    /// Returns a raw event byte for debugger/save-state inspection.
    /// </summary>
    public byte GetEventByteRaw(int byteIndex)
    {
        if ((uint)byteIndex >= EventByteCount)
            throw new ArgumentOutOfRangeException(nameof(byteIndex), byteIndex, "Event byte index must be in the SRAM mirror's eight-byte range.");

        return _events[byteIndex];
    }

    /// <summary>Restores the complete native event-bit allocation from a save slot.</summary>
    public void LoadEventBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, EventByteCount);

        bytes.CopyTo(_events);
    }

    /// <summary>
    /// Tests the bit selected by a nonnegative room-PLM argument, matching the shared
    /// <c>PrepareBitAccess</c> path used by instruction <c>$84:887C</c>.
    /// </summary>
    public bool HasCollectedItemBit(int bitIndex)
    {
        (int byteIndex, int bitMask) = ResolveCollectedItemBit(bitIndex);
        return (_collectedItemBits[byteIndex] & bitMask) != 0;
    }

    /// <summary>Returns one raw Chozo-state byte for cartridge-compatible SRAM encoding.</summary>
    public byte GetRoomChozoByteRaw(int byteIndex)
    {
        if ((uint)byteIndex >= RoomChozoBitByteCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteIndex),
                byteIndex,
                "Chozo-state byte index must be in the native 64-byte table.");
        }

        return RoomChozoBits[byteIndex];
    }

    /// <summary>Restores all persistent broken-orb bits from the native save payload.</summary>
    public void LoadRoomChozoBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, RoomChozoBitByteCount);

        bytes.CopyTo(RoomChozoBits);
    }

    /// <summary>
    /// Marks one physical pickup as collected, matching PLM instruction
    /// <c>$84:8899</c>. This table identifies locations; it is deliberately independent
    /// from Samus's equipment words, where multiple tanks of one kind share one meaning.
    /// </summary>
    public void SetCollectedItemBit(int bitIndex)
    {
        (int byteIndex, int bitMask) = ResolveCollectedItemBit(bitIndex);
        _collectedItemBits[byteIndex] |= checked((byte)bitMask);
    }

    /// <summary>Returns one raw byte for cartridge-compatible SRAM encoding.</summary>
    public byte GetCollectedItemByteRaw(int byteIndex)
    {
        if ((uint)byteIndex >= ItemBitByteCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteIndex),
                byteIndex,
                "Collected-item byte index must be in the native 64-byte table.");
        }

        return _collectedItemBits[byteIndex];
    }

    /// <summary>
    /// Restores the native picked-up-item table from a save-slot payload. Keeping the
    /// whole allocation preserves ROM hacks and otherwise-unused high bit indices too.
    /// </summary>
    public void LoadCollectedItemBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, ItemBitByteCount);

        bytes.CopyTo(_collectedItemBits);
    }

    /// <summary>
    /// Tests the persistent bit selected by a colored-door PLM argument. Negative room
    /// arguments are deliberately handled by the PLM owner because the ROM uses bit 15
    /// as a transient/no-save marker rather than indexing before this allocation.
    /// </summary>
    public bool HasOpenedDoorBit(int bitIndex)
    {
        (int byteIndex, int bitMask) = ResolvePersistentRoomBit(
            bitIndex,
            DoorBitByteCount,
            "Opened-door bit index must fit the native 64-byte table.");
        return (_openedDoorBits[byteIndex] & bitMask) != 0;
    }

    /// <summary>
    /// Persists a colored door after its final valid hit, matching the door instruction
    /// at <c>$84:8A91</c>. Re-entering the room then converts that colored cap into its
    /// ordinary blue-door counterpart.
    /// </summary>
    public void SetOpenedDoorBit(int bitIndex)
    {
        (int byteIndex, int bitMask) = ResolvePersistentRoomBit(
            bitIndex,
            DoorBitByteCount,
            "Opened-door bit index must fit the native 64-byte table.");
        _openedDoorBits[byteIndex] |= checked((byte)bitMask);
    }

    /// <summary>Returns one raw opened-door byte for cartridge-compatible SRAM encoding.</summary>
    public ushort GetOpenedDoorWordRaw(int byteIndex)
    {
        if ((uint)byteIndex >= DoorBitByteCount - 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteIndex),
                byteIndex,
                "Opened-door word index must leave its high byte in the native 64-byte table.");
        }

        return SnesLittleEndian.Word(_openedDoorBits[byteIndex], _openedDoorBits[byteIndex + 1]);
    }

    /// <summary>Returns one raw opened-door byte from the native 64-byte table.</summary>
    public byte GetOpenedDoorByteRaw(int byteIndex)
    {
        if ((uint)byteIndex >= DoorBitByteCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteIndex),
                byteIndex,
                "Opened-door byte index must be in the native 64-byte table.");
        }

        return _openedDoorBits[byteIndex];
    }

    /// <summary>Restores all persistent opened-door bits from a native save payload.</summary>
    public void LoadOpenedDoorBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, DoorBitByteCount);

        bytes.CopyTo(_openedDoorBits);
    }

    /// <summary>
    /// Marks the area-map cell containing Samus, matching <c>$90:A8A6/$90:A91B</c>.
    /// The 64-column map is stored as two 32-by-32 bit planes, each row occupying four
    /// bytes; it is not a conventional eight-byte-wide linear bitmap.
    /// </summary>
    public void MarkExploredMapTile(int areaIndex, Position16<MapTiles> cell)
    {
        ValidateExploredMapCoordinate(areaIndex, cell);
        int byteIndex = ResolveExploredMapByteIndex(areaIndex, cell);
        _exploredMapTiles[byteIndex] |= AreaMapLayout.GetBitMask(cell.X);
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public void MarkExploredMapTile(AreaId areaIndex, Position16<MapTiles> cell) =>
        MarkExploredMapTile(AreaIds.ToIndex(areaIndex), cell);

    /// <summary>Returns whether a 64-by-32 area-map cell has been visited.</summary>
    public bool IsMapTileExplored(int areaIndex, Position16<MapTiles> cell)
    {
        ValidateExploredMapCoordinate(areaIndex, cell);
        int byteIndex = ResolveExploredMapByteIndex(areaIndex, cell);
        return (_exploredMapTiles[byteIndex] & AreaMapLayout.GetBitMask(cell.X)) != 0;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public bool IsMapTileExplored(AreaId areaIndex, Position16<MapTiles> cell) =>
        IsMapTileExplored(AreaIds.ToIndex(areaIndex), cell);

    /// <summary>Returns one byte from an area's native explored-map plane for SRAM packing.</summary>
    public byte GetExploredMapByteRaw(int areaIndex, int byteIndex)
    {
        if ((uint)areaIndex >= ExploredMapAreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)byteIndex >= ExploredMapBytesPerArea)
            throw new ArgumentOutOfRangeException(nameof(byteIndex));
        return _exploredMapTiles[areaIndex * ExploredMapBytesPerArea + byteIndex];
    }

    /// <summary>Restores all seven unpacked WRAM explored-map planes from a save snapshot.</summary>
    public void LoadExploredMapBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, _exploredMapTiles.Length);
        bytes.CopyTo(_exploredMapTiles);
    }

    /// <summary>Marks a save point in the low byte of an area's native two-byte marker.</summary>
    public void MarkSaveStationUsed(int areaIndex, int stationBitIndex)
    {
        ValidateAreaIndex(areaIndex);
        if ((uint)stationBitIndex >= 8)
            throw new ArgumentOutOfRangeException(nameof(stationBitIndex));
        _usedSaveStationsAndElevators[areaIndex * 2] |=
            unchecked((byte)(1 << stationBitIndex));
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public void MarkSaveStationUsed(AreaId areaIndex, int stationBitIndex) =>
        MarkSaveStationUsed(AreaIds.ToIndex(areaIndex), stationBitIndex);

    /// <summary>Returns one raw save/elevator marker byte for cartridge-compatible SRAM.</summary>
    public byte GetUsedSaveStationByteRaw(int byteIndex)
    {
        if ((uint)byteIndex >= UsedSaveStationByteCount)
            throw new ArgumentOutOfRangeException(nameof(byteIndex));
        return _usedSaveStationsAndElevators[byteIndex];
    }

    /// <summary>Restores the complete native save/elevator marker allocation.</summary>
    public void LoadUsedSaveStationBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, UsedSaveStationByteCount);
        bytes.CopyTo(_usedSaveStationsAndElevators);
    }

    /// <summary>Returns whether the area has downloaded its map-station data.</summary>
    public bool HasAreaMap(int areaIndex)
    {
        if ((uint)areaIndex >= ExploredMapAreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        return _mapStations[areaIndex] != 0;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public bool HasAreaMap(AreaId areaIndex) => HasAreaMap(AreaIds.ToIndex(areaIndex));

    /// <summary>Marks an area's map station as acquired, matching the native $FF byte write.</summary>
    public void SetAreaMapAcquired(int areaIndex)
    {
        if ((uint)areaIndex >= ExploredMapAreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        _mapStations[areaIndex] = byte.MaxValue;
    }

    /// <summary>Typed retail-area overload for live room and gameplay callers.</summary>
    public void SetAreaMapAcquired(AreaId areaIndex) => SetAreaMapAcquired(AreaIds.ToIndex(areaIndex));

    /// <summary>Returns one raw map-station byte for cartridge-compatible SRAM.</summary>
    public byte GetMapStationByteRaw(int byteIndex)
    {
        if ((uint)byteIndex >= MapStationByteCount)
            throw new ArgumentOutOfRangeException(nameof(byteIndex));
        return _mapStations[byteIndex];
    }

    /// <summary>Restores the complete native map-station allocation.</summary>
    public void LoadMapStationBytes(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, MapStationByteCount);
        bytes.CopyTo(_mapStations);
    }

    /// <summary>
    /// C# equivalent of <c>$80:818E</c>: split a bit number into a byte index and mask.
    /// </summary>
    private static ProgressionBit ResolveEventBit(int eventNumber)
    {
        // The assembly BRKs when bit 15 is set. The actual event allocation is even
        // tighter, so model the eight-byte SRAM object rather than allowing adjacent save
        // data to be indexed accidentally.
        if ((uint)eventNumber >= EventByteCount * 8)
            throw new ArgumentOutOfRangeException(nameof(eventNumber), eventNumber, "Event number must be in the allocated $00-$3F range.");

        int byteIndex = eventNumber >> 3;
        return new ProgressionBit(byteIndex, 1 << (eventNumber & 7));
    }

    /// <summary>
    /// Rejects a forged enum value instead of silently treating the named API as another
    /// spelling of the raw SRAM index API.
    /// </summary>
    private static int ResolveNamedEvent(EventNumber eventNumber)
    {
        if (!Enum.IsDefined(eventNumber))
        {
            throw new ArgumentOutOfRangeException(
                nameof(eventNumber),
                eventNumber,
                "The event has no proven retail EventNumber identity; use the explicit raw API for cartridge data.");
        }

        return eventNumber.ToEventBit();
    }

    /// <summary>Maps a PLM item argument to a checked collected-item byte and bit mask.</summary>
    private static ProgressionBit ResolveCollectedItemBit(int bitIndex)
    {
        return ResolvePersistentRoomBit(
            bitIndex,
            ItemBitByteCount,
            "Collected-item bit index must fit the native 64-byte table.");
    }

    /// <summary>Bounds a Chozo-orb or opened-door bit against its 64-byte persistent table.</summary>
    internal static ProgressionBit ResolvePersistentRoomBit(
        int bitIndex,
        int byteCount,
        string errorMessage)
    {
        if ((uint)bitIndex >= byteCount * 8)
            throw new ArgumentOutOfRangeException(nameof(bitIndex), bitIndex, errorMessage);

        return new ProgressionBit(bitIndex >> 3, unchecked((byte)(1 << (bitIndex & 7))));
    }

    /// <summary>Rejects area indices outside the cartridge's eight persistent boss bytes.</summary>
    private static void ValidateAreaIndex(int areaIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex), areaIndex, "Area index must be in the allocated 0-7 range.");
    }

    /// <summary>Maps a 64-by-32 cell into the native two-plane explored-map byte layout.</summary>
    private static int ResolveExploredMapByteIndex(int areaIndex, Position16<MapTiles> cell)
    {
        int areaOffset = areaIndex * ExploredMapBytesPerArea;
        return areaOffset + AreaMapLayout.GetBitByteIndex(cell);
    }

    /// <summary>Validates area and cell bounds before a map exploration bit is read or written.</summary>
    private static void ValidateExploredMapCoordinate(int areaIndex, Position16<MapTiles> cell)
    {
        if ((uint)areaIndex >= ExploredMapAreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if (!AreaMapLayout.Size.Contains(cell))
            throw new ArgumentOutOfRangeException(nameof(cell));
    }
}
