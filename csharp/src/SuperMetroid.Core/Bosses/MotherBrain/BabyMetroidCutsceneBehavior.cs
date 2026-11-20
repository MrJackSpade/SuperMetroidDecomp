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
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup) => EnemyOccupant.Of(new BabyMetroidCutsceneEntity(this, occupant, place, common, setup));

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
    /// Builds <paramref name="lateFight"/>'s Baby in the first free place of the room's enemies, runs its setup, and leaves
    /// it undrawn until its first scheduled turn.
    /// </summary>
    internal void SpawnMotherBrainBabyMetroid(MotherBrainRainbowBeamAttackSequence lateFight)
    {
        if (Ensure.NotNull(lateFight).State.Baby is not BabyMetroidArrival.Awaited)
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
        // The Baby's setup builds the late fight's Baby in its place (#2218, #2471).
        _lifecycle.SetUpJoiningEnemy(babyPlace, lateFight);

        // Spawning leaves the Baby undrawn after its setup while it animates.
        // Its animation is still live and shows its first visible frame on its first
        // scheduled turn; this update's list of running enemies was fixed before the Baby was placed.
        babyPlace.Frame = babyPlace.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.PointAtOrigin
            : null;
        _places.CoverSpawn(babyPlace);
    }

    /// <summary>Builds <paramref name="lateFight"/>'s Baby in <paramref name="place"/> and starts its first animation, showing its own colours as it arrives.</summary>
    internal static BabyMetroidCutsceneState InitializeMotherBrainBabyMetroid(RoomEnemyPlace place, MotherBrainRainbowBeamAttackSequence lateFight)
    {
        Ensure.NotNull(lateFight);
        MotherBrainEnemyState state = lateFight.Encounter;
        // The place keeps its previous occupant's fractions, which the Baby starts from (BabyMetroidArrivalFractions).
        BabyMetroidCutsceneState baby = lateFight.BabyArrives();
        // The place owns the Baby's contact, processing and presence (#2310): initialization makes it block the Plasma
        // Beam and starts its animation; its population entry's presence stands.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);

        ShowBabyMetroidShading(place, baby, state.Colours);
        // The Baby's opening pulse, which it is made playing, plays its first step on the next update.
        place.AnimationClock.Restart();

        // The Baby arrives in its own colours; where it is transparent the room's backdrop shows.
        ShowBabyMetroidArrival(state.Colours);
        return baby;
    }

    /// <summary>Shows the Baby's arrival colours.</summary>
    private static void ShowBabyMetroidArrival(MotherBrainColours colours) =>
        Ensure.NotNull(colours).ShowBabyArrived();

    /// <summary>Advances the later Baby place, publishes cries/healing, and writes body and Baby animation changes for their next ordered enemy turns.</summary>
    private void RunMotherBrainBabyMetroidMain(
        RoomEnemyPlace place, BabyMetroidCutsceneState baby,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = baby.LateFight.Encounter;

        place.Shake.End();
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.IdleUntilNoHealth)
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
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            samus.Health >= MotherBrainHealthSounds.MinimumEnergy &&
            MotherBrainCadenceDefinitions.BabyHealSound.IsDue(_frameContext.EnemyPassCount))
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Low);


        if (step.StartedAnimation is { } started)
            baby.Playback = EnemyAnimationPlayback.Start(place, MotherBrainBabyAnimations.StartingAt[started]);
        ShowBabyMetroidShading(place, baby, state.Colours);
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
    private static void ShowBabyMetroidFade(MotherBrainColours colours, MotherBrainFadeImage image) =>
        Ensure.NotNull(colours).ShowBabyFade(image);

    /// <summary>Runs BabyMetroidCutscene's main AI.</summary>
    internal void RunBabyMetroidCutsceneMainAi(RoomEnemyPlace place, BabyMetroidCutsceneState state, IEnemyAiFrame frame)
    {
        RunMotherBrainBabyMetroidMain(place, state, frame.Samus, frame.Camera);
    }

    /// <summary>
    /// Plays the Baby's animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and the clock notes each newly shown frame. A held pose rests: the last frame stays and the clock
    /// stops.
    /// </summary>
    internal static void AdvanceBabyMetroidCutsceneAnimation(RoomEnemyPlace place, BabyMetroidCutsceneState baby)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<MotherBrainBabySegment> playback = baby.Playback;
        EnemyAnimationPlayback.Play(place, MotherBrainBabyAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of,
            cue => throw new InvalidOperationException($"The cutscene Baby's animations raise no event {cue}."));
        baby.Playback = playback;
    }
}
