using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks compiled Samus and enemy square-slope quadrants against native byte tables.</summary>
internal static class SquareSlopeDefinitionsVerification
{
    /// <summary>Runs separate missile-point and enemy-quadrant comparisons.</summary>
    internal static void VerifyCompiledSquareSlopes(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifySamusSquareSlopeAlgorithm), () => VerifySamusSquareSlopeAlgorithm(rom));
        Suite(nameof(VerifyEnemySquareSlopeAlgorithm), () => VerifyEnemySquareSlopeAlgorithm(rom));
    }

    /// <summary>Checks all native Samus quadrant bytes and actual missile reactions for five shapes and BTS orientations.</summary>
    private static void VerifySamusSquareSlopeAlgorithm(SuperMetroidAddressSpace rom)
    {
        var native = new byte[20];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = rom.ReadCartridgeByte(SamusProjectileRomDataCollision.SquareSlopeDefinitions + i);
            AssertEqual(native[i], SquareSlopes.ReadSamusQuadrant(i), "Native Samus square quadrant byte");
        }
        foreach (int invalid in new[] { -1, 20, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SquareSlopes.ReadSamusQuadrant(invalid),
                $"Samus square quadrant rejects {invalid}");

        var missile = new Func<RoomCollisionBlock, SamusProjectileSlot, bool, bool>(ProjectileMotion.MissileSlopePointReaction);
        var bus = NewSlopeHeightNoReadBus();
        var projectile = new SamusProjectileSlot(0);
        int cases = 0;
        for (int raw = 0; raw < 256; raw++)
        {
            int shape = raw & 31;
            if (shape >= 5) continue;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([0x1000], [(byte)raw], [0]));
            var block = level.GetCollisionBlock(new(0, 0));
            for (ushort x = 0; x < 16; x++)
            for (ushort y = 0; y < 16; y++)
            {
                int column = (raw & 64) != 0 ? 15 - x : x;
                int row = (raw & 128) != 0 ? 15 - y : y;
                bool expected = native[shape * 4 + row / 8 * 2 + column / 8] != 0;
                projectile.Position = projectile.Position.WithWhole(new(x, y));
                AssertEqual(expected, missile(block, projectile, true), "Actual horizontal missile square quadrant");
                AssertEqual(expected, missile(block, projectile, false), "Actual vertical missile square quadrant");
                cases++;
            }
        }
        AssertEqual(10240, cases, "Every square shape/BTS orientation/pixel");
        Console.WriteLine("Samus square-slope algorithm: 20 native bytes, bounds and 10240 real missile point cases in both axes pass without ROM reads.");
    }

    /// <summary>Compares both native enemy quadrant copies and rejects indices outside their twenty-byte range.</summary>
    private static void VerifyEnemySquareSlopeAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 20; index++)
        {
            byte actual = SquareSlopes.ReadEnemyQuadrant(index);
            AssertEqual(rom.ReadCartridgeByte(SquareSlopes.EnemyReferenceAddress + index), actual,
                $"enemy square quadrant byte {index}, including identity bits");
            AssertEqual(rom.ReadCartridgeByte(SquareSlopes.ProjectileReferenceAddress + index), actual,
                $"enemy-projectile square quadrant copy {index}");
        }
        foreach (int invalid in new[] { -1, 20, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SquareSlopes.ReadEnemyQuadrant(invalid),
                $"enemy square quadrant rejects {invalid}");
        Console.WriteLine("Enemy square-slope algorithm: both twenty-byte native copies and bounds pass.");
    }
}
