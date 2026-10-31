using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled wall Space Pirate attack and climbing programs through production enemy behavior.</summary>
internal static class WallSpacePirateInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and runs wall Pirate instruction checks.</summary>
    internal static void VerifyWallSpacePirateInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks attack, wall climb, collision reversal, and visual selection with source reads forbidden.</summary>
    internal static void VerifyWallSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(WallSpacePirateAnimationsVerification.VerifyWallSpacePirateAnimations), () => WallSpacePirateAnimationsVerification.VerifyWallSpacePirateAnimations(rom));
        RoomLevelData level = CreateOpenWallPirateRoom();
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: false));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: true));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateProgram.LandedOnLeftWall,
            expectedFunction: WallSpacePirateFunction.ClimbingLeftWall,
            frames: 12));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateProgram.MovingUpLeftWall,
            expectedFunction: WallSpacePirateFunction.ClimbingLeftWall,
            frames: 260));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateProgram.MovingDownLeftWall,
            expectedFunction: WallSpacePirateFunction.ClimbingLeftWall,
            frames: 260));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateProgram.LandedOnRightWall,
            expectedFunction: WallSpacePirateFunction.ClimbingRightWall,
            frames: 12));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateProgram.MovingDownRightWall,
            expectedFunction: WallSpacePirateFunction.ClimbingRightWall,
            frames: 260));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateProgram.MovingUpRightWall,
            expectedFunction: WallSpacePirateFunction.ClimbingRightWall,
            frames: 260));
        Suite(nameof(VerifyCollisionReversal), () => VerifyCollisionReversal(onRightWall: false));
        Suite(nameof(VerifyCollisionReversal), () => VerifyCollisionReversal(onRightWall: true));
        Suite(nameof(VerifyJumpAngleWrap), () => VerifyJumpAngleWrap(jumpingRight: true));
        Suite(nameof(VerifyJumpAngleWrap), () => VerifyJumpAngleWrap(jumpingRight: false));

        Console.WriteLine("Wall space pirate: programs and their cues pass through real behavior.");

        void VerifyAttack(bool movingRight)
        {
            WallSpacePirateProgram program = movingRight
                ? WallSpacePirateProgram.FireAndJumpRight
                : WallSpacePirateProgram.FireAndJumpLeft;
            var expectedCursor = movingRight
                ? new AnimationProgramCursor<WallSpacePirateProgram>(WallSpacePirateProgram.FireAndJumpRight, 10)
                : new AnimationProgramCursor<WallSpacePirateProgram>(WallSpacePirateProgram.FireAndJumpLeft, 10);
            WallSpacePirateFunction expectedFunction = movingRight
                ? WallSpacePirateFunction.WallJumpingRight
                : WallSpacePirateFunction.WallJumpingLeft;
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            RunFrames(enemies, slot, samus, level, 70);
            AssertEqual(expectedCursor, state.Program,
                $"wall Pirate {(movingRight ? "right" : "left")} attack sleeps");
            AssertEqual(expectedFunction, state.Function,
                $"wall Pirate {(movingRight ? "right" : "left")} attack jump handoff");
            AssertEqual(1, state.SpawnedLaserCount,
                $"wall Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual(EnemySoundEffectDefinitions.WallPirateJumpSound, (enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect.")),
                $"wall Pirate {(movingRight ? "right" : "left")} jump sound");
        }

        void VerifyClimb(
            WallSpacePirateProgram program,
            WallSpacePirateFunction expectedFunction,
            int frames)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            ushort initialY = slot.Position.Y.Whole;
            state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            RunFrames(enemies, slot, samus, level, frames);
            AssertEqual(expectedFunction, state.Function,
                $"wall Pirate program {program} climb function");
            if (frames > 12)
            {
                AssertTrue(slot.Position.Y.Whole != initialY,
                    $"wall Pirate program {program} applies climb displacement");
            }
        }

        void VerifyCollisionReversal(bool onRightWall)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(onRightWall
                ? WallSpacePirateProgram.MovingDownRightWall
                : WallSpacePirateProgram.MovingDownLeftWall);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            RunFrames(
                enemies,
                slot,
                samus,
                CreateWallPirateFloorRoom(),
                11);
            AssertEqual(WallSpacePirateClimbDirection.Up, state.ClimbDirection,
                $"wall Pirate {(onRightWall ? "right" : "left")} collision reverses direction");
            AssertEqual(
                onRightWall
                    ? new AnimationProgramCursor<WallSpacePirateProgram>(WallSpacePirateProgram.MovingUpRightWallLoop, 1)
                    : new AnimationProgramCursor<WallSpacePirateProgram>(WallSpacePirateProgram.MovingUpLeftWallLoop, 1),
                state.Program,
                $"wall Pirate {(onRightWall ? "right" : "left")} collision installs upward list");
        }

        // A fast jump steps its angle four table units per update through the turn's wrap: a rightward jump from a
        // quarter turn down through zero to three quarters, a leftward one from three quarters up through zero to a
        // quarter. It lands on the update the angle reaches its endpoint, thirty-two updates in, and not before.
        void VerifyJumpAngleWrap(bool jumpingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            AssertEqual(4, state.JumpAngleDelta, "the fixture Pirate makes fast jumps");
            RotationAngle start = jumpingRight ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
            WallSpacePirateProgram landing = jumpingRight
                ? WallSpacePirateProgram.LandedOnRightWall
                : WallSpacePirateProgram.LandedOnLeftWall;
            state.Function = jumpingRight ? WallSpacePirateFunction.WallJumpingRight : WallSpacePirateFunction.WallJumpingLeft;
            state.WallJumpArcAngle = start;
            state.WallJumpArcCenter = slot.Position.Whole;
            state.Program = AnimationProgramCursor<WallSpacePirateProgram>.Start(
                jumpingRight ? WallSpacePirateProgram.FireAndJumpRight : WallSpacePirateProgram.FireAndJumpLeft);
            var frame = new ClockedEnemyAiFrame(
                new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus, Level = level },
                new FrameCounters(0));
            for (int update = 1; update <= 32; update++)
            {
                WallSpacePirateBehavior.RunWallSpacePirateMainAi(slot, frame);
                RotationAngle expected = start.AddTableUnits(jumpingRight ? -4 * update : 4 * update);
                AssertEqual(expected, state.WallJumpArcAngle,
                    $"wall Pirate {(jumpingRight ? "right" : "left")} jump angle after update {update} wraps around the turn");
                AssertEqual(update == 32, state.Program == AnimationProgramCursor<WallSpacePirateProgram>.Start(landing),
                    $"wall Pirate {(jumpingRight ? "right" : "left")} jump lands only at its endpoint, update {update}");
            }
            AssertEqual(jumpingRight ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn, state.WallJumpArcAngle,
                $"wall Pirate {(jumpingRight ? "right" : "left")} jump ends at its endpoint angle");
        }

        WallSpacePirateInstructionProgramDefinitionsCreateSystemResult CreateSystem()
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 0;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.PirateGreyWall, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition) with
            {
                Damage = 20,
            });
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.Radius = new(8, 16);
            slot.SetSpawnParameter2(EnemyDefinitionId.PirateGreyWall, 0x0080);
            WallSpacePirateBehavior.InitializeWallSpacePirate(slot);
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return new WallSpacePirateInstructionProgramDefinitionsCreateSystemResult(enemies, slot, (enemies.WallSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized WallSpacePirate state.")), samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            RoomLevelData level,
            int frames)
        {
            object?[] arguments =
                [slot, samus, level, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Creates unobstructed terrain so each wall Pirate program can advance without collision.</summary>
    private static RoomLevelData CreateOpenWallPirateRoom() =>
        VerificationHelpersVerification.CreateEmptyRoom(new(32, 32));

    /// <summary>Creates a solid row that triggers the wall Pirate's downward-climb reversal.</summary>
    private static RoomLevelData CreateWallPirateFloorRoom() =>
        VerificationHelpersVerification.CreateSolidRowsRoom(new(32, 32), firstSolidRow: 17, solidRowCount: 1);

    /// <summary>Runs the standalone wall Space Pirate instruction verification command.</summary>
    [VerificationCommand("--wall-space-pirate-instruction-mechanics")]
    internal static int WallSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
