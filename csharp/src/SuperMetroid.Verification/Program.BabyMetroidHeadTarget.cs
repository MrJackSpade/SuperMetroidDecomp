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

/// <summary>Represents baby metroid head target verification.</summary>
internal static class BabyMetroidHeadTargetVerification
{
    // #1269: every Baby routine reads the head enemy Enemy[1]'s X/Y, which head AI moved
    // earlier this frame. The port read the rainbow sequence's own brain copy, one frame
    // stale, so in the 100% movie the Baby latched onto the head's previous position
    // ($7E,$5B-$18 instead of $7D,$5D-$18).
    /// <summary>Verifies baby metroid head target.</summary>
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
        void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
        Set(nameof(baby.Phase), BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody);
        Set(nameof(baby.Position), FixedPosition<RoomPixels>.FromWhole(new(0x007d, 0x0045)).WithFractionsOf(baby.Position));

        runtime.Enemies.BabyMetroidCutsceneBehavior.RunMotherBrainBabyMetroidMain(slot, runtime.Player.Samus, Position16<RoomPixels>.Zero);
        AssertEqual(BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse, baby.Phase, "the Baby reaches the head");
        AssertEqual((ushort)0x007d, slot.Position.X.Whole, "the Baby pins to the head enemy's current X");
        AssertEqual((ushort)0x0045, slot.Position.Y.Whole, "the Baby pins $18 above the head enemy's current Y");
        Console.WriteLine("Baby Metroid head target: the Baby reads the head enemy's current position.");
    }

    /// <summary>Baby metroid head target command.</summary>
    [VerificationCommand("--baby-metroid-head-target")]
    internal static int BabyMetroidHeadTargetCommand()
    {
        VerifyBabyMetroidHeadTarget();
        return 0;
    }
}
