using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Input;

/// <summary>
/// A demo's recorded-input playback, running a typed <see cref="DemoInputProgram"/> (#1300).
/// </summary>
/// <remarks>
/// The playback holds only the recording: where it is, how long the current input lasts and the controller words it
/// publishes. What a demo watches for before each step (the title leaving, Samus reaching her scripted place) is its
/// owner's own state (#1554): the owner checks it while <see cref="IsPlaying"/>, may switch the playback to another
/// recording, and then steps it; the recording's cues go to the owner. The playback's own frame hold gates the
/// recording, so each input lasts exactly as long as its recorded input.
/// </remarks>
public sealed class DemoInputState
{
    /// <summary>Whether the playback runs: set when a demo starts, cleared when its recording ends the demo's input.</summary>
    public bool Enabled { get; private set; }

    /// <summary>Where the recording resumes; null once the recording has ended.</summary>
    internal AnimationProgramCursor<DemoInputProgram>? Cursor { get; private set; }

    /// <summary>Whether the next <see cref="Step"/> plays the recording: the playback runs and its recording has not ended.</summary>
    public bool IsPlaying => Enabled && Cursor is not null;

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

    /// <summary>The held word published before this step, which Samus's input filter reads as the previous held input.</summary>
    public ControllerButton PublishedPreviousHeld { get; private set; }

    /// <summary>The edge word published before this step, which Samus's input filter reads as the previous new input.</summary>
    public ControllerButton PublishedPreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Clears every demo-input value, as ending a demo's input does.
    /// </summary>
    public void Clear()
    {
        Enabled = false;
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

    /// <summary>Starts the playback; loading and enabling remain separate steps.</summary>
    public void Enable() => Enabled = true;

    /// <summary>Stops the playback without discarding its loaded recording.</summary>
    public void Disable() => Enabled = false;

    /// <summary>Loads <paramref name="program"/>, with its first record due on the next step.</summary>
    public void Load(DemoInputProgram program)
    {
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(program);
        FrameHold = WrappingCountdown.Start(1);
        Timer = WrappingCountdown.Start(0);
    }

    /// <summary>
    /// Switches the playback to <paramref name="program"/>, whose first record is due after <paramref name="frameHold"/>
    /// steps, as an owner's watch does before a step.
    /// </summary>
    /// <remarks>
    /// The recording and its countdown change together, so a switch never publishes the new recording with the old
    /// countdown for one step, a state the game cannot expose.
    /// </remarks>
    public void SwitchTo(DemoInputProgram program, int frameHold = 1)
    {
        Cursor = AnimationProgramCursor<DemoInputProgram>.Start(program);
        FrameHold = WrappingCountdown.FromRaw(frameHold);
    }

    /// <summary>
    /// Plays one step of the recording and publishes the resulting controller words. The owner runs the recording's cues
    /// in <paramref name="cue"/>. The owner checks what it watches for before this step, while <see cref="IsPlaying"/>.
    /// </summary>
    public void Step(Action<DemoInputCue> cue)
    {
        Ensure.NotNull(cue);
        if (!Enabled || Cursor is not { } cursor)
            return;

        // The countdown wraps below zero to 65535. In particular, a zero duration wraps and lasts
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
                    FrameHold = WrappingCountdown.FromRaw(shown.Duration);
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
}
