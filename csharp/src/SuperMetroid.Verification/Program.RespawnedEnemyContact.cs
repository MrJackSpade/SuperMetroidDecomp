using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: Respawn_Enemy ($86:F264) leaves the spritemap word cleared, and $A0:A08C
    // skips Samus contact while it is zero. In the 100% movie a Hellway Zebbo respawning
    // beside Samus therefore hurts her one frame after it reappears, not on that frame.
    private static void VerifyRespawnedEnemyContact()
    {
        const ushort respawningEnemy = 0xf1d3;
        const ushort respawnX = 0x1e0;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Hellway);
        var samus = runtime.Samus!;
        samus.InputLocked = false;

        RoomEnemySlot enemy = runtime.Enemies.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == respawningEnemy && slot.XPosition == respawnX);
        AssertTrue(enemy.Properties.HasAny(EnemyProperties.RespawnIfKilled),
            "the Hellway actor respawns when killed");
        runtime.Enemies.StartGenericEnemyDeath(enemy, deathAnimation: 0);

        StepUntil(
            () => enemy.EnemyDefinitionPointer == respawningEnemy,
            _ => runtime.StepFrame(0),
            maximumFrames: 200,
            context: "Hellway enemy respawn");
        AssertEqual((ushort)0, enemy.SpritemapPointer,
            "Respawn_Enemy does not install the empty spritemap");
        // $A0:A08C rejects Samus contact while this word is zero; the first instruction
        // then installs the actor's map and contact can resume on the following pass.
        runtime.StepFrame(0);
        AssertEqual((ushort)0x8a82, enemy.SpritemapPointer,
            "the respawned actor's first instruction installs its first spritemap");
        Console.WriteLine("Respawned enemy contact: Respawn_Enemy keeps a zero spritemap until the first instruction.");
    }
}
