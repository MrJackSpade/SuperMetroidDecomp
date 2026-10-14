using static PolypInstructionProgramDefinitionsVerification;
using static PolypRockInstructionProgramDefinitionsVerification;
using static SingleFrameEnemyVisualsVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Programs;

/// <summary>Checks Kzan's compiled idle program, native frame selection, and live visual boundary.</summary>
internal static class KzanInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the Kzan instruction checks.</summary>
    internal static void VerifyKzanInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares mechanics words and executes initialization through terminal sleep with source reads blocked.</summary>
    internal static void VerifyKzanInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KzanInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                KzanInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | definition.Address)),
                $"Kzan instruction mechanics word $A6:{definition.Address:X4}");
        }

        var guard = NewKzanInstructionProgramGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);
        Action<RoomEnemySlot> initialize = enemies.KzanBehavior.InitializeKzanTop;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.KzanTop;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        slot.Position = slot.Position.WithWholeY(0x0080);
        initialize(slot);
        AssertEqual(KzanInstructionProgramDefinitions.Idle, slot.CurrentInstruction,
            "Kzan initializer installs compiled program identity");

        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        EnemyInstructionRunner.Process(enemies, arguments);
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 |
                KzanInstructionProgramDefinitionsTooling.PresentationWord)), slot.SpritemapPointer,
            "Kzan selects its exact native frame without cartridge reads");
        EnemyInstructionRunner.Process(enemies, arguments);
        AssertEqual(unchecked((ushort)(KzanInstructionProgramDefinitions.Idle + 4)),
            slot.CurrentInstruction,
            "Kzan reaches terminal native sleep");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Kzan mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitionsTooling.PresentationWord),
            "Kzan spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KzanInstructionProgramDefinitions.ReadMechanicsWord(0x8b2f),
            "adjacent Kzan initializer code is rejected as mechanics");

        _ = ProbeKzanInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKzanInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kzan allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kzan mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Kzan instruction mechanics: two compiled words, the complete production " +
            "program and exact frame selection pass with instruction bytes forbidden.");
    }

    /// <summary>Repeatedly looks up the idle mechanics word to expose warmed-call allocation.</summary>
    private static int ProbeKzanInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KzanInstructionProgramDefinitions.ReadMechanicsWord(
                KzanInstructionProgramDefinitions.Idle);
        }
        return checksum;
    }

    /// <summary>Forbids runtime reads of both compiled control bytes and the migrated visual operand.</summary>
    private static CartridgeReadGuard NewKzanInstructionProgramGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(KzanInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Kzan mechanics byte ${address:X6}.")
            .ForbidRange(0xa60000 | KzanInstructionProgramDefinitionsTooling.PresentationWord, 2,
                _ => "Production read the compiled Kzan visual operand.");

    /// <summary>Runs the Kzan, Polyp, and Polyp-rock single-frame program and artwork checks.</summary>
    [VerificationCommand("--single-frame-enemy-visuals")]
    internal static int SingleFrameEnemyVisualsCommand()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions());
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions());
        Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions());
        Suite(nameof(VerifySingleFrameEnemyVisuals), () => VerifySingleFrameEnemyVisuals());
        return 0;
    }

    /// <summary>Runs the focused Kzan instruction mechanics check.</summary>
    [VerificationCommand("--kzan-instruction-mechanics")]
    internal static int KzanInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions());
        return 0;
    }
}
