using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMultiviolaInstructionProgramDefinitions()
    {
        VerifyMultiviolaInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMultiviolaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MultiviolaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MultiviolaInstructionMechanicsWord definition =
                MultiviolaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMultiviolaInstructionWord(rom, definition.Address),
                $"Multiviola mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new MultiviolaInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeMultiviola", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MultiviolaDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.Parameter1 = 0x0058;
        slot.Parameter2 = 1;
        initialize(slot);
        AssertEqual(MultiviolaInstructionProgramDefinitions.Flying,
            slot.CurrentInstruction,
            "real Multiviola initializer installs compiled flying program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < 15; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(MultiviolaInstructionProgramDefinitions.Flying + 4)),
            slot.CurrentInstruction,
            "Multiviola program completes its native goto and first repeated frame");
        AssertEqual(MultiviolaInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Multiviola spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Multiviola mechanics byte");
        AssertThrows<InvalidDataException>(
            () => MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(0xb2de),
            "Multiviola spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(0xb318),
            "unused Multiviola program is rejected as production mechanics");

        _ = ProbeMultiviolaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMultiviolaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Multiviola allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Multiviola mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Multiviola instruction mechanics: 16 compiled words, the complete production " +
            "loop, and 14 live spritemap reads pass with mechanics bytes forbidden.");
    }

    private static int ProbeMultiviolaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MultiviolaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MultiviolaInstructionProgramDefinitions.Flying
                    : (ushort)0xb314);
        }
        return checksum;
    }

    private static ushort ReadMultiviolaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class MultiviolaInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MultiviolaInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Multiviola mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MultiviolaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MultiviolaInstructionProgramDefinitions.PresentationWordAddress(index);
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
