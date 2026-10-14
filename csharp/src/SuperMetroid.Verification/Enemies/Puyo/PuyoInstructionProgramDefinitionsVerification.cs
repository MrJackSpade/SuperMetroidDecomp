using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Puyo grounded and airborne instruction timing and selected frames against bank $A2.</summary>
internal static class PuyoInstructionProgramDefinitionsVerification
{
    /// <summary>Grounded animation loops whose goto instructions must remain executable from compiled data.</summary>
    internal static readonly ushort[] PuyoGroundedInstructionPrograms =
    [
        PuyoInstructionProgramDefinitions.GroundedFast,
        PuyoInstructionProgramDefinitions.GroundedMedium,
        PuyoInstructionProgramDefinitions.GroundedSlow,
    ];

    /// <summary>Airborne poses whose final instruction sleeps after selecting a compiled frame.</summary>
    internal static readonly ushort[] PuyoAirborneInstructionPrograms =
    [
        PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4,
        PuyoInstructionProgramDefinitions.RightFrame1LeftFrame3,
        PuyoInstructionProgramDefinitions.Frame2,
        PuyoInstructionProgramDefinitions.RightFrame3LeftFrame1,
        PuyoInstructionProgramDefinitions.RightFrame4LeftFrame0,
    ];

    /// <summary>Loads the retail cartridge as the oracle for Puyo instruction mechanics.</summary>
    internal static void VerifyPuyoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the compiled words, grounded loops, airborne poses, and absence of runtime cartridge reads.</summary>
    internal static void VerifyPuyoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PuyoInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PuyoInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPuyoInstructionWord(rom, definition.Address),
                $"Puyo mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewPuyoInstructionReadGuard(rom, forbidPresentation: true);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.PuyoBehavior.InitializePuyo);

        RoomEnemySlot puyo = enemies.SlotTable.Slots[0];
        puyo.EnemyDefinitionPointer = EnemyDefinitionId.Puyo;
        puyo.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(puyo);
        AssertEqual(PuyoInstructionProgramDefinitions.GroundedFast,
            puyo.CurrentInstruction,
            "real Puyo initializer installs compiled fast grounded program");

        object?[] processArguments =
            [puyo, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        foreach (ushort program in PuyoGroundedInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, processArguments, puyo, 5);
            AssertEqual(unchecked((ushort)(program + 4)),
                puyo.CurrentInstruction,
                $"Puyo grounded program $A2:{program:X4} completes goto and repeats");
        }

        foreach (ushort program in PuyoAirborneInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, processArguments, puyo, 2);
            AssertEqual(unchecked((ushort)(program + 4)),
                puyo.CurrentInstruction,
                $"Puyo airborne pose $A2:{program:X4} reaches terminal sleep");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "all Puyo spritemap operands select compiled presentation frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Puyo mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Puyo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                PuyoInstructionProgramDefinitions.FirstAdjacentDefinition),
            "adjacent Puyo hop definitions are rejected as instruction mechanics");

        _ = ProbePuyoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePuyoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Puyo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Puyo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Puyo instruction mechanics: 28 compiled words, all three grounded loops, " +
            "all five airborne poses, and 17 compiled visual selectors pass with mechanics " +
            "bytes forbidden.");
    }

    /// <summary>Advances a Puyo instruction program by expiring its instruction timer before each runner call.</summary>
    internal static void ExecutePuyoInstructionCalls(
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Alternates two compiled mechanics words repeatedly to expose allocation in the lookup path.</summary>
    private static int ProbePuyoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PuyoInstructionProgramDefinitions.GroundedFast
                    : PuyoInstructionProgramDefinitions.LastSleepOpcode);
        }
        return checksum;
    }

    /// <summary>Reads a Puyo instruction word from bank A2 of the cartridge oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Puyo instruction words")]
    internal static ushort ReadPuyoInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa20000 | address);

    /// <summary>Rejects reads of compiled Puyo mechanics and optionally rejects visual selector reads as well.</summary>
    internal static CartridgeReadGuard NewPuyoInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false)
    {
        static ushort SelectorWord(int address) => Enumerable.Range(0, PuyoInstructionProgramDefinitionsTooling.PresentationWordCount)
            .Select(PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress)
            .First(word => unchecked((ushort)address) == word || unchecked((ushort)address) == unchecked((ushort)(word + 1)));
        CartridgeReadGuard guard = new CartridgeReadGuard(source)
            .Forbid(PuyoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Puyo mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa20000, PuyoInstructionProgramDefinitionsTooling.PresentationWordCount, PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Installed Puyo read cartridge visual selector $A2:{SelectorWord(address):X4}.")
            : guard.ObserveWords(0xa20000, PuyoInstructionProgramDefinitionsTooling.PresentationWordCount, PuyoInstructionProgramDefinitionsTooling.PresentationWordAddress);
    }

    /// <summary>Runs the Puyo instruction mechanics verification from the command dispatcher.</summary>
    [VerificationCommand("--puyo-instruction-mechanics")]
    internal static int PuyoInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPuyoInstructionProgramDefinitions), () => VerifyPuyoInstructionProgramDefinitions());
        return 0;
    }
}
