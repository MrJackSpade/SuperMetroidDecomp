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
using static SuiteTimingVerification;

/// <summary>Represents walking space pirate instruction program definitions verification.</summary>
internal static class WalkingSpacePirateInstructionProgramDefinitionsVerification
{
    /// <summary>Verifies walking space pirate instruction program definitions.</summary>
    internal static void VerifyWalkingSpacePirateInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Verifies walking space pirate instruction program definitions.</summary>
    internal static void VerifyWalkingSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb20000 | definition.Address)),
                $"walking Pirate mechanics word $B2:{definition.Address:X4}");
        }

        var guard = NewWalkingSpacePirateInstructionReadGuard(rom);
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft,
            frames: 81,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.WalkingRight,
            frames: 81,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingLeft,
            frames: 17,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingRight,
            frames: 17,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingLeft,
            frames: 125,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingRight,
            frames: 125,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: false));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: true));

        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            var (enemies, slot, _, samus) = CreateSystem();
            slot.CurrentInstruction = unchecked((ushort)(address - 2));
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, [slot, samus, null, Position16<RoomPixels>.Zero, (ushort)0]);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb20000 | address)),
                slot.SpritemapPointer,
                $"production execution selects walking Pirate frame $B2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled walking Pirate mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xfb52),
            "walking Pirate spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xfc68),
            "adjacent walking Pirate callback code is rejected as mechanics");

        _ = ProbeWalkingSpacePirateInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWalkingSpacePirateInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "walking Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed walking Pirate mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Walking Space Pirate instruction mechanics: 92 compiled words, all eight " +
            "production programs, both three-laser attacks, and fifty compiled frame " +
            "selectors pass with source bytes forbidden.");

        void VerifyAttack(bool movingRight)
        {
            ushort program = movingRight
                ? WalkingSpacePirateInstructionProgramDefinitions.FireLasersRight
                : WalkingSpacePirateInstructionProgramDefinitions.FireLasersLeft;
            ushort expectedCursor = movingRight ? (ushort)0xfc16 : (ushort)0xfb94;
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            samus.Position = samus.Position.WithWholeX(movingRight ? (ushort)0x0180 : (ushort)0x0080);
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, 113);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"walking Pirate {(movingRight ? "right" : "left")} attack loops");
            AssertEqual(3, state.SpawnedLaserCount,
                $"walking Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual((ushort)0x0067, enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect!.Value,
                $"walking Pirate {(movingRight ? "right" : "left")} laser sound");
        }

        void VerifyProgram(
            ushort program,
            int frames,
            ushort expectedCursor,
            WalkingSpacePirateFunction expectedFunction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, frames);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"walking Pirate program $B2:{program:X4} cursor");
            AssertEqual(expectedFunction, state.Function,
                $"walking Pirate program $B2:{program:X4} function");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot,
            WalkingSpacePirateEnemyState State, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.PirateGreyWalking;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            enemies.WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(slot);
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return (enemies, slot, enemies.WalkingSpacePirateStates[0]!, samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            object?[] arguments =
                [slot, samus, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Probes walking space pirate instruction mechanics allocation.</summary>
    private static int ProbeWalkingSpacePirateInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft);
        }
        return checksum;
    }

    /// <summary>New walking space pirate instruction read guard.</summary>
    private static CartridgeReadGuard NewWalkingSpacePirateInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(WalkingSpacePirateInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled walking Pirate mechanics ${address:X6}.")
            .ForbidWords(0xb20000, WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount, WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Production read compiled walking Pirate frame selector ${address:X6}.");

    /// <summary>Walking space pirate instruction mechanics command.</summary>
    [VerificationCommand("--walking-space-pirate-instruction-mechanics")]
    internal static int WalkingSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
