using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Bosses.MotherBrain;
/// <summary>Checks that a Power Bomb's blast passes the Shitroid by (#2261).</summary>
internal static class ShitroidPowerBombVerification
{
    /// <summary>
    /// The Shitroid takes no Power Bomb damage, so the Power Bomb pass passes it by before any reaction, as the original
    /// passes every enemy its Power Bomb cannot hurt: a blast centred on it with the camera at column 512, where a step of
    /// its main behaviour would start its entrance, leaves it waiting for the camera.
    /// </summary>
    internal static void VerifyPowerBombPassesTheShitroid()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.BigBoy);
        RoomEnemyPlace shitroid = runtime.Enemies.Places.InUpdateOrder.First(place =>
            place.DefinitionId == EnemyDefinitionId.BabyMetroid);
        ShitroidEnemyState state = shitroid.SpeciesState<ShitroidEnemyState>()
            ?? throw new InvalidOperationException("The Big Boy room's Shitroid has no state.");
        state.Action = ShitroidAction.WaitForCamera;

        _ = runtime.Enemies.Collision.ResolveOrdinaryPowerBombHits(
            shitroid.Position.Whole, 64, runtime.Room.LoadedLevelData, new(512, 0), runtime.Player.IntroducedSamus);
        AssertEqual(ShitroidAction.WaitForCamera, state.Action, "the blast runs no step of the Shitroid's behaviour");
        Console.WriteLine("  Shitroid Power Bomb: the blast passes the Shitroid by.");
    }
}
