using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyWrapShotEnemySeparation()
    {
        foreach (bool left in new[] { false, true })
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.RedTower);
            var enemy = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer != 0 &&
                ((slot.Definition.Bank << 16) | slot.Definition.InitializationAiPointer) == (int)EnemyAiRoutine.InitAI_Ripper);
            foreach (var other in runtime.Enemies.Slots) if (!ReferenceEquals(other, enemy)) other.Clear();
            enemy.Properties |= (ushort)EnemyProperties.ProcessOffScreen;
            // Run the first real animation entry before freezing; a never-drawn
            // zero spritemap is intentionally ineligible for native shot hits.
            for (int warmup = 0; warmup < 32 && enemy.SpritemapPointer is 0 or 0x804d; warmup++)
                runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new((ushort)Math.Max(0, enemy.Position.X.Whole - 128), (ushort)Math.Max(0, enemy.Position.Y.Whole - 128)), TimeIsFrozen = false, Level = runtime.LevelData });
            AssertTrue(enemy.SpritemapPointer is not (0 or 0x804d), "Real enemy animation has published a collision-eligible spritemap");
            int target = left ? 0x123f : 0x0280;
            enemy.Position = enemy.Position.WithWhole(new((ushort)((target % 64) * 16 + 8), (ushort)((target / 64) * 16 + 8)));
            enemy.Properties |= (ushort)EnemyProperties.ProcessOffScreen;
            enemy.InvincibilityTimer = 0;
            // Populate the real interactive list while freezing AI movement. The
            // remote enemy must be eligible, not silently excluded as offscreen.
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Level = runtime.LevelData });
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
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                bombs.StepFrame(bus, level, samus, input, input);
                shots.StepFrame(bus, level, samus, input, input, new((ushort)(left ? 0 : 768), 0), bombs, roomPlms: plms);
                AssertEqual(0, runtime.Enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs), "Tile alias does not redirect the projectile enemy hitbox");
            }
            AssertEqual(0x0052, level.GetCollisionBlockByIndex(target).LevelWord, "Same shot actually triggers the remote tile");
            AssertEqual(health, enemy.Health, "Remote enemy health is unchanged");
            AssertEqual(0, enemy.FlashTimer, "Remote enemy receives no impact flash");

            // Positive control: move only the eligible enemy onto the unchanged
            // live projectile. A real collision proves the negative was spatial,
            // not a dead projectile, inactive enemy or disabled collision path.
            enemy.Position = enemy.Position.WithWhole(shots.Slots[0].Position.Whole);
            AssertEqual(1, runtime.Enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs), "World-space overlap reaches the same enemy collision dispatcher");
        }
        Console.WriteLine("PASS wrap-shot enemy separation: both remote tile hits spare eligible enemies; world-space controls collide.");
    }
}
