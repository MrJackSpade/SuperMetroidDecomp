using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static AssertionsVerification;

internal static class SbugMovementDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Sbug movement definitions")]
    internal static void VerifySbugMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0xa3a111;
        const int activationTable = 0xa3a121;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            NewSbugMovementReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeSbug", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var runMain = typeof(RoomEnemySystem).GetMethod("RunSbugMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState, RoomLevelData?>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort directionIndex = 0; directionIndex < 16; directionIndex++)
        {
            int address = instructionTable + (directionIndex >> 1) * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, SbugMovementDefinitions.FacingInstructionList(directionIndex),
                $"compiled Sbug facing selector {directionIndex}");
        }

        for (byte behavior = 0; behavior <= (byte)SbugActivationBehavior.MoveAwayFromSamus; behavior++)
        {
            int address = activationTable + behavior * 2;
            var native = (SbugEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)behavior),
                $"compiled Sbug activation selector {behavior}");
        }

        for (ushort selector = 0; selector < 8; selector++)
        {
            byte angle = unchecked((byte)(0x30 + selector * 0x20));
            slot.Parameter1 = (ushort)(angle << 8 | 1);
            slot.Parameter2 = 1;
            initialize(slot);

            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(instructionTable + selector * 2);
            SbugEnemyState state = enemies.SbugStates[0]!;
            AssertEqual(unchecked((ushort)(selector * 2)), state.ForwardInstructionIndex,
                $"Sbug production facing index {selector}");
            AssertEqual(native, slot.CurrentInstruction,
                $"Sbug production facing instruction {selector}");
        }

        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
        };
        slot.Position = slot.Position.WithWhole(samus.Position.Whole);
        for (byte behavior = 0; behavior <= (byte)SbugActivationBehavior.MoveAwayFromSamus; behavior++)
        {
            SbugEnemyState state = enemies.SbugStates[0]!;
            state.Function = SbugEnemyFunction.WaitForSamus;
            slot.Parameter2 = (ushort)(behavior << 8 | 1);
            runMain(slot, samus, null);

            int address = activationTable + behavior * 2;
            var native = (SbugEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, state.Function, $"Sbug production activation {behavior}");
        }

        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingInstructionList(16),
            "Sbug facing selector beyond authored table");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.FacingInstructionList(ushort.MaxValue),
            "Sbug restored facing selector does not read adjacent code");
        AssertThrows<InvalidDataException>(
            () => SbugMovementDefinitions.ActivationFunction((SbugActivationBehavior)7),
            "Sbug activation selector beyond authored table");

        for (int index = 0;
             index < SbugInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SbugInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | definition.Address),
                $"Sbug mechanics word $A3:{definition.Address:X4}");
        }

        var programGuard = NewSbugProgramReadGuard(rom);
        ushort[] entries =
        [
            SbugInstructionProgramDefinitions.Right,
            SbugInstructionProgramDefinitions.UpRight,
            SbugInstructionProgramDefinitions.Up,
            SbugInstructionProgramDefinitions.UpLeft,
            SbugInstructionProgramDefinitions.Left,
            SbugInstructionProgramDefinitions.DownLeft,
            SbugInstructionProgramDefinitions.Down,
            SbugInstructionProgramDefinitions.DownRight,
        ];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
        {
            var programSystem = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                programSystem,
                programGuard);
            RoomEnemySlot programSlot = programSystem.Slots[0];
            programSlot.EnemyDefinitionPointer = entryIndex < 4
                ? EnemyDefinitionId.Sbug
                : EnemyDefinitionId.Sbug2;
            programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            programSlot.CurrentInstruction = entries[entryIndex];
            programSlot.InstructionTimer = 1;
            object?[] arguments =
                [programSlot, null, null, Position16<RoomPixels>.Zero, (ushort)0];

            // Four five-frame entries total 20 frames; the margin proves the terminal
            // goto restarts the production stream rather than merely reaching its target.
            for (int frame = 0; frame < 25; frame++)
                process.Invoke(programSystem, arguments);
        }

        AssertEqual(0, programGuard.ObservedWords.Count,
            "all Sbug presentation selectors are compiled, not reread from cartridge");
        for (int index = 0;
             index < SbugInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SbugInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | address),
                EnemySpritemapDefinitions.SbugFrameAt(address),
                $"compiled Sbug visual selector $A3:{address:X4} matches cartridge");
        }
        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids every compiled Sbug mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SbugInstructionProgramDefinitions.ReadMechanicsWord(0xa073),
            "interleaved Sbug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SbugInstructionProgramDefinitions.ReadMechanicsWord(0xffff),
            "restored pointer outside all Sbug programs fails loudly");

        _ = SbugInstructionProgramDefinitions.ReadMechanicsWord(
            SbugInstructionProgramDefinitions.Right);
        _ = ProbeSbugInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSbugInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Sbug allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Sbug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Sbug movement definitions: eight facing lists, seven activation callbacks, " +
            "48 compiled instruction words, and eight complete loops pass with mechanics " +
            "and all 32 visual-selector reads forbidden.");
    }

    private static int ProbeSbugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SbugInstructionProgramDefinitions.ReadMechanicsWord(
                SbugInstructionProgramDefinitions.Right);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewSbugMovementReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa3a111 and < 0xa3a12f,
            address => $"Sbug movement attempted migrated selector read ${address:X6}.");

    private static CartridgeReadGuard NewSbugProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SbugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Sbug mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, SbugInstructionProgramDefinitions.PresentationWordCount, SbugInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--sbug-instruction-mechanics")]
    internal static int SbugInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySbugMovementDefinitions), () => VerifySbugMovementDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
