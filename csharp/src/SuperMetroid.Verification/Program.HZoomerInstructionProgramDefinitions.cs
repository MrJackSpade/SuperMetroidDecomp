using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHZoomerInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyHZoomerInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog? artwork = null)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < HZoomerInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                HZoomerInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadHZoomerInstructionWord(rom, definition.Address),
                $"HZoomer mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new HZoomerInstructionReadGuard(rom, forbidPresentation: true);
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
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "HZoomer production loops avoid cartridge visual selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled HZoomer mechanics byte");

        AssertThrows<InvalidDataException>(
            () => HZoomerInstructionProgramDefinitions.ReadMechanicsWord(
                HZoomerInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
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

        Console.WriteLine(artwork is null
            ? "HZoomer instruction mechanics: thirty-six compiled words, four surface " +
              "loops and movement callbacks, and twenty compiled visual selectors pass."
            : "Installed HZoomer: four surface loops execute with cartridge visual " +
              "selector reads forbidden.");
    }

    private static void ExecuteHZoomerProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
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

    private sealed class HZoomerInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (HZoomerInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled HZoomer mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < HZoomerInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        HZoomerInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                        {
                            ForbiddenReadAttempts++;
                            throw new InvalidOperationException(
                                $"HZoomer read visual-selector byte ${address:X6}.");
                        }
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
