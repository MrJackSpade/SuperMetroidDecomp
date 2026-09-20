using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyZeroInstructionProgramDefinitions()
    {
        VerifyZeroInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyZeroInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < ZeroInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ZeroInstructionMechanicsWord definition =
                ZeroInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadZeroInstructionWord(rom, definition.Address),
                $"Zero mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new ZeroInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeCrawler", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ZeroDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies,
                [slot, CrawlerAnimationFamily.Zero, (ushort?)10]);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Zero, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Zero real initializer selects {orientation}");
            ExecuteZeroProgram(enemies, process, slot, callCount: 7);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, enemies.CrawlerStates[0]!.Function,
                $"Zero {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"Zero {orientation} completes and loops all six frames");
        }

        AssertEqual(ZeroInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Zero spritemap operands remain cartridge reads");
        for (int index = 0;
             index < ZeroInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ZeroInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Zero presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Zero mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ZeroInstructionProgramDefinitions.ReadMechanicsWord(
                ZeroInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Zero spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ZeroInstructionProgramDefinitions.ReadMechanicsWord(
                ZeroInstructionProgramDefinitions.UnusedAlternateUpsideRight),
            "retail-unused alternate Zero program is outside production mechanics");

        _ = ProbeZeroInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeZeroInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Zero allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Zero mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Zero instruction mechanics: forty compiled words, all four production surface " +
            "loops and movement callbacks, and twenty-four live spritemap reads pass with " +
            "mechanics bytes forbidden.");
    }

    private static void ExecuteZeroProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeZeroInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ZeroInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ZeroInstructionProgramDefinitions.UpsideRight
                    : ZeroInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    private static ushort ReadZeroInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class ZeroInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ZeroInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Zero mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ZeroInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ZeroInstructionProgramDefinitions.PresentationWordAddress(index);
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
