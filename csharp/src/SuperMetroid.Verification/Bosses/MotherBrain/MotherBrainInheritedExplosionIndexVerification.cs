using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks shared WRAM word $0FF2 across Mother Brain's ascent dust and death explosion sequence.</summary>
internal static class MotherBrainInheritedExplosionIndexVerification
{
    /// <summary>Confirms ascent dust leaves $0FF2 at two so the first death burst uses row one, preserving the offsets lost by issue #1269's separate index.</summary>
    internal static void VerifyMotherBrainInheritedExplosionIndex()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");

        state.BodySubFunctionTimer = WrappingCountdown.Start(2);
        runtime.Enemies.MotherBrainBehavior.Attacks.StartLiveMotherBrainRainbowBeam(state, runtime.Player.IntroducedSamus, null);
        MotherBrainRainbowBeamAttackSequence sequence = (state.RainbowBeamSequence ?? throw new InvalidOperationException("Verification requires state.RainbowBeamSequence."));
        AssertEqual((ushort)2, sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex.RawValue, "the sequence inherits word $0FF2");

        var requests = new List<MotherBrainDeathExplosionRequest>();
        sequence.ThirdPhaseDeath.GenerateDeathExplosions(MotherBrainDeathExplosionStyle.Smoky, () => 0, requests);
        AssertEqual(2, requests.Count, "a smoky burst spawns two explosions");
        AssertEqual((ushort)1, sequence.ThirdPhaseDeath.DeathAndEscapeExplosionIndex.RawValue, "the first burst decrements to row 1");
        AssertEqual(new OctantVector(0x0011, unchecked((short)0xffc9)), new OctantVector(requests[0].Offset.X, requests[0].Offset.Y),
            "row 1's first offset pair");
        AssertEqual(new OctantVector(0x001e, unchecked((short)0xffea)), new OctantVector(requests[1].Offset.X, requests[1].Offset.Y),
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
