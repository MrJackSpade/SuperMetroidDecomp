using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
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
        Script.Load(scene.Input.Program);
        Script.Enable();
    }

    /// <summary>
    /// Checks what the playback watches for while it plays, then plays one step. The check runs first, in the same step,
    /// so leaving the title ends the input before its next record and the shinespark's release continues it at once.
    /// </summary>
    public void StepStock(SuperMetroidGameState gameState, SamusMovementType movementType)
    {
        if (Script.IsPlaying)
            CheckWatch(gameState, movementType);
        Script.Step(cue => throw new InvalidOperationException($"Title demonstrations call no {cue} routine."));
    }

    /// <summary>Ends the input when the title leaves, or releases the shinespark using the game's actual Draygon-grab comparison.</summary>
    private void CheckWatch(SuperMetroidGameState gameState, SamusMovementType movementType)
    {
        switch (Watch)
        {
            case AttractDemoInputWatch.TitleLeaving:
                if (gameState == SuperMetroidGameState.RoutingAfterDemo)
                    Script.SwitchTo(DemoInputRecordingId.AttractDelete);
                break;
            case AttractDemoInputWatch.ShinesparkRelease:
                // The release's condition does not match the shinespark the recording shows: it tests the Draygon grab.
                // Retain the actual branch rather than correcting that oddity.
                if (movementType != SamusMovementType.DraygonHeld)
                {
                    Watch = AttractDemoInputWatch.TitleLeaving;
                    Script.SwitchTo(DemoInputRecordingId.AttractDiagonalShinesparkContinuation);
                }
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(AttractDemoInputWatch)} {Watch}.");
        }
    }
}
