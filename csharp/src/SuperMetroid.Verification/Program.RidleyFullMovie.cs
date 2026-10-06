using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyRidleySpinFireball()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var body = runtime.Enemies.Slots[0]; var state = runtime.Enemies.Ridley!; var samus = runtime.Samus!;
        var tick = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyPogo", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var sample in new[] { (0x7f, false, 2, false), (0x80, false, 2, true),
            (0xff, true, 2, false), (0xff, false, 1, false), (0x180, false, 0, true) })
        {
            runtime.System.SetRandomNumber((ushort)sample.Item1);
            state.Roaring = sample.Item2; state.FacingDirection = (ushort)sample.Item3;
            state.FunctionTimer = 20; samus.Pose = SamusPoseIds.SpinJumpRightPose;
            body.CurrentInstruction = RidleyMovieMemory.RidleyRightFlyingSleep;
            body.InstructionTimer = 9; body.Timer = 11;
            tick.Invoke(runtime.Enemies, [body, state, samus, true]);
            AssertEqual(sample.Item4 ? RidleyInstructionProgramDefinitions.Fireballing : RidleyMovieMemory.RidleyRightFlyingSleep,
                body.CurrentInstruction, "native spin-response fireball threshold/roar/facing gate");
            AssertEqual(sample.Item4 ? (ushort)1 : (ushort)9, body.InstructionTimer, "native fireball restarts instruction timer only when admitted");
            AssertEqual(sample.Item4 ? (ushort)0 : (ushort)11, body.Timer, "native fireball clears loop counter only when admitted");
            AssertEqual((ushort)sample.Item1, runtime.System.RandomNumber, "native fireball admission does not generate random state");
        }
        Console.WriteLine("Ridley spin-response fireball: native RNG threshold, roar/turn gates and instruction reset pass.");
    }

    private static void VerifyRidleyContactOrdering()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        runtime.Camera!.SetPosition(0, 158);
        var body = runtime.Enemies.Slots[0]; var state = runtime.Enemies.Ridley!; var samus = runtime.Samus!;
        body.XPosition = 139; body.YPosition = 282;
        body.SpritemapPointer = 0xe9e9; body.Properties = 0x3800;
        state.FightMode = 1; state.MovementAnimationEnabled = 1; state.FacingDirection = 0;
        state.Function = RidleyAiFunction.NorfairCarryRelease; state.FunctionTimer = 10;
        state.HorizontalVelocity = 0xfc00; state.VerticalVelocity = 0x0300;
        state.TailFunctionIndex = 0;
        samus.XPosition = 110; samus.YPosition = 300; samus.Pose = 0x69;
        samus.Health = 399; samus.EquippedItems = (ushort)SamusEquipmentFlags.GravitySuit;
        samus.InvincibilityTimer = 0; samus.RefreshCollisionRadii(bus);
        var enemyPhase = typeof(SuperMetroidRuntime).GetMethod("RunEnemyMainPhase", BindingFlags.Instance | BindingFlags.NonPublic)!;
        enemyPhase.Invoke(runtime, null);
        AssertEqual((ushort)399, samus.Health, "source4234 body entering overlap cannot damage before next contact pass");
        AssertTrue(body.XPosition < 139, "fixture advances body into collision boundary");
        body.SpritemapPointer = 0xe9e9;
        runtime.Enemies.ResolveRidleySamusContact(samus, 0);
        AssertEqual((ushort)359, samus.Health, "next pre-AI body overlap remains damaging");
        Console.WriteLine("Ridley body contact: pre-movement boundary and subsequent damaging overlap pass.");
    }

    private static void VerifyRidleySwoopTimer()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var body = runtime.Enemies.Slots[0]; var state = runtime.Enemies.Ridley!;
        state.Function = RidleyAiFunction.NorfairSwoopSetup; state.FunctionTimer = 112;
        body.XPosition = 212; body.YPosition = 200; state.FacingDirection = 2;
        var dispatch = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", BindingFlags.Instance | BindingFlags.NonPublic)!;
        dispatch.Invoke(runtime.Enemies, [body, state, runtime.Samus, (ushort)0, runtime.LevelData]);
        AssertEqual((ushort)112, state.FunctionTimer, "native swoop setup retains general AI timer");
        AssertEqual((ushort)10, state.SwoopPhaseTimer, "native swoop setup initializes independent countdown");
        state.Function = RidleyAiFunction.NorfairSwoopAimDown; state.SwoopPhaseTimer = 1;
        dispatch.Invoke(runtime.Enemies, [body, state, runtime.Samus, (ushort)0, runtime.LevelData]);
        AssertEqual((ushort)0, state.SwoopPhaseTimer, "native swoop decrements its own timer");
        AssertEqual((ushort)112, state.FunctionTimer, "native swoop countdown retains general timer");
        dispatch.Invoke(runtime.Enemies, [body, state, runtime.Samus, (ushort)0, runtime.LevelData]);
        AssertEqual((ushort)20, state.SwoopPhaseTimer, "native next swoop phase installs its own duration");
        AssertEqual((ushort)112, state.FunctionTimer, "native phase transition retains general timer");
        Console.WriteLine("Ridley swoop: independent countdown and retained general AI timer pass.");
    }

    private static void VerifyRidleyDeathFinish()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var body = runtime.Enemies.Slots[0]; var state = runtime.Enemies.Ridley!;
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.Enemies, runtime.Samus);
        state.Function = RidleyAiFunction.NorfairDeathFinish; state.FunctionTimer = 0;
        var dispatch = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", BindingFlags.Instance | BindingFlags.NonPublic)!;
        dispatch.Invoke(runtime.Enemies, [body, state, runtime.Samus, (ushort)0, runtime.LevelData]);
        AssertEqual(RidleyAiFunction.NorfairDeathComplete, state.Function, "death finish selects native C600 return");
        AssertTrue(body.Properties.HasAny(EnemyProperties.Deleted), "death finish publishes deletion");
        AssertTrue(state.BossDefeatPublished && runtime.Enemies.RidleyDeathDropRequested, "death finish publishes boss defeat and drops");
        ushort random = runtime.System.RandomNumber;
        dispatch.Invoke(runtime.Enemies, [body, state, runtime.Samus, (ushort)0, runtime.LevelData]);
        AssertEqual(random, runtime.System.RandomNumber, "terminal return does not spawn another drop scatter");
        AssertEqual(ushort.MaxValue, state.FunctionTimer, "terminal return retains expired countdown");
        Console.WriteLine("Ridley death finish: native terminal return, deletion, defeat/drop publication and inert subsequent dispatch pass.");
    }

    private static void VerifyRidleyGrabEntry()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var body = runtime.Enemies.Slots[0]; var state = runtime.Enemies.Ridley!; var samus = runtime.Samus!;
        body.XPosition = 211; body.YPosition = 346;
        state.HorizontalVelocity = 0xc0; state.VerticalVelocity = 0x400;
        state.FacingDirection = 2; state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.GrabState = 0; state.FeetDistanceIndex = 0;
        samus.XPosition = 198; samus.YPosition = 402; samus.Pose = 0x54;
        samus.RefreshCollisionRadii(bus);
        var attack = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyGroundAttack", BindingFlags.Instance | BindingFlags.NonPublic)!;
        attack.Invoke(runtime.Enemies, [body, state, samus, runtime.LevelData]);
        AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "native grab immediately enters carry movement");
        AssertEqual((ushort)31, state.FunctionTimer, "native setup falls through first carry countdown");
        AssertEqual((ushort)0xb6, state.HorizontalVelocity, "native same-update grab X acceleration");
        AssertEqual((ushort)0xfbfc, state.VerticalVelocity, "native same-update grab Y acceleration");
        AssertTrue(samus.StationaryScriptControlLocked, "native grab installs command-zero alpha/beta pair");
        var bombs = CreateBombFixture();
        typeof(SamusBombProjectileSystem).GetMethod("SetSharedCooldown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(bombs, [(ushort)10]);
        bombs.StepFrame(bus, runtime.LevelData!, samus, 0, 0, advancePowerBombHdma: false);
        AssertEqual((ushort)10, bombs.CooldownTimer, "native locked alpha preserves cooldown");
        var release = typeof(RoomEnemySystem).GetMethod("ReleaseNorfairRidleyGrab", BindingFlags.Instance | BindingFlags.NonPublic)!;
        release.Invoke(runtime.Enemies, [state, samus]);
        AssertTrue(!samus.InputLocked && !samus.StationaryScriptControlLocked, "native release restores ordinary control");
        bombs.StepFrame(bus, runtime.LevelData!, samus, 0, 0, advancePowerBombHdma: false);
        AssertEqual((ushort)9, bombs.CooldownTimer, "native unlocked alpha resumes cooldown");
        // Native source8395: a descending lunge grabs standing Samus. Ground attacks
        // have their own reversal; this entry must negate after its lunge acceleration.
        body.XPosition = 172; body.XSubposition = 0x6300;
        body.YPosition = 345; body.YSubposition = 0x0a00;
        state.HorizontalVelocity = 0x2b0; state.VerticalVelocity = 0x3cd;
        state.FacingDirection = 2; state.HealthStage = 3; state.HitRoomBoundary = false;
        state.GrabState = 0; state.FeetDistanceIndex = 0;
        samus.XPosition = 156; samus.YPosition = 411; samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.RefreshCollisionRadii(bus);
        var lunge = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyGrabApproach", BindingFlags.Instance | BindingFlags.NonPublic)!;
        lunge.Invoke(runtime.Enemies, [body, state, samus]);
        AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "lunge grab enters immediate carry");
        AssertEqual((ushort)0x2b2, state.HorizontalVelocity, "native lunge grab X acceleration");
        AssertEqual((ushort)0xfc25, state.VerticalVelocity, "native lunge grab reverses before carry acceleration");
        AssertEqual((ushort)31, state.FunctionTimer, "native lunge grab falls through countdown");
        // Source10132 takes the zero-health branch, whose JMP executes C538 immediately.
        body.XPosition = 134; body.XSubposition = 0x7d00;
        body.YPosition = 356; body.YSubposition = 0xec00; body.Health = 0;
        state.HorizontalVelocity = 0x1de; state.VerticalVelocity = 0x419;
        state.FightMode = 1; state.GrabState = 0; state.FunctionTimer = 0x29;
        samus.XPosition = 134; samus.YPosition = 411; samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        lunge.Invoke(runtime.Enemies, [body, state, samus]);
        AssertEqual(RidleyAiFunction.NorfairReleaseSamus, state.Function, "zero-health grab enters death movement");
        AssertEqual((ushort)0x1c6, state.HorizontalVelocity, "death entry immediately accelerates X toward death spot");
        AssertEqual((ushort)0xfbde, state.VerticalVelocity, "death entry immediately accelerates Y toward death spot");
        AssertEqual((ushort)0x29, state.FunctionTimer, "death entry retains existing general countdown");
        var dispatch = typeof(RoomEnemySystem).GetMethod("RunNorfairRidleyFunction", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var function in new[] { RidleyAiFunction.NorfairCarryRise, RidleyAiFunction.NorfairCarryRelease })
        {
            state.Function = function; state.FunctionTimer = 0;
            state.HorizontalVelocity = 0xffca; state.VerticalVelocity = 0xfecb;
            state.TargetX = 208; body.XPosition = 212; body.YPosition = 142;
            dispatch.Invoke(runtime.Enemies, [body, state, samus, (ushort)0, runtime.LevelData]);
            AssertEqual((ushort)0xffca, state.HorizontalVelocity, "native expiry skips X acceleration");
            AssertEqual((ushort)0xfecb, state.VerticalVelocity, "native expiry skips Y acceleration");
            AssertEqual(function == RidleyAiFunction.NorfairCarryRise ? RidleyAiFunction.NorfairCarryRelease : RidleyAiFunction.NorfairSelectAttack, state.Function, "native expiry selects next carry phase");
            AssertEqual((ushort)(function == RidleyAiFunction.NorfairCarryRise ? 8 : 16), state.IdealInterSegmentTailAngle, "native expiry changes ideal tail angle");
            AssertEqual((ushort)240, state.TailExtensionSpeed, "native expiry restores tail extension speed");
        }
        Console.WriteLine("Ridley grab entry: native immediate carry, velocity, countdown and paired control lock/release pass.");
    }

    private static void VerifyRetainedHorizontalSpeed()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CreateRoom(16, 16, new ushort[256], new byte[256]);
        foreach (int branch in new[] { 0, 1, 2, 3 })
        {
            var samus = new SamusState
            {
                Pose = branch switch
                {
                    0 => SamusPoseIds.NormalJumpForwardRightPose,
                    2 => SamusPoseIds.SpringBallJumpRightPose,
                    _ => SamusPoseIds.FallingAimDownRightPose,
                },
                XPosition = 128, YPosition = 128,
            };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.YDirection = 2;
            samus.HorizontalSpeed.CalculateTotalSpeed(0x00054321);
            samus.HorizontalSpeed.BaseSpeed = branch == 3 ? (ushort)0 : (ushort)5;
            if (branch == 0) SamusAerialMovement.StepNormalJump(bus, room, samus, 0, 0);
            else if (branch == 1) SamusAerialMovement.StepFalling(bus, room, samus, 0, 0);
            else if (branch == 2) SamusMorphBallMovement.StepSpringBallInAir(bus, room, samus, 0, 0);
            else
            {
                samus.Grapple.ReleasedMovementActive = true;
                SamusAerialMovement.StepReleasedFromGrapple(bus, room, samus, 0, 0);
            }
            AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, $"branch {branch} clears base speed");
            AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSubspeed, $"branch {branch} clears base fraction");
            AssertEqual((ushort)128, samus.XPosition, $"branch {branch} does not move horizontally");
            AssertEqual((ushort)5, samus.HorizontalSpeed.TotalSpeed, $"branch {branch} retains total speed");
            AssertEqual((ushort)0x4321, samus.HorizontalSpeed.TotalSubspeed, $"branch {branch} retains total fraction");
        }
        Console.WriteLine("No-direction normal jump, fall, Spring Ball jump and grapple release retain total speed while clearing base motion.");
    }

    private static void VerifyPauseDispatcherRandom()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        // The recorded pause enters an already-running room HDMA callback.
        runtime.RoomLayer3Fx.AdvanceHdmaSharedState(runtime.System, false);
        var advance = typeof(SuperMetroidGame).GetMethod("AdvanceMenuRandom", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var state in new[] { SuperMetroidGameState.Pausing, SuperMetroidGameState.PausedA,
            SuperMetroidGameState.PausedB, SuperMetroidGameState.UnpausingA, SuperMetroidGameState.UnpausingB,
            SuperMetroidGameState.PausingDarkening, SuperMetroidGameState.Unpausing })
        {
            runtime.System.SetRandomNumber(0x117d);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, state);
            advance.Invoke(game, null);
            bool gameplayOwnsRandom = state is SuperMetroidGameState.PausingDarkening or SuperMetroidGameState.Unpausing;
            ushort expected = gameplayOwnsRandom ? (ushort)0x117d :
                state == SuperMetroidGameState.Pausing ? (ushort)0x7266 : (ushort)0x5882;
            AssertEqual(expected, runtime.System.RandomNumber,
                "pause entry runs lava HDMA before RNG; paused dispatches advance once; gameplay fades retain runtime ownership");
        }
        Console.WriteLine("Pause RNG: all five pause-only dispatchers advance once; gameplay fades avoid duplicate advances.");
    }

    private static void VerifySpinFallbackHistory()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (byte pose in new[] { SamusPoseIds.SpinJumpRightPose, SamusPoseIds.SpinJumpLeftPose,
            SamusPoseIds.SpaceJumpRightPose, SamusPoseIds.SpaceJumpLeftPose,
            SamusPoseIds.ScrewAttackRightPose, SamusPoseIds.ScrewAttackLeftPose })
        {
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Pose = pose; samus.XPosition = 512; samus.YPosition = 400;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.ScrewAttack);
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.PoseHistory.PreviousPose = pose;
            ushort direction = samus.ReadPoseXDirection(bus);
            ushort spinMovement = (ushort)(((ushort)SamusMovementType.SpinJumping << 8) | direction);
            samus.PoseHistory.PreviousDirectionAndMovement = spinMovement;
            samus.PoseHistory.LastDifferentPose = SamusPoseIds.RunningAimUpRightPose;
            samus.PoseHistory.LastDifferentDirectionAndMovement = (ushort)(((ushort)SamusMovementType.Running << 8) | direction);
            runtime.StepFrame(0);
            AssertEqual((ushort)pose, samus.PoseHistory.PreviousPose, "fallback retains spin pose");
            AssertEqual((ushort)pose, samus.PoseHistory.LastDifferentPose, "fallback commits same-pose history");
            AssertEqual(spinMovement, samus.PoseHistory.LastDifferentDirectionAndMovement, "fallback publishes older spin movement");
            AssertTrue(samus.PoseHistory.AllowsWallJumpProbe, "next update admits native wall observation");
        }
        Console.WriteLine("Spin fallback history: ordinary, Space Jump and Screw Attack both facings retain pose and admit wall probe.");
    }

    private static void VerifyMorphCameraCheckpoint()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(16, 32, new ushort[16 * 32], new byte[16 * 32]);
        foreach (bool left in new[] { false, true })
        {
            var samus = new SamusState
            {
                Pose = left ? SamusPoseIds.FallingAimDownLeftPose : SamusPoseIds.FallingAimDownRightPose,
                XPosition = 128, YPosition = 402,
                EquippedItems = (ushort)SamusEquipmentFlags.MorphBall,
            };
            samus.Kinematics.YSubposition = 0x97ff;
            samus.RefreshCollisionRadii(bus);
            var previous = new SamusCameraPoint(128, 0, 401, 0xb7ff);
            AssertTrue(samus.TryApplyMorphTransition(bus, level,
                left ? SamusPoseIds.MorphingTransitionLeftPose : SamusPoseIds.MorphingTransitionRightPose, 0),
                "source6289 airborne morph accepted");
            AssertEqual((ushort)411, samus.YPosition, "native morph center alignment");
            AssertEqual((ushort)0x97ff, samus.Kinematics.YSubposition, "morph preserves current fraction");
            AssertEqual(previous with { YPosition = 411 }, samus.ApplyPoseCollisionCameraCheckpoint(previous),
                "command seven replaces previous whole Y and retains previous fraction");
            AssertEqual(previous, samus.ApplyPoseCollisionCameraCheckpoint(previous), "checkpoint consumed once");

            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            samus.RefreshCollisionRadii(bus);
            AssertTrue(samus.TryApplyMorphTransition(bus, level,
                left ? SamusPoseIds.UnmorphingTransitionLeftPose : SamusPoseIds.UnmorphingTransitionRightPose, 0),
                "unmorph command seven accepted");
            AssertEqual(previous with { YPosition = samus.YPosition }, samus.ApplyPoseCollisionCameraCheckpoint(previous),
                "zero alignment entry still replaces previous whole Y");
        }
        Console.WriteLine("Morph camera checkpoint: both facings, alignment, fractions, one-time consumption and unmorph pass.");
    }

    private static void VerifyAimUpLandingAnimation()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = CreateRoom(16, 32, new ushort[16 * 32], new byte[16 * 32]);
        foreach (byte pose in new[] { SamusPoseIds.FallingAimUpRightPose, SamusPoseIds.FallingAimUpLeftPose })
        {
            var samus = new SamusState { Pose = pose, XPosition = 128, YPosition = 200 };
            samus.RefreshCollisionRadii(bus);
            AssertTrue(samus.TryApplyAerialLanding(bus, level, false, 0, 0), "aim-up landing fits");
            AssertEqual((ushort)1, samus.AnimationFrame, "native standing initializer skips raised-gun frame");
            AssertEqual((ushort)2, samus.AnimationFrameTimer, "native source3604 landing delay");
            samus.Pose = pose;
            samus.RefreshCollisionRadii(bus);
            samus.ApplyAerialLanding(bus, false);
            AssertEqual((ushort)1, samus.AnimationFrame, "roomless landing shares native frame skip");
            byte standing = pose == SamusPoseIds.FallingAimUpRightPose
                ? SamusPoseIds.StandingAimUpRightPose : SamusPoseIds.StandingAimUpLeftPose;
            typeof(SamusState).GetProperty("PendingTransitionalPose")!.SetValue(samus, standing);
            AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "landing completes through animation command");
            AssertEqual((ushort)1, samus.AnimationFrame, "native landing completion retains raised-gun frame");
            AssertEqual((ushort)16, samus.AnimationFrameTimer, "native source3606 standing delay");
        }
        Console.WriteLine("Aim-up landing: both facing directions preserve raised-gun animation frame and native delay.");
    }

    private static void VerifyRidleyFireballSquareSlope()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var projectile = runtime.Enemies.EnemyProjectiles[16];
        projectile.XPosition = 0x49; projectile.XSubposition = 0x8c00;
        projectile.YPosition = 0x54; projectile.YSubposition = 0xd900;
        projectile.XVelocity = 0xfb64; projectile.YVelocity = 0xfe1b;
        projectile.XRadius = 6; projectile.YRadius = 6;
        var move = typeof(RoomEnemySystem).GetMethod("MoveProjectileAxis", BindingFlags.Instance | BindingFlags.NonPublic)!;
        AssertTrue(!(bool)move.Invoke(runtime.Enemies, [projectile, runtime.LevelData, true])!, "source1248 fireball crosses empty square-slope quadrant horizontally");
        AssertTrue(!(bool)move.Invoke(runtime.Enemies, [projectile, runtime.LevelData, false])!, "source1248 fireball clears terrain vertically");
        AssertEqual((ushort)0x44, projectile.XPosition, "native source1249 fireball X");
        AssertEqual((ushort)0xf000, projectile.XSubposition, "native source1249 fireball X fraction");
        AssertEqual((ushort)0x52, projectile.YPosition, "native source1249 fireball Y");
        AssertEqual((ushort)0xf400, projectile.YSubposition, "native source1249 fireball Y fraction");
        projectile.XPosition = 60; projectile.XSubposition = 0;
        projectile.YPosition = 84; projectile.XVelocity = 0xff00;
        AssertTrue((bool)move.Invoke(runtime.Enemies, [projectile, runtime.LevelData, true])!, "occupied top-left quarter stops horizontal motion");
        AssertEqual((ushort)62, projectile.XPosition, "native square reaction snaps to eight-pixel edge even when initially embedded");
        projectile.XPosition = 52; projectile.YPosition = 94;
        projectile.YSubposition = 0; projectile.YVelocity = 0xff00;
        AssertTrue((bool)move.Invoke(runtime.Enemies, [projectile, runtime.LevelData, false])!, "occupied top-left quarter stops vertical motion");
        AssertEqual((ushort)94, projectile.YPosition, "native square reaction snaps below occupied quarter");
        Console.WriteLine("Ridley fireball square slope: native source1248 trajectory and occupied horizontal/vertical quadrants pass.");
    }

    private static void VerifyRidleyFireballDamage()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnRidleyFireball", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var afterburn = typeof(RoomEnemySystem).GetMethod("SpawnDirectionalAfterburn", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var collide = typeof(RoomEnemySystem).GetMethod("ResolveEnemyProjectileSamusCollision", BindingFlags.Static | BindingFlags.NonPublic)!;
        int slotIndex = 17;
        foreach (var sample in new[] { (AreaId.Ceres, (ushort)3), (AreaId.Norfair, (ushort)60), (AreaId.Tourian, (ushort)80) })
        {
            var samus = runtime.Samus!;
            samus.LiquidPhysics.RoomIdentity = new RoomIdentity(sample.Item1, 0);
            spawn.Invoke(runtime.Enemies, [runtime.Enemies.Slots[0], false]);
            var fireball = runtime.Enemies.EnemyProjectiles[slotIndex--];
            AssertEqual(sample.Item2, fireball.Damage, "native area initializer overrides fireball damage");
            afterburn.Invoke(runtime.Enemies, [fireball, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight, (ushort)0x0e00, (ushort)0]);
            AssertEqual(sample.Item2, runtime.Enemies.EnemyProjectiles[slotIndex--].Damage, "directional afterburn runs the same area initializer");
            samus.XPosition = fireball.XPosition; samus.YPosition = fireball.YPosition;
            samus.Health = 700; samus.EquippedItems = (ushort)SamusEquipmentFlags.GravitySuit;
            samus.RefreshCollisionRadii(bus);
            collide.Invoke(null, [fireball, samus]);
            AssertEqual((ushort)(700 - sample.Item2 / 4), samus.Health, "area damage passes through native Gravity reduction");
        }
        Console.WriteLine("Ridley fireball damage: default/Norfair/Tourian initializers, directional afterburn and suit reduction pass.");
    }

    private static void VerifyRidleyTailImpact()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        var state = runtime.Enemies.Ridley!;
        var body = runtime.Enemies.Slots[0];
        state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.TailSegments[6].XPosition = 4;
        state.TailSegments[6].YPosition = 118;
        var impact = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyGroundAttack", BindingFlags.Instance | BindingFlags.NonPublic)!;
        impact.Invoke(runtime.Enemies, [body, state, null, runtime.LevelData]);
        var dust = runtime.Enemies.EnemyProjectiles[17];
        AssertEqual(RoomEnemyProjectileKind.MiscDustExplosion, dust.Kind, "native tail impact allocates highest free slot");
        AssertEqual((ushort)4, dust.XPosition, "native tail impact dust X");
        AssertEqual((ushort)130, dust.YPosition, "native tail impact dust Y includes twelve-pixel offset");
        AssertEqual(MiscDustProjectileDefinitions.InstructionList(9), dust.InstructionPointer, "native impact selects variant nine");
        AssertTrue(runtime.Enemies.SoundRequests.Any(request => request.SoundEffect == SoundEffectLibrary2Sounds.RidleyTailTerrainImpact && request.MaximumQueued == 6), "native impact queues library-two sound 76");
        Console.WriteLine("Ridley tail impact: native dust slot, position, instruction variant and sound pass.");
    }

    private static void VerifyRidleyScreenGate()
    {
        var update = typeof(RoomEnemySystem).GetMethod("UpdateNorfairRidleyIntangibility", BindingFlags.Static | BindingFlags.NonPublic)!;
        var body = new RoomEnemySlot(0) { XPosition = 64, YPosition = 253 };
        var state = new RidleyEnemyState { FightMode = 1, IntangibilityTimer = 3 };
        // Original source 3062/3063: pre-movement Y253 is outside camera Y287.
        update.Invoke(null, [body, state, (ushort)0, (ushort)287]);
        AssertTrue(body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "native above-camera Ridley disables collision");
        AssertEqual((ushort)3, state.IntangibilityTimer, "offscreen return preserves release timer");
        // At the next AI call Y257 is inside. EnemyMain already tested the old
        // collision bit; the cleared gate is consumed by the following update.
        body.YPosition = 257;
        update.Invoke(null, [body, state, (ushort)0, (ushort)287]);
        AssertTrue(!body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "native reentry clears collision gate");
        AssertEqual((ushort)2, state.IntangibilityTimer, "onscreen AI resumes release timer");
        AssertTrue(RidleyCollisionDefinitions.IsOutsideInteractionWindow(64, 254, 0, 287), "one pixel above native window");
        AssertTrue(!RidleyCollisionDefinitions.IsOutsideInteractionWindow(64, 255, 0, 287), "native upper edge is admitted");
        AssertTrue(RidleyCollisionDefinitions.IsOutsideInteractionWindow(64, 543, 0, 287), "native lower edge is excluded");
        AssertTrue(!RidleyCollisionDefinitions.IsOutsideInteractionWindow(64, 542, 0, 287), "last lower pixel is admitted");
        Console.WriteLine("Ridley screen gate: recorded reentry, signed bounds and timer ownership pass.");
    }

    private static void VerifySpringBallRelease()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        runtime.InitializeDebugGroundedSamus(79, 425, 16);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseIds.SpringBallMovingLeftPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(5, 3);
        samus.XPosition = 79; samus.Kinematics.XSubposition = 0x8000;
        samus.YPosition = 425; samus.Kinematics.YSubposition = ushort.MaxValue;
        samus.HorizontalSpeed.BaseSpeed = 3; samus.HorizontalSpeed.BaseSubspeed = 0xc000;
        samus.HorizontalSpeed.AccelerationMode = 0;
        runtime.Controller1.Latch(0x0200);
        // Original movie source 1616 -> 1617: release Left while rolling.
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.XFixed, "spring release retains native final displacement");
        AssertEqual(SamusPoseIds.SpringBallGroundLeftPose, samus.Pose, "spring release selects stationary pose immediately");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "spring release clears base momentum after movement");
        AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode, "spring release clears acceleration mode");
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.XFixed, "released spring ball stays stopped next update");
        // Original source 1983: stationary Spring Ball keeps command six even
        // while the hurt mover calculates a fresh fractional base speed.
        samus.Pose = SamusPoseIds.SpringBallGroundRightPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.XPosition = 202; samus.Kinematics.XSubposition = 0x8000;
        samus.YPosition = 425; samus.Kinematics.YSubposition = ushort.MaxValue;
        samus.KnockbackActive = true; samus.KnockbackDirection = 2;
        samus.KnockbackXDirection = 1; samus.KnockbackTimer = 4;
        samus.InvincibilityTimer = 95;
        samus.Kinematics.YDirection = 1; samus.Kinematics.YSpeed = 5; samus.Kinematics.YSubspeed = 0;
        runtime.StepFrame(0);
        AssertEqual(0x00cb4000u, samus.Kinematics.XFixed, "stationary spring hurt frame preserves native displacement");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "stationary spring fallback clears hurt mover base speed");
        AssertEqual(SamusPoseIds.SpringBallGroundRightPose, samus.Pose, "stationary spring hurt fallback keeps pose");
        Console.WriteLine("Spring Ball release: native movie displacement, pose and immediate momentum reset pass.");
    }

    private static void VerifyRidleyTailOffsets()
    {
        var tick = typeof(RoomEnemySystem).GetMethod("TickRidleyTailSegment", BindingFlags.Static | BindingFlags.NonPublic)!;
        var state = new RidleyEnemyState
        {
            TailSegments = Enumerable.Range(0, 7).Select(_ => new RidleyTailSegment()).ToArray(),
            IdealInterSegmentTailAngle = 16, TailAngleDelta = 2,
            TailMinimumClockwiseAngle = 0x3fc0, TailMaximumCounterClockwiseAngle = 0x4010,
            TailWhipTargetClockwiseAngle = ushort.MaxValue,
            TailWhipTargetCounterClockwiseAngle = ushort.MaxValue,
        };
        var segment = state.TailSegments[0];
        segment.Active = true; segment.StaggerAngle = 2;
        segment.XOffset = 7; segment.YOffset = 11;
        tick.Invoke(null, [state, 0]);
        AssertEqual((ushort)4, segment.StaggerAngle, "native stagger advances before returning");
        AssertEqual((ushort)7, segment.XOffset, "stagger retains X offset");
        AssertEqual((ushort)11, segment.YOffset, "stagger retains Y offset");
        segment.StaggerAngle = ushort.MaxValue; segment.Angle = 0x4000;
        segment.MovementDirection = 0x8000; segment.Distance = 0x0200;
        tick.Invoke(null, [state, 0]);
        AssertEqual((ushort)0x3ffe, segment.Angle, "native clockwise comparison decrements then restores one before storing");
        AssertEqual(ushort.MaxValue, segment.XOffset, "signed Mode 7 multiplication floors negative fraction");
        AssertEqual((ushort)1, segment.YOffset, "signed Mode 7 cosine product");
        state.TailWhipTargetClockwiseAngle = 0x4000;
        segment.Angle = 0x4000; segment.XOffset = 7; segment.YOffset = 11;
        tick.Invoke(null, [state, 0]);
        AssertTrue(!segment.Active, "root stops at whip target");
        AssertEqual((ushort)7, segment.XOffset, "stop retains X offset");
        AssertEqual((ushort)11, segment.YOffset, "stop retains Y offset");
        var child = state.TailSegments[1];
        segment.Active = true;
        child.Active = true; child.StaggerAngle = ushort.MaxValue;
        child.Angle = 0x4000; child.MovementDirection = 0x8000; child.Distance = 0x0800;
        tick.Invoke(null, [state, 1]);
        AssertTrue(child.Active, "moving predecessor prevents child deactivation");
        AssertEqual((ushort)0x3fc0, child.Angle, "blocked child clamps to native clockwise limit");
        var flip = typeof(RoomEnemySystem).GetMethod("MirrorRidleyTail", BindingFlags.Static | BindingFlags.NonPublic)!;
        foreach (var part in state.TailSegments)
        {
            part.Distance = 0x1200; part.TargetDistance = 0x0c00;
            part.Angle = 0x3ff8; part.XOffset = 0xfffd; part.YOffset = 13;
        }
        flip.Invoke(null, [state]);
        ushort[] nativeRest = [0x0200, 0x0800, 0x0800, 0x0800, 0x0800, 0x0800, 0x0500];
        for (int i = 0; i < state.TailSegments.Length; i++)
        {
            var part = state.TailSegments[i];
            AssertEqual(nativeRest[i], part.Distance, "native flip restores each authored rest distance");
            AssertEqual((ushort)0x4008, part.Angle, "native flip reflects angle");
            AssertEqual((ushort)0x0c00, part.TargetDistance, "native flip preserves extension target");
            AssertEqual((ushort)0xfffd, part.XOffset, "native flip preserves current X offset");
            AssertEqual((ushort)13, part.YOffset, "native flip preserves current Y offset");
        }
        Console.WriteLine("Ridley tail offsets: stagger/stop retention, clockwise arithmetic, signed multiplication and predecessor gate pass.");
    }

    private static void VerifyRidleyCenterFacing()
    {
        var method = typeof(RoomEnemySystem).GetMethod("SelectNorfairRidleyFacingInstruction",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        (ushort X, ushort Facing, bool Turn)[] cases = [
            (0x004a, 2, false), // Original movie frame 734: already facing inward.
            (0x00c0, 2, true), (0x004a, 0, true), (0x00c0, 0, false),
            (0x004a, 1, false), (0x007f, 2, false), (0x0080, 2, true),
            (0x014a, 2, false), // Native tests the low position byte, not full X >= 128.
        ];
        foreach (var sample in cases)
        {
            var slot = new RoomEnemySlot(0) { XPosition = sample.X,
                CurrentInstruction = RidleyMovieMemory.RidleyRightFlyingSleep,
                InstructionTimer = 7, Timer = 9 };
            var state = new RidleyEnemyState { FacingDirection = sample.Facing };
            method.Invoke(null, [slot, state]);
            ushort expected = !sample.Turn ? RidleyMovieMemory.RidleyRightFlyingSleep
                : sample.Facing == 0 ? RidleyInstructionProgramDefinitions.TurnFromLeftToRight
                : RidleyInstructionProgramDefinitions.TurnFromRightToLeft;
            AssertEqual(expected, slot.CurrentInstruction, $"native center-facing instruction at {sample.X:X4}, facing {sample.Facing}");
            AssertEqual(sample.Turn ? (ushort)2 : (ushort)7, slot.InstructionTimer, "turn timer changes only when native condition is met");
            AssertEqual(sample.Turn ? (ushort)0 : (ushort)9, slot.Timer, "loop counter is preserved when no turn is needed");
        }
        var hover = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyHover", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var hoverState = new RidleyEnemyState { FunctionTimer = 0, Function = RidleyAiFunction.NorfairHover };
        hover.Invoke(new RoomEnemySystem(), [new RoomEnemySlot(0), hoverState]);
        AssertEqual(ushort.MaxValue, hoverState.FunctionTimer, "native hover decrements zero before selecting next attack");
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, hoverState.Function, "negative hover timer exits before movement");
        Console.WriteLine("Ridley center facing: movie trigger, both sides/directions, mid-turn and native low-byte boundary agree.");
    }

    private static void VerifyRidleyDoorEntry()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.SourceRoom);
        var level = runtime.LevelData!;
        bool found = false;
        for (int y = 0; y < level.HeightInBlocks && !found; y++)
        for (int x = 0; x < level.WidthInBlocks && !found; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
            if (door.DestinationRoomPointer != RidleyMovieMemory.RidleyRoom) continue;
            level.ResolveDoorCollision(bus, block.Behavior, 1, true);
            found = true;
        }
        AssertTrue(found, "native Ridley entry door");
        // Original movie: accepted main-loop samples 155, 156, 157. The source
        // acid callback was installed long before the recorded door collision.
        runtime.RoomLayer3Fx.AdvanceHdmaSharedState(runtime.System, false);
        runtime.System.SetRandomNumber(0xd562);
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter))!.SetValue(runtime, (ushort)0xa4df);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.XPosition = 20; samus.Kinematics.XSubposition = 0x2000;
        samus.YPosition = 111; samus.Kinematics.YSubposition = 0x5bff;
        uint xFixed = samus.Kinematics.XFixed, yFixed = samus.Kinematics.YFixed;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        game.Step(0);
        AssertEqual((ushort)0xef3a, runtime.System.RandomNumber, "native entry HDMA swap then RNG");
        AssertEqual((ushort)0xa4e0, runtime.NmiFrameCounter, "entry accepts exactly one NMI");
        AssertEqual(xFixed, samus.Kinematics.XFixed, "entry keeps Samus X fixed");
        AssertEqual(yFixed, samus.Kinematics.YFixed, "entry keeps Samus Y fixed");
        game.Step(0);
        AssertEqual((ushort)0x27bc, runtime.System.RandomNumber, "native sound-wait HDMA swap then RNG");
        AssertEqual((ushort)0xa4e1, runtime.NmiFrameCounter, "sound wait accepts exactly one NMI");
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification, "source fade begins after sound drain");
        var actor = runtime.Enemies.Slots[6];
        AssertEqual(PipeBugDefinitions.StrongBrinstarEnemyDefinition, actor.EnemyDefinitionPointer, "native source-room pipe bug");
        actor.CurrentInstruction = RidleyMovieMemory.PipeBugBeforeFadeInstruction;
        actor.InstructionTimer = 1;
        actor.SpritemapPointer = RidleyMovieMemory.PipeBugBeforeFadeSpritemap;
        game.Step(0);
        AssertEqual((ushort)0xadd4, runtime.System.RandomNumber, "first source fade advances native HDMA/RNG");
        AssertEqual((ushort)0xa4e2, runtime.NmiFrameCounter, "fade accepts exactly one NMI per update");
        AssertEqual(RidleyMovieMemory.PipeBugAfterFadeInstruction, actor.CurrentInstruction, "fade advances the native enemy instruction");
        AssertEqual(RidleyMovieMemory.PipeBugAfterFadeSpritemap, actor.SpritemapPointer, "fade changes to the native enemy sprite");
        AssertEqual((ushort)2, actor.InstructionTimer, "fade installs the native visual duration");
        int fadeSteps = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FadeOutSourcePalette && fadeSteps++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.LoadDoorHeader, game.DoorTransitionPhaseForVerification, "source palette fade finishes");
        AssertEqual((ushort)0xe19e, runtime.System.RandomNumber, "native update 172 fade endpoint RNG");
        // Original native input-boundary records 173..178. The first dispatch
        // still runs source HDMA; LoadDoorHeader disables it for the following ones.
        ushort[] nativeLoadingRandom = [0x1b76, 0x8a5f, 0xb4ec, 0x89ad, 0xb172, 0x784b];
        for (int index = 0; index < nativeLoadingRandom.Length; index++)
        {
            game.Step(0);
            AssertEqual(nativeLoadingRandom[index], runtime.System.RandomNumber, $"native loading RNG update {173 + index}");
            AssertEqual((ushort)(0xa4f1 + index), runtime.NmiFrameCounter, "loading accepts one NMI per update");
            if (index == 5)
            {
                AssertEqual(0x010e9000u, samus.Kinematics.XFixed, "native placement rebases and advances Samus while loading tiles");
                AssertEqual((ushort)0x00f8, runtime.Camera!.XPosition, "first loading IRQ moves camera four pixels");
            }
            if (index == 4)
            {
                AssertEqual(0x00135800u, samus.Kinematics.XFixed, "native scrolling setup moves Samus before destination placement");
                AssertEqual(yFixed, samus.Kinematics.YFixed, "left scrolling setup preserves perpendicular coordinate");
                AssertEqual((ushort)0x00fc, runtime.Camera!.XPosition, "native setup camera origin");
            }
        }
        game.Step(0); // The atomic destination loader must use the pre-setup source.
        AssertEqual(DoorTransitionPhase.WaitForDoorOpeningScroll, game.DoorTransitionPhaseForVerification, "loaded destination owns the opening trajectory");
        AssertEqual(0x010dc800u, samus.Kinematics.XFixed, "destination load carries both loading IRQ steps without restarting the scroll");
        int scrollCalls = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForDoorOpeningScroll && scrollCalls < 64)
        {
            game.Step(0);
            scrollCalls++;
        }
        AssertEqual(61, scrollCalls, "left trajectory completes on its 61st remaining IRQ call");
        AssertEqual(DoorTransitionPhase.FinishDoorLoading, game.DoorTransitionPhaseForVerification, "post-scroll NMI remains inside the loading coroutine");
        AssertEqual(0x00de2000u, samus.Kinematics.XFixed, "native source frame 276 scrolling endpoint");
        game.Step(0);
        AssertEqual(DoorTransitionPhase.HandleAnimatedTiles, game.DoorTransitionPhaseForVerification, "loading alignment precedes music wait");
        AssertEqual((ushort)0x5a88, runtime.System.RandomNumber, "loading continuation does not advance outer RNG");
        AssertEqual(0x00d82000u, samus.Kinematics.XFixed, "native final doorway alignment preserves original subposition");
        AssertEqual(yFixed, samus.Kinematics.YFixed, "left door endpoint preserves perpendicular coordinate");
        game.Step(0);
        AssertEqual((ushort)0xc5b9, runtime.System.RandomNumber, "native animated-tile outer dispatch RNG");
        game.Step(0);
        AssertEqual((ushort)0xa1ea, runtime.System.RandomNumber, "native music-wait outer dispatch RNG");
        int musicWaits = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForMusicQueue && musicWaits++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.HandleTransition, game.DoorTransitionPhaseForVerification, "native music queue reaches final door dispatch");
        ushort nmiBeforeNudge = runtime.NmiFrameCounter;
        ushort samusAnimation = samus.AnimationFrame;
        var ridley = runtime.Enemies.Slots[0];
        ushort bodyInstruction = ridley.CurrentInstruction;
        game.Step(0);
        AssertEqual(DoorTransitionPhase.BuildDestinationOam, game.DoorTransitionPhaseForVerification, "final nudge returns before first fade");
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 1)), runtime.NmiFrameCounter, "final transition accepts exactly one NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "final transition does not animate Samus");
        AssertEqual(bodyInstruction, ridley.CurrentInstruction, "final transition does not advance destination enemy instructions");
        game.Step(0);
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 2)), runtime.NmiFrameCounter, "first fade accepts exactly one additional NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "destination fade does not animate Samus");
        AssertEqual(RidleyMovieMemory.RidleyFirstFadeInstruction, ridley.CurrentInstruction, "first fade runs native Ridley instruction list");
        AssertEqual(RidleyMovieMemory.RidleyFirstFadeSpritemap, ridley.SpritemapPointer, "first fade publishes native Ridley sprite");
        AssertEqual((ushort)12, ridley.InstructionTimer, "first native fade visual duration");
        AssertEqual(RidleyAiFunction.WaitForDoorTransition, runtime.Enemies.Ridley!.Function, "native Ridley AI remains gated during fade visuals");
        AssertEqual((ushort)0, runtime.Enemies.Ridley.FunctionTimer, "fade does not consume the reveal countdown");
        game.Step(0);
        AssertEqual((ushort)11, ridley.InstructionTimer, "later fade updates continue enemy animation");
        int remainingFade = 0;
        while (game.GameState == SuperMetroidGameState.LoadingNextRoomB && remainingFade++ < 32)
        {
            game.Step(0);
            AssertEqual(RidleyAiFunction.WaitForDoorTransition, runtime.Enemies.Ridley.Function, "reveal timer remains gated through the final fade dispatch");
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "fade releases ordinary gameplay");
        AssertTrue(!runtime.Enemies.EnemyDoorTransitionActive, "completed fade clears enemy gate");
        game.Step(0);
        AssertEqual(RidleyAiFunction.InitialDelay, runtime.Enemies.Ridley.Function, "first gameplay update begins reveal");
        AssertEqual((ushort)169, runtime.Enemies.Ridley.FunctionTimer, "native first gameplay update consumes one of 170 reveal ticks");
        Console.WriteLine("Ridley door entry/loading/fade: native positions, RNG, one NMI per dispatch, stationary Samus animation and continuing enemy visuals agree.");
    }

    private static void VerifyRidleyFullMovie(string directory)
    {
        byte[] movie = File.ReadAllBytes("csharp/test-fixtures/issue-1266-ridley/Ridley fight showcase.smv");
        AssertTrue(Convert.ToHexString(SHA256.HashData(movie)) == "7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66", "original Ridley movie hash");
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "updates.json")));
        var root = manifest.RootElement;
        AssertEqual("super-metroid-gameplay-updates-v3", root.GetProperty("format").GetString()!, "converted replay format");
        AssertEqual(Convert.ToHexString(SHA256.HashData(movie)), root.GetProperty("movieSha256").GetString()!, "converted movie identity");
        AssertEqual(10890, root.GetProperty("sourceFrameCount").GetInt32(), "complete original movie coverage");
        var updates = root.GetProperty("updates").EnumerateArray().ToArray();
        int length = root.GetProperty("updateCount").GetInt32();
        AssertEqual(length, updates.Length, "converted update count");
        using var file = File.OpenRead(Path.Combine(directory, "update-boundaries.wram.gz"));
        AssertEqual(Convert.ToHexString(SHA256.HashData(file)), root.GetProperty("checkpointsSha256").GetString()!, "native checkpoint identity");
        file.Position = 0;
        using var trace = new GZipStream(file, CompressionMode.Decompress);
        var record = new byte[131080];
        int lastNativeRecord = -1;
        byte[] ReadFrame(int update)
        {
            int wantedRecord = update == 0 ? 0 : updates[update - 1].GetProperty("expectedRecord").GetInt32();
            AssertTrue(wantedRecord > lastNativeRecord, "native checkpoint order remains forward-only");
            while (lastNativeRecord < wantedRecord)
            {
                trace.ReadExactly(record);
                lastNativeRecord++;
            }
            int expectedFrame = update < length ? updates[update].GetProperty("sourceFrame").GetInt32() : 10890;
            AssertEqual(expectedFrame, BinaryPrimitives.ReadInt32LittleEndian(record), "native input-boundary frame");
            AssertEqual(update < length ? RidleyMovieMemory.ReadControllerInput : 0,
                BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(4)), "native accepted-input boundary or terminal");
            return record.AsSpan(8).ToArray();
        }
        byte[] memory = ReadFrame(0);
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.MoonwalkEnabled = W(RidleyMovieMemory.MoonwalkOption) != 0;
        runtime.System.LoadCollectedItemBytes(memory.AsSpan(RidleyMovieMemory.CollectedItemBits, Bank80SystemState.ItemBitByteCount));
        runtime.System.LoadBossBytes(memory.AsSpan(RidleyMovieMemory.BossBits, Bank80SystemState.AreaCount));
        runtime.System.LoadEventBytes(memory.AsSpan(RidleyMovieMemory.Events, Bank80SystemState.EventByteCount));
        runtime.System.LoadOpenedDoorBytes(memory.AsSpan(RidleyMovieMemory.OpenedDoors, Bank80SystemState.DoorBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(RidleyMovieMemory.Room), W(RidleyMovieMemory.CameraX), W(RidleyMovieMemory.CameraY));
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.XSubposition))!.SetValue(runtime.Camera, W(RidleyMovieMemory.CameraXFraction));
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.YSubposition))!.SetValue(runtime.Camera, W(RidleyMovieMemory.CameraYFraction));
        foreach (var (property, address) in new[]
        {
            (nameof(ScrollBoundaryCamera.IdealXPosition), RidleyMovieMemory.IdealCameraX),
            (nameof(ScrollBoundaryCamera.IdealYPosition), RidleyMovieMemory.IdealCameraY),
            (nameof(ScrollBoundaryCamera.CameraXSpeed), RidleyMovieMemory.CameraSpeedX),
            (nameof(ScrollBoundaryCamera.CameraXSubspeed), RidleyMovieMemory.CameraSpeedXFraction),
            (nameof(ScrollBoundaryCamera.CameraYSpeed), RidleyMovieMemory.CameraSpeedY),
            (nameof(ScrollBoundaryCamera.CameraYSubspeed), RidleyMovieMemory.CameraSpeedYFraction),
        }) typeof(ScrollBoundaryCamera).GetProperty(property)!.SetValue(runtime.Camera, W(address));
        runtime.Camera!.FinishSamusScrolling(new SamusCameraPoint(
            W(RidleyMovieMemory.PreviousSamusX), W(RidleyMovieMemory.PreviousSamusXFraction),
            W(RidleyMovieMemory.PreviousSamusY), W(RidleyMovieMemory.PreviousSamusYFraction)));
        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load room collision data.");
        for (int index = 0; index < level.WidthInBlocks * level.HeightInBlocks; index++)
        {
            level.SetForegroundEntry(index, W(RidleyMovieMemory.Level + index * sizeof(ushort)));
            level.SetBehavior(index, memory[RidleyMovieMemory.Bts + index]);
        }

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load Samus.");
        samus.InputLocked = false;
        for (int address = RidleyMovieMemory.ProjectileInheritancePrefix;
             address < RidleyMovieMemory.SamusSlopeAdjusted; address++)
            bus.WriteByte(address, memory[address]);
        samus.Kinematics.PositionAdjustedBySlope = W(RidleyMovieMemory.SamusSlopeAdjusted) != 0;
        for (int direction = 0; direction < 4; direction++)
            samus.Kinematics.RecordSolidEnemyCollision((SamusCollisionDirection)direction,
                W(RidleyMovieMemory.SamusSolidEnemyIndices + direction * 2));
        typeof(SamusHorizontalSpeedState).GetProperty(nameof(samus.HorizontalSpeed.ActiveSpeedTableBaseAddress))!
            .SetValue(samus.HorizontalSpeed, W(RidleyMovieMemory.HorizontalSpeedTable));
        samus.HorizontalSpeed.DecelerationMultiplier = memory[RidleyMovieMemory.HorizontalDecelerationMultiplier];
        samus.HorizontalSpeed.EchoSoundFlag = W(RidleyMovieMemory.SpeedEchoSoundLatch);
        typeof(SamusHorizontalSpeedState).GetProperty(nameof(samus.HorizontalSpeed.TotalSpeed))!
            .SetValue(samus.HorizontalSpeed, W(RidleyMovieMemory.TotalHorizontalSpeed));
        typeof(SamusHorizontalSpeedState).GetProperty(nameof(samus.HorizontalSpeed.TotalSubspeed))!
            .SetValue(samus.HorizontalSpeed, W(RidleyMovieMemory.TotalHorizontalSubspeed));
        runtime.Projectiles.GetType().GetProperty("ProjectileCounter")!.SetValue(runtime.Projectiles, W(RidleyMovieMemory.ProjectileCount));
        runtime.Projectiles.GetType().GetProperty("PreviousBeamChargeCounter")!.SetValue(runtime.Projectiles, W(RidleyMovieMemory.PreviousCharge));
        runtime.Projectiles.GetType().GetProperty("ProjectileInvincibilityTimer")!.SetValue(runtime.Projectiles, W(RidleyMovieMemory.ProjectileInteractionImmunity));
        runtime.Projectiles.GetType().GetProperty("ChargedShotGlowTimer")!.SetValue(runtime.Projectiles, W(RidleyMovieMemory.ChargedShotGlow));
        runtime.Projectiles.GetType().GetProperty("SamusChargePaletteIndex")!.SetValue(runtime.Projectiles, W(RidleyMovieMemory.ChargePaletteIndex));
        runtime.BombProjectiles.GetType().GetProperty("BombCounter")!.SetValue(runtime.BombProjectiles, W(RidleyMovieMemory.BombCount));
        samus.GetType().GetProperty("BombSpreadChargeTimeoutCounter")!.SetValue(samus, W(RidleyMovieMemory.BombSpreadChargeTimeout));
        samus.GetType().GetProperty("PoseTransitionShotDirection")!.SetValue(samus, W(RidleyMovieMemory.PoseShotDirection));
        samus.GetType().GetProperty("HyperBeam")!.SetValue(samus, W(RidleyMovieMemory.HyperBeam));
        samus.GetType().GetProperty("ResumeChargingBeamSoundFlag")!.SetValue(samus, W(RidleyMovieMemory.ResumeChargeSound));
        typeof(SamusState).GetProperty("PreviousDrawHeldInput")!.SetValue(samus, W(RidleyMovieMemory.SamusFilteredHeld));
        typeof(SamusState).GetProperty("PreviousDrawNewInput")!.SetValue(samus, W(RidleyMovieMemory.SamusFilteredNew));
        typeof(SamusState).GetProperty("AutoJumpTimer")!.SetValue(samus, W(RidleyMovieMemory.SamusAutoJumpTimer));
        typeof(SamusState).GetProperty("PreviousHealthForHurtCheck")!.SetValue(samus, W(RidleyMovieMemory.SamusPreviousHealthForFlash));
        AssertTrue(W(RidleyMovieMemory.SamusInputHandler) is
            RidleyMovieMemory.SamusNormalInputHandler or RidleyMovieMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        typeof(SamusState).GetProperty(nameof(samus.AutoJumpInputPending))!.SetValue(samus,
            W(RidleyMovieMemory.SamusInputHandler) == RidleyMovieMemory.SamusAutoJumpInputHandler);
        samus.EquippedItems = W(RidleyMovieMemory.Items);
        samus.EquippedBeams = W(RidleyMovieMemory.Beams);
        samus.Health = W(RidleyMovieMemory.Health);
        samus.MaxHealth = W(RidleyMovieMemory.MaxHealth);
        samus.InvincibilityTimer = W(RidleyMovieMemory.InvincibilityTimer);
        samus.KnockbackTimer = W(RidleyMovieMemory.KnockbackTimer);
        samus.KnockbackDirection = W(RidleyMovieMemory.KnockbackDirection);
        samus.KnockbackXDirection = W(RidleyMovieMemory.KnockbackXDirection);
        samus.HurtFlashCounter = W(RidleyMovieMemory.HurtFlashCounter);
        samus.SubunitHealth = W(RidleyMovieMemory.SubunitHealth);
        samus.SelectedHudItem = W(RidleyMovieMemory.SelectedHudItem);
        samus.AutoCancelHudItemIndex = W(RidleyMovieMemory.AutoCancelHudItemIndex);
        samus.ReserveTankMode = W(RidleyMovieMemory.ReserveMode);
        samus.MaxReserveEnergy = W(RidleyMovieMemory.MaxReserve);
        samus.ReserveEnergy = W(RidleyMovieMemory.Reserve);
        samus.Pose = (byte)W(RidleyMovieMemory.Pose);
        samus.XPosition = W(RidleyMovieMemory.X);
        samus.YPosition = W(RidleyMovieMemory.Y);
        samus.Kinematics.XSubposition = W(RidleyMovieMemory.XFraction);
        samus.Kinematics.YSubposition = W(RidleyMovieMemory.YFraction);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(W(RidleyMovieMemory.Animation), W(RidleyMovieMemory.AnimationTimer));
        samus.PoseHistory.PreviousPose = W(RidleyMovieMemory.PreviousPose);
        samus.PoseHistory.PreviousDirectionAndMovement = W(RidleyMovieMemory.PreviousDirection);
        samus.PoseHistory.LastDifferentPose = W(RidleyMovieMemory.LastDifferentPose);
        samus.PoseHistory.LastDifferentDirectionAndMovement = W(RidleyMovieMemory.LastDifferentDirection);
        samus.HorizontalSpeed.BaseSpeed = W(RidleyMovieMemory.BaseSpeed);
        samus.HorizontalSpeed.BaseSubspeed = W(RidleyMovieMemory.BaseFraction);
        samus.HorizontalSpeed.ExtraRunSpeed = W(RidleyMovieMemory.ExtraSpeed);
        samus.HorizontalSpeed.ExtraRunSubspeed = W(RidleyMovieMemory.ExtraFraction);
        samus.HorizontalSpeed.AccelerationMode = W(RidleyMovieMemory.AccelerationMode);
        samus.HorizontalSpeed.HasRunningMomentum = W(RidleyMovieMemory.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = W(RidleyMovieMemory.BoostCounter);
        samus.Kinematics.YSpeed = W(RidleyMovieMemory.VerticalSpeed);
        samus.Kinematics.YSubspeed = W(RidleyMovieMemory.VerticalFraction);
        samus.Kinematics.YDirection = W(RidleyMovieMemory.VerticalDirection);
        samus.Kinematics.XRadius = W(RidleyMovieMemory.SamusXRadius);
        samus.Kinematics.YRadius = W(RidleyMovieMemory.SamusYRadius);
        samus.Kinematics.YAcceleration = W(RidleyMovieMemory.Gravity);
        samus.Kinematics.YSubacceleration = W(RidleyMovieMemory.GravityFraction);
        samus.Kinematics.ExtraXDisplacement = W(RidleyMovieMemory.ExtraXDisplacement);
        samus.Kinematics.ExtraXSubdisplacement = W(RidleyMovieMemory.ExtraXDisplacementFraction);
        samus.Kinematics.ExtraYDisplacement = W(RidleyMovieMemory.ExtraYDisplacement);
        samus.Kinematics.ExtraYSubdisplacement = W(RidleyMovieMemory.ExtraYDisplacementFraction);
        samus.Kinematics.HorizontalSlopeCollisionEnable = W(RidleyMovieMemory.SlopeCollisionEnable);
        samus.HorizontalSpeed.SpeedDivisor = W(RidleyMovieMemory.SpeedDivisor);
        samus.HorizontalSpeed.ContactDamageIndex = W(RidleyMovieMemory.ContactDamageIndex);
        samus.MorphBallBounceState = W(RidleyMovieMemory.MorphBallBounceState);
        samus.BombJumpDirection = W(RidleyMovieMemory.BombJumpDirection);


        foreach (var trail in runtime.Projectiles.TrailSlots)
        {
            typeof(SamusProjectileTrailSide).GetProperty("InstructionTimer")!.SetValue(trail.Left, W(RidleyMovieMemory.TrailLeftInstructionTimer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionTimer")!.SetValue(trail.Right, W(RidleyMovieMemory.TrailRightInstructionTimer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionPointer")!.SetValue(trail.Left, W(RidleyMovieMemory.TrailLeftInstructionPointer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionPointer")!.SetValue(trail.Right, W(RidleyMovieMemory.TrailRightInstructionPointer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("TileNumberAttributes")!.SetValue(trail.Left, W(RidleyMovieMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("TileNumberAttributes")!.SetValue(trail.Right, W(RidleyMovieMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("XPosition")!.SetValue(trail.Left, W(RidleyMovieMemory.TrailLeftXPosition + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("XPosition")!.SetValue(trail.Right, W(RidleyMovieMemory.TrailRightXPosition + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("YPosition")!.SetValue(trail.Left, W(RidleyMovieMemory.TrailLeftYPosition + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("YPosition")!.SetValue(trail.Right, W(RidleyMovieMemory.TrailRightYPosition + trail.NativeByteIndex));
        }

        for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
        {
            ushort packed = W(RidleyMovieMemory.AtmosphericFrameAndType + index * 2);
            samus.LiquidPhysics.AtmosphericEffects.SetSlot(index, (byte)(packed >> 8), (byte)packed,
                W(RidleyMovieMemory.AtmosphericTimer + index * 2),
                W(RidleyMovieMemory.AtmosphericX + index * 2), W(RidleyMovieMemory.AtmosphericY + index * 2));
        }
        typeof(SamusState).GetProperty(nameof(samus.AnimationFrameBuffer))!.SetValue(samus, W(RidleyMovieMemory.AnimationFrameBuffer));
        typeof(SamusLiquidPhysicsState).GetProperty("LiquidPhysicsType")!.SetValue(samus.LiquidPhysics, W(RidleyMovieMemory.LiquidPhysicsType));
        typeof(SamusLiquidPhysicsState).GetProperty("PeriodicSubDamage")!.SetValue(samus.LiquidPhysics, W(RidleyMovieMemory.PeriodicSubDamage));
        typeof(SamusLiquidPhysicsState).GetProperty("PeriodicDamage")!.SetValue(samus.LiquidPhysics, W(RidleyMovieMemory.PeriodicDamage));

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialPlms = (Array)typeof(RoomPlmSystem).GetField("_slots", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Plms)!;
        for (int index = 0; index < initialPlms.Length; index++)
        {
            AssertTrue(W(RidleyMovieMemory.PlmHeaders + index * 2) == 0, "native initial PLM pool is empty");
            object slot = initialPlms.GetValue(index)!;
            slot.GetType().GetProperty("Active")!.SetValue(slot, false);
        }
        foreach (var projectile in runtime.Enemies.EnemyProjectiles)
        {
            int index = projectile.SlotIndex * 2;
            AssertEqual((ushort)0, W(RidleyMovieMemory.EnemyProjectileId + index), "initial native enemy projectile pool is inactive");
            foreach (var field in new[] {
                ("XPosition", RidleyMovieMemory.EnemyProjectileX),
                ("YPosition", RidleyMovieMemory.EnemyProjectileY),
                ("XVelocity", RidleyMovieMemory.EnemyProjectileXVelocity),
                ("YVelocity", RidleyMovieMemory.EnemyProjectileYVelocity) })
                typeof(RoomEnemyProjectileSlot).GetProperty(field.Item1)!.SetValue(projectile, W(field.Item2 + index));
        }
        var checkedProjectileCompositions = new HashSet<(ushort Operand, ushort Direct, ushort Native)>();
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(RidleyMovieMemory.Random));
        // Native frame zero already has the acid BG3 callback installed at $18F0.
        AssertTrue(W(RidleyMovieMemory.AcidHdmaPreInstruction) == RidleyMovieMemory.AcidHdmaCallback, "initial native acid HDMA callback");
        typeof(RoomLayer3FxState).GetField("lavaAcidBg3PreInstructionInstalled", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, true);
        typeof(RoomLayer3FxState).GetField("tidePhase", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, W(RidleyMovieMemory.TidePhase));
        typeof(RoomLayer3FxState).GetField("tideFixedOffset", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx,
            unchecked((int)((uint)W(RidleyMovieMemory.TideOffset) << 16 | W(RidleyMovieMemory.TideOffsetFraction))));
        typeof(RoomLayer3FxState).GetField("baseYSubposition", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, W(RidleyMovieMemory.LiquidBaseFraction));
        typeof(RoomLayer3FxState).GetProperty(nameof(RoomLayer3FxState.CurrentYPosition))!.SetValue(runtime.RoomLayer3Fx, W(RidleyMovieMemory.AcidSurface));
        runtime.RoomLayer3Fx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        string[] slotWords = ["EnemyDefinitionPointer", "XPosition", "XSubposition", "YPosition", "YSubposition", "XRadius", "YRadius", "Properties", "ExtraProperties", "AiHandlerBits", "Health", "SpritemapPointer", "Timer", "CurrentInstruction", "InstructionTimer", "PaletteIndex", "VramTilesIndex", "Layer", "FlashTimer", "FrozenTimer", "InvincibilityTimer", "ShakeTimer", "FrameCounter"];
        for (int index = 0; index < runtime.Enemies.Slots.Count; index++)
        {
            var slot = runtime.Enemies.Slots[index];
            int address = RidleyMovieMemory.EnemyBase + index * 64;
            if (W(address) != 0)
                AssertTrue(slot.EnemyDefinitionPointer == W(address), "initial enemy species agrees with room population");
            for (int word = 0; word < slotWords.Length; word++)
                typeof(RoomEnemySlot).GetProperty(slotWords[word])!.SetValue(slot, W(address + word * 2));
            for (int word = 0; word < 6; word++)
                typeof(RoomEnemySlot).GetProperty("Variable" + (char)('A' + word))!.SetValue(slot, W(address + 48 + word * 2));
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnX = slot.VariableB; pipe.SpawnY = slot.VariableC;
                pipe.DelayOrCounter = slot.VariableD;
                pipe.AnimationState = (PipeBugAnimationSelector)slot.VariableE;
                pipe.EmergenceTopY = W(RidleyMovieMemory.EnemyExtra + index * 64);
                pipe.InstalledAnimationState = (PipeBugAnimationSelector)W(RidleyMovieMemory.EnemyExtraPreviousAnimation + index * 64);
            }
        }
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter))!.SetValue(runtime, W(RidleyMovieMemory.NmiCounter));
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter8))!.SetValue(runtime, memory[RidleyMovieMemory.NmiCounterByte]);
        samus.CollectedItems = W(RidleyMovieMemory.CollectedItems); samus.CollectedBeams = W(RidleyMovieMemory.CollectedBeams);
        samus.Missiles = W(RidleyMovieMemory.Missiles); samus.MaxMissiles = W(RidleyMovieMemory.MaxMissiles);
        samus.SuperMissiles = W(RidleyMovieMemory.SuperMissiles); samus.MaxSuperMissiles = W(RidleyMovieMemory.MaxSuperMissiles);
        samus.PowerBombs = W(RidleyMovieMemory.PowerBombs); samus.MaxPowerBombs = W(RidleyMovieMemory.MaxPowerBombs);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch(W(RidleyMovieMemory.HeldInput));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, (SuperMetroidGameState)W(RidleyMovieMemory.GameState));
        var audio = new CartridgeAudioRenderer(runtimeFixtureInstallation.Value.LoadAudio());
        ushort[]? pendingLoadedOwners = null;
        int loadingIntervals = 0;
        ushort[] CaptureLoadedOwners()
        {
            var words = new List<ushort> { runtime.System.RandomNumber };
            foreach (var actor in runtime.Enemies.Slots)
                words.AddRange([actor.EnemyDefinitionPointer, actor.XPosition, actor.XSubposition,
                    actor.YPosition, actor.YSubposition, actor.Health, actor.SpritemapPointer,
                    actor.CurrentInstruction, actor.InstructionTimer]);
            return words.ToArray();
        }
        for (int frame = 0; frame <= length; frame++)
        {
            if (frame != 0) memory = ReadFrame(frame);
            var mismatches = new List<string>();
            void Check(string name, ushort actual, int address)
            {
                ushort expected = W(address);
                if (actual != expected) mismatches.Add($"{name}: native={expected:X4} port={actual:X4}");
            }
            void CheckBytes(string name, int count, Func<int, byte> read, int address)
            {
                for (int index = 0; index < count; index++)
                {
                    byte actual = read(index);
                    byte expected = memory[address + index];
                    if (actual != expected)
                        mismatches.Add($"{name}[{index}]: native={expected:X2} port={actual:X2}");
                }
            }
            CheckBytes("Boss bits", Bank80SystemState.AreaCount, runtime.System.GetBossBitsRaw, RidleyMovieMemory.BossBits);
            CheckBytes("Event bits", Bank80SystemState.EventByteCount, runtime.System.GetEventByteRaw, RidleyMovieMemory.Events);
            CheckBytes("Collected item bits", Bank80SystemState.ItemBitByteCount, runtime.System.GetCollectedItemByteRaw, RidleyMovieMemory.CollectedItemBits);
            CheckBytes("Opened door bits", Bank80SystemState.DoorBitByteCount, runtime.System.GetOpenedDoorByteRaw, RidleyMovieMemory.OpenedDoors);
            // Only the reference's proven hardware-upload NMI count is normalized;
            // gameplay state is never copied back into the production runtime.
            int excludedNmis = frame == 0 ? 0 : updates[frame - 1].GetProperty("excludedNmiAfter").GetInt32();
            ushort normalizedNmi = unchecked((ushort)(W(RidleyMovieMemory.NmiCounter) - excludedNmis));
            if (runtime.NmiFrameCounter != normalizedNmi)
                mismatches.Add($"Accepted gameplay NMI: native={normalizedNmi:X4} port={runtime.NmiFrameCounter:X4}");
            Check("Game state", (ushort)game.GameState, RidleyMovieMemory.GameState);
            Check("Enemy door gate", runtime.Enemies.EnemyDoorTransitionActive ? (ushort)1 : (ushort)0, RidleyMovieMemory.EnemyDoorTransition);
            // Native LoadDoorHeader publishes the destination room pointer before
            // loading its room/state data. The port keeps that identity in the pending
            // door while ActiveRoom still owns the source room's loaded data.
            ushort selectedRoom = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.AlignSourceCamera or DoorTransitionPhase.FixDoorsMovingUp or
                DoorTransitionPhase.SetupNewRoom or DoorTransitionPhase.SetupScrolling or
                DoorTransitionPhase.PlaceSamusAndLoadTiles or DoorTransitionPhase.LoadMoreThingsAndOpenDoor
                ? (runtime.PendingDoorTransition ?? throw new InvalidDataException("Missing selected destination door")).DestinationRoomPointer
                : runtime.ActiveRoom!.Pointer;
            Check("Selected room", selectedRoom, RidleyMovieMemory.Room);
            Check("Camera X", runtime.Camera!.XPosition, RidleyMovieMemory.CameraX);
            Check("Camera X fraction", runtime.Camera.XSubposition, RidleyMovieMemory.CameraXFraction);
            Check("Camera Y", runtime.Camera.YPosition, RidleyMovieMemory.CameraY);
            Check("Camera Y fraction", runtime.Camera.YSubposition, RidleyMovieMemory.CameraYFraction);
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                Check("Ideal camera X", runtime.Camera.IdealXPosition, RidleyMovieMemory.IdealCameraX);
                Check("Ideal camera Y", runtime.Camera.IdealYPosition, RidleyMovieMemory.IdealCameraY);
                Check("Camera speed X", runtime.Camera.CameraXSpeed, RidleyMovieMemory.CameraSpeedX);
                Check("Camera speed X fraction", runtime.Camera.CameraXSubspeed, RidleyMovieMemory.CameraSpeedXFraction);
                Check("Camera speed Y", runtime.Camera.CameraYSpeed, RidleyMovieMemory.CameraSpeedY);
                Check("Camera speed Y fraction", runtime.Camera.CameraYSubspeed, RidleyMovieMemory.CameraSpeedYFraction);
                // A fresh door camera defers its first sample to the runtime's frame-start
                // fallback. Compare that effective sample instead of requiring storage.
                var previous = runtime.Camera.PreviousSamusPoint ?? new SamusCameraPoint(
                    samus.XPosition, samus.Kinematics.XSubposition, samus.YPosition, samus.Kinematics.YSubposition);
                Check("Previous Samus X", previous.XPosition, RidleyMovieMemory.PreviousSamusX);
                Check("Previous Samus X fraction", previous.XSubposition, RidleyMovieMemory.PreviousSamusXFraction);
                Check("Previous Samus Y", previous.YPosition, RidleyMovieMemory.PreviousSamusY);
                Check("Previous Samus Y fraction", previous.YSubposition, RidleyMovieMemory.PreviousSamusYFraction);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
                {
                    var effect = samus.LiquidPhysics.AtmosphericEffects.Slots[index];
                    Check($"Atmosphere {index} frame/type", effect.FrameAndType, RidleyMovieMemory.AtmosphericFrameAndType + index * 2);
                    if (effect.Type == 0) continue;
                    Check($"Atmosphere {index} timer", effect.AnimationTimer, RidleyMovieMemory.AtmosphericTimer + index * 2);
                    Check($"Atmosphere {index} X", effect.XPosition, RidleyMovieMemory.AtmosphericX + index * 2);
                    Check($"Atmosphere {index} Y", effect.YPosition, RidleyMovieMemory.AtmosphericY + index * 2);
                }
                Check("Liquid animation buffer", samus.AnimationFrameBuffer, RidleyMovieMemory.AnimationFrameBuffer);
                Check("LiquidPhysicsType", samus.LiquidPhysics.LiquidPhysicsType, RidleyMovieMemory.LiquidPhysicsType);
                Check("PeriodicSubDamage", samus.LiquidPhysics.PeriodicSubDamage, RidleyMovieMemory.PeriodicSubDamage);
                Check("PeriodicDamage", samus.LiquidPhysics.PeriodicDamage, RidleyMovieMemory.PeriodicDamage);
                Check("Acid damage surface", samus.LiquidPhysics.LavaAcidYPosition, RidleyMovieMemory.AcidSurface);
                Check("Liquid tide phase", (ushort)typeof(RoomLayer3FxState).GetField("tidePhase", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.RoomLayer3Fx)!, RidleyMovieMemory.TidePhase);
            }
            Check("Samus X", samus.XPosition, RidleyMovieMemory.X);
            Check("Samus X fraction", samus.Kinematics.XSubposition, RidleyMovieMemory.XFraction);
            Check("Samus Y", samus.YPosition, RidleyMovieMemory.Y);
            Check("Samus Y fraction", samus.Kinematics.YSubposition, RidleyMovieMemory.YFraction);
            Check("Samus pose", samus.Pose, RidleyMovieMemory.Pose);
            Check("PreviousDrawHeldInput", samus.PreviousDrawHeldInput, RidleyMovieMemory.SamusFilteredHeld);
            Check("PreviousDrawNewInput", samus.PreviousDrawNewInput, RidleyMovieMemory.SamusFilteredNew);
            Check("AutoJumpTimer", samus.AutoJumpTimer, RidleyMovieMemory.SamusAutoJumpTimer);
            Check("PreviousHealthForHurtCheck", samus.PreviousHealthForHurtCheck, RidleyMovieMemory.SamusPreviousHealthForFlash);
            AssertTrue(!samus.ShinesparkPoseInputLocked && !samus.CrystalFlashPoseInputLocked,
                "Movie now uses a special pose-input lock; map its handler explicitly");
            Check("Samus input handler", samus.AutoJumpInputPending
                ? RidleyMovieMemory.SamusAutoJumpInputHandler : RidleyMovieMemory.SamusNormalInputHandler,
                RidleyMovieMemory.SamusInputHandler);

            Check("Samus previous pose", samus.PoseHistory.PreviousPose, RidleyMovieMemory.PreviousPose);
            Check("Samus previous movement", samus.PoseHistory.PreviousDirectionAndMovement, RidleyMovieMemory.PreviousDirection);
            Check("Samus last different pose", samus.PoseHistory.LastDifferentPose, RidleyMovieMemory.LastDifferentPose);
            Check("Samus last different movement", samus.PoseHistory.LastDifferentDirectionAndMovement, RidleyMovieMemory.LastDifferentDirection);
            Check("Samus animation", samus.AnimationFrame, RidleyMovieMemory.Animation);
            Check("Samus animation timer", samus.AnimationFrameTimer, RidleyMovieMemory.AnimationTimer);
            Check("Samus base speed", samus.HorizontalSpeed.BaseSpeed, RidleyMovieMemory.BaseSpeed);
            Check("Samus base fraction", samus.HorizontalSpeed.BaseSubspeed, RidleyMovieMemory.BaseFraction);
            Check("Samus extra speed", samus.HorizontalSpeed.ExtraRunSpeed, RidleyMovieMemory.ExtraSpeed);
            Check("Samus extra fraction", samus.HorizontalSpeed.ExtraRunSubspeed, RidleyMovieMemory.ExtraFraction);
            Check("Samus vertical speed", samus.Kinematics.YSpeed, RidleyMovieMemory.VerticalSpeed);
            Check("Samus vertical fraction", samus.Kinematics.YSubspeed, RidleyMovieMemory.VerticalFraction);
            Check("Samus vertical direction", samus.Kinematics.YDirection, RidleyMovieMemory.VerticalDirection);
            Check("Samus SamusXRadius", samus.Kinematics.XRadius, RidleyMovieMemory.SamusXRadius);
            Check("Samus SamusYRadius", samus.Kinematics.YRadius, RidleyMovieMemory.SamusYRadius);
            Check("Samus Gravity", samus.Kinematics.YAcceleration, RidleyMovieMemory.Gravity);
            Check("Samus GravityFraction", samus.Kinematics.YSubacceleration, RidleyMovieMemory.GravityFraction);
            Check("Samus ExtraXDisplacement", samus.Kinematics.ExtraXDisplacement, RidleyMovieMemory.ExtraXDisplacement);
            Check("Samus ExtraXDisplacementFraction", samus.Kinematics.ExtraXSubdisplacement, RidleyMovieMemory.ExtraXDisplacementFraction);
            Check("Samus ExtraYDisplacement", samus.Kinematics.ExtraYDisplacement, RidleyMovieMemory.ExtraYDisplacement);
            Check("Samus ExtraYDisplacementFraction", samus.Kinematics.ExtraYSubdisplacement, RidleyMovieMemory.ExtraYDisplacementFraction);
            Check("Samus SlopeCollisionEnable", samus.Kinematics.HorizontalSlopeCollisionEnable, RidleyMovieMemory.SlopeCollisionEnable);
            Check("Samus SpeedDivisor", samus.HorizontalSpeed.SpeedDivisor, RidleyMovieMemory.SpeedDivisor);
            Check("Samus ContactDamageIndex", samus.HorizontalSpeed.ContactDamageIndex, RidleyMovieMemory.ContactDamageIndex);
            Check("Samus MorphBallBounceState", samus.MorphBallBounceState, RidleyMovieMemory.MorphBallBounceState);
            Check("Samus BombJumpDirection", samus.BombJumpDirection, RidleyMovieMemory.BombJumpDirection);
            Check("Samus running momentum", samus.HorizontalSpeed.HasRunningMomentum ? (ushort)1 : (ushort)0, RidleyMovieMemory.Momentum);
            Check("Samus speed boost counter", samus.HorizontalSpeed.SpeedBoostCounter, RidleyMovieMemory.BoostCounter);
            Check("Samus horizontal speed table", samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, RidleyMovieMemory.HorizontalSpeedTable);
            AssertEqual(memory[RidleyMovieMemory.HorizontalDecelerationMultiplier], samus.HorizontalSpeed.DecelerationMultiplier,
                $"update {frame}: Samus horizontal deceleration multiplier");
            Check("Samus echo sound latch", samus.HorizontalSpeed.EchoSoundFlag, RidleyMovieMemory.SpeedEchoSoundLatch);
            Check("Samus slope adjustment", samus.Kinematics.PositionAdjustedBySlope ? (ushort)1 : (ushort)0,
                RidleyMovieMemory.SamusSlopeAdjusted);
            for (int direction = 0; direction < 4; direction++)
                Check($"Samus solid enemy {direction}", samus.Kinematics.SolidEnemyCollisionIndexes[direction],
                    RidleyMovieMemory.SamusSolidEnemyIndices + direction * 2);
            for (int address = RidleyMovieMemory.ProjectileInheritancePrefix;
                 address < RidleyMovieMemory.SamusSlopeAdjusted; address += 2)
                Check($"Projectile inherited movement ${address:X4}",
                    (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8), address);
            Check("Samus total horizontal speed", samus.HorizontalSpeed.TotalSpeed, RidleyMovieMemory.TotalHorizontalSpeed);
            Check("Samus total horizontal fraction", samus.HorizontalSpeed.TotalSubspeed, RidleyMovieMemory.TotalHorizontalSubspeed);
            Check("Samus movement handler", samus.KnockbackActive
                ? RidleyMovieMemory.KnockbackMovementHandler : RidleyMovieMemory.NormalMovementHandler,
                RidleyMovieMemory.SamusMovementHandler);
            // DoorTransitionState uses InputLocked to suppress host control, while
            // native door dispatch retains ordinary alpha/beta pointers unused.
            bool nativeControlLock = samus.InputLocked && game.GameState is not
                (SuperMetroidGameState.HitDoorBlock or SuperMetroidGameState.LoadingNextRoomA or
                 SuperMetroidGameState.LoadingNextRoomB);
            Check("Samus alpha handler", nativeControlLock
                ? RidleyMovieMemory.LockedAlphaHandler : RidleyMovieMemory.NormalAlphaHandler,
                RidleyMovieMemory.SamusAlphaHandler);
            Check("Samus beta handler", nativeControlLock
                ? RidleyMovieMemory.LockedBetaHandler : RidleyMovieMemory.NormalBetaHandler,
                RidleyMovieMemory.SamusBetaHandler);

            Check("Samus health", samus.Health, RidleyMovieMemory.Health);
            Check("Samus general Samus damage immunity countdown", samus.InvincibilityTimer, RidleyMovieMemory.InvincibilityTimer);
            Check("Samus Samus knockback countdown", samus.KnockbackTimer, RidleyMovieMemory.KnockbackTimer);
            Check("Samus Samus knockback direction", samus.KnockbackDirection, RidleyMovieMemory.KnockbackDirection);
            Check("Samus horizontal knockback direction", samus.KnockbackXDirection, RidleyMovieMemory.KnockbackXDirection);
            Check("Samus hurt palette/audio recovery countdown", samus.HurtFlashCounter, RidleyMovieMemory.HurtFlashCounter);
            Check("Samus fractional health word", samus.SubunitHealth, RidleyMovieMemory.SubunitHealth);
            Check("Samus selected HUD weapon", samus.SelectedHudItem, RidleyMovieMemory.SelectedHudItem);
            Check("Samus auto-cancel HUD selection", samus.AutoCancelHudItemIndex, RidleyMovieMemory.AutoCancelHudItemIndex);
            Check("Samus acceleration mode", samus.HorizontalSpeed.AccelerationMode, RidleyMovieMemory.AccelerationMode);
            Check("Samus maximum health", samus.MaxHealth, RidleyMovieMemory.MaxHealth);
            Check("Samus equipped items", samus.EquippedItems, RidleyMovieMemory.Items);
            Check("Samus collected items", samus.CollectedItems, RidleyMovieMemory.CollectedItems);
            Check("Samus equipped beams", samus.EquippedBeams, RidleyMovieMemory.Beams);
            Check("Samus collected beams", samus.CollectedBeams, RidleyMovieMemory.CollectedBeams);
            Check("Samus missiles", samus.Missiles, RidleyMovieMemory.Missiles);
            Check("Samus missile capacity", samus.MaxMissiles, RidleyMovieMemory.MaxMissiles);
            Check("Samus super missiles", samus.SuperMissiles, RidleyMovieMemory.SuperMissiles);
            Check("Samus super missile capacity", samus.MaxSuperMissiles, RidleyMovieMemory.MaxSuperMissiles);
            Check("Samus power bombs", samus.PowerBombs, RidleyMovieMemory.PowerBombs);
            Check("Samus power bomb capacity", samus.MaxPowerBombs, RidleyMovieMemory.MaxPowerBombs);
            Check("Samus reserve mode", samus.ReserveTankMode, RidleyMovieMemory.ReserveMode);
            Check("Samus reserve capacity", samus.MaxReserveEnergy, RidleyMovieMemory.MaxReserve);
            Check("Samus reserve energy", samus.ReserveEnergy, RidleyMovieMemory.Reserve);
            // The native CPU can still be decompressing source-room tiles while
            // IRQ scrolling advances; the port loads the destination atomically.
            // Align these owners at completed loading, not elapsed upload time.
            // Movement/input remain compared on EVERY IRQ interval. A stable owner
            // snapshot also proves the port does not run destination actors early.
            bool nativeLoading = game.GameState == SuperMetroidGameState.LoadingNextRoomB &&
                W(RidleyMovieMemory.DoorFunction) is RidleyMovieMemory.PlaceSamusLoadTiles or RidleyMovieMemory.LoadMoreThings;
            bool portWaitingForScroll = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading;
            bool deferLoadingOwners = nativeLoading && portWaitingForScroll;
            if (deferLoadingOwners)
            {
                ushort[] owners = CaptureLoadedOwners();
                if (pendingLoadedOwners is not null && !owners.AsSpan().SequenceEqual(pendingLoadedOwners))
                    throw new InvalidDataException("Destination RNG/enemy owners advanced while native hardware loading was still pending.");
                pendingLoadedOwners ??= owners;
                loadingIntervals++;
            }
            else
            {
                if (pendingLoadedOwners is not null)
                    AssertEqual(RidleyMovieMemory.HandleAnimTiles, W(RidleyMovieMemory.DoorFunction), "deferred destination owners reach native completed-loading boundary");
                Check("RNG", runtime.System.RandomNumber, RidleyMovieMemory.Random);
            }
            foreach (var actor in runtime.Enemies.Slots)
            {
                if (deferLoadingOwners) continue;
                int address = RidleyMovieMemory.EnemyBase + actor.NativeIndex;
                string owner = $"Enemy {actor.SlotIndex}";
                Check(owner + " identity", actor.EnemyDefinitionPointer, address);
                if (actor.EnemyDefinitionPointer == 0 || W(address) == 0) continue;
                Check(owner + " X", actor.XPosition, address + 2);
                Check(owner + " X fraction", actor.XSubposition, address + 4);
                Check(owner + " Y", actor.YPosition, address + 6);
                Check(owner + " Y fraction", actor.YSubposition, address + 8);
                Check(owner + " health", actor.Health, address + 20);
                Check(owner + " spritemap", actor.SpritemapPointer, address + 22);
                Check(owner + " instruction", actor.CurrentInstruction, address + 26);
                Check(owner + " instruction timer", actor.InstructionTimer, address + 28);
                Check(owner + " XRadius", actor.XRadius, address + 10);
                Check(owner + " YRadius", actor.YRadius, address + 12);
                Check(owner + " Properties", actor.Properties, address + 14);
                Check(owner + " ExtraProperties", actor.ExtraProperties, address + 16);
                Check(owner + " AiHandlerBits", actor.AiHandlerBits, address + 18);
                Check(owner + " Timer", actor.Timer, address + 24);
                Check(owner + " PaletteIndex", actor.PaletteIndex, address + 30);
                Check(owner + " VramTilesIndex", actor.VramTilesIndex, address + 32);
                Check(owner + " Layer", actor.Layer, address + 34);
                Check(owner + " FlashTimer", actor.FlashTimer, address + 36);
                Check(owner + " FrozenTimer", actor.FrozenTimer, address + 38);
                Check(owner + " InvincibilityTimer", actor.InvincibilityTimer, address + 40);
                Check(owner + " ShakeTimer", actor.ShakeTimer, address + 42);
                Check(owner + " FrameCounter", actor.FrameCounter, address + 44);
            }
            if (!deferLoadingOwners && runtime.Enemies.Ridley is { } ridleyState &&
                W(RidleyMovieMemory.EnemyBase) == RoomEnemySystem.NorfairRidleyDefinition)
            {
                bool expectedGate = (W(RidleyMovieMemory.EnemyBase + 14) & 0x0400) != 0;
                if (runtime.Enemies.Slots[0].Properties.HasAny(EnemyProperties.IgnoreSamusCollision) != expectedGate)
                    mismatches.Add($"Ridley interaction gate: native={expectedGate}");
                Check("Ridley AI function", (ushort)ridleyState.Function, RidleyMovieMemory.RidleyFunction);
                Check("Ridley AI timer", ridleyState.FunctionTimer, RidleyMovieMemory.RidleyFunctionTimer);
                Check("Ridley TailFunctionIndex", ridleyState.TailFunctionIndex, RidleyMovieMemory.RidleyTailFunctionIndex);
                Check("Ridley IdleTailWhipEnabled", ridleyState.IdleTailWhipEnabled, RidleyMovieMemory.RidleyIdleTailWhipEnabled);
                Check("Ridley TailWhipRequest", ridleyState.TailWhipRequest, RidleyMovieMemory.RidleyTailWhipRequest);
                Check("Ridley TailExtensionSpeed", ridleyState.TailExtensionSpeed, RidleyMovieMemory.RidleyTailExtensionSpeed);
                Check("Ridley TailAngleDelta", ridleyState.TailAngleDelta, RidleyMovieMemory.RidleyTailAngleDelta);
                Check("Ridley TailMinimumClockwiseAngle", ridleyState.TailMinimumClockwiseAngle, RidleyMovieMemory.RidleyTailMinimumClockwiseAngle);
                Check("Ridley TailMaximumCounterClockwiseAngle", ridleyState.TailMaximumCounterClockwiseAngle, RidleyMovieMemory.RidleyTailMaximumCounterClockwiseAngle);
                Check("Ridley TailWhipTargetClockwiseAngle", ridleyState.TailWhipTargetClockwiseAngle, RidleyMovieMemory.RidleyTailWhipTargetClockwiseAngle);
                Check("Ridley TailWhipTargetCounterClockwiseAngle", ridleyState.TailWhipTargetCounterClockwiseAngle, RidleyMovieMemory.RidleyTailWhipTargetCounterClockwiseAngle);
                Check("Ridley IdealInterSegmentTailAngle", ridleyState.IdealInterSegmentTailAngle, RidleyMovieMemory.RidleyIdealInterSegmentTailAngle);
                Check("Ridley HorizontalVelocity", ridleyState.HorizontalVelocity, RidleyMovieMemory.RidleyHorizontalVelocity);
                Check("Ridley VerticalVelocity", ridleyState.VerticalVelocity, RidleyMovieMemory.RidleyVerticalVelocity);
                Check("Ridley FightMode", ridleyState.FightMode, RidleyMovieMemory.RidleyFightMode);
                Check("Ridley MovementAnimationEnabled", ridleyState.MovementAnimationEnabled, RidleyMovieMemory.RidleyMovementAnimationEnabled);
                Check("Ridley WingFrame", ridleyState.WingFrame, RidleyMovieMemory.RidleyWingFrame);
                Check("Ridley WingAnimationTimerDelta", ridleyState.WingAnimationTimerDelta, RidleyMovieMemory.RidleyWingAnimationTimerDelta);
                Check("Ridley WingAnimationTimer", ridleyState.WingAnimationTimer, RidleyMovieMemory.RidleyWingAnimationTimer);
                Check("Ridley FacingDirection", ridleyState.FacingDirection, RidleyMovieMemory.RidleyFacingDirection);
                Check("Ridley HealthStage", ridleyState.HealthStage, RidleyMovieMemory.RidleyHealthStage);
                Check("Ridley GrabXOffset", ridleyState.GrabXOffset, RidleyMovieMemory.RidleyGrabXOffset);
                Check("Ridley GrabYOffset", ridleyState.GrabYOffset, RidleyMovieMemory.RidleyGrabYOffset);
                Check("Ridley TailDamage", ridleyState.TailDamage, RidleyMovieMemory.RidleyTailDamage);
                Check("Ridley FeetDistanceIndex", ridleyState.FeetDistanceIndex, RidleyMovieMemory.RidleyFeetDistanceIndex);
                Check("Ridley IntangibilityTimer", ridleyState.IntangibilityTimer, RidleyMovieMemory.RidleyIntangibilityTimer);
                if (ridleyState.Function is RidleyAiFunction.NorfairSwoopMoveToStart or
                    RidleyAiFunction.NorfairSwoopAimDown or RidleyAiFunction.NorfairSwoopAimSideways or
                    RidleyAiFunction.NorfairSwoopAimUp or RidleyAiFunction.NorfairSwoopClimb or RidleyAiFunction.NorfairSwoopRecover)
                    Check("Ridley swoop timer", ridleyState.SwoopPhaseTimer, RidleyMovieMemory.RidleySwoopTimer);
                if (game.GameState == SuperMetroidGameState.MainGameplay)
                {
                    // Tail workspace becomes live after its first fade-owned composition.
                    Check("Ridley tail tip X", ridleyState.TailSegments[6].XPosition, RidleyMovieMemory.TailTipX);
                    Check("Ridley tail tip Y", ridleyState.TailSegments[6].YPosition, RidleyMovieMemory.TailTipY);
                    for (int tailIndex = 0; tailIndex < ridleyState.TailSegments.Length; tailIndex++)
                    {
                        var segment = ridleyState.TailSegments[tailIndex];
                        int tailAddress = RidleyMovieMemory.TailSegments + tailIndex * RidleyMovieMemory.TailSegmentStride;
                        string tailOwner = $"Ridley tail {tailIndex}";
                        Check(tailOwner + " active", segment.Active ? RidleyMovieMemory.TailSegmentActive : (ushort)0, tailAddress);
                        Check(tailOwner + " StaggerAngle", segment.StaggerAngle, tailAddress + 2);
                        Check(tailOwner + " MovementDirection", segment.MovementDirection, tailAddress + 4);
                        Check(tailOwner + " Distance", segment.Distance, tailAddress + 6);
                        Check(tailOwner + " TargetDistance", segment.TargetDistance, tailAddress + 8);
                        Check(tailOwner + " Angle", segment.Angle, tailAddress + 10);
                        Check(tailOwner + " XPosition", segment.XPosition, tailAddress + 12);
                        Check(tailOwner + " YPosition", segment.YPosition, tailAddress + 14);
                        Check(tailOwner + " XOffset", segment.XOffset, tailAddress + 16);
                        Check(tailOwner + " YOffset", segment.YOffset, tailAddress + 18);
                    }
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                var plmSlots = runtime.Plms.PopulationSlots.ToDictionary(slot => slot.NativeSlotIndex);
                for (int slotIndex = 0; slotIndex < initialPlms.Length; slotIndex++)
                {
                    int offset = slotIndex * 2;
                    bool active = plmSlots.TryGetValue(slotIndex, out var slot);
                    Check($"PLM {slotIndex} header", active ? slot.HeaderPointer : (ushort)0, RidleyMovieMemory.PlmHeaders + offset);
                    if (!active || W(RidleyMovieMemory.PlmHeaders + offset) == 0) continue;
                    ushort instruction = slot.InstructionPointer;
                    ushort preInstruction = slot.PreInstruction == 0 ? RidleyMovieMemory.PlmDefaultPreInstruction : slot.PreInstruction;
                    var greyDoor = runtime.Plms.GreyDoors.FirstOrDefault(door => door.Header == slot.HeaderPointer && door.BlockIndex == slot.BlockIndex);
                    AssertTrue(greyDoor.Header != 0,
                        $"Movie PLM {slot.HeaderPointer:X4} needs a family-variable coverage mapping");
                    Check($"PLM {slotIndex} grey-door condition", (ushort)((int)greyDoor.Condition * 2), RidleyMovieMemory.PlmFamilyVariable + offset);
                    // Grey doors require one hit. Opening owns the incremented counter;
                    // before that transition setup's zero is the live counter value.
                    Check($"PLM {slotIndex} grey-door hit count",
                        greyDoor.Phase == GreyDoorPhase.Opening ? (ushort)1 : (ushort)0,
                        RidleyMovieMemory.PlmExtraVariable + offset);
                    if (greyDoor.Phase != GreyDoorPhase.Closing)
                    {
                        ushort NativeProgramWord(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                        ushort activation = NativeProgramWord(RidleyMovieMemory.PlmProgramBank | (greyDoor.InitialList + 6));
                        ushort link = NativeProgramWord(RidleyMovieMemory.PlmProgramBank | (activation + 2));
                        if (greyDoor.Phase == GreyDoorPhase.Locked)
                        {
                            instruction = unchecked((ushort)(greyDoor.InitialList + 14));
                            preInstruction = NativeProgramWord(RidleyMovieMemory.GreyDoorConditionTable + (int)greyDoor.Condition * 2);
                            link = activation;
                        }
                        else if (greyDoor.Phase == GreyDoorPhase.Flashing)
                            preInstruction = NativeProgramWord(RidleyMovieMemory.PlmProgramBank | (activation + 6));
                        Check($"PLM {slotIndex} semantic grey-door link", link, RidleyMovieMemory.PlmLinkInstruction + offset);
                    }
                    Check($"PLM {slotIndex} BlockIndex", unchecked((ushort)(slot.BlockIndex * 2)), RidleyMovieMemory.PlmBlockIndex + offset);
                    Check($"PLM {slotIndex} PreInstruction", preInstruction, RidleyMovieMemory.PlmPreInstruction + offset);
                    Check($"PLM {slotIndex} InstructionPointer", instruction, RidleyMovieMemory.PlmInstructionPointer + offset);
                    Check($"PLM {slotIndex} LoopTimer", slot.LoopTimer, RidleyMovieMemory.PlmLoopTimer + offset);
                    Check($"PLM {slotIndex} RoomArgument", slot.RoomArgument, RidleyMovieMemory.PlmRoomArgument + offset);
                    // Locked semantic doors omit Sleep's unconsumed countdown; the
                    // condition callback resets it to one before waking the native list.
                    if (greyDoor.Header == 0 || greyDoor.Phase != GreyDoorPhase.Locked)
                        Check($"PLM {slotIndex} InstructionTimer", slot.InstructionTimer, RidleyMovieMemory.PlmInstructionTimer + offset);
                    // Closing never consumes the retained link. Its fallthrough into
                    // InitialList overwrites it before installing the condition callback;
                    // all subsequent live links are checked through the semantic phase.
                }
                var activeLevel = runtime.LevelData ?? throw new InvalidDataException("Missing active collision data.");
                for (int block = 0; block < activeLevel.WidthInBlocks * activeLevel.HeightInBlocks; block++)
                {
                    Check($"Room block {block}", activeLevel.ForegroundEntries.Span[block], RidleyMovieMemory.Level + block * 2);
                    byte expectedBehavior = memory[RidleyMovieMemory.Bts + block];
                    byte actualBehavior = activeLevel.BehaviorBytes.Span[block];
                    if (actualBehavior != expectedBehavior)
                        mismatches.Add($"Room BTS {block}: native={expectedBehavior:X2} port={actualBehavior:X2}");
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                foreach (var trail in runtime.Projectiles.TrailSlots)
                {
                    Check($"Trail {trail.SlotIndex} Left timer", trail.Left.InstructionTimer, RidleyMovieMemory.TrailLeftInstructionTimer + trail.NativeByteIndex);
                    if (trail.Left.InstructionTimer != 0)
                    {
                        Check($"Trail {trail.SlotIndex} Left InstructionPointer", trail.Left.InstructionPointer, RidleyMovieMemory.TrailLeftInstructionPointer + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left TileNumberAttributes", trail.Left.TileNumberAttributes, RidleyMovieMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left XPosition", trail.Left.XPosition, RidleyMovieMemory.TrailLeftXPosition + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left YPosition", trail.Left.YPosition, RidleyMovieMemory.TrailLeftYPosition + trail.NativeByteIndex);
                    }
                    Check($"Trail {trail.SlotIndex} Right timer", trail.Right.InstructionTimer, RidleyMovieMemory.TrailRightInstructionTimer + trail.NativeByteIndex);
                    if (trail.Right.InstructionTimer != 0)
                    {
                        Check($"Trail {trail.SlotIndex} Right InstructionPointer", trail.Right.InstructionPointer, RidleyMovieMemory.TrailRightInstructionPointer + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right TileNumberAttributes", trail.Right.TileNumberAttributes, RidleyMovieMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right XPosition", trail.Right.XPosition, RidleyMovieMemory.TrailRightXPosition + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right YPosition", trail.Right.YPosition, RidleyMovieMemory.TrailRightYPosition + trail.NativeByteIndex);
                    }
                }
                Check("Projectile cooldown", runtime.BombProjectiles.CooldownTimer, RidleyMovieMemory.ProjectileCooldown);
                Check("Beam charge", runtime.Projectiles.FlareCounter, RidleyMovieMemory.BeamCharge);
                Check("ProjectileCounter", runtime.Projectiles.ProjectileCounter, RidleyMovieMemory.ProjectileCount);
                Check("PreviousBeamChargeCounter", runtime.Projectiles.PreviousBeamChargeCounter, RidleyMovieMemory.PreviousCharge);
                Check("ProjectileInvincibilityTimer", runtime.Projectiles.ProjectileInvincibilityTimer, RidleyMovieMemory.ProjectileInteractionImmunity);
                Check("ChargedShotGlowTimer", runtime.Projectiles.ChargedShotGlowTimer, RidleyMovieMemory.ChargedShotGlow);
                Check("SamusChargePaletteIndex", runtime.Projectiles.SamusChargePaletteIndex, RidleyMovieMemory.ChargePaletteIndex);
                Check("BombCounter", runtime.BombProjectiles.BombCounter, RidleyMovieMemory.BombCount);
                // This movie never places a bomb. Check every physical bomb slot and
                // both activation owners so that absence is verified, not assumed from
                // the aggregate counter or from ordinary beam/missile comparisons.
                foreach (var bomb in runtime.BombProjectiles.Slots)
                {
                    int address = RidleyMovieMemory.ProjectileType + (bomb.Index + 5) * 2;
                    Check($"Bomb {bomb.Index} type", bomb.Type, address);
                    AssertTrue(W(address) == 0,
                        "Movie now activates a bomb slot; add its full live-state mapping");
                }
                var explosion = runtime.BombProjectiles.PowerBombExplosion;
                Check("Power-bomb armed flag", explosion.Flag, RidleyMovieMemory.PowerBombArmedFlag);
                Check("Power-bomb explosion status", explosion.Status, RidleyMovieMemory.PowerBombExplosionStatus);
                AssertTrue(W(RidleyMovieMemory.PowerBombArmedFlag) == 0 &&
                    W(RidleyMovieMemory.PowerBombExplosionStatus) == 0,
                    "Movie now activates a power bomb; add its full live-state mapping");
                Check("BombSpreadChargeTimeoutCounter", samus.BombSpreadChargeTimeoutCounter, RidleyMovieMemory.BombSpreadChargeTimeout);
                Check("PoseTransitionShotDirection", samus.PoseTransitionShotDirection, RidleyMovieMemory.PoseShotDirection);
                Check("HyperBeam", samus.HyperBeam, RidleyMovieMemory.HyperBeam);
                Check("ResumeChargingBeamSoundFlag", samus.ResumeChargingBeamSoundFlag, RidleyMovieMemory.ResumeChargeSound);

            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Projectiles.Slots.Take(5))
            {
                int index = projectile.NativeByteIndex;
                string owner = $"Projectile {projectile.SlotIndex}";
                Check(owner + " type", projectile.Type, RidleyMovieMemory.ProjectileType + index);
                if (projectile.Type == 0 || W(RidleyMovieMemory.ProjectileType + index) == 0) continue;
                Check(owner + " X", projectile.XPosition, RidleyMovieMemory.ProjectileX + index);
                Check(owner + " Y", projectile.YPosition, RidleyMovieMemory.ProjectileY + index);
                Check(owner + " X radius", projectile.XRadius, RidleyMovieMemory.ProjectileXRadius + index);
                Check(owner + " Y radius", projectile.YRadius, RidleyMovieMemory.ProjectileYRadius + index);
                Check(owner + " damage", projectile.Damage, RidleyMovieMemory.ProjectileDamage + index);
                Check(owner + " XFraction", unchecked((ushort)projectile.XSubposition), RidleyMovieMemory.ProjectileXFraction + index);
                Check(owner + " YFraction", unchecked((ushort)projectile.YSubposition), RidleyMovieMemory.ProjectileYFraction + index);
                Check(owner + " XVelocity", unchecked((ushort)projectile.XVelocity), RidleyMovieMemory.ProjectileXVelocity + index);
                Check(owner + " YVelocity", unchecked((ushort)projectile.YVelocity), RidleyMovieMemory.ProjectileYVelocity + index);
                Check(owner + " Direction", unchecked((ushort)projectile.Direction), RidleyMovieMemory.ProjectileDirection + index);
                Check(owner + " Instruction", unchecked((ushort)projectile.InstructionPointer), RidleyMovieMemory.ProjectileInstruction + index);
                ushort callback = projectile.PreInstruction switch
                {
                    SamusProjectilePreInstruction.None => RidleyMovieMemory.ProjectileEmptyCallback,
                    SamusProjectilePreInstruction.NoWaveBeam => SamusBeamPreInstructionCodes.NoWave,
                    SamusProjectilePreInstruction.WaveBeamThreeFrameTrail => SamusBeamPreInstructionCodes.WaveThreeFrameTrail,
                    SamusProjectilePreInstruction.WaveBeamFourFrameTrail => SamusBeamPreInstructionCodes.WaveFourFrameTrail,
                    SamusProjectilePreInstruction.Missile => RidleyMovieMemory.ProjectileMissileCallback,
                    SamusProjectilePreInstruction.SuperMissile => RidleyMovieMemory.ProjectileSuperMissileCallback,
                    SamusProjectilePreInstruction.SuperMissileLink => RidleyMovieMemory.ProjectileSuperMissileLinkCallback,
                    _ => throw new InvalidDataException($"Movie projectile callback {projectile.PreInstruction} needs a native identity mapping"),
                };
                Check(owner + " PreInstruction", callback, RidleyMovieMemory.ProjectilePreInstruction + index);
                Check(owner + " InstructionTimer", unchecked((ushort)projectile.InstructionTimer), RidleyMovieMemory.ProjectileInstructionTimer + index);
                Check(owner + " Variable", unchecked((ushort)projectile.Variable), RidleyMovieMemory.ProjectileVariable + index);
                Check(owner + " TrailTimer", unchecked((ushort)projectile.TrailTimer), RidleyMovieMemory.ProjectileTrailTimer + index);
                Check(owner + " AuxiliaryPhase", unchecked((ushort)projectile.AuxiliaryPhase), RidleyMovieMemory.ProjectileAuxiliaryPhase + index);
                Check(owner + " Spritemap", unchecked((ushort)projectile.SpritemapPointer), RidleyMovieMemory.ProjectileSpritemap + index);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Enemies.EnemyProjectiles)
            {
                int index = projectile.SlotIndex * 2;
                string owner = $"Enemy projectile {projectile.SlotIndex}";
                Check(owner + " identity", (ushort)projectile.Kind, RidleyMovieMemory.EnemyProjectileId + index);
                if (!projectile.IsActive || W(RidleyMovieMemory.EnemyProjectileId + index) == 0) continue;
                Check(owner + " X", projectile.XPosition, RidleyMovieMemory.EnemyProjectileX + index);
                Check(owner + " Y", projectile.YPosition, RidleyMovieMemory.EnemyProjectileY + index);
                Check(owner + " Graphics", projectile.GraphicsIndex, RidleyMovieMemory.EnemyProjectileGraphics + index);
                Check(owner + " Timer", projectile.GeneralTimer, RidleyMovieMemory.EnemyProjectileTimer + index);
                Check(owner + " PreInstruction", projectile.PreInstruction, RidleyMovieMemory.EnemyProjectilePreInstruction + index);
                Check(owner + " XFraction", projectile.XSubposition, RidleyMovieMemory.EnemyProjectileXFraction + index);
                Check(owner + " YFraction", projectile.YSubposition, RidleyMovieMemory.EnemyProjectileYFraction + index);
                Check(owner + " XVelocity", projectile.XVelocity, RidleyMovieMemory.EnemyProjectileXVelocity + index);
                Check(owner + " YVelocity", projectile.YVelocity, RidleyMovieMemory.EnemyProjectileYVelocity + index);
                Check(owner + " Instruction", projectile.InstructionPointer, RidleyMovieMemory.EnemyProjectileInstruction + index);
                ushort nativeMap = W(RidleyMovieMemory.EnemyProjectileSpritemap + index);
                if (checkedProjectileCompositions.Add((projectile.PresentationOperandAddress, projectile.SpritemapPointer, nativeMap)))
                {
                    var artwork = runtime.Enemies.TileArtwork!.ProjectileSpritemaps!;
                    var parts = projectile.PresentationOperandAddress != 0
                        ? artwork.GetProgramFrame(projectile.PresentationOperandAddress)
                        : artwork.Get(projectile.SpritemapPointer);
                    var expectedOam = new OamBuffer();
                    var actualOam = new OamBuffer();
                    expectedOam.BeginFrame();
                    actualOam.BeginFrame();
                    DrawImportedEnemyProjectileSpritemap(bus, expectedOam, nativeMap, 128, 96, 0, true);
                    actualOam.AddEnemySpritemap(parts.Span, 128, 96, 0, 0,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    if (actualOam.NextByteOffset != expectedOam.NextByteOffset ||
                        !actualOam.LowTable.SequenceEqual(expectedOam.LowTable) ||
                        !actualOam.HighTable.SequenceEqual(expectedOam.HighTable))
                        mismatches.Add(owner + $" composition differs from native ${nativeMap:X4}");
                }
                Check(owner + " InstructionTimer", projectile.InstructionTimer, RidleyMovieMemory.EnemyProjectileInstructionTimer + index);
                Check(owner + " radii", (ushort)(projectile.XRadius | projectile.YRadius << 8), RidleyMovieMemory.EnemyProjectileRadius + index);
                ushort properties = projectile.Damage;
                if (projectile.DrawPriority == EnemyProjectileDrawPriority.High) properties |= RidleyMovieMemory.EnemyProjectileHighDraw;
                if (!projectile.CanDamageSamus) properties |= RidleyMovieMemory.EnemyProjectileNoContact;
                if (projectile.PersistsOnSamusContact) properties |= RidleyMovieMemory.EnemyProjectilePersistent;
                if (projectile.BlocksSamusProjectiles) properties |= RidleyMovieMemory.EnemyProjectileShotCollision;
                Check(owner + " properties", properties, RidleyMovieMemory.EnemyProjectileProperties + index);
                ushort variableE = projectile.Variable0, variableF = projectile.Variable1;
                switch (projectile.Kind)
                {
                    case RoomEnemyProjectileKind.CeresRidleyFireball:
                        // $86:940E consumes F only as a zero/nonzero afterburn gate.
                        // The caller may leave a noncanonical nonzero parameter (e.g. $E).
                        if ((projectile.RemainingAfterburns != 0) !=
                            (W(RidleyMovieMemory.EnemyProjectileVariableF + index) == 0))
                            mismatches.Add(owner + " fireball afterburn gate differs");
                        break;
                    case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter:
                    case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter:
                    case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight:
                    case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft:
                    case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp:
                    case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown:
                        variableE = projectile.RemainingAfterburns;
                        variableF = projectile.NextAfterburnKind;
                        break;
                    case RoomEnemyProjectileKind.MiscDustExplosion:
                    case RoomEnemyProjectileKind.EnemyDeathExplosion:
                    case RoomEnemyProjectileKind.EnemyDeathPickup:
                        break;
                    default:
                        throw new InvalidDataException($"Movie projectile {projectile.Kind} needs an E/F mapping");
                }
                Check(owner + " variable E", variableE, RidleyMovieMemory.EnemyProjectileVariableE + index);
                if (projectile.Kind != RoomEnemyProjectileKind.CeresRidleyFireball)
                    Check(owner + " variable F", variableF, RidleyMovieMemory.EnemyProjectileVariableF + index);
                Check(owner + " variable G", projectile.CollidedProjectileType, RidleyMovieMemory.EnemyProjectileVariableG + index);
                Check(owner + " collision option", projectile.CollisionOption, RidleyMovieMemory.EnemyProjectileCollisionOption + index);
                if (projectile.Kind is RoomEnemyProjectileKind.EnemyDeathExplosion or RoomEnemyProjectileKind.EnemyDeathPickup)
                {
                    AssertTrue(projectile.ItemDropChancesPointerOverride == 0,
                        "Movie drop uses a direct chance-table override; map its native source before comparing");
                    // Death explosions consume their per-slot header on their later
                    // drop instruction. Direct F337 pickups consume it immediately via
                    // caller X ($86:EF3E/F118), not allocated-slot Y, and never read it
                    // again. Their retained per-slot word is not a live source identity.
                    if (projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion)
                        Check(owner + " source enemy header", projectile.EnemyHeaderPointer, RidleyMovieMemory.EnemyProjectileEnemyHeader + index);
                    Check(owner + " killed enemy index", projectile.KilledEnemyNativeIndex, RidleyMovieMemory.EnemyProjectileKilledEnemy + index);
                }
                ushort nativeDamage = (ushort)(W(RidleyMovieMemory.EnemyProjectileProperties + index) & 0x0fff);
                if (projectile.Damage != nativeDamage) mismatches.Add(owner + $" damage: native={nativeDamage} port={projectile.Damage}");
            }
            if (mismatches.Count != 0)
            {
                level = runtime.LevelData ?? throw new InvalidDataException("Missing active room collision data.");
                Console.Error.WriteLine($"Pose history: port={samus.PoseHistory.PreviousPose:X4}/{samus.PoseHistory.PreviousDirectionAndMovement:X4}/{samus.PoseHistory.LastDifferentPose:X4}/{samus.PoseHistory.LastDifferentDirectionAndMovement:X4}, native={W(RidleyMovieMemory.PreviousPose):X4}/{W(RidleyMovieMemory.PreviousDirection):X4}/{W(RidleyMovieMemory.LastDifferentPose):X4}/{W(RidleyMovieMemory.LastDifferentDirection):X4}");
                Console.Error.WriteLine($"Liquid diagnostic: Y={samus.YPosition:X4}, surface={samus.LiquidPhysics.LavaAcidYPosition:X4}, pose={samus.Pose:X2}, radius={samus.Kinematics.YRadius}");
                Console.Error.WriteLine($"Shot diagnostic: locked={samus.InputLocked}, HUD={samus.SelectedHudItem}, grappleDebug={runtime.DebugGrappleItemSelected}, charge={runtime.Projectiles.FlareCounter}, cooldown={runtime.BombProjectiles.CooldownTimer}, held={runtime.Controller1.Current:X4}, new={runtime.Controller1.NewlyPressed:X4}, spawn={runtime.Projectiles.LastFiredProjectileSnapshot}");
                Console.Error.WriteLine($"Room width={level.WidthInBlocks}, Samus radius={samus.Kinematics.XRadius}/{samus.Kinematics.YRadius}, speed={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4}+{samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4}");
                for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
                {
                    ushort expectedBlock = W(RidleyMovieMemory.Level + block * 2);
                    ushort actualBlock = level.ForegroundEntries.Span[block];
                    if (expectedBlock != actualBlock) Console.Error.WriteLine($"Block {block} ({block % level.WidthInBlocks},{block / level.WidthInBlocks}): native={expectedBlock:X4} port={actualBlock:X4}");
                }
                throw new InvalidDataException($"Full movie first divergence at update {frame} (SMV source frame {(frame == 0 ? 0 : updates[frame - 1].GetProperty("sourceFrame").GetInt32())}): " + string.Join("; ", mismatches));
            }
            if (!deferLoadingOwners && pendingLoadedOwners is not null)
            {
                Console.WriteLine($"Completed-loading RNG/enemy owners match after {loadingIntervals} IRQ intervals; movement/input checked throughout.");
                pendingLoadedOwners = null;
            }
            // Only the converted controller event enters production; reference memory is
            // read-only. An accepted input read during APU transfer is not another
            // gameplay update. Until its latch/counter effects are normalized, stop
            // explicitly instead of replaying hardware upload time as gameplay.
            if (frame < length)
            {
                if (updates[frame].GetProperty("timingClass").GetString() == "apu-upload-continuation")
                    throw new InvalidDataException($"SMV source frame {updates[frame].GetProperty("sourceFrame").GetInt32()} is an APU upload continuation; hardware-wait input normalization is not implemented.");
                var output = game.Step((ushort)updates[frame].GetProperty("input").GetInt32());
                audio.RenderFrame(output.AudioCommands);
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            }
        }
        AssertTrue(pendingLoadedOwners is null, "no deferred loading comparison remains at movie end");
        AssertTrue(trace.ReadByte() == -1, "trace ends after movie terminal frame");
        Console.WriteLine($"Full movie input replay: {length} updates across all 10890 source frames match the currently instrumented fields.");
    }
}
