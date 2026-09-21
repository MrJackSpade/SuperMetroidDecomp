using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFakeKraidProjectileInstructionProgramDefinitions()
    {
        VerifyFakeKraidProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyFakeKraidProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FakeKraidProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FakeKraidProjectileInstructionMechanicsWord definition =
                FakeKraidProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadFakeKraidProjectileInstructionWord(rom, definition.Address),
                $"Fake Kraid projectile mechanics word $86:{definition.Address:X4}");
        }

        var guard = new FakeKraidProjectileInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        MethodInfo spawnSpit = typeof(RoomEnemySystem).GetMethod(
            "SpawnFakeKraidSpit",
            instanceFlags)!;
        MethodInfo spawnSpike = typeof(RoomEnemySystem).GetMethod(
            "SpawnFakeKraidSpike",
            instanceFlags)!;

        var source = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new FakeKraidEnemyState(source);

        AssertTrue((bool)spawnSpit.Invoke(
                enemies,
                [source, (short)12, (ushort)0x0180, unchecked((ushort)-0x0200)])!,
            "real Fake Kraid spit producer allocates its projectile");
        source.VariableC = unchecked((ushort)-4);
        AssertTrue((bool)spawnSpike.Invoke(enemies, [source, state, 0])!,
            "real Fake Kraid left-spike producer allocates its projectile");
        source.VariableC = 4;
        AssertTrue((bool)spawnSpike.Invoke(enemies, [source, state, 1])!,
            "real Fake Kraid right-spike producer allocates its projectile");

        var cases = new[]
        {
            (Kind: RoomEnemyProjectileKind.FakeKraidSpit,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.Spit,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpitSleep),
            (Kind: RoomEnemyProjectileKind.FakeKraidSpikeLeft,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpikeLeftSleep),
            (Kind: RoomEnemyProjectileKind.FakeKraidSpikeRight,
                Initial: FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
                Sleep: FakeKraidProjectileInstructionProgramDefinitions.SpikeRightSleep),
        };

        foreach (var testCase in cases)
        {
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
                candidate => candidate.Kind == testCase.Kind);
            AssertEqual(testCase.Initial, projectile.InstructionPointer,
                $"{testCase.Kind} selects its named cartridge program");
            RunForcedTick(projectile);
            AssertEqual(testCase.Sleep, projectile.InstructionPointer,
                $"{testCase.Kind} schedules its terminal sleep");
            RunForcedTick(projectile);
            AssertEqual(testCase.Sleep, projectile.InstructionPointer,
                $"{testCase.Kind} sleeps at the authored opcode");
            AssertEqual((ushort)0, projectile.InstructionTimer,
                $"{testCase.Kind} sleep leaves the timer stopped");

            projectile.InstructionPointer =
                CommonEnemyProjectileInstructionProgramDefinitions.Delete;
            RunForcedTick(projectile);
            AssertTrue(!projectile.IsActive,
                $"{testCase.Kind} shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(
            FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Fake Kraid projectile spritemap operands remain cartridge reads");
        for (int index = 0;
             index < FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = FakeKraidProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Fake Kraid presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Fake Kraid and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9ddc),
            "Fake Kraid spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x9dec),
            "adjacent Fake Kraid initializer is rejected as mechanics");

        _ = ProbeFakeKraidProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFakeKraidProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fake Kraid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fake Kraid projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Fake Kraid projectile instruction mechanics: six compiled words, all three " +
            "real spit/spike producers, three terminal sleeps, shared shot deletion, and " +
            "three live spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeFakeKraidProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FakeKraidProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FakeKraidProjectileInstructionProgramDefinitions.Spit
                    : FakeKraidProjectileInstructionProgramDefinitions.SpikeRight);
        }
        return checksum;
    }

    private static ushort ReadFakeKraidProjectileInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class FakeKraidProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FakeKraidProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fake Kraid mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FakeKraidProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = FakeKraidProjectileInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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
