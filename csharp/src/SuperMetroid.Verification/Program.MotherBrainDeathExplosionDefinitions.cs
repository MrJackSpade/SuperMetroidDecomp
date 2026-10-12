using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class MotherBrainDeathExplosionDefinitionsVerification
{
    internal static void VerifyMotherBrainDeathExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c929 + parameter * 2)),
                MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                $"Mother Brain death explosion instruction {parameter}");
        }

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainDeathExplosion",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo motherBrainField = typeof(RoomEnemySystem).GetField(
            "<MotherBrain>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = NewMotherBrainDeathExplosionReadGuard(rom);
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var body = new RoomEnemySlot(0) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0x0200))};
            motherBrainField.SetValue(enemies, new MotherBrainEnemyState(body));
            var request = new MotherBrainDeathExplosionRequest(
                Offset: new(-7, 9),
                ProjectileParameter: parameter,
                SoundEffect: 0);
            spawn.Invoke(enemies, [request]);

            RoomEnemyProjectileSlot projectile =
                enemies.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainDeathExplosion, projectile.Kind,
                $"production Mother Brain death explosion kind {parameter}");
            AssertEqual(MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                projectile.InstructionPointer,
                $"production Mother Brain death explosion instruction {parameter}");
            AssertEqual(0x00f9, projectile.Position.X.Whole,
                $"production Mother Brain death explosion X {parameter}");
            AssertEqual(0x0209, projectile.Position.Y.Whole,
                $"production Mother Brain death explosion Y {parameter}");
            AssertEqual(unchecked((ushort)-7), projectile.Velocity.X.RawValue,
                $"production Mother Brain death explosion X offset {parameter}");
            AssertEqual(9, projectile.Velocity.Y.RawValue,
                $"production Mother Brain death explosion Y offset {parameter}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainDeathExplosionDefinitions.InstructionList(3),
            "Mother Brain death explosion parameter past table");
        Console.WriteLine(
            "Mother Brain death explosions: three native program selectors and all three real body-relative spawns pass with the selector table forbidden.");
    }

    private static CartridgeReadGuard NewMotherBrainDeathExplosionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86c929 and < 0x86c92f,
                address => $"Mother Brain death explosion attempted migrated selector read ${address:X6}.");
}
