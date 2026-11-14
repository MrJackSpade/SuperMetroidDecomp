using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the Shitroid's scroll changes in the Big Boy room (#1741): the screens it names by column and row are the cells
/// the original stores to.
/// </summary>
internal static class ShitroidScrollPairVerification
{
    /// <summary>
    /// The original rescrolls the room's first four cells, row by row, in the Big Boy room's row of four screens: the second
    /// and fourth take the new state and the first and third are rewritten with the state they hold. It does so when the
    /// camera reaches the room's left half (closing them) and when the Shitroid rises after feeding (opening them with the
    /// camera aligned to their top). The cell after the room's is never written.
    /// </summary>
    internal static void VerifyShitroidScrollPair()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.DefinitionId == EnemyDefinitionId.BabyMetroid);
        ShitroidEnemyState state = shitroid.SpeciesState<ShitroidEnemyState>()
            ?? throw new InvalidOperationException("The Big Boy room's Shitroid has no state.");
        RoomScrollGrid scrolls = runtime.Room.LoadedCamera.Scrolls;
        AssertEqual(new Extent16<RoomScreens>(4, 1), scrolls.Size, "the Big Boy room is one row of four screens");
        var camera = new Position16<RoomPixels>(512, 0);

        Seed(scrolls, [RoomScrollState.Open, RoomScrollState.Open, RoomScrollState.OpenAlignedToTop, RoomScrollState.Open, RoomScrollState.Open]);
        state.Action = ShitroidAction.WaitForCamera;
        runtime.Enemies.ShitroidBehavior.RunShitroidMain(shitroid, runtime.Player.Samus, camera, null);
        AssertEqual(ShitroidAction.BeginEntranceDelay, state.Action, "the camera at X 512 starts the entrance");
        Expect(scrolls, [RoomScrollState.Open, RoomScrollState.Closed, RoomScrollState.OpenAlignedToTop, RoomScrollState.Closed, RoomScrollState.Open],
            "the entrance");

        Seed(scrolls, [RoomScrollState.Closed, RoomScrollState.Closed, RoomScrollState.Open, RoomScrollState.Closed, RoomScrollState.Closed]);
        state.Action = ShitroidAction.RiseAfterFeeding;
        state.StateTimer = 0;
        runtime.Enemies.ShitroidBehavior.RunShitroidMain(shitroid, runtime.Player.Samus, camera, null);
        AssertEqual(ShitroidAction.HoverNearSamus, state.Action, "the rise's timer running out starts the hover");
        Expect(scrolls, [RoomScrollState.Closed, RoomScrollState.OpenAlignedToTop, RoomScrollState.Open, RoomScrollState.OpenAlignedToTop, RoomScrollState.Closed],
            "the rise after feeding");
        Console.WriteLine("  Shitroid scroll pair: the second and fourth screens of the Big Boy room are its second and fourth cells.");
    }

    /// <summary>
    /// Sets each of the room's screens, in order, to the matching one of <paramref name="states"/>, and the screen below the
    /// first, the kept cell after the room's own, to the last.
    /// </summary>
    private static void Seed(RoomScrollGrid scrolls, RoomScrollState[] states)
    {
        foreach ((Position16<RoomScreens> screen, RoomScrollState state) in scrolls.Screens.Zip(states))
            scrolls.SetLogicalState(screen, state);
        scrolls.SetScreen(new(0, scrolls.Size.Y), states[^1]);
    }

    /// <summary>Requires the room's first kept cells, in order, to hold <paramref name="states"/>.</summary>
    private static void Expect(RoomScrollGrid scrolls, RoomScrollState[] states, string change)
    {
        RoomScrollState[] kept = scrolls.KeptStates;
        for (int cell = 0; cell < states.Length; cell++)
            AssertEqual(states[cell], kept[cell], $"{change} leaves cell {cell} {states[cell]}");
    }
}
