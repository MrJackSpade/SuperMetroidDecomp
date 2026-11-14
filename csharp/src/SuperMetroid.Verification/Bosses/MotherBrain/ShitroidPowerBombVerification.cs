using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the Shitroid's Power Bomb reaction (#1813): the main step it runs reads the power-bomb pass's camera.</summary>
internal static class ShitroidPowerBombVerification
{
    /// <summary>
    /// A Power Bomb reaching the Shitroid runs its main step with the camera the power-bomb pass carries, which is the
    /// live camera the original's wait for the camera reads, not the one an earlier main step was given: after a main step
    /// with the camera right of column 513, a Power Bomb with the camera at 512 starts the entrance; after a main step
    /// with the camera at the room's corner, a Power Bomb with the camera at 600 leaves the Shitroid waiting.
    /// </summary>
    internal static void VerifyShitroidPowerBombReadsPassCamera()
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
        ShitroidBehavior behavior = runtime.Enemies.ShitroidBehavior;
        EnemyPowerBombStage reaction = behavior.ShitroidPowerBombReaction.ReplacesCommon
            ?? throw new InvalidOperationException("The Shitroid's Power Bomb reaction replaces the common damage.");

        // The main step waits with the camera right of the column; the Power Bomb's camera is left of it.
        state.Action = ShitroidAction.WaitForCamera;
        state.ReactsToSamus = false;
        behavior.RunShitroidMain(shitroid, runtime.Player.Samus, new Position16<RoomPixels>(600, 0), null);
        AssertEqual(ShitroidAction.WaitForCamera, state.Action, "a main step with the camera at column 600 leaves the Shitroid waiting");
        reaction(new EnemyPowerBombContext(shitroid, runtime.Player.Samus, runtime.Room.LoadedLevelData, new(512, 0)));
        AssertEqual(ShitroidAction.BeginEntranceDelay, state.Action, "a Power Bomb with the camera at column 512 starts the entrance");

        // The main step runs dormant with the camera at the room's corner; the Power Bomb's camera is right of the column.
        state.Action = ShitroidAction.Dormant;
        behavior.RunShitroidMain(shitroid, runtime.Player.Samus, new Position16<RoomPixels>(0, 0), null);
        state.Action = ShitroidAction.WaitForCamera;
        reaction(new EnemyPowerBombContext(shitroid, runtime.Player.Samus, runtime.Room.LoadedLevelData, new(600, 0)));
        AssertEqual(ShitroidAction.WaitForCamera, state.Action, "a Power Bomb with the camera at column 600 leaves the Shitroid waiting");
        Console.WriteLine("  Shitroid Power Bomb: the reaction's main step reads the pass's camera.");
    }
}
