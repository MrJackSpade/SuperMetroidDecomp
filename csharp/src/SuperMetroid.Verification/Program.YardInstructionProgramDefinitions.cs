using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly ushort[] YardInstructionEntries =
    [
        YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingUp,
        YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.CrawlingUpsideDownMovingRight,
        YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingRight,
        YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp,
        YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.CrawlingUpsideUpMovingRight,
        YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingRight,
        YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown,
        YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingDown,
        YardInstructionProgramDefinitions.CrawlingUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingUp,
        YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingRight,
        YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingRight,
        YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingDown,
        YardInstructionProgramDefinitions.HidingUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.HiddenUpsideUpMovingLeft,
        YardInstructionProgramDefinitions.HidingUpsideDownMovingLeft,
        YardInstructionProgramDefinitions.HidingUpsideDownMovingRight,
        YardInstructionProgramDefinitions.HidingUpsideUpMovingRight,
        YardInstructionProgramDefinitions.HiddenUpsideUpMovingRight,
        YardInstructionProgramDefinitions.HidingUpsideRightMovingUp,
        YardInstructionProgramDefinitions.HidingUpsideLeftMovingUp,
        YardInstructionProgramDefinitions.HidingUpsideLeftMovingDown,
        YardInstructionProgramDefinitions.HidingUpsideRightMovingDown,
        YardInstructionProgramDefinitions.AirborneFacingLeft,
        YardInstructionProgramDefinitions.AirborneFacingLeftLoop,
        YardInstructionProgramDefinitions.AirborneFacingRight,
        YardInstructionProgramDefinitions.AirborneFacingRightLoop,
    ];

    private static void VerifyYardInstructionProgramDefinitions()
    {
        VerifyYardInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyYardInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        int mechanics = 0;
        int presentation = 0;
        for (int address = 0xc8c6; address < 0xcc36; address += 2)
        {
            if (YardInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address))
            {
                mechanics++;
                AssertEqual(ReadYardInstructionWord(rom, unchecked((ushort)address)),
                    YardInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)address)),
                    $"Yard mechanics word $A3:{address:X4}");
            }
            else
            {
                presentation++;
                AssertThrows<InvalidDataException>(
                    () => YardInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)address)),
                    $"Yard presentation word $A3:{address:X4} is rejected as mechanics");
            }
        }
        AssertEqual(YardInstructionProgramDefinitions.MechanicsWordCount, mechanics,
            "Yard compiled mechanics word count");
        AssertEqual(YardInstructionProgramDefinitions.PresentationWordCount, presentation,
            "Yard live presentation word count");

        var guard = new YardInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateYardInstructionSystem(guard, out RoomEnemySlot yard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (ushort entry in YardInstructionEntries)
        {
            yard.CurrentInstruction = entry;
            for (int call = 0; call < 80; call++)
            {
                yard.InstructionTimer = 1;
                process.Invoke(
                    enemies,
                    [yard, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);
            }
        }

        AssertEqual(YardInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all reachable Yard spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Yard production execution avoids compiled mechanics bytes");
        for (int index = 0; index < YardInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Yard presentation $A3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => YardInstructionProgramDefinitions.ReadMechanicsWord(0xc8c7),
            "odd Yard instruction pointer is rejected");
        AssertThrows<InvalidDataException>(
            () => YardInstructionProgramDefinitions.ReadMechanicsWord(0xcc36),
            "adjacent Yard callback code is rejected as instruction mechanics");

        _ = ProbeYardInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYardInstructionAllocation();
        AssertTrue(checksum != 0, "Yard instruction allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yard instruction mechanics lookups allocate no storage");

        Console.WriteLine(
            "Yard instruction mechanics: 328 compiled words, all 38 authored entries, " +
            "five private callbacks, and 112 live presentation reads pass.");
    }

    private static RoomEnemySystem CreateYardInstructionSystem(
        ISnesAddressSpace bus,
        out RoomEnemySlot yard)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0));

        yard = enemies.Slots[0];
        yard.EnemyDefinitionPointer = RoomEnemySystem.YardDefinition;
        yard.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        var state = new YardEnemyState(yard);
        var states = (YardEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_yardStates", flags)!.GetValue(enemies)!;
        states[0] = state;
        return enemies;
    }

    private static int ProbeYardInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YardInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft
                    : YardInstructionProgramDefinitions.AirborneFacingRightLoop);
        }
        return checksum;
    }

    private static ushort ReadYardInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class YardInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (YardInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yard mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YardInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        YardInstructionProgramDefinitions.PresentationWordAddress(index);
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
