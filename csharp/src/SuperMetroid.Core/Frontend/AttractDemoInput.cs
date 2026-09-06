using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Title owner for the shared bank-$91 demo-input interpreter. Current and newly-pressed
/// words come independently from ROM; deriving edges from held input loses scripted edges.
/// </summary>
public sealed class AttractDemoInput
{
    public DemoInputState Script { get; } = new();

    public AttractDemoInput(ISnesAddressSpace bus, AttractDemoScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        Script.LoadObject(bus, scene.InputObject, scene.InputObject);
        Script.Enable();
    }

    /// <summary>Executes the cartridge pre-instruction before this frame's input record.</summary>
    public void Step(ISnesAddressSpace bus, SuperMetroidGameState gameState, SamusMovementType movementType)
    {
        Script.Step(bus, (_, pointer) =>
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
        });
    }
}
