using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHZoomerInstructionProgramDefinitions()
    {
        VerifyHZoomerInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyHZoomerInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < HZoomerInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            HZoomerInstructionMechanicsWord definition =
                HZoomerInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadHZoomerInstructionWord(rom, definition.Address),
                $"HZoomer mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new HZoomerInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeHZoomer", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        (CrawlerSurfaceOrientation Orientation, CrawlerEnemyFunction Function)[] cases =
        [
            (CrawlerSurfaceOrientation.UpsideRight,
                CrawlerEnemyFunction.HZoomerCrawlingVertically),
            (CrawlerSurfaceOrientation.UpsideLeft,
                CrawlerEnemyFunction.HZoomerCrawlingVertically),
            (CrawlerSurfaceOrientation.UpsideDown,
                CrawlerEnemyFunction.HZoomerCrawlingHorizontally),
            (CrawlerSurfaceOrientation.UpsideUp,
                CrawlerEnemyFunction.HZoomerCrawlingHorizontally),
        ];

        foreach ((CrawlerSurfaceOrientation orientation, CrawlerEnemyFunction function) in cases)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.HZoomerDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies, [slot]);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.HZoomer, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"HZoomer real initializer selects {orientation}");
            ExecuteHZoomerProgram(enemies, process, slot, callCount: 6);
            AssertEqual(function, enemies.CrawlerStates[0]!.Function,
                $"HZoomer {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"HZoomer {orientation} completes and loops all five frames");
        }

        AssertEqual(HZoomerInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all HZoomer spritemap operands remain cartridge reads");
        for (int index = 0;
             index < HZoomerInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                HZoomerInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads HZoomer presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled HZoomer mechanics byte");

        AssertThrows<InvalidDataException>(
            () => HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                HZoomerInstructionProgramDefinitions.PresentationWordAddress(0)),
            "HZoomer spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                HZoomerInstructionProgramDefinitions.AdjacentFunctionCode),
            "adjacent HZoomer callback implementation is rejected as mechanics");

        _ = ProbeHZoomerInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeHZoomerInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "HZoomer allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed HZoomer mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "HZoomer instruction mechanics: thirty-six compiled words, all four surface " +
            "loops and movement callbacks, and twenty live spritemap reads pass with " +
            "mechanics bytes forbidden.");
    }

    private static void ExecuteHZoomerProgram(
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

    private static int ProbeHZoomerInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? HZoomerInstructionProgramDefinitions.UpsideRight
                    : HZoomerInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    private static ushort ReadHZoomerInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class HZoomerInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (HZoomerInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled HZoomer mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < HZoomerInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        HZoomerInstructionProgramDefinitions.PresentationWordAddress(index);
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
