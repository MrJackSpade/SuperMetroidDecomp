using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that the enemies' sounds reach the sound queues in the order the enemies make them (#1620).</summary>
internal static class EnemySoundOrderVerification
{
    /// <summary>The random number each Metroid's cry reads.</summary>
    private const ushort CryRandom = 5;

    /// <summary>
    /// Three enemies run their animation scripts in place order within one update: a Metroid reaching its random cry, a
    /// Boyon starting its bounce, and a second Metroid reaching its cry. Each sound is queued the moment its enemy makes
    /// it, as the game queues every enemy sound: the update holds the first cry, the bounce, then the second cry, in that
    /// order. The two cries of one species in one update are both queued, and no later enemy's sound moves ahead of an
    /// earlier one's.
    /// </summary>
    internal static void VerifyEnemySoundsQueueInRunOrder()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => CryRandom;
        // The room's population load sets up a Metroid, the Boyon and a second Metroid, in that order (#2554).
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        RoomEnemyPlacement metroid = PopulationPlacementAccess.Entry(EnemyDefinitionId.Metroid,
            EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Metroid, EnemySpawnSettingsCodec.Blank(EnemyDefinitionId.Metroid)));
        // Parameter two is the horizontal trigger range Samus must be within to start a bounce.
        var boyonSettings = (BoyonSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Boyon, PopulationWords.Parameters(0, 64));
        IReadOnlyList<RoomEnemyPlace> places = enemies.LoadPopulation(samus,
            metroid, PopulationPlacementAccess.Entry(EnemyDefinitionId.Boyon, boyonSettings, new(256, 512)), metroid);
        RoomEnemyPlace boyon = BoyonStartingItsBounce(places[1], samus);
        RoomEnemyPlace firstMetroid = MetroidAtItsCry(enemies, places[0]);
        RoomEnemyPlace secondMetroid = MetroidAtItsCry(enemies, places[2]);

        enemies.Audio.BeginFrame();
        // Each enemy's animation runs in place order: the Metroids' as the shared enemy animation runs a Metroid's, over the
        // state each one's setup built, and the Boyon's through the entity its placement built.
        void AnimateMetroid(RoomEnemyPlace metroid) =>
            enemies.MetroidBehavior.AdvanceMetroidTypedAnimation(metroid, metroid.RequireSpeciesState<MetroidEnemyState>());
        AnimateMetroid(firstMetroid);
        EnemyAnimationPlayer.AdvanceAnimation(boyon, samus: RoomEnemyFrameFixtures.StandaloneSamus(), level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);
        AnimateMetroid(secondMetroid);

        SoundEffectId cry = MetroidBehaviorDefinitions.Cry.Choose(CryRandom);
        AssertEqual(EnemyAudioQueueAccess.SoundList([cry, EnemySoundEffectDefinitions.BoyonBounceSound, cry]), enemies.Audio.QueuedSoundList,
            "each enemy's sound is queued as it runs, in place order, and both Metroid cries are queued");
        AssertTrue(enemies.Audio.SoundRequests.All(request => request.Priority == SoundRequestPriority.Normal),
            "the cries and the bounce join the queue at normal priority");
        Console.WriteLine("  Enemy sound order: a Metroid's cry, a Boyon's bounce and a second Metroid's cry are queued in the order they run.");
    }

    /// <summary>
    /// A Samus shot rejected by an indestructible enemy projectile (a Nuclear Waffle link) queues the dud sound at once, in
    /// the collision pass that follows the enemy pass, at normal priority, as the game's projectile-collision dud does. A
    /// shot a protected body part rejects queues the same sound at low priority.
    /// </summary>
    internal static void VerifyProjectileDudSoundsQueueWhenRejected()
    {
        const ushort populationPointer = 0x9400;
        var bus = new TestAddressSpace();
        bus.WriteByte(0xa10000 | populationPointer, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 1, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 2, 0);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: bus, services: new StandaloneRoomEnemyServices(() => 0));
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(bus.ReadEnemyPopulation(populationPointer), BgArtworkFixtures.For(new SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus()));

        EnemyProjectile link = FamilyProjectileStarts.Spawn(enemies, RoomEnemyProjectileKind.NuclearWaffleBody, SpriteGraphics.SharedStandard);
        link.Position = link.Position.WithWhole(new(0x0100, 0x0100));
        link.BlocksSamusProjectiles = true;
        link.CollisionOption = EnemyProjectileCollisionOption.IndestructibleDud;
        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        SamusProjectile shot = shots.Shots[0];
        shot.Occupy(SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false), 1);
        shot.HeldShot().Direction = SamusProjectileDirection.Right;
        shot.HeldShot().Position = shot.Position.WithWhole(new(0x0100, 0x0100));

        enemies.Audio.BeginFrame();
        enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(shots, new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
        AssertSequenceEqual(new[] { new SoundRequest(SamusSound.DudShot, SoundRequestPriority.Normal) }, enemies.Audio.SoundRequests,
            "the rejected shot queues the dud sound at once, at normal priority");

        // The body part's dud is made from the shot that struck it, which holds the second place.
        SamusProjectileOccupant bodyPartShot = shots.Shots[1].Occupy(SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false), 1);
        enemies.Audio.BeginFrame();
        enemies.Projectiles.CreateEnemyProjectileDudShot(bodyPartShot);
        AssertSequenceEqual(new[] { new SoundRequest(SamusSound.DudShot, SoundRequestPriority.Low) }, enemies.Audio.SoundRequests,
            "a protected body part's dud queues the same sound at low priority");
        Console.WriteLine("  Dud sounds: an indestructible projectile's dud is queued at once at normal priority; a body part's at low.");
    }

    /// <summary>
    /// The set-up Metroid in <paramref name="enemy"/>, its chasing loop, which its setup starts, played on through the
    /// Metroid's own animation over every frame of the pulses before the loop's random cry: each step shows the next frame,
    /// so its next script step is the cry.
    /// </summary>
    private static RoomEnemyPlace MetroidAtItsCry(RoomEnemySystem enemies, RoomEnemyPlace enemy)
    {
        MetroidEnemyState state = enemy.RequireSpeciesState<MetroidEnemyState>();
        Animation<MetroidSegment> chasing = state.Playback.Animation;
        AssertEqual(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus), chasing, "the set-up Metroid plays its chasing loop");
        AssertEqual(AnimationPlace.StartOfAnimation, state.Playback.Place, "the set-up Metroid's chasing loop stands at its start");
        int pulseFrames = chasing.Phases.TakeWhile(phase => phase.Segment != MetroidSegment.ChasingCry).Sum(phase => phase.Plays * phase.BeatCount);
        for (int step = 0; step < pulseFrames; step++)
        {
            enemy.AnimationClock.StepOnNextUpdate();
            enemies.MetroidBehavior.AdvanceMetroidTypedAnimation(enemy, state);
        }
        enemy.AnimationClock.StepOnNextUpdate();
        return enemy;
    }

    /// <summary>
    /// The set-up Boyon in <paramref name="enemy"/>, beside <paramref name="samus"/>, whose next script step starts its bounce, reached as the game reaches it
    /// (#2521): its first main update derives its arc, and the next, with Samus standing on it, starts its bounce and the
    /// bouncing animation whose first cue is the bounce sound. The bouncing animation never plays before the Boyon bounces.
    /// </summary>
    private static RoomEnemyPlace BoyonStartingItsBounce(RoomEnemyPlace enemy, SamusState samus)
    {
        _ = enemy.EntityOf<BoyonEntity>();
        samus.Position = FixedPosition<RoomPixels>.FromWhole(enemy.Position.Whole);
        enemy.RunMainAi(samus);
        enemy.RunMainAi(samus);
        enemy.AnimationClock.StepOnNextUpdate();
        return enemy;
    }

    /// <summary>Runs the enemy sound order check.</summary>
    [VerificationCommand("--enemy-sound-order")]
    internal static int EnemySoundOrderCommand()
    {
        Suite(nameof(VerifyEnemySoundsQueueInRunOrder), () => VerifyEnemySoundsQueueInRunOrder());
        Suite(nameof(VerifyProjectileDudSoundsQueueWhenRejected), () => VerifyProjectileDudSoundsQueueWhenRejected());
        return 0;
    }
}
