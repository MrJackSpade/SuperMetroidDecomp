using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCacatacProjectileInstructionProgramDefinitions()
    {
        VerifyCacatacProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCacatacProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < CacatacProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CacatacProjectileInstructionMechanicsWord definition =
                CacatacProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadCacatacProjectileInstructionWord(rom, 0x860000 | definition.Address),
                $"Cacatac spike instruction mechanics word $86:{definition.Address:X4}");
        }

        var guard = new CacatacProjectileInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnCacatacSpike", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 0x1234;
        source.YPosition = 0x5678;

        foreach (CacatacSpikeDirection direction in Enum.GetValues<CacatacSpikeDirection>())
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
                projectile.Clear();

            spawn(source, (ushort)direction);
            RoomEnemyProjectileSlot actual = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
            ushort program = CacatacProjectileDefinitions.InstructionList(direction);
            AssertEqual(program, actual.InstructionPointer,
                $"Cacatac spike {direction} begins at its named program");

            object?[] arguments = [actual, null, (ushort)0, (ushort)0];
            process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(program + 4)), actual.InstructionPointer,
                $"Cacatac spike {direction} installs its live spritemap");
            AssertEqual(ReadCacatacProjectileInstructionWord(rom, 0x860000 | program + 2),
                actual.SpritemapPointer,
                $"Cacatac spike {direction} retains cartridge presentation");

            process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(program + 4)), actual.InstructionPointer,
                $"Cacatac spike {direction} sleeps at its terminal instruction");
            AssertEqual((ushort)0, actual.InstructionTimer,
                $"Cacatac spike {direction} terminal sleep is stable");
        }

        AssertEqual(CacatacProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Cacatac spike spritemap words remain cartridge reads");
        for (int index = 0;
             index < CacatacProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Cacatac spike presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Cacatac spike mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd930),
            "Cacatac spike spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd96a),
            "adjacent Cacatac spike selector table is rejected as mechanics");

        _ = ProbeCacatacProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCacatacProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Cacatac spike allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Cacatac spike mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Cacatac spike instruction mechanics: twenty compiled words, ten complete " +
            "production programs, and ten live spritemap reads pass with mechanics bytes " +
            "forbidden.");
    }

    private static int ProbeCacatacProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CacatacProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                CacatacProjectileInstructionProgramDefinitions.LeftFacingUp);
        }
        return checksum;
    }

    private static ushort ReadCacatacProjectileInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class CacatacProjectileInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CacatacProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Cacatac spike mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0x860000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CacatacProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
