using SuperMetroid.Core.Input;
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
        var body = runtime.Enemies.Slots[0]; var state = runtime.Enemies.Ridley!; var samus = runtime.Samus!;
        var tick = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyPogo", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var sample in new[] { (0x7f, false, 2, false), (0x80, false, 2, true),
            (0xff, true, 2, false), (0xff, false, 1, false), (0x180, false, 0, true) })
        {
            runtime.System.SetRandomNumber((ushort)sample.Item1);
            state.Roaring = sample.Item2; state.FacingDirection = (ushort)sample.Item3;
            state.FunctionTimer = 20; samus.Pose = SamusPoseIds.SpinJumpRightPose;
            body.CurrentInstruction = NativeSnapshotMemory.RidleyRightFlyingSleep;
            body.InstructionTimer = 9; body.Timer = 11;
            tick.Invoke(runtime.Enemies, [body, state, samus, true]);
            AssertEqual(sample.Item4 ? RidleyInstructionProgramDefinitions.Fireballing : NativeSnapshotMemory.RidleyRightFlyingSleep,
                body.CurrentInstruction, "native spin-response fireball threshold/roar/facing gate");
            AssertEqual(sample.Item4 ? (ushort)1 : (ushort)9, body.InstructionTimer, "native fireball restarts instruction timer only when admitted");
            AssertEqual(sample.Item4 ? (ushort)0 : (ushort)11, body.Timer, "native fireball clears loop counter only when admitted");
            AssertEqual((ushort)sample.Item1, runtime.System.RandomNumber, "native fireball admission does not generate random state");
        }
        Console.WriteLine("Ridley spin-response fireball: native RNG threshold, roar/turn gates and instruction reset pass.");
    }

    private static void VerifyRidleyContactOrdering()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
        runtime.Enemies.ResolveRidleySamusContact(samus);
        AssertEqual((ushort)359, samus.Health, "next pre-AI body overlap remains damaging");
        Console.WriteLine("Ridley body contact: pre-movement boundary and subsequent damaging overlap pass.");
    }

    private static void VerifyRidleySwoopTimer()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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

    private static void VerifyRidleyMapInitialization()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool defeated in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.Hud.EnableMinimapAfterDoorEntry();
            runtime.System.LoadExploredMapBytes(new byte[Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea]);
            if (defeated) runtime.System.SetBossBits(AreaId.Norfair, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
            var room = runtime.ActiveRoom!;
            AssertEqual(!defeated, runtime.Hud.MinimapDisabled, "only live Ridley disables the minimap");
            for (int row = 0; row < 2; row++)
                AssertEqual(!defeated, runtime.System.IsMapTileExplored(room.AreaIndex, room.MapX, room.MapY + row + 1),
                    "Ridley initializes both native arena map cells");
            if (!defeated)
            {
                for (int y = 0; y < 3; y++)
                for (int x = 0; x < 5; x++)
                    AssertEqual((ushort)MapTileWords.HudBlank, runtime.Hud.Tiles[26 + y * HudState.WidthInTiles + x],
                        "Ridley blanks all fifteen minimap tiles");
                runtime.Hud.EnableMinimapAfterDoorEntry();
                AssertTrue(!runtime.Hud.MinimapDisabled, "door entry restores minimap updates");
            }
        }
        Console.WriteLine("Ridley map: live initializer explores both arena cells and blanks all fifteen HUD cells; defeated gate and door reset pass.");
    }

    private static void VerifyRetainedHorizontalSpeed()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
            AssertEqual(previous with { YPosition = 411 }, samus.ApplyPreviousPositionWrites(previous),
                "command seven replaces previous whole Y and retains previous fraction");
            AssertEqual(previous, samus.ApplyPreviousPositionWrites(previous), "checkpoint consumed once");

            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            samus.RefreshCollisionRadii(bus);
            AssertTrue(samus.TryApplyMorphTransition(bus, level,
                left ? SamusPoseIds.UnmorphingTransitionLeftPose : SamusPoseIds.UnmorphingTransitionRightPose, 0),
                "unmorph command seven accepted");
            AssertEqual(previous with { YPosition = samus.YPosition }, samus.ApplyPreviousPositionWrites(previous),
                "zero alignment entry still replaces previous whole Y");
        }
        Console.WriteLine("Morph camera checkpoint: both facings, alignment, fractions, one-time consumption and unmorph pass.");
    }

    private static void VerifyAimUpLandingAnimation()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
        var projectile = runtime.Enemies.EnemyProjectiles[16];
        projectile.XPosition = 0x49; projectile.XSubposition = 0x8c00;
        projectile.YPosition = 0x54; projectile.YSubposition = 0xd900;
        projectile.XVelocity = 0xfb64; projectile.YVelocity = 0xfe1b;
        projectile.XRadius = 6; projectile.YRadius = 6;
        var move = typeof(RoomEnemySystem).GetMethod("MoveProjectileAxis", BindingFlags.Static | BindingFlags.NonPublic)!;
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.RidleyRoom);
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
                CurrentInstruction = NativeSnapshotMemory.RidleyRightFlyingSleep,
                InstructionTimer = 7, Timer = 9 };
            var state = new RidleyEnemyState { FacingDirection = sample.Facing };
            method.Invoke(null, [slot, state]);
            ushort expected = !sample.Turn ? NativeSnapshotMemory.RidleyRightFlyingSleep
                : sample.Facing == 0 ? RidleyInstructionProgramDefinitions.TurnFromLeftToRight
                : RidleyInstructionProgramDefinitions.TurnFromRightToLeft;
            AssertEqual(expected, slot.CurrentInstruction, $"native center-facing instruction at {sample.X:X4}, facing {sample.Facing}");
            AssertEqual(sample.Turn ? (ushort)2 : (ushort)7, slot.InstructionTimer, "turn timer changes only when native condition is met");
            AssertEqual(sample.Turn ? (ushort)0 : (ushort)9, slot.Timer, "loop counter is preserved when no turn is needed");
        }
        var hover = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyHover", BindingFlags.Static | BindingFlags.NonPublic)!;
        var hoverState = new RidleyEnemyState { FunctionTimer = 0, Function = RidleyAiFunction.NorfairHover };
        hover.Invoke(new RoomEnemySystem(), [new RoomEnemySlot(0), hoverState]);
        AssertEqual(ushort.MaxValue, hoverState.FunctionTimer, "native hover decrements zero before selecting next attack");
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, hoverState.Function, "negative hover timer exits before movement");
        Console.WriteLine("Ridley center facing: movie trigger, both sides/directions, mid-turn and native low-byte boundary agree.");
    }

    private static void VerifyRidleyPausePageTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { MaxReserveEnergy = 300, ReserveEnergy = 300, ReserveTankMode = 1 };
        var menu = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Norfair, 0, 0,
            mapPresentation: RetailPresentationFixture());
        T Get<T>(string field) => (T)typeof(PauseMenuState).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menu)!;
        for (int direction = 0; direction < 2; direction++)
        {
            int source = direction, destination = 1 - direction;
            menu.Step((ushort)(direction == 0 ? SnesButton.R : SnesButton.L), 0);
            AssertEqual(source, menu.ScreenMode, "page-switch request retains source page");
            for (int update = 1; update <= 15; update++)
            {
                menu.Step(0, 0);
                AssertEqual(source, menu.ScreenMode, "source page remains through fade-out");
                AssertEqual(15 - update, Get<int>("transitionBrightness"), "native zero-delay fade-out brightness");
            }
            AssertEqual(direction == 0 ? PauseMenuTransition.MapToEquipmentLoad : PauseMenuTransition.EquipmentToMapLoad,
                Get<PauseMenuTransition>("transition"), "black frame returns into separate load dispatcher");
            menu.Step(0, 0);
            AssertEqual(destination, menu.ScreenMode, "separate load dispatch installs destination");
            AssertEqual(0, Get<int>("transitionBrightness"), "load dispatch remains black");
            for (int update = 1; update <= 30; update++)
            {
                menu.Step(0, 0);
                AssertEqual(update / 2, Get<int>("transitionBrightness"), "native delay-one fade-in brightness");
                AssertEqual(update == 30, Get<PauseMenuTransition>("transition") == PauseMenuTransition.None,
                    "input returns only after the thirtieth fade-in update");
            }
        }
        Console.WriteLine("Both pause-page directions preserve 15 fade-out, one load and 30 fade-in updates.");
    }

    private static void VerifyRidleyPaletteSelection()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);
        samus.RefreshCollisionRadii(bus);
        samus.XPosition = 512; samus.YPosition = 400;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
        samus.SelectedHudItem = 1; samus.SuperMissiles = 5;
        samus.LoadSuitPalette(bus, runtime.Cgram);
        Bgr555[] expected = runtime.Cgram.Colors.Slice(192, 16).ToArray();
        for (int color = 192; color < 208; color++) runtime.Cgram.SetColor(color, Bgr555.FromWord(0x1234));
        runtime.StepFrame((ushort)SnesButton.Select);
        AssertEqual((ushort)2, samus.SelectedHudItem, "selection enters Super Missiles without charge");
        AssertTrue(runtime.Cgram.Colors.Slice(192, 16).SequenceEqual(expected),
            "HUD handler restores all suit colors even when charge was zero");
        Console.WriteLine("HUD selection restores the complete suit palette without an active charge.");
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        var stateProperty = typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!;
        stateProperty.SetValue(game, SuperMetroidGameState.Pausing);
        game.Step(0);
        var menu = typeof(SuperMetroidGame).GetField("pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        var menuPalette = (SnesCgram)typeof(PauseMenuState).GetField("cgram", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menu)!;
        Bgr555[] initial = menuPalette.Colors.ToArray();
        int fadeFrames = 0;
        while (game.GameState == SuperMetroidGameState.PausedA && fadeFrames++ < 32)
        {
            game.Step(0);
            AssertTrue(menuPalette.Colors.SequenceEqual(initial), "pause fade-in retains all palette colors");
        }
        AssertEqual(SuperMetroidGameState.PausedB, game.GameState, "pause fade-in completes");
        game.Step(0);
        AssertTrue(!menuPalette.Colors.SequenceEqual(initial), "stable pause starts palette animation");
        Bgr555[] animated = menuPalette.Colors.ToArray();
        stateProperty.SetValue(game, SuperMetroidGameState.UnpausingA);
        typeof(SuperMetroidGame).GetMethod("BeginPauseFade", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(game, [(byte)15]);
        fadeFrames = 0;
        while (game.GameState == SuperMetroidGameState.UnpausingA && fadeFrames++ < 32)
        {
            game.Step(0);
            AssertTrue(menuPalette.Colors.SequenceEqual(animated), "unpause fade-out retains the last palette phase");
        }
        AssertEqual(SuperMetroidGameState.UnpausingB, game.GameState, "unpause fade-out completes");
        Console.WriteLine("Pause palette advances only in stable pause, remaining latched through both outer fades.");
    }

    private static void VerifyRidleyDoorEntry()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NativeSnapshotMemory.SourceRoom);
        var level = runtime.LevelData!;
        bool found = false;
        for (int y = 0; y < level.HeightInBlocks && !found; y++)
        for (int x = 0; x < level.WidthInBlocks && !found; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
            if (door.Door?.DestinationRoomPointer != NativeSnapshotMemory.RidleyRoom) continue;
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
        actor.CurrentInstruction = NativeSnapshotMemory.PipeBugBeforeFadeInstruction;
        actor.InstructionTimer = 1;
        actor.SpritemapPointer = NativeSnapshotMemory.PipeBugBeforeFadeSpritemap;
        game.Step(0);
        AssertEqual((ushort)0xadd4, runtime.System.RandomNumber, "first source fade advances native HDMA/RNG");
        AssertEqual((ushort)0xa4e2, runtime.NmiFrameCounter, "fade accepts exactly one NMI per update");
        AssertEqual(NativeSnapshotMemory.PipeBugAfterFadeInstruction, actor.CurrentInstruction, "fade advances the native enemy instruction");
        AssertEqual(NativeSnapshotMemory.PipeBugAfterFadeSpritemap, actor.SpritemapPointer, "fade changes to the native enemy sprite");
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
        AssertEqual((byte)0, runtime.NmiFrameCounter8, "door end-drawing IRQ clears the adjacent byte counter");
        ushort palettePointer = (ushort)(bus.ReadByte(NativeSnapshotMemory.BeamPalettePointers + (samus.EquippedBeams & 0x0fff) * 2) |
            bus.ReadByte(NativeSnapshotMemory.BeamPalettePointers + (samus.EquippedBeams & 0x0fff) * 2 + 1) << 8);
        for (int color = 0; color < 16; color++)
        {
            int address = NativeSnapshotMemory.CannonDefinitionBank | (palettePointer + color * 2);
            ushort expected = (ushort)((bus.ReadByte(address) | bus.ReadByte(address + 1) << 8) & 0x7fff);
            AssertEqual(expected, runtime.Cgram.Colors[224 + color], "beam palette is live during door scrolling");
        }
        AssertEqual((ushort)0, runtime.Cgram.Colors[196], "visor remains faded before scroll completes");
        int scrollCalls = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForDoorOpeningScroll && scrollCalls < 64)
        {
            game.Step(0);
            AssertEqual((byte)0, runtime.NmiFrameCounter8, "every moving door IRQ renews the byte-counter reset");
            scrollCalls++;
        }
        AssertEqual(61, scrollCalls, "left trajectory completes on its 61st remaining IRQ call");
        AssertEqual(DoorTransitionPhase.FinishDoorLoading, game.DoorTransitionPhaseForVerification, "post-scroll NMI remains inside the loading coroutine");
        AssertEqual((ushort)0x3be0, runtime.Cgram.Colors[196], "final scrolling update publishes native visor green before PLMs");
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
        AssertEqual((byte)0, runtime.NmiFrameCounter8, "final door scanout resets the byte before IRQ ownership changes");
        game.Step(0);
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 2)), runtime.NmiFrameCounter, "first fade accepts exactly one additional NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "destination fade does not animate Samus");
        AssertEqual((byte)1, runtime.NmiFrameCounter8, "first destination drawing frame resumes the byte counter");
        AssertEqual(NativeSnapshotMemory.RidleyFirstFadeInstruction, ridley.CurrentInstruction, "first fade runs native Ridley instruction list");
        AssertEqual(NativeSnapshotMemory.RidleyFirstFadeSpritemap, ridley.SpritemapPointer, "first fade publishes native Ridley sprite");
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
        var ridleyMovie = ReplayMovie.Load("Ridley", "csharp/test-fixtures/issue-1266-ridley/Ridley fight showcase.smv",
            "7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66");
        byte[] movie = ridleyMovie.Bytes;
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "updates.json")));
        var root = manifest.RootElement;
        // v5-v7 only add message-box and boot-entering dispatch evidence; this snapshot movie
        // never reboots, which the integer expectedRecord read below confirms.
        AssertTrue(root.GetProperty("format").GetString() is "super-metroid-gameplay-updates-v7", "converted replay format");
        AssertEqual(0, root.GetProperty("initialRecord").GetInt32(), "snapshot movie has no folded boot prelude");
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
            AssertEqual(update < length ? NativeSnapshotMemory.ReadControllerInput : 0,
                BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(4)), "native accepted-input boundary or terminal");
            return record.AsSpan(8).ToArray();
        }
        byte[] memory = ReadFrame(0);
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var (bus, runtime, samus, game) = ImportNativeSnapshot(memory, ridleyMovie.InitialSaveRam);
        var previousHudSelection = typeof(HudState)
            .GetField("_previousSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var initialPlms = (Array)typeof(RoomPlmSystem).GetField("_slots", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Plms)!;
        var checkedProjectileCompositions = new HashSet<(ushort Operand, ushort Direct, ushort Native)>();
        var audio = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());
        ushort[]? pendingLoadedOwners = null;
        int loadingIntervals = 0;
        ushort[] CaptureLoadedOwners()
        {
            var words = new List<ushort> { runtime.System.RandomNumber };
            // The final scrolling IRQ publishes the visor before the deferred
            // loading owners reach their shared completion boundary. Check it there.
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color != DoorTransitionPaletteDefinitions.VisorColorIndex)
                    words.Add(runtime.Cgram.Colors[color].ToWord());
            foreach (var actor in runtime.Enemies.Slots)
                words.AddRange([actor.EnemyDefinitionPointer, actor.XPosition, actor.XSubposition,
                    actor.YPosition, actor.YSubposition, actor.Health, actor.SpritemapPointer,
                    actor.CurrentInstruction, actor.InstructionTimer]);
            return words.ToArray();
        }
        int pairedBodyDraws = 0, phaseShiftedBodyDraws = 0, retainedHiddenBodyRecords = 0;
        (ushort Top, ushort Bottom, ushort X, ushort Y) BodyRecord() =>
            (samus.TopSpritemapIndex, samus.BottomSpritemapIndex,
                samus.SpritemapXPosition, samus.SpritemapYPosition);
        var previousBodyRecord = BodyRecord();
        for (int frame = 0; frame <= length; frame++)
        {
            ushort previousKnockbackTimer = W(NativeSnapshotMemory.KnockbackTimer);
            ushort previousInvincibilityTimer = W(NativeSnapshotMemory.InvincibilityTimer);
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
            CheckBytes("Boss bits", Bank80SystemState.AreaCount, runtime.System.GetBossBitsRaw, NativeSnapshotMemory.BossBits);
            CheckBytes("Event bits", Bank80SystemState.EventByteCount, runtime.System.GetEventByteRaw, NativeSnapshotMemory.Events);
            CheckBytes("Collected item bits", Bank80SystemState.ItemBitByteCount, runtime.System.GetCollectedItemByteRaw, NativeSnapshotMemory.CollectedItemBits);
            CheckBytes("Opened door bits", Bank80SystemState.DoorBitByteCount, runtime.System.GetOpenedDoorByteRaw, NativeSnapshotMemory.OpenedDoors);
            if (game.GameState is not (SuperMetroidGameState.HitDoorBlock or
                SuperMetroidGameState.LoadingNextRoomA or SuperMetroidGameState.LoadingNextRoomB))
                for (int tile = 0; tile < HudState.MutableTileCount; tile++)
                    Check($"HUD tile {tile}", runtime.Hud.Tiles[tile], NativeSnapshotMemory.HudTilemap + tile * 2);
            Check("Previous HUD selection", (ushort)previousHudSelection.GetValue(runtime.Hud)!, NativeSnapshotMemory.PreviousHudSelection);
            CheckBytes("Save/elevator markers", Bank80SystemState.UsedSaveStationByteCount,
                runtime.System.GetUsedSaveStationByteRaw, NativeSnapshotMemory.SaveElevatorMarkers);
            CheckBytes("Map-station markers", Bank80SystemState.MapStationByteCount,
                runtime.System.GetMapStationByteRaw, NativeSnapshotMemory.MapStationMarkers);
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                // $82:8B44 draws before $A0:9169 ages hurt timers. A final one
                // therefore still affects this update's draw although the checkpoint
                // stores zero. This movie has no debug invincibility/timer-reset path.
                bool knockbackAtDraw = W(NativeSnapshotMemory.KnockbackTimer) != 0 ||
                    (frame != 0 && previousKnockbackTimer == 1);
                bool invincibleAtDraw = W(NativeSnapshotMemory.InvincibilityTimer) != 0 ||
                    (frame != 0 && previousInvincibilityTimer == 1);
                bool forcedBodyVisible = knockbackAtDraw || !invincibleAtDraw ||
                    W(NativeSnapshotMemory.SamusShineTimer) != 0;
                bool nativeBodyVisible = forcedBodyVisible || (W(NativeSnapshotMemory.NmiCounter) & 1) == 0;
                bool normalizedBodyVisible = forcedBodyVisible || (runtime.NmiFrameCounter & 1) == 0;
                if (frame != 0)
                {
                    AssertEqual(normalizedBodyVisible, runtime.LastSamusBodyDrawn,
                        $"update {frame}: body visibility uses lag-free NMI phase");
                    if (nativeBodyVisible && normalizedBodyVisible) pairedBodyDraws++;
                    if (nativeBodyVisible != normalizedBodyVisible) phaseShiftedBodyDraws++;
                    if (!normalizedBodyVisible)
                    {
                        AssertEqual(previousBodyRecord, BodyRecord(),
                            $"update {frame}: hidden body retains prior sprite records");
                        retainedHiddenBodyRecords++;
                    }
                }
                if (frame != 0)
                {
                    var nativeDraw = RidleyNativeBodyRecord(bus, memory);
                    if (nativeBodyVisible)
                        AssertEqual((W(NativeSnapshotMemory.SamusTopSpritemap), W(NativeSnapshotMemory.SamusBottomSpritemap),
                                W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY)),
                            nativeDraw, $"update {frame}: cartridge draw oracle matches native visible records");
                    if (normalizedBodyVisible)
                        AssertEqual(nativeDraw, BodyRecord(),
                            $"update {frame}: visible body matches cartridge draw oracle regardless of flicker phase");
                }
                // Flicker deliberately follows the retained NMI clock. Removed hardware
                // waits can change its parity, and hidden frames retain the last visible
                // origin/indices. Compare newly published records when both draws execute.
                if (frame == 0 || (nativeBodyVisible && normalizedBodyVisible))
                {
                    Check("Samus top spritemap", samus.TopSpritemapIndex, NativeSnapshotMemory.SamusTopSpritemap);
                    Check("Samus bottom spritemap", samus.BottomSpritemapIndex, NativeSnapshotMemory.SamusBottomSpritemap);
                    Check("Samus sprite X", samus.SpritemapXPosition, NativeSnapshotMemory.SamusSpriteX);
                    Check("Samus sprite Y", samus.SpritemapYPosition, NativeSnapshotMemory.SamusSpriteY);
                }
                if (frame != 0)
                {
                    var nativeCannon = RidleyNativeCannonDraw(bus, memory, invincibleAtDraw, W(NativeSnapshotMemory.NmiCounter));
                    if (nativeCannon.SpriteWritten)
                        AssertTrue(ContainsMovieCannonSprite(memory.AsSpan(NativeSnapshotMemory.OamLow, 512),
                            memory.AsSpan(NativeSnapshotMemory.OamHigh, 32), nativeCannon),
                            $"update {frame}: native OAM contains reference cannon sprite");
                    // The normalized draw is observed through its outputs: the port OAM must hold the
                    // reference sprite (position and attributes), and the VRAM queue must hold
                    // exactly the reference tile transfer.
                    var cannon = RidleyNativeCannonDraw(bus, memory, invincibleAtDraw, runtime.NmiFrameCounter);
                    if (cannon.SpriteWritten)
                        AssertTrue(ContainsMovieCannonSprite(runtime.Oam.LowTable, runtime.Oam.HighTable, cannon),
                            $"update {frame}: port OAM contains reference cannon sprite");
                    var transfers = runtime.VramWrites.Entries.Where(entry =>
                        entry.EncodedVramDestination == NativeSnapshotMemory.CannonTileDestination).ToArray();
                    AssertEqual(cannon.TileUploadQueued ? 1 : 0, transfers.Length,
                        $"update {frame}: cannon upload count");
                    if (cannon.TileUploadQueued)
                    {
                        AssertEqual((ushort)32, transfers[0].SizeInBytes, $"update {frame}: cannon upload bytes");
                        AssertEqual(NativeSnapshotMemory.CannonTileBank | cannon.TileSource, transfers[0].SourceAddress,
                            $"update {frame}: cannon upload source");
                    }
                }
                Check("Cannon flags", (ushort)(samus.ArmCannon.OpenFlag | samus.ArmCannon.CloseFlag << 8), NativeSnapshotMemory.CannonFlags);
                Check("Cannon frame", samus.ArmCannon.Frame, NativeSnapshotMemory.CannonFrame);
                Check("Cannon toggle", samus.ArmCannon.ToggleFlag, NativeSnapshotMemory.CannonToggle);
                Check("Cannon drawing mode", samus.ArmCannon.DrawingMode, NativeSnapshotMemory.CannonDrawingMode);
                Check("Minimap disabled", runtime.Hud.MinimapDisabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.MinimapDisabled);
                CheckBytes("Room scroll storage", RoomScrollGrid.StorageByteCount,
                    runtime.Camera!.Scrolls.ReadStorage, NativeSnapshotMemory.ScrollStorage);
                for (int area = 0; area < Bank80SystemState.ExploredMapAreaCount; area++)
                    CheckBytes($"Explored map {area}", Bank80SystemState.ExploredMapBytesPerArea,
                        index => runtime.System.GetExploredMapByteRaw(area, index),
                        area == W(NativeSnapshotMemory.CurrentArea) ? NativeSnapshotMemory.LiveExploredMap :
                            NativeSnapshotMemory.SavedExploredMaps + area * Bank80SystemState.ExploredMapBytesPerArea);
            }
            // Only the reference's proven hardware-upload NMI count is normalized;
            // gameplay state is never copied back into the production runtime.
            int excludedNmis = frame == 0 ? 0 : updates[frame - 1].GetProperty("excludedNmiAfter").GetInt32();
            ushort normalizedNmi = unchecked((ushort)(W(NativeSnapshotMemory.NmiCounter) - excludedNmis));
            if (runtime.NmiFrameCounter != normalizedNmi)
                mismatches.Add($"Accepted gameplay NMI: native={normalizedNmi:X4} port={runtime.NmiFrameCounter:X4}");
            // All excluded accepted upload NMIs in this movie occur while the door
            // IRQ repeatedly clears this byte. Its post-door phase therefore compares
            // directly, unlike the independently retained word counter above.
            AssertEqual(memory[NativeSnapshotMemory.NmiCounterByte], runtime.NmiFrameCounter8,
                $"update {frame}: byte NMI counter including native door IRQ reset");
            Check("Game state", (ushort)game.GameState, NativeSnapshotMemory.GameState);
            Check("Enemy door gate", runtime.Enemies.EnemyDoorTransitionActive ? (ushort)1 : (ushort)0, NativeSnapshotMemory.EnemyDoorTransition);
            // Native LoadDoorHeader publishes the destination room pointer before
            // loading its room/state data. The port keeps that identity in the pending
            // door while ActiveRoom still owns the source room's loaded data.
            ushort selectedRoom = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.AlignSourceCamera or DoorTransitionPhase.FixDoorsMovingUp or
                DoorTransitionPhase.SetupNewRoom or DoorTransitionPhase.SetupScrolling or
                DoorTransitionPhase.PlaceSamusAndLoadTiles or DoorTransitionPhase.LoadMoreThingsAndOpenDoor
                ? (runtime.PendingDoorTransition ?? throw new InvalidDataException("Missing selected destination door")).DestinationRoomPointer
                : runtime.ActiveRoom!.Pointer;
            Check("Selected room", selectedRoom, NativeSnapshotMemory.Room);
            Check("Camera X", runtime.Camera!.XPosition, NativeSnapshotMemory.CameraX);
            Check("Camera X fraction", runtime.Camera.XSubposition, NativeSnapshotMemory.CameraXFraction);
            Check("Camera Y", runtime.Camera.YPosition, NativeSnapshotMemory.CameraY);
            Check("Camera Y fraction", runtime.Camera.YSubposition, NativeSnapshotMemory.CameraYFraction);
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                Check("Ideal camera X", runtime.Camera.IdealXPosition, NativeSnapshotMemory.IdealCameraX);
                Check("Ideal camera Y", runtime.Camera.IdealYPosition, NativeSnapshotMemory.IdealCameraY);
                Check("Camera speed X", runtime.Camera.CameraXSpeed, NativeSnapshotMemory.CameraSpeedX);
                Check("Camera speed X fraction", runtime.Camera.CameraXSubspeed, NativeSnapshotMemory.CameraSpeedXFraction);
                Check("Camera speed Y", runtime.Camera.CameraYSpeed, NativeSnapshotMemory.CameraSpeedY);
                Check("Camera speed Y fraction", runtime.Camera.CameraYSubspeed, NativeSnapshotMemory.CameraSpeedYFraction);
                // A fresh door camera defers its first sample to the runtime's frame-start
                // fallback. Compare that effective sample instead of requiring storage.
                var previous = runtime.Camera.PreviousSamusPoint ?? new SamusCameraPoint(
                    samus.XPosition, samus.Kinematics.XSubposition, samus.YPosition, samus.Kinematics.YSubposition);
                Check("Previous Samus X", previous.XPosition, NativeSnapshotMemory.PreviousSamusX);
                Check("Previous Samus X fraction", previous.XSubposition, NativeSnapshotMemory.PreviousSamusXFraction);
                Check("Previous Samus Y", previous.YPosition, NativeSnapshotMemory.PreviousSamusY);
                Check("Previous Samus Y fraction", previous.YSubposition, NativeSnapshotMemory.PreviousSamusYFraction);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
                {
                    var effect = samus.LiquidPhysics.AtmosphericEffects.Slots[index];
                    Check($"Atmosphere {index} frame/type", effect.FrameAndType, NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
                    if (effect.Type == 0) continue;
                    Check($"Atmosphere {index} timer", effect.AnimationTimer, NativeSnapshotMemory.AtmosphericTimer + index * 2);
                    Check($"Atmosphere {index} X", effect.XPosition, NativeSnapshotMemory.AtmosphericX + index * 2);
                    Check($"Atmosphere {index} Y", effect.YPosition, NativeSnapshotMemory.AtmosphericY + index * 2);
                }
                Check("Liquid animation buffer", samus.AnimationFrameBuffer, NativeSnapshotMemory.AnimationFrameBuffer);
                Check("LiquidPhysicsType", samus.LiquidPhysics.LiquidPhysicsType, NativeSnapshotMemory.LiquidPhysicsType);
                Check("PeriodicSubDamage", samus.LiquidPhysics.PeriodicSubDamage, NativeSnapshotMemory.PeriodicSubDamage);
                Check("PeriodicDamage", samus.LiquidPhysics.PeriodicDamage, NativeSnapshotMemory.PeriodicDamage);
                Check("Acid damage surface", samus.LiquidPhysics.LavaAcidYPosition, NativeSnapshotMemory.AcidSurface);
                Check("Liquid tide phase", (ushort)typeof(RoomLayer3FxState).GetField("tidePhase", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.RoomLayer3Fx)!, NativeSnapshotMemory.TidePhase);
            }
            Check("Samus X", samus.XPosition, NativeSnapshotMemory.X);
            Check("Samus X fraction", samus.Kinematics.XSubposition, NativeSnapshotMemory.XFraction);
            Check("Samus Y", samus.YPosition, NativeSnapshotMemory.Y);
            Check("Samus Y fraction", samus.Kinematics.YSubposition, NativeSnapshotMemory.YFraction);
            Check("Samus pose", samus.Pose, NativeSnapshotMemory.Pose);
            Check("PreviousDrawHeldInput", samus.PreviousDrawHeldInput, NativeSnapshotMemory.SamusFilteredHeld);
            Check("PreviousDrawNewInput", samus.PreviousDrawNewInput, NativeSnapshotMemory.SamusFilteredNew);
            Check("AutoJumpTimer", samus.AutoJumpTimer, NativeSnapshotMemory.SamusAutoJumpTimer);
            Check("PreviousHealthForHurtCheck", samus.PreviousHealthForHurtCheck, NativeSnapshotMemory.SamusPreviousHealthForFlash);
            AssertTrue(!samus.ShinesparkPoseInputLocked && !samus.CrystalFlashPoseInputLocked,
                "Movie now uses a special pose-input lock; map its handler explicitly");
            Check("Samus input handler", samus.AutoJumpInputPending
                ? NativeSnapshotMemory.SamusAutoJumpInputHandler : NativeSnapshotMemory.SamusNormalInputHandler,
                NativeSnapshotMemory.SamusInputHandler);

            Check("Samus previous pose", samus.PoseHistory.PreviousPose, NativeSnapshotMemory.PreviousPose);
            Check("Samus previous movement", samus.PoseHistory.PreviousDirectionAndMovement, NativeSnapshotMemory.PreviousDirection);
            Check("Samus last different pose", samus.PoseHistory.LastDifferentPose, NativeSnapshotMemory.LastDifferentPose);
            Check("Samus last different movement", samus.PoseHistory.LastDifferentDirectionAndMovement, NativeSnapshotMemory.LastDifferentDirection);
            Check("Samus animation", samus.AnimationFrame, NativeSnapshotMemory.Animation);
            Check("Samus animation timer", samus.AnimationFrameTimer, NativeSnapshotMemory.AnimationTimer);
            Check("Samus base speed", samus.HorizontalSpeed.BaseSpeed, NativeSnapshotMemory.BaseSpeed);
            Check("Samus base fraction", samus.HorizontalSpeed.BaseSubspeed, NativeSnapshotMemory.BaseFraction);
            Check("Samus extra speed", samus.HorizontalSpeed.ExtraRunSpeed, NativeSnapshotMemory.ExtraSpeed);
            Check("Samus extra fraction", samus.HorizontalSpeed.ExtraRunSubspeed, NativeSnapshotMemory.ExtraFraction);
            Check("Samus vertical speed", samus.Kinematics.YSpeed, NativeSnapshotMemory.VerticalSpeed);
            Check("Samus vertical fraction", samus.Kinematics.YSubspeed, NativeSnapshotMemory.VerticalFraction);
            Check("Samus vertical direction", samus.Kinematics.YDirection, NativeSnapshotMemory.VerticalDirection);
            Check("Samus SamusXRadius", samus.Kinematics.XRadius, NativeSnapshotMemory.SamusXRadius);
            Check("Samus SamusYRadius", samus.Kinematics.YRadius, NativeSnapshotMemory.SamusYRadius);
            Check("Samus Gravity", samus.Kinematics.YAcceleration, NativeSnapshotMemory.Gravity);
            Check("Samus GravityFraction", samus.Kinematics.YSubacceleration, NativeSnapshotMemory.GravityFraction);
            Check("Samus ExtraXDisplacement", samus.Kinematics.ExtraXDisplacement, NativeSnapshotMemory.ExtraXDisplacement);
            Check("Samus ExtraXDisplacementFraction", samus.Kinematics.ExtraXSubdisplacement, NativeSnapshotMemory.ExtraXDisplacementFraction);
            Check("Samus ExtraYDisplacement", samus.Kinematics.ExtraYDisplacement, NativeSnapshotMemory.ExtraYDisplacement);
            Check("Samus ExtraYDisplacementFraction", samus.Kinematics.ExtraYSubdisplacement, NativeSnapshotMemory.ExtraYDisplacementFraction);
            Check("Samus SlopeCollisionEnable", samus.Kinematics.HorizontalSlopeCollisionEnable, NativeSnapshotMemory.SlopeCollisionEnable);
            Check("Samus SpeedDivisor", samus.HorizontalSpeed.SpeedDivisor, NativeSnapshotMemory.SpeedDivisor);
            Check("Samus ContactDamageIndex", samus.HorizontalSpeed.ContactDamageIndex, NativeSnapshotMemory.ContactDamageIndex);
            Check("Samus MorphBallBounceState", samus.MorphBallBounceState, NativeSnapshotMemory.MorphBallBounceState);
            Check("Samus BombJumpDirection", samus.BombJumpDirection, NativeSnapshotMemory.BombJumpDirection);
            Check("Samus running momentum", samus.HorizontalSpeed.HasRunningMomentum ? (ushort)1 : (ushort)0, NativeSnapshotMemory.Momentum);
            Check("Samus speed boost counter", samus.HorizontalSpeed.SpeedBoostCounter, NativeSnapshotMemory.BoostCounter);
            Check("Samus horizontal speed table", samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, NativeSnapshotMemory.HorizontalSpeedTable);
            AssertEqual(memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier], samus.HorizontalSpeed.DecelerationMultiplier,
                $"update {frame}: Samus horizontal deceleration multiplier");
            Check("Samus echo sound latch", samus.HorizontalSpeed.EchoSoundFlag, NativeSnapshotMemory.SpeedEchoSoundLatch);
            Check("Samus slope adjustment", samus.Kinematics.PositionAdjustedBySlope ? (ushort)1 : (ushort)0,
                NativeSnapshotMemory.SamusSlopeAdjusted);
            for (int direction = 0; direction < 4; direction++)
                Check($"Samus solid enemy {direction}", samus.Kinematics.SolidEnemyCollisionIndexes[direction],
                    NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2);
            for (int address = NativeSnapshotMemory.ProjectileInheritancePrefix;
                 address < NativeSnapshotMemory.SamusSlopeAdjusted; address += 2)
                Check($"Projectile inherited movement ${address:X4}",
                    (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8), address);
            Check("Samus total horizontal speed", samus.HorizontalSpeed.TotalSpeed, NativeSnapshotMemory.TotalHorizontalSpeed);
            Check("Samus total horizontal fraction", samus.HorizontalSpeed.TotalSubspeed, NativeSnapshotMemory.TotalHorizontalSubspeed);
            Check("Samus movement handler", samus.KnockbackActive
                ? NativeSnapshotMemory.KnockbackMovementHandler : NativeSnapshotMemory.NormalMovementHandler,
                NativeSnapshotMemory.SamusMovementHandler);
            // DoorTransitionState uses InputLocked to suppress host control, while
            // native door dispatch retains ordinary alpha/beta pointers unused.
            bool nativeControlLock = samus.InputLocked && game.GameState is not
                (SuperMetroidGameState.HitDoorBlock or SuperMetroidGameState.LoadingNextRoomA or
                 SuperMetroidGameState.LoadingNextRoomB);
            Check("Samus alpha handler", nativeControlLock
                ? NativeSnapshotMemory.LockedAlphaHandler : NativeSnapshotMemory.NormalAlphaHandler,
                NativeSnapshotMemory.SamusAlphaHandler);
            Check("Samus beta handler", nativeControlLock
                ? NativeSnapshotMemory.LockedBetaHandler : NativeSnapshotMemory.NormalBetaHandler,
                NativeSnapshotMemory.SamusBetaHandler);

            Check("Samus health", samus.Health, NativeSnapshotMemory.Health);
            Check("Samus general Samus damage immunity countdown", samus.InvincibilityTimer, NativeSnapshotMemory.InvincibilityTimer);
            Check("Samus Samus knockback countdown", samus.KnockbackTimer, NativeSnapshotMemory.KnockbackTimer);
            Check("Samus Samus knockback direction", samus.KnockbackDirection, NativeSnapshotMemory.KnockbackDirection);
            Check("Samus horizontal knockback direction", samus.KnockbackXDirection, NativeSnapshotMemory.KnockbackXDirection);
            Check("Samus hurt palette/audio recovery countdown", samus.HurtFlashCounter, NativeSnapshotMemory.HurtFlashCounter);
            Check("Samus fractional health word", samus.SubunitHealth, NativeSnapshotMemory.SubunitHealth);
            Check("Samus selected HUD weapon", samus.SelectedHudItem, NativeSnapshotMemory.SelectedHudItem);
            Check("Samus auto-cancel HUD selection", samus.AutoCancelHudItemIndex, NativeSnapshotMemory.AutoCancelHudItemIndex);
            Check("Samus acceleration mode", samus.HorizontalSpeed.AccelerationMode, NativeSnapshotMemory.AccelerationMode);
            Check("Samus maximum health", samus.MaxHealth, NativeSnapshotMemory.MaxHealth);
            Check("Samus equipped items", samus.EquippedItems, NativeSnapshotMemory.Items);
            Check("Samus collected items", samus.CollectedItems, NativeSnapshotMemory.CollectedItems);
            Check("Samus equipped beams", samus.EquippedBeams, NativeSnapshotMemory.Beams);
            Check("Samus collected beams", samus.CollectedBeams, NativeSnapshotMemory.CollectedBeams);
            Check("Samus missiles", samus.Missiles, NativeSnapshotMemory.Missiles);
            Check("Samus missile capacity", samus.MaxMissiles, NativeSnapshotMemory.MaxMissiles);
            Check("Samus super missiles", samus.SuperMissiles, NativeSnapshotMemory.SuperMissiles);
            Check("Samus super missile capacity", samus.MaxSuperMissiles, NativeSnapshotMemory.MaxSuperMissiles);
            Check("Samus power bombs", samus.PowerBombs, NativeSnapshotMemory.PowerBombs);
            Check("Samus power bomb capacity", samus.MaxPowerBombs, NativeSnapshotMemory.MaxPowerBombs);
            Check("Samus reserve mode", samus.ReserveTankMode, NativeSnapshotMemory.ReserveMode);
            Check("Samus reserve capacity", samus.MaxReserveEnergy, NativeSnapshotMemory.MaxReserve);
            Check("Samus reserve energy", samus.ReserveEnergy, NativeSnapshotMemory.Reserve);
            // The native CPU can still be decompressing source-room tiles while
            // IRQ scrolling advances; the port loads the destination atomically.
            // Align these owners at completed loading, not elapsed upload time.
            // Movement/input remain compared on EVERY IRQ interval. A stable owner
            // snapshot also proves the port does not run destination actors early.
            bool nativeLoading = game.GameState == SuperMetroidGameState.LoadingNextRoomB &&
                W(NativeSnapshotMemory.DoorFunction) is NativeSnapshotMemory.PlaceSamusLoadTiles or NativeSnapshotMemory.LoadMoreThings;
            bool portWaitingForScroll = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading;
            bool deferLoadingOwners = nativeLoading && portWaitingForScroll;
            SnesCgram comparedPalette = runtime.Cgram;
            if (game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB or
                SuperMetroidGameState.UnpausingA or SuperMetroidGameState.UnpausingB)
            {
                object menu = typeof(SuperMetroidGame).GetField("pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(game) ?? throw new InvalidDataException("Native pause palette requires the active pause menu.");
                comparedPalette = (SnesCgram)typeof(PauseMenuState).GetField("cgram", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(menu)!;
            }
            if (!deferLoadingOwners)
            for (int color = 0; color < SnesCgram.ColorCount; color++)
            {
                ushort nativeColor = (ushort)(W(NativeSnapshotMemory.PaletteBuffer + color * 2) & 0x7fff);
                if (comparedPalette.Colors[color].ToWord() != nativeColor)
                    mismatches.Add($"Palette {color}: native={nativeColor:X4} port={comparedPalette.Colors[color]:X4}");
            }
            if (deferLoadingOwners)
            {
                if (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FinishDoorLoading)
                    Check("Completed-scroll visor", runtime.Cgram.Colors[DoorTransitionPaletteDefinitions.VisorColorIndex].ToWord(),
                        NativeSnapshotMemory.PaletteBuffer + DoorTransitionPaletteDefinitions.VisorColorIndex * 2);
                ushort[] owners = CaptureLoadedOwners();
                if (pendingLoadedOwners is not null && !owners.AsSpan().SequenceEqual(pendingLoadedOwners))
                    throw new InvalidDataException("Destination RNG/palette/enemy owners advanced while native hardware loading was still pending.");
                pendingLoadedOwners ??= owners;
                loadingIntervals++;
            }
            else
            {
                if (pendingLoadedOwners is not null)
                    AssertEqual(NativeSnapshotMemory.HandleAnimTiles, W(NativeSnapshotMemory.DoorFunction), "deferred destination owners reach native completed-loading boundary");
                Check("RNG", runtime.System.RandomNumber, NativeSnapshotMemory.Random);
            }
            foreach (var actor in runtime.Enemies.Slots)
            {
                if (deferLoadingOwners) continue;
                int address = NativeSnapshotMemory.EnemyBase + actor.NativeIndex;
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
                W(NativeSnapshotMemory.EnemyBase) == RoomEnemySystem.NorfairRidleyDefinition)
            {
                bool expectedGate = (W(NativeSnapshotMemory.EnemyBase + 14) & 0x0400) != 0;
                if (runtime.Enemies.Slots[0].Properties.HasAny(EnemyProperties.IgnoreSamusCollision) != expectedGate)
                    mismatches.Add($"Ridley interaction gate: native={expectedGate}");
                Check("Ridley AI function", (ushort)ridleyState.Function, NativeSnapshotMemory.RidleyFunction);
                Check("Ridley AI timer", ridleyState.FunctionTimer, NativeSnapshotMemory.RidleyFunctionTimer);
                Check("Ridley TailFunctionIndex", ridleyState.TailFunctionIndex, NativeSnapshotMemory.RidleyTailFunctionIndex);
                Check("Ridley IdleTailWhipEnabled", ridleyState.IdleTailWhipEnabled, NativeSnapshotMemory.RidleyIdleTailWhipEnabled);
                Check("Ridley TailWhipRequest", ridleyState.TailWhipRequest, NativeSnapshotMemory.RidleyTailWhipRequest);
                Check("Ridley TailExtensionSpeed", ridleyState.TailExtensionSpeed, NativeSnapshotMemory.RidleyTailExtensionSpeed);
                Check("Ridley TailAngleDelta", ridleyState.TailAngleDelta, NativeSnapshotMemory.RidleyTailAngleDelta);
                Check("Ridley TailMinimumClockwiseAngle", ridleyState.TailMinimumClockwiseAngle, NativeSnapshotMemory.RidleyTailMinimumClockwiseAngle);
                Check("Ridley TailMaximumCounterClockwiseAngle", ridleyState.TailMaximumCounterClockwiseAngle, NativeSnapshotMemory.RidleyTailMaximumCounterClockwiseAngle);
                Check("Ridley TailWhipTargetClockwiseAngle", ridleyState.TailWhipTargetClockwiseAngle, NativeSnapshotMemory.RidleyTailWhipTargetClockwiseAngle);
                Check("Ridley TailWhipTargetCounterClockwiseAngle", ridleyState.TailWhipTargetCounterClockwiseAngle, NativeSnapshotMemory.RidleyTailWhipTargetCounterClockwiseAngle);
                Check("Ridley IdealInterSegmentTailAngle", ridleyState.IdealInterSegmentTailAngle, NativeSnapshotMemory.RidleyIdealInterSegmentTailAngle);
                Check("Ridley HorizontalVelocity", ridleyState.HorizontalVelocity, NativeSnapshotMemory.RidleyHorizontalVelocity);
                Check("Ridley VerticalVelocity", ridleyState.VerticalVelocity, NativeSnapshotMemory.RidleyVerticalVelocity);
                Check("Ridley FightMode", ridleyState.FightMode, NativeSnapshotMemory.RidleyFightMode);
                Check("Ridley MovementAnimationEnabled", ridleyState.MovementAnimationEnabled, NativeSnapshotMemory.RidleyMovementAnimationEnabled);
                Check("Ridley WingFrame", ridleyState.WingFrame, NativeSnapshotMemory.RidleyWingFrame);
                Check("Ridley WingAnimationTimerDelta", ridleyState.WingAnimationTimerDelta, NativeSnapshotMemory.RidleyWingAnimationTimerDelta);
                Check("Ridley WingAnimationTimer", ridleyState.WingAnimationTimer, NativeSnapshotMemory.RidleyWingAnimationTimer);
                Check("Ridley FacingDirection", ridleyState.FacingDirection, NativeSnapshotMemory.RidleyFacingDirection);
                Check("Ridley HealthStage", ridleyState.HealthStage, NativeSnapshotMemory.RidleyHealthStage);
                Check("Ridley GrabXOffset", ridleyState.GrabXOffset, NativeSnapshotMemory.RidleyGrabXOffset);
                Check("Ridley GrabYOffset", ridleyState.GrabYOffset, NativeSnapshotMemory.RidleyGrabYOffset);
                Check("Ridley TailDamage", ridleyState.TailDamage, NativeSnapshotMemory.RidleyTailDamage);
                Check("Ridley FeetDistanceIndex", ridleyState.FeetDistanceIndex, NativeSnapshotMemory.RidleyFeetDistanceIndex);
                Check("Ridley IntangibilityTimer", ridleyState.IntangibilityTimer, NativeSnapshotMemory.RidleyIntangibilityTimer);
                if (ridleyState.Function is RidleyAiFunction.NorfairSwoopMoveToStart or
                    RidleyAiFunction.NorfairSwoopAimDown or RidleyAiFunction.NorfairSwoopAimSideways or
                    RidleyAiFunction.NorfairSwoopAimUp or RidleyAiFunction.NorfairSwoopClimb or RidleyAiFunction.NorfairSwoopRecover)
                    Check("Ridley swoop timer", ridleyState.SwoopPhaseTimer, NativeSnapshotMemory.RidleySwoopTimer);
                if (game.GameState == SuperMetroidGameState.MainGameplay)
                {
                    // Tail workspace becomes live after its first fade-owned composition.
                    Check("Ridley tail tip X", ridleyState.TailSegments[6].XPosition, NativeSnapshotMemory.TailTipX);
                    Check("Ridley tail tip Y", ridleyState.TailSegments[6].YPosition, NativeSnapshotMemory.TailTipY);
                    for (int tailIndex = 0; tailIndex < ridleyState.TailSegments.Length; tailIndex++)
                    {
                        var segment = ridleyState.TailSegments[tailIndex];
                        int tailAddress = NativeSnapshotMemory.TailSegments + tailIndex * NativeSnapshotMemory.TailSegmentStride;
                        string tailOwner = $"Ridley tail {tailIndex}";
                        Check(tailOwner + " active", segment.Active ? NativeSnapshotMemory.TailSegmentActive : (ushort)0, tailAddress);
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
                    Check($"PLM {slotIndex} header", active ? slot.HeaderPointer : (ushort)0, NativeSnapshotMemory.PlmHeaders + offset);
                    if (!active || W(NativeSnapshotMemory.PlmHeaders + offset) == 0) continue;
                    ushort instruction = slot.InstructionPointer;
                    ushort preInstruction = slot.PreInstruction == 0 ? NativeSnapshotMemory.PlmDefaultPreInstruction : slot.PreInstruction;
                    var greyDoor = runtime.Plms.GreyDoors.FirstOrDefault(door => door.Header == slot.HeaderPointer && door.BlockIndex == slot.BlockIndex);
                    AssertTrue(greyDoor.Header != 0,
                        $"Movie PLM {slot.HeaderPointer:X4} needs a family-variable coverage mapping");
                    Check($"PLM {slotIndex} grey-door condition", (ushort)((int)greyDoor.Condition * 2), NativeSnapshotMemory.PlmFamilyVariable + offset);
                    // Grey doors require one hit. Opening owns the incremented counter;
                    // before that transition setup's zero is the live counter value.
                    Check($"PLM {slotIndex} grey-door hit count",
                        greyDoor.Phase == GreyDoorPhase.Opening ? (ushort)1 : (ushort)0,
                        NativeSnapshotMemory.PlmExtraVariable + offset);
                    if (greyDoor.Phase != GreyDoorPhase.Closing)
                    {
                        ushort NativeProgramWord(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                        ushort activation = NativeProgramWord(NativeSnapshotMemory.PlmProgramBank | (greyDoor.InitialList + 6));
                        ushort link = NativeProgramWord(NativeSnapshotMemory.PlmProgramBank | (activation + 2));
                        if (greyDoor.Phase == GreyDoorPhase.Locked)
                        {
                            instruction = unchecked((ushort)(greyDoor.InitialList + 14));
                            preInstruction = NativeProgramWord(NativeSnapshotMemory.GreyDoorConditionTable + (int)greyDoor.Condition * 2);
                            link = activation;
                        }
                        else if (greyDoor.Phase == GreyDoorPhase.Flashing)
                            preInstruction = NativeProgramWord(NativeSnapshotMemory.PlmProgramBank | (activation + 6));
                        Check($"PLM {slotIndex} semantic grey-door link", link, NativeSnapshotMemory.PlmLinkInstruction + offset);
                    }
                    Check($"PLM {slotIndex} BlockIndex", unchecked((ushort)(slot.BlockIndex * 2)), NativeSnapshotMemory.PlmBlockIndex + offset);
                    Check($"PLM {slotIndex} PreInstruction", preInstruction, NativeSnapshotMemory.PlmPreInstruction + offset);
                    Check($"PLM {slotIndex} InstructionPointer", instruction, NativeSnapshotMemory.PlmInstructionPointer + offset);
                    Check($"PLM {slotIndex} LoopTimer", slot.LoopTimer, NativeSnapshotMemory.PlmLoopTimer + offset);
                    Check($"PLM {slotIndex} RoomArgument", slot.RoomArgument, NativeSnapshotMemory.PlmRoomArgument + offset);
                    // Locked semantic doors omit Sleep's unconsumed countdown; the
                    // condition callback resets it to one before waking the native list.
                    if (greyDoor.Header == 0 || greyDoor.Phase != GreyDoorPhase.Locked)
                        Check($"PLM {slotIndex} InstructionTimer", slot.InstructionTimer, NativeSnapshotMemory.PlmInstructionTimer + offset);
                    // Closing never consumes the retained link. Its fallthrough into
                    // InitialList overwrites it before installing the condition callback;
                    // all subsequent live links are checked through the semantic phase.
                }
                var activeLevel = runtime.LevelData ?? throw new InvalidDataException("Missing active collision data.");
                for (int block = 0; block < activeLevel.WidthInBlocks * activeLevel.HeightInBlocks; block++)
                {
                    Check($"Room block {block}", activeLevel.ForegroundEntries.Span[block], NativeSnapshotMemory.Level + block * 2);
                    byte expectedBehavior = memory[NativeSnapshotMemory.Bts + block];
                    byte actualBehavior = activeLevel.BehaviorBytes.Span[block];
                    if (actualBehavior != expectedBehavior)
                        mismatches.Add($"Room BTS {block}: native={expectedBehavior:X2} port={actualBehavior:X2}");
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                foreach (var trail in runtime.Projectiles.TrailSlots)
                {
                    Check($"Trail {trail.SlotIndex} Left timer", trail.Left.InstructionTimer, NativeSnapshotMemory.TrailLeftInstructionTimer + trail.NativeByteIndex);
                    if (trail.Left.InstructionTimer != 0)
                    {
                        Check($"Trail {trail.SlotIndex} Left InstructionPointer", trail.Left.InstructionPointer, NativeSnapshotMemory.TrailLeftInstructionPointer + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left TileNumberAttributes", trail.Left.TileNumberAttributes, NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left XPosition", trail.Left.XPosition, NativeSnapshotMemory.TrailLeftXPosition + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left YPosition", trail.Left.YPosition, NativeSnapshotMemory.TrailLeftYPosition + trail.NativeByteIndex);
                    }
                    Check($"Trail {trail.SlotIndex} Right timer", trail.Right.InstructionTimer, NativeSnapshotMemory.TrailRightInstructionTimer + trail.NativeByteIndex);
                    if (trail.Right.InstructionTimer != 0)
                    {
                        Check($"Trail {trail.SlotIndex} Right InstructionPointer", trail.Right.InstructionPointer, NativeSnapshotMemory.TrailRightInstructionPointer + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right TileNumberAttributes", trail.Right.TileNumberAttributes, NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right XPosition", trail.Right.XPosition, NativeSnapshotMemory.TrailRightXPosition + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right YPosition", trail.Right.YPosition, NativeSnapshotMemory.TrailRightYPosition + trail.NativeByteIndex);
                    }
                }
                Check("Projectile cooldown", runtime.BombProjectiles.CooldownTimer, NativeSnapshotMemory.ProjectileCooldown);
                Check("Beam charge", runtime.Projectiles.FlareCounter, NativeSnapshotMemory.BeamCharge);
                Check("ProjectileCounter", runtime.Projectiles.ProjectileCounter, NativeSnapshotMemory.ProjectileCount);
                Check("PreviousBeamChargeCounter", runtime.Projectiles.PreviousBeamChargeCounter, NativeSnapshotMemory.PreviousCharge);
                Check("ProjectileInvincibilityTimer", runtime.Projectiles.ProjectileInvincibilityTimer, NativeSnapshotMemory.ProjectileInteractionImmunity);
                Check("ChargedShotGlowTimer", runtime.Projectiles.ChargedShotGlowTimer, NativeSnapshotMemory.ChargedShotGlow);
                Check("SamusChargePaletteIndex", runtime.Projectiles.SamusChargePaletteIndex, NativeSnapshotMemory.ChargePaletteIndex);
                Check("BombCounter", runtime.BombProjectiles.BombCounter, NativeSnapshotMemory.BombCount);
                // This movie never places a bomb. Check every physical bomb slot and
                // both activation owners so that absence is verified, not assumed from
                // the aggregate counter or from ordinary beam/missile comparisons.
                foreach (var bomb in runtime.BombProjectiles.Slots)
                {
                    int address = NativeSnapshotMemory.ProjectileType + (bomb.Index + 5) * 2;
                    Check($"Bomb {bomb.Index} type", bomb.Type, address);
                    AssertTrue(W(address) == 0,
                        "Movie now activates a bomb slot; add its full live-state mapping");
                }
                var explosion = runtime.BombProjectiles.PowerBombExplosion;
                Check("Power-bomb armed flag", explosion.Flag, NativeSnapshotMemory.PowerBombArmedFlag);
                Check("Power-bomb explosion status", explosion.Status, NativeSnapshotMemory.PowerBombExplosionStatus);
                AssertTrue(W(NativeSnapshotMemory.PowerBombArmedFlag) == 0 &&
                    W(NativeSnapshotMemory.PowerBombExplosionStatus) == 0,
                    "Movie now activates a power bomb; add its full live-state mapping");
                Check("BombSpreadChargeTimeoutCounter", samus.BombSpreadChargeTimeoutCounter, NativeSnapshotMemory.BombSpreadChargeTimeout);
                Check("PoseTransitionShotDirection", samus.PoseTransitionShotDirection, NativeSnapshotMemory.PoseShotDirection);
                Check("HyperBeam", samus.HyperBeam, NativeSnapshotMemory.HyperBeam);
                Check("ResumeChargingBeamSoundFlag", samus.ResumeChargingBeamSoundFlag, NativeSnapshotMemory.ResumeChargeSound);

            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Projectiles.Slots.Take(5))
            {
                int index = projectile.NativeByteIndex;
                string owner = $"Projectile {projectile.SlotIndex}";
                Check(owner + " type", projectile.Type, NativeSnapshotMemory.ProjectileType + index);
                if (projectile.Type == 0 || W(NativeSnapshotMemory.ProjectileType + index) == 0) continue;
                Check(owner + " X", projectile.XPosition, NativeSnapshotMemory.ProjectileX + index);
                Check(owner + " Y", projectile.YPosition, NativeSnapshotMemory.ProjectileY + index);
                Check(owner + " X radius", projectile.XRadius, NativeSnapshotMemory.ProjectileXRadius + index);
                Check(owner + " Y radius", projectile.YRadius, NativeSnapshotMemory.ProjectileYRadius + index);
                Check(owner + " damage", projectile.Damage, NativeSnapshotMemory.ProjectileDamage + index);
                Check(owner + " XFraction", unchecked((ushort)projectile.XSubposition), NativeSnapshotMemory.ProjectileXFraction + index);
                Check(owner + " YFraction", unchecked((ushort)projectile.YSubposition), NativeSnapshotMemory.ProjectileYFraction + index);
                Check(owner + " XVelocity", unchecked((ushort)projectile.XVelocity), NativeSnapshotMemory.ProjectileXVelocity + index);
                Check(owner + " YVelocity", unchecked((ushort)projectile.YVelocity), NativeSnapshotMemory.ProjectileYVelocity + index);
                Check(owner + " Direction", unchecked((ushort)projectile.Direction), NativeSnapshotMemory.ProjectileDirection + index);
                Check(owner + " Instruction", unchecked((ushort)projectile.InstructionPointer), NativeSnapshotMemory.ProjectileInstruction + index);
                ushort callback = projectile.PreInstruction switch
                {
                    SamusProjectilePreInstruction.None => NativeSnapshotMemory.ProjectileEmptyCallback,
                    SamusProjectilePreInstruction.NoWaveBeam => SamusBeamPreInstructionCodes.NoWave,
                    SamusProjectilePreInstruction.WaveBeamThreeFrameTrail => SamusBeamPreInstructionCodes.WaveThreeFrameTrail,
                    SamusProjectilePreInstruction.WaveBeamFourFrameTrail => SamusBeamPreInstructionCodes.WaveFourFrameTrail,
                    SamusProjectilePreInstruction.Missile => NativeSnapshotMemory.ProjectileMissileCallback,
                    SamusProjectilePreInstruction.SuperMissile => NativeSnapshotMemory.ProjectileSuperMissileCallback,
                    SamusProjectilePreInstruction.SuperMissileLink => NativeSnapshotMemory.ProjectileSuperMissileLinkCallback,
                    _ => throw new InvalidDataException($"Movie projectile callback {projectile.PreInstruction} needs a native identity mapping"),
                };
                Check(owner + " PreInstruction", callback, NativeSnapshotMemory.ProjectilePreInstruction + index);
                Check(owner + " InstructionTimer", unchecked((ushort)projectile.InstructionTimer), NativeSnapshotMemory.ProjectileInstructionTimer + index);
                Check(owner + " Variable", unchecked((ushort)projectile.Variable), NativeSnapshotMemory.ProjectileVariable + index);
                Check(owner + " TrailTimer", unchecked((ushort)projectile.TrailTimer), NativeSnapshotMemory.ProjectileTrailTimer + index);
                Check(owner + " AuxiliaryPhase", unchecked((ushort)projectile.AuxiliaryPhase), NativeSnapshotMemory.ProjectileAuxiliaryPhase + index);
                Check(owner + " Spritemap", unchecked((ushort)projectile.SpritemapPointer), NativeSnapshotMemory.ProjectileSpritemap + index);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Enemies.EnemyProjectiles)
            {
                int index = projectile.SlotIndex * 2;
                string owner = $"Enemy projectile {projectile.SlotIndex}";
                Check(owner + " identity", (ushort)projectile.Kind, NativeSnapshotMemory.EnemyProjectileId + index);
                if (!projectile.IsActive || W(NativeSnapshotMemory.EnemyProjectileId + index) == 0) continue;
                Check(owner + " X", projectile.XPosition, NativeSnapshotMemory.EnemyProjectileX + index);
                Check(owner + " Y", projectile.YPosition, NativeSnapshotMemory.EnemyProjectileY + index);
                Check(owner + " Graphics", projectile.GraphicsIndex, NativeSnapshotMemory.EnemyProjectileGraphics + index);
                Check(owner + " Timer", projectile.GeneralTimer, NativeSnapshotMemory.EnemyProjectileTimer + index);
                Check(owner + " PreInstruction", projectile.PreInstruction, NativeSnapshotMemory.EnemyProjectilePreInstruction + index);
                Check(owner + " XFraction", projectile.XSubposition, NativeSnapshotMemory.EnemyProjectileXFraction + index);
                Check(owner + " YFraction", projectile.YSubposition, NativeSnapshotMemory.EnemyProjectileYFraction + index);
                Check(owner + " XVelocity", projectile.XVelocity, NativeSnapshotMemory.EnemyProjectileXVelocity + index);
                Check(owner + " YVelocity", projectile.YVelocity, NativeSnapshotMemory.EnemyProjectileYVelocity + index);
                Check(owner + " Instruction", projectile.InstructionPointer, NativeSnapshotMemory.EnemyProjectileInstruction + index);
                ushort nativeMap = W(NativeSnapshotMemory.EnemyProjectileSpritemap + index);
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
                Check(owner + " InstructionTimer", projectile.InstructionTimer, NativeSnapshotMemory.EnemyProjectileInstructionTimer + index);
                Check(owner + " radii", (ushort)(projectile.XRadius | projectile.YRadius << 8), NativeSnapshotMemory.EnemyProjectileRadius + index);
                ushort properties = projectile.Damage;
                if (projectile.DrawPriority == EnemyProjectileDrawPriority.High) properties |= NativeSnapshotMemory.EnemyProjectileHighDraw;
                if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
                if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
                if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
                Check(owner + " properties", properties, NativeSnapshotMemory.EnemyProjectileProperties + index);
                ushort variableE = projectile.Variable0, variableF = projectile.Variable1;
                switch (projectile.Kind)
                {
                    case RoomEnemyProjectileKind.CeresRidleyFireball:
                        // $86:940E consumes F only as a zero/nonzero afterburn gate.
                        // The caller may leave a noncanonical nonzero parameter (e.g. $E).
                        if ((projectile.RemainingAfterburns != 0) !=
                            (W(NativeSnapshotMemory.EnemyProjectileVariableF + index) == 0))
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
                Check(owner + " variable E", variableE, NativeSnapshotMemory.EnemyProjectileVariableE + index);
                if (projectile.Kind != RoomEnemyProjectileKind.CeresRidleyFireball)
                    Check(owner + " variable F", variableF, NativeSnapshotMemory.EnemyProjectileVariableF + index);
                Check(owner + " variable G", projectile.CollidedProjectileType, NativeSnapshotMemory.EnemyProjectileVariableG + index);
                Check(owner + " collision option", projectile.CollisionOption, NativeSnapshotMemory.EnemyProjectileCollisionOption + index);
                if (projectile.Kind is RoomEnemyProjectileKind.EnemyDeathExplosion or RoomEnemyProjectileKind.EnemyDeathPickup)
                {
                    AssertTrue(projectile.ItemDropChancesPointerOverride == 0,
                        "Movie drop uses a direct chance-table override; map its native source before comparing");
                    // Death explosions consume their per-slot header on their later
                    // drop instruction. Direct F337 pickups consume it immediately via
                    // caller X ($86:EF3E/F118), not allocated-slot Y, and never read it
                    // again. Their retained per-slot word is not a live source identity.
                    if (projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion)
                        Check(owner + " source enemy header", projectile.EnemyHeaderPointer, NativeSnapshotMemory.EnemyProjectileEnemyHeader + index);
                    Check(owner + " killed enemy index", projectile.KilledEnemyNativeIndex, NativeSnapshotMemory.EnemyProjectileKilledEnemy + index);
                }
                ushort nativeDamage = (ushort)(W(NativeSnapshotMemory.EnemyProjectileProperties + index) & 0x0fff);
                if (projectile.Damage != nativeDamage) mismatches.Add(owner + $" damage: native={nativeDamage} port={projectile.Damage}");
            }
            if (mismatches.Count != 0)
            {
                RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException("Missing active room collision data.");
                Console.Error.WriteLine($"Pose history: port={samus.PoseHistory.PreviousPose:X4}/{samus.PoseHistory.PreviousDirectionAndMovement:X4}/{samus.PoseHistory.LastDifferentPose:X4}/{samus.PoseHistory.LastDifferentDirectionAndMovement:X4}, native={W(NativeSnapshotMemory.PreviousPose):X4}/{W(NativeSnapshotMemory.PreviousDirection):X4}/{W(NativeSnapshotMemory.LastDifferentPose):X4}/{W(NativeSnapshotMemory.LastDifferentDirection):X4}");
                Console.Error.WriteLine($"Liquid diagnostic: Y={samus.YPosition:X4}, surface={samus.LiquidPhysics.LavaAcidYPosition:X4}, pose={samus.Pose:X2}, radius={samus.Kinematics.YRadius}");
                Console.Error.WriteLine($"Shot diagnostic: locked={samus.InputLocked}, HUD={samus.SelectedHudItem}, grappleDebug={runtime.DebugGrappleItemSelected}, charge={runtime.Projectiles.FlareCounter}, cooldown={runtime.BombProjectiles.CooldownTimer}, held={runtime.Controller1.Current:X4}, new={runtime.Controller1.NewlyPressed:X4}, spawn={runtime.Projectiles.LastFiredProjectileSnapshot}");
                Console.Error.WriteLine($"Room width={level.WidthInBlocks}, Samus radius={samus.Kinematics.XRadius}/{samus.Kinematics.YRadius}, speed={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4}+{samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4}");
                for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
                {
                    ushort expectedBlock = W(NativeSnapshotMemory.Level + block * 2);
                    ushort actualBlock = level.ForegroundEntries.Span[block];
                    if (expectedBlock != actualBlock) Console.Error.WriteLine($"Block {block} ({block % level.WidthInBlocks},{block / level.WidthInBlocks}): native={expectedBlock:X4} port={actualBlock:X4}");
                }
                throw new InvalidDataException($"Full movie first divergence at update {frame} (SMV source frame {(frame == 0 ? 0 : updates[frame - 1].GetProperty("sourceFrame").GetInt32())}): " + string.Join("; ", mismatches));
            }
            if (!deferLoadingOwners && pendingLoadedOwners is not null)
            {
                Console.WriteLine($"Completed-loading RNG/palette/enemy owners match after {loadingIntervals} IRQ intervals; movement/input checked throughout.");
                pendingLoadedOwners = null;
            }
            // Only the converted controller event enters production; reference memory is
            // read-only. An accepted input read during APU transfer is not another
            // gameplay update. Until its latch/counter effects are normalized, stop
            // explicitly instead of replaying hardware upload time as gameplay.
            if (frame < length)
            {
                if (updates[frame].GetProperty("timingClass").GetString() is
                    "apu-upload-continuation" or "apu-upload-tail-continuation")
                    throw new InvalidDataException($"SMV source frame {updates[frame].GetProperty("sourceFrame").GetInt32()} is an APU upload continuation; hardware-wait input normalization is not implemented.");
                previousBodyRecord = BodyRecord();
                var output = game.Step((ushort)updates[frame].GetProperty("input").GetInt32());
                audio.RenderFrame(output.AudioCommands);
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            }
        }
        AssertTrue(pendingLoadedOwners is null, "no deferred loading comparison remains at movie end");
        AssertTrue(trace.ReadByte() == -1, "trace ends after movie terminal frame");
        Console.WriteLine($"Samus body records: {pairedBodyDraws} paired visible updates, {phaseShiftedBodyDraws} hardware-phase-shifted updates, {retainedHiddenBodyRecords} hidden-record retention checks.");
        Console.WriteLine($"Full movie input replay: {length} updates across all 10890 source frames match the currently instrumented fields.");
    }
}
