using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyZebetiteInstructionProgramDefinitions()
    {
        VerifyZebetiteInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyZebetiteInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort[] healthByTier = [1000, 700, 500, 300, 100];

        AssertEqual(10, ZebetiteInstructionProgramDefinitions.ProgramCount,
            "Zebetite production program count");
        AssertEqual(20, ZebetiteInstructionProgramDefinitions.MechanicsWordCount,
            "Zebetite compiled mechanics word count");
        AssertEqual(10, ZebetiteInstructionProgramDefinitions.PresentationWordCount,
            "Zebetite live presentation word count");

        for (int index = 0;
             index < ZebetiteInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ZebetiteInstructionMechanicsWord definition =
                ZebetiteInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadZebetiteInstructionWord(rom, definition.Address),
                $"Zebetite mechanics word $A6:{definition.Address:X4}");
        }

        var guard = new ZebetiteInstructionReadGuard(rom);
        MethodInfo select = typeof(RoomEnemySystem).GetMethod(
            "SelectZebetiteHealthAnimation",
            flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int programIndex = 0;
             programIndex < ZebetiteInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            bool pairedSmall = programIndex >= healthByTier.Length;
            int tier = programIndex % healthByTier.Length;
            ZebetiteInstructionProgram expected =
                ZebetiteInstructionProgramDefinitions.ProgramAt(programIndex);

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ZebetiteDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.Health = healthByTier[tier];
            var state = new ZebetiteEnemyState(slot)
            {
                GenerationFlags = pairedSmall ? (ushort)0x8000 : (ushort)0,
            };

            select.Invoke(null, [slot, state]);
            AssertEqual(expected.Entry, slot.CurrentInstruction,
                $"Zebetite program {programIndex} production health selection");

            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            process.Invoke(enemies, arguments);
            AssertEqual(expected.Presentation, guard.LastObservedPresentationWord,
                $"Zebetite program {programIndex} live spritemap read");
            AssertEqual(expected.Sleep, slot.CurrentInstruction,
                $"Zebetite program {programIndex} frame handoff");
            process.Invoke(enemies, arguments);
            AssertEqual(expected.Sleep, slot.CurrentInstruction,
                $"Zebetite program {programIndex} terminal sleep");
        }

        AssertEqual(ZebetiteInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Zebetite spritemap operands remain cartridge reads");
        for (int index = 0;
             index < ZebetiteInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ZebetiteInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Zebetite presentation word $A6:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Zebetite mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(
                ZebetiteInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Zebetite spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(
                ZebetiteInstructionProgramDefinitions.FirstSpritemap),
            "adjacent Zebetite spritemap data is rejected as mechanics");

        _ = ProbeZebetiteInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeZebetiteInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Zebetite allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Zebetite mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Zebetite instruction mechanics: 20 compiled words, all ten health-tier " +
            "programs, and ten live spritemap reads pass with mechanics bytes forbidden.");
    }

    private static int ProbeZebetiteInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ZebetiteInstructionProgramDefinitions.BigHealthAtLeast800
                    : ZebetiteInstructionProgramDefinitions.SmallHealthBelow200);
        }
        return checksum;
    }

    private static ushort ReadZebetiteInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa60000 | address) |
            source.ReadByte(0xa60000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class ZebetiteInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal ushort LastObservedPresentationWord { get; private set; }
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ZebetiteInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Zebetite mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ZebetiteInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ZebetiteInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        LastObservedPresentationWord = presentation;
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
