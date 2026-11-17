using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.Artwork;

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
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(WallSpacePirateAnimationsVerification.VerifyWallSpacePirateAnimations), () => WallSpacePirateAnimationsVerification.VerifyWallSpacePirateAnimations(rom));
        RoomLevelData level = CreateOpenWallPirateRoom();
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: false));
        Suite(nameof(VerifyAttack), () => VerifyAttack(movingRight: true));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateSegment.LandedOnLeftWall,
            expectedWall: HorizontalDirection.Left,
            frames: 12));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateSegment.MovingUpLeftWall,
            expectedWall: HorizontalDirection.Left,
            frames: 260));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateSegment.MovingDownLeftWall,
            expectedWall: HorizontalDirection.Left,
            frames: 260));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateSegment.LandedOnRightWall,
            expectedWall: HorizontalDirection.Right,
            frames: 12));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateSegment.MovingDownRightWall,
            expectedWall: HorizontalDirection.Right,
            frames: 260));
        Suite(nameof(VerifyClimb), () => VerifyClimb(
            WallSpacePirateSegment.MovingUpRightWall,
            expectedWall: HorizontalDirection.Right,
            frames: 260));
        Suite(nameof(VerifyCollisionReversal), () => VerifyCollisionReversal(onRightWall: false));
        Suite(nameof(VerifyCollisionReversal), () => VerifyCollisionReversal(onRightWall: true));
        Suite(nameof(VerifyJumpAngleWrap), () => VerifyJumpAngleWrap(jumpingRight: true));
        Suite(nameof(VerifyJumpAngleWrap), () => VerifyJumpAngleWrap(jumpingRight: false));

        Console.WriteLine("Wall space pirate: programs and their cues pass through real behavior.");

        void VerifyAttack(bool movingRight)
        {
            WallSpacePirateSegment program = movingRight
                ? WallSpacePirateSegment.FireAndJumpRight
                : WallSpacePirateSegment.FireAndJumpLeft;
            // The attack has played every beat of its one phase and holds on its last pose.
            Animation<WallSpacePirateSegment> attack = WallSpacePirateAnimations.StartingWith(program);
            var expected = new AnimationPlayback<WallSpacePirateSegment>(attack, AnimationPlace.HoldingIn(0, attack.Phases[0].BeatCount));
            HorizontalDirection expectedWall = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            (RoomEnemySystem enemies, RoomEnemyPlace place,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Playback = new(WallSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, level, 70);
            AssertEqual(expected, state.Playback,
                $"wall Pirate {(movingRight ? "right" : "left")} attack sleeps");
            AssertEqual(WallSpacePirateAction.WallJumping, state.Action,
                $"wall Pirate {(movingRight ? "right" : "left")} attack jump handoff");
            AssertEqual(expectedWall, state.Wall,
                $"wall Pirate {(movingRight ? "right" : "left")} attack jumps to the other wall (#2165)");
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser),
                $"wall Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual(EnemySoundEffectDefinitions.WallPirateJumpSound, (enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a pending space pirate sound.")),
                $"wall Pirate {(movingRight ? "right" : "left")} jump sound");
        }

        void VerifyClimb(
            WallSpacePirateSegment program,
            HorizontalDirection expectedWall,
            int frames)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            ushort initialY = place.Position.Y.Whole;
            state.Playback = new(WallSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, level, frames);
            AssertEqual(WallSpacePirateAction.Climbing, state.Action,
                $"wall Pirate program {program} climb function");
            AssertEqual(expectedWall, state.Wall,
                $"wall Pirate program {program} clings to its wall (#2165)");
            if (frames > 12)
            {
                AssertTrue(place.Position.Y.Whole != initialY,
                    $"wall Pirate program {program} applies climb displacement");
            }
        }

        void VerifyCollisionReversal(bool onRightWall)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            state.Playback = new(WallSpacePirateAnimations.StartingWith(onRightWall
                ? WallSpacePirateSegment.MovingDownRightWall
                : WallSpacePirateSegment.MovingDownLeftWall), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(
                enemies,
                place,
                samus,
                CreateWallPirateFloorRoom(),
                11);
            AssertEqual(WallSpacePirateClimbDirection.Up, state.ClimbDirection,
                $"wall Pirate {(onRightWall ? "right" : "left")} collision reverses direction");
            // The contact continues at the upward climb, whose repeated loop has shown its first pose of its first play.
            WallSpacePirateSegment climb = onRightWall ? WallSpacePirateSegment.MovingUpRightWall : WallSpacePirateSegment.MovingUpLeftWall;
            WallSpacePirateSegment loop = onRightWall ? WallSpacePirateSegment.ClimbingUpRightWall : WallSpacePirateSegment.ClimbingUpLeftWall;
            AssertEqual(
                new AnimationPlayback<WallSpacePirateSegment>(WallSpacePirateAnimations.StartingWith(climb),
                    AnimationSegments<WallSpacePirateSegment, EnemyFrameId, WallSpacePirateCue>.PlaceOf(WallSpacePirateAnimations.StartingWith(climb), loop, 1)
                        .WithPlaysLeft(WallSpacePirateAnimations.StartingWith(climb).Phases.Single(phase => phase.Segment == loop && phase.IsRepeated).Plays)),
                state.Playback,
                $"wall Pirate {(onRightWall ? "right" : "left")} collision installs upward list");
        }

        // A fast jump steps its angle four table units per update through the turn's wrap: a rightward jump from a
        // quarter turn down through zero to three quarters, a leftward one from three quarters up through zero to a
        // quarter. It lands on the update the angle reaches its endpoint, thirty-two updates in, and not before.
        void VerifyJumpAngleWrap(bool jumpingRight)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place,
                WallSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            AssertEqual(4, state.JumpAngleDelta, "the fixture Pirate makes fast jumps");
            RotationAngle start = jumpingRight ? RotationAngle.QuarterTurn : RotationAngle.ThreeQuarterTurn;
            WallSpacePirateSegment landing = jumpingRight
                ? WallSpacePirateSegment.LandedOnRightWall
                : WallSpacePirateSegment.LandedOnLeftWall;
            state.Action = WallSpacePirateAction.WallJumping;
            state.Wall = jumpingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            state.WallJumpArcAngle = start;
            state.WallJumpArcCenter = place.Position.Whole;
            state.Playback = new(WallSpacePirateAnimations.StartingWith(
                jumpingRight ? WallSpacePirateSegment.FireAndJumpRight : WallSpacePirateSegment.FireAndJumpLeft), AnimationPlace.StartOfAnimation);
            var frame = new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(level, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
                new FrameCounters(0));
            for (int update = 1; update <= 32; update++)
            {
                WallSpacePirateBehavior.RunWallSpacePirateMainAi(place, frame);
                RotationAngle expected = start.TurnedBySteps(jumpingRight ? -4 * update : 4 * update);
                AssertEqual(expected, state.WallJumpArcAngle,
                    $"wall Pirate {(jumpingRight ? "right" : "left")} jump angle after update {update} wraps around the turn");
                AssertEqual(update == 32, state.Playback == new AnimationPlayback<WallSpacePirateSegment>(WallSpacePirateAnimations.StartingWith(landing), AnimationPlace.StartOfAnimation),
                    $"wall Pirate {(jumpingRight ? "right" : "left")} jump lands only at its endpoint, update {update}");
            }
            AssertEqual(jumpingRight ? RotationAngle.ThreeQuarterTurn : RotationAngle.QuarterTurn, state.WallJumpArcAngle,
                $"wall Pirate {(jumpingRight ? "right" : "left")} jump ends at its endpoint angle");
        }

        WallSpacePirateInstructionProgramDefinitionsCreateSystemResult CreateSystem()
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 0;
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.PirateGreyWall, place.Definition);
            place.Bind(place.DefinitionId, default(RoomEnemyDefinition) with
            {
                Damage = 20,
            });
            place.Position = place.Position.WithWhole(new(0x0100, 0x0100));
            place.Radius = new(8, 16);
            place.SetSpawnParameter2(EnemyDefinitionId.PirateGreyWall, 0x0080);
            WallSpacePirateBehavior.InitializeWallSpacePirate(place);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)),
            };
            return new WallSpacePirateInstructionProgramDefinitionsCreateSystemResult(enemies, place, (enemies.WallSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized WallSpacePirate state.")), samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemyPlace place,
            SamusState samus,
            RoomLevelData level,
            int frames)
        {
            object?[] arguments =
                [place, samus, level, Position16<RoomPixels>.Zero, default(GameplayAction)];
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
