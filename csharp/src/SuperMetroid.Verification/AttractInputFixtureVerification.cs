using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static FrontendVerification;
using SuperMetroid.Core.Frontend.Attract;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Movement;

/// <summary>Provides a cartridge-backed interpreter for comparing compiled attract-demo input playback.</summary>
internal static class AttractInputFixtureVerification
{
    /// <summary>Interprets cartridge or synthetic demo words as the independent reference input stream.</summary>
    /// <remarks>Production playback uses <c>StockAttractInputProgramsCatalog</c> and <c>StepStock</c>.</remarks>
    internal sealed class ReferenceAttractInput
    {
        /// <summary>Mutable reference script state, including independently authored held and new inputs.</summary>
        public DemoInputState Script { get; } = new();

        /// <summary>Loads the scene's native input object through the reference word reader.</summary>
        public ReferenceAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
        {
            Script.LoadObject(bus, scene.InputObject, scene.InputObject,
                definitionWord: pointer => ReadDemoFixtureWord(bus, pointer));
            Script.Enable();
        }

        /// <summary>Advances one reference input frame using native words and pre-instructions.</summary>
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
        /// <summary>Reads a cartridge or synthetic fixture word for the reference script.</summary>
        public ushort ReadInstructionWord(ushort pointer) => ReadDemoFixtureWord(bus, pointer);

        /// <summary>Applies demo leave and movement branches using the native pre-instruction identities.</summary>
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

        /// <summary>Defers all special instructions to the ordinary demo input interpreter.</summary>
        public DemoInputInstructionResult RunSpecialInstruction(DemoInputState demo, ushort opcode, ushort cursor) =>
            DemoInputInstructionResult.NotHandled(cursor);
    }
}
