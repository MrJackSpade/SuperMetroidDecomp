using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// A title demonstration's input: its recorded-input playback and what the playback watches for before each step.
/// Held and newly-pressed buttons are independent; deriving edges loses scripted edges.
/// </summary>
public sealed class AttractDemoInput
{
    /// <summary>Owned, mutable recorded-input playback, loaded and enabled for the constructor's scene; the same instance is returned rather than a snapshot.</summary>
    /// <remarks><see cref="StepStock"/> advances its record timers and independently authored held and newly pressed buttons, which the gameplay runtime consumes without deriving new edges.</remarks>
    public DemoInputState Script { get; } = new();

    /// <summary>What the playback watches for before its next step.</summary>
    public AttractDemoInputWatch Watch { get; private set; }

    /// <summary>Starts the scene's recording, watching for what the scene's input first watches for.</summary>
    public AttractDemoInput(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        Watch = scene.Input.FirstWatch;
        Script.Load(scene.Input.Animation);
        Script.Enable();
    }

    /// <summary>
    /// Checks what the playback watches for while it plays, then plays one step. The check runs first, in the same step,
    /// so the shinespark's release continues the input at once.
    /// </summary>
    public void StepStock()
    {
        if (Script.IsPlaying)
            CheckWatch();
        Script.Step(cue => throw new InvalidOperationException($"Title demonstrations handle no {cue} cue."));
    }

    /// <summary>Continues the shinespark's input into its continuation once it is released.</summary>
    private void CheckWatch()
    {
        switch (Watch)
        {
            case AttractDemoInputWatch.TitleLeaving:
                // The playback steps only while the demo plays, never once the demo set has begun returning to the
                // title, so the title never leaves while this watch checks; the input ends with the demo's gameplay.
                break;
            case AttractDemoInputWatch.ShinesparkRelease:
                // Samus is never held by Draygon in the shinespark's demonstration, so its release is taken on the first
                // step that plays.
                Watch = AttractDemoInputWatch.TitleLeaving;
                Script.SwitchTo(DemoInputRecordingId.AttractDiagonalShinesparkContinuation);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(AttractDemoInputWatch)} {Watch}.");
        }
    }
}
