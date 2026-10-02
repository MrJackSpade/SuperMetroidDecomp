using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoStatueInstructionProgramDefinitions() =>
        VerifyBombTorizoStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyBombTorizoStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        VerifyBombTorizoStatueInitialDurations(rom);
        VerifyStatueProgramControlLayout(rom);
        VerifyStatueProgramPresentationLayout();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < BombTorizoStatueInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BombTorizoStatueInstructionMechanicsWord definition =
                BombTorizoStatueInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Bomb Torizo statue mechanics word $86:{definition.Address:X4}");
        }

        var guard = new BombTorizoStatueInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        for (int index = 0;
             index < BombTorizoStatueInstructionProgramDefinitions.ProgramCount;
             index++)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            ushort parameter = unchecked((ushort)(index * 2));
            RoomEnemyProjectileSlot fragment =
                enemies.SpawnBombTorizoStatueBreakingProjectile(
                    new BombTorizoStatueProjectileRequest(
                        BombTorizoStatueFragmentDefinitions.ProjectileDefinition,
                        parameter,
                        PlmBlockX: 10,
                        PlmBlockY: 20)) ??
                throw new InvalidDataException(
                    $"Bomb Torizo statue fragment {index} failed to allocate.");
            ushort program = BombTorizoStatueInstructionProgramDefinitions.Program(index);
            ushort initialDuration =
                (ushort)(rom.ReadByte(0x860000 | program) | rom.ReadByte(0x860000 | (program + 1)) << 8);
            AssertEqual(program, fragment.InstructionPointer,
                $"Bomb Torizo statue fragment {index} uses compiled program");

            int visibleFrames = initialDuration + 0x0070;
            for (int frame = 1; frame <= visibleFrames; frame++)
            {
                process.Invoke(enemies, [fragment, null, (ushort)0, (ushort)0]);
                AssertEqual((ushort)(program + (frame <= initialDuration ? 2 : 13)), fragment.PresentationOperandAddress,
                    "statue fragment presents the native operand on the exact delay/falling tick" );
                AssertTrue(fragment.IsActive,
                    $"Bomb Torizo statue fragment {index} remains active through frame {frame}");
                if (frame == initialDuration + 1)
                {
                    AssertEqual(
                        EnemyProjectileCodePointers.PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling,
                        fragment.PreInstruction,
                        $"Bomb Torizo statue fragment {index} installs falling callback");
                    AssertEqual((ushort)0x0070, fragment.InstructionTimer,
                        $"Bomb Torizo statue fragment {index} installs falling lifetime");
                    AssertEqual(unchecked((ushort)(program + 15)), fragment.InstructionPointer,
                        $"Bomb Torizo statue fragment {index} advances to deletion");
                }
            }
            process.Invoke(enemies, [fragment, null, (ushort)0, (ushort)0]);
            AssertTrue(!fragment.IsActive,
                $"Bomb Torizo statue fragment {index} deletes after {visibleFrames} visible frames");
        }

        AssertEqual(0,
            guard.ObservedPresentationWords.Count,
            "Bomb Torizo statue fragments use installed presentation operands without ROM reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Bomb Torizo statue mechanics byte");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BombTorizoStatueInstructionProgramDefinitions.Program(-1),
            "negative Bomb Torizo statue program index");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BombTorizoStatueInstructionProgramDefinitions.Program(16),
            "high Bomb Torizo statue program index");
        AssertThrows<InvalidDataException>(
            () => BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(0xa4c5),
            "Bomb Torizo statue spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(0xa4c9),
            "Bomb Torizo statue packed sound byte is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(0xa5d3),
            "adjacent drool initialization AI is rejected as statue mechanics");

        _ = ProbeBombTorizoStatueInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBombTorizoStatueInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Bomb Torizo statue allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Bomb Torizo statue mechanics lookups allocate no storage");

        Console.WriteLine(
            "Bomb Torizo statue instruction mechanics: ninety-six compiled words, all " +
            "sixteen real fragment producers, exact staggered waits, falling handoffs, " +
            "and deletions pass with mechanics bytes forbidden.");
    }

    private static void VerifyStatueProgramControlLayout(SuperMetroidAddressSpace rom)
    {
        ushort[] starts = [0xa4c3,0xa4d4,0xa4e5,0xa4f6,0xa507,0xa518,0xa529,0xa53a,
            0xa54b,0xa55c,0xa56d,0xa57e,0xa58f,0xa5a0,0xa5b1,0xa5c2];
        // Native instruction encoding: duration at zero, sound opcode at four,
        // packed sound byte at six, preinstruction opcode/argument at seven/nine,
        // second duration at eleven and delete at fifteen.
        int[] offsets = [0,4,7,9,11,15];
        var words = new HashSet<ushort>();
        var bytes = new HashSet<int>();
        int index = 0;
        foreach (ushort start in starts)
        foreach (int offset in offsets)
        {
            ushort address = (ushort)(start + offset);
            var actual = BombTorizoStatueInstructionProgramDefinitions.MechanicsWord(index++);
            ushort native = (ushort)(rom.ReadByte(0x860000 | address) | rom.ReadByte(0x860000 | (address + 1)) << 8);
            AssertEqual(address, actual.Address, "statue native control layout order");
            AssertEqual(native, actual.Value, "statue enumerated native control word");
            AssertEqual(native, BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(address), "statue direct native control word");
            words.Add(address); bytes.Add(address); bytes.Add(address + 1);
        }
        AssertEqual(index, BombTorizoStatueInstructionProgramDefinitions.MechanicsWordCount, "statue complete control count");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), BombTorizoStatueInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address), "statue full byte ownership excludes packed audio and visuals");
            AssertEqual(bytes.Contains(address), BombTorizoStatueInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1860000 | address), "statue existing high-bit alias");
            AssertTrue(!BombTorizoStatueInstructionProgramDefinitions.IsCompiledMechanicsByte(0x870000 | address), "statue rejects other bank");
        }
        for (int address = 0xa4c1; address <= 0xa5d4; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "statue rejects non-control starts including packed sound bytes");
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(address), "statue distant invalid control");
    }
    private static void VerifyStatueProgramPresentationLayout()
    {
        ushort[] expected = [0xa4c5,0xa4d0,0xa4d6,0xa4e1,0xa4e7,0xa4f2,0xa4f8,0xa503,
            0xa509,0xa514,0xa51a,0xa525,0xa52b,0xa536,0xa53c,0xa547,
            0xa54d,0xa558,0xa55e,0xa569,0xa56f,0xa57a,0xa580,0xa58b,
            0xa591,0xa59c,0xa5a2,0xa5ad,0xa5b3,0xa5be,0xa5c4,0xa5cf];
        AssertEqual(expected.Length, BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount, "statue native visual operand count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress(i), "statue native visual operand order");
        foreach (int index in new[] {int.MinValue,-1,32,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress(index), "statue visual ordinal bounds");
    }
    private static void VerifyBombTorizoStatueInitialDurations(SuperMetroidAddressSpace rom)
    {
        ushort[] starts = [0xa4c3,0xa4d4,0xa4e5,0xa4f6,0xa507,0xa518,0xa529,0xa53a,
            0xa54b,0xa55c,0xa56d,0xa57e,0xa58f,0xa5a0,0xa5b1,0xa5c2];
        AssertEqual(starts.Length, BombTorizoStatueInstructionProgramDefinitions.ProgramCount, "statue native program count");
        for (int i = 0; i < starts.Length; i++)
        {
            int address = 0x860000 | starts[i];
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(starts[i], BombTorizoStatueInstructionProgramDefinitions.Program(i), "statue native start position");
            var enumerated = BombTorizoStatueInstructionProgramDefinitions.MechanicsWord(i * 6);
            AssertEqual(starts[i], enumerated.Address, "statue delay word position");
            AssertEqual(native, enumerated.Value, "statue enumerated native initial duration");
            AssertEqual(native, BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(starts[i]), "statue direct native initial duration");
        }
        foreach (int index in new[] {int.MinValue,-1,16,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueInstructionProgramDefinitions.Program(index), "statue initial duration program domain");
        foreach (int index in new[] {int.MinValue,-1,96,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BombTorizoStatueInstructionProgramDefinitions.MechanicsWord(index), "statue initial duration enumeration domain");
    }
    private static int ProbeBombTorizoStatueInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(
                BombTorizoStatueInstructionProgramDefinitions.Program(index & 15));
        }
        return checksum;
    }

    private sealed class BombTorizoStatueInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (BombTorizoStatueInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Bomb Torizo statue byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = BombTorizoStatueInstructionProgramDefinitions
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
