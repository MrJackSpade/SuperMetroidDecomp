using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class PhantoonFlameSpawnsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon flame spawns")]
    internal static void VerifyCompiledPhantoonFlameSpawns(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 16; i++)
            AssertEqual(rom.ReadCartridgeByte(0x8698b4 + i), PhantoonFlameSpawnDefinitions.RageAngle(i), "All native rage start angles");
        for (int i = 0; i < 9; i++)
            AssertEqual(rom.ReadCartridgeByte(0x8698f7 + i), PhantoonFlameSpawnDefinitions.RainX(i), "All native rain columns");
        for (int i = 0; i < 8; i++)
            AssertEqual(rom.ReadCartridgeByte(0x869979 + i), PhantoonFlameSpawnDefinitions.SpiralAngle(i), "All native spiral start angles");
        var parameters = Enumerable.Range(0, 16).Select(i => (ushort)(0x200 + i))
            .Concat(Enumerable.Range(0, 8).Select(i => (ushort)(0x600 + i)))
            .Concat(Enumerable.Range(0, 16).SelectMany(delay => Enumerable.Range(0, 9).Select(column => (ushort)(0x400 + delay * 16 + column))))
            .Append((ushort)0).ToArray();
        foreach (ushort origin in new ushort[] { 0, 128, 0xfff8 })
        foreach (ushort parameter in parameters)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(NewPhantoonFlameSpawnReadGuard(rom));
            Func<RoomEnemySlot, PhantoonFlameSpawnType, byte, bool> spawn = enemies.PhantoonBehavior.SpawnPhantoonDestroyableFlame;
            var body = enemies.SlotTable.Slots[0];
            body.Position = body.Position.WithWhole(new(origin, origin));
            PhantoonFlameSpawnType type = ClosedNativeWords.Decode<PhantoonFlameSpawnType>(
                SnesLittleEndian.HighByte(parameter), "Phantoon flame spawn type");
            int index = SnesLittleEndian.LowByte(parameter);
            AssertTrue(spawn(body, type, (byte)parameter), "Real Phantoon flame allocation succeeds");
            var flame = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            ushort angle = type switch
            {
                PhantoonFlameSpawnType.Enraged => rom.ReadCartridgeByte(0x8698b4 + index),
                PhantoonFlameSpawnType.Spiral => rom.ReadCartridgeByte(0x869979 + index),
                PhantoonFlameSpawnType.Casual or PhantoonFlameSpawnType.Rain => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameSpawnType)} {(int)type}."),
            };
            ushort x = type == PhantoonFlameSpawnType.Rain ? rom.ReadCartridgeByte(0x8698f7 + (index & 15)) : origin;
            ushort y = type == PhantoonFlameSpawnType.Rain ? (ushort)40
                : unchecked((ushort)(origin + (type == PhantoonFlameSpawnType.Spiral ? 16 : 32)));
            ushort speed = type switch
            {
                PhantoonFlameSpawnType.Enraged => unchecked((ushort)(index < 8 ? 2 : -2)),
                PhantoonFlameSpawnType.Spiral => 128,
                PhantoonFlameSpawnType.Rain => (ushort)((index & 240) >> 1),
                PhantoonFlameSpawnType.Casual => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameSpawnType)} {(int)type}."),
            };
            AssertEqual(angle, flame.Variable0, "Actual flame initial angle");
            AssertEqual(x, flame.Position.X.Whole, "Actual flame initial X");
            AssertEqual(y, flame.Position.Y.Whole, "Actual flame initial wrapped Y");
            AssertEqual(speed, flame.Velocity.X.RawValue, "Actual flame direction or packed delay");
            AssertEqual((ushort)0, flame.Velocity.Y.RawValue, "Actual flame initial radius/vertical speed");
            AssertEqual((ushort)0, flame.Position.X.Fraction, "Actual flame initial fractional X");
            AssertEqual((ushort)0, flame.Position.Y.Fraction, "Actual flame initial fractional Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawnDefinitions.RageAngle(16), "Rage angle bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawnDefinitions.RainX(9), "Rain column bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawnDefinitions.SpiralAngle(8), "Spiral angle bounds");
        Console.WriteLine("Phantoon flame spawns: 33 native bytes and 507 real initializers reject migrated table reads.");
    }

    private static CartridgeReadGuard NewPhantoonFlameSpawnReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x8698b4 and < 0x8698c4 or >= 0x8698f7 and < 0x869900 or >= 0x869979 and < 0x869981,
                address => $"Migrated Phantoon spawn table read at {address:X6}.")
            .RejectWrites(_ => "Unexpected flame initialization bus write.");
}
