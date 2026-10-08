using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: SpawnEnemy ($A0:9275) and the cutscene Baby's init ($A9:C710) write the whole
    // X/Y words but never the fractions, so the Baby keeps those its slot's previous occupant
    // left. The port's actor zeroed them: in the 100% movie native's Baby carried Y fraction
    // $2800 into slot 3 and the port's carried $0000.
    private static void VerifyBabyMetroidInheritedFractions()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();

        RoomEnemySlot free = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0);
        free.XSubposition = 0x1400;
        free.YSubposition = 0x2800;
        typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainBabyMetroid",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [state]);

        AssertTrue(ReferenceEquals(free, state.BabyMetroidSlot), "the Baby takes the first free slot");
        AssertEqual((ushort)0x0140, free.XPosition, "init writes the Baby's X");
        AssertEqual((ushort)0x0060, free.YPosition, "init writes the Baby's Y");
        AssertEqual((ushort)0x1400, free.XSubposition, "the Baby keeps the slot's X fraction");
        AssertEqual((ushort)0x2800, free.YSubposition, "the Baby keeps the slot's Y fraction");
        AssertEqual((ushort)0x2800, state.BabyMetroid!.YSubposition, "the actor moves from the inherited fraction");
        Console.WriteLine("Baby Metroid inherited fractions: spawn and init leave the slot's fractions in place.");
    }
}
