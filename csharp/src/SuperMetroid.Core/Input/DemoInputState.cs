using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Input;

/// <summary>
/// Bank-$91's ROM-driven demo-controller object (<c>$91:834E..8463</c>).
/// </summary>
/// <remarks>
/// A demo input list is bytecode, not merely an array of controller samples. Positive
/// words begin six-byte records (<c>duration, held, newly pressed</c>); negative words are
/// control instructions that can change the object's pre-instruction, loop, or delete it.
/// Keeping this interpreter independent of any one cinematic lets the intro flashbacks and
/// the eventual title-screen demonstrations consume the same cartridge-authored machinery.
///
/// The object-specific initializer and pre-instruction are deliberately supplied as
/// callbacks. Bank $91 dispatches those pointers as machine code, while this class owns only
/// the shared object handler. A missing callback is accepted solely for the native RTS
/// stubs; silently treating any other pointer as a no-op would fabricate game behavior.
/// </remarks>
public sealed class DemoInputState
{
    private const int DemoInputBank = 0x910000;
    private const ushort NoOpRoutine = 0x83bf;
    private const ushort ClearedPreInstructionRoutine = 0x8447;

    private const ushort DeleteInstruction = 0x8427;
    private const ushort SetPreInstruction = 0x8434;
    private const ushort ClearPreInstruction = 0x843f;
    private const ushort GotoInstruction = 0x8448;
    private const ushort DecrementTimerAndGoto = 0x844f;
    private const ushort SetTimer = 0x8459;

    /// <summary>High-bit enable flag installed by <c>$91:834E</c>.</summary>
    public bool Enabled { get; private set; }

    /// <summary>Object-specific routine called before the shared instruction timer.</summary>
    public ushort PreInstructionPointer { get; private set; }

    /// <summary>Address of the next bank-$91 input record or control instruction.</summary>
    public ushort InstructionPointer { get; private set; }

    /// <summary>Frames remaining in the active input record, with native word wrapping.</summary>
    public ushort InstructionTimer { get; private set; }

    /// <summary>General loop timer manipulated by instructions <c>$844F/$8459</c>.</summary>
    public ushort Timer { get; private set; }

    /// <summary>Initializer parameter stored at WRAM <c>$0A80</c>.</summary>
    public ushort InitializationParameter { get; private set; }

    /// <summary>Controller word currently replacing player-held input.</summary>
    public ushort Held { get; private set; }

    /// <summary>Controller edge word currently replacing newly pressed input.</summary>
    public ushort NewlyPressed { get; private set; }

    /// <summary>Previous demo-held word retained by <c>$91:83D3</c>'s publication seam.</summary>
    public ushort PreviousHeld { get; private set; }

    /// <summary>Previous demo-edge word retained by <c>$91:83D9</c>'s publication seam.</summary>
    public ushort PreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Prior held word published to <c>joypad1_input_samusfilter</c> for this handler call.
    /// </summary>
    public ushort PublishedPreviousHeld { get; private set; }

    /// <summary>
    /// Prior edge word published to <c>joypad1_newinput_samusfilter</c> for this call.
    /// </summary>
    public ushort PublishedPreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Clears every demo-input WRAM word exactly as <c>$91:8370</c> does.
    /// </summary>
    public void Clear()
    {
        Enabled = false;
        PreInstructionPointer = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        Timer = 0;
        InitializationParameter = 0;
        Held = 0;
        NewlyPressed = 0;
        PreviousHeld = 0;
        PreviousNewlyPressed = 0;
        PublishedPreviousHeld = 0;
        PublishedPreviousNewlyPressed = 0;
    }

    /// <summary>Models <c>$91:834E</c>; loading and enabling remain separate native steps.</summary>
    public void Enable() => Enabled = true;

    /// <summary>Models <c>$91:835F</c> without destroying the loaded object's RAM.</summary>
    public void Disable() => Enabled = false;

    /// <summary>
    /// Applies an object-specific pre-instruction redirect to the shared object words.
    /// </summary>
    /// <remarks>
    /// Routines such as $91:864F update all three words synchronously. Providing one method
    /// prevents a host callback from publishing a new list with a stale countdown for one
    /// frame, a state the native routine cannot expose.
    /// </remarks>
    public void Redirect(
        ushort preInstructionPointer,
        ushort instructionPointer,
        ushort instructionTimer = 1)
    {
        PreInstructionPointer = preInstructionPointer;
        InstructionPointer = instructionPointer;
        InstructionTimer = instructionTimer;
    }

    /// <summary>
    /// Loads one six-byte bank-$91 object definition as <c>$91:8395</c> does.
    /// </summary>
    public void LoadObject(
        ISnesAddressSpace bus,
        ushort objectPointer,
        ushort initializationParameter = 0,
        Action<DemoInputState, ushort>? initializer = null)
    {
        ArgumentNullException.ThrowIfNull(bus);

        InitializationParameter = initializationParameter;
        ushort initializerPointer = ReadWord(bus, objectPointer);
        PreInstructionPointer = ReadWord(bus, unchecked((ushort)(objectPointer + 2)));
        InstructionPointer = ReadWord(bus, unchecked((ushort)(objectPointer + 4)));
        InstructionTimer = 1;
        Timer = 0;

        if (initializer is not null)
            initializer(this, initializerPointer);
        else if (initializerPointer != NoOpRoutine)
            throw UnsupportedRoutine("initializer", initializerPointer);
    }

    /// <summary>
    /// Executes <c>$91:83C0/$83F2</c> once and publishes the resulting controller words.
    /// </summary>
    public void Step(
        ISnesAddressSpace bus,
        Action<DemoInputState, ushort>? preInstruction = null,
        Func<DemoInputState, ushort, ushort, DemoInputInstructionResult>? specialInstruction = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!Enabled || InstructionPointer == 0)
            return;

        if (preInstruction is not null)
            preInstruction(this, PreInstructionPointer);
        else if (PreInstructionPointer is not (NoOpRoutine or ClearedPreInstructionRoutine))
            throw UnsupportedRoutine("pre-instruction", PreInstructionPointer);

        // DEC is a 16-bit 65C816 operation. In particular, a zero duration wraps and lasts
        // 65,536 handler calls; using an int countdown would quietly change malformed or
        // intentionally unusual ROM data.
        InstructionTimer = unchecked((ushort)(InstructionTimer - 1));
        if (InstructionTimer == 0)
            ProcessInstructionList(bus, specialInstruction);

        // $91:83D3 publishes the old demo words to the drawing-input history before it
        // replaces the live joypad words and remembers the newly produced pair.
        PublishedPreviousHeld = PreviousHeld;
        PublishedPreviousNewlyPressed = PreviousNewlyPressed;
        PreviousHeld = Held;
        PreviousNewlyPressed = NewlyPressed;
    }

    private void ProcessInstructionList(
        ISnesAddressSpace bus,
        Func<DemoInputState, ushort, ushort, DemoInputInstructionResult>? specialInstruction)
    {
        ushort cursor = InstructionPointer;
        while (true)
        {
            ushort word = ReadWord(bus, cursor);
            if ((word & 0x8000) == 0)
            {
                InstructionTimer = word;
                Held = ReadWord(bus, unchecked((ushort)(cursor + 2)));
                NewlyPressed = ReadWord(bus, unchecked((ushort)(cursor + 4)));
                InstructionPointer = unchecked((ushort)(cursor + 6));
                return;
            }

            // Native increments Y past the opcode before dispatching, so every instruction
            // below treats cursor as the first argument and either returns another cursor
            // to the loop or terminates processing in the delete case.
            cursor = unchecked((ushort)(cursor + 2));
            switch (word)
            {
                case DeleteInstruction:
                    InstructionPointer = 0;
                    Held = 0;
                    NewlyPressed = 0;
                    return;

                case SetPreInstruction:
                    PreInstructionPointer = ReadWord(bus, cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                case ClearPreInstruction:
                    PreInstructionPointer = ClearedPreInstructionRoutine;
                    break;

                case GotoInstruction:
                    cursor = ReadWord(bus, cursor);
                    break;

                case DecrementTimerAndGoto:
                    Timer = unchecked((ushort)(Timer - 1));
                    cursor = Timer != 0
                        ? ReadWord(bus, cursor)
                        : unchecked((ushort)(cursor + 2));
                    break;

                case SetTimer:
                    Timer = ReadWord(bus, cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                default:
                    // Demo lists may call arbitrary bank-$91 routines in addition to the
                    // six shared object opcodes. The callback must explicitly identify a
                    // routine and return its post-operand cursor; this prevents a handler
                    // with parameters from accidentally being treated as a no-argument
                    // instruction and desynchronizing the cartridge bytecode stream.
                    DemoInputInstructionResult result = specialInstruction?.Invoke(
                        this,
                        word,
                        cursor) ?? DemoInputInstructionResult.NotHandled(cursor);
                    if (!result.Handled)
                    {
                        // The reusable interpreter cannot infer an object's private
                        // operand width. Its owner must supply the matching callback;
                        // omission is an invalid composition, not an unknown generic opcode.
                        throw new InvalidOperationException(
                            $"Demo-input instruction $91:{word:X4} was not handled by the owning object's callback.");
                    }

                    cursor = result.NextInstructionPointer;
                    if (result.TerminateProcessing)
                    {
                        InstructionPointer = cursor;
                        return;
                    }
                    break;
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort address) =>
        unchecked((ushort)(bus.ReadByte(DemoInputBank | address) |
            (bus.ReadByte(DemoInputBank | unchecked((ushort)(address + 1))) << 8)));

    private static InvalidOperationException UnsupportedRoutine(string kind, ushort pointer) =>
        new($"Demo-input {kind} $91:{pointer:X4} was not supplied by the owning object.");
}

/// <summary>Control-flow result from one explicitly translated demo-list instruction.</summary>
/// <param name="Handled">Whether the callback recognizes the bank-$91 routine pointer.</param>
/// <param name="NextInstructionPointer">Cursor after all operands consumed by that routine.</param>
/// <param name="TerminateProcessing">Whether the generic handler must return immediately.</param>
public readonly record struct DemoInputInstructionResult(
    bool Handled,
    ushort NextInstructionPointer,
    bool TerminateProcessing)
{
    /// <summary>Reports that a callback does not own the encountered routine.</summary>
    public static DemoInputInstructionResult NotHandled(ushort argumentPointer) =>
        new(false, argumentPointer, false);

    /// <summary>Continues decoding at the supplied cursor after a handled instruction.</summary>
    public static DemoInputInstructionResult ContinueAt(ushort nextInstructionPointer) =>
        new(true, nextInstructionPointer, false);

    /// <summary>Stops this handler call after a translated terminating instruction.</summary>
    public static DemoInputInstructionResult TerminateAt(ushort nextInstructionPointer) =>
        new(true, nextInstructionPointer, true);
}
