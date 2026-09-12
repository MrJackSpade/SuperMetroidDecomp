using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonFlameSpawns(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 16; i++)
            AssertEqual(rom.ReadByte(0x8698b4 + i), PhantoonFlameSpawnDefinitions.RageAngle(i), "All native rage start angles");
        for (int i = 0; i < 9; i++)
            AssertEqual(rom.ReadByte(0x8698f7 + i), PhantoonFlameSpawnDefinitions.RainX(i), "All native rain columns");
        for (int i = 0; i < 8; i++)
            AssertEqual(rom.ReadByte(0x869979 + i), PhantoonFlameSpawnDefinitions.SpiralAngle(i), "All native spiral start angles");
        var parameters = Enumerable.Range(0, 16).Select(i => (ushort)(0x200 + i))
            .Concat(Enumerable.Range(0, 8).Select(i => (ushort)(0x600 + i)))
            .Concat(Enumerable.Range(0, 16).SelectMany(delay => Enumerable.Range(0, 9).Select(column => (ushort)(0x400 + delay * 16 + column))))
            .Append((ushort)0).ToArray();
        foreach (ushort origin in new ushort[] { 0, 128, 0xfff8 })
        foreach (ushort parameter in parameters)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(enemies, new PhantoonFlameSpawnReadGuard(rom));
            var spawn = typeof(RoomEnemySystem).GetMethod("SpawnPhantoonDestroyableFlame", BindingFlags.NonPublic | BindingFlags.Instance)!
                .CreateDelegate<Func<RoomEnemySlot, ushort, bool>>(enemies);
            var body = enemies.Slots[0];
            body.XPosition = body.YPosition = origin;
            AssertTrue(spawn(body, parameter), "Real Phantoon flame allocation succeeds");
            var flame = enemies.EnemyProjectiles.Single(p => p.IsActive);
            int type = parameter >> 8, index = parameter & 255;
            ushort angle = type == 2 ? rom.ReadByte(0x8698b4 + index) : type == 6 ? rom.ReadByte(0x869979 + index) : (ushort)0;
            ushort x = type == 4 ? rom.ReadByte(0x8698f7 + (index & 15)) : origin;
            ushort y = type == 4 ? (ushort)40 : unchecked((ushort)(origin + (type == 6 ? 16 : 32)));
            ushort speed = type == 2 ? unchecked((ushort)(index < 8 ? 2 : -2)) : type == 6 ? (ushort)128 : type == 4 ? (ushort)((index & 240) >> 1) : (ushort)0;
            AssertEqual(angle, flame.Variable0, "Actual flame initial angle");
            AssertEqual(x, flame.XPosition, "Actual flame initial X");
            AssertEqual(y, flame.YPosition, "Actual flame initial wrapped Y");
            AssertEqual(speed, flame.XVelocity, "Actual flame direction or packed delay");
            AssertEqual((ushort)0, flame.YVelocity, "Actual flame initial radius/vertical speed");
            AssertEqual((ushort)0, flame.XSubposition, "Actual flame initial fractional X");
            AssertEqual((ushort)0, flame.YSubposition, "Actual flame initial fractional Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawnDefinitions.RageAngle(16), "Rage angle bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawnDefinitions.RainX(9), "Rain column bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawnDefinitions.SpiralAngle(8), "Spiral angle bounds");
        Console.WriteLine("Phantoon flame spawns: 33 native bytes and 507 real initializers reject migrated table reads.");
    }

    private sealed class PhantoonFlameSpawnReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x8698b4 and < 0x8698c4 or >= 0x8698f7 and < 0x869900 or >= 0x869979 and < 0x869981)
                throw new InvalidOperationException($"Migrated Phantoon spawn table read at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected flame initialization bus write.");
    }
}
