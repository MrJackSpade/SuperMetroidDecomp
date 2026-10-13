using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Mother Brain escape-door fragment offsets, velocities, and production spawns.</summary>
internal static class MotherBrainDoorFragmentDefinitionsVerification
{
    /// <summary>Compares all eight fragments with native physical words and forbids those table reads during spawn.</summary>
    internal static void VerifyMotherBrainDoorFragmentDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 8; parameter++)
        {
            int byteOffset = parameter * 4;
            MotherBrainDoorFragmentDefinition definition =
                MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c992 + byteOffset))),
                definition.Offset.X, $"Mother Brain door fragment X offset {parameter}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c994 + byteOffset))),
                definition.Offset.Y, $"Mother Brain door fragment Y offset {parameter}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c9b2 + byteOffset))),
                definition.Velocity.X.SignedRawValue, $"Mother Brain door fragment X velocity {parameter}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c9b4 + byteOffset))),
                definition.Velocity.Y.SignedRawValue, $"Mother Brain door fragment Y velocity {parameter}");
        }

        var guarded = NewMotherBrainDoorFragmentReadGuard(rom);
        for (ushort parameter = 0; parameter < 8; parameter++)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            enemies.MotherBrainBehavior.SpawnMotherBrainDoorFragment(parameter);

            MotherBrainDoorFragmentDefinition definition =
                MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
            RoomEnemyProjectileSlot fragment =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, fragment.Kind,
                $"production Mother Brain door fragment kind {parameter}");
            AssertEqual(unchecked((ushort)(MotherBrainDeathRomData.DoorX + definition.Offset.X)),
                fragment.Position.X.Whole, $"production Mother Brain door fragment X {parameter}");
            AssertEqual(unchecked((ushort)(MotherBrainDeathRomData.DoorY + definition.Offset.Y)),
                fragment.Position.Y.Whole, $"production Mother Brain door fragment Y {parameter}");
            AssertEqual(definition.Velocity.X, fragment.Velocity.X,
                $"production Mother Brain door fragment X velocity {parameter}");
            AssertEqual(definition.Velocity.Y, fragment.Velocity.Y,
                $"production Mother Brain door fragment Y velocity {parameter}");
            AssertEqual(MotherBrainDeathRomData.DoorFragmentLifetime, fragment.Variable0,
                $"production Mother Brain door fragment lifetime {parameter}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainDoorFragmentDefinitions.ForParameter(8),
            "Mother Brain door fragment parameter past table");
        Console.WriteLine(
            "Mother Brain door fragments: 32 native physical words and all eight real spawns pass with offset/velocity tables forbidden.");
    }

    /// <summary>Rejects production reads of the migrated fragment offset and velocity tables.</summary>
    private static CartridgeReadGuard NewMotherBrainDoorFragmentReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86c992 and < 0x86c9d2,
                address => $"Mother Brain door fragment attempted migrated table read ${address:X6}.");
}
