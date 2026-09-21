using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresFallingDebrisInstructionProgramDefinitions() =>
        VerifyCeresFallingDebrisInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyCeresFallingDebrisInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresFallingDebrisInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CeresFallingDebrisInstructionMechanicsWord definition =
                CeresFallingDebrisInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Ceres falling-debris mechanics word $86:{definition.Address:X4}");
        }

        var guard = new CeresFallingDebrisInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;

        foreach ((bool dark, RoomEnemyProjectileKind kind, ushort program, ushort sleep) in
                 new[]
                 {
                     (false, RoomEnemyProjectileKind.CeresFallingDebrisLight,
                         CeresFallingDebrisInstructionProgramDefinitions.Light, (ushort)0x9754),
                     (true, RoomEnemyProjectileKind.CeresFallingDebrisDark,
                         CeresFallingDebrisInstructionProgramDefinitions.Dark, (ushort)0x975a),
                 })
        {
            RoomEnemySystem enemies = NewSystem();
            enemies.SpawnCeresFallingDebris(96, dark);
            RoomEnemyProjectileSlot debris = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == kind);
            AssertEqual(program, debris.InstructionPointer,
                "real Ceres debris producer selects the named pose");
            RunForcedTick(enemies, debris);
            AssertEqual(sleep, debris.InstructionPointer,
                "Ceres debris displays its pose before terminal sleep");
            RunForcedTick(enemies, debris);
            AssertEqual(sleep, debris.InstructionPointer,
                "Ceres debris terminal sleep retains its own opcode");
            AssertEqual((ushort)0, debris.InstructionTimer,
                "Ceres debris terminal sleep stores the native zero timer");

            debris.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
            debris.InstructionTimer = 1;
            RunForcedTick(enemies, debris);
            AssertTrue(!debris.IsActive,
                "Ceres debris shot reaction reaches the compiled shared delete program");
        }

        AssertEqual(CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "both Ceres debris spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared-delete debris mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(0x9752),
            "Ceres debris spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(0x974e),
            "word before Ceres debris programs is rejected as mechanics");

        _ = ProbeCeresFallingDebrisInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresFallingDebrisInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres debris allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ceres debris mechanics lookups allocate no storage");

        Console.WriteLine(
            "Ceres falling-debris instruction mechanics: four compiled words, both real " +
            "producers, terminal sleeps, shared deletion, and two live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        void RunForcedTick(RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = 1;
            process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
        }
    }

    private static int ProbeCeresFallingDebrisInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresFallingDebrisInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? CeresFallingDebrisInstructionProgramDefinitions.Light
                    : CeresFallingDebrisInstructionProgramDefinitions.Dark);
        }
        return checksum;
    }

    private sealed class CeresFallingDebrisInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CeresFallingDebrisInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres debris mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresFallingDebrisInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = CeresFallingDebrisInstructionProgramDefinitions
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
