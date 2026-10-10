using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

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
        ArgumentNullException.ThrowIfNull(scene);
        Script.LoadStockAttractObject(scene.InputObject);
        Script.Enable();
    }

    /// <summary>Steps application-owned inputs; no address space is required.</summary>
    public void StepStock(SuperMetroidGameState gameState, SamusMovementType movementType) =>
        Script.StepStockAttract((_, pointer) => ApplyPreInstruction(pointer, gameState, movementType));

    /// <summary>Applies one compiled attract-demo pre-instruction, redirecting the script when its gameplay conditions match.</summary>
    /// <param name="pointer">Bank-$91 pre-instruction identity selected by the running demo script.</param>
    /// <param name="gameState">Current game state used by conditional demo transitions.</param>
    /// <param name="movementType">Current Samus movement mode used by the shinespark branch.</param>
    private void ApplyPreInstruction(ushort pointer, SuperMetroidGameState gameState, SamusMovementType movementType)
    {
        switch (pointer)
        {
            case DemoInputRomData.Routines.NoOp:
            case DemoInputRomData.Routines.ClearedPreInstruction:
                break;
            case DemoInputRomData.Attract.CheckLeave:
                if (gameState == SuperMetroidGameState.TransitionFromDemoB)
                    Script.Redirect(pointer, DemoInputRomData.Attract.DeleteList);
                break;
            case DemoInputRomData.Attract.ShinesparkPreInstruction:
                // The routine's name does not match its type-$1A comparison. Retain
                // the actual cartridge branch rather than correcting that oddity.
                if (movementType != SamusMovementType.DraygonHeld)
                    Script.Redirect(DemoInputRomData.Attract.CheckLeave,
                        DemoInputRomData.Attract.ShinesparkContinuation);
                break;
            default:
                throw new InvalidDataException($"Unknown title-demo pre-instruction $91:{pointer:X4}.");
        }
    }
}
