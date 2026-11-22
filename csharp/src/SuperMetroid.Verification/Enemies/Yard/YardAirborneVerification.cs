using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Saves;
namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>Checks Yard kick distance publication and airborne fixed-point trajectories.</summary>
internal static class YardAirborneVerification
{
    /// <summary>
    /// Confirms the runtime's scrolling records the camera's measure of Samus's sideways movement, with its added pixel and
    /// fraction, in the runtime's Yard kick camera measure (#1962).
    /// </summary>
    internal static void VerifyYardRuntimeDistancePublication()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.StepFrame(0);
        AssertEqual(SignedSixteenSixteen.FromWholePixels(1), runtime.YardKickDistance.HorizontalDistancePlusOne,
            "a stationary Samus's measure includes one pixel for the next Yard kick");
        runtime.Player.Samus.Kinematics.Position = runtime.Player.Samus.Kinematics.Position.WithFractionX(0xE000);
        runtime.StepFrame(0);
        AssertEqual(SignedSixteenSixteen.FromParts(1, 57344), runtime.YardKickDistance.HorizontalDistancePlusOne,
            "the Yard kick receives the camera's full fractional measure from its previous tracking point");
        runtime.Player.Samus.Kinematics.Position = runtime.Player.Samus.Kinematics.Position.WithFractionX(0xC000);
        runtime.StepFrame(0);
        AssertEqual(SignedSixteenSixteen.FromParts(0, 57344), runtime.YardKickDistance.HorizontalDistancePlusOne,
            "the whole-pixel direction test leaves a measure below one pixel");
    }

    /// <summary>
    /// Confirms every initialisation of a new Samus records the one-pixel camera measure her initialisation leaves (#1990),
    /// replacing whatever measure the runtime held: starting at Ceres, arriving on Zebes after Ceres, loading a saved game
    /// and loading an attract demo. A kick before the camera next tracks her therefore flies at one pixel an update.
    /// </summary>
    internal static void VerifyYardKickDistanceStartsAtSamusInitialization()
    {
        SignedSixteenSixteen initial = SignedSixteenSixteen.FromWholePixels(1);
        SignedSixteenSixteen stale = SignedSixteenSixteen.FromParts(3, 0x8000);

        var runtime = CreateRetailRuntimeFixture();
        // A runtime holds its Samus from its construction (#2554), whose initialisation leaves the one-pixel measure.
        AssertEqual(initial, runtime.YardKickDistance.HorizontalDistancePlusOne,
            "a runtime starts with the measure its first Samus's initialisation leaves");
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.YardKickDistance.Record(stale);
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        AssertEqual(initial, runtime.YardKickDistance.HorizontalDistancePlusOne, "starting at Ceres initialises Samus with one pixel");
        runtime.YardKickDistance.Record(stale);
        runtime.RoomLoader.InitializePostCeresZebesRoom();
        AssertEqual(initial, runtime.YardKickDistance.HorizontalDistancePlusOne,
            "arriving on Zebes after Ceres initialises Samus with one pixel");

        var snapshot = SaveSnapshotFixtures.NewGame with { Checkpoint = LoadStationDefinitions.CrateriaSave };
        var saves = new SaveSlotStore(RetailPresentationFixtureVerification.RetailPresentationFixture());
        saves.SaveSlot(SaveSlot.A, snapshot);
        SuperMetroidSaveSlot slot = saves.ReadSlot(SaveSlot.A) ?? throw new InvalidDataException("The Yard kick fixture save failed validation.");
        var loaded = CreateRetailRuntimeFixture();
        loaded.YardKickDistance.Record(stale);
        loaded.RoomLoader.InitializeSavedGame(slot);
        AssertEqual(initial, loaded.YardKickDistance.HorizontalDistancePlusOne, "loading a saved game initialises Samus with one pixel");

        var attract = CreateRetailRuntimeFixture();
        attract.YardKickDistance.Record(stale);
        attract.AttractDemo.InitializeAttractDemo(StockAttractDemoScenes.Playlist(completionSetUnlocked: false)[0].Scenes[0]);
        AssertEqual(initial, attract.YardKickDistance.HorizontalDistancePlusOne, "loading an attract demo initialises Samus with one pixel");
    }

    /// <summary>
    /// Confirms a kicked Yard in a room the runtime loads flies off with the camera measure the runtime's scrolling recorded
    /// before the load (#1962): the room loader hands the runtime's measure to the room's Yards, a door load keeps it, and the
    /// kick takes it, fraction and all, as its sideways speed and its whole pixels for its upward speed.
    /// </summary>
    internal static void VerifyYardKickTakesTheCameraDistance()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.StepFrame(0);
        runtime.Player.Samus.Kinematics.Position = runtime.Player.Samus.Kinematics.Position.WithFractionX(0xE000);
        runtime.StepFrame(0);
        SignedSixteenSixteen measured = SignedSixteenSixteen.FromParts(1, 57344);
        AssertEqual(measured, runtime.YardKickDistance.HorizontalDistancePlusOne, "the scrolling recorded the camera's measure");

        SelectedRoom aqueduct = runtime.RoomLoader.LoadSelectedRoom(RoomDefinitions.Aqueduct);
        runtime.RoomLoader.LoadRoom(MaridiaDoorDefinitions.CrabShaftToAqueduct, aqueduct, new Position16<RoomPixels>(0, 0),
            RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn);
        AssertEqual(measured, runtime.YardKickDistance.HorizontalDistancePlusOne, "the door load keeps the camera's measure");

        RoomEnemySystem enemies = runtime.Room.LoadedScope.Enemies;
        RoomEnemyPlace actor = enemies.Places.InUpdateOrder.First(place => place.Holds(EnemyDefinitionId.Yard));
        // The Yard's setup, run on the Yard the room placed, builds the state its touch reaction kicks.
        YardEnemyState state = YardEnemyBehavior.InitializeYard(actor.HeldOccupant().SpawnSettings<YardSpawnSettings>(), actor);
        SamusState samus = runtime.Player.Samus;
        samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        state.Motion = new YardMotion.Airborne(YardDirections.ForAirborneFacing(state.AirborneFacingDirection).HidingAnimation);
        state.Behavior = YardBehavior.NormalCrawling;
        samus.Position = samus.Position.WithWhole(actor.Position.Whole);
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        // The Yard's own touch reaction kicks it, at the camera's measure the scrolling recorded.
        enemies.YardBehavior.ReactToYardTouch(actor.HeldOccupant(), actor.HeldOccupant().SpawnSettings<YardSpawnSettings>(),
            new EnemyTouchContext(actor.HeldOccupant(), samus, RoomEnemyFrameFixtures.StandalonePseudoScrew(), default, runtime.Room.LoadedLevelData,
                UsesFrameHitboxes: false, HitboxTouch: actor.Definition.Touch, Position16<RoomPixels>.Zero), state);
        AssertEqual(YardBehavior.Kicked, state.Behavior, "the touch kicks the Yard");
        AssertEqual(measured, state.AirborneVelocity.X, "the kicked Yard flies right at the camera's measure, fraction and all");
        AssertEqual(YardKicks.ForHorizontalSpeed(measured.UnsignedWholePixels), state.AirborneVelocity.Y,
            "the kicked Yard rises at the speed the measure's whole pixels select");
    }

    /// <summary>Checks dropped, kicked, and beam-launched motion across signed carry and vertical speed caps.</summary>
    internal static void VerifyYardAirborneTrajectories(SelectedRoom room)
    {
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(64, 64));
        // Signed whole/fraction boundary cases exercise native ADC carry, the
        // unusual suppressed zero-high-word write, and the vertical speed cap.
        uint[] horizontal = [0, 0x00008000, 0x00010000, 0x00018000, 0xFFFF8000, 0xFFFF0000, 0xFFFE8000];
        uint[] vertical = [0xFFFE0000, 0xFFFF8000, 0, 0x0003F000];
        int samples = 0;
        foreach (uint initialX in horizontal)
        foreach (uint initialY in vertical)
        foreach (YardBehavior behavior in new[] { YardBehavior.Dropped, YardBehavior.Kicked, YardBehavior.BeamLaunched })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(FixtureEnemyTileArtwork()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new StandaloneRoomEnemyServices(() => 0));
            enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(room.State.EnemyPopulation, BgArtworkFixtures.For(new SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus()));
            var actor = enemies.Places.InUpdateOrder[0];
            // The Yard's setup builds the state its main AI, which the check drives, runs on.
            YardEnemyState state = YardEnemyBehavior.InitializeYard(actor.HeldOccupant().SpawnSettings<YardSpawnSettings>(), actor);
            actor.Position = new(new(512, 0xF000), new(512, 0xF000));
            actor.AnimationClock.HoldFor(ushort.MaxValue);
            state.Motion = new YardMotion.Airborne(YardDirections.ForAirborneFacing(state.AirborneFacingDirection).HidingAnimation);
            state.Behavior = behavior;
            state.AirborneVelocity = new FixedVector<RoomPixels>(
                SixteenSixteenWords.ToDisplacement(unchecked((int)initialX)),
                SixteenSixteenWords.ToDisplacement(unchecked((int)initialY)));
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(32, 32)),
                Health = 999, MaxHealth = 999 }.InPose(SamusPoseId.FacingRightNormalPose);
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            uint x = 0x0200F000, y = 0x0200F000, vx = initialX, vy = initialY;
            for (int frame = 0; frame < 24; frame++)
            {
                // Oracle follows $A3:D1B3's word stores independently of production
                // helpers. Movement consumes old velocity, then friction/gravity run.
                if (behavior != YardBehavior.Dropped)
                {
                    x = unchecked(x + vx);
                    uint changed = unchecked(vx + ((vx & 0x80000000) != 0 ? 0x1000u : 0xFFFFF000u));
                    vx = SixteenSixteenWords.WholeOf(changed) == 0
                        ? SixteenSixteenWords.Compose((ushort)SixteenSixteenWords.WholeOf(vx), (ushort)SixteenSixteenWords.FractionOf(changed)) : changed;
                }
                y = unchecked(y + vy);
                uint accelerated = unchecked(vy + 0x2000);
                vy = unchecked((short)(SixteenSixteenWords.WholeOf(accelerated) - 4)) < 0
                    ? accelerated : SixteenSixteenWords.Compose((ushort)SixteenSixteenWords.WholeOf(vy), (ushort)SixteenSixteenWords.FractionOf(accelerated));
                enemies.YardBehavior.RunYardMainAi(actor, state, PlacedEnemyAccess.AiFrame(samus, level: level, camera: new(384, 384)));
                AssertEqual(SixteenSixteenWords.ToCoordinate(x), actor.Position.X, "Yard airborne X trajectory");
                AssertEqual(SixteenSixteenWords.ToCoordinate(y), actor.Position.Y, "Yard airborne Y trajectory");
                AssertEqual(vx, unchecked((uint)state.AirborneVelocity.X.RawValue), "Yard native friction words");
                AssertEqual(vy, unchecked((uint)state.AirborneVelocity.Y.RawValue), "Yard native gravity words");
                AssertEqual(YardMovement.Airborne, state.Movement, "empty fixture remains airborne");
                samples++;
            }
        }
        Console.WriteLine($"Yard airborne: {samples} exact position/velocity samples across drop, kick and shot states.");
    }
}
