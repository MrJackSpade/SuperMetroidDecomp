using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledSlopeHeights(SuperMetroidAddressSpace rom)
    {
        byte[] native = new byte[512];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = rom.ReadByte(0x948b2b + i);
            AssertEqual((byte)(native[i] & 31), SlopeHeightDefinitions.Read(i / 16, i % 16), "All native slope height samples");
        }
        var forbidden = new SlopeHeightNoReadBus();
        var enemies = new RoomEnemySystem();
        var enemy = enemies.Slots[0];
        var align = typeof(RoomEnemySystem).GetMethod("AlignAgainstSlopeAtPixel", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Func<RoomLevelData, RoomEnemySlot, ushort, ushort, bool, bool>>(enemies);
        var missile = typeof(SamusProjectileSystem).GetMethod("MissileSlopePointReaction", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, RoomCollisionBlock, SamusProjectileSlot, bool, bool>>();
        var bomb = typeof(SamusBombProjectileSystem).GetMethod("BombSpreadCollides", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, RoomLevelData, SamusBombProjectileSlot, bool>>();
        var projectile = new SamusProjectileSlot(0);
        var spread = new SamusBombProjectileSlot(0);
        int collisionCases = 0;
        for (int raw = 0; raw < 256; raw++)
        {
            var bts = new RoomBlockBehavior((byte)raw);
            var level = new RoomLevelData(1, 1, [0x1000], [(byte)raw], [0], new byte[8]);
            var block = level.GetCollisionBlock(0, 0);
            for (ushort x = 0; x < 16; x++)
            {
                int column = (raw & 64) != 0 ? 15 - x : x;
                byte height = (byte)(native[(raw & 31) * 16 + column] & 31);
                foreach (ushort origin in new ushort[] { 0, 0x1230, 0xfff0 })
                    AssertEqual(height, SamusSlopePhysics.ReadAlignmentHeight(forbidden, bts, (ushort)(origin + x)), "Samus real mirrored sample independent of world high bits");
                if ((raw & 31) < 5) continue; // Square slopes have a separate table.
                for (ushort y = 0; y < 16; y++)
                {
                    int edge = (raw & 128) != 0 ? 15 - y : y;
                    bool solid = height <= edge;
                    projectile.XPosition = spread.XPosition = x;
                    projectile.YPosition = spread.YPosition = y;
                    AssertEqual(solid, missile(forbidden, block, projectile, true), "Missile horizontal slope point collision");
                    AssertEqual(solid, missile(forbidden, block, projectile, false), "Missile vertical slope point collision");
                    AssertEqual(solid, bomb(forbidden, level, spread), "Bomb spread real slope collision");
                    enemy.YPosition = 128;
                    enemy.YSubposition = 0x1234;
                    int adjustment = height - edge - 1;
                    bool changed = align(level, enemy, x, y, (raw & 128) != 0);
                    AssertEqual(adjustment < 0, changed, "Enemy real slope alignment admission");
                    int expectedY = 128 + (adjustment < 0 ? (raw & 128) != 0 ? -adjustment : adjustment : 0);
                    AssertEqual((ushort)expectedY, enemy.YPosition, "Enemy exact floor/ceiling alignment Y");
                    AssertEqual((ushort)0x1234, enemy.YSubposition, "Enemy alignment preserves fractional Y");
                    collisionCases++;
                }
            }
        }
        AssertEqual(55296, collisionCases, "All non-square shapes/orientations/points covered");
        Console.WriteLine("Shared slopes: 512 native heights, 12288 Samus samples and 55296 cross-consumer collision/alignment cases pass with ROM reads forbidden.");
    }

    private sealed class SlopeHeightNoReadBus : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException($"Unexpected slope ROM read at {address:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected slope bus write.");
    }
}
