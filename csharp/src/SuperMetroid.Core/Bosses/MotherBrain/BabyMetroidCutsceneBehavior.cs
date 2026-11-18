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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.BabyMetroidCutscene];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BabyMetroidCutsceneEntity(this, place, common);

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

    /// <summary>Places the Baby in the first free place of the room's enemies, runs its setup, and leaves it undrawn until its first scheduled turn.</summary>
    internal void SpawnMotherBrainBabyMetroid(MotherBrainEnemyState state)
    {
        if (state.BabyMetroidPlace is not null)
        {
            throw new InvalidOperationException(
                "Mother Brain attempted to allocate the cutscene Baby more than once.");
        }

        // The spawn takes the first free place; the Baby is a single part. A full room spawns nothing.
        if (_places.FindFreePlace() is not { } babyPlace)
            return;

        RoomEnemyPlacement population = MotherBrainBabyMetroidDefinitions.SpawnPopulation;

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(population.DefinitionId);
        _lifecycle.InitializePlaceFromDefinition(babyPlace, population, definition);
        // The Baby holds the encounter that spawned it from its setup (#2218).
        babyPlace.SetSpeciesState(state);
        _lifecycle.RunSpawnedInitializationAi(babyPlace);

        // Spawning leaves the Baby undrawn after its setup while it animates.
        // Its animation is still live and shows its first visible frame on its first
        // scheduled turn; this update's list of running enemies was fixed before the Baby was placed.
        babyPlace.Frame = babyPlace.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.Empty
            : null;
        _places.CoverSpawn(babyPlace);
    }

    /// <summary>Sets the Baby up once the rainbow sequence exists and starts its first animation and fifteen nontransparent colors.</summary>
    internal static void InitializeMotherBrainBabyMetroid(RoomEnemyPlace place)
    {
        MotherBrainEnemyState state = place.RequireSpeciesState<MotherBrainEnemyState>();
        if (state.RainbowBeamSequence is null)
        {
            throw new InvalidDataException(
                "The cutscene Baby was initialized before the final rainbow sequence.");
        }

        var baby = new BabyMetroidCutsceneState();
        // The place keeps its previous occupant's fractions, which the Baby starts from (BabyMetroidArrivalFractions).
        baby.Initialize(place.Position);
        // The place owns the Baby's contact, processing and presence (#2310): initialization makes it block the Plasma
        // Beam and starts its animation; its population entry's presence stands.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        state.BabyMetroidPlace = place;
        state.BabyMetroid = baby;

        CopyBabyMetroidActorToPhysicalPlace(place, baby, state.Colours);
        // The Baby's opening pulse, which it is made playing, plays its first step on the next update.
        place.AnimationClock.Restart();

        // The Baby arrives in its own colours; where it is transparent the room's backdrop shows.
        ShowBabyMetroidArrival(state.Colours);
    }

    /// <summary>Lays the Baby's start colours, which leave the transparent colour untouched.</summary>
    internal static void ShowBabyMetroidArrival(MotherBrainColours colours) =>
        Ensure.NotNull(colours).ShowBabyArrived();

    /// <summary>Advances the later Baby place, publishes cries/healing, and writes body and Baby animation changes for their next ordered enemy turns.</summary>
    internal void RunMotherBrainBabyMetroidMain(
        RoomEnemyPlace place,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = place.RequireSpeciesState<MotherBrainEnemyState>();
        BabyMetroidCutsceneState baby = state.BabyMetroid ??
            throw new InvalidOperationException("The Baby has no cutscene state.");

        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException("The cutscene Baby lost the rainbow sequence.");

        place.ShakeTimer = 0;
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.IdleUntilNoHealth)
        {
            // However many rings struck it since its last turn, the Baby cries once, and every one of those cries is spent.
            state.PendingBabyCryCount = 0;
            _motherBrainEncounter.RequestBodySound(ShitroidSoundDefinitions.DrainCry);
        }
        BabyMetroidCutsceneStepResult step = baby.Step(
            samus,
            sequence,
            state.Head.Position.Whole,
            cameraPosition: camera,
            enemyFrameCounter: place.FrameCounter,
            randomNumber: _room.Random.Read());
        // Both outcomes of healing Samus queue the energy sound after her energy rises.
        // Use the phase that ran, including its final full-health update.
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            samus.Health >= MotherBrainHealthSounds.MinimumEnergy &&
            MotherBrainCadenceDefinitions.BabyHealSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter))
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Low);


        if (step.StartedAnimation is { } started)
            baby.Playback = EnemyAnimationPlayback.Start(place, MotherBrainBabyAnimations.StartingAt[started]);
        CopyBabyMetroidActorToPhysicalPlace(place, baby, state.Colours);
        ShowOrDeleteBabyMetroid(place, step);
        ApplyBabyMetroidFrameEffects(state.Colours, step);
    }

    /// <summary>
    /// Shows, hides or deletes the Baby's place as <paramref name="step"/> asks: its death's flicker and finished fade
    /// show and hide it, and the cutscene's end deletes it (#2310).
    /// </summary>
    private static void ShowOrDeleteBabyMetroid(RoomEnemyPlace place, BabyMetroidCutsceneStepResult step)
    {
        switch (step.Visibility)
        {
            case BabyMetroidVisibilityRequest.Show:
                place.Presence = place.Presence.Without(EnemyPresence.Hidden);
                break;
            case BabyMetroidVisibilityRequest.Hide:
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                break;
            case BabyMetroidVisibilityRequest.Unchanged:
                break;
            default:
                throw new InvalidOperationException($"Unsupported Baby visibility request {step.Visibility}.");
        }
        if (step.BabyDeleted)
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
    }

    /// <summary>Copies cutscene position, shading and health state into the live enemy.</summary>
    private static void CopyBabyMetroidActorToPhysicalPlace(
        RoomEnemyPlace place,
        BabyMetroidCutsceneState baby,
        MotherBrainColours colours)
    {
        place.Position = baby.Position;
        place.Shading = baby.HurtFlashShown ? EnemyShades.HurtFlashLook : colours.BabyShading;
        place.Health = baby.Health;
    }

    /// <summary>Applies one step's music, sounds, dust, fade and recovery-light requests to room systems.</summary>
    internal void ApplyBabyMetroidFrameEffects(MotherBrainColours colours, BabyMetroidCutsceneStepResult step)
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
            _motherBrainEncounter.RequestBodySound(ShitroidSoundDefinitions.RemorseCry);

        foreach (BabyMetroidReleaseDustRequest dust in step.ReleaseDustClouds)
            _projectiles.SpawnDustOrExplosion(dust.Position, dust.Animation);

        if (step.DeathExplosion is { } explosion)
        {
            _projectiles.SpawnDustOrExplosion(explosion.Position, explosion.Animation);
            // The explosion competes at low priority (MotherBrainEncounter.RequestCueSound).
            _motherBrainEncounter.RequestCueSound(explosion.SoundEffect);
        }

        if (step.BabyFadeImage is { } image)
            ShowBabyMetroidFade(colours, image);

        if (step.RecoveryLightsImage is { } lights)
            colours.ShowLightsRecovering(lights);
    }

    /// <summary>Shows <paramref name="image"/> of the Baby's fade to black from its installed colours.</summary>
    /// <exception cref="ArgumentException">The image is not one of the Baby's fade's.</exception>
    internal static void ShowBabyMetroidFade(MotherBrainColours colours, MotherBrainFadeImage image) =>
        Ensure.NotNull(colours).ShowBabyFade(image);

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
        BabyMetroidCutsceneState baby = place.RequireSpeciesState<MotherBrainEnemyState>().BabyMetroid ??
            throw new InvalidOperationException("The Baby's animation ran without its cutscene state.");
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<MotherBrainBabySegment> playback = baby.Playback;
        EnemyAnimationPlayback.Play(place, MotherBrainBabyAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of,
            cue => throw new InvalidOperationException($"The cutscene Baby's animations raise no event {cue}."));
        baby.Playback = playback;
    }
}
