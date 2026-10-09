using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    // Reference interpreter for cartridge/synthetic input streams. Production attract
    // playback continues to use StockAttractInputPrograms and StepStock.
    /// <summary>Runs the reference attract-demo input interpreter against fixture-backed instruction data.</summary>
    private sealed class ReferenceAttractInput
    {
        /// <summary>Loaded script state, including its current instruction pointer and playback position.</summary>
        public DemoInputState Script { get; } = new();

        /// <summary>Loads the selected scene's input object through the verification fixture and enables playback.</summary>
        /// <param name="bus">Memory bus used to resolve and load the scene's input object.</param>
        /// <param name="scene">Attract scene whose cartridge input-object pointer supplies the script.</param>
        public ReferenceAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
        {
            Script.LoadObject(bus, scene.InputObject, scene.InputObject,
                definitionWord: pointer => ReadDemoFixtureWord(bus, pointer));
            Script.Enable();
        }

        /// <summary>Advances the reference script one input step and handles the supported attract pre-instructions.</summary>
        /// <param name="bus">Memory bus used to read instruction words from the verification fixture.</param>
        /// <param name="gameState">Current game state used to evaluate the demo-exit instruction.</param>
        /// <param name="movementType">Current Samus movement type used to evaluate the shinespark continuation.</param>
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
