using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static CompiledEnemyVisualSelectorAssertionsVerification;
using static SuiteTimingVerification;

/// <summary>Checks Yard's crawling, turning, hiding, and airborne instruction entries against bank A3.</summary>
internal static class YardInstructionProgramDefinitionsVerification
{
    /// <summary>The 38 authored entry pointers used to exercise every Yard instruction-list family.</summary>
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

    /// <summary>Loads the retail cartridge as the oracle for Yard mechanics words and visual selectors.</summary>
    internal static void VerifyYardInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled words, executes all 38 entries, and checks visual selectors without live reads.</summary>
    internal static void VerifyYardInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        int mechanics = 0;
        int presentation = 0;
        for (int address = 0xc8c6; address < 0xcc36; address += 2)
        {
            if (YardInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | address))
            {
                mechanics++;
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | (unchecked((ushort)address))),
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
        AssertEqual(YardInstructionProgramDefinitionsTooling.MechanicsWordCount, mechanics,
            "Yard compiled mechanics word count");
        AssertEqual(YardInstructionProgramDefinitions.PresentationWordCount, presentation,
            "Yard live presentation word count");

        var guard = NewYardInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateYardInstructionSystem(guard, out RoomEnemySlot yard);
        foreach (ushort entry in YardInstructionEntries)
        {
            yard.CurrentInstruction = entry;
            for (int call = 0; call < 80; call++)
            {
                yard.InstructionTimer = 1;
                enemies.Instructions.ProcessInstructions(yard, null, null, Position16<RoomPixels>.Zero, (ushort)0);
            }
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Yard execution uses compiled spritemap selectors");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Yard production execution avoids compiled mechanics bytes");
        for (int index = 0; index < YardInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = YardInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Yard,
                0xa3, address, $"Yard $A3:{address:X4}");
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
            "five private callbacks, and 112 compiled visual selectors pass.");
    }

    /// <summary>Creates a bank A3 Yard slot with deterministic random input and its behavior state installed.</summary>
    internal static RoomEnemySystem CreateYardInstructionSystem(
        ISnesAddressSpace bus,
        out RoomEnemySlot yard)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

        yard = enemies.SlotTable.Slots[0];
        yard.EnemyDefinitionPointer = EnemyDefinitionId.Yard;
        yard.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        var state = new YardEnemyState(yard);
        var states = (YardEnemyState?[])enemies.YardBehavior._yardStates;
        states[0] = state;
        return enemies;
    }

    /// <summary>Alternates crawling and airborne words repeatedly to expose mechanics lookup allocations.</summary>
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

    /// <summary>Rejects compiled mechanics reads and optionally rejects visual operand reads as well.</summary>
    internal static CartridgeReadGuard NewYardInstructionReadGuard(ISnesAddressSpace source, bool forbidPresentation = false)
    {
        var guard = new CartridgeReadGuard(source).Forbid(YardInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled Yard mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa30000, YardInstructionProgramDefinitions.PresentationWordCount, YardInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Installed Yard read presentation byte ${address:X6}.")
            : guard.ObserveWords(0xa30000, YardInstructionProgramDefinitions.PresentationWordCount, YardInstructionProgramDefinitions.PresentationWordAddress);
    }

    /// <summary>Runs the Yard instruction comparison from the verification command dispatcher.</summary>
    [VerificationCommand("--yard-instruction-program-definitions")]
    internal static int YardInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyYardInstructionProgramDefinitions), () => VerifyYardInstructionProgramDefinitions());
        return 0;
    }
}
