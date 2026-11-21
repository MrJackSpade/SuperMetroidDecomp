using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.EffectActors;
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
using SuperMetroid.Core.Enemies.Artwork;

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
        RoomEnemyPlace firstMetroid = MetroidAtItsCry(enemies, 0);
        RoomEnemyPlace boyon = BoyonStartingItsBounce(enemies, 1);
        RoomEnemyPlace secondMetroid = MetroidAtItsCry(enemies, 2);

        enemies.Audio.BeginFrame();
        foreach (RoomEnemyPlace place in new[] { firstMetroid, boyon, secondMetroid })
            EnemyAnimationPlayer.AdvanceAnimation(place, samus: RoomEnemyFrameFixtures.StandaloneSamus(), level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);

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

        EnemyProjectile link = FamilyProjectileStarts.Spawn(enemies, 17, RoomEnemyProjectileKind.NuclearWaffleBody, SpriteGraphics.SharedStandard);
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

        enemies.Audio.BeginFrame();
        enemies.Projectiles.CreateEnemyProjectileDudShot(shots.Shots[1].HeldShot());
        AssertSequenceEqual(new[] { new SoundRequest(SamusSound.DudShot, SoundRequestPriority.Low) }, enemies.Audio.SoundRequests,
            "a protected body part's dud queues the same sound at low priority");
        Console.WriteLine("  Dud sounds: an indestructible projectile's dud is queued at once at normal priority; a body part's at low.");
    }

    /// <summary>A Metroid in <paramref name="place"/> whose next script step is its chasing loop's random cry.</summary>
    private static RoomEnemyPlace MetroidAtItsCry(RoomEnemySystem enemies, int place)
    {
        RoomEnemyPlace enemy = enemies.Places.InUpdateOrder[place];
        enemy.Bind(EnemyDefinitionId.Metroid, enemy.KeptDefinition);
        enemy.Bind(enemy.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        enemy.AnimationClock.StepOnNextUpdate();
        enemy.SetSpeciesState(new MetroidEnemyState(
            new RoomEffectActor(RoomEffectActorKind.MetroidElectricity, default, SpriteGraphics.SharedStandard),
            new RoomEffectActor(RoomEffectActorKind.MetroidShell, default, SpriteGraphics.SharedStandard))
        {
            Playback = new(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus),
                AnimationSegments<MetroidSegment, EnemyFrameId, MetroidCue>.PlaceOf(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus),
                    MetroidSegment.ChasingCry, 0)),
        });
        return enemy;
    }

    /// <summary>
    /// A Boyon in <paramref name="place"/> whose next script step starts its bounce, reached as the game reaches it
    /// (#2521): its first main update derives its arc, and the next, with Samus standing on it, starts its bounce and the
    /// bouncing animation whose first cue is the bounce sound. The bouncing animation never plays before the Boyon bounces.
    /// </summary>
    private static RoomEnemyPlace BoyonStartingItsBounce(RoomEnemySystem enemies, int place)
    {
        RoomEnemyPlace enemy = enemies.Places.InUpdateOrder[place];
        // Parameter two is the horizontal trigger range Samus must be within to start a bounce.
        var settings = (BoyonSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Boyon, PopulationWords.Parameters(0, 64));
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        enemy.PlaceAs<BoyonEntity>(EnemyDefinitionId.Boyon, settings, samus, new(256, 512));
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
