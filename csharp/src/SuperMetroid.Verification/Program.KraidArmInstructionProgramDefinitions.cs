using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidArmInstructionProgramDefinitions()
    {
        VerifyKraidArmInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKraidArmInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        VerifyKraidArmCollisionDefinitions(rom);
        VerifyKraidArmGeneratedMechanics(rom);
        VerifyKraidArmGeneratedPresentation(rom);

        var guard = new KraidArmInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        RoomEnemySystem enemies = CreateKraidArmInstructionSystem(guard);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot arm = enemies.Slots[1];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        body.Health = 100;
        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.Normal, calls: 19);
        AssertEqual(unchecked((ushort)(KraidArmInstructionProgramDefinitions.Normal + 4)),
            arm.CurrentInstruction,
            "healthy Kraid arm completes its normal loop");

        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.Slow, calls: 19);
        AssertEqual(unchecked((ushort)(KraidArmInstructionProgramDefinitions.Slow + 4)),
            arm.CurrentInstruction,
            "low-health Kraid arm completes its slow loop");

        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.RisingOrSinking, calls: 19);
        AssertEqual(unchecked((ushort)(
                KraidArmInstructionProgramDefinitions.RisingOrSinking + 4)),
            arm.CurrentInstruction,
            "Kraid arm completes its rising/sinking loop");

        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.DyingOrPreparingToLunge, calls: 4);
        AssertEqual((ushort)0x8afc,
            arm.CurrentInstruction,
            "Kraid arm dying/lunge program reaches terminal sleep");

        body.Health = 51;
        arm.CurrentInstruction = 0x8a3b;
        arm.InstructionTimer = 1;
        InvokeKraidArmInstructionProcessor(rom, executedOperands, process, enemies, arm);
        AssertEqual(unchecked((ushort)(KraidArmInstructionProgramDefinitions.Slow + 4)),
            arm.CurrentInstruction,
            "below-half-health callback enters slow arm program");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(KraidArmInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid arm execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidArmInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidArmInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Kraid arm presentation $A7:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid arm presentation $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(
                KraidArmInstructionProgramDefinitions.AdjacentLintProgram),
            "adjacent Kraid lint program is rejected as arm mechanics");

        _ = ProbeKraidArmInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidArmInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid arm allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid arm mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid arm instruction mechanics: 66 compiled words, all four programs, " +
            "the half-health handoff, and 57 native sprite selections pass with zero live reads.");
    }

    private static RoomEnemySystem CreateKraidArmInstructionSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField(
            "_bus",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, bus);
        var state = new KraidEnemyState();
        state.InitialHealth = 104;
        typeof(RoomEnemySystem).GetField(
            "_kraidState",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, state);

        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.KraidDefinition;
        RoomEnemySlot arm = enemies.Slots[1];
        arm.EnemyDefinitionPointer = RoomEnemySystem.KraidArmDefinition;
        arm.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
        return enemies;
    }

    private static void RunKraidArmProgram(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot arm,
        ushort entry,
        int calls)
    {
        arm.CurrentInstruction = entry;
        arm.InstructionTimer = 1;
        for (int call = 0; call < calls; call++)
        {
            arm.InstructionTimer = 1;
            InvokeKraidArmInstructionProcessor(rom, executedOperands, process, enemies, arm);
        }
    }

    private static void InvokeKraidArmInstructionProcessor(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot arm)
    {
        process.Invoke(
            enemies,
            [arm, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
        VerifyExecutedEnemySelector(rom, arm, executedOperands);
    }

    private static int ProbeKraidArmInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidArmInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidArmInstructionProgramDefinitions.Normal
                    : KraidArmInstructionProgramDefinitions.DyingOrPreparingToLunge);
        }
        return checksum;
    }

    private static ushort ReadKraidArmInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class KraidArmInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (KraidArmInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kraid arm mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KraidArmInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KraidArmInstructionProgramDefinitions.PresentationWordAddress(index);
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
