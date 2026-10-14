using SuperMetroid.Core.Hardware;
using System.Reflection;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Draygon turret timing, goop speed and bomb behavior, and dead-body reactions.</summary>
internal static class DraygonTurretCadenceVerification
{
    /// <summary>
    /// Checks that wall turrets use the 16-bit NMI frame counter at $05B6, whose low six bits
    /// triggered the recorded shot even though the separate $05B5 counter disagreed (#1269).
    /// Also checks goop velocity, power-bomb deletion, and repeated dead-body hits.
    /// </summary>
    internal static void VerifyDraygonTurretCadence()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60, camera: new Position16<RoomPixels>(256, 288));
        var boss = runtime.Enemies.DraygonBehavior.Draygon;
        AssertEqual(DraygonAiFunction.IntroInitialDelay, (boss ?? throw new InvalidOperationException("Verification requires boss.")).Function, "fight intro waits before the dance");

        int StepWith(byte counter8, ushort counter16)
        {
            int before = boss.TurretCadenceChecks;
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData, NmiFrameCounter8 = counter8, NmiFrameCounter = counter16 });
            return boss.TurretCadenceChecks - before;
        }

        AssertEqual(1, StepWith(counter8: 0x01, counter16: 0x3000),
            "a $05B6 multiple of $40 runs the turret check whatever $05B5 holds");
        AssertEqual(0, StepWith(counter8: 0x40, counter16: 0x3001),
            "$05B5 alone never runs the turret check");

        // #1269: both goop instructions pass A=2 to $86:8027, which stores it as the speed
        // parameter. A prior turret spawn (speed 3) must not change the goop's velocity.
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        (ushort X, ushort XSub, ushort Y, ushort YSub, ushort Angle) SpawnGoop()
        {
            runtime.System.SetRandomNumber(0x1234);
            runtime.Enemies.DraygonBehavior.SpawnDraygonGoop(boss, false);
            RoomEnemyProjectileSlot goop = runtime.Enemies.Projectiles.EnemyProjectiles.Last(p =>
                p.Kind == RoomEnemyProjectileKind.DraygonGoop);
            var velocity = (goop.Velocity.X.RawValue, goop.Variable0, goop.Velocity.Y.RawValue, goop.Variable1, goop.DirectionParameter);
            goop.Clear();
            return velocity;
        }
        var beforeTurret = SpawnGoop();
        runtime.Enemies.DraygonBehavior.SpawnDraygonWallTurret(boss, runtime.Player.IntroducedSamus, (ushort)0);
        var afterTurret = SpawnGoop();
        AssertEqual(beforeTurret, afterTurret, "goop speed does not inherit a turret's parameter");
        int expectedX = EnemyMath.ReadUnsignedSineMagnitudeProduct(afterTurret.Angle, DraygonProjectileSpeeds.Goop, 0x40);
        AssertEqual(expectedX, unchecked((int)SnesSixteenSixteen.Compose(afterTurret.X, afterTurret.XSub)), "goop X velocity is speed two");

        // #1269: $86:8E0F starts with $86:8D5C. Inside the power-bomb ellipse the goop loses
        // its ID and Samus's XSpeedDivisor clears, but the routine still moves the slot.
        runtime.System.SetRandomNumber(0x1234);
        runtime.Enemies.DraygonBehavior.SpawnDraygonGoop(boss, false);
        RoomEnemyProjectileSlot bombed = runtime.Enemies.Projectiles.EnemyProjectiles.Last(p =>
            p.Kind == RoomEnemyProjectileKind.DraygonGoop);
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        explosion.Spawn(bombed.Position.Whole);
        explosion.ExplosionRadius = (ushort)0x2000;
        runtime.Enemies.Audio.PowerBomb = explosion;
        runtime.Player.IntroducedSamus.XSpeedDivisor = 3;
        ushort bombedX = bombed.Position.X.Whole;
        runtime.Enemies.StepEnemyProjectileInstructions(runtime.Room.LoadedLevelData, runtime.Player.Samus,
            new(runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y));
        AssertTrue(!bombed.IsActive, "a power-bombed goop loses its projectile ID");
        AssertEqual((ushort)0, runtime.Player.IntroducedSamus.XSpeedDivisor, "$86:8D93 clears Samus's X speed divisor");
        AssertTrue(bombed.Position.X.Whole != bombedX, "the goop routine still moves the released slot");

        // #1269: DraygonReaction_Common's dead branch ($A5:9618) has no "already dying"
        // test. A later hit on the zero-health body reinstalls the drift function.
        boss.Body.Health = 0;
        boss.Function = DraygonAiFunction.DyingSink;
        runtime.Enemies.DraygonBehavior.ResolveDraygonReaction(boss.Body, runtime.Player.Samus);
        AssertEqual(DraygonAiFunction.Dying, boss.Function,
            "a hit on the dead body restarts the drift to the death spot");
        Console.WriteLine("Draygon turret cadence: $A5:87AA follows NMI_FrameCounter ($05B6), not $05B5; goop speed is two; power bombs delete goop; dead-body hits restart the death drift.");
    }

    /// <summary>Runs the focused Draygon timing and reaction fixture.</summary>
    [VerificationCommand("--draygon-turret-cadence")]
    internal static int DraygonTurretCadenceCommand()
    {
        VerifyDraygonTurretCadence();
        return 0;
    }
}
