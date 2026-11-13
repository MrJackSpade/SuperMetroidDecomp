using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon flame spawn tables through real projectile initialization.</summary>
internal static class PhantoonFlameSpawnsVerification
{
    /// <summary>The cartridge's enraged start-angle table: sixteen bytes, entries eight on circling counterclockwise.</summary>
    private const int RageAngleTable = 0x8698b4;

    /// <summary>The cartridge's rain column table: nine screen-X bytes.</summary>
    private const int RainColumnTable = 0x8698f7;

    /// <summary>The cartridge's spiral start-angle table: eight bytes.</summary>
    private const int SpiralAngleTable = 0x869979;

    /// <summary>
    /// Compares the rage waves, spiral and intro starts and the rain columns with the cartridge tables, and 507 spawn
    /// parameters, decoded into the typed starts, through the real producers.
    /// </summary>
    internal static void VerifyCompiledPhantoonFlameSpawns(CartridgeImportAddressSpace rom)
    {
        // The cartridge's even wave counts its index down from six through zero and its odd wave from fifteen through
        // eight; entry seven, which neither countdown reads, has no typed start.
        int[] evenWaveIndices = [6, 5, 4, 3, 2, 1, 0];
        int[] oddWaveIndices = [15, 14, 13, 12, 11, 10, 9, 8];
        AssertEqual(evenWaveIndices.Length, PhantoonFlameSpawns.EvenRageWave.Count, "Even rage wave flame count");
        AssertEqual(oddWaveIndices.Length, PhantoonFlameSpawns.OddRageWave.Count, "Odd rage wave flame count");
        for (int place = 0; place < evenWaveIndices.Length; place++)
            AssertEqual(RageStart(rom, evenWaveIndices[place]), PhantoonFlameSpawns.EvenRageWave[place], $"Even rage wave flame {place}");
        for (int place = 0; place < oddWaveIndices.Length; place++)
            AssertEqual(RageStart(rom, oddWaveIndices[place]), PhantoonFlameSpawns.OddRageWave[place], $"Odd rage wave flame {place}");
        for (int i = 0; i < 9; i++)
            AssertEqual(rom.ReadCartridgeByte(RainColumnTable + i), PhantoonFlameSpawns.RainX(i), "All native rain columns");
        // The cartridge spawns the spiral from direction seven down to zero.
        AssertEqual(8, PhantoonFlameSpawns.SpiralStarts.Count, "Spiral flame count");
        for (int place = 0; place < 8; place++)
            AssertEqual(new TrigonometryAngle(rom.ReadCartridgeByte(SpiralAngleTable + 7 - place)), PhantoonFlameSpawns.SpiralStarts[place], $"Spiral flame {place} start angle");
        // The cartridge starts intro flame n at n eighths of a turn.
        for (int order = 0; order < 8; order++)
            AssertEqual(new TrigonometryAngle(order * 32), PhantoonFlameSpawns.StartingFlameStart(order), $"Intro flame {order} start angle");

        var parameters = Enumerable.Range(0, 16).Select(i => (ushort)(0x200 + i))
            .Concat(Enumerable.Range(0, 8).Select(i => (ushort)(0x600 + i)))
            .Concat(Enumerable.Range(0, 16).SelectMany(delay => Enumerable.Range(0, 9).Select(column => (ushort)(0x400 + delay * 16 + column))))
            .Append((ushort)0).ToArray();
        foreach (ushort origin in new ushort[] { 0, 128, 0xfff8 })
        foreach (ushort parameter in parameters)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            PhantoonPatterns patterns = enemies.PhantoonBehavior.Patterns;
            var body = enemies.SlotTable.Slots[0];
            body.Position = body.Position.WithWhole(new(origin, origin));
            PhantoonFlameSpawnType type = PhantoonNativeWords.FlameSpawnTypes.Decode(
                LittleEndianBytes.HighByte(parameter), "Phantoon flame spawn type");
            int index = LittleEndianBytes.LowByte(parameter);
            // The parameter's index byte decodes into the producer's typed start: a rage flame's start angle and turn, a
            // spiral flame's start angle, or a rain flame's column in its low nibble and fall delay over eight above it.
            bool spawned = type switch
            {
                PhantoonFlameSpawnType.Casual => patterns.SpawnPhantoonCasualFlame(body),
                PhantoonFlameSpawnType.Enraged => patterns.SpawnPhantoonRageFlame(body, RageStart(rom, index)),
                PhantoonFlameSpawnType.Rain => patterns.SpawnPhantoonRainFlame(body, index & 15, (index & 240) >> 1),
                PhantoonFlameSpawnType.Spiral => patterns.SpawnPhantoonSpiralFlame(body, new TrigonometryAngle(rom.ReadCartridgeByte(SpiralAngleTable + index))),
                _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameSpawnType)} {type}."),
            };
            AssertTrue(spawned, "Real Phantoon flame allocation succeeds");
            var flame = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            ushort angle = type switch
            {
                PhantoonFlameSpawnType.Enraged => rom.ReadCartridgeByte(RageAngleTable + index),
                PhantoonFlameSpawnType.Spiral => rom.ReadCartridgeByte(SpiralAngleTable + index),
                PhantoonFlameSpawnType.Casual or PhantoonFlameSpawnType.Rain => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameSpawnType)} {type}."),
            };
            ushort x = type == PhantoonFlameSpawnType.Rain ? rom.ReadCartridgeByte(RainColumnTable + (index & 15)) : origin;
            ushort y = type == PhantoonFlameSpawnType.Rain ? (ushort)40
                : unchecked((ushort)(origin + (type == PhantoonFlameSpawnType.Spiral ? 16 : 32)));
            ushort speed = type switch
            {
                PhantoonFlameSpawnType.Enraged => unchecked((ushort)(index < 8 ? 2 : -2)),
                PhantoonFlameSpawnType.Spiral => 0,
                PhantoonFlameSpawnType.Rain => (ushort)((index & 240) >> 1),
                PhantoonFlameSpawnType.Casual => 0,
                _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameSpawnType)} {type}."),
            };
            AssertEqual(angle, flame.State is PhantoonOrbitingFlameState orbit ? orbit.Angle.RawValue : (ushort)0, "Actual flame initial angle; flames that do not orbit keep none");
            AssertEqual(x, flame.Position.X.Whole, "Actual flame initial X");
            AssertEqual(y, flame.Position.Y.Whole, "Actual flame initial wrapped Y");
            // The native word that kept a rage flame's signed turn step or a rain flame's fall delay: each is now a named
            // field, the rage flame's a turn whose signed step is the native word.
            ushort typedSpeed = flame.State switch
            {
                PhantoonRainFlameState rain => checked((ushort)rain.FallDelay),
                PhantoonOrbitingFlameState rage when type == PhantoonFlameSpawnType.Enraged =>
                    unchecked((ushort)rage.TurnPerUpdate.StepsFrom(TrigonometryAngle.Zero)),
                _ => 0,
            };
            AssertEqual(speed, typedSpeed, "Actual flame turn step or fall delay");
            AssertEqual(0, flame.State is PhantoonOrbitingFlameState start ? start.Radius : 0, "Actual flame initial radius");
            AssertEqual((ushort)0, flame.Velocity.Y.RawValue, "Actual flame initial vertical speed");
            AssertEqual((ushort)0, flame.Position.X.Fraction, "Actual flame initial fractional X");
            AssertEqual((ushort)0, flame.Position.Y.Fraction, "Actual flame initial fractional Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawns.StartingFlameStart(8), "Intro flame bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => PhantoonFlameSpawns.RainX(9), "Rain column bounds");
        Console.WriteLine("Phantoon flame spawns: the rage waves, spiral, intro and rain starts equal the cartridge tables, and 507 decoded parameters initialize real flames.");
    }

    /// <summary>
    /// The typed start the cartridge's rage parameter <paramref name="index"/> names: the table's start angle, circling
    /// clockwise for entries zero through seven and counterclockwise from entry eight on.
    /// </summary>
    private static PhantoonRageFlameStart RageStart(CartridgeImportAddressSpace rom, int index) =>
        new(new TrigonometryAngle(rom.ReadCartridgeByte(RageAngleTable + index)), index >= 8 ? RotationDirection.Counterclockwise : RotationDirection.Clockwise);
}
