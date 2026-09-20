using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyViolaInstructionProgramDefinitions()
    {
        VerifyViolaInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyViolaInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < ViolaInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ViolaInstructionMechanicsWord definition =
                ViolaInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadViolaInstructionWord(rom, definition.Address),
                $"Viola mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new ViolaInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeCrawler", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ViolaDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            initialize.Invoke(enemies,
                [slot, CrawlerAnimationFamily.Viola, (ushort?)6]);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Viola, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Viola real initializer selects {orientation}");
            ExecuteViolaProgram(enemies, process, slot, callCount: 15);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, enemies.CrawlerStates[0]!.Function,
                $"Viola {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(ViolaInstructionProgramDefinitions.NormalLoop + 4)),
                slot.CurrentInstruction,
                $"Viola {orientation} enters and loops all fourteen shared frames");
        }

        AssertEqual(ViolaInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Viola spritemap operands remain cartridge reads");
        for (int index = 0;
             index < ViolaInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Viola presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Viola mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                ViolaInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Viola spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                ViolaInstructionProgramDefinitions.UnusedXFlipped),
            "retail-unused Viola program is outside production mechanics");

        _ = ProbeViolaInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeViolaInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Viola allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Viola mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Viola instruction mechanics: thirty compiled words, all four surface entries, " +
            "the shared fourteen-frame loop, and fourteen live spritemap reads pass with " +
            "mechanics bytes forbidden.");
    }

    private static void ExecuteViolaProgram(
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

    private static int ProbeViolaInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ViolaInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ViolaInstructionProgramDefinitions.UpsideRight
                    : ViolaInstructionProgramDefinitions.NormalLoop);
        }
        return checksum;
    }

    private static ushort ReadViolaInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class ViolaInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ViolaInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Viola mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ViolaInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ViolaInstructionProgramDefinitions.PresentationWordAddress(index);
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
