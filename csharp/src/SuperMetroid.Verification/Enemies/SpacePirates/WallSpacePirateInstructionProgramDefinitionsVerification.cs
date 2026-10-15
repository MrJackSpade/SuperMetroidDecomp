using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

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
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
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
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, level, 70);
            AssertEqual(expectedCursor, state.Program,
                $"wall Pirate {(movingRight ? "right" : "left")} attack sleeps");
            AssertEqual(expectedFunction, state.Function,
                $"wall Pirate {(movingRight ? "right" : "left")} attack jump handoff");
            AssertEqual(1, state.SpawnedLaserCount,
                $"wall Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual((ushort)0x0066, (enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect.")),
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
            slot.InstructionTimer = NativeCountdown.Start(1);
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
            slot.InstructionTimer = NativeCountdown.Start(1);
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

        (RoomEnemySystem Enemies, RoomEnemySlot Slot,
            WallSpacePirateEnemyState State, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.PirateGreyWall;
            slot.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xb2,
                Damage = 20,
            };
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.Radius = new(8, 16);
            slot.Parameter2 = 0x0080;
            enemies.WallSpacePirateBehavior.InitializeWallSpacePirate(slot);
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return (enemies, slot, (enemies.WallSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized WallSpacePirate state.")), samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            RoomLevelData level,
            int frames)
        {
            object?[] arguments =
                [slot, samus, level, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Creates unobstructed terrain so each wall Pirate program can advance without collision.</summary>
    private static RoomLevelData CreateOpenWallPirateRoom()
    {
        const int width = 32;
        const int height = 32;
        int blockCount = width * height;
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[blockCount], new byte[blockCount], new ushort[blockCount]), new byte[8]);
    }

    /// <summary>Creates a solid row that triggers the wall Pirate's downward-climb reversal.</summary>
    private static RoomLevelData CreateWallPirateFloorRoom()
    {
        const int width = 32;
        const int height = 32;
        var foreground = new ushort[width * height];
        for (int column = 0; column < width; column++)
            foreground[17 * width + column] = 0x8000;
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(foreground, new byte[foreground.Length], new ushort[foreground.Length]), new byte[8]);
    }

    /// <summary>Runs the standalone wall Space Pirate instruction verification command.</summary>
    [VerificationCommand("--wall-space-pirate-instruction-mechanics")]
    internal static int WallSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyWallSpacePirateInstructionProgramDefinitions), () => VerifyWallSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
