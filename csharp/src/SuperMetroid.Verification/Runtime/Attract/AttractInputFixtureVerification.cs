using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Verification.Input;
using static SuperMetroid.Verification.Frontend.FrontendVerification;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Provides a cartridge-backed interpreter for comparing typed attract-demo input playback.</summary>
internal static class AttractInputFixtureVerification
{
    /// <summary>Interprets cartridge or synthetic demo words as the independent reference input stream.</summary>
    /// <remarks>Production playback plays <c>DemoInputRecordings</c> through <c>StepStock</c>.</remarks>
    internal sealed class ReferenceAttractInput
    {
        /// <summary>The reference object, including independently authored held and new inputs.</summary>
        public NativeDemoInputReference Script { get; }

        /// <summary>Loads the scene's native input object through the reference word reader.</summary>
        public ReferenceAttractInput(ISnesAddressSpace bus, AttractDemoScene scene)
        {
            Script = new NativeDemoInputReference(pointer => ReadDemoFixtureWord(bus, pointer));
            ushort header = AttractDemoInputDefinitions.NativeHeaderOf(scene.Input);
            Script.Load(header, header);
            Script.Enable();
        }

        /// <summary>Advances one reference input frame using native words and pre-instructions.</summary>
        public void Step(SuperMetroidGameState gameState, SamusMovementType movementType) =>
            Script.Step((state, pointer) => RunFrameBehavior(state, pointer, gameState, movementType), NativeDemoInputReference.NoRoutines);
    }

    /// <summary>Applies demo leave and movement branches using the native pre-instruction identities.</summary>
    private static void RunFrameBehavior(NativeDemoInputReference state, ushort pointer, SuperMetroidGameState gameState,
        SamusMovementType movementType)
    {
        switch (ClosedEnumWordDecoding.Decode<NativeDemoFrameBehavior>(pointer, "reference demo pre-instruction"))
        {
            case NativeDemoFrameBehavior.ReturnOnly:
            case NativeDemoFrameBehavior.Cleared:
                break;
            case NativeDemoFrameBehavior.AttractCheckLeave:
                if (gameState == SuperMetroidGameState.RoutingAfterDemo)
                    state.Redirect(pointer, DemoInputNativeListAddresses.Entry(DemoInputRecordingId.AttractDelete));
                break;
            case NativeDemoFrameBehavior.AttractShinespark:
                if (movementType != SamusMovementType.DraygonHeld)
                    state.Redirect((ushort)NativeDemoFrameBehavior.AttractCheckLeave,
                        DemoInputNativeListAddresses.Entry(DemoInputRecordingId.AttractDiagonalShinesparkContinuation));
                break;
            case NativeDemoFrameBehavior.IntroBabyRunningLeft:
            case NativeDemoFrameBehavior.IntroBabyStopAndLook:
                throw new InvalidOperationException($"A title demonstration's input object cannot hold {pointer:X4}.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(NativeDemoFrameBehavior)} {pointer:X4}.");
        }
    }
}
