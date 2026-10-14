using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Mother Brain glass-shard selection and spawn placement.</summary>
internal static class MotherBrainGlassShardDefinitionsVerification
{
    /// <summary>Compares native selector and offset tables, then checks 48 real shard spawns without source reads.</summary>
    internal static void VerifyMotherBrainGlassShardDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGlassShardXPlacement), () => VerifyGlassShardXPlacement(rom));
        Suite(nameof(VerifyGlassShardYPlacement), () => VerifyGlassShardYPlacement(rom));
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = NewMotherBrainGlassShardReadGuard(rom);

        Suite(nameof(VerifyGlassShardProgramSelection), () => VerifyGlassShardProgramSelection(rom));

        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            MotherBrainGlassShardPlacement placement =
                MotherBrainGlassShardDefinitions.Placement(parameter);
            for (ushort animationIndex = 0; animationIndex < 16; animationIndex++)
            {
                var random = new Queue<ushort>([unchecked((ushort)(animationIndex << 4)), 8, 8]);
                var enemies = new RoomEnemySystem();
                enemies.BindFixtureBus(guarded);
                enemies.VerificationServices().NextRandom = ((Func<ushort>)(() => random.Dequeue()));
                enemies.MotherBrainBehavior.SpawnMotherBrainGlassProjectile(new(
                    (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
                    parameter,
                    PlmBlock: new(0x20, 0x08)));

                RoomEnemyProjectileSlot shard =
                    enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
                AssertTrue(shard.Animation is MotherBrainGlassAnimation started &&
                    started.Program == MotherBrainGlassAnimations.ShardProgram(animationIndex),
                    $"production Mother Brain glass-shard program {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0200 + unchecked((short)ReadMotherBrainGlassShardWord(rom, 0x86ce61 + parameter)))), shard.Position.X.Whole,
                    $"production Mother Brain glass-shard X {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0080 + unchecked((short)ReadMotherBrainGlassShardWord(rom, 0x86ce67 + parameter)))), shard.Position.Y.Whole,
                    $"production Mother Brain glass-shard Y {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(animationIndex << 5)), shard.Variable0,
                    $"production Mother Brain glass-shard angle offset {parameter}/{animationIndex}");
                AssertEqual(MotherBrainGlassShardDefinitions.GraphicsIndex, shard.GraphicsIndex,
                    $"production Mother Brain glass-shard graphics index {parameter}/{animationIndex}");
                AssertEqual(0, random.Count,
                    $"production Mother Brain glass-shard RNG count {parameter}/{animationIndex}");
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainGlassAnimations.ShardProgram(16),
            "Mother Brain glass-shard animation past table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainGlassShardDefinitions.Placement(1),
            "Mother Brain glass-shard odd placement parameter");
        Console.WriteLine(
            "Mother Brain glass-shard definitions: sixteen instruction selectors, six placement words and 48 real spawns pass with source tables forbidden.");
    }

    /// <summary>Checks the sixteen selector words and rejects every other ushort index.</summary>
    private static void VerifyGlassShardProgramSelection(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort index = (ushort)raw;
            if (raw >= 16)
                AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainGlassAnimations.ShardProgram(index),
                    "Glass shard selector full rejected domain");
            else
                AssertEqual(ReadMotherBrainGlassShardWord(rom, 0x86ce41 + 2 * raw),
                    MotherBrainNativeListAddresses.GlassEntries[MotherBrainGlassAnimations.ShardProgram(index)], "Glass shard original angular selector");
        }
    }

    /// <summary>Checks signed horizontal offsets for the three supported shard parameters.</summary>
    private static void VerifyGlassShardXPlacement(SuperMetroidAddressSpace rom) => VerifyGlassShardPlacementField(rom, false);
    /// <summary>Checks signed vertical offsets for the three supported shard parameters.</summary>
    private static void VerifyGlassShardYPlacement(SuperMetroidAddressSpace rom) => VerifyGlassShardPlacementField(rom, true);

    /// <summary>Checks one placement axis and the complete invalid-parameter domain.</summary>
    private static void VerifyGlassShardPlacementField(SuperMetroidAddressSpace rom, bool yAxis)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort parameter = (ushort)raw;
            if (raw is not (0 or 2 or 4))
            {
                AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainGlassShardDefinitions.Placement(parameter),
                    "Glass shard placement rejects every unsupported parameter");
                continue;
            }
            var actual = MotherBrainGlassShardDefinitions.Placement(parameter);
            short expected = unchecked((short)ReadMotherBrainGlassShardWord(rom, (yAxis ? 0x86ce67 : 0x86ce61) + parameter));
            AssertEqual(expected, yAxis ? actual.Offset.Y : actual.Offset.X, "Glass shard original signed placement field");
        }
    }

    /// <summary>Reads a native shard table word solely for the independent comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain glass shard words")]
    internal static ushort ReadMotherBrainGlassShardWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Forbids live reads of the compiled selector and placement region.</summary>
    private static CartridgeReadGuard NewMotherBrainGlassShardReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86ce41 and < 0x86ce6d,
                address => $"Mother Brain glass shard attempted migrated definition read ${address:X6}.");
}
