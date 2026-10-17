using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Verification.Input;
using static SuperMetroid.Verification.Frontend.FrontendVerification;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Provides a cartridge-backed interpreter for comparing typed attract-demo input playback.</summary>
internal static class AttractInputFixtureVerification
{
    /// <summary>Interprets cartridge or synthetic demo words as the independent reference input stream.</summary>
    /// <remarks>Production playback runs <c>DemoInputAnimations</c> through <c>StepStock</c>.</remarks>
    internal sealed class ReferenceAttractInput
    {
        /// <summary>The reference object, including independently authored held and new inputs.</summary>
        public NativeDemoInputReference Script { get; }

        /// <summary>Loads the scene's native input object through the reference word reader.</summary>
        public ReferenceAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
        {
            Script = new NativeDemoInputReference(pointer => ReadDemoFixtureWord(bus, pointer));
            Script.Load(scene.InputObject, scene.InputObject);
            Script.Enable();
        }

        /// <summary>Advances one reference input frame using native words and pre-instructions.</summary>
        public void Step(SuperMetroidGameState gameState, SamusMovementType movementType) =>
            Script.Step((state, pointer) => RunPreInstruction(state, pointer, gameState, movementType), NativeDemoInputReference.NoRoutines);
    }

    /// <summary>Applies demo leave and movement branches using the native pre-instruction identities.</summary>
    private static void RunPreInstruction(NativeDemoInputReference state, ushort pointer, SuperMetroidGameState gameState,
        SamusMovementType movementType)
    {
        switch (ClosedNativeWords.Decode<NativeDemoPreInstruction>(pointer, "reference demo pre-instruction"))
        {
            case NativeDemoPreInstruction.ReturnOnly:
            case NativeDemoPreInstruction.Cleared:
                break;
            case NativeDemoPreInstruction.AttractCheckLeave:
                if (gameState == SuperMetroidGameState.TransitionFromDemoB)
                    state.Redirect(pointer, DemoInputNativeListAddresses.Entry(DemoInputProgram.AttractDelete));
                break;
            case NativeDemoPreInstruction.AttractShinespark:
                if (movementType != SamusMovementType.DraygonHeld)
                    state.Redirect((ushort)NativeDemoPreInstruction.AttractCheckLeave,
                        DemoInputNativeListAddresses.Entry(DemoInputProgram.AttractDiagonalShinesparkContinuation));
                break;
            case NativeDemoPreInstruction.IntroBabyRunningLeft:
            case NativeDemoPreInstruction.IntroBabyStopAndLook:
                throw new InvalidOperationException($"A title demonstration's input object cannot hold {pointer:X4}.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(NativeDemoPreInstruction)} {pointer:X4}.");
        }
    }
}
