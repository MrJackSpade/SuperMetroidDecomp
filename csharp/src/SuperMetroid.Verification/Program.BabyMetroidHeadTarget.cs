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

/// <summary>Reproduces the Baby Metroid's head-target position from issue #1269.</summary>
internal static class BabyMetroidHeadTargetVerification
{
    /// <summary>
    /// Confirms the Baby latches to Enemy[1]'s current head position after head AI moves
    /// in the same frame. The 100% movie exposed the stale rainbow-sequence copy at
    /// ($7E,$5B-$18) rather than the updated ($7D,$5D-$18) position.
    /// </summary>
    internal static void VerifyBabyMetroidHeadTarget()
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
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");
        var sequence = new MotherBrainRainbowBeamAttackSequence { BrainPosition = new(0x007e, 0x005b) };
        state.RainbowBeamSequence = sequence;
        runtime.Enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(state);
        RoomEnemySlot slot = state.BabyMetroidSlot!;
        BabyMetroidCutsceneState baby = state.BabyMetroid!;

        head.Position = head.Position.WithWhole(new(0x007d, 0x005d));
        baby.Phase = BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody;
        baby.Position = FixedPosition<RoomPixels>.FromWhole(new(0x007d, 0x0045)).WithFractionsOf(baby.Position);

        runtime.Enemies.BabyMetroidCutsceneBehavior.RunMotherBrainBabyMetroidMain(slot, runtime.Player.Samus, Position16<RoomPixels>.Zero);
        AssertEqual(BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse, baby.Phase, "the Baby reaches the head");
        AssertEqual((ushort)0x007d, slot.Position.X.Whole, "the Baby pins to the head enemy's current X");
        AssertEqual((ushort)0x0045, slot.Position.Y.Whole, "the Baby pins $18 above the head enemy's current Y");
        Console.WriteLine("Baby Metroid head target: the Baby reads the head enemy's current position.");
    }

    /// <summary>Runs the focused same-frame Mother Brain head-position regression.</summary>
    [VerificationCommand("--baby-metroid-head-target")]
    internal static int BabyMetroidHeadTargetCommand()
    {
        VerifyBabyMetroidHeadTarget();
        return 0;
    }
}
