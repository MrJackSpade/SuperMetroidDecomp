using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySciserInstructionProgramDefinitions()
    {
        VerifySciserInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifySciserInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < SciserInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SciserInstructionMechanicsWord definition =
                SciserInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadSciserInstructionWord(rom, definition.Address),
                $"Sciser mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new SciserInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeCrawler", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.SciserDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies,
                [slot, CrawlerAnimationFamily.Sciser, (ushort?)8]);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Sciser, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Sciser real initializer selects {orientation}");
            ExecuteSciserProgram(enemies, process, slot, callCount: 5);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, enemies.CrawlerStates[0]!.Function,
                $"Sciser {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"Sciser {orientation} completes and loops all four frames");
        }

        AssertEqual(SciserInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Sciser spritemap operands remain cartridge reads");
        for (int index = 0;
             index < SciserInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SciserInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Sciser presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Sciser mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SciserInstructionProgramDefinitions.ReadMechanicsWord(
                SciserInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Sciser spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SciserInstructionProgramDefinitions.ReadMechanicsWord(
                SciserInstructionProgramDefinitions.AdjacentPreviousCode),
            "adjacent pre-Sciser code is rejected as mechanics");

        _ = ProbeSciserInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSciserInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Sciser allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Sciser mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Sciser instruction mechanics: thirty-two compiled words, all four surface " +
            "loops and movement callbacks, and sixteen live spritemap reads pass with " +
            "mechanics bytes forbidden.");
    }

    private static void ExecuteSciserProgram(
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

    private static int ProbeSciserInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SciserInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SciserInstructionProgramDefinitions.UpsideRight
                    : SciserInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    private static ushort ReadSciserInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class SciserInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SciserInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Sciser mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SciserInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SciserInstructionProgramDefinitions.PresentationWordAddress(index);
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
