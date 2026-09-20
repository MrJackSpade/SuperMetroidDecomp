using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidNailInstructionProgramDefinitions()
    {
        VerifyKraidNailInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKraidNailInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(10, KraidNailInstructionProgramDefinitions.MechanicsWordCount,
            "Kraid fingernail compiled mechanics word count");
        AssertEqual(8, KraidNailInstructionProgramDefinitions.PresentationWordCount,
            "Kraid fingernail live presentation word count");
        for (int index = 0;
             index < KraidNailInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KraidNailInstructionMechanicsWord definition =
                KraidNailInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKraidNailInstructionWord(rom, definition.Address),
                $"Kraid fingernail mechanics word $A7:{definition.Address:X4}");
        }

        var guard = new KraidNailInstructionReadGuard(rom);
        foreach (ushort definitionPointer in new ushort[]
                 {
                     RoomEnemySystem.KraidGoodNailDefinition,
                     RoomEnemySystem.KraidBadNailDefinition,
                 })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField(
                "_bus",
                BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definitionPointer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
            slot.InstructionTimer = 1;

            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            for (int frame = 0; frame < 9; frame++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(
                    enemies,
                    [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            }
            AssertEqual(unchecked((ushort)(KraidNailInstructionProgramDefinitions.Loop + 4)),
                slot.CurrentInstruction,
                $"Kraid fingernail ${definitionPointer:X4} loops to its first frame");
        }

        AssertEqual(8, guard.ObservedPresentationWords.Count,
            "all Kraid fingernail spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid fingernail production execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidNailInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidNailInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Kraid fingernail spritemap $A7:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => KraidNailInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid fingernail spritemap $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidNailInstructionProgramDefinitions.ReadMechanicsWord(
                KraidNailInstructionProgramDefinitions.AdjacentPresentationData),
            "adjacent Kraid arm presentation data is rejected as fingernail mechanics");

        _ = ProbeKraidNailInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidNailInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid fingernail allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid fingernail mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid fingernail instruction mechanics: ten compiled words, both nail " +
            "definitions, and eight live spritemap reads pass.");
    }

    private static int ProbeKraidNailInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidNailInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidNailInstructionProgramDefinitions.Loop
                    : unchecked((ushort)(KraidNailInstructionProgramDefinitions.Loop + 0x20)));
        }
        return checksum;
    }

    private static ushort ReadKraidNailInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class KraidNailInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KraidNailInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kraid fingernail mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KraidNailInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KraidNailInstructionProgramDefinitions.PresentationWordAddress(index);
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
