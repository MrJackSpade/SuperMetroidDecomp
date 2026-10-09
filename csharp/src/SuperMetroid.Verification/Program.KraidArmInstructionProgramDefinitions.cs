using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Kraid arm instruction checks using the retail cartridge image.</summary>
    private static void VerifyKraidArmInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidArmInstructionProgramDefinitions), () => VerifyKraidArmInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// Checks compiled Kraid arm mechanics against cartridge data and executes each arm program
    /// through the production processor while guarding against runtime mechanics reads.
    /// </summary>
    /// <param name="rom">Cartridge address space used as the reference for native instruction data.</param>
    private static void VerifyKraidArmInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(rom));
        Suite(nameof(VerifyKraidArmGeneratedMechanics), () => VerifyKraidArmGeneratedMechanics(rom));
        Suite(nameof(VerifyKraidArmGeneratedPresentation), () => VerifyKraidArmGeneratedPresentation(rom));

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

    /// <summary>Creates a room-enemy system with Kraid body and arm slots wired to the supplied address space.</summary>
    /// <param name="bus">Address space assigned to the enemy system for instruction execution.</param>
    /// <returns>A system whose first two slots represent Kraid's body and arm.</returns>
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

    /// <summary>Runs a selected arm program for a fixed number of processor calls and records executed visual operands.</summary>
    /// <param name="rom">Cartridge data used to validate each selected visual operand.</param>
    /// <param name="executedOperands">Set updated with presentation words selected during execution.</param>
    /// <param name="process">Reflected production instruction-processor method.</param>
    /// <param name="enemies">Room-enemy system that owns the arm slot.</param>
    /// <param name="arm">Arm slot whose instruction pointer and timer are advanced.</param>
    /// <param name="entry">Starting instruction address for the program.</param>
    /// <param name="calls">Number of processor calls to make.</param>
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

    /// <summary>Invokes one production instruction tick and verifies the visual operand it selects.</summary>
    /// <param name="rom">Cartridge data used by the selected-visual verification.</param>
    /// <param name="executedOperands">Set updated with the operand selected during this tick.</param>
    /// <param name="process">Reflected production instruction-processor method.</param>
    /// <param name="enemies">Room-enemy system whose processor is invoked.</param>
    /// <param name="arm">Arm slot advanced by the instruction processor.</param>
    private static void InvokeKraidArmInstructionProcessor(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot arm)
    {
        process.Invoke(
            enemies,
            [arm, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
        Suite(nameof(VerifyExecutedEnemySelector), () => VerifyExecutedEnemySelector(rom, arm, executedOperands));
    }

    /// <summary>Repeats mechanics lookups across both normal and dying program entries for allocation measurement.</summary>
    /// <returns>A checksum that keeps the lookup results observable to the caller.</returns>
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

    /// <summary>Reads one little-endian word from the Kraid arm instruction bank.</summary>
    /// <param name="source">Address space containing the cartridge bytes.</param>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <returns>The combined 16-bit instruction word.</returns>
    private static ushort ReadKraidArmInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Blocks live reads of compiled Kraid arm mechanics and observes reads of presentation operands.</summary>
    /// <param name="source">Backing cartridge address space for permitted reads and forwarded writes.</param>
    private sealed class KraidArmInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Presentation-word addresses observed during guarded instruction execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempts to read a byte owned by the compiled mechanics definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import reads through the same checks and observation as ordinary cartridge reads.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The backing byte when the address is not a forbidden mechanics byte.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled-mechanics reads, records presentation reads, and forwards other reads.</summary>
        /// <param name="address">Address requested from the cartridge.</param>
        /// <returns>The backing byte when the read is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (KraidArmInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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

        /// <summary>Forwards the write to the backing address space.</summary>
        /// <param name="address">Cartridge address to update.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
