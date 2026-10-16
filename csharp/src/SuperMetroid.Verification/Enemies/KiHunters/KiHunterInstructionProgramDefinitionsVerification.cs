using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.KiHunters;
using SuperMetroid.Verification.Harness;

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
            AssertEqual((ushort)0x004c, (enemies.KiHunterBehavior.LastKiHunterSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.KiHunterBehavior.LastKiHunterSoundEffect.")),
                $"KiHunter {(movingRight ? "right" : "left")} spit sound");
            RoomEnemyProjectileKind kind = movingRight
                ? RoomEnemyProjectileKind.KiHunterAcidSpitRight
                : RoomEnemyProjectileKind.KiHunterAcidSpitLeft;
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == kind),
                $"KiHunter {(movingRight ? "right" : "left")} acid spawn");
            AssertAsleep(state, program, $"KiHunter {(movingRight ? "right" : "left")} spit reaches terminal sleep");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Body, RoomEnemySlot Wings,
            KiHunterEnemyState BodyState, KiHunterEnemyState WingState) CreatePair()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            Action<RoomEnemySlot> initializeBody = KiHunterBehavior.InitializeKiHunter;
            Action<RoomEnemySlot> initializeWings = enemies.KiHunterBehavior.InitializeKiHunterWings;

            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.Bind(EnemyDefinitionId.KihunterGreen, body.Definition);
            body.Bind(body.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa8 });
            body.Position = body.Position.WithWhole(new(0x0080, 0x0080));
            RoomEnemySlot wings = enemies.SlotTable.Slots[1];
            wings.Bind(EnemyDefinitionId.KihunterGreenWings, wings.Definition);
            wings.Bind(wings.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa8 });
            initializeBody(body);
            initializeWings(wings);
            return (enemies, body, wings,
                (enemies.KiHunterStates[0] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter state.")), (enemies.KiHunterStates[1] ?? throw new InvalidOperationException("Verification requires the initialized KiHunter state.")));
        }

        static void Install(RoomEnemySlot slot, KiHunterEnemyState state, KiHunterProgram program)
        {
            state.Program = AnimationProgramCursor<KiHunterProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.Timer = NativeCountdown.Start(0);
        }

        // The cursor rests on the program's sleep step.
        static void AssertAsleep(KiHunterEnemyState state, KiHunterProgram program, string message)
        {
            AssertEqual(program, state.Program.Program, message);
            AssertEqual(AnimationProgramStepKind.Hold, KiHunterAnimations.Programs.Steps(program)[state.Program.Position].Kind, message);
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
