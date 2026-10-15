using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Input;

/// <summary>
/// Bank-$91's demo-controller object (<c>$91:834E..8463</c>), running a typed <see cref="DemoInputProgram"/> (#1300).
/// </summary>
/// <remarks>
/// The object's pre-instruction is the owner's: each step hands it to the owner's callback, which may redirect the
/// object, and the program's cues go to the owner as the object's private routines did. The object's own instruction
/// timer gates the program, so each input lasts exactly as long as its native record.
/// </remarks>
public sealed class DemoInputState
{
    /// <summary>High-bit enable flag installed by <c>$91:834E</c>.</summary>
    public bool Enabled { get; private set; }

    /// <summary>Object-specific routine called before the shared instruction timer.</summary>
    public ushort PreInstructionPointer { get; private set; }

    /// <summary>Where the object's program resumes; null once the object is deleted.</summary>
    internal AnimationProgramCursor<DemoInputProgram>? Cursor { get; private set; }

    /// <summary>Whether the object still runs a program.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>Frames remaining in the active input record, with native word wrapping.</summary>
    public NativeCountdown InstructionTimer { get; private set; }

    /// <summary>General loop timer of instructions <c>$844F/$8459</c>; no demo program counts it.</summary>
    public NativeCountdown Timer { get; private set; }

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
        Cursor = null;
        InstructionTimer = NativeCountdown.Start(0);
        Timer = NativeCountdown.Start(0);
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
    /// Loads an object as <c>$91:8395</c> does: its pre-instruction and program, with the first record due on the next
    /// step. Every object's initializer is the return-only <c>$91:83BF</c>.
    /// </summary>
    public void Load(DemoInputObjectDefinition definition, ushort initializationParameter)
    {
        InitializationParameter = initializationParameter;
        PreInstructionPointer = definition.PreInstruction;
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(definition.Program);
        InstructionTimer = NativeCountdown.Start(1);
        Timer = NativeCountdown.Start(0);
    }

    /// <summary>
    /// Applies an object-specific pre-instruction redirect to the shared object words.
    /// </summary>
    /// <remarks>
    /// Routines such as $91:864F update all three words synchronously. Providing one method
    /// prevents a host callback from publishing a new list with a stale countdown for one
    /// frame, a state the native routine cannot expose.
    /// </remarks>
    public void Redirect(ushort preInstructionPointer, DemoInputProgram program, ushort instructionTimer = 1)
    {
        PreInstructionPointer = preInstructionPointer;
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(program);
        InstructionTimer = NativeCountdown.FromRaw(instructionTimer);
    }

    /// <summary>
    /// Executes <c>$91:83C0/$83F2</c> once and publishes the resulting controller words. The owner runs the object's
    /// pre-instruction in <paramref name="preInstruction"/> and its private routines in <paramref name="cue"/>.
    /// </summary>
    public void Step(Action<DemoInputState, ushort> preInstruction, Action<DemoInputCue> cue)
    {
        Ensure.NotNull(preInstruction);
        Ensure.NotNull(cue);
        if (!Enabled || Cursor is not { } cursor)
            return;

        preInstruction(this, PreInstructionPointer);
        cursor = Cursor ?? throw new InvalidOperationException("A demo pre-instruction deleted its object.");

        // DEC is a 16-bit 65C816 operation. In particular, a zero duration wraps and lasts
        // 65,536 handler calls; using an int countdown would quietly change malformed or
        // intentionally unusual ROM data.
        NativeWordCounterStep instructionTimer = InstructionTimer.Decrement();
        InstructionTimer = instructionTimer.Remaining;
        if (instructionTimer.IsZero)
        {
            NativeCountdown timer = Timer;
            AnimationProgramAdvance<DemoInputFrame> advance = DemoInputAnimations.Programs.Advance(ref cursor, ref timer,
                (published, _) =>
                {
                    cue(published);
                    return null;
                });
            Timer = timer;
            switch (advance.Kind)
            {
                case AnimationProgramAdvanceKind.Show:
                    InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                    Held = (ushort)advance.Frame.Held;
                    NewlyPressed = (ushort)advance.Frame.NewlyPressed;
                    Cursor = cursor;
                    break;
                case AnimationProgramAdvanceKind.Stop:
                    Cursor = null;
                    Held = 0;
                    NewlyPressed = 0;
                    break;
                default:
                    throw new InvalidOperationException($"Demo-input program {cursor.Program} ended its advance with {advance.Kind}.");
            }
        }

        // $91:83D3 publishes the old demo words to the drawing-input history before it
        // replaces the live joypad words and remembers the newly produced pair.
        PublishedPreviousHeld = PreviousHeld;
        PublishedPreviousNewlyPressed = PreviousNewlyPressed;
        PreviousHeld = Held;
        PreviousNewlyPressed = NewlyPressed;
    }

    /// <summary>
    /// The pre-instruction of an object whose owner has none of its own: only the return-only routines, the object
    /// definition's <c>$91:83BF</c> and the cleared <c>$91:8447</c>, may be installed.
    /// </summary>
    public static void RunReturnOnlyPreInstruction(DemoInputState _, ushort preInstruction)
    {
        if (!Enum.IsDefined((ReturnOnlyPreInstruction)preInstruction))
            throw new InvalidOperationException($"Demo-input pre-instruction $91:{preInstruction:X4} was not supplied by the owning object.");
    }

    /// <summary>The return-only pre-instructions an object without its own may hold.</summary>
    private enum ReturnOnlyPreInstruction : ushort
    {
        /// <summary>$91:83BF, installed by the demo object definition.</summary>
        NoOp = DemoInputRomData.Routines.NoOp,
        /// <summary>$91:8447, installed when the pre-instruction is cleared.</summary>
        Cleared = DemoInputRomData.Routines.ClearedPreInstruction,
    }
}
