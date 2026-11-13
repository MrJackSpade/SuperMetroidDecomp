using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => CryRandom;
        RoomEnemySlot firstMetroid = MetroidAtItsCry(enemies, 0);
        RoomEnemySlot boyon = BoyonStartingItsBounce(enemies, 1);
        RoomEnemySlot secondMetroid = MetroidAtItsCry(enemies, 2);

        enemies.Audio.BeginFrame();
        foreach (RoomEnemySlot slot in new[] { firstMetroid, boyon, secondMetroid })
            EnemyAnimationPlayer.AdvanceAnimation(slot, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);

        SoundEffectId cry = MetroidReactions.RandomCrySoundEffect(CryRandom);
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: bus, services: new StandaloneRoomEnemyServices(() => 0));
        enemies.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(populationPointer), BgArtworkFixtures.For(new SnesVram())));

        EnemyProjectile link = enemies.Projectiles.EnemyProjectiles[^1];
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(link, RoomEnemyProjectileKind.NuclearWaffleBody, SpriteGraphics.SharedStandard);
        link.Position = link.Position.WithWhole(new(0x0100, 0x0100));
        link.BlocksSamusProjectiles = true;
        link.CollisionOption = EnemyProjectileCollisionOption.IndestructibleDud;
        var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        SamusProjectile shot = shots.Slots[0];
        shot.Type = SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false);
        shot.Damage = 1;
        shot.Direction = SamusProjectileDirection.Right;
        shot.Position = shot.Position.WithWhole(new(0x0100, 0x0100));

        enemies.Audio.BeginFrame();
        AssertEqual(1, enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(shots, new SamusBombProjectileSystem()),
            "the indestructible link rejects the shot");
        AssertSequenceEqual(new[] { new SoundRequest(SamusSound.DudShot, SoundRequestPriority.Normal) }, enemies.Audio.SoundRequests,
            "the rejected shot queues the dud sound at once, at normal priority");

        enemies.Audio.BeginFrame();
        enemies.Projectiles.CreateEnemyProjectileDudShot(shots.Slots[1]);
        AssertSequenceEqual(new[] { new SoundRequest(SamusSound.DudShot, SoundRequestPriority.Low) }, enemies.Audio.SoundRequests,
            "a protected body part's dud queues the same sound at low priority");
        Console.WriteLine("  Dud sounds: an indestructible projectile's dud is queued at once at normal priority; a body part's at low.");
    }

    /// <summary>A Metroid in <paramref name="place"/> whose next script step is its chasing loop's random cry.</summary>
    private static RoomEnemySlot MetroidAtItsCry(RoomEnemySystem enemies, int place)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[place];
        slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.AnimationClock.StepOnNextUpdate();
        slot.SetSpeciesState(new MetroidEnemyState(
            new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyA, default, SpriteGraphics.SharedStandard),
            new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyB, default, SpriteGraphics.SharedStandard))
        {
            Playback = new(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus),
                AnimationSegments<MetroidSegment, EnemyFrameId, MetroidCue>.PlaceOf(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus),
                    MetroidSegment.ChasingSamus,
                    MetroidAnimations.Segments.Beats(MetroidSegment.ChasingSamus).ToList().FindIndex(beat => beat is AnimationBeat<EnemyFrameId, MetroidCue>.Event))),
        });
        return slot;
    }

    /// <summary>A Boyon in <paramref name="place"/> whose next script step starts its bounce.</summary>
    private static RoomEnemySlot BoyonStartingItsBounce(RoomEnemySystem enemies, int place)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[place];
        slot.Bind(EnemyDefinitionId.Boyon, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter2(EnemyDefinitionId.Boyon, 64);
        slot.Position = slot.Position.WithWhole(new(256, 512));
        BoyonBehavior.InitializeBoyon(slot);
        BoyonEnemyState state = slot.RequireSpeciesState<BoyonEnemyState>();
        state.Playback = new AnimationPlayback<BoyonSegment>(BoyonAnimations.StartingWith(BoyonSegment.Bouncing), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
        return slot;
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
