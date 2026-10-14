using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;

/// <summary>Retail-data and guarded-execution checks for Maridia Large Snail's eight directional programs.</summary>
internal static class MaridiaLargeSnailInstructionDefinitionsVerification
{
    /// <summary>Checks selector installation, each program's callbacks and frames, and the forward-roll attack window.</summary>
    internal static void VerifyMaridiaLargeSnailInstructionDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var executedOperands = new HashSet<ushort>();
        var guarded = NewMaridiaLargeSnailInstructionReadGuard(rom);

        for (ushort animationIndex = 0; animationIndex < 8; animationIndex++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2cb77 + animationIndex * 2));
            AssertEqual(expected,
                MaridiaLargeSnailInstructionDefinitions.InstructionPointer(animationIndex),
                $"Maridia Large Snail instruction selector {animationIndex}");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            var state = new MaridiaLargeSnailEnemyState(slot)
            {
                RequestedInstructionListIndex = animationIndex,
                InstalledInstructionListIndex = ushort.MaxValue,
            };
            slot.InstructionTimer = NativeCountdown.Start(0x1234);
            slot.Timer = NativeCountdown.Start(0x5678);

            MaridiaLargeSnailBehavior.InstallMaridiaLargeSnailInstructionList(slot, state);
            AssertEqual(animationIndex, state.InstalledInstructionListIndex,
                $"production Maridia Large Snail installed selector {animationIndex}");
            AssertEqual(expected, slot.CurrentInstruction,
                $"production Maridia Large Snail instruction {animationIndex}");
            AssertEqual((ushort)1, slot.InstructionTimer.RawValue,
                $"production Maridia Large Snail instruction timer {animationIndex}");
            AssertEqual((ushort)0, slot.Timer.RawValue,
                $"production Maridia Large Snail general timer {animationIndex}");
        }

        for (int index = 0;
             index < MaridiaLargeSnailInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Maridia Large Snail mechanics word $A2:{definition.Address:X4}");
        }

        (ushort Program, int Calls, ushort Terminal, bool FinishesAttack, bool PlaysSplash,
            string Name)[] programs =
        [
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle, 2, 0xca4f,
                false, false, "left idle"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftAttacking, 14, 0xca89,
                true, true, "left attack"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards, 9,
                0xca8f, false, false, "left forward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingBackwards, 9,
                0xcab7, false, false, "left backward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightIdle, 2, 0xcadf,
                false, false, "right idle"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking, 14, 0xcb19,
                true, true, "right attack"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingForwards, 9,
                0xcb1f, false, false, "right forward roll"),
            (MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingBackwards, 9,
                0xcb47, false, false, "right backward roll"),
        ];

        foreach (var program in programs)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            Action<RoomEnemySlot> initialize = enemies.MaridiaLargeSnailBehavior.InitializeMaridiaLargeSnail;
            RoomEnemySlot snail = enemies.SlotTable.Slots[0];
            snail.EnemyDefinitionPointer = EnemyDefinitionId.Oum;
            snail.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(snail);

            AssertEqual(MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle,
                snail.CurrentInstruction,
                $"real Maridia Large Snail initializer before {program.Name}");
            ExecuteMaridiaLargeSnailProgram(rom, executedOperands,
                enemies, snail, program.Program, program.Calls);
            AssertEqual(program.Terminal, snail.CurrentInstruction,
                $"Maridia Large Snail {program.Name} terminal instruction");

            MaridiaLargeSnailEnemyState state = (enemies.MaridiaLargeSnailStates[0] ?? throw new InvalidOperationException("Verification requires enemies.MaridiaLargeSnailStates[0]."));
            AssertEqual(program.FinishesAttack, (state ?? throw new InvalidOperationException("Verification requires state.")).AttackAnimationFinished,
                $"Maridia Large Snail {program.Name} completion flag");
            AssertEqual(program.PlaysSplash ? (ushort?)0x000e : null,
                enemies.MaridiaLargeSnailBehavior.LastMaridiaLargeSnailSoundEffect,
                $"Maridia Large Snail {program.Name} splash sound");
            AssertEqual(false, state.AttackAllowsRotation,
                $"Maridia Large Snail {program.Name} leaves rotation disallowed");
        }

        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            Action<RoomEnemySlot> initialize = enemies.MaridiaLargeSnailBehavior.InitializeMaridiaLargeSnail;
            RoomEnemySlot snail = enemies.SlotTable.Slots[0];
            snail.EnemyDefinitionPointer = EnemyDefinitionId.Oum;
            snail.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(snail);
            ExecuteMaridiaLargeSnailProgram(rom, executedOperands,
                enemies,
                snail,
                MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards,
                2);
            AssertTrue((enemies.MaridiaLargeSnailStates[0] ?? throw new InvalidOperationException("Verification requires enemies.MaridiaLargeSnailStates[0].")).AttackAllowsRotation,
                "Maridia Large Snail forward roll enables its attack window");
            ExecuteMaridiaLargeSnailProgram(rom, executedOperands,
                enemies,
                snail,
                snail.CurrentInstruction,
                1);
            AssertEqual(false, (enemies.MaridiaLargeSnailStates[0] ?? throw new InvalidOperationException("Verification requires enemies.MaridiaLargeSnailStates[0].")).AttackAllowsRotation,
                "Maridia Large Snail forward roll closes its attack window");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MaridiaLargeSnailInstructionDefinitions.InstructionPointer(8),
            "Maridia Large Snail selector past table");
        AssertEqual(MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int operandIndex = 0; operandIndex < MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount; operandIndex++)
            AssertTrue(executedOperands.Contains(MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordAddress(operandIndex)),
                "execution covers each authored presentation operand");
        AssertEqual(0, guarded.ObservedWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids compiled Maridia Large Snail mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Maridia Large Snail presentation pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                MaridiaLargeSnailInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Maridia Large Snail selector data is rejected as mechanics");

        _ = ProbeMaridiaLargeSnailInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMaridiaLargeSnailInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Maridia Large Snail allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Maridia Large Snail mechanics lookups allocate no per-frame storage");
        Console.WriteLine(
            "Maridia Large Snail instruction mechanics: all eight native selectors, " +
            "eighty-four compiled mechanics words, all eight programs and callback side " +
            "effects, and sixty executed selectors match cartridge data with runtime reads forbidden.");
    }

    /// <summary>Forces the requested instruction calls while recording each executed visual operand.</summary>
    private static void ExecuteMaridiaLargeSnailProgram(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        RoomEnemySlot snail,
        ushort program,
        int callCount)
    {
        snail.CurrentInstruction = program;
        object?[] arguments =
            [snail, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            snail.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
            VerifyExecutedEnemySelector(rom, snail, executedOperands);
        }
    }

    /// <summary>Alternates warmed idle and attack control lookups for allocation accounting.</summary>
    private static int ProbeMaridiaLargeSnailInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MaridiaLargeSnailInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle
                    : unchecked((ushort)(
                        MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking +
                        0x38)));
        }
        return checksum;
    }

    /// <summary>Rejects compiled selector and control reads while observing frame-operand accesses.</summary>
    private static CartridgeReadGuard NewMaridiaLargeSnailInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa2cb77 and < 0xa2cb87 ||
                    MaridiaLargeSnailInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Maridia Large Snail attempted migrated mechanics read ${address:X6}.")
            .ObserveWords(0xa20000,
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordCount,
                MaridiaLargeSnailInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs the retail-backed Maridia Large Snail instruction migration command.</summary>
    [VerificationCommand("--maridia-large-snail-instruction-definitions")]
    internal static int MaridiaLargeSnailInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyMaridiaLargeSnailInstructionDefinitions), () => VerifyMaridiaLargeSnailInstructionDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
