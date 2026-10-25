using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Core.Input;
using System.Security.Cryptography;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.NativeSnapshotImportVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleySamusDrawReferenceVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Bosses.Ridley;

using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>Focused Ridley fight checks and the full recorded-movie gameplay comparison.</summary>
internal static class RidleyFullMovieVerification
{
    /// <summary>The supplied Ridley showcase movie (#1266), replayed by <c>--ridley-full-movie</c>.</summary>
    internal const string RidleyMoviePath = "csharp/test-fixtures/issue-1266-ridley/Ridley fight showcase.smv";

    /// <summary>SHA-256 of <see cref="RidleyMoviePath"/>.</summary>
    internal const string RidleyMovieSha256 = "7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66";

    /// <summary>The committed compact trace of <see cref="RidleyMoviePath"/>.</summary>
    internal const string RidleyTraceDirectory = "csharp/test-fixtures/replay-traces/ridley";

    /// <summary>Checks spin fireball emission against Ridley's native animation and attack phase.</summary>
    internal static void VerifyRidleySpinFireball()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var pointers = new RidleyNativeInstructionPointers(CartridgeImportSource.Require(bus));
        var body = runtime.Enemies.SlotTable.Slots[0]; var state = runtime.Enemies.RidleyEncounter.Ridley; var samus = runtime.Player.IntroducedSamus;
        foreach (var sample in new[] { new VerifyRidleySpinFireballCase(0x7f, false, RidleyFacing.Right, false), new VerifyRidleySpinFireballCase(0x80, false, RidleyFacing.Right, true),
            new VerifyRidleySpinFireballCase(0xff, true, RidleyFacing.Right, false), new VerifyRidleySpinFireballCase(0xff, false, RidleyFacing.Turning, false), new VerifyRidleySpinFireballCase(0x180, false, RidleyFacing.Left, true) })
        {
            runtime.System.SetRandomNumber((ushort)sample.First);
            (state ?? throw new InvalidOperationException("Verification requires state.")).Roaring = sample.Second; state.FacingDirection = sample.Third;
            state.FunctionTimer = WrappingCountdown.Start(20); samus.Pose = SamusPoseId.SpinJumpRightPose;
            state.BodyProgram = pointers.BodyCursorAt(NativeSnapshotMemory.RidleyRightFlyingSleep);
            body.InstructionTimer = WrappingCountdown.Start(9); body.Timer = WrappingCountdown.Start(11);
            runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyPogo(body, state, samus, VerticalDirection.Down);
            AssertEqual(sample.Fourth ? RidleyNativeInstructionPointers.BodyEntry(RidleyBodyProgram.Fireballing) : NativeSnapshotMemory.RidleyRightFlyingSleep,
                pointers.Body(state), "native spin-response fireball threshold/roar/facing gate");
            AssertEqual(sample.Fourth ? (ushort)1 : (ushort)9, body.InstructionTimer.RawValue, "native fireball restarts instruction timer only when admitted");
            AssertEqual(sample.Fourth ? (ushort)0 : (ushort)11, body.Timer.RawValue, "native fireball clears loop counter only when admitted");
            AssertEqual((ushort)sample.First, runtime.System.RandomNumber, "native fireball admission does not generate random state");
        }
        Console.WriteLine("Ridley spin-response fireball: native RNG threshold, roar/turn gates and instruction reset pass.");
    }

    /// <summary>Checks the order of Ridley touch damage, invulnerability and combat reactions.</summary>
    internal static void VerifyRidleyContactOrdering()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 158));
        var body = runtime.Enemies.SlotTable.Slots[0]; var state = runtime.Enemies.RidleyEncounter.Ridley; var samus = runtime.Player.IntroducedSamus;
        body.Position = body.Position.WithWhole(new(139, 282));
        body.SpritemapPointer = 0xe9e9; body.Properties = EnemyPropertyWord.FromNativeWord(0x3800);
        (state ?? throw new InvalidOperationException("Verification requires state.")).FightMode = RidleyFightMode.Fighting; state.MovementAnimationEnabled = true; state.FacingDirection = RidleyFacing.Left;
        state.Function = RidleyAiFunction.NorfairCarryRelease; state.FunctionTimer = WrappingCountdown.Start(10);
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0xfc00)); state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0x0300));
        state.TailFunctionIndex = RidleyTailFunction.None;
        samus.Position = samus.Position.WithWhole(new(110, 300)); samus.Pose = SamusPoseId.NormalJumpAimDiagonalUpRightPose;
        samus.Health = 399; samus.EquippedItems = SamusEquipmentFlags.GravitySuit;
        samus.InvincibilityTimer = WrappingCountdown.Start(0); SamusPoseMetadata.RefreshCollisionRadii(samus);
        // One EnemyMain pass outside the frame loop, as the door-transition wait runs it: no prepared processing list.
        runtime.EnemyPhase.RunEnemyMainPhase(processingList: EnemyProcessingList.Unprepared, runtime.Room.Camera?.PreviousSamusPoint, runtime.Room.LoadedScope);
        AssertEqual(399, samus.Health, "source4234 body entering overlap cannot damage before next contact pass");
        AssertTrue(body.Position.X.Whole < 139, "fixture advances body into collision boundary");
        body.SpritemapPointer = 0xe9e9;
        runtime.Enemies.ResolveRidleySamusContact(samus);
        AssertEqual(359, samus.Health, "next pre-AI body overlap remains damaging");
        Console.WriteLine("Ridley body contact: pre-movement boundary and subsequent damaging overlap pass.");
    }

    /// <summary>Checks the timer boundary that admits Ridley's swooping attack.</summary>
    internal static void VerifyRidleySwoopTimer()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var body = runtime.Enemies.SlotTable.Slots[0]; var state = runtime.Enemies.RidleyEncounter.Ridley;
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = RidleyAiFunction.NorfairSwoopSetup; state.FunctionTimer = WrappingCountdown.Start(112);
        body.Position = body.Position.WithWhole(new(212, 200)); state.FacingDirection = RidleyFacing.Right;
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual((ushort)112, state.FunctionTimer.RawValue, "native swoop setup retains general AI timer");
        AssertEqual((ushort)10, state.SwoopPhaseTimer.RawValue, "native swoop setup initializes independent countdown");
        state.Function = RidleyAiFunction.NorfairSwoopAimDown; state.SwoopPhaseTimer = WrappingCountdown.Start(1);
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual((ushort)0, state.SwoopPhaseTimer.RawValue, "native swoop decrements its own timer");
        AssertEqual((ushort)112, state.FunctionTimer.RawValue, "native swoop countdown retains general timer");
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual((ushort)20, state.SwoopPhaseTimer.RawValue, "native next swoop phase installs its own duration");
        AssertEqual((ushort)112, state.FunctionTimer.RawValue, "native phase transition retains general timer");
        Console.WriteLine("Ridley swoop: independent countdown and retained general AI timer pass.");
    }

    /// <summary>Checks the death animation's final transition after Ridley's health reaches zero.</summary>
    internal static void VerifyRidleyDeathFinish()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var body = runtime.Enemies.SlotTable.Slots[0]; var state = runtime.Enemies.RidleyEncounter.Ridley;
        runtime.Enemies.DeathsAndDrops.BindSamus(runtime.Player.Samus);
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = RidleyAiFunction.NorfairDeathFinish; state.FunctionTimer = WrappingCountdown.Start(0);
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(RidleyAiFunction.NorfairDeathComplete, state.Function, "death finish selects native C600 return");
        AssertTrue(body.Properties.HasAny(EnemyProperties.Deleted), "death finish publishes deletion");
        AssertTrue(state.BossDefeatPublished && runtime.Enemies.RidleyBehavior.Death.RidleyDeathDropRequested, "death finish publishes boss defeat and drops");
        int random = runtime.System.RandomNumber;
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(random, runtime.System.RandomNumber, "terminal return does not spawn another drop scatter");
        AssertEqual(ushort.MaxValue, state.FunctionTimer.RawValue, "terminal return retains expired countdown");
        Console.WriteLine("Ridley death finish: native terminal return, deletion, defeat/drop publication and inert subsequent dispatch pass.");
    }

    /// <summary>Checks descending-lunge grab entry and the zero-health branch's immediate native handoff.</summary>
    internal static void VerifyRidleyGrabEntry()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var body = runtime.Enemies.SlotTable.Slots[0]; var state = runtime.Enemies.RidleyEncounter.Ridley; var samus = runtime.Player.IntroducedSamus;
        body.Position = body.Position.WithWhole(new(211, 346));
        (state ?? throw new InvalidOperationException("Verification requires state.")).Velocity = state.Velocity.WithX(new SnesSignedEightEight(0xc0)); state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0x400));
        state.FacingDirection = RidleyFacing.Right; state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.GrabState = RidleyGrabState.Released; state.FeetDistanceIndex = 0;
        samus.Position = samus.Position.WithWhole(new(198, 402)); samus.Pose = SamusPoseId.KnockbackLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGroundAttack(body, state, samus, runtime.Room.LevelData);
        AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "native grab immediately enters carry movement");
        AssertEqual((ushort)31, state.FunctionTimer.RawValue, "native setup falls through first carry countdown");
        AssertEqual((ushort)0xb6, state.Velocity.X.RawValue, "native same-update grab X acceleration");
        AssertEqual((ushort)0xfbfc, state.Velocity.Y.RawValue, "native same-update grab Y acceleration");
        AssertTrue(samus.Control.Mode.Beta == SamusBetaHandler.ScriptLocked, "native grab installs command-zero alpha/beta pair");
        var bombs = CreateBombFixture();
        bombs.SetSharedCooldown(10);
        bombs.StepFrame(new SamusTerrain { Level = runtime.Room.LoadedLevelData }, samus, 0, 0, explosion: PowerBombExplosionStep.Skip);
        AssertEqual((ushort)10, bombs.CooldownTimer.RawValue, "native locked alpha preserves cooldown");
        NorfairRidleyDeath.ReleaseNorfairRidleyGrab(state, samus);
        AssertTrue(!samus.Control.AlphaLocked && samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked, "native release restores ordinary control");
        bombs.StepFrame(new SamusTerrain { Level = runtime.Room.LoadedLevelData }, samus, 0, 0, explosion: PowerBombExplosionStep.Skip);
        AssertEqual((ushort)9, bombs.CooldownTimer.RawValue, "native unlocked alpha resumes cooldown");
        // Native source8395: a descending lunge grabs standing Samus. Ground attacks
        // have their own reversal; this entry must negate after its lunge acceleration.
        body.Position = new(new(172, 0x6300), new(345, 0x0a00));
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0x2b0)); state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0x3cd));
        state.FacingDirection = RidleyFacing.Right; state.HealthStage = 3; state.HitRoomBoundary = false;
        state.GrabState = RidleyGrabState.Released; state.FeetDistanceIndex = 0;
        samus.Position = samus.Position.WithWhole(new(156, 411)); samus.Pose = SamusPoseId.FacingLeftNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGrabApproach(body, state, samus);
        AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "lunge grab enters immediate carry");
        AssertEqual((ushort)0x2b2, state.Velocity.X.RawValue, "native lunge grab X acceleration");
        AssertEqual((ushort)0xfc25, state.Velocity.Y.RawValue, "native lunge grab reverses before carry acceleration");
        AssertEqual((ushort)31, state.FunctionTimer.RawValue, "native lunge grab falls through countdown");
        // Source10132 takes the zero-health branch, whose JMP executes C538 immediately.
        body.Position = new(new(134, 0x7d00), new(356, 0xec00)); body.Health = 0;
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0x1de)); state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0x419));
        state.FightMode = RidleyFightMode.Fighting; state.GrabState = RidleyGrabState.Released; state.FunctionTimer = WrappingCountdown.Start(0x29);
        samus.Position = samus.Position.WithWhole(new(134, 411)); samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGrabApproach(body, state, samus);
        AssertEqual(RidleyAiFunction.NorfairReleaseSamus, state.Function, "zero-health grab enters death movement");
        AssertEqual((ushort)0x1c6, state.Velocity.X.RawValue, "death entry immediately accelerates X toward death spot");
        AssertEqual((ushort)0xfbde, state.Velocity.Y.RawValue, "death entry immediately accelerates Y toward death spot");
        AssertEqual((ushort)0x29, state.FunctionTimer.RawValue, "death entry retains existing general countdown");
        foreach (var function in new[] { RidleyAiFunction.NorfairCarryRise, RidleyAiFunction.NorfairCarryRelease })
        {
            state.Function = function; state.FunctionTimer = WrappingCountdown.Start(0);
            state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(0xffca)); state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0xfecb));
            state.Target = state.Target.WithX(208); body.Position = body.Position.WithWhole(new(212, 142));
            runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, samus, 0, runtime.Room.LevelData);
            AssertEqual((ushort)0xffca, state.Velocity.X.RawValue, "native expiry skips X acceleration");
            AssertEqual((ushort)0xfecb, state.Velocity.Y.RawValue, "native expiry skips Y acceleration");
            AssertEqual(function == RidleyAiFunction.NorfairCarryRise ? RidleyAiFunction.NorfairCarryRelease : RidleyAiFunction.NorfairSelectAttack, state.Function, "native expiry selects next carry phase");
            AssertEqual((ushort)(function == RidleyAiFunction.NorfairCarryRise ? 8 : 16), state.IdealInterSegmentTailAngle, "native expiry changes ideal tail angle");
            AssertEqual(240, state.TailExtensionSpeed, "native expiry restores tail extension speed");
        }
        Console.WriteLine("Ridley grab entry: native immediate carry, velocity, countdown and paired control lock/release pass.");
    }

    /// <summary>Checks initial Ridley room map state at the fight's entry boundary.</summary>
    internal static void VerifyRidleyMapInitialization()
    {
        foreach (bool defeated in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.Hud.EnableMinimapAfterDoorEntry();
            runtime.System.LoadExploredMapBytes(new byte[Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea]);
            if (defeated) runtime.System.SetBossBits(AreaId.Norfair, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
            var room = runtime.Room.LoadedRoom;
            AssertEqual(!defeated, runtime.Hud.MinimapDisabled, "only live Ridley disables the minimap");
            for (int row = 0; row < 2; row++)
                AssertEqual(!defeated, runtime.System.IsMapTileExplored(room.AreaIndex, room.MapPosition + new Offset16<MapTiles>(0, checked((short)(row + 1)))),
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

    /// <summary>Checks Samus's retained horizontal momentum across the recorded Ridley movement transition.</summary>
    internal static void VerifyRetainedHorizontalSpeed()
    {
        var room = CreateRoom(new(16, 16), new ushort[256], new byte[256]);
        foreach (int branch in new[] { 0, 1, 2, 3 })
        {
            var samus = new SamusState
            {
                Pose = branch switch
                {
                    0 => SamusPoseId.NormalJumpForwardRightPose,
                    2 => SamusPoseId.SpringBallJumpRightPose,
                    _ => SamusPoseId.FallingAimDownRightPose,
                },
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
            samus.HorizontalSpeed.CalculateTotalSpeed(0x00054321);
            samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(branch == 3 ? (ushort)0 : (ushort)5);
            if (branch == 0) SamusAerialMovement.StepNormalJump(room, samus, 0, 0);
            else if (branch == 1) SamusAerialMovement.StepFalling(room, samus, 0, 0);
            else if (branch == 2) SamusMorphBallMovement.StepSpringBallInAir(room, samus, 0, 0);
            else
            {
                samus.Grapple.ReleasedMovementActive = true;
                SamusAerialMovement.StepReleasedFromGrapple(room, samus, 0, 0);
            }
            AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, $"branch {branch} clears base speed");
            AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Fraction, $"branch {branch} clears base fraction");
            AssertEqual((ushort)128, samus.Position.X.Whole, $"branch {branch} does not move horizontally");
            AssertEqual(5, samus.HorizontalSpeed.TotalSpeed.Whole, $"branch {branch} retains total speed");
            AssertEqual(0x4321, samus.HorizontalSpeed.TotalSpeed.Fraction, $"branch {branch} retains total fraction");
        }
        Console.WriteLine("No-direction normal jump, fall, Spring Ball jump and grapple release retain total speed while clearing base motion.");
    }

    /// <summary>Checks RNG consumption while pause enters a room HDMA callback already in progress.</summary>
    internal static void VerifyPauseDispatcherRandom()
    {
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        // The recorded pause enters an already-running room HDMA callback.
        runtime.RoomLayer3Fx.AdvanceHdmaSharedState(runtime.System, false);
        foreach (var state in new[] { SuperMetroidGameState.Pausing, SuperMetroidGameState.PausedA,
            SuperMetroidGameState.PausedB, SuperMetroidGameState.UnpausingA, SuperMetroidGameState.UnpausingB,
            SuperMetroidGameState.PausingDarkening, SuperMetroidGameState.Unpausing })
        {
            runtime.System.SetRandomNumber(0x117d);
            game.GameState = state;
            game.AdvanceMenuRandom();
            bool gameplayOwnsRandom = state is SuperMetroidGameState.PausingDarkening or SuperMetroidGameState.Unpausing;
            ushort expected = gameplayOwnsRandom ? (ushort)0x117d :
                state == SuperMetroidGameState.Pausing ? (ushort)0x7266 : (ushort)0x5882;
            AssertEqual(expected, runtime.System.RandomNumber,
                "pause entry runs lava HDMA before RNG; paused dispatches advance once; gameplay fades retain runtime ownership");
        }
        Console.WriteLine("Pause RNG: all five pause-only dispatchers advance once; gameplay fades avoid duplicate advances.");
    }

    /// <summary>Checks spin-pose fallback selection using the preceding pose history.</summary>
    internal static void VerifySpinFallbackHistory()
    {
        foreach (SamusPoseId pose in new[] { SamusPoseId.SpinJumpRightPose, SamusPoseId.SpinJumpLeftPose,
            SamusPoseId.SpaceJumpRightPose, SamusPoseId.SpaceJumpLeftPose,
            SamusPoseId.ScrewAttackRightPose, SamusPoseId.ScrewAttackLeftPose })
        {
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.Pose = pose; samus.Position = samus.Position.WithWhole(new(512, 400));
            samus.EquippedItems = (SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.ScrewAttack);
            SamusPoseMetadata.RefreshCollisionRadii(samus); SamusPresentation.InitializeAnimation(samus);
            samus.PoseHistory.PreviousPose = pose;
            ushort direction = SamusPoseMetadata.ReadPoseXDirection(samus);
            ushort spinMovement = SnesLittleEndian.Word(unchecked((byte)direction), (byte)SamusMovementType.SpinJumping);
            samus.PoseHistory.PreviousDirectionAndMovement = spinMovement;
            samus.PoseHistory.LastDifferentPose = SamusPoseId.RunningAimUpRightPose;
            samus.PoseHistory.LastDifferentDirectionAndMovement = SnesLittleEndian.Word(unchecked((byte)direction), (byte)SamusMovementType.Running);
            runtime.StepFrame(0);
            AssertEqual(pose, samus.PoseHistory.PreviousPose, "fallback retains spin pose");
            AssertEqual(pose, samus.PoseHistory.LastDifferentPose, "fallback commits same-pose history");
            AssertEqual(spinMovement, samus.PoseHistory.LastDifferentDirectionAndMovement, "fallback publishes older spin movement");
            AssertTrue(samus.PoseHistory.AllowsWallJumpProbe, "next update admits native wall observation");
        }
        Console.WriteLine("Spin fallback history: ordinary, Space Jump and Screw Attack both facings retain pose and admit wall probe.");
    }

    /// <summary>Checks the camera checkpoint selected during Samus's morph transition.</summary>
    internal static void VerifyMorphCameraCheckpoint()
    {
        var level = CreateRoom(new(16, 32), new ushort[16 * 32], new byte[16 * 32]);
        foreach (bool left in new[] { false, true })
        {
            var samus = new SamusState
            {
                Pose = left ? SamusPoseId.FallingAimDownLeftPose : SamusPoseId.FallingAimDownRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 402)),
                EquippedItems = SamusEquipmentFlags.MorphBall,
            };
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x97ff);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var previous = FixedPosition<RoomPixels>.FromWords(128, 0, 401, 0xb7ff);
            AssertTrue(SamusPosturePoseTransitions.TryApplyMorphTransition(samus, level,
                left ? SamusPoseId.MorphingTransitionLeftPose : SamusPoseId.MorphingTransitionRightPose, 0),
                "source6289 airborne morph accepted");
            AssertEqual((ushort)411, samus.Position.Y.Whole, "native morph center alignment");
            AssertEqual((ushort)0x97ff, samus.Kinematics.Position.Y.Fraction, "morph preserves current fraction");
            AssertEqual(previous.WithY(previous.Y.WithWhole(411)), samus.ApplyPreviousPositionWrites(previous),
                "command seven replaces previous whole Y and retains previous fraction");
            AssertEqual(previous, samus.ApplyPreviousPositionWrites(previous), "checkpoint consumed once");

            samus.Pose = left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            AssertTrue(SamusPosturePoseTransitions.TryApplyMorphTransition(samus, level,
                left ? SamusPoseId.UnmorphingTransitionLeftPose : SamusPoseId.UnmorphingTransitionRightPose, 0),
                "unmorph command seven accepted");
            AssertEqual(previous.WithY(previous.Y.WithWhole(samus.Position.Y.Whole)), samus.ApplyPreviousPositionWrites(previous),
                "zero alignment entry still replaces previous whole Y");
        }
        Console.WriteLine("Morph camera checkpoint: both facings, alignment, fractions, one-time consumption and unmorph pass.");
    }

    /// <summary>Checks the aim-up landing pose's animation timing at the recorded boundary.</summary>
    internal static void VerifyAimUpLandingAnimation()
    {
        var level = CreateRoom(new(16, 32), new ushort[16 * 32], new byte[16 * 32]);
        foreach (SamusPoseId pose in new[] { SamusPoseId.FallingAimUpRightPose, SamusPoseId.FallingAimUpLeftPose })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 200)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            AssertTrue(SamusPoseTransitions.TryApplyAerialLanding(samus, level, false, 0, 0), "aim-up landing fits");
            AssertEqual(1, samus.AnimationFrame, "native standing initializer skips raised-gun frame");
            AssertEqual((ushort)2, samus.AnimationFrameTimer.RawValue, "native source3604 landing delay");
            samus.Pose = pose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPoseTransitions.ApplyAerialLanding(samus, false);
            AssertEqual(1, samus.AnimationFrame, "roomless landing shares native frame skip");
            SamusPoseId standing = pose == SamusPoseId.FallingAimUpRightPose
                ? SamusPoseId.StandingAimUpRightPose : SamusPoseId.StandingAimUpLeftPose;
            samus.PendingTransitionalPose = standing;
            AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus), "landing completes through animation command");
            AssertEqual(1, samus.AnimationFrame, "native landing completion retains raised-gun frame");
            AssertEqual((ushort)16, samus.AnimationFrameTimer.RawValue, "native source3606 standing delay");
        }
        Console.WriteLine("Aim-up landing: both facing directions preserve raised-gun animation frame and native delay.");
    }

    /// <summary>Checks Ridley's fireball collision against occupied square-slope quadrants.</summary>
    internal static void VerifyRidleyFireballSquareSlope()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var projectile = runtime.Enemies.Projectiles.EnemyProjectiles[16];
        projectile.Position = new(new(0x49, 0x8c00), new(0x54, 0xd900));
        projectile.Velocity = new(new SnesSignedEightEight(0xfb64), new SnesSignedEightEight(0xfe1b));
        projectile.Radius = new(6, 6);
        AssertTrue(!EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Horizontal), "source1248 fireball crosses empty square-slope quadrant horizontally");
        AssertTrue(!EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Vertical), "source1248 fireball clears terrain vertically");
        AssertEqual((ushort)0x44, projectile.Position.X.Whole, "native source1249 fireball X");
        AssertEqual((ushort)0xf000, projectile.Position.X.Fraction, "native source1249 fireball X fraction");
        AssertEqual((ushort)0x52, projectile.Position.Y.Whole, "native source1249 fireball Y");
        AssertEqual((ushort)0xf400, projectile.Position.Y.Fraction, "native source1249 fireball Y fraction");
        projectile.Position = projectile.Position.WithWholeX(60).WithFractionX(0).WithWholeY(84); projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0xff00));
        AssertTrue(EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Horizontal), "occupied top-left quarter stops horizontal motion");
        AssertEqual((ushort)62, projectile.Position.X.Whole, "native square reaction snaps to eight-pixel edge even when initially embedded");
        projectile.Position = projectile.Position.WithWholeX(52).WithWholeY(94).WithFractionY(0); projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0xff00));
        AssertTrue(EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Vertical), "occupied top-left quarter stops vertical motion");
        AssertEqual((ushort)94, projectile.Position.Y.Whole, "native square reaction snaps below occupied quarter");
        Console.WriteLine("Ridley fireball square slope: native source1248 trajectory and occupied horizontal/vertical quadrants pass.");
    }

    /// <summary>Checks Samus damage and projectile deletion when a Ridley fireball hits.</summary>
    internal static void VerifyRidleyFireballDamage()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        int slotIndex = 17;
        foreach (var sample in new[] { new VerifyRidleyFireballDamageCase(AreaId.Ceres, 3), new VerifyRidleyFireballDamageCase(AreaId.Norfair, 60), new VerifyRidleyFireballDamageCase(AreaId.Tourian, 80) })
        {
            var samus = runtime.Player.IntroducedSamus;
            samus.LiquidPhysics.RoomIdentity = new RoomIdentity(sample.First, 0);
            runtime.Enemies.RidleyProjectiles.SpawnRidleyFireball(runtime.Enemies.SlotTable.Slots[0], afterburns: 0);
            var fireball = runtime.Enemies.Projectiles.EnemyProjectiles[slotIndex--];
            AssertEqual(sample.Second, fireball.Damage, "native area initializer overrides fireball damage");
            runtime.Enemies.RidleyProjectiles.SpawnDirectionalAfterburn(fireball, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight, new EightEightVector<RoomPixels>(0x0e00, 0));
            AssertEqual(sample.Second, runtime.Enemies.Projectiles.EnemyProjectiles[slotIndex--].Damage, "directional afterburn runs the same area initializer");
            samus.Position = samus.Position.WithWhole(fireball.Position.Whole);
            samus.Health = 700; samus.EquippedItems = SamusEquipmentFlags.GravitySuit;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            runtime.Enemies.Projectiles.ResolveEnemyProjectileSamusCollision(fireball, samus);
            AssertEqual((ushort)(700 - sample.Second / 4), samus.Health, "area damage passes through native Gravity reduction");
        }
        Console.WriteLine("Ridley fireball damage: default/Norfair/Tourian initializers, directional afterburn and suit reduction pass.");
    }

    /// <summary>Checks the tail's impact response against the recorded contact geometry.</summary>
    internal static void VerifyRidleyTailImpact()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var state = runtime.Enemies.RidleyEncounter.Ridley;
        var body = runtime.Enemies.SlotTable.Slots[0];
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = RidleyAiFunction.NorfairFireballAttack;
        state.TailSegments[6].Position = new Position16<RoomPixels>(4, 118);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGroundAttack(body, state, null, runtime.Room.LevelData);
        var dust = runtime.Enemies.Projectiles.EnemyProjectiles[17];
        AssertEqual(RoomEnemyProjectileKind.MiscDustExplosion, dust.Kind, "native tail impact allocates highest free slot");
        AssertEqual((ushort)4, dust.Position.X.Whole, "native tail impact dust X");
        AssertEqual((ushort)130, dust.Position.Y.Whole, "native tail impact dust Y includes twelve-pixel offset");
        AssertTrue(dust.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.SmallDustCloud }, "native impact selects variant nine");
        AssertTrue(runtime.Enemies.Audio.SoundRequests.Any(request => request.SoundEffect == Library2Sound.Quake && request.MaximumQueued.Requests == 6), "native impact queues the quake sound");
        Console.WriteLine("Ridley tail impact: native dust slot, position, instruction variant and sound pass.");
    }

    /// <summary>Checks that Ridley enters his screen gate only after the prior Y position becomes visible.</summary>
    internal static void VerifyRidleyScreenGate()
    {
        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(64, 253))};
        var state = new RidleyEnemyState { FightMode = RidleyFightMode.Fighting, IntangibilityTimer = WrappingCountdown.Start(3) };
        // Original source 3062/3063: pre-movement Y253 is outside camera Y287.
        RidleyInteractions.UpdateNorfairRidleyIntangibility(body, state, new Position16<RoomPixels>(0, 287));
        AssertTrue(body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "native above-camera Ridley disables collision");
        AssertEqual((ushort)3, state.IntangibilityTimer.RawValue, "offscreen return preserves release timer");
        // At the next AI call Y257 is inside. EnemyMain already tested the old
        // collision bit; the cleared gate is consumed by the following update.
        body.Position = body.Position.WithWholeY(257);
        RidleyInteractions.UpdateNorfairRidleyIntangibility(body, state, new Position16<RoomPixels>(0, 287));
        AssertTrue(!body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "native reentry clears collision gate");
        AssertEqual((ushort)2, state.IntangibilityTimer.RawValue, "onscreen AI resumes release timer");
        AssertTrue(RidleyCollisionDefinitions.IsOutsideInteractionWindow(new(64, 254), new(0, 287)), "one pixel above native window");
        AssertTrue(!RidleyCollisionDefinitions.IsOutsideInteractionWindow(new(64, 255), new(0, 287)), "native upper edge is admitted");
        AssertTrue(RidleyCollisionDefinitions.IsOutsideInteractionWindow(new(64, 543), new(0, 287)), "native lower edge is excluded");
        AssertTrue(!RidleyCollisionDefinitions.IsOutsideInteractionWindow(new(64, 542), new(0, 287)), "last lower pixel is admitted");
        Console.WriteLine("Ridley screen gate: recorded reentry, signed bounds and timer ownership pass.");
    }

    /// <summary>Checks rolling Left release and stationary Spring Ball movement command retention.</summary>
    internal static void VerifySpringBallRelease()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        runtime.InitializeDebugGroundedSamus(79, 425, 16);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseId.SpringBallMovingLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus); SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 5, 3);
        samus.Position = new(new(79, 0x8000), new(425, ushort.MaxValue));
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(3); samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithFraction(0xc000);
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        runtime.Controller1.Latch(SnesButton.Left);
        // Original movie source 1616 -> 1617: release Left while rolling.
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.Position.X.RawValue, "spring release retains native final displacement");
        AssertEqual(SamusPoseId.SpringBallGroundLeftPose, samus.Pose, "spring release selects stationary pose immediately");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "spring release clears base momentum after movement");
        AssertEqual(SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode, "spring release clears acceleration mode");
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.Position.X.RawValue, "released spring ball stays stopped next update");
        // Original source 1983: stationary Spring Ball keeps command six even
        // while the hurt mover calculates a fresh fractional base speed.
        samus.Pose = SamusPoseId.SpringBallGroundRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus); SamusPresentation.InitializeAnimation(samus);
        samus.Position = new(new(202, 0x8000), new(425, ushort.MaxValue));
        samus.KnockbackActive = true; samus.KnockbackDirection = SamusKnockbackDirection.UpRight;
        samus.KnockbackXDirection = KnockbackHorizontalDirection.Right; samus.KnockbackTimer = WrappingCountdown.Start(4);
        samus.InvincibilityTimer = WrappingCountdown.Start(95);
        samus.Kinematics.YDirection = SamusVerticalDirection.Up; samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(5); samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithFraction(0);
        runtime.StepFrame(0);
        AssertEqual(0x00cb4000u, samus.Kinematics.Position.X.RawValue, "stationary spring hurt frame preserves native displacement");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "stationary spring fallback clears hurt mover base speed");
        AssertEqual(SamusPoseId.SpringBallGroundRightPose, samus.Pose, "stationary spring hurt fallback keeps pose");
        Console.WriteLine("Spring Ball release: native movie displacement, pose and immediate momentum reset pass.");
    }

    /// <summary>Checks the tail segment offsets used to draw and collide with Ridley's animated body.</summary>
    internal static void VerifyRidleyTailOffsets()
    {
        var state = new RidleyEnemyState
        {
            TailSegments = Enumerable.Range(0, 7).Select(_ => new RidleyTailSegment()).ToArray(),
            IdealInterSegmentTailAngle = 16, TailAngleDelta = 2,
            TailMinimumClockwiseAngle = SnesAngle.FromRaw(0x3fc0), TailMaximumCounterClockwiseAngle = SnesAngle.FromRaw(0x4010),
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
        };
        var segment = state.TailSegments[0];
        segment.Active = true; segment.StaggerAngle = 2;
        segment.Offset = new Offset16<RoomPixels>(7, 11);
        RidleyTail.TickRidleyTailSegment(state, 0);
        AssertEqual(4, segment.StaggerAngle, "native stagger advances before returning");
        AssertEqual((short)7, segment.Offset.X, "stagger retains X offset");
        AssertEqual((short)11, segment.Offset.Y, "stagger retains Y offset");
        segment.StaggerAngle = null; segment.Angle = 0x4000;
        segment.Clockwise = true; segment.Distance = 0x0200;
        RidleyTail.TickRidleyTailSegment(state, 0);
        AssertEqual(0x3ffe, segment.Angle, "native clockwise comparison decrements then restores one before storing");
        AssertEqual((short)-1, segment.Offset.X, "signed Mode 7 multiplication floors negative fraction");
        AssertEqual((short)1, segment.Offset.Y, "signed Mode 7 cosine product");
        state.ClockwiseWhipTarget = 0x4000;
        segment.Angle = 0x4000; segment.Offset = new Offset16<RoomPixels>(7, 11);
        RidleyTail.TickRidleyTailSegment(state, 0);
        AssertTrue(!segment.Active, "root stops at whip target");
        AssertEqual((short)7, segment.Offset.X, "stop retains X offset");
        AssertEqual((short)11, segment.Offset.Y, "stop retains Y offset");
        var child = state.TailSegments[1];
        segment.Active = true;
        child.Active = true; child.StaggerAngle = null;
        child.Angle = 0x4000; child.Clockwise = true; child.Distance = 0x0800;
        RidleyTail.TickRidleyTailSegment(state, 1);
        AssertTrue(child.Active, "moving predecessor prevents child deactivation");
        AssertEqual(0x3fc0, child.Angle, "blocked child clamps to native clockwise limit");
        foreach (var part in state.TailSegments)
        {
            part.Distance = 0x1200; part.TargetDistance = 0x0c00;
            part.Angle = 0x3ff8; part.Offset = new Offset16<RoomPixels>(-3, 13);
        }
        RidleyTail.MirrorRidleyTail(state);
        ushort[] nativeRest = [0x0200, 0x0800, 0x0800, 0x0800, 0x0800, 0x0800, 0x0500];
        for (int i = 0; i < state.TailSegments.Length; i++)
        {
            var part = state.TailSegments[i];
            AssertEqual(nativeRest[i], part.Distance, "native flip restores each authored rest distance");
            AssertEqual(0x4008, part.Angle, "native flip reflects angle");
            AssertEqual(0x0c00, part.TargetDistance, "native flip preserves extension target");
            AssertEqual((short)-3, part.Offset.X, "native flip preserves current X offset");
            AssertEqual((short)13, part.Offset.Y, "native flip preserves current Y offset");
        }
        Console.WriteLine("Ridley tail offsets: stagger/stop retention, clockwise arithmetic, signed multiplication and predecessor gate pass.");
    }

    /// <summary>Checks Ridley's facing selection at the Samus/center alignment boundary.</summary>
    internal static void VerifyRidleyCenterFacing()
    {
        RidleyFullMovieCase[] cases = [
            new RidleyFullMovieCase(0x004a, RidleyFacing.Right, false), // Original movie frame 734: already facing inward.
            new RidleyFullMovieCase(0x00c0, RidleyFacing.Right, true), new RidleyFullMovieCase(0x004a, RidleyFacing.Left, true), new RidleyFullMovieCase(0x00c0, RidleyFacing.Left, false),
            new RidleyFullMovieCase(0x004a, RidleyFacing.Turning, false), new RidleyFullMovieCase(0x007f, RidleyFacing.Right, false), new RidleyFullMovieCase(0x0080, RidleyFacing.Right, true),
            new RidleyFullMovieCase(0x014a, RidleyFacing.Right, false), // Native tests the low position byte, not full X >= 128.
        ];
        var pointers = new RidleyNativeInstructionPointers(CartridgeImportSource.Require(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        foreach (var sample in cases)
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(sample.X, 0)),
                InstructionTimer = WrappingCountdown.Start(7), Timer = WrappingCountdown.Start(9) };
            var state = new RidleyEnemyState { FacingDirection = sample.Facing };
            state.BodyProgram = pointers.BodyCursorAt(NativeSnapshotMemory.RidleyRightFlyingSleep);
            NorfairRidleyCombat.SelectNorfairRidleyFacingInstruction(slot, state);
            ushort expected = !sample.Turn ? NativeSnapshotMemory.RidleyRightFlyingSleep
                : sample.Facing == RidleyFacing.Left ? RidleyNativeInstructionPointers.BodyEntry(RidleyBodyProgram.TurnFromLeftToRight)
                : RidleyNativeInstructionPointers.BodyEntry(RidleyBodyProgram.TurnFromRightToLeft);
            AssertEqual(expected, pointers.Body(state), $"native center-facing instruction at {sample.X:X4}, facing {sample.Facing}");
            AssertEqual(sample.Turn ? (ushort)2 : (ushort)7, slot.InstructionTimer.RawValue, "turn timer changes only when native condition is met");
            AssertEqual(sample.Turn ? (ushort)0 : (ushort)9, slot.Timer.RawValue, "loop counter is preserved when no turn is needed");
        }
        var hoverState = new RidleyEnemyState { FunctionTimer = WrappingCountdown.Start(0), Function = RidleyAiFunction.NorfairHover };
        NorfairRidleyCombat.TickNorfairRidleyHover(new RoomEnemySlot(0, EnemySlotFixtures.Registry), hoverState);
        AssertEqual(ushort.MaxValue, hoverState.FunctionTimer.RawValue, "native hover decrements zero before selecting next attack");
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, hoverState.Function, "negative hover timer exits before movement");
        Console.WriteLine("Ridley center facing: movie trigger, both sides/directions, mid-turn and native low-byte boundary agree.");
    }

    /// <summary>Checks pause page and HDMA timing across Ridley's fight.</summary>
    internal static void VerifyRidleyPausePageTiming()
    {
        var samus = new SamusState { MaxReserveEnergy = 300, ReserveEnergy = 300, ReserveTankMode = ReserveTankMode.Auto };
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = RetailPresentationFixture(), SpriteSheets = MenuSheetFixtures.For(RetailPresentationFixture()) });
        foreach (PauseScreenMode source in Enum.GetValues<PauseScreenMode>())
        {
            PauseScreenMode destination = source == PauseScreenMode.Map ? PauseScreenMode.Equipment : PauseScreenMode.Map;
            menu.Step((source == PauseScreenMode.Map ? SnesButton.R : SnesButton.L), 0);
            AssertEqual(source, menu.ScreenMode, "page-switch request retains source page");
            for (int update = 1; update <= 15; update++)
            {
                menu.Step(0, 0);
                AssertEqual(source, menu.ScreenMode, "source page remains through fade-out");
                AssertEqual(15 - update, menu.TransitionBrightness, "native zero-delay fade-out brightness");
            }
            AssertEqual(source == PauseScreenMode.Map ? PauseMenuTransition.MapToEquipmentLoad : PauseMenuTransition.EquipmentToMapLoad,
                menu.Transition, "black frame returns into separate load dispatcher");
            menu.Step(0, 0);
            AssertEqual(destination, menu.ScreenMode, "separate load dispatch installs destination");
            AssertEqual(0, menu.TransitionBrightness, "load dispatch remains black");
            for (int update = 1; update <= 30; update++)
            {
                menu.Step(0, 0);
                AssertEqual(update / 2, menu.TransitionBrightness, "native delay-one fade-in brightness");
                AssertEqual(update == 30, menu.Transition == PauseMenuTransition.None,
                    "input returns only after the thirtieth fade-in update");
            }
        }
        Console.WriteLine("Both pause-page directions preserve 15 fade-out, one load and 30 fade-in updates.");
    }

    /// <summary>Checks the selected Ridley palette's CGRAM destination and color words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyRidleyPaletteSelection()
    {
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPresentation.InitializeAnimation(samus);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.Position = samus.Position.WithWhole(new(512, 400));
        samus.EquippedItems = SamusEquipmentFlags.VariaSuit;
        samus.SelectedHudItem = HudItem.Missile; samus.SuperMissiles = 5;
        samus.Presentation.LoadSuitPalette(samus, runtime.Player.Colours);
        Bgr555[] expected = runtime.Cgram.Colors.Slice(192, 16).ToArray();
        for (int color = 192; color < 208; color++) runtime.Cgram.AsPaletteColors().SetColor(color, Bgr555.FromWord(0x1234));
        runtime.StepFrame(SnesButton.Select);
        AssertEqual(HudItem.SuperMissile, samus.SelectedHudItem, "selection enters Super Missiles without charge");
        AssertTrue(runtime.Cgram.Colors.Slice(192, 16).SequenceEqual(expected),
            "HUD handler restores all suit colors even when charge was zero");
        Console.WriteLine("HUD selection restores the complete suit palette without an active charge.");
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        game.GameState = SuperMetroidGameState.Pausing;
        game.Step(0);
        var menu = game.Session.ScreenAs<PauseMenuState>();
        var menuPalette = (menu ?? throw new InvalidOperationException("Verification requires menu.")).screen.Palette;
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
        game.GameState = SuperMetroidGameState.UnpausingA;
        game.Session.BeginPauseFade(15);
        fadeFrames = 0;
        while (game.GameState == SuperMetroidGameState.UnpausingA && fadeFrames++ < 32)
        {
            game.Step(0);
            AssertTrue(menuPalette.Colors.SequenceEqual(animated), "unpause fade-out retains the last palette phase");
        }
        AssertEqual(SuperMetroidGameState.UnpausingB, game.GameState, "unpause fade-out completes");
        Console.WriteLine("Pause palette advances only in stable pause, remaining latched through both outer fades.");
    }

    /// <summary>Checks door-entry setup, queued HDMA and palette state at the Ridley room boundary.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyRidleyDoorEntry()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LNFarming);
        var level = runtime.Room.LoadedLevelData;
        bool found = false;
        for (int y = 0; y < level.Size.Y && !found; y++)
        for (int x = 0; x < level.Size.X && !found; x++)
        {
            var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(block.Bts.Value, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), false);
            if (door.Door?.DestinationRoom != RoomHeaderDefinitions.Ridley) continue;
            level.ResolveDoorCollision(block.Bts.Value, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), true);
            found = true;
        }
        AssertTrue(found, "native Ridley entry door");
        // Original movie: accepted main-loop samples 155, 156, 157. The source
        // acid callback was installed long before the recorded door collision.
        runtime.RoomLayer3Fx.AdvanceHdmaSharedState(runtime.System, false);
        runtime.System.SetRandomNumber(0xd562);
        runtime.Clock.Import(runtime.Clock.Counter8, 0xa4df);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Position = new(new(20, 0x2000), new(111, 0x5bff));
        uint xFixed = samus.Kinematics.Position.X.RawValue, yFixed = samus.Kinematics.Position.Y.RawValue;
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.GameState = SuperMetroidGameState.HitDoorBlock;
        game.Step(0);
        AssertEqual(0xef3a, runtime.System.RandomNumber, "native entry HDMA swap then RNG");
        AssertEqual(0xa4e0, runtime.Clock.Counter, "entry accepts exactly one NMI");
        AssertEqual(xFixed, samus.Kinematics.Position.X.RawValue, "entry keeps Samus X fixed");
        AssertEqual(yFixed, samus.Kinematics.Position.Y.RawValue, "entry keeps Samus Y fixed");
        game.Step(0);
        AssertEqual(0x27bc, runtime.System.RandomNumber, "native sound-wait HDMA swap then RNG");
        AssertEqual(0xa4e1, runtime.Clock.Counter, "sound wait accepts exactly one NMI");
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification, "source fade begins after sound drain");
        var actor = runtime.Enemies.SlotTable.Slots[6];
        AssertEqual(EnemyDefinitionId.Zebbo, actor.DefinitionId, "native source-room pipe bug");
        var pipeBugPointers = new PipeBugNativeInstructionPointers(CartridgeImportSource.Require(bus));
        PipeBugEnemyState pipeBug = runtime.Enemies.PipeBugStates[6] ?? throw new InvalidOperationException("The source-room pipe bug has no state.");
        pipeBug.Program = pipeBugPointers.CursorAt(NativeSnapshotMemory.PipeBugBeforeFadeInstruction);
        actor.InstructionTimer = WrappingCountdown.Start(1);
        actor.SpritemapPointer = NativeSnapshotMemory.PipeBugBeforeFadeSpritemap;
        game.Step(0);
        AssertEqual(0xadd4, runtime.System.RandomNumber, "first source fade advances native HDMA/RNG");
        AssertEqual(0xa4e2, runtime.Clock.Counter, "fade accepts exactly one NMI per update");
        AssertEqual(NativeSnapshotMemory.PipeBugAfterFadeInstruction, pipeBugPointers.Pointer(pipeBug.Program), "fade advances the native enemy instruction");
        AssertEqual(NativeSnapshotMemory.PipeBugAfterFadeSpritemap, actor.SpritemapPointer, "fade changes to the native enemy sprite");
        AssertEqual((ushort)2, actor.InstructionTimer.RawValue, "fade installs the native visual duration");
        int fadeSteps = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FadeOutSourcePalette && fadeSteps++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.LoadDoorHeader, game.DoorTransitionPhaseForVerification, "source palette fade finishes");
        AssertEqual(0xe19e, runtime.System.RandomNumber, "native update 172 fade endpoint RNG");
        // Original native input-boundary records 173..178. The first dispatch
        // still runs source HDMA; LoadDoorHeader disables it for the following ones.
        ushort[] nativeLoadingRandom = [0x1b76, 0x8a5f, 0xb4ec, 0x89ad, 0xb172, 0x784b];
        for (int index = 0; index < nativeLoadingRandom.Length; index++)
        {
            game.Step(0);
            AssertEqual(nativeLoadingRandom[index], runtime.System.RandomNumber, $"native loading RNG update {173 + index}");
            AssertEqual((ushort)(0xa4f1 + index), runtime.Clock.Counter, "loading accepts one NMI per update");
            if (index == 5)
            {
                AssertEqual(0x010e9000u, samus.Kinematics.Position.X.RawValue, "native placement rebases and advances Samus while loading tiles");
                AssertEqual((ushort)0x00f8, runtime.Room.LoadedCamera.PixelPosition.X, "first loading IRQ moves camera four pixels");
            }
            if (index == 4)
            {
                AssertEqual(0x00135800u, samus.Kinematics.Position.X.RawValue, "native scrolling setup moves Samus before destination placement");
                AssertEqual(yFixed, samus.Kinematics.Position.Y.RawValue, "left scrolling setup preserves perpendicular coordinate");
                AssertEqual((ushort)0x00fc, runtime.Room.LoadedCamera.PixelPosition.X, "native setup camera origin");
            }
        }
        game.Step(0); // The atomic destination loader must use the pre-setup source.
        AssertEqual(DoorTransitionPhase.WaitForDoorOpeningScroll, game.DoorTransitionPhaseForVerification, "loaded destination owns the opening trajectory");
        AssertEqual(0x010dc800u, samus.Kinematics.Position.X.RawValue, "destination load carries both loading IRQ steps without restarting the scroll");
        AssertEqual(0, runtime.Clock.Counter8, "door end-drawing IRQ clears the adjacent byte counter");
        ushort[] beamPalette = ReadRetailBeamPalette(bus, samus.EquippedBeams.ToNativeWord());
        for (int color = 0; color < 16; color++)
            AssertEqual(beamPalette[color], runtime.Cgram.Colors[224 + color], "beam palette is live during door scrolling");
        AssertEqual(0, runtime.Cgram.Colors[196], "visor remains faded before scroll completes");
        int scrollCalls = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForDoorOpeningScroll && scrollCalls < 64)
        {
            game.Step(0);
            AssertEqual(0, runtime.Clock.Counter8, "every moving door IRQ renews the byte-counter reset");
            scrollCalls++;
        }
        AssertEqual(61, scrollCalls, "left trajectory completes on its 61st remaining IRQ call");
        AssertEqual(DoorTransitionPhase.FinishDoorLoading, game.DoorTransitionPhaseForVerification, "post-scroll NMI remains inside the loading coroutine");
        AssertEqual(0x3be0, runtime.Cgram.Colors[196], "final scrolling update publishes native visor green before PLMs");
        AssertEqual(0x00de2000u, samus.Kinematics.Position.X.RawValue, "native source frame 276 scrolling endpoint");
        game.Step(0);
        AssertEqual(DoorTransitionPhase.HandleAnimatedTiles, game.DoorTransitionPhaseForVerification, "loading alignment precedes music wait");
        AssertEqual(0x5a88, runtime.System.RandomNumber, "loading continuation does not advance outer RNG");
        AssertEqual(0x00d82000u, samus.Kinematics.Position.X.RawValue, "native final doorway alignment preserves original subposition");
        AssertEqual(yFixed, samus.Kinematics.Position.Y.RawValue, "left door endpoint preserves perpendicular coordinate");
        game.Step(0);
        AssertEqual(0xc5b9, runtime.System.RandomNumber, "native animated-tile outer dispatch RNG");
        game.Step(0);
        AssertEqual(0xa1ea, runtime.System.RandomNumber, "native music-wait outer dispatch RNG");
        int musicWaits = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForMusicQueue && musicWaits++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.HandleTransition, game.DoorTransitionPhaseForVerification, "native music queue reaches final door dispatch");
        int nmiBeforeNudge = runtime.Clock.Counter;
        int samusAnimation = samus.AnimationFrame;
        var ridley = runtime.Enemies.SlotTable.Slots[0];
        var pointers = new RidleyNativeInstructionPointers(CartridgeImportSource.Require(bus));
        ushort bodyInstruction = pointers.Enemy(runtime.Enemies, ridley);
        game.Step(0);
        AssertEqual(DoorTransitionPhase.BuildDestinationOam, game.DoorTransitionPhaseForVerification, "final nudge returns before first fade");
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 1)), runtime.Clock.Counter, "final transition accepts exactly one NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "final transition does not animate Samus");
        AssertEqual(bodyInstruction, pointers.Enemy(runtime.Enemies, ridley), "final transition does not advance destination enemy instructions");
        AssertEqual(0, runtime.Clock.Counter8, "final door scanout resets the byte before IRQ ownership changes");
        game.Step(0);
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 2)), runtime.Clock.Counter, "first fade accepts exactly one additional NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "destination fade does not animate Samus");
        AssertEqual(1, runtime.Clock.Counter8, "first destination drawing frame resumes the byte counter");
        AssertEqual(NativeSnapshotMemory.RidleyFirstFadeInstruction, pointers.Enemy(runtime.Enemies, ridley), "first fade runs native Ridley instruction list");
        AssertEqual(NativeSnapshotMemory.RidleyFirstFadeSpritemap, ridley.SpritemapPointer, "first fade publishes native Ridley sprite");
        AssertEqual((ushort)12, ridley.InstructionTimer.RawValue, "first native fade visual duration");
        AssertEqual(RidleyAiFunction.WaitForDoorTransition, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).Function, "native Ridley AI remains gated during fade visuals");
        AssertEqual((ushort)0, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).FunctionTimer.RawValue, "fade does not consume the reveal countdown");
        game.Step(0);
        AssertEqual((ushort)11, ridley.InstructionTimer.RawValue, "later fade updates continue enemy animation");
        int remainingFade = 0;
        while (game.GameState == SuperMetroidGameState.LoadingNextRoomB && remainingFade++ < 32)
        {
            game.Step(0);
            AssertEqual(RidleyAiFunction.WaitForDoorTransition, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).Function, "reveal timer remains gated through the final fade dispatch");
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "fade releases ordinary gameplay");
        AssertTrue(!runtime.Enemies.RoomStatus.EnemyDoorTransitionActive, "completed fade clears enemy gate");
        game.Step(0);
        AssertEqual(RidleyAiFunction.InitialDelay, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).Function, "first gameplay update begins reveal");
        AssertEqual((ushort)169, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).FunctionTimer.RawValue, "native first gameplay update consumes one of 170 reveal ticks");
        Console.WriteLine("Ridley door entry/loading/fade: native positions, RNG, one NMI per dispatch, stationary Samus animation and continuing enemy visuals agree.");
    }

    /// <summary>The 16 native beam colors $90:C3C9 selects for <paramref name="equippedBeams"/>, BGR555 only.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the equipped-beam palette pointer table and palettes")]
    private static ushort[] ReadRetailBeamPalette(ISnesAddressSpace bus, ushort equippedBeams)
    {
        int pointerAddress = NativeSnapshotMemory.BeamPalettePointers + (equippedBeams & 0x0fff) * 2;
        ushort palettePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAddress);
        var colors = new ushort[16];
        for (int color = 0; color < colors.Length; color++)
        {
            int address = NativeSnapshotMemory.CannonDefinitionBank | (palettePointer + color * 2);
            colors[color] = (ushort)(CartridgeImportSource.Require(bus).ReadCartridgeWord(address) & 0x7fff);
        }
        return colors;
    }

    /// <summary>A native little-endian PLM program word at a bank-$84 CPU address.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for bank-$84 PLM program words")]
    internal static ushort ReadRetailPlmProgramWord(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Replays the complete supplied Ridley movie through production gameplay and compares accepted update state with native checkpoints.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables")]
    internal static void VerifyRidleyFullMovie(string directory)
    {
        var ridleyMovie = ReplayMovie.Load("Ridley", RidleyMoviePath, RidleyMovieSha256);
        byte[] movie = ridleyMovie.Bytes;
        using var replayTrace = ReplayTrace.OpenCompact(directory, ReplayTraceMemoryCatalog.RidleyShowcase);
        var root = replayTrace.Updates;
        // v5-v7 only add message-box and boot-entering dispatch evidence; this snapshot movie
        // never reboots, which the integer expectedRecord read below confirms.
        AssertTrue(root.GetProperty("format").GetString() is "super-metroid-gameplay-updates-v7", "converted replay format");
        AssertEqual(0, root.GetProperty("initialRecord").GetInt32(), "snapshot movie has no folded boot prelude");
        AssertEqual(Convert.ToHexString(SHA256.HashData(movie)), root.GetProperty("movieSha256").GetString(), "converted movie identity");
        AssertEqual(10890, root.GetProperty("sourceFrameCount").GetInt32(), "complete original movie coverage");
        var updates = root.GetProperty("updates").EnumerateArray().ToArray();
        int length = root.GetProperty("updateCount").GetInt32();
        AssertEqual(length, updates.Length, "converted update count");
        // The compact trace checks its stream against its compact manifest; the manifest's
        // source capture must be the one the converted manifest names.
        AssertEqual(replayTrace.SourceCheckpointsSha256, root.GetProperty("checkpointsSha256").GetString(), "native checkpoint identity");
        int lastNativeRecord = -1;
        NativeCheckpointMemory ReadFrame(int update)
        {
            int wantedRecord = update == 0 ? 0 : updates[update - 1].GetProperty("expectedRecord").GetInt32();
            AssertTrue(wantedRecord > lastNativeRecord, "native checkpoint order remains forward-only");
            NativeCheckpoint record = replayTrace.Records.Read(wantedRecord);
            lastNativeRecord = wantedRecord;
            int expectedFrame = update < length ? updates[update].GetProperty("sourceFrame").GetInt32() : 10890;
            AssertEqual(expectedFrame, record.SourceFrame, "native input-boundary frame");
            AssertEqual(update < length ? NativeSnapshotMemory.ReadControllerInput : 0,
                record.BoundaryPc, "native accepted-input boundary or terminal");
            return record.Memory;
        }
        NativeCheckpointMemory memory = ReadFrame(0);
        ushort W(int address) => memory.Word(address);
        var (bus, runtime, samus, game) = ImportNativeSnapshot(memory.ToWorkRam(), ridleyMovie.InitialSaveRam);
        // Ridley's body, fragments and fireballs run typed programs (#1287); compare their cursors as native pointers.
        var pointers = new RidleyNativeInstructionPointers(CartridgeImportSource.Require(bus));
        PlmSlot[] initialPlms = [.. runtime.Plms.SlotTable.Slots];
        var checkedProjectileCompositions = new HashSet<CheckedProjectileComposition>();
        ushort[]? pendingLoadedOwners = null;
        int loadingIntervals = 0;
        ushort[] CaptureLoadedOwners()
        {
            var words = new List<ushort> { checked((ushort)runtime.System.RandomNumber) };
            // The final scrolling IRQ publishes the visor before the deferred
            // loading owners reach their shared completion boundary. Check it there.
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                if (color != XrayScopeDefinitionsPaletteConstants.VisorCgramIndex)
                    words.Add(runtime.Cgram.Colors[color].ToWord());
            foreach (var actor in runtime.Enemies.SlotTable.Slots)
                words.AddRange([SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(actor.DefinitionId), actor.Position.X.Whole, actor.Position.X.Fraction,
                    actor.Position.Y.Whole, actor.Position.Y.Fraction, checked((ushort)actor.Health), actor.SpritemapPointer,
                    checked((ushort)actor.CurrentInstruction), actor.InstructionTimer.RawValue]);
            return words.ToArray();
        }
        int pairedBodyDraws = 0, phaseShiftedBodyDraws = 0, retainedHiddenBodyRecords = 0;
        BodyRecordResult BodyRecord() =>
            new BodyRecordResult(samus.Presentation.TopSpritemapIndex, samus.Presentation.BottomSpritemapIndex,
                samus.Presentation.SpritemapPosition.X, samus.Presentation.SpritemapPosition.Y);
        var previousBodyRecord = BodyRecord();
        for (int frame = 0; frame <= length; frame++)
        {
            ushort previousKnockbackTimer = W(NativeSnapshotMemory.KnockbackTimer);
            ushort previousInvincibilityTimer = W(NativeSnapshotMemory.InvincibilityTimer);
            if (frame != 0) memory = ReadFrame(frame);
            var mismatches = new List<string>();
            // The cartridge parks an unaimed whip target at $FFFF.
            static ushort WhipTargetWord(int? target) => target is int angle ? checked((ushort)angle) : ushort.MaxValue;

            void Check(string name, int actual, int address)
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
            Check("Previous HUD selection", runtime.Hud.PreviousSelectedItem.ToNativeWord(), NativeSnapshotMemory.PreviousHudSelection);
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
                bool normalizedBodyVisible = forcedBodyVisible || (runtime.Clock.Counter & 1) == 0;
                if (frame != 0)
                {
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
                        AssertEqual(new RidleyNativeBodyRecordResult(W(NativeSnapshotMemory.SamusTopSpritemap), W(NativeSnapshotMemory.SamusBottomSpritemap),
                                W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY)),
                            nativeDraw, $"update {frame}: cartridge draw oracle matches native visible records");
                    if (normalizedBodyVisible)
                        AssertEqual(new BodyRecordResult(nativeDraw.Top, nativeDraw.Bottom, nativeDraw.X, nativeDraw.Y), BodyRecord(),
                            $"update {frame}: visible body matches cartridge draw oracle regardless of flicker phase");
                }
                // Flicker deliberately follows the retained NMI clock. Removed hardware
                // waits can change its parity, and hidden frames retain the last visible
                // origin/indices. Compare newly published records when both draws execute.
                if (frame == 0 || (nativeBodyVisible && normalizedBodyVisible))
                {
                    Check("Samus top spritemap", samus.Presentation.TopSpritemapIndex, NativeSnapshotMemory.SamusTopSpritemap);
                    Check("Samus bottom spritemap", samus.Presentation.BottomSpritemapIndex, NativeSnapshotMemory.SamusBottomSpritemap);
                    Check("Samus sprite X", samus.Presentation.SpritemapPosition.X, NativeSnapshotMemory.SamusSpriteX);
                    Check("Samus sprite Y", samus.Presentation.SpritemapPosition.Y, NativeSnapshotMemory.SamusSpriteY);
                }
                if (frame != 0)
                {
                    var nativeCannon = RidleyNativeCannonDraw(bus, memory, invincibleAtDraw, W(NativeSnapshotMemory.NmiCounter));
                    if (nativeCannon.SpriteWritten)
                        AssertTrue(ContainsMovieCannonSprite(memory.Slice(NativeSnapshotMemory.OamLow, 512),
                            memory.Slice(NativeSnapshotMemory.OamHigh, 32), nativeCannon),
                            $"update {frame}: native OAM contains reference cannon sprite");
                    // The normalized draw is observed through its outputs: the OAM image of the port's frame must
                    // hold the reference sprite (position and attributes), and the cover draw must show the frame
                    // composed over the cover tile the reference uploads.
                    var cannon = RidleyNativeCannonDraw(bus, memory, invincibleAtDraw, checked((ushort)runtime.Clock.Counter));
                    OamBuffer portImage = runtime.Sprites.FinalizedAsOam();
                    if (cannon.SpriteWritten)
                        AssertTrue(ContainsMovieCannonSprite(portImage.LowTable, portImage.HighTable, cannon),
                            $"update {frame}: port frame contains reference cannon sprite");
                    SpriteDraw[] covers = [.. runtime.Sprites.Draws
                        .Where(draw => draw.Source.Kind == SpriteSourceKind.SamusArmCannon)];
                    AssertEqual(cannon.SpriteWritten ? 1 : 0, covers.Length, $"update {frame}: cannon draw count");
                    if (cannon.SpriteWritten && cannon.TileUploadQueued)
                        AssertEqual(ArmCannonTileSources.CoverTileDmaSources.IndexOf(cannon.TileSource),
                            SheetFrameSources.CharacterDraw((ImageSpriteDraw)covers[0]).Artwork.Id.Variant,
                            $"update {frame}: cannon draw binds the reference cover tile");
                }
                Check("Cannon flags", SnesLittleEndian.Word(checked((byte)samus.ArmCannon.OpenFlag), checked((byte)samus.ArmCannon.CloseFlag)), NativeSnapshotMemory.CannonFlags);
                Check("Cannon frame", samus.ArmCannon.Frame, NativeSnapshotMemory.CannonFrame);
                Check("Cannon toggle", samus.ArmCannon.ToggleFlag, NativeSnapshotMemory.CannonToggle);
                Check("Cannon drawing mode", samus.ArmCannon.DrawingMode, NativeSnapshotMemory.CannonDrawingMode);
                Check("Minimap disabled", runtime.Hud.MinimapDisabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.MinimapDisabled);
                CheckBytes("Room scroll storage", RoomScrollGrid.StorageByteCount,
                    runtime.Room.LoadedCamera.Scrolls.ReadStorage, NativeSnapshotMemory.ScrollStorage);
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
            if (runtime.Clock.Counter != normalizedNmi)
                mismatches.Add($"Accepted gameplay NMI: native={normalizedNmi:X4} port={runtime.Clock.Counter:X4}");
            // All excluded accepted upload NMIs in this movie occur while the door
            // IRQ repeatedly clears this byte. Its post-door phase therefore compares
            // directly, unlike the independently retained word counter above.
            AssertEqual(memory[NativeSnapshotMemory.NmiCounterByte], runtime.Clock.Counter8,
                $"update {frame}: byte NMI counter including native door IRQ reset");
            Check("Game state", (ushort)game.GameState, NativeSnapshotMemory.GameState);
            Check("Enemy door gate", runtime.Enemies.RoomStatus.EnemyDoorTransitionActive ? (ushort)1 : (ushort)0, NativeSnapshotMemory.EnemyDoorTransition);
            // Native LoadDoorHeader publishes the destination room pointer before
            // loading its room/state data. The port keeps that identity in the pending
            // door while ActiveRoom still owns the source room's loaded data.
            ushort selectedRoom = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.AlignSourceCamera or DoorTransitionPhase.FixDoorsMovingUp or
                DoorTransitionPhase.SetupNewRoom or DoorTransitionPhase.SetupScrolling or
                DoorTransitionPhase.PlaceSamusAndLoadTiles or DoorTransitionPhase.LoadMoreThingsAndOpenDoor
                ? RoomHeaderDefinitions.PointerOf((runtime.RoomLoader.PendingDoorTransition ?? throw new InvalidDataException("Missing selected destination door")).DestinationRoom)
                : runtime.Room.LoadedRoom.Pointer;
            Check("Selected room", selectedRoom, NativeSnapshotMemory.Room);
            Check("Camera X", runtime.Room.LoadedCamera.PixelPosition.X, NativeSnapshotMemory.CameraX);
            Check("Camera X fraction", runtime.Room.LoadedCamera.Position.X.Fraction, NativeSnapshotMemory.CameraXFraction);
            Check("Camera Y", runtime.Room.LoadedCamera.PixelPosition.Y, NativeSnapshotMemory.CameraY);
            Check("Camera Y fraction", runtime.Room.LoadedCamera.Position.Y.Fraction, NativeSnapshotMemory.CameraYFraction);
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                Check("Ideal camera X", runtime.Room.LoadedCamera.IdealPosition.X, NativeSnapshotMemory.IdealCameraX);
                Check("Ideal camera Y", runtime.Room.LoadedCamera.IdealPosition.Y, NativeSnapshotMemory.IdealCameraY);
                Check("Camera speed X", runtime.Room.LoadedCamera.Speed.X.WholeWord, NativeSnapshotMemory.CameraSpeedX);
                Check("Camera speed X fraction", runtime.Room.LoadedCamera.Speed.X.Fraction, NativeSnapshotMemory.CameraSpeedXFraction);
                Check("Camera speed Y", runtime.Room.LoadedCamera.Speed.Y.WholeWord, NativeSnapshotMemory.CameraSpeedY);
                Check("Camera speed Y fraction", runtime.Room.LoadedCamera.Speed.Y.Fraction, NativeSnapshotMemory.CameraSpeedYFraction);
                // A fresh door camera defers its first sample to the runtime's frame-start
                // fallback. Compare that effective sample instead of requiring storage.
                var previous = runtime.Room.LoadedCamera.PreviousSamusPoint ?? samus.Position;
                Check("Previous Samus X", previous.X.Whole, NativeSnapshotMemory.PreviousSamusX);
                Check("Previous Samus X fraction", previous.X.Fraction, NativeSnapshotMemory.PreviousSamusXFraction);
                Check("Previous Samus Y", previous.Y.Whole, NativeSnapshotMemory.PreviousSamusY);
                Check("Previous Samus Y fraction", previous.Y.Fraction, NativeSnapshotMemory.PreviousSamusYFraction);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
                {
                    var effect = samus.LiquidPhysics.AtmosphericEffects.Slots[index];
                    Check($"Atmosphere {index} frame/type", effect.FrameAndType, NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
                    if (effect.Type == 0) continue;
                    Check($"Atmosphere {index} timer", effect.AnimationTimer.RawValue, NativeSnapshotMemory.AtmosphericTimer + index * 2);
                    Check($"Atmosphere {index} X", effect.Position.X, NativeSnapshotMemory.AtmosphericX + index * 2);
                    Check($"Atmosphere {index} Y", effect.Position.Y, NativeSnapshotMemory.AtmosphericY + index * 2);
                }
                Check("Liquid animation buffer", checked((ushort)samus.AnimationFrameBuffer), NativeSnapshotMemory.AnimationFrameBuffer);
                Check("LiquidPhysicsType", (ushort)samus.LiquidPhysics.LiquidPhysicsType, NativeSnapshotMemory.LiquidPhysicsType);
                Check("PeriodicSubDamage", checked((ushort)samus.LiquidPhysics.PeriodicSubDamage), NativeSnapshotMemory.PeriodicSubDamage);
                Check("PeriodicDamage", checked((ushort)samus.LiquidPhysics.PeriodicDamage), NativeSnapshotMemory.PeriodicDamage);
                Check("Acid damage surface", checked((ushort)samus.LiquidPhysics.LavaAcidYPosition), NativeSnapshotMemory.AcidSurface);
                Check("Liquid tide phase", checked((ushort)runtime.RoomLayer3Fx.TidePhase), NativeSnapshotMemory.TidePhase);
            }
            Check("Samus X", samus.Position.X.Whole, NativeSnapshotMemory.X);
            Check("Samus X fraction", samus.Kinematics.Position.X.Fraction, NativeSnapshotMemory.XFraction);
            Check("Samus Y", samus.Position.Y.Whole, NativeSnapshotMemory.Y);
            Check("Samus Y fraction", samus.Kinematics.Position.Y.Fraction, NativeSnapshotMemory.YFraction);
            Check("Samus pose", (ushort)samus.Pose, NativeSnapshotMemory.Pose);
            Check("PreviousDrawHeldInput", (ushort)samus.InputSnapshot.PreviousDrawHeldInput, NativeSnapshotMemory.SamusFilteredHeld);
            Check("PreviousDrawNewInput", (ushort)samus.InputSnapshot.PreviousDrawNewInput, NativeSnapshotMemory.SamusFilteredNew);
            Check("AutoJumpTimer", samus.InputSnapshot.AutoJumpTimer, NativeSnapshotMemory.SamusAutoJumpTimer);
            Check("PreviousHealthForHurtCheck", SamusQuantity.Encode(samus.PreviousHealthForHurtCheck), NativeSnapshotMemory.SamusPreviousHealthForFlash);
            AssertTrue(!samus.InputSnapshot.ShinesparkPoseInputLocked && !samus.InputSnapshot.CrystalFlashPoseInputLocked,
                "Movie now uses a special pose-input lock; map its handler explicitly");
            Check("Samus input handler", samus.InputSnapshot.AutoJumpInputPending
                ? NativeSnapshotMemory.SamusAutoJumpInputHandler : NativeSnapshotMemory.SamusNormalInputHandler,
                NativeSnapshotMemory.SamusInputHandler);

            Check("Samus previous pose", (ushort)samus.PoseHistory.PreviousPose, NativeSnapshotMemory.PreviousPose);
            Check("Samus previous movement", samus.PoseHistory.PreviousDirectionAndMovement, NativeSnapshotMemory.PreviousDirection);
            Check("Samus last different pose", (ushort)samus.PoseHistory.LastDifferentPose, NativeSnapshotMemory.LastDifferentPose);
            Check("Samus last different movement", samus.PoseHistory.LastDifferentDirectionAndMovement, NativeSnapshotMemory.LastDifferentDirection);
            Check("Samus animation", samus.AnimationFrame, NativeSnapshotMemory.Animation);
            Check("Samus animation timer", samus.AnimationFrameTimer.RawValue, NativeSnapshotMemory.AnimationTimer);
            Check("Samus base speed", samus.HorizontalSpeed.BaseSpeed.Whole, NativeSnapshotMemory.BaseSpeed);
            Check("Samus base fraction", samus.HorizontalSpeed.BaseSpeed.Fraction, NativeSnapshotMemory.BaseFraction);
            Check("Samus extra speed", samus.HorizontalSpeed.ExtraRunSpeed.Whole, NativeSnapshotMemory.ExtraSpeed);
            Check("Samus extra fraction", samus.HorizontalSpeed.ExtraRunSpeed.Fraction, NativeSnapshotMemory.ExtraFraction);
            Check("Samus vertical speed", samus.Kinematics.YSpeed.Whole, NativeSnapshotMemory.VerticalSpeed);
            Check("Samus vertical fraction", samus.Kinematics.YSpeed.Fraction, NativeSnapshotMemory.VerticalFraction);
            Check("Samus vertical direction", (ushort)samus.Kinematics.YDirection, NativeSnapshotMemory.VerticalDirection);
            Check("Samus SamusXRadius", samus.Kinematics.Radius.X, NativeSnapshotMemory.SamusXRadius);
            Check("Samus SamusYRadius", samus.Kinematics.Radius.Y, NativeSnapshotMemory.SamusYRadius);
            Check("Samus Gravity", samus.Kinematics.YAcceleration.Whole, NativeSnapshotMemory.Gravity);
            Check("Samus GravityFraction", samus.Kinematics.YAcceleration.Fraction, NativeSnapshotMemory.GravityFraction);
            Check("Samus ExtraXDisplacement", unchecked((ushort)samus.Kinematics.ExtraDisplacement.X.WholePixels), NativeSnapshotMemory.ExtraXDisplacement);
            Check("Samus ExtraXDisplacementFraction", samus.Kinematics.ExtraDisplacement.X.Fraction, NativeSnapshotMemory.ExtraXDisplacementFraction);
            Check("Samus ExtraYDisplacement", unchecked((ushort)samus.Kinematics.ExtraDisplacement.Y.WholePixels), NativeSnapshotMemory.ExtraYDisplacement);
            Check("Samus ExtraYDisplacementFraction", samus.Kinematics.ExtraDisplacement.Y.Fraction, NativeSnapshotMemory.ExtraYDisplacementFraction);
            Check("Samus SlopeCollisionEnable", samus.Kinematics.HorizontalSlopeCollisionEnable, NativeSnapshotMemory.SlopeCollisionEnable);
            Check("Samus SpeedDivisor", checked((ushort)samus.HorizontalSpeed.SpeedDivisor), NativeSnapshotMemory.SpeedDivisor);
            Check("Samus ContactDamageIndex", (ushort)samus.HorizontalSpeed.ContactDamageIndex, NativeSnapshotMemory.ContactDamageIndex);
            Check("Samus MorphBallBounceState", (ushort)samus.MorphBallBounceState, NativeSnapshotMemory.MorphBallBounceState);
            Check("Samus BombJumpDirection", samus.BombJumpDirection, NativeSnapshotMemory.BombJumpDirection);
            Check("Samus running momentum", samus.HorizontalSpeed.HasRunningMomentum ? (ushort)1 : (ushort)0, NativeSnapshotMemory.Momentum);
            Check("Samus speed boost counter", checked((ushort)samus.HorizontalSpeed.SpeedBoostCounter.RawValue), NativeSnapshotMemory.BoostCounter);
            Check("Samus horizontal speed table", samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, NativeSnapshotMemory.HorizontalSpeedTable);
            AssertEqual(memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier], samus.HorizontalSpeed.DecelerationMultiplier,
                $"update {frame}: Samus horizontal deceleration multiplier");
            Check("Samus echo sound latch", samus.HorizontalSpeed.EchoSoundFlag, NativeSnapshotMemory.SpeedEchoSoundLatch);
            Check("Samus slope adjustment", samus.Kinematics.PositionAdjustedBySlope ? (ushort)1 : (ushort)0,
                NativeSnapshotMemory.SamusSlopeAdjusted);
            for (int direction = 0; direction < 4; direction++)
                Check($"Samus solid enemy {direction}", samus.Kinematics.SolidEnemyCollisionIndexes[direction],
                    NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2);
            var inheritance = new ProjectileInheritanceInspection(samus.ProjectileInheritance);
            for (int offset = 0; offset < inheritance.ByteCount; offset += 2)
            {
                int address = NativeSnapshotMemory.ProjectileInheritancePrefix + offset;
                Check($"Projectile inherited movement ${address:X4}", inheritance.Word(offset), address);
            }
            Check("Samus total horizontal speed", samus.HorizontalSpeed.TotalSpeed.Whole, NativeSnapshotMemory.TotalHorizontalSpeed);
            Check("Samus total horizontal fraction", samus.HorizontalSpeed.TotalSpeed.Fraction, NativeSnapshotMemory.TotalHorizontalSubspeed);
            Check("Samus movement handler", samus.KnockbackActive
                ? NativeSnapshotMemory.KnockbackMovementHandler : NativeSnapshotMemory.NormalMovementHandler,
                NativeSnapshotMemory.SamusMovementHandler);
            // DoorTransitionState installs a locked pair to suppress host control, while
            // native door dispatch retains ordinary alpha/beta pointers unused.
            bool nativeControlLock = samus.Control.AlphaLocked && game.GameState is not
                (SuperMetroidGameState.HitDoorBlock or SuperMetroidGameState.LoadingNextRoomA or
                 SuperMetroidGameState.LoadingNextRoomB);
            Check("Samus alpha handler", nativeControlLock
                ? NativeSnapshotMemory.LockedAlphaHandler : NativeSnapshotMemory.NormalAlphaHandler,
                NativeSnapshotMemory.SamusAlphaHandler);
            Check("Samus beta handler", nativeControlLock
                ? NativeSnapshotMemory.LockedBetaHandler : NativeSnapshotMemory.NormalBetaHandler,
                NativeSnapshotMemory.SamusBetaHandler);

            Check("Samus health", SamusQuantity.Encode(samus.Health), NativeSnapshotMemory.Health);
            Check("Samus general Samus damage immunity countdown", samus.InvincibilityTimer.RawValue, NativeSnapshotMemory.InvincibilityTimer);
            Check("Samus Samus knockback countdown", samus.KnockbackTimer.RawValue, NativeSnapshotMemory.KnockbackTimer);
            Check("Samus Samus knockback direction", (ushort)samus.KnockbackDirection, NativeSnapshotMemory.KnockbackDirection);
            Check("Samus horizontal knockback direction", (ushort)samus.KnockbackXDirection, NativeSnapshotMemory.KnockbackXDirection);
            Check("Samus hurt palette/audio recovery countdown", checked((ushort)samus.HurtFlashCounter), NativeSnapshotMemory.HurtFlashCounter);
            Check("Samus fractional health word", checked((ushort)samus.SubunitHealth), NativeSnapshotMemory.SubunitHealth);
            Check("Samus selected HUD weapon", samus.SelectedHudItem.ToNativeWord(), NativeSnapshotMemory.SelectedHudItem);
            Check("Samus auto-cancel HUD selection", samus.AutoCancelHudItemIndex.ToNativeWord(), NativeSnapshotMemory.AutoCancelHudItemIndex);
            Check("Samus acceleration mode", SamusAccelerationModeCodec.ToNative(samus.HorizontalSpeed.AccelerationMode), NativeSnapshotMemory.AccelerationMode);
            Check("Samus maximum health", SamusQuantity.Encode(samus.MaxHealth), NativeSnapshotMemory.MaxHealth);
            Check("Samus equipped items", samus.EquippedItems.ToNativeWord(), NativeSnapshotMemory.Items);
            Check("Samus collected items", samus.CollectedItems.ToNativeWord(), NativeSnapshotMemory.CollectedItems);
            Check("Samus equipped beams", samus.EquippedBeams.ToNativeWord(), NativeSnapshotMemory.Beams);
            Check("Samus collected beams", samus.CollectedBeams.ToNativeWord(), NativeSnapshotMemory.CollectedBeams);
            Check("Samus missiles", SamusQuantity.Encode(samus.Missiles), NativeSnapshotMemory.Missiles);
            Check("Samus missile capacity", SamusQuantity.Encode(samus.MaxMissiles), NativeSnapshotMemory.MaxMissiles);
            Check("Samus super missiles", SamusQuantity.Encode(samus.SuperMissiles), NativeSnapshotMemory.SuperMissiles);
            Check("Samus super missile capacity", SamusQuantity.Encode(samus.MaxSuperMissiles), NativeSnapshotMemory.MaxSuperMissiles);
            Check("Samus power bombs", SamusQuantity.Encode(samus.PowerBombs), NativeSnapshotMemory.PowerBombs);
            Check("Samus power bomb capacity", SamusQuantity.Encode(samus.MaxPowerBombs), NativeSnapshotMemory.MaxPowerBombs);
            Check("Samus reserve mode", samus.ReserveTankMode.ToNativeWord(), NativeSnapshotMemory.ReserveMode);
            Check("Samus reserve capacity", SamusQuantity.Encode(samus.MaxReserveEnergy), NativeSnapshotMemory.MaxReserve);
            Check("Samus reserve energy", SamusQuantity.Encode(samus.ReserveEnergy), NativeSnapshotMemory.Reserve);
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
            IPaletteColors comparedPalette = runtime.Cgram.AsPaletteColors();
            if (game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB or
                SuperMetroidGameState.UnpausingA or SuperMetroidGameState.UnpausingB)
            {
                object menu = game.Session.ScreenAs<PauseMenuState>() ?? throw new InvalidDataException("Native pause palette requires the active pause menu.");
                comparedPalette = ((PauseMenuState)menu).screen.Palette;
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
                    Check("Completed-scroll visor", runtime.Cgram.Colors[XrayScopeDefinitionsPaletteConstants.VisorCgramIndex].ToWord(),
                        NativeSnapshotMemory.PaletteBuffer + XrayScopeDefinitionsPaletteConstants.VisorCgramIndex * 2);
                ushort[] owners = CaptureLoadedOwners();
                if (pendingLoadedOwners is not null && !owners.SequenceEqual(pendingLoadedOwners))
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
            foreach (var actor in runtime.Enemies.SlotTable.Slots)
            {
                if (deferLoadingOwners) continue;
                int address = NativeSnapshotMemory.EnemyBase + actor.EnemyIndex;
                string owner = $"Enemy {actor.SlotIndex}";
                Check(owner + " identity", SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(actor.DefinitionId), address);
                if (actor.DefinitionId == 0 || W(address) == 0) continue;
                Check(owner + " X", actor.Position.X.Whole, address + 2);
                Check(owner + " X fraction", actor.Position.X.Fraction, address + 4);
                Check(owner + " Y", actor.Position.Y.Whole, address + 6);
                Check(owner + " Y fraction", actor.Position.Y.Fraction, address + 8);
                Check(owner + " health", checked((ushort)actor.Health), address + 20);
                Check(owner + " spritemap", actor.SpritemapPointer, address + 22);
                Check(owner + " instruction", pointers.Enemy(runtime.Enemies, actor), address + 26);
                Check(owner + " instruction timer", actor.InstructionTimer.RawValue, address + 28);
                Check(owner + " XRadius", actor.Radius.X, address + 10);
                Check(owner + " YRadius", actor.Radius.Y, address + 12);
                Check(owner + " Properties", actor.Properties.ToNativeWord(), address + 14);
                Check(owner + " ExtraProperties", actor.ExtraProperties.ToNativeWord(), address + 16);
                Check(owner + " AiStates", actor.AiStates.NativeWord, address + 18);
                Check(owner + " Timer", actor.Timer.RawValue, address + 24);
                Check(owner + " PaletteIndex", actor.PaletteIndex, address + 30);
                Check(owner + " TileIndexBase", actor.TileIndexBase, address + 32);
                Check(owner + " Layer", actor.Layer, address + 34);
                Check(owner + " FlashTimer", actor.FlashTimer.RawValue, address + 36);
                Check(owner + " FrozenTimer", actor.FrozenTimer.RawValue, address + 38);
                Check(owner + " InvincibilityTimer", actor.InvincibilityTimer.RawValue, address + 40);
                Check(owner + " ShakeTimer", actor.ShakeTimer.RawValue, address + 42);
                Check(owner + " FrameCounter", actor.FrameCounter, address + 44);
            }
            if (!deferLoadingOwners && runtime.Enemies.RidleyEncounter.Ridley is { } ridleyState &&
                W(NativeSnapshotMemory.EnemyBase) == SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.Ridley))
            {
                bool expectedGate = (W(NativeSnapshotMemory.EnemyBase + 14) & 0x0400) != 0;
                if (runtime.Enemies.SlotTable.Slots[0].Properties.HasAny(EnemyProperties.IgnoreSamusCollision) != expectedGate)
                    mismatches.Add($"Ridley interaction gate: native={expectedGate}");
                // Ridley's AI function is an identity (#1413): the native routine word is translated and the identities compared.
                ushort nativeFunctionWord = W(NativeSnapshotMemory.RidleyFunction);
                if (!RidleyNativeWords.AiFunctions.TryDecode(nativeFunctionWord, out RidleyAiFunction nativeFunction) ||
                    nativeFunction != ridleyState.Function)
                    mismatches.Add($"Ridley AI function: native={nativeFunctionWord:X4} port={ridleyState.Function}");
                Check("Ridley AI timer", ridleyState.FunctionTimer.RawValue, NativeSnapshotMemory.RidleyFunctionTimer);
                Check("Ridley TailFunctionIndex", (ushort)ridleyState.TailFunctionIndex, NativeSnapshotMemory.RidleyTailFunctionIndex);
                Check("Ridley IdleTailWhipEnabled", ridleyState.IdleTailWhipEnabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.RidleyIdleTailWhipEnabled);
                Check("Ridley TailWhipRequest", checked((ushort)ridleyState.TailWhipRequest), NativeSnapshotMemory.RidleyTailWhipRequest);
                Check("Ridley TailExtensionSpeed", checked((ushort)ridleyState.TailExtensionSpeed), NativeSnapshotMemory.RidleyTailExtensionSpeed);
                Check("Ridley TailAngleDelta", checked((ushort)ridleyState.TailAngleDelta), NativeSnapshotMemory.RidleyTailAngleDelta);
                Check("Ridley TailMinimumClockwiseAngle", ridleyState.TailMinimumClockwiseAngle.RawValue, NativeSnapshotMemory.RidleyTailMinimumClockwiseAngle);
                Check("Ridley TailMaximumCounterClockwiseAngle", ridleyState.TailMaximumCounterClockwiseAngle.RawValue, NativeSnapshotMemory.RidleyTailMaximumCounterClockwiseAngle);
                Check("Ridley TailWhipTargetClockwiseAngle", WhipTargetWord(ridleyState.ClockwiseWhipTarget), NativeSnapshotMemory.RidleyTailWhipTargetClockwiseAngle);
                Check("Ridley TailWhipTargetCounterClockwiseAngle", WhipTargetWord(ridleyState.CounterClockwiseWhipTarget), NativeSnapshotMemory.RidleyTailWhipTargetCounterClockwiseAngle);
                Check("Ridley IdealInterSegmentTailAngle", checked((ushort)ridleyState.IdealInterSegmentTailAngle), NativeSnapshotMemory.RidleyIdealInterSegmentTailAngle);
                Check("Ridley HorizontalVelocity", ridleyState.Velocity.X.RawValue, NativeSnapshotMemory.RidleyHorizontalVelocity);
                Check("Ridley VerticalVelocity", ridleyState.Velocity.Y.RawValue, NativeSnapshotMemory.RidleyVerticalVelocity);
                Check("Ridley FightMode", (ushort)ridleyState.FightMode, NativeSnapshotMemory.RidleyFightMode);
                Check("Ridley MovementAnimationEnabled", ridleyState.MovementAnimationEnabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.RidleyMovementAnimationEnabled);
                Check("Ridley WingFrame", checked((ushort)ridleyState.WingFrame), NativeSnapshotMemory.RidleyWingFrame);
                Check("Ridley WingAnimationTimerDelta", checked((ushort)ridleyState.WingAnimationTimerDelta), NativeSnapshotMemory.RidleyWingAnimationTimerDelta);
                Check("Ridley WingAnimationTimer", ridleyState.WingAnimationTimer.RawValue, NativeSnapshotMemory.RidleyWingAnimationTimer);
                Check("Ridley FacingDirection", (ushort)ridleyState.FacingDirection, NativeSnapshotMemory.RidleyFacingDirection);
                Check("Ridley HealthStage", checked((ushort)ridleyState.HealthStage), NativeSnapshotMemory.RidleyHealthStage);
                Check("Ridley GrabXOffset", unchecked((ushort)ridleyState.GrabOffset.X), NativeSnapshotMemory.RidleyGrabXOffset);
                Check("Ridley GrabYOffset", unchecked((ushort)ridleyState.GrabOffset.Y), NativeSnapshotMemory.RidleyGrabYOffset);
                Check("Ridley TailDamage", checked((ushort)ridleyState.TailDamage), NativeSnapshotMemory.RidleyTailDamage);
                Check("Ridley FeetDistanceIndex", checked((ushort)(ridleyState.FeetDistanceIndex * sizeof(ushort))), NativeSnapshotMemory.RidleyFeetDistanceIndex);
                Check("Ridley IntangibilityTimer", ridleyState.IntangibilityTimer.RawValue, NativeSnapshotMemory.RidleyIntangibilityTimer);
                if (ridleyState.Function is RidleyAiFunction.NorfairSwoopMoveToStart or
                    RidleyAiFunction.NorfairSwoopAimDown or RidleyAiFunction.NorfairSwoopAimSideways or
                    RidleyAiFunction.NorfairSwoopAimUp or RidleyAiFunction.NorfairSwoopClimb or RidleyAiFunction.NorfairSwoopRecover)
                    Check("Ridley swoop timer", ridleyState.SwoopPhaseTimer.RawValue, NativeSnapshotMemory.RidleySwoopTimer);
                if (game.GameState == SuperMetroidGameState.MainGameplay)
                {
                    // Tail workspace becomes live after its first fade-owned composition.
                    Check("Ridley tail tip X", ridleyState.TailSegments[6].Position.X, NativeSnapshotMemory.TailTipX);
                    Check("Ridley tail tip Y", ridleyState.TailSegments[6].Position.Y, NativeSnapshotMemory.TailTipY);
                    for (int tailIndex = 0; tailIndex < ridleyState.TailSegments.Length; tailIndex++)
                    {
                        var segment = ridleyState.TailSegments[tailIndex];
                        int tailAddress = NativeSnapshotMemory.TailSegments + tailIndex * NativeSnapshotMemory.TailSegmentStride;
                        string tailOwner = $"Ridley tail {tailIndex}";
                        Check(tailOwner + " active", segment.Active ? NativeSnapshotMemory.TailSegmentActive : (ushort)0, tailAddress);
                        // A propagated stagger has no angle in the port (#1413); the cartridge stores -1 there.
                        Check(tailOwner + " StaggerAngle", segment.StaggerAngle is { } stagger ? checked((ushort)stagger) : ushort.MaxValue, tailAddress + 2);
                        Check(tailOwner + " MovementDirection", segment.Clockwise ? (ushort)0x8000 : (ushort)0, tailAddress + 4);
                        Check(tailOwner + " Distance", checked((ushort)segment.Distance), tailAddress + 6);
                        Check(tailOwner + " TargetDistance", checked((ushort)segment.TargetDistance), tailAddress + 8);
                        Check(tailOwner + " Angle", checked((ushort)segment.Angle), tailAddress + 10);
                        Check(tailOwner + " XPosition", segment.Position.X, tailAddress + 12);
                        Check(tailOwner + " YPosition", segment.Position.Y, tailAddress + 14);
                        Check(tailOwner + " XOffset", unchecked((ushort)segment.Offset.X), tailAddress + 16);
                        Check(tailOwner + " YOffset", unchecked((ushort)segment.Offset.Y), tailAddress + 18);
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
                    Check($"PLM {slotIndex} header", active ? (ushort)slot.HeaderPointer : (ushort)0, NativeSnapshotMemory.PlmHeaders + offset);
                    if (!active || W(NativeSnapshotMemory.PlmHeaders + offset) == 0) continue;
                    ushort preInstruction = slot.PreInstruction;
                    var greyDoor = runtime.Plms.GreyDoorSnapshots.FirstOrDefault(door => door.Header == slot.HeaderPointer && door.BlockIndex == slot.BlockIndex);
                    AssertTrue(greyDoor.Header != PlmHeaderId.None,
                        $"Movie PLM {(int)slot.HeaderPointer:X4} needs a family-variable coverage mapping");
                    Check($"PLM {slotIndex} grey-door condition", (ushort)((int)greyDoor.Condition * 2), NativeSnapshotMemory.PlmFamilyVariable + offset);
                    // Grey doors require one hit. Opening owns the incremented counter;
                    // before that transition setup's zero is the live counter value.
                    Check($"PLM {slotIndex} grey-door hit count", checked(greyDoor.HitCounter), NativeSnapshotMemory.PlmExtraVariable + offset);
                    // The typed cap names its native instruction and link through the oracle's step addresses; a closing
                    // cap's link is the one its previous program left, which the closing never reads.
                    GreyDoorPlmAnimation typedDoor = slot.Animation as GreyDoorPlmAnimation
                        ?? throw new InvalidDataException($"Movie grey door at block {slot.BlockIndex} runs no typed program.");
                    ushort instruction = RoomPlmProgramsVerification.GreyDoorNativeInstruction(bus, typedDoor);
                    if (greyDoor.Phase != GreyDoorPhase.Closing && typedDoor.Link is { } link)
                        Check($"PLM {slotIndex} grey-door link", RoomPlmProgramsVerification.GreyDoorNativeEntry(typedDoor.Variant, link),
                            NativeSnapshotMemory.PlmLinkInstruction + offset);
                    Check($"PLM {slotIndex} BlockIndex", unchecked((ushort)(slot.BlockIndex * 2)), NativeSnapshotMemory.PlmBlockIndex + offset);
                    Check($"PLM {slotIndex} PreInstruction", preInstruction, NativeSnapshotMemory.PlmPreInstruction + offset);
                    Check($"PLM {slotIndex} InstructionPointer", instruction, NativeSnapshotMemory.PlmInstructionPointer + offset);
                    Check($"PLM {slotIndex} LoopTimer", slot.LoopTimer, NativeSnapshotMemory.PlmLoopTimer + offset);
                    Check($"PLM {slotIndex} RoomArgument", slot.RoomArgument, NativeSnapshotMemory.PlmRoomArgument + offset);
                    // The native sleep leaves the countdown at zero and lets it run on from $FFFF; the typed hold re-arms
                    // one on each pass. Both wake with one, so only a sleeping door's countdown is left uncompared.
                    if (typedDoor.Program != GreyDoorProgram.Sleep)
                        Check($"PLM {slotIndex} InstructionTimer", slot.InstructionTimer, NativeSnapshotMemory.PlmInstructionTimer + offset);
                }
                var activeLevel = runtime.Room.LevelData ?? throw new InvalidDataException("Missing active collision data.");
                for (int block = 0; block < activeLevel.Size.X * activeLevel.Size.Y; block++)
                {
                    Check($"Room block {block}", activeLevel.ForegroundEntries.Span[block], NativeSnapshotMemory.Level + block * 2);
                    byte expectedBehavior = memory[NativeSnapshotMemory.Bts + block];
                    byte actualBehavior = activeLevel.BehaviorBytes.AsSpan()[block];
                    if (actualBehavior != expectedBehavior)
                        mismatches.Add($"Room BTS {block}: native={expectedBehavior:X2} port={actualBehavior:X2}");
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                foreach (var trail in runtime.Projectiles.TrailSlots)
                {
                    Check($"Trail {trail.SlotIndex} Left timer", trail.Left.InstructionTimer.RawValue, NativeSnapshotMemory.TrailLeftInstructionTimer + trail.NativeByteIndex);
                    if (!trail.Left.InstructionTimer.IsZero)
                    {
                        Check($"Trail {trail.SlotIndex} Left InstructionPointer", trail.Left.InstructionPointer, NativeSnapshotMemory.TrailLeftInstructionPointer + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left TileNumberAttributes", ProjectileTrailNative.NativeWord(trail.Left.ShownPose), NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left XPosition", trail.Left.Position.X, NativeSnapshotMemory.TrailLeftXPosition + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Left YPosition", trail.Left.Position.Y, NativeSnapshotMemory.TrailLeftYPosition + trail.NativeByteIndex);
                    }
                    Check($"Trail {trail.SlotIndex} Right timer", trail.Right.InstructionTimer.RawValue, NativeSnapshotMemory.TrailRightInstructionTimer + trail.NativeByteIndex);
                    if (!trail.Right.InstructionTimer.IsZero)
                    {
                        Check($"Trail {trail.SlotIndex} Right InstructionPointer", trail.Right.InstructionPointer, NativeSnapshotMemory.TrailRightInstructionPointer + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right TileNumberAttributes", ProjectileTrailNative.NativeWord(trail.Right.ShownPose), NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right XPosition", trail.Right.Position.X, NativeSnapshotMemory.TrailRightXPosition + trail.NativeByteIndex);
                        Check($"Trail {trail.SlotIndex} Right YPosition", trail.Right.Position.Y, NativeSnapshotMemory.TrailRightYPosition + trail.NativeByteIndex);
                    }
                }
                Check("Projectile cooldown", runtime.BombProjectiles.CooldownTimer.RawValue, NativeSnapshotMemory.ProjectileCooldown);
                Check("Beam charge", runtime.Projectiles.Charge.FlareCounter, NativeSnapshotMemory.BeamCharge);
                Check("ProjectileCounter", runtime.Projectiles.SlotTable.ProjectileCounter, NativeSnapshotMemory.ProjectileCount);
                Check("PreviousBeamChargeCounter", runtime.Projectiles.Charge.PreviousBeamChargeCounter, NativeSnapshotMemory.PreviousCharge);
                Check("ProjectileInvincibilityTimer", runtime.Projectiles.Firing.ProjectileInvincibilityTimer.RawValue, NativeSnapshotMemory.ProjectileInteractionImmunity);
                Check("ChargedShotGlowTimer", runtime.Projectiles.Charge.ChargedShotGlowTimer.RawValue, NativeSnapshotMemory.ChargedShotGlow);
                Check("SamusChargePaletteIndex", runtime.Projectiles.Charge.SamusChargePaletteIndex, NativeSnapshotMemory.ChargePaletteIndex);
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
                Check("Power-bomb armed flag", explosion.IsArmed ? ushort.MaxValue : (ushort)0, NativeSnapshotMemory.PowerBombArmedFlag);
                Check("Power-bomb explosion status", checked((ushort)explosion.Status), NativeSnapshotMemory.PowerBombExplosionStatus);
                AssertTrue(W(NativeSnapshotMemory.PowerBombArmedFlag) == 0 &&
                    W(NativeSnapshotMemory.PowerBombExplosionStatus) == 0,
                    "Movie now activates a power bomb; add its full live-state mapping");
                Check("BombSpreadChargeTimeoutCounter", checked((ushort)samus.BombSpreadChargeTimeoutCounter), NativeSnapshotMemory.BombSpreadChargeTimeout);
                Check("PoseTransitionShotDirection", samus.PoseTransitionShotDirection, NativeSnapshotMemory.PoseShotDirection);
                Check("HyperBeam", SamusSnapshotWords.EncodeHyperBeam(samus.HyperBeamEnabled), NativeSnapshotMemory.HyperBeam);
                if (samus.ResumeChargingBeamSound != SamusSnapshotWords.DecodeResumeChargingBeamSound(W(NativeSnapshotMemory.ResumeChargeSound)))
                    mismatches.Add($"ResumeChargingBeamSound: native={W(NativeSnapshotMemory.ResumeChargeSound):X4} port={samus.ResumeChargingBeamSound}");

            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Projectiles.Slots.Take(5))
            {
                int index = projectile.NativeByteIndex;
                string owner = $"Projectile {projectile.SlotIndex}";
                Check(owner + " type", projectile.Type, NativeSnapshotMemory.ProjectileType + index);
                if (projectile.Type == 0 || W(NativeSnapshotMemory.ProjectileType + index) == 0) continue;
                Check(owner + " X", projectile.Position.X.Whole, NativeSnapshotMemory.ProjectileX + index);
                Check(owner + " Y", projectile.Position.Y.Whole, NativeSnapshotMemory.ProjectileY + index);
                Check(owner + " X radius", projectile.Radius.X, NativeSnapshotMemory.ProjectileXRadius + index);
                Check(owner + " Y radius", projectile.Radius.Y, NativeSnapshotMemory.ProjectileYRadius + index);
                Check(owner + " damage", projectile.Damage, NativeSnapshotMemory.ProjectileDamage + index);
                Check(owner + " XFraction", unchecked(projectile.Position.X.Fraction), NativeSnapshotMemory.ProjectileXFraction + index);
                Check(owner + " YFraction", unchecked(projectile.Position.Y.Fraction), NativeSnapshotMemory.ProjectileYFraction + index);
                Check(owner + " XVelocity", unchecked((ushort)projectile.Velocity.X.SignedRawValue), NativeSnapshotMemory.ProjectileXVelocity + index);
                Check(owner + " YVelocity", unchecked((ushort)projectile.Velocity.Y.SignedRawValue), NativeSnapshotMemory.ProjectileYVelocity + index);
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
                    SamusProjectilePreInstruction.HyperBeam or
                        SamusProjectilePreInstruction.IceCombo or
                        SamusProjectilePreInstruction.IceComboOutward or
                        SamusProjectilePreInstruction.WaveCombo or
                        SamusProjectilePreInstruction.SpazerCombo or
                        SamusProjectilePreInstruction.PlasmaCombo or
                        SamusProjectilePreInstruction.SpazerComboFalling or
                        SamusProjectilePreInstruction.ShinesparkEcho or
                        SamusProjectilePreInstruction.SpacetimePaletteCopyTail or
                        SamusProjectilePreInstruction.ChainsawPowerBomb or
                        SamusProjectilePreInstruction.ChargedChainsawLowWramExecution or
                        SamusProjectilePreInstruction.MurderBeamMisalignedExecution => throw new InvalidDataException($"Movie projectile callback {projectile.PreInstruction} needs a native identity mapping"),
                    _ => throw new InvalidDataException($"Movie projectile callback {projectile.PreInstruction} needs a native identity mapping"),
                };
                Check(owner + " PreInstruction", callback, NativeSnapshotMemory.ProjectilePreInstruction + index);
                Check(owner + " InstructionTimer", unchecked(projectile.InstructionTimer.RawValue), NativeSnapshotMemory.ProjectileInstructionTimer + index);
                Check(owner + " Variable", unchecked((ushort)projectile.Variable), NativeSnapshotMemory.ProjectileVariable + index);
                Check(owner + " TrailTimer", unchecked(projectile.TrailTimer.RawValue), NativeSnapshotMemory.ProjectileTrailTimer + index);
                Check(owner + " AuxiliaryPhase", unchecked((ushort)projectile.AuxiliaryPhase), NativeSnapshotMemory.ProjectileAuxiliaryPhase + index);
                Check(owner + " Spritemap", unchecked((ushort)projectile.SpritemapPointer), NativeSnapshotMemory.ProjectileSpritemap + index);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            {
                int index = projectile.SlotIndex * 2;
                string owner = $"Enemy projectile {projectile.SlotIndex}";
                Check(owner + " identity", (ushort)projectile.Kind, NativeSnapshotMemory.EnemyProjectileId + index);
                if (!projectile.IsActive || W(NativeSnapshotMemory.EnemyProjectileId + index) == 0) continue;
                Check(owner + " X", projectile.Position.X.Whole, NativeSnapshotMemory.EnemyProjectileX + index);
                Check(owner + " Y", projectile.Position.Y.Whole, NativeSnapshotMemory.EnemyProjectileY + index);
                Check(owner + " Graphics", projectile.GraphicsIndex, NativeSnapshotMemory.EnemyProjectileGraphics + index);
                Check(owner + " Timer", projectile.GeneralTimer, NativeSnapshotMemory.EnemyProjectileTimer + index);
                Check(owner + " PreInstruction", projectile.PreInstructionWord, NativeSnapshotMemory.EnemyProjectilePreInstruction + index);
                Check(owner + " XFraction", projectile.Position.X.Fraction, NativeSnapshotMemory.EnemyProjectileXFraction + index);
                Check(owner + " YFraction", projectile.Position.Y.Fraction, NativeSnapshotMemory.EnemyProjectileYFraction + index);
                Check(owner + " XVelocity", projectile.Velocity.X.RawValue, NativeSnapshotMemory.EnemyProjectileXVelocity + index);
                Check(owner + " YVelocity", projectile.Velocity.Y.RawValue, NativeSnapshotMemory.EnemyProjectileYVelocity + index);
                Check(owner + " Instruction", pointers.Projectile(projectile), NativeSnapshotMemory.NativeEnemyProjectileInstruction + index);
                ushort nativeMap = W(NativeSnapshotMemory.EnemyProjectileSpritemap + index);
                if (checkedProjectileCompositions.Add(new CheckedProjectileComposition(projectile.PresentationOperandAddress, projectile.SpritemapPointer, nativeMap)))
                {
                    var artwork = (runtime.Enemies.TileArtwork ?? throw new InvalidOperationException("Verification requires runtime.Enemies.TileArtwork.")).ProjectileSpritemaps;
                    var parts = projectile.PresentationOperandAddress != 0
                        ? (artwork ?? throw new InvalidOperationException("Verification requires artwork.")).GetProgramFrame(projectile.PresentationOperandAddress)
                        : (artwork ?? throw new InvalidOperationException("Verification requires artwork.")).Get(projectile.SpritemapPointer);
                    var expectedOam = new OamBuffer();
                    var actualOam = new OamBuffer();
                    expectedOam.BeginFrame();
                    actualOam.BeginFrame();
                    DrawImportedEnemyProjectileSpritemap(bus, expectedOam, nativeMap, new(128, 96), 0, true);
                    actualOam.AddEnemySpritemap(parts.Span, new Position16<OamPixels>(128, 96), 0, 0,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    if (actualOam.NextByteOffset != expectedOam.NextByteOffset ||
                        !actualOam.LowTable.SequenceEqual(expectedOam.LowTable) ||
                        !actualOam.HighTable.SequenceEqual(expectedOam.HighTable))
                        mismatches.Add(owner + $" composition differs from native ${nativeMap:X4}");
                }
                Check(owner + " InstructionTimer", projectile.InstructionTimer.RawValue, NativeSnapshotMemory.EnemyProjectileInstructionTimer + index);
                Check(owner + " radii", SnesLittleEndian.Word(unchecked((byte)projectile.Radius.X), unchecked((byte)projectile.Radius.Y)), NativeSnapshotMemory.EnemyProjectileRadius + index);
                ushort properties = checked((ushort)projectile.Damage);
                if (projectile.DrawPriority == EnemyProjectileDrawPriority.High) properties |= NativeSnapshotMemory.EnemyProjectileHighDraw;
                if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
                if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
                if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
                Check(owner + " properties", properties, NativeSnapshotMemory.EnemyProjectileProperties + index);
                ushort variableE = checked((ushort)projectile.Variable0), variableF = checked((ushort)projectile.Variable1);
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
                        variableE = checked((ushort)projectile.RemainingAfterburns);
                        variableF = (ushort)projectile.NextAfterburnKind;
                        break;
                    case RoomEnemyProjectileKind.MiscDustExplosion:
                    case RoomEnemyProjectileKind.EnemyDeathExplosion:
                    case RoomEnemyProjectileKind.EnemyDeathPickup:
                        break;
                    case RoomEnemyProjectileKind.None:
                    case RoomEnemyProjectileKind.PrePhantoonRoom:
                    case RoomEnemyProjectileKind.YappingMawBody:
                    case RoomEnemyProjectileKind.SkreeParticleDownRight:
                    case RoomEnemyProjectileKind.SkreeParticleUpRight:
                    case RoomEnemyProjectileKind.SkreeParticleDownLeft:
                    case RoomEnemyProjectileKind.SkreeParticleUpLeft:
                    case RoomEnemyProjectileKind.MetareeParticleDownRight:
                    case RoomEnemyProjectileKind.MetareeParticleUpRight:
                    case RoomEnemyProjectileKind.MetareeParticleDownLeft:
                    case RoomEnemyProjectileKind.MetareeParticleUpLeft:
                    case RoomEnemyProjectileKind.CrocomireProjectile:
                    case RoomEnemyProjectileKind.CrocomireBridgeCrumbling:
                    case RoomEnemyProjectileKind.CrocomireSpikeWallPieces:
                    case RoomEnemyProjectileKind.KraidSpitRock:
                    case RoomEnemyProjectileKind.KraidCeilingRock:
                    case RoomEnemyProjectileKind.KraidRisingRockLeft:
                    case RoomEnemyProjectileKind.KraidRisingRockRight:
                    case RoomEnemyProjectileKind.PhantoonDestroyableFlame:
                    case RoomEnemyProjectileKind.PhantoonStartingFlame:
                    case RoomEnemyProjectileKind.DraygonGoop:
                    case RoomEnemyProjectileKind.DraygonWallTurret:
                    case RoomEnemyProjectileKind.MotherBrainRoomTurret:
                    case RoomEnemyProjectileKind.MotherBrainRoomTurretBullet:
                    case RoomEnemyProjectileKind.MotherBrainGlassShard:
                    case RoomEnemyProjectileKind.MotherBrainGlassSparkle:
                    case RoomEnemyProjectileKind.MotherBrainOnionRing:
                    case RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion:
                    case RoomEnemyProjectileKind.MotherBrainBomb:
                    case RoomEnemyProjectileKind.MotherBrainHandBeamCharging:
                    case RoomEnemyProjectileKind.MotherBrainHandBeamFired:
                    case RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging:
                    case RoomEnemyProjectileKind.MotherBrainPurpleBreathBig:
                    case RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall:
                    case RoomEnemyProjectileKind.MotherBrainDrool:
                    case RoomEnemyProjectileKind.MotherBrainDyingDrool:
                    case RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion:
                    case RoomEnemyProjectileKind.MotherBrainDeathExplosion:
                    case RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment:
                    case RoomEnemyProjectileKind.MotherBrainEscapeSubtitle:
                    case RoomEnemyProjectileKind.MotherBrainTopRightTube:
                    case RoomEnemyProjectileKind.MotherBrainTopLeftTube:
                    case RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube:
                    case RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube:
                    case RoomEnemyProjectileKind.CeresFallingDebrisLight:
                    case RoomEnemyProjectileKind.CeresFallingDebrisDark:
                    case RoomEnemyProjectileKind.GunshipLiftoffDustCloud:
                    case RoomEnemyProjectileKind.AlcoonFireball:
                    case RoomEnemyProjectileKind.PowampSpike:
                    case RoomEnemyProjectileKind.WorkRobotLaserUpLeft:
                    case RoomEnemyProjectileKind.WorkRobotLaserHorizontal:
                    case RoomEnemyProjectileKind.WorkRobotLaserDownLeft:
                    case RoomEnemyProjectileKind.WorkRobotLaserUpRight:
                    case RoomEnemyProjectileKind.WorkRobotLaserDownRight:
                    case RoomEnemyProjectileKind.FallingSpark:
                    case RoomEnemyProjectileKind.NuclearWaffleBody:
                    case RoomEnemyProjectileKind.FakeKraidSpit:
                    case RoomEnemyProjectileKind.FakeKraidSpikeLeft:
                    case RoomEnemyProjectileKind.FakeKraidSpikeRight:
                    case RoomEnemyProjectileKind.KiHunterAcidSpitLeft:
                    case RoomEnemyProjectileKind.KiHunterAcidSpitRight:
                    case RoomEnemyProjectileKind.PirateMotherBrainLaser:
                    case RoomEnemyProjectileKind.PirateClaw:
                    case RoomEnemyProjectileKind.PolypRock:
                    case RoomEnemyProjectileKind.CacatacSpike:
                    case RoomEnemyProjectileKind.StokeProjectile:
                    case RoomEnemyProjectileKind.NamiheFireball:
                    case RoomEnemyProjectileKind.FuneFireball:
                    case RoomEnemyProjectileKind.LavaThrownByMagdollite:
                    case RoomEnemyProjectileKind.DragonFireball:
                    case RoomEnemyProjectileKind.EyeDoorProjectile:
                    case RoomEnemyProjectileKind.EyeDoorSweat:
                    case RoomEnemyProjectileKind.EyeDoorSmoke:
                    case RoomEnemyProjectileKind.KagoBug:
                    case RoomEnemyProjectileKind.BotwoonBody:
                    case RoomEnemyProjectileKind.BotwoonSpit:
                    case RoomEnemyProjectileKind.BombTorizoLowHealthDrool:
                    case RoomEnemyProjectileKind.BombTorizoInitialDrool:
                    case RoomEnemyProjectileKind.BombTorizoExplosiveSwipe:
                    case RoomEnemyProjectileKind.BombTorizoLowHealthExplosion:
                    case RoomEnemyProjectileKind.BombTorizoDeathExplosion:
                    case RoomEnemyProjectileKind.BombTorizoStatueBreaking:
                    case RoomEnemyProjectileKind.BombTorizoChozoOrb:
                    case RoomEnemyProjectileKind.BombTorizoSonicBoom:
                    case RoomEnemyProjectileKind.GoldenTorizoChozoOrb:
                    case RoomEnemyProjectileKind.GoldenTorizoSonicBoom:
                    case RoomEnemyProjectileKind.BombTorizoRightFootDust:
                    case RoomEnemyProjectileKind.BombTorizoLeftFootDust:
                    case RoomEnemyProjectileKind.GoldenTorizoEgg:
                    case RoomEnemyProjectileKind.GoldenTorizoSuperMissile:
                    case RoomEnemyProjectileKind.GoldenTorizoEyeBeam:
                    case RoomEnemyProjectileKind.TourianStatueSplash:
                    case RoomEnemyProjectileKind.TourianStatueEyeGlow:
                    case RoomEnemyProjectileKind.TourianStatueParticle:
                    case RoomEnemyProjectileKind.TourianStatueTail:
                    case RoomEnemyProjectileKind.TourianStatueSoul:
                    case RoomEnemyProjectileKind.TourianStatueRidley:
                    case RoomEnemyProjectileKind.TourianStatuePhantoon:
                    case RoomEnemyProjectileKind.TourianStatueBaseDecoration:
                    case RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep:
                    case RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate:
                    case RoomEnemyProjectileKind.TourianStatueDescentDust:
                    case RoomEnemyProjectileKind.ShaktoolAttackFrontCircle:
                    case RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle:
                    case RoomEnemyProjectileKind.ShaktoolAttackBackCircle:
                    case RoomEnemyProjectileKind.SporeSpawnStalk:
                    case RoomEnemyProjectileKind.SporeSpawnSpore:
                    case RoomEnemyProjectileKind.SporeSpawnSpawner:
                    case RoomEnemyProjectileKind.SaveStationElectricity:
                    case RoomEnemyProjectileKind.DownwardGateMoving:
                    case RoomEnemyProjectileKind.DownwardGateClosed:
                    case RoomEnemyProjectileKind.NoobTubeCrack:
                    case RoomEnemyProjectileKind.NoobTubeShard:
                    case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
                    default:
                        throw new InvalidDataException($"Movie projectile {projectile.Kind} needs an E/F mapping");
                }
                Check(owner + " variable E", variableE, NativeSnapshotMemory.EnemyProjectileVariableE + index);
                if (projectile.Kind != RoomEnemyProjectileKind.CeresRidleyFireball)
                    Check(owner + " variable F", variableF, NativeSnapshotMemory.EnemyProjectileVariableF + index);
                Check(owner + " variable G", projectile.CollidedProjectileType, NativeSnapshotMemory.EnemyProjectileVariableG + index);
                Check(owner + " collision option", EnemyProjectileCollisionOptionCodec.ToNative(projectile.CollisionOption), NativeSnapshotMemory.EnemyProjectileCollisionOption + index);
                if (projectile.Kind is RoomEnemyProjectileKind.EnemyDeathExplosion or RoomEnemyProjectileKind.EnemyDeathPickup)
                {
                    AssertTrue(projectile.DropChancesOverride is null,
                        "Movie drop uses a direct chance-table override; map its native source before comparing");
                    // Death explosions consume their per-slot header on their later
                    // drop instruction. Direct F337 pickups consume it immediately via
                    // caller X ($86:EF3E/F118), not allocated-slot Y, and never read it
                    // again. Their retained per-slot word is not a live source identity.
                    if (projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion)
                        Check(owner + " source enemy header", SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(projectile.DropDefinitionId), NativeSnapshotMemory.EnemyProjectileEnemyHeader + index);
                    // The native word holds the killed slot's table offset, bit 15 for a respawn, $FFFF for none.
                    KilledEnemy killed = projectile.KilledEnemy;
                    Check(owner + " killed enemy index", killed.SlotIndex < 0 ? 0xffff
                        : killed.SlotIndex * EnemySlotTable.EnemyIndexStride | (killed.Respawns ? 0x8000 : 0),
                        NativeSnapshotMemory.EnemyProjectileKilledEnemy + index);
                }
                ushort nativeDamage = (ushort)(W(NativeSnapshotMemory.EnemyProjectileProperties + index) & 0x0fff);
                if (projectile.Damage != nativeDamage) mismatches.Add(owner + $" damage: native={nativeDamage} port={projectile.Damage}");
            }
            if (mismatches.Count != 0)
            {
                RoomLevelData level = runtime.Room.LevelData ?? throw new InvalidDataException("Missing active room collision data.");
                Console.Error.WriteLine($"Pose history: port={(int)samus.PoseHistory.PreviousPose:X4}/{samus.PoseHistory.PreviousDirectionAndMovement:X4}/{(int)samus.PoseHistory.LastDifferentPose:X4}/{samus.PoseHistory.LastDifferentDirectionAndMovement:X4}, native={W(NativeSnapshotMemory.PreviousPose):X4}/{W(NativeSnapshotMemory.PreviousDirection):X4}/{W(NativeSnapshotMemory.LastDifferentPose):X4}/{W(NativeSnapshotMemory.LastDifferentDirection):X4}");
                Console.Error.WriteLine($"Liquid diagnostic: Y={samus.Position.Y.Whole:X4}, surface={samus.LiquidPhysics.LavaAcidYPosition:X4}, pose={(int)samus.Pose:X2}, radius={samus.Kinematics.Radius.Y}");
                Console.Error.WriteLine($"Shot diagnostic: locked={samus.Control.AlphaLocked}, HUD={samus.SelectedHudItem}, grappleDebug={runtime.Player.DebugGrappleItemSelected}, charge={runtime.Projectiles.Charge.FlareCounter}, cooldown={runtime.BombProjectiles.CooldownTimer.RawValue}, held={runtime.Controller1.Current:X}, new={runtime.Controller1.NewlyPressed:X}");
                Console.Error.WriteLine($"Room width={level.Size.X}, Samus radius={samus.Kinematics.Radius.X}/{samus.Kinematics.Radius.Y}, speed={samus.HorizontalSpeed.BaseSpeed.Whole:X4}.{samus.HorizontalSpeed.BaseSpeed.Fraction:X4}+{samus.HorizontalSpeed.ExtraRunSpeed.Whole:X4}.{samus.HorizontalSpeed.ExtraRunSpeed.Fraction:X4}");
                for (int block = 0; block < level.Size.X * level.Size.Y; block++)
                {
                    ushort expectedBlock = W(NativeSnapshotMemory.Level + block * 2);
                    ushort actualBlock = level.ForegroundEntries.Span[block];
                    if (expectedBlock != actualBlock) Console.Error.WriteLine($"Block {block} ({block % level.Size.X},{block / level.Size.X}): native={expectedBlock:X4} port={actualBlock:X4}");
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
                game.Step(SnesButtons.FromRaw((ushort)updates[frame].GetProperty("input").GetInt32(), $"movie update {frame}"));
            }
        }
        AssertTrue(pendingLoadedOwners is null, "no deferred loading comparison remains at movie end");
        AssertTrue(replayTrace.Records.IsExhausted(), "trace ends after movie terminal frame");
        Console.WriteLine($"Samus body records: {pairedBodyDraws} paired visible updates, {phaseShiftedBodyDraws} hardware-phase-shifted updates, {retainedHiddenBodyRecords} hidden-record retention checks.");
        Console.WriteLine($"Full movie input replay: {length} updates across all 10890 source frames match the currently instrumented fields.");
    }

    /// <summary>Runs the Ridley fireball contact-damage fixture.</summary>
    [VerificationCommand("--ridley-fireball-damage")]
    internal static int RidleyFireballDamageCommand()
    {
     VerifyRidleyFireballDamage(); return 0; 
    }

    /// <summary>Runs the spin fireball attack timing fixture.</summary>
    [VerificationCommand("--ridley-spin-fireball")]
    internal static int RidleySpinFireballCommand()
    {
     VerifyRidleySpinFireball(); return 0; 
    }

    /// <summary>Runs Ridley's contact-order fixture.</summary>
    [VerificationCommand("--ridley-contact-ordering")]
    internal static int RidleyContactOrderingCommand()
    {
     VerifyRidleyContactOrdering(); return 0; 
    }

    /// <summary>Runs the swoop-timer boundary fixture.</summary>
    [VerificationCommand("--ridley-swoop-timer")]
    internal static int RidleySwoopTimerCommand()
    {
     VerifyRidleySwoopTimer(); return 0; 
    }

    /// <summary>Runs the death-finish transition fixture.</summary>
    [VerificationCommand("--ridley-death-finish")]
    internal static int RidleyDeathFinishCommand()
    {
     VerifyRidleyDeathFinish(); return 0; 
    }

    /// <summary>Runs the descending-lunge grab fixture.</summary>
    [VerificationCommand("--ridley-grab-entry")]
    internal static int RidleyGrabEntryCommand()
    {
     VerifyRidleyGrabEntry(); return 0; 
    }

    /// <summary>Runs the fight-entry map initialization fixture.</summary>
    [VerificationCommand("--ridley-map-initialization")]
    internal static int RidleyMapInitializationCommand()
    {
     VerifyRidleyMapInitialization(); return 0; 
    }

    /// <summary>Runs Samus's retained-momentum transition fixture.</summary>
    [VerificationCommand("--samus-retained-horizontal-speed")]
    internal static int SamusRetainedHorizontalSpeedCommand()
    {
     VerifyRetainedHorizontalSpeed(); return 0; 
    }

    /// <summary>Runs the pause/HDMA RNG-order fixture.</summary>
    [VerificationCommand("--pause-dispatch-rng")]
    internal static int PauseDispatchRngCommand()
    {
     VerifyPauseDispatcherRandom(); return 0; 
    }

    /// <summary>Runs the spin fallback pose-history fixture.</summary>
    [VerificationCommand("--spin-fallback-history")]
    internal static int SpinFallbackHistoryCommand()
    {
     VerifySpinFallbackHistory(); return 0; 
    }

    /// <summary>Runs the morph-transition camera checkpoint fixture.</summary>
    [VerificationCommand("--morph-camera-checkpoint")]
    internal static int MorphCameraCheckpointCommand()
    {
     VerifyMorphCameraCheckpoint(); return 0; 
    }

    /// <summary>Runs the aim-up landing-animation fixture.</summary>
    [VerificationCommand("--aim-up-landing-animation")]
    internal static int AimUpLandingAnimationCommand()
    {
     VerifyAimUpLandingAnimation(); return 0; 
    }

    /// <summary>Runs Ridley fireball square-slope collision checks.</summary>
    [VerificationCommand("--ridley-fireball-square-slope")]
    internal static int RidleyFireballSquareSlopeCommand()
    {
     VerifyRidleyFireballSquareSlope(); return 0; 
    }

    /// <summary>Runs Ridley's tail-impact contact fixture.</summary>
    [VerificationCommand("--ridley-tail-impact")]
    internal static int RidleyTailImpactCommand()
    {
     VerifyRidleyTailImpact(); return 0; 
    }

    /// <summary>Runs the offscreen-to-onscreen Ridley gate fixture.</summary>
    [VerificationCommand("--ridley-screen-gate")]
    internal static int RidleyScreenGateCommand()
    {
     VerifyRidleyScreenGate(); return 0; 
    }

    /// <summary>Runs the rolling release and stationary Spring Ball fixtures.</summary>
    [VerificationCommand("--spring-ball-release")]
    internal static int SpringBallReleaseCommand()
    {
     VerifySpringBallRelease(); return 0; 
    }

    /// <summary>Runs tail-segment offset checks.</summary>
    [VerificationCommand("--ridley-tail-offsets")]
    internal static int RidleyTailOffsetsCommand()
    {
     VerifyRidleyTailOffsets(); return 0; 
    }

    /// <summary>Runs Ridley's center-alignment facing fixture.</summary>
    [VerificationCommand("--ridley-center-facing")]
    internal static int RidleyCenterFacingCommand()
    {
     VerifyRidleyCenterFacing(); return 0; 
    }

    /// <summary>Runs pause page and HDMA timing checks.</summary>
    [VerificationCommand("--ridley-pause-page-timing")]
    internal static int RidleyPausePageTimingCommand()
    {
     VerifyRidleyPausePageTiming(); return 0; 
    }

    /// <summary>Runs the Ridley CGRAM palette-selection check.</summary>
    [VerificationCommand("--ridley-palette-selection")]
    internal static int RidleyPaletteSelectionCommand()
    {
     VerifyRidleyPaletteSelection(); return 0; 
    }

    /// <summary>Runs the recorded Ridley door-entry setup check.</summary>
    [VerificationCommand("--ridley-door-entry")]
    internal static int RidleyDoorEntryCommand()
    {
     VerifyRidleyDoorEntry(); return 0; 
    }

    /// <summary>Runs the complete Ridley movie comparison from its imported initial state.</summary>
    [VerificationCommand("--ridley-full-movie")]
    internal static int RidleyFullMovieCommand()
    {
     VerifyRidleyFullMovie(Path.GetFullPath(RidleyTraceDirectory)); return 0; 
    }
}
