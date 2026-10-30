using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon flame spawn tables through real projectile initialization.</summary>
internal static class PhantoonFlameSpawnsVerification
{
    /// <summary>Compares rage, rain, and spiral bytes and 507 spawn positions without table rereads.</summary>
    internal static void VerifyCompiledPhantoonFlameSpawns(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 16; i++)
            AssertEqual(rom.ReadCartridgeByte(0x8698b4 + i), PhantoonFlameSpawns.RageAngle(i), "All native rage start angles");
        for (int i = 0; i < 9; i++)
            AssertEqual(rom.ReadCartridgeByte(0x8698f7 + i), PhantoonFlameSpawns.RainX(i), "All native rain columns");
        for (int i = 0; i < 8; i++)
            AssertEqual(rom.ReadCartridgeByte(0x869979 + i), PhantoonFlameSpawns.SpiralAngle(i), "All native spiral start angles");
        var parameters = Enumerable.Range(0, 16).Select(i => (ushort)(0x200 + i))
            .Concat(Enumerable.Range(0, 8).Select(i => (ushort)(0x600 + i)))
            .Concat(Enumerable.Range(0, 16).SelectMany(delay => Enumerable.Range(0, 9).Select(column => (ushort)(0x400 + delay * 16 + column))))
            .Append((ushort)0).ToArray();
        foreach (ushort origin in new ushort[] { 0, 128, 0xfff8 })
        foreach (ushort parameter in parameters)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Func<RoomEnemySlot, PhantoonFlameSpawnType, byte, bool> spawn = (flameBody, flameType, flameIndex) =>
                enemies.PhantoonBehavior.Patterns.SpawnPhantoonDestroyableFlame(flameBody, flameType, flameIndex);
            var body = enemies.SlotTable.Slots[0];
            body.Position = body.Position.WithWhole(new(origin, origin));
            PhantoonFlameSpawnType type = ClosedEnumWords.Decode<PhantoonFlameSpawnType>(
                LittleEndianBytes.HighByte(parameter), "Phantoon flame spawn type");
            int index = LittleEndianBytes.LowByte(parameter);
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
                PhantoonFlameSpawnType.Spiral => 0,
                PhantoonFlameSpawnType.Rain => (ushort)((index & 240) >> 1),
                PhantoonFlameSpawnType.Casual => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameSpawnType)} {(int)type}."),
            };
            AssertEqual(angle, flame.State is PhantoonOrbitingFlameState orbit ? orbit.Angle.RawValue : (ushort)0, "Actual flame initial angle; flames that do not orbit keep none");
            AssertEqual(x, flame.Position.X.Whole, "Actual flame initial X");
            AssertEqual(y, flame.Position.Y.Whole, "Actual flame initial wrapped Y");
            // The native word that kept a rage flame's turn step or a rain flame's fall delay: each is now a named field.
            ushort typedSpeed = flame.State switch
            {
                PhantoonRainFlameState rain => checked((ushort)rain.FallDelay),
                PhantoonOrbitingFlameState rage when type == PhantoonFlameSpawnType.Enraged => unchecked((ushort)rage.AngleStep),
                _ => 0,
            };
            AssertEqual(speed, typedSpeed, "Actual flame turn step or fall delay");
            AssertEqual(0, flame.State is PhantoonOrbitingFlameState start ? start.Radius : 0, "Actual flame initial radius");
            AssertEqual((ushort)0, flame.Velocity.Y.RawValue, "Actual flame initial vertical speed");
            AssertEqual((ushort)0, flame.Position.X.Fraction, "Actual flame initial fractional X");
            AssertEqual((ushort)0, flame.Position.Y.Fraction, "Actual flame initial fractional Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawns.RageAngle(16), "Rage angle bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawns.RainX(9), "Rain column bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawns.SpiralAngle(8), "Spiral angle bounds");
        Console.WriteLine("Phantoon flame spawns: 33 native bytes and 507 real initializers reject migrated table reads.");
    }

}
