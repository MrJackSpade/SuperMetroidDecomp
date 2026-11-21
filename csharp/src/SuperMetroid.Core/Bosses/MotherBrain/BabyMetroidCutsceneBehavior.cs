using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid cutscene species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BabyMetroidCutsceneBehavior : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.BabyMetroidCutscene], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new BabyMetroidCutsceneEntity(this, occupant, place, common));

    /// <summary>Initializes the dynamically allocated Baby through the ordinary enemy lifecycle.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>The loaded room's bindings: its game-state services, random source and installed Baby artwork.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues healing, death, and cutscene cues with queue limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Allocates the Baby in the first free enemy place.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Supplies the enemy-frame counter used to gate incremental healing sound.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns release dust and Baby death explosions.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Shares Mother Brain's body, head, rainbow sequence, and requested music state with her.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Connects the cutscene Baby to its Mother Brain encounter and room services.</summary>
    internal BabyMetroidCutsceneBehavior(BabyMetroidCutsceneBehaviorDependencies dependencies)
    {
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _places = dependencies.Places;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
    }

    /// <summary>
    /// Makes <paramref name="lateFight"/>'s Baby in the first free place of the room's enemies, runs its setup, and leaves
    /// it undrawn until its first scheduled turn.
    /// </summary>
    internal void SpawnMotherBrainBabyMetroid(MotherBrainRainbowBeamAttackSequence lateFight)
    {
        Ensure.NotNull(lateFight);

        // The spawn takes the first free place; the Baby is a single part. A full room spawns nothing.
        if (_places.FindFreePlace() is not { } babyPlace)
            return;

        RoomEnemyPlacement population = MotherBrainBabyMetroidDefinitions.SpawnPopulation;

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(population.DefinitionId);
        _lifecycle.InitializePlaceFromDefinition(babyPlace, population, definition);
        // The Baby's place holds the Baby its late fight makes there, from its setup (#2218, #2471).
        babyPlace.SetSpeciesState(lateFight.MakeBaby(babyPlace));
        _lifecycle.SetUpSpawnedEnemy(babyPlace);

        // Spawning leaves the Baby undrawn after its setup while it animates.
        // Its animation is still live and shows its first visible frame on its first
        // scheduled turn; this update's list of running enemies was fixed before the Baby was placed.
        babyPlace.Frame = babyPlace.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.PointAtOrigin
            : null;
        _places.CoverSpawn(babyPlace);
    }

    /// <summary>Sets the Baby up and starts its first animation, showing its own colours as it arrives.</summary>
    internal static void InitializeMotherBrainBabyMetroid(RoomEnemyPlace place)
    {
        BabyMetroidCutsceneState baby = place.RequireSpeciesState<BabyMetroidCutsceneState>();
        MotherBrainEnemyState state = baby.LateFight.Encounter;
        // The place keeps its previous occupant's fractions, which the Baby starts from (BabyMetroidArrivalFractions).
        baby.Arrive();
        // The place owns the Baby's contact, processing and presence (#2310): initialization makes it block the Plasma
        // Beam and starts its animation; its population entry's presence stands.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);

        ShowBabyMetroidShading(place, baby, state.Colours);
        // The Baby's opening pulse, which it is made playing, plays its first step on the next update.
        place.AnimationClock.Restart();

        // The Baby arrives in its own colours; where it is transparent the room's backdrop shows.
        ShowBabyMetroidArrival(state.Colours);
    }

    /// <summary>Shows the Baby's arrival colours.</summary>
    private static void ShowBabyMetroidArrival(MotherBrainColours colours) =>
        Ensure.NotNull(colours).ShowBabyArrived();

    /// <summary>Advances the later Baby place, publishes cries/healing, and writes body and Baby animation changes for their next ordered enemy turns.</summary>
    private void RunMotherBrainBabyMetroidMain(
        RoomEnemyPlace place,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        BabyMetroidCutsceneState baby = place.RequireSpeciesState<BabyMetroidCutsceneState>();
        MotherBrainEnemyState state = baby.LateFight.Encounter;

        place.Shake.End();
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.Flying.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.Flying.IdleUntilNoHealth)
        {
            // However many rings struck it since its last turn, the Baby cries once, and every one of those cries is spent.
            state.PendingBabyCryCount = 0;
            _motherBrainEncounter.RequestBodySound(BabyMetroidSoundDefinitions.DrainCry);
        }
        BabyMetroidCutsceneStepResult step = baby.Step(
            samus,
            state.Head.Position.Whole,
            cameraPosition: camera,
            enemyFrameCounter: place.FrameCounter,
            randomNumber: _room.Random.Read());
        // Both outcomes of healing Samus queue the energy sound after her energy rises.
        // Use the phase that ran, including its final full-health update.
        if (step.PhaseBefore is BabyMetroidCutscenePhase.Flying.HealSamusToFullHealth &&
            samus.Health >= MotherBrainHealthSounds.MinimumEnergy &&
            MotherBrainCadenceDefinitions.BabyHealSound.IsDue(_frameContext.EnemyPassCount))
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Low);

        if (step.StartedAnimation is { } started)
            baby.Playback = EnemyAnimationPlayback.Start(place, MotherBrainBabyAnimations.StartingAt[started]);
        ShowBabyMetroidShading(place, baby, state.Colours);
        ShowOrDeleteBabyMetroid(place, step);
        ApplyBabyMetroidFrameEffects(state.Colours, step);
    }

    /// <summary>What <see cref="ShowOrDeleteBabyMetroid"/> does for each <see cref="BabyMetroidVisibilityRequest"/> member, each member's own arm.</summary>
    private static IdentityTable<BabyMetroidVisibilityRequest, Action<RoomEnemyPlace>> ShowOrDeleteBabyMetroidForBabyMetroidVisibilityRequest => field ??= new(new Dictionary<BabyMetroidVisibilityRequest, Action<RoomEnemyPlace>>
    {
        [BabyMetroidVisibilityRequest.Show] = static place => place.Presence = place.Presence.Without(EnemyPresence.Hidden),
        [BabyMetroidVisibilityRequest.Hide] = static place => place.Presence = place.Presence.With(EnemyPresence.Hidden),
        [BabyMetroidVisibilityRequest.Unchanged] = static place =>
        {
        },
    });

    /// <summary>
    /// Shows, hides or deletes the Baby's place as <paramref name="step"/> asks: its death's flicker and finished fade
    /// show and hide it, and the cutscene's end deletes it (#2310).
    /// </summary>
    private static void ShowOrDeleteBabyMetroid(RoomEnemyPlace place, BabyMetroidCutsceneStepResult step)
    {
        ShowOrDeleteBabyMetroidForBabyMetroidVisibilityRequest[step.Visibility](place);
        if (step.BabyDeleted)
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
    }

    /// <summary>Shows the Baby in its own colours, or in the hurt flash's while an onion ring's hit flashes it.</summary>
    private static void ShowBabyMetroidShading(
        RoomEnemyPlace place,
        BabyMetroidCutsceneState baby,
        MotherBrainColours colours) =>
        place.Shading = baby.HurtFlashShown ? EnemyShades.HurtFlashLook : colours.BabyShading;

    /// <summary>Applies one step's music, sounds, dust, fade and recovery-light requests to room systems.</summary>
    private void ApplyBabyMetroidFrameEffects(MotherBrainColours colours, BabyMetroidCutsceneStepResult step)
    {
        Ensure.NotNull(colours);
        if (step.FatalBlowStarted)
        {
            _audio.QueueEnemySound(RoomSound.BabyMetroidDeath, priority: SoundRequestPriority.Normal);
            colours.ShowLightsOut();
            _motherBrainEncounter.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
        }
        if (step.SamusThemeStarted)
        {
            _motherBrainEncounter.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.SamusThemeUpperCrateria),
                MusicQueueDelay.Shortest);
            _motherBrainEncounter.RequestMusic(MusicRequest.SelectTrack(MotherBrainBabyMetroidDefinitions.SamusThemeMusicTrack),
                MusicQueueDelay.Shortest);
        }
        if (step.LatchSoundQueued)
            _motherBrainEncounter.RequestRainbowBeamSound(MotherBrainSoundDefinitions.StartRainbowDrain);
        if (step.AmbientCrySoundQueued)
            _motherBrainEncounter.RequestBodySound(BabyMetroidSoundDefinitions.RemorseCry);

        foreach (BabyMetroidReleaseDustRequest dust in step.ReleaseDustClouds)
            _projectiles.SpawnDustOrExplosion(dust.Position, dust.Animation);

        if (step.DeathExplosion is { } explosion)
        {
            _projectiles.SpawnDustOrExplosion(explosion.Position, explosion.Animation);
            // The explosion competes at the low priority its request carries.
            _motherBrainEncounter.RequestCueSound(explosion.Sound);
        }

        if (step.BabyFadeImage is { } image)
            ShowBabyMetroidFade(colours, image);

        if (step.RecoveryLightsImage is { } lights)
            colours.ShowFade(lights);
    }

    /// <summary>Shows <paramref name="image"/> of the Baby's fade to black from its installed colours.</summary>
    private static void ShowBabyMetroidFade(MotherBrainColours colours, MotherBrainFadeImage image) =>
        Ensure.NotNull(colours).ShowFade(image);

    /// <summary>Runs BabyMetroidCutscene's main AI.</summary>
    internal void RunBabyMetroidCutsceneMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMotherBrainBabyMetroidMain(place, frame.Samus, frame.Camera);
    }

    /// <summary>
    /// Plays the Baby's animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and the clock notes each newly shown frame. A held pose rests: the last frame stays and the clock
    /// stops.
    /// </summary>
    internal static void AdvanceBabyMetroidCutsceneAnimation(RoomEnemyPlace place)
    {
        BabyMetroidCutsceneState baby = place.RequireSpeciesState<BabyMetroidCutsceneState>();
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<MotherBrainBabySegment> playback = baby.Playback;
        EnemyAnimationPlayback.PlaySilently(place, MotherBrainBabyAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of);
        baby.Playback = playback;
    }
}
