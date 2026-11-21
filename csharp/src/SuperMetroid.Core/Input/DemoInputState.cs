namespace SuperMetroid.Core.Input;

/// <summary>
/// A demo's recorded-input playback, playing a <see cref="DemoRecording"/> (#1300, #1600).
/// </summary>
/// <remarks>
/// The playback holds only the recording: where it is, how long the current input lasts and the actions it presses in
/// the player's place. Its owner may switch it to another recording before a step while <see cref="IsPlaying"/> (the
/// diagonal shinespark's demonstration continues into its second recording on its first step that plays), and then
/// steps it; the cue a recording gives as it ends goes to the owner (an intro scene ends on it). The playback's own frame
/// hold gates the recording, so each input lasts exactly as long as its recorded input.
/// </remarks>
public sealed class DemoInputState
{
    /// <summary>Whether the playback runs: set when a demo starts, cleared when its recording ends the demo's input.</summary>
    public bool Enabled { get; private set; }

    /// <summary>Where the recording resumes; null once the recording has ended.</summary>
    internal DemoPlaybackPosition? Position { get; set; }

    /// <summary>Whether the next <see cref="Step"/> plays the recording: the playback runs and its recording has not ended.</summary>
    public bool IsPlaying => Enabled && Position is not null;

    /// <summary>
    /// The active demo input's hold, counted once each step: running while the recording plays, since every recorded input
    /// lasts at least one update (<see cref="DemoInput.Duration"/>), and over once the recording has ended or been cleared.
    /// </summary>
    private readonly UpdateCountdown _frameHold = UpdateCountdown.Ended();

    /// <summary>The actions the demo newly pressed on the previous step, handed to the drawing-input history on the next.</summary>
    private GameplayAction _previousNewlyPressed;

    /// <summary>The actions the demo currently holds in place of the player's held actions.</summary>
    public GameplayAction Held { get; private set; }

    /// <summary>The actions the demo currently presses anew in place of the player's newly pressed actions.</summary>
    public GameplayAction NewlyPressed { get; private set; }

    /// <summary>The newly pressed actions pressed before this step, which Samus's input filter reads as the previous new input.</summary>
    public GameplayAction PublishedPreviousNewlyPressed { get; private set; }

    /// <summary>
    /// Clears every demo-input value, as ending a demo's input does.
    /// </summary>
    public void Clear()
    {
        Enabled = false;
        Position = null;
        _frameHold.End();
        Held = GameplayAction.None;
        NewlyPressed = GameplayAction.None;
        _previousNewlyPressed = GameplayAction.None;
        PublishedPreviousNewlyPressed = GameplayAction.None;
    }

    /// <summary>Starts the playback; loading and enabling remain separate steps.</summary>
    public void Enable() => Enabled = true;

    /// <summary>Stops the playback without discarding its loaded recording.</summary>
    public void Disable() => Enabled = false;

    /// <summary>
    /// Loads <paramref name="recording"/>, with its first input due on the next step: the playback's first recording, or
    /// the one its owner switches it to before a step.
    /// </summary>
    /// <remarks>
    /// The recording and its countdown change together, so a switch never publishes the new recording with the old
    /// countdown for one step, a state the game cannot expose.
    /// </remarks>
    public void Load(DemoInputRecordingId recording)
    {
        Position = DemoPlaybackPosition.Start(recording);
        _frameHold.Restart(1);
    }

    /// <summary>
    /// Plays one step of the recording and presses the resulting actions in the player's place. The owner acts on the cue
    /// a recording gives as it ends in <paramref name="cue"/>.
    /// </summary>
    public void Step(Action<DemoInputCue> cue)
    {
        Ensure.NotNull(cue);
        if (!Enabled || Position is not { } position)
            return;

        if (_frameHold.StepEnds())
            PlayNextInput(position, cue);
        RememberPressed();
    }

    /// <summary>
    /// Hands the demo's previous actions to the drawing-input history and remembers the actions this step pressed, as
    /// every step that plays ends.
    /// </summary>
    private void RememberPressed()
    {
        PublishedPreviousNewlyPressed = _previousNewlyPressed;
        _previousNewlyPressed = NewlyPressed;
    }

    /// <summary>
    /// Plays the input <paramref name="position"/> names, or ends the recording as it says once its inputs have run out:
    /// a held last input plays again at once, and a signal reaches the owner in <paramref name="cue"/> before the input ends.
    /// </summary>
    private void PlayNextInput(DemoPlaybackPosition position, Action<DemoInputCue> cue)
    {
        DemoRecording recording = DemoInputRecordings.Of(position.Recording);
        if (position.NextInput >= recording.Inputs.Count)
        {
            switch (recording.End)
            {
                case DemoRecordingEnd.HoldLastInput:
                    position = position with { NextInput = recording.Inputs.Count - 1 };
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

        DemoInput input = recording.Inputs[position.NextInput];
        _frameHold.Restart(input.Duration);
        Held = input.Held;
        NewlyPressed = input.NewlyPressed;
        Position = position with { NextInput = position.NextInput + 1 };
    }

    /// <summary>Ends the demo's input: the recording is done and the demo releases every action.</summary>
    private void EndInput()
    {
        Position = null;
        Held = GameplayAction.None;
        NewlyPressed = GameplayAction.None;
    }
}
