using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static FrontendVerification;

/// <summary>Represents attract input fixture verification.</summary>
internal static class AttractInputFixtureVerification
{
    // Reference interpreter for cartridge/synthetic input streams. Production attract
    // playback continues to use StockAttractInputPrograms and StepStock.
    /// <summary>Represents reference attract input.</summary>
    internal sealed class ReferenceAttractInput
    {
        /// <summary>Gets script.</summary>
        public DemoInputState Script { get; } = new();

        /// <summary>Initializes a new reference attract input instance.</summary>
        public ReferenceAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
        {
            Script.LoadObject(bus, scene.InputObject, scene.InputObject,
                definitionWord: pointer => ReadDemoFixtureWord(bus, pointer));
            Script.Enable();
        }

        /// <summary>Step.</summary>
        public void Step(ISnesAddressSpace bus, SuperMetroidGameState gameState,
            SamusMovementType movementType)
        {
            Script.Step(bus, new ReferenceAttractProgram(bus, gameState, movementType));
        }
    }

    /// <summary>The attract scene's reference owner for one frame: fixture words and the scene's pre-instructions (#633).</summary>
    internal sealed class ReferenceAttractProgram(ISnesAddressSpace bus, SuperMetroidGameState gameState,
        SamusMovementType movementType) : IDemoInputProgram
    {
        /// <summary>Reads instruction word.</summary>
        public ushort ReadInstructionWord(ushort pointer) => ReadDemoFixtureWord(bus, pointer);

        /// <summary>Tries to run pre instruction.</summary>
        public bool TryRunPreInstruction(DemoInputState state, ushort pointer)
        {
            switch (ClosedNativeWords.Decode<AttractDemoPreInstruction>(pointer, "reference demo pre-instruction"))
            {
                case AttractDemoPreInstruction.NoOp:
                case AttractDemoPreInstruction.Cleared:
                    break;
                case AttractDemoPreInstruction.CheckLeave:
                    if (gameState == SuperMetroidGameState.TransitionFromDemoB)
                        state.Redirect(pointer, DemoInputRomData.Attract.DeleteList);
                    break;
                case AttractDemoPreInstruction.Shinespark:
                    if (movementType != SamusMovementType.DraygonHeld)
                        state.Redirect((ushort)AttractDemoPreInstruction.CheckLeave,
                            DemoInputRomData.Attract.ShinesparkContinuation);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(AttractDemoPreInstruction)} {pointer:X4}.");
            }
            return true;
        }

        /// <summary>Runs special instruction.</summary>
        public DemoInputInstructionResult RunSpecialInstruction(DemoInputState demo, ushort opcode, ushort cursor) =>
            DemoInputInstructionResult.NotHandled(cursor);
    }
}