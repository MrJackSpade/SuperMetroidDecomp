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

internal static class BabyMetroidInheritedFractionsVerification
{
    // #1269: SpawnEnemy ($A0:9275) and the cutscene Baby's init ($A9:C710) write the whole
    // X/Y words but never the fractions, so the Baby keeps those its slot's previous occupant
    // left. The port's actor zeroed them: in the 100% movie native's Baby carried Y fraction
    // $2800 into slot 3 and the port's carried $0000.
    internal static void VerifyBabyMetroidInheritedFractions()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
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
        AssertEqual((ushort)0x2800, state.BabyMetroid!.Position.Y.Fraction, "the actor moves from the inherited fraction");
        Console.WriteLine("Baby Metroid inherited fractions: spawn and init leave the slot's fractions in place.");
    }

    [VerificationCommand("--baby-metroid-inherited-fractions")]
    internal static int BabyMetroidInheritedFractionsCommand()
    {
        VerifyBabyMetroidInheritedFractions();
        return 0;
    }
}
