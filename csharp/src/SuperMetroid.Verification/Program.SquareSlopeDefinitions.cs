using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledSquareSlopes(SuperMetroidAddressSpace rom)
    {
        var native = new byte[20];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = rom.ReadByte(SamusProjectileRomData.Collision.SquareSlopeDefinitions + i);
            AssertEqual(native[i], SquareSlopeDefinitions.SamusQuadrants[i], "Native Samus square quadrant byte");
            AssertEqual(rom.ReadByte(0xa0c435 + i), SquareSlopeDefinitions.EnemyQuadrants[i], "Native enemy square quadrant byte");
            AssertEqual(rom.ReadByte(0x868729 + i), SquareSlopeDefinitions.EnemyQuadrants[i], "Native enemy-projectile square quadrant byte");
            AssertEqual(native[i], (byte)(SquareSlopeDefinitions.EnemyQuadrants[i] & 128), "Native bank-specific encodings have identical solidity");
        }

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
        Console.WriteLine("Square slopes: all 60 native bank-specific bytes and 10240 real missile point cases in both axes pass without ROM reads.");
    }
}
