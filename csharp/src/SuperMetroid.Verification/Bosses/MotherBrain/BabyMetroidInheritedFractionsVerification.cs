using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Baby Metroid's inherited position fractions when Mother Brain spawns it.</summary>
internal static class BabyMetroidInheritedFractionsVerification
{
    /// <summary>
    /// Verifies that spawning and initializing the cutscene Baby overwrite only whole X/Y words,
    /// retaining both fractions from the first free enemy slot for subsequent movement.
    /// This matches SpawnEnemy ($A0:9275) and Baby initialization ($A9:C710); issue #1269
    /// exposed the lost Y fraction in the 100% movie.
    /// </summary>
    internal static void VerifyBabyMetroidInheritedFractions()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrainEncounter.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();

        RoomEnemySlot free = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == 0);
        free.Position = free.Position.WithFractionX(0x1400).WithFractionY(0x2800);
        runtime.Enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(state);

        AssertTrue(ReferenceEquals(free, state.BabyMetroidSlot), "the Baby takes the first free slot");
        AssertEqual((ushort)0x0140, free.Position.X.Whole, "init writes the Baby's X");
        AssertEqual((ushort)0x0060, free.Position.Y.Whole, "init writes the Baby's Y");
        AssertEqual((ushort)0x1400, free.Position.X.Fraction, "the Baby keeps the slot's X fraction");
        AssertEqual((ushort)0x2800, free.Position.Y.Fraction, "the Baby keeps the slot's Y fraction");
        AssertEqual((ushort)0x2800, (state.BabyMetroid ?? throw new InvalidOperationException("Verification requires state.BabyMetroid.")).Position.Y.Fraction, "the actor moves from the inherited fraction");
        Console.WriteLine("Baby Metroid inherited fractions: spawn and init leave the slot's fractions in place.");
    }

    /// <summary>Runs the inherited fraction regression against a retail Mother Brain room fixture.</summary>
    [VerificationCommand("--baby-metroid-inherited-fractions")]
    internal static int BabyMetroidInheritedFractionsCommand()
    {
        VerifyBabyMetroidInheritedFractions();
        return 0;
    }
}
