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
using static PolypInstructionProgramDefinitionsVerification;
using static PolypRockInstructionProgramDefinitionsVerification;
using static SingleFrameEnemyVisualsVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Represents kzan instruction program definitions verification.</summary>
internal static class KzanInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies kzan instruction program definitions.</summary>
    internal static void VerifyKzanInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies kzan instruction program definitions.</summary>
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

    /// <summary>Probe kzan instruction mechanics allocation.</summary>
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

    /// <summary>New kzan instruction program guard.</summary>
    private static CartridgeReadGuard NewKzanInstructionProgramGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(KzanInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Kzan mechanics byte ${address:X6}.")
            .ForbidRange(0xa60000 | KzanInstructionProgramDefinitionsTooling.PresentationWord, 2,
                _ => "Production read the compiled Kzan visual operand.");

    /// <summary>Single frame enemy visuals command.</summary>
    [VerificationCommand("--single-frame-enemy-visuals")]
    internal static int SingleFrameEnemyVisualsCommand()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions());
        Suite(nameof(VerifyPolypInstructionProgramDefinitions), () => VerifyPolypInstructionProgramDefinitions());
        Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions());
        Suite(nameof(VerifySingleFrameEnemyVisuals), () => VerifySingleFrameEnemyVisuals());
        return 0;
    }

    /// <summary>Kzan instruction mechanics command.</summary>
    [VerificationCommand("--kzan-instruction-mechanics")]
    internal static int KzanInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKzanInstructionProgramDefinitions), () => VerifyKzanInstructionProgramDefinitions());
        return 0;
    }
}
