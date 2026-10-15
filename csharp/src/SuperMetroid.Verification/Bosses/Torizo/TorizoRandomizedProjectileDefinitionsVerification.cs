using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bomb and Golden Torizo projectile tuples and live randomized spawns.</summary>
internal static class TorizoRandomizedProjectileDefinitionsVerification
{
    /// <summary>Compares eight five-word tuples with the cartridge and checks RNG-adjusted projectiles.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo randomized projectile definitions")]
    internal static void VerifyTorizoRandomizedProjectileDefinitions(
        SuperMetroidAddressSpace rom)
    {
        (int Address, TorizoRandomizedProjectileDefinition Definition, string Name)[] definitions =
        [
            (0x86ac08, TorizoRandomizedProjectileDefinitions.BombChozoOrb(true), "Bomb orb right"),
            (0x86ac12, TorizoRandomizedProjectileDefinitions.BombChozoOrb(false), "Bomb orb left"),
            (0x86ac99, TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(true), "Golden orb right"),
            (0x86aca3, TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(false), "Golden orb left"),
            (0x86b02f, TorizoRandomizedProjectileDefinitions.GoldenEgg(true), "Golden egg right"),
            (0x86b039, TorizoRandomizedProjectileDefinitions.GoldenEgg(false), "Golden egg left"),
            (0x86b376, TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(true), "Golden eye beam right"),
            (0x86b380, TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(false), "Golden eye beam left"),
        ];
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        var guarded = NewTorizoRandomizedProjectileReadGuard(rom);
        foreach ((int address, TorizoRandomizedProjectileDefinition definition, string name)
            in definitions)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), TorizoNativeListAddresses.ProjectileEntry(definition.Program),
                $"{name} instruction");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), definition.Offset.X,
                $"{name} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))), definition.BaseVelocity.X.SignedRawValue,
                $"{name} base X velocity");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6))), definition.Offset.Y,
                $"{name} Y offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 8))), definition.BaseVelocity.Y.SignedRawValue,
                $"{name} base Y velocity");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            int randomCall = 0;
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => randomCall++ == 0 ? (ushort)0 : (ushort)0x00ff);
            RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
            torizo.Position = torizo.Position.WithWhole(new(1000, 500));
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
            enemies.BombTorizoBehavior.InitializeTorizoRandomizedProjectile(projectile, torizo, definition);

            AssertEqual<TorizoProjectileProgram?>(definition.Program, (projectile.Animation as TorizoProjectileAnimation)?.Cursor.Program,
                $"{name} production program");
            AssertEqual(unchecked((ushort)(1000 + definition.Offset.X)), projectile.Position.X.Whole,
                $"{name} production X");
            AssertEqual(unchecked((ushort)(definition.BaseVelocity.X.SignedRawValue - 128)), projectile.Velocity.X.RawValue,
                $"{name} production X jitter");
            AssertEqual(unchecked((ushort)(500 + definition.Offset.Y)), projectile.Position.Y.Whole,
                $"{name} production Y");
            AssertEqual(unchecked((ushort)(definition.BaseVelocity.Y.SignedRawValue + 127)), projectile.Velocity.Y.RawValue,
                $"{name} production Y jitter");
            AssertEqual(2, randomCall, $"{name} independent RNG calls");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            VerifyTorizoRandomizedSpawn(
                guarded,
                nameof(BombTorizoBehavior.SpawnBombTorizoChozoOrb),
                (enemies, torizo) => enemies.BombTorizoBehavior.SpawnBombTorizoChozoOrb(torizo),
                facingRight,
                TorizoRandomizedProjectileDefinitions.BombChozoOrb(facingRight),
                velocityIsReplaced: false);
            VerifyTorizoRandomizedSpawn(
                guarded,
                nameof(BombTorizoBehavior.SpawnGoldenTorizoChozoOrb),
                (enemies, torizo) => enemies.BombTorizoBehavior.SpawnGoldenTorizoChozoOrb(torizo),
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(facingRight),
                velocityIsReplaced: false);
            VerifyTorizoRandomizedSpawn(
                guarded,
                nameof(BombTorizoBehavior.SpawnGoldenTorizoEgg),
                (enemies, torizo) => enemies.BombTorizoBehavior.SpawnGoldenTorizoEgg(torizo),
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenEgg(facingRight),
                velocityIsReplaced: false);
            VerifyTorizoRandomizedSpawn(
                guarded,
                nameof(BombTorizoBehavior.SpawnGoldenTorizoEyeBeam),
                (enemies, torizo) => enemies.BombTorizoBehavior.SpawnGoldenTorizoEyeBeam(torizo, 0),
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(facingRight),
                velocityIsReplaced: true);
        }

        Console.WriteLine(
            "Torizo randomized projectiles: all forty tuple words, signed-jitter boundaries, and eight real Bomb/Golden orb, egg and eye-beam spawns pass with every tuple forbidden.");
    }

    /// <summary>Checks each direction's spawner uses its compiled instruction, offset, and velocity policy.</summary>
    private static void VerifyTorizoRandomizedSpawn(
        ISnesAddressSpace guarded,
        string methodName,
        Action<RoomEnemySystem, RoomEnemySlot> spawn,
        bool facingRight,
        TorizoRandomizedProjectileDefinition expected,
        bool velocityIsReplaced)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guarded);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(1000, 500));
        torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;

        spawn(enemies, torizo);

        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
        AssertTrue(projectile.IsActive, $"{methodName} facing {facingRight} allocation");
        AssertEqual<TorizoProjectileProgram?>(expected.Program, (projectile.Animation as TorizoProjectileAnimation)?.Cursor.Program,
            $"{methodName} facing {facingRight} program");
        AssertEqual(unchecked((ushort)(1000 + expected.Offset.X)), projectile.Position.X.Whole,
            $"{methodName} facing {facingRight} X");
        AssertEqual(unchecked((ushort)(500 + expected.Offset.Y)), projectile.Position.Y.Whole,
            $"{methodName} facing {facingRight} Y");
        if (!velocityIsReplaced)
        {
            AssertEqual(unchecked((ushort)(expected.BaseVelocity.X.SignedRawValue - 128)), projectile.Velocity.X.RawValue,
                $"{methodName} facing {facingRight} X jitter");
            AssertEqual(unchecked((ushort)(expected.BaseVelocity.Y.SignedRawValue - 128)), projectile.Velocity.Y.RawValue,
                $"{methodName} facing {facingRight} Y jitter");
        }
    }

    /// <summary>Rejects live reads of all eight migrated projectile tuple ranges.</summary>
    private static CartridgeReadGuard NewTorizoRandomizedProjectileReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86ac08 and < 0x86ac1c or
                >= 0x86ac99 and < 0x86acad or
                >= 0x86b02f and < 0x86b043 or
                >= 0x86b376 and < 0x86b38a,
            address => $"Torizo randomized projectile attempted migrated tuple read ${address:X6}.");
}
