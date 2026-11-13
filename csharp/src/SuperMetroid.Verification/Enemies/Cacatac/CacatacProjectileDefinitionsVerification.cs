using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>Checks Cacatac spike selector and speed definitions through production spawns.</summary>
internal static class CacatacProjectileDefinitionsVerification
{
    /// <summary>Compares ten retail instruction selectors, both speed components, and copied source position.</summary>
    internal static void VerifyCacatacProjectileDefinitions(CartridgeImportAddressSpace rom)
    {
        const int instructionTable = 0x86d96a;

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, CacatacSpikeDirection> spawn = enemies.CacatacBehavior.SpawnCacatacSpike;
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = new(new(0x1234, 0x5678), new(0x9abc, 0xdef0));
        source.Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook;

        foreach (CacatacSpikeDirection direction in Enum.GetValues<CacatacSpikeDirection>())
        {
            foreach (EnemyProjectile projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Delete();

            ushort raw = CacatacNativeWords.SpikeDirections.Word(direction);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(instructionTable + raw);
            bool diagonal = raw >= CacatacNativeWords.SpikeDirections.Word(CacatacSpikeDirection.UpLeft);
            AssertEqual(native, CacatacProjectileNativeListAddresses.Entry(CacatacSpikes.Animation(direction)),
                $"compiled Cacatac spike instruction {direction}");

            CacatacSpikeSpeedPair speeds = CacatacSpikes.SpeedPair(direction);
            ushort expectedNegative = diagonal
                ? (ushort)0xfe80
                : (ushort)0xfe00;
            ushort expectedPositive = diagonal
                ? (ushort)0x0180
                : (ushort)0x0200;
            AssertEqual(expectedNegative, speeds.Negative.RawValue, $"compiled Cacatac negative speed {direction}");
            AssertEqual(expectedPositive, speeds.Positive.RawValue, $"compiled Cacatac positive speed {direction}");

            spawn(source, direction);
            EnemyProjectile actual = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
            AssertTrue(actual.Animation is CacatacProjectileAnimation { PlayingSegment: var spawned } &&
                    CacatacProjectileNativeListAddresses.Entry(spawned) == native,
                $"spawned Cacatac instruction {direction}");
            // A spike moves toward each side its direction names, at the negative speed toward left or up and the
            // positive speed toward right or down, and not at all along an axis its direction does not name.
            ushort expectedX = direction switch
            {
                CacatacSpikeDirection.LeftFacingUp or CacatacSpikeDirection.LeftFacingDown or
                    CacatacSpikeDirection.UpLeft or CacatacSpikeDirection.DownLeft => expectedNegative,
                CacatacSpikeDirection.RightFacingUp or CacatacSpikeDirection.RightFacingDown or
                    CacatacSpikeDirection.UpRight or CacatacSpikeDirection.DownRight => expectedPositive,
                CacatacSpikeDirection.Up or CacatacSpikeDirection.Down => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(CacatacSpikeDirection)} {direction}."),
            };
            ushort expectedY = direction switch
            {
                CacatacSpikeDirection.Up or CacatacSpikeDirection.UpLeft or CacatacSpikeDirection.UpRight => expectedNegative,
                CacatacSpikeDirection.Down or CacatacSpikeDirection.DownLeft or CacatacSpikeDirection.DownRight => expectedPositive,
                CacatacSpikeDirection.LeftFacingUp or CacatacSpikeDirection.LeftFacingDown or
                    CacatacSpikeDirection.RightFacingUp or CacatacSpikeDirection.RightFacingDown => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(CacatacSpikeDirection)} {direction}."),
            };
            AssertEqual(expectedX, actual.Velocity.X.RawValue, $"spawned Cacatac horizontal velocity {direction}");
            AssertEqual(expectedY, actual.Velocity.Y.RawValue, $"spawned Cacatac vertical velocity {direction}");
            AssertEqual(source.Position.X.Whole, actual.Position.X.Whole, $"spawned Cacatac X {direction}");
            AssertEqual(source.Position.X.Fraction, actual.Position.X.Fraction, $"spawned Cacatac X fraction {direction}");
            AssertEqual(source.Position.Y.Whole, actual.Position.Y.Whole, $"spawned Cacatac Y {direction}");
            AssertEqual(source.Position.Y.Fraction, actual.Position.Y.Fraction, $"spawned Cacatac Y fraction {direction}");
        }

        AssertEqual(false, CacatacNativeWords.SpikeDirections.TryDecode(1, out _), "Cacatac odd spike selector");
        AssertEqual(false, CacatacNativeWords.SpikeDirections.TryDecode(20, out _), "Cacatac spike selector beyond authored table");

        Console.WriteLine(
            "Cacatac spike definitions: ten native selectors, speed pairs and production spawns pass with selector reads forbidden.");
    }

}
