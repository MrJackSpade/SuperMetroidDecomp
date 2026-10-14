using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

/// <summary>Checks walking Space Pirate movement, flinch, look, and laser lists against bank B2.</summary>
internal static class WalkingSpacePirateInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge as the oracle for Pirate instruction words and frames.</summary>
    internal static void VerifyWalkingSpacePirateInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the typed programs against the cartridge and exercises every program and both three-laser attacks.</summary>
    internal static void VerifyWalkingSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(WalkingSpacePirateAnimationsVerification.VerifyWalkingSpacePirateAnimations), () => WalkingSpacePirateAnimationsVerification.VerifyWalkingSpacePirateAnimations(rom));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateProgram.WalkingLeft,
            frames: 81,
            expectedCursor: new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingLeftLoop, 1),
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateProgram.WalkingRight,
            frames: 81,
            expectedCursor: new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingRightLoop, 1),
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateProgram.FlinchFacingLeft,
            frames: 17,
            expectedCursor: new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingLeftLoop, 1),
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateProgram.FlinchFacingRight,
            frames: 17,
            expectedCursor: new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingRightLoop, 1),
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateProgram.LookingFacingLeft,
            frames: 125,
            expectedCursor: new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingRightLoop, 1),
            expectedFunction: WalkingSpacePirateFunction.WalkingRight));
        Suite(nameof(VerifyProgram), () => VerifyProgram(
            WalkingSpacePirateProgram.LookingFacingRight,
            frames: 125,
            expectedCursor: new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.WalkingLeftLoop, 1),
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: false));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: true));

        Console.WriteLine("Walking space pirate: programs and their cues pass through real behavior.");

        void VerifyAttack(bool movingRight)
        {
            WalkingSpacePirateProgram program = movingRight
                ? WalkingSpacePirateProgram.FireLasersRight
                : WalkingSpacePirateProgram.FireLasersLeft;
            var expectedCursor = movingRight
                ? new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.FireLasersRight, 2)
                : new AnimationProgramCursor<WalkingSpacePirateProgram>(WalkingSpacePirateProgram.FireLasersLeft, 2);
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            samus.Position = samus.Position.WithWholeX(movingRight ? (ushort)0x0180 : (ushort)0x0080);
            state.Program = AnimationProgramCursor<WalkingSpacePirateProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 113);
            AssertEqual(expectedCursor, state.Program,
                $"walking Pirate {(movingRight ? "right" : "left")} attack loops");
            AssertEqual(3, state.SpawnedLaserCount,
                $"walking Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual((ushort)0x0067, (enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect.")),
                $"walking Pirate {(movingRight ? "right" : "left")} laser sound");
        }

        void VerifyProgram(
            WalkingSpacePirateProgram program,
            int frames,
            AnimationProgramCursor<WalkingSpacePirateProgram> expectedCursor,
            WalkingSpacePirateFunction expectedFunction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Program = AnimationProgramCursor<WalkingSpacePirateProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, frames);
            AssertEqual(expectedCursor, state.Program,
                $"walking Pirate program {program} cursor");
            AssertEqual(expectedFunction, state.Function,
                $"walking Pirate program {program} function");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot,
            WalkingSpacePirateEnemyState State, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.PirateGreyWalking;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            enemies.WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(slot);
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return (enemies, slot, (enemies.WalkingSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized WalkingSpacePirate state.")), samus);
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

    /// <summary>Runs walking Pirate instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--walking-space-pirate-instruction-mechanics")]
    internal static int WalkingSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWalkingSpacePirateInstructionProgramDefinitions), () => VerifyWalkingSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
