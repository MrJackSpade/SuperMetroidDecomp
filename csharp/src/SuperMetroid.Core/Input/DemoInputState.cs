namespace SuperMetroid.Core.Input;

/// <summary>
/// A demo's recorded-input playback, playing a <see cref="DemoRecording"/> (#1300, #1600).
/// </summary>
/// <remarks>
/// The playback holds only the recording: where it is, how long the current input lasts and the buttons it presses in
/// the player's place. What a demo watches for before each step (the title leaving, Samus reaching her scripted place) is its
/// owner's own state (#1554): the owner checks it while <see cref="IsPlaying"/>, may switch the playback to another
/// recording, and then steps it; the recording's cues go to the owner. The playback's own frame hold gates the
/// recording, so each input lasts exactly as long as its recorded input.
/// </remarks>
public sealed class DemoInputState
{
    /// <summary>Whether the playback runs: set when a demo starts, cleared when its recording ends the demo's input.</summary>
    public bool Enabled { get; private set; }

    /// <summary>Where the recording resumes; null once the recording has ended.</summary>
    internal DemoPlaybackPosition? Position { get; private set; }

    /// <summary>Whether the next <see cref="Step"/> plays the recording: the playback runs and its recording has not ended.</summary>
    public bool IsPlaying => Enabled && Position is not null;

    /// <summary>Frames remaining in the active input record, wrapping at sixteen bits.</summary>
    public WrappingCountdown FrameHold { get; private set; }

    /// <summary>The buttons the demo currently holds in place of the player's held buttons.</summary>
    public ControllerButton Held { get; private set; }

    /// <summary>The buttons the demo currently presses anew in place of the player's newly pressed buttons.</summary>
    public ControllerButton NewlyPressed { get; private set; }

    /// <summary>The buttons the demo held on the previous step.</summary>
    public ControllerButton PreviousHeld { get; private set; }

    /// <summary>The buttons the demo newly pressed on the previous step.</summary>
    public ControllerButton PreviousNewlyPressed { get; private set; }

    /// <summary>The newly pressed buttons pressed before this step, which Samus's input filter reads as the previous new input.</summary>
    public ControllerButton PublishedPreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Clears every demo-input value, as ending a demo's input does.
    /// </summary>
    public void Clear()
    {
        Enabled = false;
        Position = null;
        FrameHold = WrappingCountdown.Start(0);
        Held = 0;
        NewlyPressed = 0;
        PreviousHeld = 0;
        PreviousNewlyPressed = 0;
        PublishedPreviousNewlyPressed = 0;
    }

    /// <summary>Starts the playback; loading and enabling remain separate steps.</summary>
    public void Enable() => Enabled = true;

    /// <summary>Stops the playback without discarding its loaded recording.</summary>
    public void Disable() => Enabled = false;

    /// <summary>Loads <paramref name="recording"/>, with its first input due on the next step.</summary>
    public void Load(DemoInputRecordingId recording)
    {
        Position = DemoPlaybackPosition.Start(recording);
        FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Switches the playback to <paramref name="recording"/>, whose first input is due after <paramref name="frameHold"/>
    /// steps, as an owner's watch does before a step.
    /// </summary>
    /// <remarks>
    /// The recording and its countdown change together, so a switch never publishes the new recording with the old
    /// countdown for one step, a state the game cannot expose.
    /// </remarks>
    public void SwitchTo(DemoInputRecordingId recording, int frameHold = 1)
    {
        Position = DemoPlaybackPosition.Start(recording);
        FrameHold = WrappingCountdown.Start(frameHold);
    }

    /// <summary>
    /// Plays one step of the recording and presses the resulting buttons in the player's place. The owner runs the recording's cues
    /// in <paramref name="cue"/>. The owner checks what it watches for before this step, while <see cref="IsPlaying"/>.
    /// </summary>
    public void Step(Action<DemoInputCue> cue)
    {
        Ensure.NotNull(cue);
        if (!Enabled || Position is not { } position)
            return;

        // The countdown wraps below zero to 65535. In particular, a zero duration wraps and lasts
        // 65,536 handler calls; using an int countdown would quietly change malformed or
        // intentionally unusual recorded input data.
        CountdownStep frameHold = FrameHold.Decrement();
        FrameHold = frameHold.Remaining;
        if (frameHold.IsZero)
            PlayNextInput(position, cue);

        // Pressing hands the demo's previous buttons to the drawing-input history before it
        // replaces the live held and newly pressed buttons and remembers the new ones.
        PublishedPreviousNewlyPressed = PreviousNewlyPressed;
        PreviousHeld = Held;
        PreviousNewlyPressed = NewlyPressed;
    }

    /// <summary>
    /// Plays the input <paramref name="position"/> names, or ends the recording as it says once its inputs have run out:
    /// a loop plays its loop input at once, and a signal reaches the owner in <paramref name="cue"/> before the input ends.
    /// </summary>
    private void PlayNextInput(DemoPlaybackPosition position, Action<DemoInputCue> cue)
    {
        DemoRecording recording = DemoInputRecordings.Of(position.Recording);
        if (position.NextRecord >= recording.Records.Count)
        {
            switch (recording.End)
            {
                case DemoRecordingEnd.Loop loop:
                    position = position with { NextRecord = loop.Record };
                    break;
                case DemoRecordingEnd.Signal signal:
                    cue(signal.Cue);
                    EndInput();
                    return;
                case DemoRecordingEnd.Stop:
                    EndInput();
                    return;
                default:
                    throw new InvalidOperationException($"Demo recording {position.Recording} has undefined end {recording.End}.");
            }
        }

        DemoInputRecord input = recording.Records[position.NextRecord];
        FrameHold = WrappingCountdown.Start(input.Duration);
        Held = input.Held;
        NewlyPressed = input.NewlyPressed;
        Position = position with { NextRecord = position.NextRecord + 1 };
    }

    /// <summary>Ends the demo's input: the recording is done and the demo releases every button.</summary>
    private void EndInput()
    {
        Position = null;
        Held = 0;
        NewlyPressed = 0;
    }
}
