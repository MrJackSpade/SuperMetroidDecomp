using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: $A5:87AA times Draygon's wall turrets from NMI_FrameCounter ($05B6), never the
    // separate 8-bit counter at $05B5. The two drift apart, and the 100% movie fired a
    // turret on a frame where only $05B6's low six bits were zero.
    private static void VerifyDraygonTurretCadence()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60, cameraX: 256, cameraY: 288);
        var boss = runtime.Enemies.Draygon!;
        AssertEqual(DraygonAiFunction.IntroInitialDelay, boss.Function, "fight intro waits before the dance");

        int StepWith(byte counter8, ushort counter16)
        {
            int before = boss.TurretCadenceChecks;
            runtime.Enemies.StepFrame(runtime.Camera!.XPosition, runtime.Camera.YPosition,
                timeIsFrozen: false, samus: runtime.Samus, level: runtime.LevelData,
                nmiFrameCounter8: counter8, nmiFrameCounter: counter16);
            return boss.TurretCadenceChecks - before;
        }

        AssertEqual(1, StepWith(counter8: 0x01, counter16: 0x3000),
            "a $05B6 multiple of $40 runs the turret check whatever $05B5 holds");
        AssertEqual(0, StepWith(counter8: 0x40, counter16: 0x3001),
            "$05B5 alone never runs the turret check");

        // #1269: both goop instructions pass A=2 to $86:8027, which stores it as the speed
        // parameter. A prior turret spawn (speed 3) must not change the goop's velocity.
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo spawnGoop = typeof(RoomEnemySystem).GetMethod("SpawnDraygonGoop", flags)!;
        MethodInfo spawnTurret = typeof(RoomEnemySystem).GetMethod("SpawnDraygonWallTurret", flags)!;
        (ushort X, ushort XSub, ushort Y, ushort YSub, ushort Angle) SpawnGoop()
        {
            runtime.System.SetRandomNumber(0x1234);
            spawnGoop.Invoke(runtime.Enemies, [boss, false]);
            RoomEnemyProjectileSlot goop = runtime.Enemies.EnemyProjectiles.Last(p =>
                p.Kind == RoomEnemyProjectileKind.DraygonGoop);
            var velocity = (goop.XVelocity, goop.Variable0, goop.YVelocity, goop.Variable1, goop.DirectionParameter);
            goop.Clear();
            return velocity;
        }
        var beforeTurret = SpawnGoop();
        spawnTurret.Invoke(runtime.Enemies, [boss, runtime.Samus!, (ushort)0]);
        var afterTurret = SpawnGoop();
        AssertEqual(beforeTurret, afterTurret, "goop speed does not inherit a turret's parameter");
        MethodInfo product = typeof(RoomEnemySystem).GetMethod("ReadUnsignedSineMagnitudeProduct",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        int expectedX = (int)product.Invoke(null, [afterTurret.Angle, DraygonProjectileSpeeds.Goop, (ushort)0x40])!;
        AssertEqual(expectedX, (afterTurret.X << 16) | afterTurret.XSub, "goop X velocity is speed two");

        // #1269: $86:8E0F starts with $86:8D5C. Inside the power-bomb ellipse the goop loses
        // its ID and Samus's XSpeedDivisor clears, but the routine still moves the slot.
        runtime.System.SetRandomNumber(0x1234);
        spawnGoop.Invoke(runtime.Enemies, [boss, false]);
        RoomEnemyProjectileSlot bombed = runtime.Enemies.EnemyProjectiles.Last(p =>
            p.Kind == RoomEnemyProjectileKind.DraygonGoop);
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Spawn(bombed.XPosition, bombed.YPosition);
        typeof(SamusPowerBombExplosionState).GetProperty(nameof(SamusPowerBombExplosionState.ExplosionRadius))!
            .SetValue(explosion, (ushort)0x2000);
        typeof(RoomEnemySystem).GetField("_audioPowerBomb", flags)!.SetValue(runtime.Enemies, explosion);
        runtime.Samus!.XSpeedDivisor = 3;
        ushort bombedX = bombed.XPosition;
        runtime.Enemies.StepEnemyProjectileInstructions(runtime.LevelData!, runtime.Samus,
            runtime.Camera!.XPosition, runtime.Camera.YPosition);
        AssertTrue(!bombed.IsActive, "a power-bombed goop loses its projectile ID");
        AssertEqual((ushort)0, runtime.Samus.XSpeedDivisor, "$86:8D93 clears Samus's X speed divisor");
        AssertTrue(bombed.XPosition != bombedX, "the goop routine still moves the released slot");
        Console.WriteLine("Draygon turret cadence: $A5:87AA follows NMI_FrameCounter ($05B6), not $05B5; goop speed is two; power bombs delete goop.");
    }
}
