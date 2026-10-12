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
using static ImportedSpritemapsVerification;
using static KraidArmCollisionDefinitionsVerification;
using static KraidArmGeneratedProgramsVerification;
using static SuiteTimingVerification;

internal static class KraidArmInstructionProgramDefinitionsVerification
{
    internal static void VerifyKraidArmInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKraidArmInstructionProgramDefinitions), () => VerifyKraidArmInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyKraidArmInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(rom));
        Suite(nameof(VerifyKraidArmGeneratedMechanics), () => VerifyKraidArmGeneratedMechanics(rom));
        Suite(nameof(VerifyKraidArmGeneratedPresentation), () => VerifyKraidArmGeneratedPresentation(rom));

        var guard = NewKraidArmInstructionGuard(rom);
        var executedOperands = new HashSet<ushort>();
        RoomEnemySystem enemies = CreateKraidArmInstructionSystem(guard);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot arm = enemies.Slots[1];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        body.Health = 100;
        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.Normal, calls: 19);
        AssertEqual(unchecked((ushort)(KraidArmInstructionProgramDefinitions.Normal + 4)),
            arm.CurrentInstruction,
            "healthy Kraid arm completes its normal loop");

        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.Slow, calls: 19);
        AssertEqual(unchecked((ushort)(KraidArmInstructionProgramDefinitions.Slow + 4)),
            arm.CurrentInstruction,
            "low-health Kraid arm completes its slow loop");

        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.RisingOrSinking, calls: 19);
        AssertEqual(unchecked((ushort)(
                KraidArmInstructionProgramDefinitions.RisingOrSinking + 4)),
            arm.CurrentInstruction,
            "Kraid arm completes its rising/sinking loop");

        RunKraidArmProgram(rom, executedOperands, process, enemies, arm,
            KraidArmInstructionProgramDefinitions.DyingOrPreparingToLunge, calls: 4);
        AssertEqual((ushort)0x8afc,
            arm.CurrentInstruction,
            "Kraid arm dying/lunge program reaches terminal sleep");

        body.Health = 51;
        arm.CurrentInstruction = 0x8a3b;
        arm.InstructionTimer = 1;
        InvokeKraidArmInstructionProcessor(rom, executedOperands, process, enemies, arm);
        AssertEqual(unchecked((ushort)(KraidArmInstructionProgramDefinitions.Slow + 4)),
            arm.CurrentInstruction,
            "below-half-health callback enters slow arm program");

        AssertEqual(0, guard.ObservedWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(KraidArmInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid arm execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidArmInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidArmInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Kraid arm presentation $A7:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid arm presentation $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidArmInstructionProgramDefinitions.ReadMechanicsWord(
                KraidArmInstructionProgramDefinitions.AdjacentLintProgram),
            "adjacent Kraid lint program is rejected as arm mechanics");

        _ = ProbeKraidArmInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidArmInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid arm allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid arm mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid arm instruction mechanics: 66 compiled words, all four programs, " +
            "the half-health handoff, and 57 native sprite selections pass with zero live reads.");
    }

    private static RoomEnemySystem CreateKraidArmInstructionSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField(
            "_bus",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, bus);
        var state = new KraidEnemyState();
        state.InitialHealth = 104;
        typeof(RoomEnemySystem).GetField(
            "<Kraid>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, state);

        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.Kraid;
        RoomEnemySlot arm = enemies.Slots[1];
        arm.EnemyDefinitionPointer = EnemyDefinitionId.KraidArm;
        arm.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
        return enemies;
    }

    private static void RunKraidArmProgram(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot arm,
        ushort entry,
        int calls)
    {
        arm.CurrentInstruction = entry;
        arm.InstructionTimer = 1;
        for (int call = 0; call < calls; call++)
        {
            arm.InstructionTimer = 1;
            InvokeKraidArmInstructionProcessor(rom, executedOperands, process, enemies, arm);
        }
    }

    private static void InvokeKraidArmInstructionProcessor(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot arm)
    {
        process.Invoke(
            enemies,
            [arm, null, null, Position16<RoomPixels>.Zero, (ushort)0]);
        Suite(nameof(VerifyExecutedEnemySelector), () => VerifyExecutedEnemySelector(rom, arm, executedOperands));
    }

    private static int ProbeKraidArmInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidArmInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidArmInstructionProgramDefinitions.Normal
                    : KraidArmInstructionProgramDefinitions.DyingOrPreparingToLunge);
        }
        return checksum;
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid arm instruction words")]
    internal static ushort ReadKraidArmInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa70000 | address);

    private static CartridgeReadGuard NewKraidArmInstructionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(KraidArmInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Kraid arm mechanics byte ${address:X6}.")
            .ObserveWords(0xa70000, KraidArmInstructionProgramDefinitions.PresentationWordCount, KraidArmInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--kraid-arm-instruction-program-definitions")]
    internal static int KraidArmInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyKraidArmInstructionProgramDefinitions), () => VerifyKraidArmInstructionProgramDefinitions());
        return 0;
    }
}
