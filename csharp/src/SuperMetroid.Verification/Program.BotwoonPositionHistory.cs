using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: InitAI_Botwoon seeds only the four delayed head positions. Its position
    // history ($7E:9000-$93FF) lies in the range Initialise_Enemies zeroes, so the body
    // segments' first placements read (0,0); the port seeded it with the head position.
    // Its hidden/dying head sets property $0400, which the port had mistranslated as $8000.
    /// <summary>Checks Botwoon's zeroed position ring, seeded head history, and non-solid hidden or dying head state.</summary>
    private static void VerifyBotwoonPositionHistory()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Botwoon);

        RoomEnemySlot head = runtime.Enemies.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.BotwoonDefinition);
        BotwoonEnemyState state = runtime.Enemies.Botwoon
            ?? throw new InvalidOperationException("Botwoon's room did not initialize its state.");
        AssertTrue(state.HistoryX.All(x => x == 0) && state.HistoryY.All(y => y == 0),
            "Botwoon's position history starts as Initialise_Enemies' zeroed RAM");
        AssertTrue(state.HeadHistoryX.All(x => x == head.XPosition) &&
            state.HeadHistoryY.All(y => y == head.YPosition),
            "InitAI_Botwoon seeds its four delayed head positions with the spawn position");

        // $B3:961B and $B3:96F5 are ORA #$0400 (ignore Samus collision), never $8000. The
        // dying head must leave the shot pass or a late hit interrupts its fall.
        AssertTrue(head.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            !head.Properties.HasAny(EnemyProperties.SolidToSamus),
            "InitAI_Botwoon hides the head from collision with property $0400");
        head.Properties = head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        head.Health = 0;
        typeof(RoomEnemySystem).GetMethod("ResolveBotwoonCombatAfterCommon",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [head]);
        AssertTrue(head.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            !head.Properties.HasAny(EnemyProperties.SolidToSamus),
            "a lethal hit makes the head intangible with property $0400");
        Console.WriteLine("Botwoon position history and collision bit: ring starts zeroed; $0400 hides the head.");
    }
}
