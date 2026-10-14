using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.CompiledEnemyVisualSelectorAssertionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Dragon's idle, wing, and attack instruction programs against cartridge words and live callbacks.</summary>
internal static class DragonInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the Dragon program comparison.</summary>
    internal static void VerifyDragonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDragonInstructionProgramDefinitions), () => VerifyDragonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks six production loops, attack completion, separated visual selectors, and ROM-free control reads.</summary>
    internal static void VerifyDragonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < DragonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DragonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Dragon instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewDragonInstructionProgramReadGuard(rom);
        (DragonAnimationSelector Selector, ushort Terminal, int Frames)[] programs =
        [
            (DragonAnimationSelector.IdleFacingLeft, 0xe59f, 2),
            (DragonAnimationSelector.IdleFacingRight, 0xe5b1, 2),
            (DragonAnimationSelector.WingsFacingLeft, 0xe5a5, 11),
            (DragonAnimationSelector.WingsFacingRight, 0xe5b7, 11),
            (DragonAnimationSelector.AttackingFacingLeft, 0xe5d5, 47),
            (DragonAnimationSelector.AttackingFacingRight, 0xe5ed, 47),
        ];

        foreach ((DragonAnimationSelector selector, ushort terminal, int frames) in programs)
        {
            RoomEnemySystem enemies =
                CreateDragonProgramSystem(guard, selector, out RoomEnemySlot slot);
            DragonEnemyState state = (enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[0]."));
            AssertEqual(DragonAnimationDefinitions.InstructionList(selector),
                slot.CurrentInstruction, $"Dragon installs {selector} program");
            RunDragonProgram(enemies, slot, frames);
            AssertEqual(terminal, slot.CurrentInstruction,
                $"Dragon {selector} program reaches its native loop/sleep cursor");
            bool attacks = selector is DragonAnimationSelector.AttackingFacingLeft or
                DragonAnimationSelector.AttackingFacingRight;
            AssertEqual(attacks, (state ?? throw new InvalidOperationException("Verification requires state.")).AnimationFinished,
                $"Dragon {selector} attack-completion callback state");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Dragon execution uses compiled spritemap selectors");
        for (int index = 0;
             index < DragonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DragonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.Dragon,
                0xa2, address, $"Dragon $A2:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dragon mechanics byte");

        AssertThrows<InvalidDataException>(
            () => DragonInstructionProgramDefinitions.ReadMechanicsWord(0xe59d),
            "interleaved Dragon spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DragonInstructionProgramDefinitions.ReadMechanicsWord(0xe5ef),
            "adjacent Dragon selector table is rejected as mechanics");

        _ = ProbeDragonInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDragonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dragon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dragon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dragon instruction mechanics: twenty-six compiled words, all six body, " +
            "wing, and attack programs, both completion callbacks, and sixteen compiled " +
            "spritemap selectors pass with mechanics bytes forbidden.");

        static RoomEnemySystem CreateDragonProgramSystem(
            CartridgeReadGuard guard,
            DragonAnimationSelector selector,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.DragonBehavior.InitializeDragon;

            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Dragon;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = selector is DragonAnimationSelector.WingsFacingLeft or
                DragonAnimationSelector.WingsFacingRight ? (ushort)1 : (ushort)0;
            initialize(slot);
            DragonEnemyState state = (enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.DragonStates[0]."));
            (state ?? throw new InvalidOperationException("Verification requires state.")).InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
            state.RequestedInstructionListIndex = selector;
            state.AnimationFinished = false;
            DragonBehavior.InstallDragonInstructionList(slot, state);
            return enemies;
        }

        static void RunDragonProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeats compiled idle-word lookup to detect per-frame allocation.</summary>
    private static int ProbeDragonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DragonInstructionProgramDefinitions.ReadMechanicsWord(
                DragonInstructionProgramDefinitions.IdleFacingLeft);
        }
        return checksum;
    }

    /// <summary>Blocks migrated Dragon mechanics and records visual-selector operand reads.</summary>
    private static CartridgeReadGuard NewDragonInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(DragonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Dragon mechanics byte ${address:X6}.")
            .ObserveWords(0xa20000, DragonInstructionProgramDefinitions.PresentationWordCount,
                DragonInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the focused Dragon instruction check against the retail ROM.</summary>
    [VerificationCommand("--dragon-instruction-mechanics")]
    internal static int DragonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDragonInstructionProgramDefinitions), () => VerifyDragonInstructionProgramDefinitions());
        return 0;
    }
}
