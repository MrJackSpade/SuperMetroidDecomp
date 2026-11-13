using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Verification.Bosses.Ridley;

using SuperMetroid.AssetExtraction.Samus;

/// <summary>Focused Ridley fight checks and the headless run of the full recorded-movie gameplay comparison.</summary>
internal static class RidleyFullMovieVerification
{
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
            state.FunctionTimer = 20; samus.Pose = SamusPoseId.SpinJumpRightPose;
            state.BodyPlayback = pointers.BodyPlaybackAt(NativeSnapshotMemory.RidleyRightFlyingSleep);
            body.AnimationClock.HoldFor(9); body.AnimationClock.Repeats = WrappingCountdown.Start(11);
            runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyPogo(body, state, samus, VerticalDirection.Down);
            AssertEqual(sample.Fourth ? RidleyNativeInstructionPointers.BodyEntry(RidleyBodySegment.Fireballing) : NativeSnapshotMemory.RidleyRightFlyingSleep,
                pointers.Body(state), "native spin-response fireball threshold/roar/facing gate");
            AssertEqual(sample.Fourth ? (ushort)1 : (ushort)9, body.AnimationClock.Hold.RawValue, "native fireball restarts instruction timer only when admitted");
            AssertEqual(sample.Fourth ? (ushort)0 : (ushort)11, body.AnimationClock.Repeats.RawValue, "native fireball clears loop counter only when admitted");
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
        body.SpritemapPointer = 0xe9e9; body.Contact = EnemyContact.BlocksPlasmaBeam;
        body.Processing = EnemyProcessing.OffScreen | EnemyProcessing.Animates;
        body.Presence = EnemyPresence.None;
        (state ?? throw new InvalidOperationException("Verification requires state.")).FightMode = RidleyFightMode.Fighting; state.MovementAnimationEnabled = true; state.FacingDirection = RidleyFacing.Left;
        state.Function = RidleyAiFunction.NorfairCarryRelease; state.FunctionTimer = 10;
        state.Velocity = state.Velocity.WithX(new SignedEightEight(0xfc00)); state.Velocity = state.Velocity.WithY(new SignedEightEight(0x0300));
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
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = RidleyAiFunction.NorfairSwoopSetup; state.FunctionTimer = 112;
        body.Position = body.Position.WithWhole(new(212, 200)); state.FacingDirection = RidleyFacing.Right;
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(112, state.FunctionTimer, "native swoop setup retains general AI timer");
        AssertEqual(10, state.SwoopPhaseTimer, "native swoop setup initializes independent countdown");
        state.Function = RidleyAiFunction.NorfairSwoopAimDown; state.SwoopPhaseTimer = 1;
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(0, state.SwoopPhaseTimer, "native swoop decrements its own timer");
        AssertEqual(112, state.FunctionTimer, "native swoop countdown retains general timer");
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(20, state.SwoopPhaseTimer, "native next swoop phase installs its own duration");
        AssertEqual(112, state.FunctionTimer, "native phase transition retains general timer");
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
        (state ?? throw new InvalidOperationException("Verification requires state.")).Function = RidleyAiFunction.NorfairDeathFinish; state.FunctionTimer = 0;
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(RidleyAiFunction.NorfairDeathComplete, state.Function, "death finish selects native C600 return");
        AssertTrue(body.Presence.HasAny(EnemyPresence.Deleted), "death finish publishes deletion");
        AssertTrue(state.BossDefeatPublished && runtime.Enemies.RidleyBehavior.Death.RidleyDeathDropRequested, "death finish publishes boss defeat and drops");
        int random = runtime.System.RandomNumber;
        runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, runtime.Player.Samus, 0, runtime.Room.LevelData);
        AssertEqual(random, runtime.System.RandomNumber, "terminal return does not spawn another drop scatter");
        AssertEqual(-1, state.FunctionTimer, "terminal return retains the countdown expired one update past zero");
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
        (state ?? throw new InvalidOperationException("Verification requires state.")).Velocity = state.Velocity.WithX(new SignedEightEight(0xc0)); state.Velocity = state.Velocity.WithY(new SignedEightEight(0x400));
        state.FacingDirection = RidleyFacing.Right; state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.GrabState = RidleyGrabState.Released; state.FeetDistanceIndex = 0;
        samus.Position = samus.Position.WithWhole(new(198, 402)); samus.Pose = SamusPoseId.KnockbackLeftPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGroundAttack(body, state, samus, runtime.Room.LevelData);
        AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "native grab immediately enters carry movement");
        AssertEqual(31, state.FunctionTimer, "native setup falls through first carry countdown");
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
        state.Velocity = state.Velocity.WithX(new SignedEightEight(0x2b0)); state.Velocity = state.Velocity.WithY(new SignedEightEight(0x3cd));
        state.FacingDirection = RidleyFacing.Right; state.HealthStage = 3; state.HitRoomBoundary = false;
        state.GrabState = RidleyGrabState.Released; state.FeetDistanceIndex = 0;
        samus.Position = samus.Position.WithWhole(new(156, 411)); samus.Pose = SamusPoseId.FacingLeftNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGrabApproach(body, state, samus);
        AssertEqual(RidleyAiFunction.NorfairCarryMoveToAnchor, state.Function, "lunge grab enters immediate carry");
        AssertEqual((ushort)0x2b2, state.Velocity.X.RawValue, "native lunge grab X acceleration");
        AssertEqual((ushort)0xfc25, state.Velocity.Y.RawValue, "native lunge grab reverses before carry acceleration");
        AssertEqual(31, state.FunctionTimer, "native lunge grab falls through countdown");
        // Source10132 takes the zero-health branch, whose JMP executes C538 immediately.
        body.Position = new(new(134, 0x7d00), new(356, 0xec00)); body.Health = 0;
        state.Velocity = state.Velocity.WithX(new SignedEightEight(0x1de)); state.Velocity = state.Velocity.WithY(new SignedEightEight(0x419));
        state.FightMode = RidleyFightMode.Fighting; state.GrabState = RidleyGrabState.Released; state.FunctionTimer = 0x29;
        samus.Position = samus.Position.WithWhole(new(134, 411)); samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        runtime.Enemies.RidleyBehavior.Combat.TickNorfairRidleyGrabApproach(body, state, samus);
        AssertEqual(RidleyAiFunction.NorfairReleaseSamus, state.Function, "zero-health grab enters death movement");
        AssertEqual((ushort)0x1c6, state.Velocity.X.RawValue, "death entry immediately accelerates X toward death spot");
        AssertEqual((ushort)0xfbde, state.Velocity.Y.RawValue, "death entry immediately accelerates Y toward death spot");
        AssertEqual(0x29, state.FunctionTimer, "death entry retains existing general countdown");
        foreach (var function in new[] { RidleyAiFunction.NorfairCarryRise, RidleyAiFunction.NorfairCarryRelease })
        {
            state.Function = function; state.FunctionTimer = 0;
            state.Velocity = state.Velocity.WithX(new SignedEightEight(0xffca)); state.Velocity = state.Velocity.WithY(new SignedEightEight(0xfecb));
            state.Target = state.Target.WithX(208); body.Position = body.Position.WithWhole(new(212, 142));
            runtime.Enemies.RidleyBehavior.RunNorfairRidleyFunction(body, state, samus, 0, runtime.Room.LevelData);
            AssertEqual((ushort)0xffca, state.Velocity.X.RawValue, "native expiry skips X acceleration");
            AssertEqual((ushort)0xfecb, state.Velocity.Y.RawValue, "native expiry skips Y acceleration");
            AssertEqual(function == RidleyAiFunction.NorfairCarryRise ? RidleyAiFunction.NorfairCarryRelease : RidleyAiFunction.NorfairSelectAttack, state.Function, "native expiry selects next carry phase");
            AssertEqual((ushort)(function == RidleyAiFunction.NorfairCarryRise ? 8 : 16), state.IdealInterSegmentTailAngle, "native expiry changes ideal tail angle");
            AssertEqual(SignedEightEight.FromParts(0, 240), state.TailExtensionSpeed, "native expiry restores tail extension speed");
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
            foreach (AreaId area in Enum.GetValues<AreaId>())
                foreach (Position16<MapTiles> cell in runtime.System.Progress.ExploredCells(area).ToList())
                    runtime.System.Progress.SetExplored(area, cell, explored: false);
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
                    AssertEqual(MapTileWords.HudBlank.Raw, runtime.Hud.TileWords(runtime.InstalledHudArtwork)[26 + y * HudStateConstants.WidthInTiles + x],
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
        var room = CreateEmptyRoom(new(16, 16));
        foreach (int branch in new[] { 0, 1, 2, 3 })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
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
            samus.HorizontalSpeed.CalculateTotalSpeed(UnsignedSixteenSixteen.FromRaw(0x00054321));
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
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        // The recorded pause enters an already-running room HDMA callback.
        runtime.RoomFxOverlay.AdvanceLiquidMotion(runtime.System, false);
        foreach (var state in new[] { SuperMetroidGameState.Pausing, SuperMetroidGameState.PauseScreenFadeIn,
            SuperMetroidGameState.Paused, SuperMetroidGameState.PauseScreenFadeOut, SuperMetroidGameState.RestoringGameplayFromPause,
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
            SamusMovementType spinMovement = SamusMovementType.SpinJumping;
            samus.PoseHistory.PreviousMovement = spinMovement;
            samus.PoseHistory.LastDifferentPose = SamusPoseId.RunningAimUpRightPose;
            samus.PoseHistory.LastDifferentMovement = SamusMovementType.Running;
            runtime.StepFrame(0);
            AssertEqual(pose, samus.PoseHistory.PreviousPose, "fallback retains spin pose");
            AssertEqual(pose, samus.PoseHistory.LastDifferentPose, "fallback commits same-pose history");
            AssertEqual(spinMovement, samus.PoseHistory.LastDifferentMovement, "fallback publishes older spin movement");
            AssertTrue(samus.PoseHistory.AllowsWallJumpProbe, "next update admits native wall observation");
        }
        Console.WriteLine("Spin fallback history: ordinary, Space Jump and Screw Attack both facings retain pose and admit wall probe.");
    }

    /// <summary>Checks the camera checkpoint selected during Samus's morph transition.</summary>
    internal static void VerifyMorphCameraCheckpoint()
    {
        var level = CreateEmptyRoom(new(16, 32));
        foreach (bool left in new[] { false, true })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = left ? SamusPoseId.FallingAimDownLeftPose : SamusPoseId.FallingAimDownRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 402)),
                EquippedItems = SamusEquipmentFlags.MorphBall,
            };
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x97ff);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            var previous = FixedPosition<RoomPixels>.FromStoredParts(128, 0, 401, 0xb7ff);
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
        var level = CreateEmptyRoom(new(16, 32));
        foreach (SamusPoseId pose in new[] { SamusPoseId.FallingAimUpRightPose, SamusPoseId.FallingAimUpLeftPose })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 200)) };
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
        projectile.Velocity = new(new SignedEightEight(0xfb64), new SignedEightEight(0xfe1b));
        projectile.Radius = new(6, 6);
        AssertTrue(!EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Horizontal), "source1248 fireball crosses empty square-slope quadrant horizontally");
        AssertTrue(!EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Vertical), "source1248 fireball clears terrain vertically");
        AssertEqual((ushort)0x44, projectile.Position.X.Whole, "native source1249 fireball X");
        AssertEqual((ushort)0xf000, projectile.Position.X.Fraction, "native source1249 fireball X fraction");
        AssertEqual((ushort)0x52, projectile.Position.Y.Whole, "native source1249 fireball Y");
        AssertEqual((ushort)0xf400, projectile.Position.Y.Fraction, "native source1249 fireball Y fraction");
        projectile.Position = projectile.Position.WithWholeX(60).WithFractionX(0).WithWholeY(84); projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(0xff00));
        AssertTrue(EnemyProjectileEngine.MoveProjectileAxis(projectile, runtime.Room.LoadedLevelData, Axis.Horizontal), "occupied top-left quarter stops horizontal motion");
        AssertEqual((ushort)62, projectile.Position.X.Whole, "native square reaction snaps to eight-pixel edge even when initially embedded");
        projectile.Position = projectile.Position.WithWholeX(52).WithWholeY(94).WithFractionY(0); projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(0xff00));
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
            runtime.Enemies.RidleyProjectiles.SpawnDirectionalAfterburn(fireball, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(3584, 0));
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
        AssertTrue(dust.Animation is MiscDustProjectileAnimation { PlayingSegment: MiscDustProjectileSegment.SmallDustCloud }, "native impact selects variant nine");
        AssertTrue(runtime.Enemies.Audio.SoundRequests.Any(request => request.SoundEffect == EnemySound.Quake && request.Priority.Requests == 6), "native impact queues the quake sound");
        Console.WriteLine("Ridley tail impact: native dust slot, position, instruction variant and sound pass.");
    }

    /// <summary>Checks that Ridley enters his screen gate only after the prior Y position becomes visible.</summary>
    internal static void VerifyRidleyScreenGate()
    {
        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(64, 253))};
        var state = new RidleyEnemyState { FightMode = RidleyFightMode.Fighting, IntangibilityTimer = 3 };
        // Original source 3062/3063: pre-movement Y253 is outside camera Y287.
        RidleyInteractions.UpdateNorfairRidleyIntangibility(body, state, new Position16<RoomPixels>(0, 287));
        AssertTrue(body.Contact.HasAny(EnemyContact.IgnoresSamus), "native above-camera Ridley disables collision");
        AssertEqual(3, state.IntangibilityTimer, "offscreen return preserves release timer");
        // At the next AI call Y257 is inside. EnemyMain already tested the old
        // collision bit; the cleared gate is consumed by the following update.
        body.Position = body.Position.WithWholeY(257);
        RidleyInteractions.UpdateNorfairRidleyIntangibility(body, state, new Position16<RoomPixels>(0, 287));
        AssertTrue(!body.Contact.HasAny(EnemyContact.IgnoresSamus), "native reentry clears collision gate");
        AssertEqual(2, state.IntangibilityTimer, "onscreen AI resumes release timer");
        AssertTrue(RidleyCollision.IsOutsideInteractionWindow(new(64, 254), new(0, 287)), "one pixel above native window");
        AssertTrue(!RidleyCollision.IsOutsideInteractionWindow(new(64, 255), new(0, 287)), "native upper edge is admitted");
        AssertTrue(RidleyCollision.IsOutsideInteractionWindow(new(64, 543), new(0, 287)), "native lower edge is excluded");
        AssertTrue(!RidleyCollision.IsOutsideInteractionWindow(new(64, 542), new(0, 287)), "last lower pixel is admitted");
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
        runtime.Controller1.Latch(ControllerButton.Left);
        // Original movie source 1616 -> 1617: release Left while rolling.
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.Position.X.RawValue, "spring release retains native final displacement");
        AssertEqual(SamusPoseId.SpringBallGroundLeftPose, samus.Pose, "spring release selects stationary pose immediately");
        AssertEqual(0u, samus.HorizontalSpeed.BaseSpeed.RawValue, "spring release clears base momentum after movement");
        AssertEqual(SamusAccelerationMode.Accelerating, samus.HorizontalSpeed.AccelerationMode, "spring release clears acceleration mode");
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.Position.X.RawValue, "released spring ball stays stopped next update");
        // Original source 1983: stationary Spring Ball keeps command six even
        // while the hurt mover calculates a fresh fractional base speed.
        samus.Pose = SamusPoseId.SpringBallGroundRightPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus); SamusPresentation.InitializeAnimation(samus);
        samus.Position = new(new(202, 0x8000), new(425, ushort.MaxValue));
        samus.KnockbackActive = true; samus.KnockbackDirection = SamusKnockbackDirection.UpRight;
        samus.KnockbackXDirection = KnockbackHorizontalDirection.Right; samus.KnockbackTimer = 4;
        samus.InvincibilityTimer = WrappingCountdown.Start(95);
        samus.Kinematics.YDirection = SamusVerticalDirection.Up; samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithWhole(5); samus.Kinematics.YSpeed = samus.Kinematics.YSpeed.WithFraction(0);
        runtime.StepFrame(0);
        AssertEqual(0x00cb4000u, samus.Kinematics.Position.X.RawValue, "stationary spring hurt frame preserves native displacement");
        AssertEqual(0u, samus.HorizontalSpeed.BaseSpeed.RawValue, "stationary spring fallback clears hurt mover base speed");
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
            TailMinimumClockwiseAngle = RidleyTailAngle.StraightDown.TurnedClockwise(64), TailMaximumCounterClockwiseAngle = RidleyTailAngle.StraightDown.TurnedCounterclockwise(16),
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
        segment.StaggerAngle = null; segment.Angle = RidleyTailAngle.StraightDown;
        segment.Clockwise = true; segment.Distance = SignedEightEight.FromParts(2, 0);
        RidleyTail.TickRidleyTailSegment(state, 0);
        AssertEqual(RidleyTailAngle.StraightDown.TurnedClockwise(2), segment.Angle, "a clockwise step short of the limit turns by the whole step");
        AssertEqual((short)-1, segment.Offset.X, "signed Mode 7 multiplication floors negative fraction");
        AssertEqual((short)1, segment.Offset.Y, "signed Mode 7 cosine product");
        state.ClockwiseWhipTarget = RidleyTailAngle.StraightDown;
        segment.Angle = RidleyTailAngle.StraightDown; segment.Offset = new Offset16<RoomPixels>(7, 11);
        RidleyTail.TickRidleyTailSegment(state, 0);
        AssertTrue(!segment.Active, "root stops at whip target");
        AssertEqual((short)7, segment.Offset.X, "stop retains X offset");
        AssertEqual((short)11, segment.Offset.Y, "stop retains Y offset");
        var child = state.TailSegments[1];
        segment.Active = true;
        child.Active = true; child.StaggerAngle = null;
        child.Angle = RidleyTailAngle.StraightDown; child.Clockwise = true; child.Distance = SignedEightEight.FromParts(8, 0);
        RidleyTail.TickRidleyTailSegment(state, 1);
        AssertTrue(child.Active, "moving predecessor prevents child deactivation");
        AssertEqual(RidleyTailAngle.StraightDown.TurnedClockwise(64), child.Angle, "blocked child clamps to native clockwise limit");
        foreach (var part in state.TailSegments)
        {
            part.Distance = SignedEightEight.FromParts(18, 0); part.TargetDistance = SignedEightEight.FromParts(12, 0);
            part.Angle = RidleyTailAngle.StraightDown.TurnedClockwise(8); part.Offset = new Offset16<RoomPixels>(-3, 13);
        }
        RidleyTail.MirrorRidleyTail(state);
        int[] restPixels = [2, 8, 8, 8, 8, 8, 5];
        for (int i = 0; i < state.TailSegments.Length; i++)
        {
            var part = state.TailSegments[i];
            AssertEqual(SignedEightEight.FromParts(restPixels[i], 0), part.Distance, "native flip restores each authored rest distance");
            AssertEqual(RidleyTailAngle.StraightDown.TurnedCounterclockwise(8), part.Angle, "native flip reflects angle about straight down");
            AssertEqual<SignedEightEight?>(SignedEightEight.FromParts(12, 0), part.TargetDistance, "native flip preserves extension target");
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
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(sample.X, 0)) }
                .WithAnimationClock(hold: 7, repeats: 9);
            var state = new RidleyEnemyState { FacingDirection = sample.Facing };
            state.BodyPlayback = pointers.BodyPlaybackAt(NativeSnapshotMemory.RidleyRightFlyingSleep);
            NorfairRidleyCombat.SelectNorfairRidleyFacingAnimation(slot, state);
            ushort expected = !sample.Turn ? NativeSnapshotMemory.RidleyRightFlyingSleep
                : sample.Facing == RidleyFacing.Left ? RidleyNativeInstructionPointers.BodyEntry(RidleyBodySegment.TurnFromLeftToRight)
                : RidleyNativeInstructionPointers.BodyEntry(RidleyBodySegment.TurnFromRightToLeft);
            AssertEqual(expected, pointers.Body(state), $"native center-facing instruction at {sample.X:X4}, facing {sample.Facing}");
            AssertEqual(sample.Turn ? (ushort)2 : (ushort)7, slot.AnimationClock.Hold.RawValue, "turn timer changes only when native condition is met");
            AssertEqual(sample.Turn ? (ushort)0 : (ushort)9, slot.AnimationClock.Repeats.RawValue, "loop counter is preserved when no turn is needed");
        }
        var hoverState = new RidleyEnemyState { FunctionTimer = 0, Function = RidleyAiFunction.NorfairHover };
        NorfairRidleyCombat.TickNorfairRidleyHover(new RoomEnemySlot(0, EnemySlotFixtures.Registry), hoverState);
        AssertEqual(-1, hoverState.FunctionTimer, "native hover counts zero one update past it before selecting next attack");
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, hoverState.Function, "negative hover timer exits before movement");
        Console.WriteLine("Ridley center facing: movie trigger, both sides/directions, mid-turn and native low-byte boundary agree.");
    }

    /// <summary>Checks pause page and HDMA timing across Ridley's fight.</summary>
    internal static void VerifyRidleyPausePageTiming()
    {
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { MaxReserveEnergy = 300, ReserveEnergy = 300, ReserveTankMode = ReserveTankMode.Auto };
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Norfair, RoomMapPosition = new(0, 0), MapPresentation = RetailPresentationFixture(), SpriteSheets = MenuSheetFixtures.For(RetailPresentationFixture()) });
        foreach (PauseScreenMode source in Enum.GetValues<PauseScreenMode>())
        {
            PauseScreenMode destination = source == PauseScreenMode.Map ? PauseScreenMode.Equipment : PauseScreenMode.Map;
            menu.Step((source == PauseScreenMode.Map ? ControllerButton.R : ControllerButton.L), 0);
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
    internal static void VerifyRidleyPaletteSelection()
    {
        var random = new SharedRandom();
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
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(runtime.Player.Colours, samus.EquippedItems);
        // Samus's suit colours are her shades (#1512): the selection leaves the suit showing the shade its load named.
        RgbColor[] expected = runtime.Player.Colours.ShownSuit(InstalledFixtures.SamusShadeSources);
        runtime.StepFrame(ControllerButton.Select);
        AssertEqual(HudItem.SuperMissile, samus.SelectedHudItem, "selection enters Super Missiles without charge");
        AssertTrue(runtime.Player.Colours.ShownSuit(InstalledFixtures.SamusShadeSources).SequenceEqual(expected),
            "HUD handler restores all suit colors even when charge was zero");
        Console.WriteLine("HUD selection restores the complete suit palette without an active charge.");
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
        game.AdoptRuntime(runtime);
        game.GameState = SuperMetroidGameState.Pausing;
        game.Step(0);
        var menu = game.Session.ScreenAs<PauseMenuState>();
        // The pause screen's colours are the shadings its pictures name (#1512): the highlight frame and the reserve arrow's.
        PauseMenuState pause = menu ?? throw new InvalidOperationException("Verification requires menu.");
        RgbColor[] initial = PauseShownColours.Of(pause);
        int fadeFrames = 0;
        while (game.GameState == SuperMetroidGameState.PauseScreenFadeIn && fadeFrames++ < 32)
        {
            game.Step(0);
            AssertTrue(PauseShownColours.Of(pause).SequenceEqual(initial), "pause fade-in retains all palette colors");
        }
        AssertEqual(SuperMetroidGameState.Paused, game.GameState, "pause fade-in completes");
        AssertEqual(SuperMetroid.Core.Assets.PictureShading.Shaded(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteShades.Highlight(0)),
            pause.mapHighlight.Shading, "before the highlight cycle's first step the pause arrows show the cycle's frame zero");
        game.Step(0);
        AssertEqual(SuperMetroid.Core.Assets.PictureShading.Shaded(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteShades.Highlight(1)),
            pause.mapHighlight.Shading, "the first stable pause step shows the highlight cycle's frame one");
        RgbColor[] animated = PauseShownColours.Of(pause);
        game.GameState = SuperMetroidGameState.PauseScreenFadeOut;
        game.Session.BeginPauseFade(15);
        fadeFrames = 0;
        while (game.GameState == SuperMetroidGameState.PauseScreenFadeOut && fadeFrames++ < 32)
        {
            game.Step(0);
            AssertTrue(PauseShownColours.Of(pause).SequenceEqual(animated), "unpause fade-out retains the last palette phase");
        }
        AssertEqual(SuperMetroidGameState.RestoringGameplayFromPause, game.GameState, "unpause fade-out completes");
        Console.WriteLine("Pause palette advances only in stable pause, remaining latched through both outer fades.");
    }

    /// <summary>Checks door-entry setup, queued HDMA and palette state at the Ridley room boundary.</summary>
    internal static void VerifyRidleyDoorEntry()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new SharedRandom();
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
            var door = level.ResolveDoorCollision(block.Behavior, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), false);
            if (door.Door?.DestinationRoom != RoomHeaderDefinitions.Ridley) continue;
            level.ResolveDoorCollision(block.Behavior, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), true);
            found = true;
        }
        AssertTrue(found, "native Ridley entry door");
        // Original movie: accepted main-loop samples 155, 156, 157. The source
        // acid callback was installed long before the recorded door collision.
        runtime.RoomFxOverlay.AdvanceLiquidMotion(runtime.System, false);
        runtime.System.SetRandomNumber(0xd562);
        runtime.Clock.Import(runtime.Clock.ShortCounter, 0xa4df);
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
        pipeBug.Playback = pipeBugPointers.PlaybackAt(NativeSnapshotMemory.PipeBugBeforeFadeInstruction);
        actor.AnimationClock.StepOnNextUpdate();
        actor.SpritemapPointer = NativeSnapshotMemory.PipeBugBeforeFadeSpritemap;
        game.Step(0);
        AssertEqual(0xadd4, runtime.System.RandomNumber, "first source fade advances native HDMA/RNG");
        AssertEqual(0xa4e2, runtime.Clock.Counter, "fade accepts exactly one NMI per update");
        AssertEqual(NativeSnapshotMemory.PipeBugAfterFadeInstruction, pipeBugPointers.Pointer(pipeBug.Playback), "fade advances the native enemy instruction");
        AssertEqual(NativeSnapshotMemory.PipeBugAfterFadeSpritemap, actor.SpritemapPointer, "fade changes to the native enemy sprite");
        AssertEqual((ushort)2, actor.AnimationClock.Hold.RawValue, "fade installs the native visual duration");
        int fadeSteps = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FadeOutSourcePalette && fadeSteps++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.BeginDoorDisplay, game.DoorTransitionPhaseForVerification, "source palette fade finishes");
        AssertEqual(0xe19e, runtime.System.RandomNumber, "native update 172 fade endpoint RNG");
        // Original native input-boundary records 173..178. The first dispatch
        // still runs source HDMA; beginning the door's display disables it for the following ones.
        ushort[] nativeLoadingRandom = [0x1b76, 0x8a5f, 0xb4ec, 0x89ad, 0xb172, 0x784b];
        // #1631: each of these updates is one stage, and the destination-setup wait spends exactly two of them
        // (updates 175 and 176), each drawing its recorded random number.
        DoorTransitionPhase[] loadingPhases =
        [
            DoorTransitionPhase.AlignSourceCamera,
            DoorTransitionPhase.DestinationSetupWait,
            DoorTransitionPhase.DestinationSetupWait,
            DoorTransitionPhase.MoveToDoorScrollStart,
            DoorTransitionPhase.PlaceSamusInDestination,
            DoorTransitionPhase.LoadDestinationRoom,
        ];
        for (int index = 0; index < nativeLoadingRandom.Length; index++)
        {
            game.Step(0);
            AssertEqual(nativeLoadingRandom[index], runtime.System.RandomNumber, $"native loading RNG update {173 + index}");
            AssertEqual(loadingPhases[index], game.DoorTransitionPhaseForVerification, $"loading update {173 + index} leaves its stage");
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
        AssertEqual(0, runtime.Clock.ShortCounter, "door end-drawing IRQ clears the adjacent byte counter");
        ushort[] beamPalette = ReadRetailBeamPalette(bus, samus.EquippedBeams.ToEquipmentWord());
        // The colours the frame shows, composed from the published shading and the import's colours (#1512).
        RgbColor[] scrolling = GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources);
        for (int color = 0; color < 16; color++)
            AssertEqual(beamPalette[color], scrolling[GameplayShownColours.SamusBeamsFirst + color].ToWord(), "beam palette is live during door scrolling");
        AssertEqual(0, scrolling[XrayScopeDefinitionsPaletteConstants.VisorCgramIndex].ToWord(), "visor remains faded before scroll completes");
        int scrollCalls = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForDoorOpeningScroll && scrollCalls < 64)
        {
            game.Step(0);
            AssertEqual(0, runtime.Clock.ShortCounter, "every moving door IRQ renews the byte-counter reset");
            scrollCalls++;
        }
        AssertEqual(61, scrollCalls, "left trajectory completes on its 61st remaining IRQ call");
        AssertEqual(DoorTransitionPhase.AlignSamusWithDoorway, game.DoorTransitionPhaseForVerification, "post-scroll NMI remains inside the loading coroutine");
        AssertEqual(0x3be0, GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources)[XrayScopeDefinitionsPaletteConstants.VisorCgramIndex].ToWord(),
            "final scrolling update publishes native visor green before PLMs");
        AssertEqual(0x00de2000u, samus.Kinematics.Position.X.RawValue, "native source frame 276 scrolling endpoint");
        game.Step(0);
        AssertEqual(DoorTransitionPhase.StepDestinationAnimatedTiles, game.DoorTransitionPhaseForVerification, "loading alignment precedes music wait");
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
        AssertEqual(DoorTransitionPhase.FinishDoorOpeningScroll, game.DoorTransitionPhaseForVerification, "native music queue reaches final door dispatch");
        int nmiBeforeNudge = runtime.Clock.Counter;
        int samusAnimation = samus.AnimationFrame;
        var ridley = runtime.Enemies.SlotTable.Slots[0];
        var pointers = new RidleyNativeInstructionPointers(CartridgeImportSource.Require(bus));
        ushort bodyInstruction = pointers.Enemy(runtime.Enemies, ridley);
        game.Step(0);
        AssertEqual(DoorTransitionPhase.DrawFirstDestinationFrame, game.DoorTransitionPhaseForVerification, "final nudge returns before first fade");
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 1)), runtime.Clock.Counter, "final transition accepts exactly one NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "final transition does not animate Samus");
        AssertEqual(bodyInstruction, pointers.Enemy(runtime.Enemies, ridley), "final transition does not advance destination enemy instructions");
        AssertEqual(0, runtime.Clock.ShortCounter, "final door scanout resets the byte before IRQ ownership changes");
        game.Step(0);
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 2)), runtime.Clock.Counter, "first fade accepts exactly one additional NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "destination fade does not animate Samus");
        AssertEqual(1, runtime.Clock.ShortCounter, "first destination drawing frame resumes the byte counter");
        AssertEqual(NativeSnapshotMemory.RidleyFirstFadeInstruction, pointers.Enemy(runtime.Enemies, ridley), "first fade runs native Ridley instruction list");
        AssertEqual(NativeSnapshotMemory.RidleyFirstFadeSpritemap, ridley.SpritemapPointer, "first fade publishes native Ridley sprite");
        AssertEqual((ushort)12, ridley.AnimationClock.Hold.RawValue, "first native fade visual duration");
        AssertEqual(RidleyAiFunction.WaitForDoorTransition, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).Function, "native Ridley AI remains gated during fade visuals");
        AssertEqual(0, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).FunctionTimer, "fade does not consume the reveal countdown");
        game.Step(0);
        AssertEqual((ushort)11, ridley.AnimationClock.Hold.RawValue, "later fade updates continue enemy animation");
        int remainingFade = 0;
        while (game.GameState == SuperMetroidGameState.DoorTransition && remainingFade++ < 32)
        {
            game.Step(0);
            AssertEqual(RidleyAiFunction.WaitForDoorTransition, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).Function, "reveal timer remains gated through the final fade dispatch");
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "fade releases ordinary gameplay");
        AssertTrue(!runtime.Enemies.RoomStatus.EnemyDoorTransitionActive, "completed fade clears enemy gate");
        game.Step(0);
        AssertEqual(RidleyAiFunction.InitialDelay, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).Function, "first gameplay update begins reveal");
        AssertEqual(169, (runtime.Enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires the loaded Ridley.")).FunctionTimer, "native first gameplay update consumes one of 170 reveal ticks");
        Console.WriteLine("Ridley door entry/loading/fade: native positions, RNG, one NMI per dispatch, stationary Samus animation and continuing enemy visuals agree.");
    }

    /// <summary>The 16 native beam colors $90:C3C9 selects for <paramref name="equippedBeams"/>, BGR555 only.</summary>
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
    internal static ushort ReadRetailObjectProgramWord(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>
    /// Replays the complete supplied Ridley movie through production gameplay and compares accepted update state with
    /// native checkpoints (<see cref="RidleyShowcaseReplay"/>), stepping the session headlessly.
    /// </summary>
    /// <param name="directory">The movie's compact native trace.</param>
    internal static void VerifyRidleyFullMovie(string directory) => HeadlessMovieReplay.Run(ReplayMovieId.Ridley, directory);

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
     VerifyRidleyFullMovie(Path.GetFullPath(ReplayMovieFixture.Ridley.TraceDirectory)); return 0; 
    }
}
