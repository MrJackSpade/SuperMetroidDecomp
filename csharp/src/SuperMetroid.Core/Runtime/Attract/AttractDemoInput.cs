using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// Title owner for compiled stock input programs and their ROM-backed verification path.
/// Held and newly-pressed words are independent; deriving edges loses scripted edges.
/// </summary>
public sealed class AttractDemoInput
{
    /// <summary>Owned, mutable demo-script execution state, loaded and enabled for the constructor's scene; the same instance is returned rather than a snapshot.</summary>
    /// <remarks><see cref="StepStock"/> advances its instruction timers and independently authored held/newly-pressed controller words, which the gameplay runtime consumes without deriving new edges.</remarks>
    public DemoInputState Script { get; } = new();

    /// <summary>Production title playback uses compiled input definitions, not ROM bytecode.</summary>
    public AttractDemoInput(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        Script.Load(scene.InputObject);
        Script.Enable();
    }

    /// <summary>Steps application-owned inputs; no address space is required.</summary>
    public void StepStock(SuperMetroidGameState gameState, SamusMovementType movementType) =>
        Script.Step((_, frameBehavior) => ApplyFrameBehavior(frameBehavior, gameState, movementType),
            cue => throw new InvalidOperationException($"Title demonstrations call no {cue} routine."));

    /// <summary>Redirects stock demo input at leave and movement branches using the cartridge's actual type-$1A comparison.</summary>
    private void ApplyFrameBehavior(DemoInputFrameBehavior frameBehavior, SuperMetroidGameState gameState, SamusMovementType movementType)
    {
        switch (frameBehavior)
        {
            case DemoInputFrameBehavior.ReturnOnly:
                break;
            case DemoInputFrameBehavior.AttractCheckLeave:
                if (gameState == SuperMetroidGameState.TransitionFromDemoB)
                    Script.Redirect(frameBehavior, DemoInputProgram.AttractDelete);
                break;
            case DemoInputFrameBehavior.AttractShinespark:
                // The routine's name does not match its type-$1A comparison. Retain
                // the actual cartridge branch rather than correcting that oddity.
                if (movementType != SamusMovementType.DraygonHeld)
                    Script.Redirect(DemoInputFrameBehavior.AttractCheckLeave,
                        DemoInputProgram.AttractDiagonalShinesparkContinuation);
                break;
            case DemoInputFrameBehavior.None:
            case DemoInputFrameBehavior.IntroBabyRunningLeft:
            case DemoInputFrameBehavior.IntroBabyStopAndLook:
                throw new InvalidOperationException($"A title demonstration's input object cannot hold the {frameBehavior} pre-instruction.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(DemoInputFrameBehavior)} {frameBehavior}.");
        }
    }
}
