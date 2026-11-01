using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Input;

/// <summary>
/// The demo-controller object, running a typed <see cref="DemoInputProgram"/> (#1300).
/// </summary>
/// <remarks>
/// The object's per-frame behaviour step is the owner's: each step hands it to the owner's callback, which may redirect the
/// object, and the program's cues go to the owner as the object's private routines did. The object's own frame
/// hold gates the program, so each input lasts exactly as long as its recorded input.
/// </remarks>
public sealed class DemoInputState
{
    /// <summary>High-bit enable flag set when a demo object is created.</summary>
    public bool Enabled { get; private set; }

    /// <summary>Object-specific routine its owner runs before the shared instruction timer.</summary>
    public DemoInputFrameBehavior FrameBehavior { get; private set; }

    /// <summary>Where the object's program resumes; null once the object is deleted.</summary>
    internal AnimationProgramCursor<DemoInputProgram>? Cursor { get; private set; }

    /// <summary>Frames remaining in the active input record, wrapping at sixteen bits.</summary>
    public WrappingCountdown FrameHold { get; private set; }

    /// <summary>General loop timer of the demo's loop steps; no demo program counts it.</summary>
    public WrappingCountdown Timer { get; private set; }

    /// <summary>Controller word currently replacing player-held input.</summary>
    public ControllerButton Held { get; private set; }

    /// <summary>Controller edge word currently replacing newly pressed input.</summary>
    public ControllerButton NewlyPressed { get; private set; }

    /// <summary>Previous demo-held word retained by the publication seam.</summary>
    public ControllerButton PreviousHeld { get; private set; }

    /// <summary>Previous demo-edge word retained by the publication seam.</summary>
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
        FrameBehavior = DemoInputFrameBehavior.None;
        Cursor = null;
        FrameHold = WrappingCountdown.Start(0);
        Timer = WrappingCountdown.Start(0);
        Held = 0;
        NewlyPressed = 0;
        PreviousHeld = 0;
        PreviousNewlyPressed = 0;
        PublishedPreviousHeld = 0;
        PublishedPreviousNewlyPressed = 0;
    }

    /// <summary>Creates the demo object; loading and enabling remain separate steps.</summary>
    public void Enable() => Enabled = true;

    /// <summary>Disables the demo object without discarding its loaded state.</summary>
    public void Disable() => Enabled = false;

    /// <summary>
    /// Loads an object: its per-frame behaviour step and animation script, with the first record due on the next
    /// step. Every object's initializer does nothing, so no initialization parameter is kept.
    /// </summary>
    public void Load(DemoInputObjectDefinition definition)
    {
        FrameBehavior = definition.FrameBehavior;
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(definition.Program);
        FrameHold = WrappingCountdown.Start(1);
        Timer = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Applies an object-specific pre-instruction redirect to the shared object words.
    /// </summary>
    /// <remarks>
    /// Redirects such as the shinespark demo's update all three words synchronously. Providing one method
    /// prevents a host callback from publishing a new list with a stale countdown for one
    /// frame, a state the game cannot expose.
    /// </remarks>
    public void Redirect(DemoInputFrameBehavior frameBehavior, DemoInputProgram program, int frameHold = 1)
    {
        FrameBehavior = frameBehavior;
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(program);
        FrameHold = WrappingCountdown.FromRaw(frameHold);
    }

    /// <summary>
    /// Runs one demo step and publishes the resulting controller words. The owner runs the object's
    /// pre-instruction in <paramref name="frameBehavior"/> and its private routines in <paramref name="cue"/>.
    /// </summary>
    public void Step(Action<DemoInputState, DemoInputFrameBehavior> frameBehavior, Action<DemoInputCue> cue)
    {
        Ensure.NotNull(frameBehavior);
        Ensure.NotNull(cue);
        if (!Enabled || Cursor is not { } cursor)
            return;

        frameBehavior(this, FrameBehavior);
        cursor = Cursor ?? throw new InvalidOperationException("A demo pre-instruction deleted its object.");

        // DEC is a 16-bit 65C816 operation. In particular, a zero duration wraps and lasts
        // 65,536 handler calls; using an int countdown would quietly change malformed or
        // intentionally unusual recorded input data.
        CountdownStep frameHold = FrameHold.Decrement();
        FrameHold = frameHold.Remaining;
        if (frameHold.IsZero)
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
                    FrameHold = WrappingCountdown.FromRaw(WordWrap.Unsigned(shown.Duration));
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

        // Publishing hands the old demo words to the drawing-input history before it
        // replaces the live joypad words and remembers the newly produced pair.
        PublishedPreviousHeld = PreviousHeld;
        PublishedPreviousNewlyPressed = PreviousNewlyPressed;
        PreviousHeld = Held;
        PreviousNewlyPressed = NewlyPressed;
    }

    /// <summary>
    /// The pre-instruction of an object whose owner has none of its own: only the return-only routine may be installed.
    /// </summary>
    public static void RunReturnOnlyFrameBehavior(DemoInputState _, DemoInputFrameBehavior frameBehavior)
    {
        if (frameBehavior != DemoInputFrameBehavior.ReturnOnly)
            throw new InvalidOperationException($"Demo-input pre-instruction {frameBehavior} was not supplied by the owning object.");
    }
}
