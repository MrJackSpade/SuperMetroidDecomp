using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Runs the Samus and enemy square-slope table comparisons and collision-algorithm checks.</summary>
    /// <param name="rom">Address space containing the native quadrant tables used as the verification oracle.</param>
    private static void VerifyCompiledSquareSlopes(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifySamusSquareSlopeAlgorithm), () => VerifySamusSquareSlopeAlgorithm(rom));
        Suite(nameof(VerifyEnemySquareSlopeAlgorithm), () => VerifyEnemySquareSlopeAlgorithm(rom));
    }

    /// <summary>Checks Samus's twenty quadrant bytes and exhaustively compares real missile point reactions for each square orientation.</summary>
    /// <param name="rom">Address space containing Samus's native square-quadrant table.</param>
    private static void VerifySamusSquareSlopeAlgorithm(SuperMetroidAddressSpace rom)
    {
        var native = new byte[20];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = rom.ReadByte(SamusProjectileRomDataCollision.SquareSlopeDefinitions + i);
            AssertEqual(native[i], SquareSlopeDefinitions.ReadSamusQuadrant(i), "Native Samus square quadrant byte");
        }
        foreach (int invalid in new[] { -1, 20, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SquareSlopeDefinitions.ReadSamusQuadrant(invalid),
                $"Samus square quadrant rejects {invalid}");

        var missile = typeof(SamusProjectileSystem).GetMethod("MissileSlopePointReaction", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, RoomCollisionBlock, SamusProjectileSlot, bool, bool>>();
        var bus = new SlopeHeightNoReadBus();
        var projectile = new SamusProjectileSlot(0);
        int cases = 0;
        for (int raw = 0; raw < 256; raw++)
        {
            int shape = raw & 31;
            if (shape >= 5) continue;
            var level = new RoomLevelData(1, 1, [0x1000], [(byte)raw], [0], new byte[8]);
            var block = level.GetCollisionBlock(0, 0);
            for (ushort x = 0; x < 16; x++)
            for (ushort y = 0; y < 16; y++)
            {
                int column = (raw & 64) != 0 ? 15 - x : x;
                int row = (raw & 128) != 0 ? 15 - y : y;
                bool expected = native[shape * 4 + row / 8 * 2 + column / 8] != 0;
                projectile.XPosition = x;
                projectile.YPosition = y;
                AssertEqual(expected, missile(bus, block, projectile, true), "Actual horizontal missile square quadrant");
                AssertEqual(expected, missile(bus, block, projectile, false), "Actual vertical missile square quadrant");
                cases++;
            }
        }
        AssertEqual(10240, cases, "Every square shape/BTS orientation/pixel");
        Console.WriteLine("Samus square-slope algorithm: 20 native bytes, bounds and 10240 real missile point cases in both axes pass without ROM reads.");
    }

    /// <summary>Checks the compiled enemy quadrant bytes against both native enemy and projectile table copies.</summary>
    /// <param name="rom">Address space containing the native enemy and enemy-projectile quadrant tables.</param>
    private static void VerifyEnemySquareSlopeAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 20; index++)
        {
            byte actual = SquareSlopeDefinitions.ReadEnemyQuadrant(index);
            AssertEqual(rom.ReadByte(SquareSlopeDefinitions.EnemyReferenceAddress + index), actual,
                $"enemy square quadrant byte {index}, including identity bits");
            AssertEqual(rom.ReadByte(SquareSlopeDefinitions.ProjectileReferenceAddress + index), actual,
                $"enemy-projectile square quadrant copy {index}");
        }
        foreach (int invalid in new[] { -1, 20, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SquareSlopeDefinitions.ReadEnemyQuadrant(invalid),
                $"enemy square quadrant rejects {invalid}");
        Console.WriteLine("Enemy square-slope algorithm: both twenty-byte native copies and bounds pass.");
    }
}
