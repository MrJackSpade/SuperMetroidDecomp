using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: every Baby routine reads the head enemy Enemy[1]'s X/Y, which head AI moved
    // earlier this frame. The port read the rainbow sequence's own brain copy, one frame
    // stale, so in the 100% movie the Baby latched onto the head's previous position
    // ($7E,$5B-$18 instead of $7D,$5D-$18).
    private static void VerifyBabyMetroidHeadTarget()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");
        var sequence = new MotherBrainRainbowBeamAttackSequence { BrainXPosition = 0x007e, BrainYPosition = 0x005b };
        state.RainbowBeamSequence = sequence;
        typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainBabyMetroid", flags)!.Invoke(runtime.Enemies, [state]);
        RoomEnemySlot slot = state.BabyMetroidSlot!;
        BabyMetroidCutsceneState baby = state.BabyMetroid!;

        head.XPosition = 0x007d;
        head.YPosition = 0x005d;
        void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
        Set(nameof(baby.Phase), BabyMetroidCutscenePhase.ActivateRainbowBeamAndMotherBrainBody);
        Set(nameof(baby.XPosition), (ushort)0x007d);
        Set(nameof(baby.YPosition), (ushort)0x0045);

        typeof(RoomEnemySystem).GetMethod("RunMotherBrainBabyMetroidMain", flags)!
            .Invoke(runtime.Enemies, [slot, runtime.Samus, (ushort)0, (ushort)0]);
        AssertEqual(BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse, baby.Phase, "the Baby reaches the head");
        AssertEqual((ushort)0x007d, slot.XPosition, "the Baby pins to the head enemy's current X");
        AssertEqual((ushort)0x0045, slot.YPosition, "the Baby pins $18 above the head enemy's current Y");
        Console.WriteLine("Baby Metroid head target: the Baby reads the head enemy's current position.");
    }
}
