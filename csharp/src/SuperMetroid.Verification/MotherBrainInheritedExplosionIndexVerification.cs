using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks shared WRAM word $0FF2 across Mother Brain's ascent dust and death explosion sequence.</summary>
internal static class MotherBrainInheritedExplosionIndexVerification
{
    /// <summary>Confirms ascent dust leaves $0FF2 at two so the first death burst uses row one, preserving the offsets lost by issue #1269's separate index.</summary>
    internal static void VerifyMotherBrainInheritedExplosionIndex()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");

        state.BodySubFunctionTimer = NativeCountdown.Start(2);
        runtime.Enemies.MotherBrainBehavior.StartLiveMotherBrainRainbowBeam(state, runtime.Player.IntroducedSamus, null);
        MotherBrainRainbowBeamAttackSequence sequence = (state.RainbowBeamSequence ?? throw new InvalidOperationException("Verification requires state.RainbowBeamSequence."));
        AssertEqual((ushort)2, (sequence ?? throw new InvalidOperationException("Verification requires sequence.")).DeathAndEscapeExplosionIndex.RawValue, "the sequence inherits word $0FF2");

        var requests = new List<MotherBrainDeathExplosionRequest>();
        sequence.GenerateDeathExplosions(false, (Func<ushort>)(() => 0), requests);
        AssertEqual(2, requests.Count, "a smoky burst spawns two explosions");
        AssertEqual((ushort)1, sequence.DeathAndEscapeExplosionIndex.RawValue, "the first burst decrements to row 1");
        AssertEqual(((short)0x0011, unchecked((short)0xffc9)), (requests[0].Offset.X, requests[0].Offset.Y),
            "row 1's first offset pair");
        AssertEqual(((short)0x001e, unchecked((short)0xffea)), (requests[1].Offset.X, requests[1].Offset.Y),
            "row 1's second offset pair");
        Console.WriteLine("Mother Brain inherited explosion index: the death explosions continue word $0FF2.");
    }

    /// <summary>Runs the focused Mother Brain shared-explosion-index check.</summary>
    [VerificationCommand("--mother-brain-inherited-explosion-index")]
    internal static int MotherBrainInheritedExplosionIndexCommand()
    {
        VerifyMotherBrainInheritedExplosionIndex();
        return 0;
    }
}
