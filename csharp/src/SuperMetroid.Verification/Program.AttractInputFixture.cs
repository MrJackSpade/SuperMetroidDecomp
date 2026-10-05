using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    // Reference interpreter for cartridge/synthetic input streams. Production attract
    // playback continues to use StockAttractInputPrograms and StepStock.
    private sealed class ReferenceAttractInput
    {
        public DemoInputState Script { get; } = new();

        public ReferenceAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
        {
            Script.LoadObject(bus, scene.InputObject, scene.InputObject,
                definitionWord: pointer => ReadDemoFixtureWord(bus, pointer));
            Script.Enable();
        }

        public void Step(ISnesAddressSpace bus, SuperMetroidGameState gameState,
            SamusMovementType movementType)
        {
            Script.Step(bus, (state, pointer) =>
            {
                switch (pointer)
                {
                    case DemoInputRomData.Routines.NoOp:
                    case DemoInputRomData.Routines.ClearedPreInstruction:
                        break;
                    case DemoInputRomData.Attract.CheckLeave:
                        if (gameState == SuperMetroidGameState.TransitionFromDemoB)
                            state.Redirect(pointer, DemoInputRomData.Attract.DeleteList);
                        break;
                    case DemoInputRomData.Attract.ShinesparkPreInstruction:
                        if (movementType != SamusMovementType.DraygonHeld)
                            state.Redirect(DemoInputRomData.Attract.CheckLeave,
                                DemoInputRomData.Attract.ShinesparkContinuation);
                        break;
                    default:
                        throw new InvalidDataException($"Unknown reference demo pre-instruction $91:{pointer:X4}.");
                }
            }, instructionWord: pointer => ReadDemoFixtureWord(bus, pointer));
        }
    }
}