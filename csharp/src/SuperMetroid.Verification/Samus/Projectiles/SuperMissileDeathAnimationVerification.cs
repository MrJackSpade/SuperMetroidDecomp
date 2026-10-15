using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the explosion program selected when a Super Missile kills enemies with different death headers.</summary>
internal static class SuperMissileDeathAnimationVerification
{
    /// <summary>
    /// A Super Missile kill starts explosion two for headers zero to two and the header's own
    /// three or four otherwise ($A0:A67B-$A69D). In the 13% movie a Super Missile kills a
    /// Dragon (header four); its big explosion's first instructions spawn two randomly placed
    /// sprites, consuming RNG the port skipped with explosion two.
    /// </summary>
    internal static void VerifySuperMissileDeathAnimation()
    {
        AssertEqual(EnemyDeathProjectileProgram.BigExplosion, SuperMissileKillProgram(EnemyDefinitionId.Dragon),
            "a Super Missile kill keeps the Dragon's big explosion");
        AssertEqual(EnemyDeathProjectileProgram.NormalExplosion, SuperMissileKillProgram(EnemyDefinitionId.Sova),
            "a Super Missile kill raises Sova's small explosion to explosion two");
        Console.WriteLine("  Super Missile death animation: headers three and four are kept, lower ones become two.");
    }

    /// <summary>Kills the selected retail enemy with a Super Missile and returns the spawned explosion's program.</summary>
    private static EnemyDeathProjectileProgram SuperMissileKillProgram(EnemyDefinitionId enemyDefinition)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xafa3);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = true;
        RoomEnemySlot enemy = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == enemyDefinition);
        enemy.Health = 1;
        enemy.InvincibilityTimer = NativeCountdown.Start(0);
        // A surfaced Dragon shows $E80C, as at native update 40203; the room loads it
        // submerged on the empty map, which the shot pass skips. Sova's first frame is kept.
        if (enemy.SpritemapPointer == 0x804d)
            enemy.SpritemapPointer = 0xe80c;
        enemy.InstructionTimer = NativeCountdown.Start(0x7fff);
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(enemy.Position.Whole);

        SamusProjectileSlot shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x0200; shot.Damage = 300; shot.Direction = 2;
        shot.Position = shot.Position.WithWhole(enemy.Position.Whole);
        shot.Radius = new(4, 4);
        shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = NativeCountdown.Start(1);
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: enemy.NativeIndex);

        RoomEnemyProjectileSlot explosion = runtime.Enemies.Projectiles.EnemyProjectiles
            .Single(projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        return ProgramOf(explosion);
    }

    /// <summary>Runs the focused Super Missile death-program check.</summary>
    [VerificationCommand("--super-missile-death-animation")]
    internal static int SuperMissileDeathAnimationCommand()
    {
        Suite(nameof(VerifySuperMissileDeathAnimation), () => VerifySuperMissileDeathAnimation());
        return 0;
    }
}
