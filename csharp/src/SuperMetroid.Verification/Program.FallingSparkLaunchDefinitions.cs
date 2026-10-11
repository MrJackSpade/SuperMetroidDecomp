using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFallingSparkLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new FallingSparkLaunchReadGuard(rom));
        ushort random = 0;
        int advances = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => { advances++; return random; }));
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnFallingSpark", flags)!.CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var move = typeof(RoomEnemySystem).GetMethod("AddFallingSparkHorizontalVelocity", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot>>();
        var source = enemies.Slots[0];
        var projectile = enemies.EnemyProjectiles[^1];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            int offset = raw & 0x1c;
            ushort whole = Word(EnemyRomTablePointersFallingSpark.HorizontalWholeWords + offset);
            ushort fraction = Word(EnemyRomTablePointersFallingSpark.HorizontalFractionWords + offset);
            var compiled = FallingSparkLaunchDefinitions.FromRandom(random);
            AssertEqual(whole, compiled.Whole, "Every RNG word preserves native spark whole velocity");
            AssertEqual(fraction, compiled.Fraction, "Every RNG word preserves native spark fractional velocity");
            projectile.Clear();
            projectile.Velocity = new(new SnesSignedEightEight(0xffff), new SnesSignedEightEight(0xffff));
            source.Position = new(new((ushort)raw, (ushort)raw), new((ushort)(ushort.MaxValue - raw), (ushort)(ushort.MaxValue - raw)));
            advances = 0;
            spawn(source);
            AssertEqual(1, advances, "Successful spark spawn advances RNG once");
            AssertEqual(RoomEnemyProjectileKind.FallingSpark, projectile.Kind, "Reverse allocator owns last free slot");
            AssertEqual(source.Position.X.Whole, projectile.Position.X.Whole, "Spark copies source X");
            AssertEqual(source.Position.X.Fraction, projectile.Position.X.Fraction, "Spark copies source X fraction");
            AssertEqual(unchecked((ushort)(source.Position.Y.Whole + 8)), projectile.Position.Y.Whole, "Spark Y offset wraps");
            AssertEqual(source.Position.Y.Fraction, projectile.Position.Y.Fraction, "Spark copies source Y fraction");
            AssertEqual((ushort)0, projectile.Velocity.X.RawValue, "Spark clears aliased vertical fraction");
            AssertEqual((ushort)0, projectile.Velocity.Y.RawValue, "Spark clears aliased vertical whole");
            AssertEqual(whole, projectile.Variable1, "Actual spark whole launch field");
            AssertEqual(fraction, projectile.Variable0, "Actual spark fractional launch field");
            uint expected = unchecked(((uint)source.Position.X.Whole << 16 | source.Position.X.Fraction) + ((uint)whole << 16 | fraction));
            move(projectile);
            AssertEqual((ushort)(expected >> 16), projectile.Position.X.Whole, "Native signed launch reaches actual wrapped movement");
            AssertEqual((ushort)expected, projectile.Position.X.Fraction, "Native fractional launch reaches actual movement");
        }
        foreach (var occupied in enemies.EnemyProjectiles) occupied.Kind = RoomEnemyProjectileKind.FallingSpark;
        advances = 0;
        ushort previousX = projectile.Position.X.Whole;
        spawn(source);
        AssertEqual(0, advances, "Full projectile pool does not consume spark RNG");
        AssertEqual(previousX, projectile.Position.X.Whole, "Failed allocation preserves occupied projectile");
        Console.WriteLine("Falling sparks: 16 native words including overread, all 65536 RNG selections, real spawns and horizontal motion pass with launch-table reads forbidden.");
    }

    private sealed class FallingSparkLaunchReadGuard(ISnesAddressSpace source) : ISnesMemoryBus, IImportCartridgeSource
    {
        public byte ReadWorkRamByte(int cpuAddress) => ((ISnesMutableMemory)source).ReadWorkRamByte(cpuAddress);
        public byte ReadSaveRamByte(int cpuAddress) => ((ISnesMutableMemory)source).ReadSaveRamByte(cpuAddress);

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) => address is >= 0x86f3d4 and < 0x86f3f4
            ? throw new InvalidOperationException("Unexpected migrated falling-spark launch read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected falling-spark bus write.");
    }
}
