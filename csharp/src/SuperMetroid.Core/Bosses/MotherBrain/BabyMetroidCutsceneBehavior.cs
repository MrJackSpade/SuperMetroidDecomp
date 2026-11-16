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

    /// <summary>Shares physical body, head, rainbow sequence, and requested music state with Mother Brain.</summary>
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

    /// <summary>Places the Baby in the first free place of the room's enemies, runs its initializer, and leaves it undrawn until its first scheduled turn.</summary>
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

        RoomEnemyPlacement population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        if (population.DefinitionId != EnemyDefinitionId.BabyMetroidCutscene)
        {
            throw new InvalidDataException(
                "Mother Brain Baby population entry " +
                $"names enemy {population.DefinitionId}, not BabyMetroidCutscene.");
        }

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(population.DefinitionId);
        _lifecycle.InitializePlaceFromDefinition(babyPlace, population, definition);
        _lifecycle.RunInitializationAi(babyPlace, EnemySpawnSurroundings.None);

        // Spawning leaves the Baby undrawn after its initializer when its hide-drawing property is set.
        // Its animation is still live and shows its first visible frame on its first
        // scheduled turn; this update's list of running enemies was fixed before the Baby was placed.
        babyPlace.Frame = babyPlace.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.Empty
            : null;
        _places.CoverSpawn(babyPlace);
    }

    /// <summary>Creates the physical Baby only after the rainbow sequence exists and starts its first animation and fifteen nontransparent colors.</summary>
    internal void InitializeMotherBrainBabyMetroid(RoomEnemyPlace place)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The cutscene Baby was initialized without Mother Brain's body and brain.");
        if (state.RainbowBeamSequence is null)
        {
            throw new InvalidDataException(
                "The cutscene Baby was initialized before the final rainbow sequence.");
        }

        var baby = new BabyMetroidCutsceneState();
        // The place keeps its previous occupant's fractions, which the Baby starts from (BabyMetroidArrivalFractions).
        baby.Initialize(place.Position, place.Contact, place.Processing, place.Presence);
        state.BabyMetroidPlace = place;
        state.BabyMetroid = baby;
        state.BabyAppliedAnimation = baby.Animation;
        state.BabyPlayback = MotherBrainBabyAnimations.Starting(baby.Animation);

        CopyBabyMetroidActorToPhysicalPlace(place, baby, _motherBrainEncounter.Colours);
        place.AnimationClock.RestartAfter(baby.FrameHold);

        // The writer receives the Baby's fifteen colours after its first. The first, the transparent
        // one, is deliberately skipped, preserving the room's backdrop.
        ShowBabyMetroidArrival();
    }

    /// <summary>Lays the Baby's start colours, which leave the transparent colour untouched.</summary>
    internal void ShowBabyMetroidArrival() =>
        _motherBrainEncounter.Colours.ShowBabyArrived();

    /// <summary>Advances the later Baby place, publishes cries/healing, and writes body and Baby animation changes for their next ordered enemy turns.</summary>
    internal void RunMotherBrainBabyMetroidMain(
        RoomEnemyPlace place,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "The cutscene Baby lost Mother Brain's encounter state.");
        BabyMetroidCutsceneState baby = state.BabyMetroid ??
            throw new InvalidOperationException("The physical Baby has no cutscene state.");
        if (!ReferenceEquals(place, state.BabyMetroidPlace))
        {
            throw new InvalidDataException(
                $"Baby state names place {state.BabyMetroidPlace?.Ordinal}, but the Baby's step " +
                $"ran place {place.Ordinal}.");
        }

        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException("The cutscene Baby lost the rainbow sequence.");
        SamusState target = samus;

        place.ShakeTimer = 0;
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.IdleUntilNoHealth)
        {
            // The ring hit check clears the complete shared value and queues only one cry, even
            // when several rings incremented it before this later enemy place ran.
            state.PendingBabyCryCount = 0;
            _motherBrainEncounter.RequestBodySound(ShitroidSoundDefinitions.DrainCry);
        }
        BabyMetroidCutsceneStepResult step = baby.Step(
            target,
            sequence,
            state.Head.Position.Whole,
            cameraPosition: camera,
            enemyFrameCounter: place.FrameCounter,
            randomNumber: _room.Random.Read());
        // Both outcomes of healing Samus queue the energy sound after her energy rises.
        // Use the phase that ran, including its final full-health update.
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            target.Health >= MotherBrainHealthSounds.MinimumEnergy &&
            MotherBrainCadenceDefinitions.BabyHealSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter))
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Low);

        // The Baby's drain step sets the body's animation from the Baby, which runs after the body.
        // The reusable sequence retains the requested animation, but the physical body already
        // completed its AI and animation pass earlier this frame; start the animation here so it
        // begins on the next enemy pass, as it does in the game.
        if (step.BodyStumbleRequested)
        {
            MotherBrainBodySegment bodySegment = sequence.RequestedBodyAnimation ?? throw new InvalidDataException(
                "The Baby asked Mother Brain's body to stumble without naming its animation.");
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, bodySegment);
        }

        // Cross-enemy writes happen during the later Baby place and become visible in
        // Mother Brain's body immediately, though body AI cannot execute them until the next frame.
        state.Phase = sequence.State.Phase;

        if (baby.Animation != state.BabyAppliedAnimation)
            SetBabyMetroidCutsceneAnimation(state, place, baby.Animation);
        CopyBabyMetroidActorToPhysicalPlace(place, baby, _motherBrainEncounter.Colours);
        ApplyBabyMetroidFrameEffects(step);
    }

    /// <summary>Copies cutscene position, contact, processing, presence, shading and health state into the live enemy.</summary>
    private static void CopyBabyMetroidActorToPhysicalPlace(
        RoomEnemyPlace place,
        BabyMetroidCutsceneState baby,
        MotherBrainColours colours)
    {
        place.Position = baby.Position;
        place.Contact = baby.Contact;
        place.Processing = baby.Processing;
        place.Presence = baby.Presence;
        place.Shading = baby.HurtFlashShown ? EnemyShades.HurtFlashLook : colours.BabyShading;
        place.Health = baby.Health;
    }

    /// <summary>Applies one step's music, sounds, dust, fade and recovery-light requests to room systems.</summary>
    internal void ApplyBabyMetroidFrameEffects(BabyMetroidCutsceneStepResult step)
    {
        if (step.FatalBlowStarted)
        {
            _audio.QueueEnemySound(RoomSound.BabyMetroidDeath, priority: SoundRequestPriority.Normal);
            _motherBrainEncounter.Colours.ShowLightsOut();
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

        if (step.BabyFadeShade is int shade)
            ShowBabyMetroidFade(shade);

        if (step.RecoveryLightsFrame is int lightsFrame)
            LoadMotherBrainRecoveryLights(lightsFrame);
    }

    /// <summary>Shows room-light recovery frame <paramref name="frame"/> (0..6) from the installed room colours.</summary>
    /// <exception cref="InvalidDataException">The frame is not one of the seven authored images.</exception>
    internal void LoadMotherBrainRecoveryLights(int frame)
    {
        if ((uint)frame >= MotherBrainRoomColorDefinitions.RecoveryLightsFrames)
            throw new InvalidDataException($"Mother Brain room-light recovery frame {frame} is not authored.");
        _motherBrainEncounter.Colours.ShowLightsRecovering(frame);
    }

    /// <summary>Shows the Baby's black-fade shade <paramref name="shade"/> (1..6) from its installed colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shade is not one of the six authored images.</exception>
    internal void ShowBabyMetroidFade(int shade)
    {
        if (shade is < 1 or > BabyMetroidCutsceneColorDefinitions.FadeFrameCount)
            throw new ArgumentOutOfRangeException(nameof(shade), shade, "The Baby's fade has six shades.");
        _motherBrainEncounter.Colours.ShowBabyFade(shade);
    }

    /// <summary>Runs BabyMetroidCutscene's main AI.</summary>
    internal void RunBabyMetroidCutsceneMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMotherBrainBabyMetroidMain(place, frame.Samus, frame.Camera);
    }

    /// <summary>
    /// Plays the Baby's animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and a newly shown frame raises the new-frame flag. A held pose rests: the timer
    /// wraps from zero and the last frame stays.
    /// </summary>
    internal void AdvanceBabyMetroidCutsceneAnimation(RoomEnemyPlace place)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The cutscene Baby's animation ran without Mother Brain's encounter state.");
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<MotherBrainBabySegment> playback = state.BabyPlayback;
        EnemyAnimationPlayback.Play(place, MotherBrainBabyAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of,
            cue => throw new InvalidOperationException($"The cutscene Baby's animations raise no event {cue}."));
        state.BabyPlayback = playback;
    }

    /// <summary>Restarts the Baby's place on <paramref name="segment"/>, as starting an enemy's animation does.</summary>
    private static void SetBabyMetroidCutsceneAnimation(MotherBrainEnemyState state, RoomEnemyPlace place, MotherBrainBabySegment segment)
    {
        state.BabyPlayback = MotherBrainBabyAnimations.Starting(segment);
        state.BabyAppliedAnimation = segment;
        place.AnimationClock.Restart();
    }
}
