using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a wrapping tile shot does not redirect its enemy collision to a remote block alias.</summary>
internal static class WrapShotEnemiesVerification
{
    /// <summary>Hits remote edge tiles in both directions while keeping an enemy eligible, then confirms a real world-space overlap hits it.</summary>
    internal static void VerifyWrapShotEnemySeparation()
    {
        foreach (bool left in new[] { false, true })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.RedTower);
            var enemy = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Ripper);
            foreach (var other in runtime.Enemies.SlotTable.Slots) if (!ReferenceEquals(other, enemy)) other.Clear();
            enemy.Properties |= (ushort)EnemyProperties.ProcessOffScreen;
            // Run the first real animation entry before freezing; a never-drawn
            // zero spritemap is intentionally ineligible for native shot hits.
            for (int warmup = 0; warmup < 32 && enemy.SpritemapPointer is 0 or 0x804d; warmup++)
                runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new((ushort)Math.Max(0, enemy.Position.X.Whole - 128), (ushort)Math.Max(0, enemy.Position.Y.Whole - 128)), TimeIsFrozen = false, Level = runtime.Room.LevelData });
            AssertTrue(enemy.SpritemapPointer is not (0 or 0x804d), "Real enemy animation has published a collision-eligible spritemap");
            int target = left ? 0x123f : 0x0280;
            enemy.Position = enemy.Position.WithWhole(new((ushort)((target % 64) * 16 + 8), (ushort)((target / 64) * 16 + 8)));
            enemy.Properties |= (ushort)EnemyProperties.ProcessOffScreen;
            enemy.InvincibilityTimer = NativeCountdown.Start(0);
            // Populate the real interactive list while freezing AI movement. The
            // remote enemy must be eligible, not silently excluded as offscreen.
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Level = runtime.Room.LevelData });
            AssertTrue(runtime.Enemies.InteractiveEnemyIndexes.Contains(enemy.NativeIndex), "Remote control enemy is collision-eligible");

            var words = new ushort[64 * 128];
            for (int row = 0; row < 128; row++)
            {
                words[row * 64 + (left ? 63 : 0)] = 0x4000;
                words[row * 64 + (left ? 0 : 63)] = 0x8000;
            }
            var level = CreateRoom(new(64, 128), words, new byte[words.Length]);
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(left ? 32 : 992), 128)),
                PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                EquippedBeams = (ushort)SamusBeamFlags.Wave };
            var shots = CreateProjectileFixture();
            var bombs = CreateBombFixture();
            var plms = new RoomPlmSystem();
            ushort health = enemy.Health;
            for (int frame = 0; frame <= (left ? 3 : 4); frame++)
            {
                SnesButton input = frame == 0 ? SnesButton.X : (ushort)0;
                bombs.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, input, input);
                shots.StepFrame(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)(left ? 0 : 768), 0), bombs);
                AssertEqual(0, runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, shots, bombs), "Tile alias does not redirect the projectile enemy hitbox");
            }
            AssertEqual(0x0052, level.GetCollisionBlockByIndex(target).LevelWord, "Same shot actually triggers the remote tile");
            AssertEqual(health, enemy.Health, "Remote enemy health is unchanged");
            AssertEqual(0, enemy.FlashTimer.RawValue, "Remote enemy receives no impact flash");

            // Positive control: move only the eligible enemy onto the unchanged
            // live projectile. A real collision proves the negative was spatial,
            // not a dead projectile, inactive enemy or disabled collision path.
            enemy.Position = enemy.Position.WithWhole(shots.Slots[0].Position.Whole);
            AssertEqual(1, runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(bus, shots, bombs), "World-space overlap reaches the same enemy collision dispatcher");
        }
        Console.WriteLine("PASS wrap-shot enemy separation: both remote tile hits spare eligible enemies; world-space controls collide.");
    }

    /// <summary>Runs the remote tile and enemy collision separation reproduction.</summary>
    [VerificationCommand("--wrap-shot-enemies")]
    internal static int WrapShotEnemiesCommand()
    {
        Suite(nameof(VerifyWrapShotEnemySeparation), () => VerifyWrapShotEnemySeparation());
        return 0;
    }
}
