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
    public DemoInputState Script { get; } = new();

    /// <summary>Production title playback uses compiled input definitions, not ROM bytecode.</summary>
    public AttractDemoInput(AttractDemoScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        Script.LoadStockAttractObject(scene.InputObject);
        Script.Enable();
    }

    /// <summary>Reference path for cartridge and synthetic-program verification.</summary>
    public AttractDemoInput(ISnesAddressSpace bus, AttractDemoScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        Script.LoadObject(bus, scene.InputObject, scene.InputObject);
        Script.Enable();
    }

    /// <summary>Executes the cartridge pre-instruction before this frame's input record.</summary>
    public void Step(ISnesAddressSpace bus, SuperMetroidGameState gameState, SamusMovementType movementType)
    {
        Script.Step(bus, (_, pointer) => ApplyPreInstruction(pointer, gameState, movementType));
    }

    /// <summary>Steps application-owned inputs; no address space is required.</summary>
    public void StepStock(SuperMetroidGameState gameState, SamusMovementType movementType) =>
        Script.StepStockAttract((_, pointer) => ApplyPreInstruction(pointer, gameState, movementType));

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
