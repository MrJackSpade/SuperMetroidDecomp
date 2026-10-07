using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: InitAI_Botwoon seeds only the four delayed head positions. Its position
    // history ($7E:9000-$93FF) lies in the range Initialise_Enemies zeroes, so the body
    // segments' first placements read (0,0); the port seeded it with the head position.
    private static void VerifyBotwoonPositionHistory()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
        Console.WriteLine("Botwoon position history: ring starts zeroed; delayed head positions start at spawn.");
    }
}
