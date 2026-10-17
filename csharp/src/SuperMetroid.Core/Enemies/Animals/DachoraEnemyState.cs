using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Typed view of the six native Dachora variables. Their meanings intentionally change
/// between phases: A/B are horizontal speed while running and vertical speed after the
/// shinespark; D is vertical subacceleration on the body but echo lifetime on echo slots.
/// Every view writes the slot variables exactly, so other enemies' aliased enemy-RAM reads
/// see the cartridge's words.
/// </summary>
public sealed class DachoraEnemyState
{
    /// <summary>Physical Dachora body or echo slot carrying its phase-dependent variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects Dachora's six native words from its occupied enemy slot.</summary>
    internal DachoraEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<DachoraProgram> Program { get; set; }

    /// <summary>
    /// Variables A (whole pixels) and B (fraction) while running, falling or shinesparking: the signed 16.16 speed in
    /// pixels per update, horizontal while running and vertical after the launch. It accumulates with the cartridge's
    /// carry and 32-bit wraparound.
    /// </summary>
    public SnesSignedSixteenSixteen Speed
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableA), _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.WholeWord;
            _slot.VariableB = value.Fraction;
        }
    }

    /// <summary>
    /// Variable A while blinking or charging: the native wrapping countdown to the next phase
    /// (<c>DEC</c>, <c>BNE</c>).
    /// </summary>
    public NativeCountdown PhaseTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>Variable A on the first echo slot: frames until the echo positions next advance.</summary>
    public NativeCountdown EchoPositionTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableA);
        internal set => _slot.VariableA = value.RawValue;
    }

    /// <summary>
    /// Variables C (whole pixels) and D (fraction) on the body during the shinespark: the signed 16.16 vertical
    /// acceleration per update, which gravity accumulates into with the cartridge's carry and 32-bit wraparound.
    /// </summary>
    public SnesSignedSixteenSixteen VerticalAcceleration
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableC), _slot.VariableD);
        internal set
        {
            _slot.VariableC = value.WholeWord;
            _slot.VariableD = value.Fraction;
        }
    }

    /// <summary>
    /// Variable D on an echo slot: the updates the echo stays visible, counted down to zero and never below it
    /// (<c>LDA</c>, <c>BEQ</c>, <c>DEC</c>); zero marks the echo free for the next trail position.
    /// </summary>
    public NativeCountdown EchoLifetime
    {
        get => NativeCountdown.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
    }

    /// <summary>
    /// Variable E's high byte: the speed or shine palette frame the cycle shows next.
    /// </summary>
    public int PaletteFrame => SnesLittleEndian.HighByte(_slot.VariableE);

    /// <summary>
    /// Variable E's low byte: the updates left before the speed palette cycle shows <see cref="PaletteFrame"/>.
    /// </summary>
    public int PaletteTicks => SnesLittleEndian.LowByte(_slot.VariableE);

    /// <summary>
    /// Sets the palette cycle to show <paramref name="frame"/> after <paramref name="ticks"/> updates, each 0 through
    /// 255, as the run, wall, launch and ceiling phases restart or clear it.
    /// </summary>
    internal void SetPaletteCycle(int frame, int ticks) =>
        _slot.VariableE = SnesLittleEndian.Word(
            checked((byte)Ensure.BetweenInclusive(0, byte.MaxValue, ticks)),
            checked((byte)Ensure.BetweenInclusive(0, byte.MaxValue, frame)));

    /// <summary>
    /// Counts one update off the speed palette cycle and reports whether its ticks reached zero. The cartridge
    /// decrements the frame and ticks together as one word, so ticks counted down from zero borrow from the frame:
    /// a cleared cycle becomes frame 255 with 255 ticks.
    /// </summary>
    internal bool TickPaletteCycle()
    {
        _slot.VariableE = NativeWordCounter.Decrement(_slot.VariableE).Value;
        return PaletteTicks == 0;
    }

    /// <summary>
    /// Moves the speed palette cycle to its next frame with <see cref="DachoraPaletteDefinitions.SpeedStep"/>'s ticks
    /// added; once that passes the last frame, the cycle stays on the brightest frame instead of wrapping to frame zero.
    /// The step and the bound are compared as the cartridge's signed word arithmetic does.
    /// </summary>
    internal void AdvanceSpeedPaletteCycle()
    {
        _slot.VariableE = unchecked((ushort)(_slot.VariableE + DachoraPaletteDefinitions.SpeedStep));
        if (unchecked((short)(_slot.VariableE - DachoraPaletteDefinitions.SpeedEnd)) >= 0)
            _slot.VariableE = DachoraPaletteDefinitions.SpeedRestart;
    }

    /// <summary>
    /// Moves the shine palette cycle to its next frame; after the last frame it returns to frame zero. The step and the
    /// bound are compared as the cartridge's signed word arithmetic does.
    /// </summary>
    internal void AdvanceShinePaletteCycle()
    {
        _slot.VariableE = unchecked((ushort)(_slot.VariableE + DachoraPaletteDefinitions.ShineStep));
        if (unchecked((short)(_slot.VariableE - DachoraPaletteDefinitions.ShineEnd)) >= 0)
            _slot.VariableE = 0;
    }

    /// <summary>Variable F: bank-$A7 indirect main-AI address.</summary>
    public DachoraAiFunction Function
    {
        get => (DachoraAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}
