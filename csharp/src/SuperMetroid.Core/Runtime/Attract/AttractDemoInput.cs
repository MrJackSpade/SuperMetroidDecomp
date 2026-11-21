using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// A title demonstration's input: its recorded-input playback, which a demonstration whose input names a continuation
/// continues into on its first step that plays. Held and newly-pressed buttons are independent; deriving edges loses
/// scripted edges.
/// </summary>
public sealed partial class AttractDemoInput
{
    /// <summary>
    /// The recording the playback continues into on its first step that plays (the diagonal shinespark's, #2190), until
    /// that step starts it; null once started, or for a demonstration that plays one recording.
    /// </summary>
    private DemoInputRecordingId? _continuation;

    /// <summary>Owned, mutable recorded-input playback, loaded and enabled for the constructor's scene; the same instance is returned rather than a snapshot.</summary>
    /// <remarks><see cref="Step"/> advances its record timers and independently authored held and newly pressed buttons, which the gameplay runtime consumes without deriving new edges.</remarks>
    public DemoInputState Script { get; } = new();

    /// <summary>Starts the scene's recording, with the continuation its input names still to start.</summary>
    public AttractDemoInput(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        _continuation = scene.Input.Continuation;
        Script.Load(scene.Input.Recording);
        Script.Enable();
    }

    /// <summary>
    /// Plays one step. On the first step that plays, a demonstration whose input names a continuation switches to it
    /// before the step plays, so the shinespark's release continues the input at once: Samus is never held by Draygon in
    /// the shinespark's demonstration, so nothing delays the release. The input ends with the demonstration's gameplay.
    /// </summary>
    public void Step()
    {
        if (Script.IsPlaying && _continuation is { } continuation)
        {
            _continuation = null;
            Script.Load(continuation);
        }
        Script.Step(cue => throw new InvalidOperationException($"Title demonstrations handle no {cue} cue."));
    }
}
