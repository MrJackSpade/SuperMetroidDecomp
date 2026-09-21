using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoStatueInstructionProgramDefinitions() =>
        VerifyBombTorizoStatueInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyBombTorizoStatueInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
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
                BombTorizoStatueInstructionProgramDefinitions.MechanicsWord(index * 6).Value;
            AssertEqual(program, fragment.InstructionPointer,
                $"Bomb Torizo statue fragment {index} uses compiled program");

            int visibleFrames = initialDuration + 0x0070;
            for (int frame = 1; frame <= visibleFrames; frame++)
            {
                process.Invoke(enemies, [fragment, null, (ushort)0, (ushort)0]);
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

        AssertEqual(BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Bomb Torizo statue-fragment spritemaps remain cartridge reads");
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
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

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
