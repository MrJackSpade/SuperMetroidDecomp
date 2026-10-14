using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;

/// <summary>Retail-data and guarded-execution checks for escape Etecoon initialization and animation.</summary>
internal static class EscapeEtecoonDefinitionsVerification
{
    /// <summary>Checks three native setup roles and all six masked selector values against production initialization.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for escape Etecoon definitions")]
    internal static void VerifyEscapeEtecoonDefinitions(SuperMetroidAddressSpace rom)
    {
        const int xTable = 0xb3e718;
        const int yTable = 0xb3e71e;
        const int preInstructionTable = 0xb3e724;
        const int instructionTable = 0xb3e72a;
        const int speedTable = 0xb3e730;

        for (int role = 0; role < 3; role++)
        {
            ushort parameter = unchecked((ushort)(role * 2));
            EscapeEtecoonInitialization initialization =
                EscapeEtecoonDefinitions.Initialization(parameter);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((xTable + parameter)), initialization.Position.X,
                $"escape Etecoon role {role} X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((yTable + parameter)), initialization.Position.Y,
                $"escape Etecoon role {role} Y");
            AssertEqual((EscapeEtecoonPreInstruction)CartridgeImportSource.Require(rom).ReadCartridgeWord((preInstructionTable + parameter)),
                initialization.PreInstruction,
                $"escape Etecoon role {role} pre-instruction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + parameter)),
                initialization.InstructionList,
                $"escape Etecoon role {role} instruction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((speedTable + parameter)),
                initialization.HorizontalSpeed,
                $"escape Etecoon role {role} speed");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeEtecoonDefinitions.Initialization(6),
            "escape Etecoon selector after authored roles");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeEtecoonDefinitions.Initialization(0xffff),
            "escape Etecoon wrapping restored selector");

        var guarded = NewEscapeEtecoonDefinitionGuard(rom);
        for (ushort parameter = 0; parameter < 6; parameter++)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            enemies.VerificationServices().HasEvent = (Func<EventNumber, bool>)(_ => false);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = parameter;
            slot.Position = slot.Position.WithWhole(new(0xaaaa, 0xbbbb));
            enemies.EscapeAnimalsBehavior.InitializeEscapeEtecoon(slot);

            int role = parameter >> 1;
            EscapeEtecoonInitialization expected =
                EscapeEtecoonDefinitions.Initialization(parameter);
            EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
                throw new InvalidDataException("Escape Etecoon initializer omitted typed state.");
            AssertEqual(expected.Position.X, slot.Position.X.Whole,
                $"production escape Etecoon selector {parameter} X");
            AssertEqual(expected.Position.Y, slot.Position.Y.Whole,
                $"production escape Etecoon selector {parameter} Y");
            AssertEqual(expected.PreInstruction, state.PreInstruction,
                $"production escape Etecoon selector {parameter} pre-instruction");
            AssertEqual(expected.InstructionList, slot.CurrentInstruction,
                $"production escape Etecoon selector {parameter} instruction");
            AssertEqual(expected.HorizontalSpeed, state.HorizontalSpeed,
                $"production escape Etecoon selector {parameter} speed");
            AssertEqual((EscapeEtecoonRole)(role * 2), (EscapeEtecoonRole)(parameter & 0xfffe),
                $"escape Etecoon selector {parameter} role mask");
        }

        Console.WriteLine(
            "Escape Etecoon definitions: fifteen native values and all six masked retail selectors pass with the authored tables forbidden during production initialization; malformed restored selectors fail explicitly.");
    }

    /// <summary>Loads retail data for the escape Etecoon instruction check.</summary>
    internal static void VerifyEscapeEtecoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEscapeEtecoonInstructionProgramDefinitions), () => VerifyEscapeEtecoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs all seven Etecoon programs, including gratitude displacement and escape handoff, with migrated reads forbidden.</summary>
    internal static void VerifyEscapeEtecoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < EscapeEtecoonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EscapeEtecoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | definition.Address)),
                $"escape Etecoon mechanics word $B3:{definition.Address:X4}");
        }

        var guard = NewEscapeEtecoonDefinitionGuard(rom);
        RoomEnemySystem enemies = CreateEscapeEtecoonProgramSystem(guard);
        RoomEnemySlot etecoon = enemies.SlotTable.Slots[0];
        var selectedPresentation = new HashSet<ushort>();
        ushort[] programs =
        [
            EscapeEtecoonInstructionProgramDefinitions.RunningLeftLowTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningLeftHighTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningRightLowTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningRightHighTide,
            EscapeEtecoonInstructionProgramDefinitions.RunningForEscape,
            EscapeEtecoonInstructionProgramDefinitions.Stationary,
        ];
        foreach (ushort program in programs)
        {
            etecoon.CurrentInstruction = program;
            RunForcedEscapeEtecoonInstructions(enemies, etecoon, 7, rom, selectedPresentation);
        }

        EscapeEtecoonEnemyState state = enemies.EscapeEtecoonStates[0] ??
            throw new InvalidDataException("Escape Etecoon test lost typed state.");
        ushort xBeforeGratitude = etecoon.Position.X.Whole;
        etecoon.CurrentInstruction =
            EscapeEtecoonInstructionProgramDefinitions.ExpressGratitudeThenEscape;
        RunForcedEscapeEtecoonInstructions(enemies, etecoon, 40, rom, selectedPresentation);
        AssertTrue(unchecked((short)(etecoon.Position.X.Whole - xBeforeGratitude)) < 0,
            "escape Etecoon gratitude program applies its repeated left displacement");
        AssertEqual(EscapeEtecoonPreInstruction.EscapeRight, state.PreInstruction,
            "escape Etecoon gratitude program hands off to rightward escape");

        AssertEqual(EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount,
            selectedPresentation.Count,
            "all escape-Etecoon native presentation operands execute");
        AssertEqual(0, guard.ObservedWords.Count,
            "escape-Etecoon uses compiled presentation without ROM reads");
        for (int index = 0;
             index < EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(selectedPresentation.Contains(address),
                $"production selects escape-Etecoon presentation operand $B3:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"escape-Etecoon spritemap $B3:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled escape-Etecoon mechanics byte");
        AssertThrows<InvalidDataException>(
            () => EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeEtecoonInstructionProgramDefinitions.FirstAdjacentCodeRoutine),
            "adjacent escape-Etecoon callback code is rejected as mechanics");

        _ = ProbeEscapeEtecoonInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEscapeEtecoonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "escape-Etecoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed escape-Etecoon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Escape Etecoon instruction mechanics: 63 compiled words, all seven live " +
            "programs and 30 installed native selectors pass with source reads forbidden.");
    }

    /// <summary>Initializes an Etecoon in a guarded bank-$B3 enemy system.</summary>
    private static RoomEnemySystem CreateEscapeEtecoonProgramSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().HasEvent = (Func<EventNumber, bool>)(_ => false);
        RoomEnemySlot etecoon = enemies.SlotTable.Slots[0];
        etecoon.EnemyDefinitionPointer = EnemyDefinitionId.EtecoonEscape;
        etecoon.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
        etecoon.Parameter1 = 0;
        enemies.EscapeAnimalsBehavior.InitializeEscapeEtecoon(etecoon);
        return enemies;
    }

    /// <summary>Forces instruction calls and matches each selected spritemap to its retail operand.</summary>
    private static void RunForcedEscapeEtecoonInstructions(
        RoomEnemySystem enemies,
        RoomEnemySlot etecoon,
        int steps,
        SuperMetroidAddressSpace rom,
        HashSet<ushort> selectedPresentation)
    {
        object?[] arguments =
            [etecoon, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int step = 0; step < steps; step++)
        {
            etecoon.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
            ushort operand = unchecked((ushort)(etecoon.CurrentInstruction - 2));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | operand)),
                etecoon.SpritemapPointer, "escape-Etecoon actual installed native visual selector");
            selectedPresentation.Add(operand);
        }
    }

    /// <summary>Repeats a warmed stationary control lookup for allocation accounting.</summary>
    private static int ProbeEscapeEtecoonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EscapeEtecoonInstructionProgramDefinitions.ReadMechanicsWord(
                EscapeEtecoonInstructionProgramDefinitions.Stationary);
        }
        return checksum;
    }

    /// <summary>Rejects migrated setup and control reads while observing visual-operand accesses.</summary>
    private static CartridgeReadGuard NewEscapeEtecoonDefinitionGuard(ISnesAddressSpace rom) =>
        new CartridgeReadGuard(rom)
            .Forbid(EscapeEtecoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled escape-Etecoon mechanics byte ${address:X6}.")
            .Forbid(address => address is >= 0xb3e718 and < 0xb3e736,
                address => $"Escape Etecoon attempted migrated definition read ${address:X6}.")
            .ObserveWords(0xb30000,
                EscapeEtecoonInstructionProgramDefinitions.PresentationWordCount,
                EscapeEtecoonInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the retail-backed Etecoon setup-definition command.</summary>
    [VerificationCommand("--escape-etecoon-definitions")]
    internal static int EscapeEtecoonDefinitionsCommand()
    {
        Suite(nameof(VerifyEscapeEtecoonDefinitions), () => VerifyEscapeEtecoonDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the retail-backed Etecoon instruction migration command.</summary>
    [VerificationCommand("--escape-etecoon-instruction-mechanics")]
    internal static int EscapeEtecoonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEscapeEtecoonInstructionProgramDefinitions), () => VerifyEscapeEtecoonInstructionProgramDefinitions());
        return 0;
    }
}
