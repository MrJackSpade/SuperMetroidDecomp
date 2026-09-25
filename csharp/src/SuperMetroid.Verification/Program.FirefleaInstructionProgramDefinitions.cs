using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFirefleaInstructionProgramDefinitions()
    {
        VerifyFirefleaInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyFirefleaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        AssertEqual(54, FirefleaInstructionProgramDefinitions.MechanicsWordCount,
            "Fireflea compiled mechanics word count");
        AssertEqual(52, FirefleaInstructionProgramDefinitions.PresentationWordCount,
            "Fireflea live presentation word count");
        for (int index = 0;
             index < FirefleaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FirefleaInstructionMechanicsWord definition =
                FirefleaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadFirefleaInstructionWord(rom, definition.Address),
                $"Fireflea mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new FirefleaInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.FirefleaDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        typeof(RoomEnemySystem).GetMethod("InitializeFireflea", flags)!
            .Invoke(enemies, [slot]);
        AssertEqual(FirefleaInstructionProgramDefinitions.Loop,
            slot.CurrentInstruction,
            "Fireflea real initializer selects compiled loop");

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0;
             call < FirefleaInstructionProgramDefinitions.FrameCount + 1;
             call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
            ushort operand = FirefleaInstructionProgramDefinitions.PresentationWordAddress(
                call % FirefleaInstructionProgramDefinitions.FrameCount);
            ushort native = ReadFirefleaInstructionWord(rom, operand);
            AssertEqual(native, EnemySpritemapDefinitions.FirefleaFrameAt(operand),
                $"compiled Fireflea visual selector $A3:{operand:X4}");
            AssertEqual(native, slot.SpritemapPointer,
                $"Fireflea production frame {call} matches cartridge selection");
        }
        AssertEqual(unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)),
            slot.CurrentInstruction,
            "Fireflea completes all 52 frames and loops to the first frame");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Fireflea mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Fireflea spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.FirefleaFrameAt(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as presentation");

        _ = ProbeFirefleaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFirefleaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fireflea allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fireflea mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fireflea instruction mechanics: 54 compiled words, the complete 52-frame " +
            "loop, and 52 exact frame selections pass with mechanics and visual source bytes forbidden.");
    }

    private static int ProbeFirefleaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FirefleaInstructionProgramDefinitions.Loop
                    : unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)));
        }
        return checksum;
    }

    private static ushort ReadFirefleaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class FirefleaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FirefleaInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                IsPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fireflea instruction byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        private static bool IsPresentationByte(int address) =>
            (address & 0xff0000) == 0xa30000 &&
            (FirefleaInstructionProgramDefinitions.IsPresentationWord(
                unchecked((ushort)address)) ||
             FirefleaInstructionProgramDefinitions.IsPresentationWord(
                unchecked((ushort)(address - 1))));

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
