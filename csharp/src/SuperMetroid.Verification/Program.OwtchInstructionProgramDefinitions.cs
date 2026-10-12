using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class OwtchInstructionProgramDefinitionsVerification
{
    internal static void VerifyOwtchInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyOwtchInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < OwtchInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                OwtchInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Owtch instruction mechanics word $A2:{definition.Address:X4}");
        }

        for (int index = 0;
             index < OwtchInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = OwtchInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | address)),
                OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Owtch, address),
                $"compiled Owtch frame selector $A2:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => OwtchStokeVisualDefinitions.FrameAt(EnemyDefinitionId.Owtch, 0xa3ad),
            "Owtch timing word is not a visual selector");

        var guard = NewOwtchInstructionProgramReadGuard(rom);
        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            initialState: OwtchBehaviorState.MovingRight,
            program: OwtchInstructionProgramDefinitions.MovingLeft,
            expectedState: OwtchBehaviorState.MovingLeft,
            expectedLoopCursor: 0xa3b1,
            direction: "left"));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            initialState: OwtchBehaviorState.MovingLeft,
            program: OwtchInstructionProgramDefinitions.MovingRight,
            expectedState: OwtchBehaviorState.MovingRight,
            expectedLoopCursor: 0xa3c3,
            direction: "right"));

        AssertEqual(3, observedFrames.Count, "both Owtch loops visit all three frames");
        foreach (ushort frame in new ushort[] { 0xa589, 0xa590, 0xa597 })
            AssertTrue(observedFrames.Contains(frame), $"Owtch live frame ${frame:X4}");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Owtch mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => OwtchInstructionProgramDefinitions.ReadMechanicsWord(0xa3af),
            "interleaved Owtch spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => OwtchInstructionProgramDefinitions.ReadMechanicsWord(0xa3cf),
            "adjacent Owtch data table is rejected as mechanics");

        _ = ProbeOwtchInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeOwtchInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Owtch allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Owtch mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Owtch instruction mechanics: twelve compiled words, both directional " +
            "callbacks, complete animation loops, and six compiled visual selectors " +
            "pass with source bytes forbidden.");

        void VerifyProgram(
            OwtchBehaviorState initialState,
            ushort program,
            OwtchBehaviorState expectedState,
            ushort expectedLoopCursor,
            string direction)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.OwtchBehavior.InitializeOwtch;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Owtch;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = (ushort)initialState;
            initialize(slot);

            OwtchEnemyState state = enemies.OwtchStates[0]!;
            state.Behavior = initialState;
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            slot.Timer = 0;
            RunOwtchProgram(enemies, slot, 25);

            AssertEqual(expectedState, state.Behavior,
                $"Owtch {direction} callback selects native behavior");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Owtch {direction} program returns through its native goto");
        }

        void RunOwtchProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
            {
                EnemyInstructionRunner.Process(enemies, arguments);
                if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
                    observedFrames.Add(slot.SpritemapPointer);
            }
        }
    }

    private static int ProbeOwtchInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += OwtchInstructionProgramDefinitions.ReadMechanicsWord(
                OwtchInstructionProgramDefinitions.MovingLeft);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewOwtchInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(OwtchInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Owtch instruction byte ${address:X6}.")
            .ForbidWords(0xa20000, OwtchInstructionProgramDefinitionsTooling.PresentationWordCount,
                OwtchInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read compiled Owtch instruction byte ${address:X6}.");

    [VerificationCommand("--lookup-stream-1-owtch-cadence")]
    internal static int LookupStream1OwtchCadenceCommand()
    {
        Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    [VerificationCommand("--owtch-instruction-mechanics")]
    internal static int OwtchInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyOwtchInstructionProgramDefinitions), () => VerifyOwtchInstructionProgramDefinitions());
        return 0;
    }
}
