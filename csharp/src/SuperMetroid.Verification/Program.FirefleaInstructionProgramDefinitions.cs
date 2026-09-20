using System.Reflection;
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
        }
        AssertEqual(unchecked((ushort)(FirefleaInstructionProgramDefinitions.Loop + 4)),
            slot.CurrentInstruction,
            "Fireflea completes all 52 frames and loops to the first frame");

        AssertEqual(FirefleaInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Fireflea spritemap operands remain cartridge reads");
        for (int index = 0;
             index < FirefleaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                FirefleaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Fireflea presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Fireflea mechanics byte");

        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Fireflea spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(
                FirefleaInstructionProgramDefinitions.AdjacentUnusedData),
            "adjacent unused Fireflea data is rejected as mechanics");

        _ = ProbeFirefleaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFirefleaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fireflea allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fireflea mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fireflea instruction mechanics: 54 compiled words, the complete 52-frame " +
            "loop, and 52 live spritemap reads pass with mechanics bytes forbidden.");
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
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FirefleaInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fireflea mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FirefleaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        FirefleaInstructionProgramDefinitions.PresentationWordAddress(index);
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
