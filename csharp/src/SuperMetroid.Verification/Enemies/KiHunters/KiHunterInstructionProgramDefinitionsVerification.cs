using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks KiHunter body and wing instruction programs, acid spawning, and compiled visual selectors.</summary>
internal static class KiHunterInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge oracle for KiHunter instruction verification.</summary>
    internal static void VerifyKiHunterInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Exercises thirteen flight, swoop, wing, ground, and spit lists while forbidding migrated bank-$A8 reads.</summary>
    internal static void VerifyKiHunterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyFlying), () => VerifyFlying(KiHunterProgram.FlyingLeft, movingRight: false));
        Suite(nameof(VerifyFlying), () => VerifyFlying(KiHunterProgram.FlyingRight, movingRight: true));
        Suite(nameof(VerifySwoop), () => VerifySwoop(KiHunterProgram.SwoopLeft, movingRight: false));
        Suite(nameof(VerifySwoop), () => VerifySwoop(KiHunterProgram.SwoopRight, movingRight: true));
        Suite(nameof(VerifyWingLoop), () => VerifyWingLoop(KiHunterProgram.WingsLeft, "left"));
        Suite(nameof(VerifyWingLoop), () => VerifyWingLoop(KiHunterProgram.WingsRight, "right"));
        Suite(nameof(VerifyDetachedWings), () => VerifyDetachedWings());
        Suite(nameof(VerifyJump), () => VerifyJump(KiHunterProgram.JumpLeft, "left"));
        Suite(nameof(VerifyJump), () => VerifyJump(KiHunterProgram.JumpRight, "right"));
        Suite(nameof(VerifyLanding), () => VerifyLanding(KiHunterProgram.LandLeft, "left"));
        Suite(nameof(VerifyLanding), () => VerifyLanding(KiHunterProgram.LandRight, "right"));
        Suite(nameof(VerifySpit), () => VerifySpit(KiHunterProgram.SpitLeft, movingRight: false));
        Suite(nameof(VerifySpit), () => VerifySpit(KiHunterProgram.SpitRight, movingRight: true));
        Suite(nameof(VerifySwoopAngularMotion), () => VerifySwoopAngularMotion(swoopLeft: false));
        Suite(nameof(VerifySwoopAngularMotion), () => VerifySwoopAngularMotion(swoopLeft: true));

        Suite(nameof(KiHunterAnimationsVerification.VerifyKiHunterAnimations), () => KiHunterAnimationsVerification.VerifyKiHunterAnimations(rom));

        Console.WriteLine("KiHunter: flight, swoop, wing, detached-wing, jump, landing and spit programs with their cues pass.");

        void VerifyFlying(KiHunterProgram program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            state.Velocity = state.Velocity.WithWholeX(movingRight ? (short)1 : (short)-1);
            Install(body, state, program);
            RunFrames(enemies, body, 7);
            AssertEqual(program, state.Program.Program,
                $"KiHunter {(movingRight ? "right" : "left")} flight remains in steady list");
        }

        void VerifySwoop(KiHunterProgram program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            state.Velocity = state.Velocity.WithWholeX(movingRight ? (short)1 : (short)-1);
            Install(body, state, program);
            RunFrames(enemies, body, 55);
            KiHunterProgram idle = movingRight
                ? KiHunterProgram.FlyingRight
                : KiHunterProgram.FlyingLeft;
            AssertEqual(idle, state.Program.Program,
                $"KiHunter {(movingRight ? "right" : "left")} swoop returns to flight");
        }

        void VerifyWingLoop(KiHunterProgram program, string direction)
        {
            (RoomEnemySystem enemies, _, RoomEnemySlot wings, _, KiHunterEnemyState wingState) = CreatePair();
            Install(wings, wingState, program);
            RunFrames(enemies, wings, 7);
            AssertEqual(program, wingState.Program.Program,
                $"KiHunter {direction} wing loop remains bounded");
        }

        void VerifyDetachedWings()
        {
            (RoomEnemySystem enemies, _, RoomEnemySlot wings, _, KiHunterEnemyState wingState) = CreatePair();
            Install(wings, wingState, KiHunterProgram.DetachedWings);
            RunFrames(enemies, wings, 3);
            AssertAsleep(wingState, KiHunterProgram.DetachedWings, "KiHunter detached wings reach terminal sleep");
        }

        void VerifyJump(KiHunterProgram program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            Install(body, state, program);
            RunFrames(enemies, body, 40);
            AssertEqual(KiHunterEnemyFunction.GroundJump, state.Function,
                $"KiHunter {direction} jump installs ground-jump owner");
            AssertAsleep(state, program, $"KiHunter {direction} jump reaches terminal sleep");
        }

        void VerifyLanding(KiHunterProgram program, string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            Install(body, state, program);
            RunFrames(enemies, body, 45);
            AssertEqual(KiHunterEnemyFunction.GroundWait, state.Function,
                $"KiHunter {direction} landing installs ground-wait owner");
            AssertAsleep(state, program, $"KiHunter {direction} landing reaches terminal sleep");
        }

        void VerifySpit(KiHunterProgram program, bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) =
                CreatePair();
            Install(body, state, program);
            RunFrames(enemies, body, 90);
            AssertEqual(KiHunterEnemyFunction.GroundWait, state.Function,
                $"KiHunter {(movingRight ? "right" : "left")} spit restores ground wait");
            AssertEqual((ushort)24, state.WaitTimer.RawValue,
                $"KiHunter {(movingRight ? "right" : "left")} spit wait timer");
            AssertEqual(Library2Sound.AcidSpit, (enemies.KiHunterBehavior.LastKiHunterSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.KiHunterBehavior.LastKiHunterSoundEffect.")),
                $"KiHunter {(movingRight ? "right" : "left")} spit sound");
            RoomEnemyProjectileKind kind = movingRight
                ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
                : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == kind),
                $"KiHunter {(movingRight ? "right" : "left")} acid spawn");
            AssertAsleep(state, program, $"KiHunter {(movingRight ? "right" : "left")} spit reaches terminal sleep");
        }

        // Drives the swoop through production main AI over open terrain. Each update adds the 16.16 angular
        // acceleration with the fraction carrying into the whole word, caps the whole word alone (the fraction keeps
        // accumulating beneath the cap), steps the angle by the whole word, and ends the swoop on the update the
        // angle reaches a full turn (rightward) or falls below a half turn (leftward). The expectation is the native
        // split-word arithmetic, computed here independently of the production fixed-point type.
        void VerifySwoopAngularMotion(bool swoopLeft)
        {
            (RoomEnemySystem enemies, RoomEnemySlot body, _, KiHunterEnemyState state, _) = CreatePair();
            ushort accelerationWhole = swoopLeft ? KiHunterMotionDefinitions.LeftSwoopAccelerationWhole : (ushort)0;
            ushort accelerationFraction = swoopLeft
                ? KiHunterMotionDefinitions.LeftSwoopAccelerationFraction
                : KiHunterMotionDefinitions.RightSwoopAccelerationFraction;
            ushort cap = swoopLeft ? unchecked((ushort)-2) : (ushort)2;
            ushort angle = swoopLeft ? (ushort)255 : (ushort)128;
            state.Function = KiHunterEnemyFunction.Swooping;
            state.AngularVelocity = SnesSignedSixteenSixteen.Zero;
            state.AngularAcceleration = SnesSignedSixteenSixteen.FromParts(unchecked((short)accelerationWhole), accelerationFraction);
            state.MaximumAngularVelocity = unchecked((short)cap);
            state.SwoopAngle = angle;
            state.AnimationChangeAngle = swoopLeft ? 240 : 144;
            state.SwoopAnimationChanged = false;
            state.SwoopTarget = body.Position.Whole;
            state.SwoopOrigin = body.Position.Whole;
            state.SwoopRadius = new Extent16<RoomPixels>(16, 16);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32),
                new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), new byte[8]);
            var frame = new ClockedEnemyAiFrame(
                new RoomEnemyFrame
                {
                    Camera = Position16<RoomPixels>.Zero,
                    TimeIsFrozen = false,
                    Samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(Position16<RoomPixels>.Zero) },
                    Level = level,
                },
                new FrameCounters(0));

            ushort velocityWhole = 0;
            ushort velocityFraction = 0;
            bool cappedKeepingFraction = false;
            bool ended = false;
            for (int update = 1; update <= 200 && !ended; update++)
            {
                uint fractionSum = (uint)velocityFraction + accelerationFraction;
                velocityWhole = unchecked((ushort)(velocityWhole + accelerationWhole + fractionSum / 65536));
                velocityFraction = unchecked((ushort)fractionSum);
                bool capped = swoopLeft
                    ? unchecked((short)(velocityWhole - cap)) < 0
                    : unchecked((short)(velocityWhole - cap)) >= 0;
                if (capped)
                {
                    velocityWhole = cap;
                    cappedKeepingFraction |= velocityFraction != 0;
                }
                angle = unchecked((ushort)(angle + velocityWhole));
                ended = swoopLeft
                    ? unchecked((short)(angle - 128)) < 0
                    : unchecked((short)(angle - 256)) >= 0;

                enemies.KiHunterBehavior.RunKiHunterMainAi(body, frame);
                string side = swoopLeft ? "left" : "right";
                AssertEqual(SnesSignedSixteenSixteen.FromParts(unchecked((short)velocityWhole), velocityFraction), state.AngularVelocity,
                    $"KiHunter {side} swoop update {update} angular velocity carries and caps only its whole part");
                AssertEqual(angle, state.SwoopAngle, $"KiHunter {side} swoop update {update} steps the angle by the whole velocity");
                AssertEqual(ended ? KiHunterEnemyFunction.FlyingPatrol : KiHunterEnemyFunction.Swooping, state.Function,
                    $"KiHunter {side} swoop update {update} ends only when the native angle test ends it");
            }
            AssertTrue(ended, $"KiHunter {(swoopLeft ? "left" : "right")} swoop ends");
            AssertTrue(cappedKeepingFraction, $"KiHunter {(swoopLeft ? "left" : "right")} swoop caps a velocity whose fraction is kept");
        }

        CreatePairResult CreatePair()
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initializeBody = KiHunterBehavior.InitializeKiHunter;
            Action<RoomEnemySlot> initializeWings = enemies.KiHunterBehavior.InitializeKiHunterWings;

            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.Bind(EnemyDefinitionId.KihunterGreen, body.Definition);
            body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
            // The header's collision half-extents: the terrain movers scan the rows the body spans.
            body.Radius = CompiledRoomEnemyDefinitions.Instance.ReadEnemyDefinition(EnemyDefinitionId.KihunterGreen).Radius;
            body.Position = body.Position.WithWhole(new(0x0080, 0x0080));
            RoomEnemySlot wings = enemies.SlotTable.Slots[1];
            wings.Bind(EnemyDefinitionId.KihunterGreenWings, wings.Definition);
            wings.Bind(wings.DefinitionId, default(RoomEnemyDefinition));
            initializeBody(body);
            initializeWings(wings);
            return new CreatePairResult(enemies, body, wings,
                (enemies.KiHunterStates[0] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter state.")), (enemies.KiHunterStates[1] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter state.")));
        }

        static void Install(RoomEnemySlot slot, KiHunterEnemyState state, KiHunterProgram program)
        {
            state.Program = AnimationProgramCursor<KiHunterProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
            slot.Timer = WrappingCountdown.Start(0);
        }

        // The cursor rests on the program's sleep step.
        static void AssertAsleep(KiHunterEnemyState state, KiHunterProgram program, string message)
        {
            AssertEqual(program, state.Program.Program, message);
            AssertTrue(KiHunterAnimations.Programs.Steps(program)[state.Program.Position] is AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>.HoldStep, message);
        }

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the KiHunter instruction check against the local retail cartridge image.</summary>
    [VerificationCommand("--kihunter-instruction-mechanics")]
    internal static int KihunterInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyKiHunterInstructionProgramDefinitions), () => VerifyKiHunterInstructionProgramDefinitions());
        return 0;
    }
}
