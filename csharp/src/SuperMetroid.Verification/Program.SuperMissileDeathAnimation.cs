using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// A Super Missile kill starts explosion two for headers zero to two and the header's own
    /// three or four otherwise ($A0:A67B-$A69D). In the 13% movie a Super Missile kills a
    /// Dragon (header four); its big explosion's first instructions spawn two randomly placed
    /// sprites, consuming RNG the port skipped with explosion two.
    /// </summary>
    private static void VerifySuperMissileDeathAnimation()
    {
        AssertEqual(EnemyDeathInstructionProgramDefinitions.BigExplosion, SuperMissileKillProgram(0xd4bf),
            "a Super Missile kill keeps the Dragon's big explosion");
        AssertEqual(EnemyDeathInstructionProgramDefinitions.NormalExplosion, SuperMissileKillProgram(0xdcbf),
            "a Super Missile kill raises Sova's small explosion to explosion two");
        Console.WriteLine("  Super Missile death animation: headers three and four are kept, lower ones become two.");
    }

    private static ushort SuperMissileKillProgram(ushort enemyDefinition)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xafa3);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = true;
        RoomEnemySlot enemy = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == enemyDefinition);
        enemy.Health = 1;
        enemy.InvincibilityTimer = 0;
        // A surfaced Dragon shows $E80C, as at native update 40203; the room loads it
        // submerged on the empty map, which the shot pass skips. Sova's first frame is kept.
        if (enemy.SpritemapPointer == 0x804d)
            enemy.SpritemapPointer = 0xe80c;
        enemy.InstructionTimer = 0x7fff;
        runtime.Enemies.PrepareEnemyProcessingList(enemy.XPosition, enemy.YPosition);

        SamusProjectileSlot shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x0200; shot.Damage = 300; shot.Direction = 2;
        shot.XPosition = enemy.XPosition; shot.YPosition = enemy.YPosition;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: enemy.NativeIndex);

        RoomEnemyProjectileSlot explosion = runtime.Enemies.EnemyProjectiles
            .Single(projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        return explosion.InstructionPointer;
    }
}
