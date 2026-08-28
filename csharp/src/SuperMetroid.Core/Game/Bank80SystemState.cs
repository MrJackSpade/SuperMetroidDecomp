namespace SuperMetroid.Core.Game;

/// <summary>
/// The small, shared pieces of game state manipulated by the first utility routines in
/// ROM bank <c>$80</c>. This is intentionally a stateful object: the original routines
/// communicate through fixed WRAM and SRAM-mirror addresses, not through return values
/// and neatly isolated objects.
/// </summary>
/// <remarks>
/// Source routines:
/// <list type="bullet">
/// <item><description><c>$80:8111</c> — generate random number.</description></item>
/// <item><description><c>$80:8146</c> — update held input.</description></item>
/// <item><description><c>$80:818E-$80:824E</c> — event and boss bit access.</description></item>
/// <item><description><c>$80:82D6</c> — unsigned 16-by-16-bit multiplication.</description></item>
/// </list>
///
/// The class does not attempt to emulate the 65C816 CPU. It preserves the externally
/// visible behavior of these routines while naming the RAM locations they operated on.
/// That gives the C# debugger useful domain-level state without losing the connection to
/// the assembly.
/// </remarks>
public sealed class Bank80SystemState
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

    // Keep these arrays private. Returning writable arrays would let callers bypass the
    // same masking semantics that the ROM routines enforce and would make watch-window
    // corruption extremely difficult to trace.
    private readonly byte[] _events = new byte[EventByteCount];
    private readonly byte[] _bossBitsByArea = new byte[AreaCount];

    /// <summary>
    /// Creates the bank-$80 state using the game's power-on RNG seed, <c>$0061</c>.
    /// </summary>
    public Bank80SystemState()
        : this(0x0061)
    {
    }

    /// <summary>
    /// Creates state with a chosen RNG seed. The overload exists primarily so all 65,536
    /// input states can be checked against the original algorithm.
    /// </summary>
    public Bank80SystemState(ushort randomNumberSeed)
    {
        RandomNumber = randomNumberSeed;
    }

    /// <summary>
    /// Current 16-bit RNG state, corresponding to WRAM <c>$05E5</c>.
    /// </summary>
    public ushort RandomNumber { get; private set; }

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
    public void SetRandomNumber(ushort value) => RandomNumber = value;

    /// <summary>
    /// Most recently calculated held input, corresponding to WRAM <c>$05D9</c>.
    /// Despite the C decompilation's name <c>joypad_released_keys</c>, the assembly names
    /// and behavior show that this is the previous value of "held but not newly pressed."
    /// </summary>
    public ushort HeldInputPrevious { get; private set; }

    /// <summary>
    /// Countdown at WRAM <c>$05DB</c>. A signed underflow from zero to <c>$FFFF</c>
    /// causes the held input to become active.
    /// </summary>
    public ushort TimedHeldInputTimer { get; private set; }

    /// <summary>
    /// Reload value at WRAM <c>$05DD</c>. A value of N requires N + 1 stable update
    /// calls after the held-input value changes.
    /// </summary>
    public ushort TimedHeldInputTimerReset { get; private set; }

    /// <summary>
    /// Filtered input at WRAM <c>$05DF</c>. Bits appear after their buttons have remained
    /// stable long enough for the timer to underflow.
    /// </summary>
    public ushort TimedHeldInput { get; private set; }

    /// <summary>
    /// Rising-edge bits for <see cref="TimedHeldInput"/>, stored at WRAM <c>$05E1</c>.
    /// </summary>
    public ushort NewlyTimedHeldInput { get; private set; }

    /// <summary>
    /// Prior active filtered input at WRAM <c>$05E3</c>. The original routine updates
    /// this only on timer underflow, not on every frame.
    /// </summary>
    public ushort TimedHeldInputPrevious { get; private set; }

    /// <summary>
    /// Advances Super Metroid's 16-bit pseudo-random sequence exactly as routine
    /// <c>$80:8111</c> does and returns the new seed.
    /// </summary>
    public ushort NextRandom()
    {
        // The ROM performs two separate 8x8 multiplies through the SNES hardware
        // multiplier ($4202/$4203). This first product is retained as a 16-bit word.
        int lowByteProduct = (RandomNumber & 0xff) * 5;

        // The second hardware multiplication is read through an 8-bit accumulator, so
        // only its low result byte participates directly in the next addition.
        int highByteProductLow = ((RandomNumber >> 8) * 5) & 0xff;

        // SEC makes the addition start with carry=1. It modifies the high byte of the
        // first product. The ninth bit is retained separately as the processor carry.
        int highByteSum = ((lowByteProduct >> 8) & 0xff) + highByteProductLow + 1;

        // STA $02,S replaces the saved product's high byte but, naturally, stores only
        // eight bits. Reconstruct that post-STA stack word explicitly.
        int savedProduct = (lowByteProduct & 0xff) | ((highByteSum & 0xff) << 8);

        // After PLA restores 16-bit A, ADC #$0011 consumes the carry left by the 8-bit
        // addition. That extra carry is the reason this is only *roughly* r*5+$0111.
        int result = savedProduct + 0x0011 + (highByteSum >> 8);

        // A 65C816 accumulator wraps at 16 bits. The explicit unchecked cast documents
        // that overflow is required behavior rather than an accidental omission.
        RandomNumber = unchecked((ushort)result);
        return RandomNumber;
    }

    /// <summary>
    /// Ports <c>$80:8146</c>, which turns stable held buttons into delayed input suitable
    /// for repeated cursor movement on the pause map and item screens.
    /// </summary>
    /// <param name="timerReset">Number of stable frames minus one before activation.</param>
    /// <param name="controllerInput">All buttons currently down (WRAM <c>$8B</c>).</param>
    /// <param name="controllerNewInput">Buttons newly pressed this frame (WRAM <c>$8F</c>).</param>
    public void UpdateHeldInput(ushort timerReset, ushort controllerInput, ushort controllerNewInput)
    {
        TimedHeldInputTimerReset = timerReset;

        // TRB in the original clears every newly-pressed bit from the current input.
        // A fresh press is deliberately not considered "held" until the following frame.
        ushort heldInput = (ushort)(controllerInput & ~controllerNewInput);

        // CMP happens before STA, so remember the comparison result before replacing the
        // previous sample. This ordering is significant when a button changes state.
        bool heldInputIsUnchanged = heldInput == HeldInputPrevious;
        HeldInputPrevious = heldInput;

        if (!heldInputIsUnchanged)
        {
            // Any change, including releasing one member of a multi-button chord, restarts
            // the delay and suppresses filtered input for this update.
            TimedHeldInputTimer = TimedHeldInputTimerReset;
            TimedHeldInput = 0;
        }
        else
        {
            // DEC is an unsigned 16-bit wrap, but BPL interprets bit 15 as the sign bit.
            // Therefore zero decrements to $FFFF and takes the activation path.
            TimedHeldInputTimer = unchecked((ushort)(TimedHeldInputTimer - 1));
            bool timerIsNonNegativeInSigned16Bit = (TimedHeldInputTimer & 0x8000) == 0;

            if (timerIsNonNegativeInSigned16Bit)
            {
                TimedHeldInput = 0;
            }
            else
            {
                // The ROM pins the expired timer to zero. Continued stable input therefore
                // underflows again on every update, keeping TimedHeldInput active.
                TimedHeldInputTimer = 0;
                TimedHeldInputPrevious = TimedHeldInput;
                TimedHeldInput = heldInput;
            }
        }

        // This is the standard rising-edge expression: current & (previous XOR current).
        // It produces a one-update pulse when a delayed button first becomes active.
        NewlyTimedHeldInput = (ushort)(TimedHeldInput & (TimedHeldInputPrevious ^ TimedHeldInput));
    }

    /// <summary>
    /// Sets one or more boss-state bits for an area, equivalent to <c>$80:81A6</c>.
    /// </summary>
    public void SetBossBits(int areaIndex, BossBits bits)
    {
        ValidateAreaIndex(areaIndex);
        _bossBitsByArea[areaIndex] |= (byte)bits;
    }

    /// <summary>
    /// Clears one or more boss-state bits for an area, equivalent to the unused original
    /// routine at <c>$80:81C0</c>. Keeping it is useful for debugger experiments.
    /// </summary>
    public void ClearBossBits(int areaIndex, BossBits bits)
    {
        ValidateAreaIndex(areaIndex);
        _bossBitsByArea[areaIndex] &= unchecked((byte)~(byte)bits);
    }

    /// <summary>
    /// Returns whether any requested boss bit is set, matching <c>$80:81DC</c>.
    /// </summary>
    public bool HasAnyBossBits(int areaIndex, BossBits bits)
    {
        ValidateAreaIndex(areaIndex);
        return (_bossBitsByArea[areaIndex] & (byte)bits) != 0;
    }

    /// <summary>
    /// Exposes an area's raw SRAM-mirror byte for save-state inspection and diagnostics.
    /// Mutations still go through the methods above.
    /// </summary>
    public byte GetBossBitsRaw(int areaIndex)
    {
        ValidateAreaIndex(areaIndex);
        return _bossBitsByArea[areaIndex];
    }

    /// <summary>
    /// Marks an event as having occurred, matching <c>$80:81FA</c>.
    /// </summary>
    public void SetEvent(int eventNumber)
    {
        (int byteIndex, byte bitMask) = ResolveEventBit(eventNumber);
        _events[byteIndex] |= bitMask;
    }

    /// <summary>
    /// Clears an event, matching <c>$80:8212</c>.
    /// </summary>
    public void ClearEvent(int eventNumber)
    {
        (int byteIndex, byte bitMask) = ResolveEventBit(eventNumber);
        _events[byteIndex] &= unchecked((byte)~bitMask);
    }

    /// <summary>
    /// Tests an event, matching <c>$80:8233</c>.
    /// </summary>
    public bool HasEvent(int eventNumber)
    {
        (int byteIndex, byte bitMask) = ResolveEventBit(eventNumber);
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

    /// <summary>
    /// Unsigned <c>16 x 16 -> 32</c> multiplication from <c>$80:82D6</c>.
    /// The widening casts must happen before multiplication or C# would discard the high
    /// word before returning it.
    /// </summary>
    public static uint Multiply16By16(ushort left, ushort right) => (uint)left * right;

    /// <summary>
    /// C# equivalent of <c>$80:818E</c>: split a bit number into a byte index and mask.
    /// </summary>
    private static (int ByteIndex, byte BitMask) ResolveEventBit(int eventNumber)
    {
        // The assembly BRKs when bit 15 is set. The actual event allocation is even
        // tighter, so model the eight-byte SRAM object rather than allowing adjacent save
        // data to be indexed accidentally.
        if ((uint)eventNumber >= EventByteCount * 8)
            throw new ArgumentOutOfRangeException(nameof(eventNumber), eventNumber, "Event number must be in the allocated $00-$3F range.");

        int byteIndex = eventNumber >> 3;
        byte bitMask = (byte)(1 << (eventNumber & 7));
        return (byteIndex, bitMask);
    }

    private static void ValidateAreaIndex(int areaIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex), areaIndex, "Area index must be in the allocated 0-7 range.");
    }
}

/// <summary>
/// Bits in each area's byte at SRAM mirror <c>$7E:D828-$7E:D82F</c>.
/// </summary>
[Flags]
public enum BossBits : byte
{
    None = 0,

    /// <summary>Kraid, Phantoon, Draygon, or either Ridley.</summary>
    AreaBoss = 1 << 0,

    /// <summary>Spore Spawn, Botwoon, Crocomire, or Mother Brain.</summary>
    AreaMiniBoss = 1 << 1,

    /// <summary>Bomb Torizo or Golden Torizo.</summary>
    AreaTorizo = 1 << 2,
}

/// <summary>
/// Known story/progression event numbers consumed by <c>$80:81FA-$80:824E</c>.
/// The backing table has room for 64 event bits, but the original game names only these.
/// </summary>
public enum EventNumber
{
    ZebesAwake = 0x00,
    ShitroidAteSidehopper = 0x01,
    MotherBrainGlassDestroyed = 0x02,
    ZebetiteDestroyedBit0 = 0x03,
    ZebetiteDestroyedBit1 = 0x04,
    ZebetiteDestroyedBit2 = 0x05,
    PhantoonStatueGrey = 0x06,
    RidleyStatueGrey = 0x07,
    DraygonStatueGrey = 0x08,
    KraidStatueGrey = 0x09,
    TourianUnlocked = 0x0a,
    MaridiaNoobTubeBroken = 0x0b,
    LowerNorfairChozoLoweredAcid = 0x0c,
    ShaktoolClearedPath = 0x0d,
    ZebesTimebombSet = 0x0e,
    CrittersEscaped = 0x0f,
    FirstMetroidHallCleared = 0x10,
    FirstMetroidShaftCleared = 0x11,
    SecondMetroidHallCleared = 0x12,
    SecondMetroidShaftCleared = 0x13,
    Unused14 = 0x14,
    OutranSpeedBoosterLavaquake = 0x15,
}
