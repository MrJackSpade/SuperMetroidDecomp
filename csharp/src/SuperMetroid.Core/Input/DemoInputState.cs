using SuperMetroid.Core.Programs;

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

    /// <summary>Object-specific routine its owner runs before the shared instruction timer.</summary>
    public DemoInputPreInstruction PreInstruction { get; private set; }

    /// <summary>Where the object's program resumes; null once the object is deleted.</summary>
    internal AnimationProgramCursor<DemoInputProgram>? Cursor { get; private set; }

    /// <summary>Frames remaining in the active input record, with native word wrapping.</summary>
    public WrappingCountdown InstructionTimer { get; private set; }

    /// <summary>General loop timer of instructions <c>$844F/$8459</c>; no demo program counts it.</summary>
    public WrappingCountdown Timer { get; private set; }

    /// <summary>Controller word currently replacing player-held input.</summary>
    public ControllerButton Held { get; private set; }

    /// <summary>Controller edge word currently replacing newly pressed input.</summary>
    public ControllerButton NewlyPressed { get; private set; }

    /// <summary>Previous demo-held word retained by <c>$91:83D3</c>'s publication seam.</summary>
    public ControllerButton PreviousHeld { get; private set; }

    /// <summary>Previous demo-edge word retained by <c>$91:83D9</c>'s publication seam.</summary>
    public ControllerButton PreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Prior held word published to <c>joypad1_input_samusfilter</c> for this handler call.
    /// </summary>
    public ControllerButton PublishedPreviousHeld { get; private set; }

    /// <summary>
    /// Prior edge word published to <c>joypad1_newinput_samusfilter</c> for this call.
    /// </summary>
    public ControllerButton PublishedPreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Clears every demo-input value, as ending a demo's input does.
    /// </summary>
    public void Clear()
    {
        Enabled = false;
        PreInstruction = DemoInputPreInstruction.None;
        Cursor = null;
        InstructionTimer = WrappingCountdown.Start(0);
        Timer = WrappingCountdown.Start(0);
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
    /// step. Every object's initializer is the return-only <c>$91:83BF</c>, so no initialization parameter is kept.
    /// </summary>
    public void Load(DemoInputObjectDefinition definition)
    {
        PreInstruction = definition.PreInstruction;
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(definition.Program);
        InstructionTimer = WrappingCountdown.Start(1);
        Timer = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Applies an object-specific pre-instruction redirect to the shared object words.
    /// </summary>
    /// <remarks>
    /// Routines such as $91:864F update all three words synchronously. Providing one method
    /// prevents a host callback from publishing a new list with a stale countdown for one
    /// frame, a state the native routine cannot expose.
    /// </remarks>
    public void Redirect(DemoInputPreInstruction preInstruction, DemoInputProgram program, int instructionTimer = 1)
    {
        PreInstruction = preInstruction;
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(program);
        InstructionTimer = WrappingCountdown.FromRaw(instructionTimer);
    }

    /// <summary>
    /// Executes <c>$91:83C0/$83F2</c> once and publishes the resulting controller words. The owner runs the object's
    /// pre-instruction in <paramref name="preInstruction"/> and its private routines in <paramref name="cue"/>.
    /// </summary>
    public void Step(Action<DemoInputState, DemoInputPreInstruction> preInstruction, Action<DemoInputCue> cue)
    {
        Ensure.NotNull(preInstruction);
        Ensure.NotNull(cue);
        if (!Enabled || Cursor is not { } cursor)
            return;

        preInstruction(this, PreInstruction);
        cursor = Cursor ?? throw new InvalidOperationException("A demo pre-instruction deleted its object.");

        // DEC is a 16-bit 65C816 operation. In particular, a zero duration wraps and lasts
        // 65,536 handler calls; using an int countdown would quietly change malformed or
        // intentionally unusual ROM data.
        CountdownStep instructionTimer = InstructionTimer.Decrement();
        InstructionTimer = instructionTimer.Remaining;
        if (instructionTimer.IsZero)
        {
            WrappingCountdown timer = Timer;
            AnimationProgramAdvance<DemoInputFrame> advance = DemoInputAnimations.Programs.Advance(ref cursor, ref timer,
                (published, _) =>
                {
                    cue(published);
                    return null;
                });
            Timer = timer;
            switch (advance)
            {
                case AnimationProgramAdvance<DemoInputFrame>.Show shown:
                    InstructionTimer = WrappingCountdown.FromRaw(WordWrap.Unsigned(shown.Duration));
                    Held = shown.Frame.Held;
                    NewlyPressed = shown.Frame.NewlyPressed;
                    Cursor = cursor;
                    break;
                case AnimationProgramAdvance<DemoInputFrame>.Stop:
                    Cursor = null;
                    Held = 0;
                    NewlyPressed = 0;
                    break;
                default:
                    throw new InvalidOperationException($"Demo-input program {cursor.Program} ended its advance with {advance}.");
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
    /// The pre-instruction of an object whose owner has none of its own: only the return-only routine may be installed.
    /// </summary>
    public static void RunReturnOnlyPreInstruction(DemoInputState _, DemoInputPreInstruction preInstruction)
    {
        if (preInstruction != DemoInputPreInstruction.ReturnOnly)
            throw new InvalidOperationException($"Demo-input pre-instruction {preInstruction} was not supplied by the owning object.");
    }
}
