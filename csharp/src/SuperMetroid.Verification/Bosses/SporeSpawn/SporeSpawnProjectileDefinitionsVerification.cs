using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

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
            AssertEqual(0, SporeSpawnProjectileDefinitions.StalkSegments[index].SpawnOffset.X, $"Spore Spawn stalk X {index}");
            AssertEqual(rom.ReadCartridgeWord((0x86dcb9 + index * 2)),
                CoordinateAxis.Wrap(SporeSpawnProjectileDefinitions.StalkSegments[index].SpawnOffset.Y),
                $"Spore Spawn stalk Y {index}");
            AssertEqual(rom.ReadCartridgeWord((0x86dce6 + index * 2)),
                SporeSpawnProjectileDefinitions.Emitters[index].Place.X,
                $"Spore Spawn emitter X {index}");
            // The cartridge mirrors the drift of a spore released in the right half of its screen.
            AssertEqual(SporeSpawnProjectileDefinitions.Emitters[index].Place.X >= 128 ? HorizontalDirection.Left : HorizontalDirection.Right,
                SporeSpawnProjectileDefinitions.Emitters[index].Drift,
                $"Spore Spawn emitter {index}'s spores' first drift");
        }
        // A shot spore's drop rolls the table the cartridge's spore drop names through the header at $A0:DF7F, whose
        // drop-chance word lies 58 bytes in (#2244).
        IImportCartridgeSource cartridge = rom;
        AssertEqual(EnemyHeaderTables.DropChances(cartridge, cartridge.ReadCartridgeWord((RoomEnemyRomLayout.DefinitionBank | 0xdf7f) + 58)),
            SporeSpawnProjectileDefinitions.SporeDropChances, "a shot spore's drop chances are the cartridge's");
        // The cartridge keeps the drift as one byte stream, each step's X then its Y, and moves a spore down by its Y byte
        // twice each step.
        AssertEqual(128, SporeSpawnProjectileDefinitions.Drift.Length, "Spore Spawn drift steps");
        for (int step = 0; step < SporeSpawnProjectileDefinitions.Drift.Length; step++)
        {
            Offset16<RoomPixels> delta = SporeSpawnProjectileDefinitions.Drift[step];
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + 2 * step)), delta.X,
                $"Spore Spawn drift X {step}");
            AssertEqual(2 * unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + 2 * step + 1)),
                delta.Y, $"Spore Spawn drift Y {step}");
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemyPlace, ushort> spawnStalk = (stalkBody, stalkIndex) => enemies.SporeSpawnBehavior.SpawnSporeSpawnStalk(stalkBody.BoundFor(EnemyDefinitionId.SporeSpawn), stalkBody, SporeSpawnProjectileDefinitions.StalkSegments[stalkIndex]);
        Action<RoomEnemyPlace, ushort> spawnSpawner = (spawnerBody, spawnerIndex) => enemies.SporeSpawnBehavior.SpawnSporeSpawnSpawner(spawnerBody.HeldOccupant(), spawnerBody, new SporeSpawnEnemyState(), SporeSpawnProjectileDefinitions.Emitters[spawnerIndex]);
        Action<EnemyProjectile> move = enemies.SporeSpawnBehavior.Spore.DriftSpore;
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Position = body.Position.WithWhole(new(0x1234, 0x0200));
        for (ushort index = 0; index < 4; index++)
        {
            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Delete();
            spawnStalk(body, index);
            EnemyProjectile stalk = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(body.Position.X.Whole, stalk.Position.X.Whole, $"Spore Spawn production stalk X {index}");
            AssertEqual(unchecked((ushort)(body.Position.Y.Whole + rom.ReadCartridgeWord((0x86dcb9 + index * 2)))),
                stalk.Position.Y.Whole, $"Spore Spawn production stalk Y {index}");

            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Delete();
            spawnSpawner(body, index);
            EnemyProjectile spawner = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(rom.ReadCartridgeWord((0x86dce6 + index * 2)), spawner.Position.X.Whole,
                $"Spore Spawn production emitter X {index}");
            AssertEqual((ushort)520, spawner.Position.Y.Whole,
                $"Spore Spawn production emitter Y {index}");
        }

        for (int step = 0; step < 128; step++)
        for (int mirrored = 0; mirrored < 2; mirrored++)
        {
            int offset = step * 2;
            EnemyProjectile spore = enemies.OccupyProjectile(0, RoomEnemyProjectileKind.SporeSpawnSpore, SpriteGraphics.SharedIn(new SporeSpawnEnemyState().Colours.Sprites));
            spore.Position = spore.Position.WithWhole(new(0x0200, 0x0200));
            enemies.SporeSpawnBehavior.Spore.Drifts.Start(spore, new SporeSpawnSporeState(mirrored != 0 ? HorizontalDirection.Left : HorizontalDirection.Right) { PathStep = ListStep<Offset16<RoomPixels>>.At(SporeSpawnProjectileDefinitions.Drift, step) });
            int x = unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + offset));
            int y = unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + ((offset + 1) & 0xff)));
            move(spore);
            AssertEqual(unchecked((ushort)(0x0200 + (mirrored == 0 ? x : -x))),
                spore.Position.X.Whole, $"Spore Spawn production X {offset:X2}/{mirrored}");
            AssertEqual(unchecked((ushort)(0x0200 + y + y)), spore.Position.Y.Whole,
                $"Spore Spawn production Y {offset:X2}/{mirrored}");
            AssertEqual((step + 1) % 128, enemies.SporeSpawnBehavior.Spore.Drifts.Of(spore).PathStep.Place,
                $"Spore Spawn production path step {offset:X2}/{mirrored}");
        }

        Console.WriteLine(
            "Spore Spawn projectile definitions: eight geometry words, 256 movement bytes, all real spawns and 512 movement/mirroring handoffs pass with source tables forbidden.");
    }

}
