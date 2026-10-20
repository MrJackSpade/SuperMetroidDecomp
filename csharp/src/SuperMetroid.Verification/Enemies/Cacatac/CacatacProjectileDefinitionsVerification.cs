using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>Checks Cacatac spike selector and speed definitions through production spawns.</summary>
internal static class CacatacProjectileDefinitionsVerification
{
    /// <summary>Compares ten retail instruction selectors, both speed components, and copied source position.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Cacatac projectile definitions")]
    internal static void VerifyCacatacProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0x86d96a;

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, CacatacSpikeDirection> spawn = enemies.CacatacBehavior.SpawnCacatacSpike;
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = new(new(0x1234, 0x5678), new(0x9abc, 0xdef0));
        source.PaletteIndex = 0x0c00;
        source.VramTilesIndex = 0x0123;

        foreach (CacatacSpikeDirection direction in Enum.GetValues<CacatacSpikeDirection>())
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Clear();

            ushort raw = (ushort)direction;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(instructionTable + raw);
            AssertEqual(native, CacatacProjectileNativeListAddresses.Entry(CacatacProjectileDefinitions.Program(direction)),
                $"compiled Cacatac spike instruction {direction}");

            CacatacSpikeSpeedPair speeds = CacatacProjectileDefinitions.SpeedPair(direction);
            ushort expectedNegative = direction >= CacatacSpikeDirection.UpLeft
                ? (ushort)0xfe80
                : (ushort)0xfe00;
            ushort expectedPositive = direction >= CacatacSpikeDirection.UpLeft
                ? (ushort)0x0180
                : (ushort)0x0200;
            AssertEqual(expectedNegative, speeds.Negative.RawValue, $"compiled Cacatac negative speed {direction}");
            AssertEqual(expectedPositive, speeds.Positive.RawValue, $"compiled Cacatac positive speed {direction}");

            spawn(source, direction);
            RoomEnemyProjectileSlot actual = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
            AssertTrue(actual.Animation is CacatacProjectileAnimation { Cursor.Program: var spawned } &&
                    CacatacProjectileNativeListAddresses.Entry(spawned) == native,
                $"spawned Cacatac instruction {direction}");
            AssertEqual(expectedNegative, actual.Velocity.Y.RawValue, $"spawned Cacatac negative speed {direction}");
            AssertEqual(expectedPositive, actual.Velocity.X.RawValue, $"spawned Cacatac positive speed {direction}");
            AssertEqual(source.Position.X.Whole, actual.Position.X.Whole, $"spawned Cacatac X {direction}");
            AssertEqual(source.Position.X.Fraction, actual.Position.X.Fraction, $"spawned Cacatac X fraction {direction}");
            AssertEqual(source.Position.Y.Whole, actual.Position.Y.Whole, $"spawned Cacatac Y {direction}");
            AssertEqual(source.Position.Y.Fraction, actual.Position.Y.Fraction, $"spawned Cacatac Y fraction {direction}");
        }

        AssertThrows<InvalidDataException>(
            () => CacatacProjectileDefinitions.Program((CacatacSpikeDirection)1),
            "Cacatac odd spike selector");
        AssertThrows<InvalidDataException>(
            () => CacatacProjectileDefinitions.Program((CacatacSpikeDirection)20),
            "Cacatac spike selector beyond authored table");

        Console.WriteLine(
            "Cacatac spike definitions: ten native selectors, speed pairs and production spawns pass with selector reads forbidden.");
    }

}
