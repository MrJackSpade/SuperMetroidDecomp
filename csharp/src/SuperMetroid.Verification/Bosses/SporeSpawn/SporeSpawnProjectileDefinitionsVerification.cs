using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>Checks Spore Spawn stalk, emitter, and spore-motion definitions against native data.</summary>
internal static class SporeSpawnProjectileDefinitionsVerification
{
    /// <summary>Compares geometry and all movement bytes, then runs real spawn and mirrored-motion paths with source reads blocked.</summary>
    internal static void VerifySporeSpawnProjectileDefinitions(CartridgeImportAddressSpace rom)
    {
        // The cartridge numbered the stalk segments and the emitters by their spawn order, the order of the typed lists.
        AssertEqual(4, SporeSpawnProjectileDefinitions.StalkSegments.Count, "Spore Spawn stalk segments");
        AssertEqual(4, SporeSpawnProjectileDefinitions.Emitters.Count, "Spore Spawn emitters");
        for (ushort index = 0; index < 4; index++)
        {
            AssertEqual(0, SporeSpawnProjectileDefinitions.StalkSegments[index].X, $"Spore Spawn stalk X {index}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dcb9 + index * 2)),
                CoordinateAxis.Wrap(SporeSpawnProjectileDefinitions.StalkSegments[index].Y),
                $"Spore Spawn stalk Y {index}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dce6 + index * 2)),
                SporeSpawnProjectileDefinitions.Emitters[index].X,
                $"Spore Spawn emitter X {index}");
        }
        // The cartridge keeps the drift as one byte stream, each step's X then its Y.
        AssertEqual(128, SporeSpawnProjectiles.DriftStepCount, "Spore Spawn drift steps");
        for (int step = 0; step < SporeSpawnProjectiles.DriftStepCount; step++)
        {
            Offset16<RoomPixels> delta = SporeSpawnProjectiles.DriftAt(step);
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + 2 * step)), delta.X,
                $"Spore Spawn drift X {step}");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + 2 * step + 1)),
                delta.Y, $"Spore Spawn drift Y {step}");
        }

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, ushort> spawnStalk = (stalkBody, stalkIndex) => enemies.SporeSpawnBehavior.SpawnSporeSpawnStalk(stalkBody, SporeSpawnProjectileDefinitions.StalkSegments[stalkIndex]);
        Action<RoomEnemySlot, ushort> spawnSpawner = (spawnerBody, spawnerIndex) => enemies.SporeSpawnBehavior.SpawnSporeSpawnSpawner(spawnerBody, SporeSpawnProjectileDefinitions.Emitters[spawnerIndex]);
        Action<EnemyProjectile> move = SporeSpawnBehavior.RunSporeSpawnSporeFrameBehavior;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(0x1234, 0x0200));
        for (ushort index = 0; index < 4; index++)
        {
            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Delete();
            spawnStalk(body, index);
            EnemyProjectile stalk = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(body.Position.X.Whole, stalk.Position.X.Whole, $"Spore Spawn production stalk X {index}");
            AssertEqual(unchecked((ushort)(body.Position.Y.Whole + CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dcb9 + index * 2)))),
                stalk.Position.Y.Whole, $"Spore Spawn production stalk Y {index}");

            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Delete();
            spawnSpawner(body, index);
            EnemyProjectile spawner = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dce6 + index * 2)), spawner.Position.X.Whole,
                $"Spore Spawn production emitter X {index}");
            AssertEqual((ushort)520, spawner.Position.Y.Whole,
                $"Spore Spawn production emitter Y {index}");
        }

        EnemyProjectile spore = enemies.Projectiles.EnemyProjectiles[0];
        for (int step = 0; step < 128; step++)
        for (int mirrored = 0; mirrored < 2; mirrored++)
        {
            int offset = step * 2;
            spore.Kind = RoomEnemyProjectileKind.SporeSpawnSpore;
            spore.Position = spore.Position.WithWhole(new(0x0200, 0x0200));
            spore.State = new SporeSpawnSporeState(mirrored: mirrored != 0) { PathStep = step };
            int x = unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + offset));
            int y = unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + ((offset + 1) & 0xff)));
            move(spore);
            AssertEqual(unchecked((ushort)(0x0200 + (mirrored == 0 ? x : -x))),
                spore.Position.X.Whole, $"Spore Spawn production X {offset:X2}/{mirrored}");
            AssertEqual(unchecked((ushort)(0x0200 + y + y)), spore.Position.Y.Whole,
                $"Spore Spawn production Y {offset:X2}/{mirrored}");
            AssertEqual((step + 1) % 128, spore.StateOf<SporeSpawnSporeState>().PathStep,
                $"Spore Spawn production path step {offset:X2}/{mirrored}");
        }

        Console.WriteLine(
            "Spore Spawn projectile definitions: eight geometry words, 256 movement bytes, all real spawns and 512 movement/mirroring handoffs pass with source tables forbidden.");
    }

}
