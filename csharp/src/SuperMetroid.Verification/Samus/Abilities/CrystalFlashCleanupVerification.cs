using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Samus.Abilities.CrystalFlashVisualsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Progression;

/// <summary>Checks Crystal Flash activation, contact, visibility, resource drain, and movement cleanup in production runtime.</summary>
internal static class CrystalFlashCleanupVerification
{
    /// <summary>The successful, refill-assisted and insufficient-ammo Crystal Flash routes, each a suite of its own.</summary>
    internal static FullRunSuitesVerification.RegisteredSuite[] CrystalFlashRuntimeSuites() =>
    [
        new($"{nameof(VerifyCrystalFlashRuntimeRoute)}Successful", () => VerifyCrystalFlashRuntimeRoute(capacity: 11, refill: false)),
        new($"{nameof(VerifyCrystalFlashRuntimeRoute)}Refilled", () => VerifyCrystalFlashRuntimeRoute(capacity: 10, refill: true)),
        new($"{nameof(VerifyCrystalFlashRuntimeRoute)}Insufficient", () => VerifyCrystalFlashRuntimeRoute(capacity: 10, refill: false)),
    ];

    /// <summary>Steps a controlled Landing Site route from Power Bomb placement through activation or rejection and post-flash movement.</summary>
    private static void VerifyCrystalFlashRuntimeRoute(ushort capacity, bool refill)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // Keep the constructed clearing visible; the old mechanics-only fixture
        // left the displayed camera at zero, outside the tested Samus trajectory.
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite, camera: new Position16<RoomPixels>(384, 384));
        for (ushort y = 1; y <= 2; y++)
        for (ushort x = 1; x <= 2; x++)
            runtime.Room.LoadedCamera.Scrolls.SetLogicalState(new(x, y), RoomScrollState.Green);
        var level = runtime.Room.LoadedLevelData;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.Size.X + x;
            level.SetTerrain(index, RoomTerrainBlock.Of(0, BlockMirroring.None,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air));
            level.SetBehavior(index, RoomBlockBehavior.None);
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.Health = 49; samus.MaxHealth = 1499;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.PowerBombs = samus.MaxPowerBombs = capacity;
        samus.SelectedHudItem = HudItem.PowerBomb;
        samus.Position = samus.Position.WithWholeX(512);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.Position = samus.Position.WithWholeY((ushort)(511 - samus.Kinematics.Radius.Y));
        SamusPresentation.InitializeAnimation(samus);
        runtime.StepFrame(0);
        ushort startingY = samus.Position.Y.Whole;
        runtime.StepFrame(runtime.Options.ControllerBindings.Shoot);
        AssertEqual(capacity - 1, samus.PowerBombs, "normal placement consumes one Power Bomb");
        ControllerButton chord = (ControllerButton.Down | ControllerButton.L | ControllerButton.R | ControllerButton.X);
        int started = -1, finished = -1;
        bool bubble = false, drained = false;
        RoomEnemySlot? contactEnemy = null;
        bool contactVerified = false;
        // Each frame's published scenes are kept and drawn once the route has run. A published scene shares no storage
        // with the runtime: its paint copies the frame's draws, its layers copy their arrays, and everything else it
        // reaches is immutable (records and classes whose arrays are copied, or replaced and never written, once
        // published) or installed content, so drawing it later draws the frame it was published as.
        var scenes = new List<CrystalFlashFrameScenes>();
        for (int frame = 0; frame < 1000; frame++)
        {
            if (refill && frame == 60)
            {
                // A constructed drop owner collides an actual Power Bomb pickup with
                // this runtime's Samus. No direct ammo write substitutes for collection.
                AssertEnemyPickupEffect(EnemyPickupKind.PowerBomb, samus, expectedSound: 5,
                    assertEffect: collected => AssertEqual(10, collected.PowerBombs,
                        "refill after placement reaches ten without increasing capacity"));
            }
            bool active = samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive;
            bool testContact = capacity == 11 && started >= 0 && frame == started + 30;
            if (testContact)
            {
                AssertEqual(CrystalFlashPhase.DrainingAmmo, samus.CrystalFlash.Phase,
                    "contact fixture enters the intended ammo-drain phase");
                // Replace the population at the contact boundary; retain retail AI,
                // header damage and the runtime's ordinary collision ordering. A
                // zero-only RNG would hang native-style drop selection on death.
                var contactBus = new CrystalFlashContactPopulation(bus, samus.Position.Whole);
                runtime.ReplaceRoomEnemiesForVerification(contactBus, new StandaloneRoomEnemyServices(() => 1), new RoomEnemyLoadRequest(contactBus.ReadEnemyPopulation(CrystalFlashContactDefinitions.Pointer), BgArtworkFixtures.For(runtime.Vram)));
                contactEnemy = runtime.Enemies.SlotTable.Slots[0];
                (contactEnemy.SpeciesState<RipperEnemyState>()
                    ?? throw new InvalidOperationException("The contact Ripper has no state.")).XVelocity = SignedSixteenSixteen.Zero;
                // Contact runs before AI publishes its first map. Seed that same
                // authored map so this fixture tests contact on this exact frame.
                contactEnemy.SpritemapPointer = RipperAnimationsConstants.Spritemaps(
                    contactEnemy.RequireSpeciesState<RipperEnemyState>().Playback.Animation.Phases[0].Segment).First();
            }
            int beforeHealth = samus.Health;
            // Later projectile processing can delete the actor and clear its header.
            // Read the contact damage before stepping, not from a recycled slot.
            int contactDamage = testContact ? (contactEnemy ?? throw new InvalidOperationException("Verification requires contactEnemy.")).Definition.Damage : 0;
            // Once activated, challenge the inert input handler with movement/jump.
            ControllerButton input = active ? (ControllerButton.Right | ControllerButton.A) : chord;
            runtime.StepFrame(input);
            if (testContact)
            {
                int afterDamage = Math.Max(0, beforeHealth - contactDamage);
                int expected = Math.Min(samus.MaxHealth, afterDamage + ((runtime.Clock.Counter & 7) == 0 ? 50 : 0));
                AssertTrue(contactDamage > 0, "constructed contact actor has nonzero retail damage");
                AssertEqual(expected, samus.Health, "enemy contact damages Crystal Flash before its energy refill");
                AssertEqual(0, samus.InvincibilityTimer.RawValue, "Crystal Flash clears immunity after actual contact");
                AssertEqual(0, samus.KnockbackTimer, "Crystal Flash suppresses the contact knockback request");
                AssertTrue(!samus.KnockbackActive, "contact does not install ordinary knockback during ammo drain");
                (contactEnemy ?? throw new InvalidOperationException("Verification requires contactEnemy.")).Position = contactEnemy.Position.WithWholeX(unchecked((ushort)(contactEnemy.Position.X.Whole + (128))));
                contactVerified = true;
                Console.WriteLine($"Runtime Crystal Flash contact: health {beforeHealth}->{samus.Health}, retail damage={contactDamage}; no retained immunity or knockback.");
            }
            if (started < 0 && samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive) started = frame;
            bubble |= runtime.BombProjectiles.PowerBombExplosion.Phase == PowerBombExplosionPhase.CrystalFlashExplosion;
            drained |= samus.CrystalFlash.Phase == CrystalFlashPhase.DrainingAmmo;
            if (started >= 0)
            {
                scenes.Add(VerifyCrystalFlashVisualFrame(runtime, bus, frame - started));
                AssertEqual(512, samus.Position.X.Whole, "Crystal Flash owns horizontal movement");
                AssertEqual(startingY - Math.Min(frame - started + 1, 10) * 2, samus.Position.Y.Whole,
                    "held Jump cannot replace the native Crystal Flash vertical trajectory");
                if (samus.CrystalFlash.Phase == CrystalFlashPhase.Inactive) { finished = frame; break; }
            }
        }
        VerifyCrystalFlashVisualFrameResult[] visibility = CrystalFlashVisualsVerification.Visibility(scenes);
        int visibleBodyFrames = visibility.Count(visible => visible.Body);
        int visibleWindowFrames = visibility.Count(visible => visible.Window);
        if (capacity == 10 && !refill)
        {
            AssertEqual(-1, started, "nine remaining Power Bombs reject Crystal Flash");
            AssertEqual(9, samus.PowerBombs, "failed activation preserves remaining Power Bombs");
            AssertEqual(49, samus.Health, "failed activation restores no energy");
            AssertTrue(!runtime.BombProjectiles.PowerBombExplosion.IsArmed, "failed activation releases the Power Bomb lock");
            Console.WriteLine("Runtime Crystal Flash: ten-capacity no-refill control rejects activation.");
            return;
        }
        AssertTrue(started >= 0 && finished > started && bubble && drained, "runtime completes the activated Crystal Flash and bubble");
        if (capacity == 11) AssertTrue(contactVerified, "runtime exercised real enemy contact during Crystal Flash");
        AssertEqual(1499, samus.Health, "runtime restores energy");
        AssertEqual(0, samus.Missiles, "runtime consumes ten missiles");
        AssertEqual(0, samus.SuperMissiles, "runtime consumes ten supers");
        AssertEqual(0, samus.PowerBombs, "runtime consumes ten remaining Power Bombs");
        Console.WriteLine($"Crystal Flash visual coverage: body={visibleBodyFrames}, window={visibleWindowFrames} frames.");
        // Power Bomb allocation consumes its own HDMA setup pass. Activation is one
        // NMI later than the former eager-setup fixture; refill remains aligned to the
        // global eight-frame cadence, shortening this particular body's lifetime by one.
        AssertEqual(249, visibleBodyFrames, "complete observed Crystal Flash body visibility duration");
        AssertEqual(34, visibleWindowFrames, "native Crystal Flash color window visibility duration");
        for (int frame = 0; frame < 30; frame++) runtime.StepFrame(ControllerButton.Right);
        AssertTrue(samus.Position.X.Whole > 512, "normal movement resumes after Crystal Flash");
        Console.WriteLine($"Runtime Crystal Flash: capacity={capacity}, refill={refill}, activation={started}, completion={finished}; placement, bubble, resources and movement ownership pass.");
    }

    /// <summary>Runs the three focused production Crystal Flash routes.</summary>
    [VerificationCommand("--crystal-flash-runtime")]
    internal static int CrystalFlashRuntimeCommand()
    {
        foreach (FullRunSuitesVerification.RegisteredSuite suite in CrystalFlashRuntimeSuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
