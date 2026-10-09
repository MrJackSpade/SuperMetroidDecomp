using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// <c>$A0:9BA6</c> returns from the extended projectile pass while an enemy shows the
    /// empty extended frame <c>$804F</c>, before reading its point hitbox. In the 13% movie
    /// a charged beam passes over a green walking pirate on that frame without damaging it;
    /// the next frame's real body takes the hit.
    /// </summary>
    private static void VerifyEmptyExtendedFrameShots()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa521);
        RoomEnemySlot pirate = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xf693);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = true;

        // Native update 37306: the pirate at ($1F4,$A0) on $804F, the charged beam at ($1F2,$A3).
        pirate.XPosition = 0x01f4;
        pirate.YPosition = 0x00a0;
        pirate.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.Frame;
        pirate.InstructionTimer = 0x7fff;
        ushort health = pirate.Health;
        runtime.Enemies.PrepareEnemyProcessingList(pirate.XPosition, pirate.YPosition);

        SamusProjectileSlot shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x9010; shot.Damage = 60; shot.Direction = 7;
        shot.XPosition = 0x01f2; shot.YPosition = 0x00a3;
        shot.XRadius = shot.YRadius = 8;
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        int hits = runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: pirate.NativeIndex);

        AssertEqual(0, hits, "the empty extended frame takes no projectile hit");
        AssertEqual(health, pirate.Health, "the beam over the empty frame deals no damage");
        Console.WriteLine("  Empty extended frame: a beam over $804F leaves the pirate untouched.");
    }
}
