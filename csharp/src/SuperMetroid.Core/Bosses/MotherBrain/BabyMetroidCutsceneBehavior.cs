using SuperMetroid.Core.Programs;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BabyMetroidCutsceneEntity(this, slot, common);

    /// <summary>Initializes the dynamically allocated Baby through ordinary enemy dispatch.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Supplies current room bus, RNG, and installed Baby artwork.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues healing, death, and cutscene cues with queue limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Allocates the Baby in the first free enemy slot.</summary>
    private readonly EnemySlotTable _slotTable;
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
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
    }

    /// <summary>Allocates the Baby in the first free slot, runs its initializer, and preserves the post-init empty map until its first scheduled turn.</summary>
    internal void SpawnMotherBrainBabyMetroid(MotherBrainEnemyState state)
    {
        if (state.BabyMetroidSlot is { DefinitionId: not 0 })
        {
            throw new InvalidOperationException(
                "Mother Brain attempted to allocate the cutscene Baby more than once.");
        }

        // The spawn takes the first free slot; the Baby is a single part. A full room spawns nothing.
        if (_slotTable.FindFreeSlot() is not { } babySlot)
            return;

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        if (population.DefinitionId != EnemyDefinitionId.BabyMetroidCutscene)
        {
            throw new InvalidDataException(
                "Mother Brain Baby spawn record " +
                $"names enemy {population.DefinitionId}, not BabyMetroidCutscene.");
        }

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(population.DefinitionId);
        _dispatch.InitializeSlotFromDefinition(babySlot, population, definition);
        _dispatch.RunInitializationAi(babySlot, EnemySpawnSurroundings.None);

        // SpawnEnemy installs the canonical empty map after init when the Baby's hide-drawing property is set.
        // The actor's animation is still live and produces its first visible map on its first
        // scheduled turn; the active-index array was frozen before this allocation.
        babySlot.Frame = babySlot.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.Empty
            : null;
        _slotTable.CoverSpawn(babySlot);
    }

    /// <summary>Creates the physical Baby only after the rainbow sequence exists and starts its first animation and fifteen nontransparent palette colors.</summary>
    internal void InitializeMotherBrainBabyMetroid(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The cutscene Baby was initialized without Mother Brain's physical records.");
        if (state.RainbowBeamSequence is null)
        {
            throw new InvalidDataException(
                "The cutscene Baby was initialized before the final rainbow sequence.");
        }

        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(slot.Position, slot.Contact, slot.Processing, slot.Presence);
        state.BabyMetroidSlot = slot;
        state.BabyMetroid = baby;
        state.BabyAppliedAnimation = baby.Animation;
        state.BabyPlayback = MotherBrainBabyAnimations.Starting(baby.Animation);

        CopyBabyMetroidActorToPhysicalSlot(slot, baby, _motherBrainEncounter.Colours);
        slot.AnimationClock.RestartAfter(baby.FrameHold);

        // The writer receives colour index 241 and count 15. Color zero is deliberately
        // skipped in the source, preserving the room's transparent backdrop entry.
        ShowBabyMetroidArrival();
    }

    /// <summary>Lays the Baby's start colours, which leave the transparent entry untouched.</summary>
    internal void ShowBabyMetroidArrival() =>
        _motherBrainEncounter.Colours.ShowBabyArrived();

    /// <summary>Advances the later Baby slot, publishes cries/healing, and writes body and Baby animation changes for their next ordered enemy turns.</summary>
    internal void RunMotherBrainBabyMetroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "The cutscene Baby lost Mother Brain's encounter state.");
        BabyMetroidCutsceneState baby = state.BabyMetroid ??
            throw new InvalidOperationException("The physical Baby has no cutscene state.");
        if (!ReferenceEquals(slot, state.BabyMetroidSlot))
        {
            throw new InvalidDataException(
                $"Baby state names slot {state.BabyMetroidSlot?.SlotIndex}, but dispatcher " +
                $"ran slot {slot.SlotIndex}.");
        }

        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException("The cutscene Baby lost the rainbow sequence.");
        SamusState target = samus ?? throw new InvalidOperationException(
            "The cutscene Baby requires the live Samus actor.");

        slot.ShakeTimer = 0;
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.IdleUntilNoHealth)
        {
            // The ring hit check clears the complete shared value and queues only one cry, even
            // when several rings incremented it before this later enemy slot ran.
            state.PendingBabyCryCount = 0;
            _motherBrainEncounter.RequestBodySound(ShitroidSoundDefinitions.DrainCry);
        }
        BabyMetroidCutsceneStepResult step = baby.Step(
            target,
            sequence,
            state.Head.Position.Whole,
            cameraPosition: camera,
            enemyFrameCounter: slot.FrameCounter,
            randomNumber: _room.Random.Read());
        // Both outcomes of the heal helper call the sound helper after the
        // energy write. Use the phase that ran, including its final full-health call.
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            target.Health >= MotherBrainHealthSounds.MinimumEnergy &&
            MotherBrainCadenceDefinitions.BabyHealSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter))
            _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Low);

        // The Baby's drain step writes the body enemy's animation from this later Baby slot.
        // The reusable sequence retains the requested animation, but the physical body already
        // completed its AI and animation pass earlier this frame; start the animation here so it
        // begins on the next increasing-slot enemy pass exactly like the cross-actor write.
        if (step.BodyStumbleRequested)
        {
            MotherBrainBodySegment bodySegment = sequence.RequestedBodyAnimation ?? throw new InvalidDataException(
                "Baby requested Mother Brain body movement without a body program.");
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, bodySegment);
        }

        // Cross-enemy writes happen during the later Baby slot and become visible in the
        // body record immediately, though body AI cannot execute them until the next frame.
        state.Function = sequence.State.Phase ?? throw new InvalidDataException("The live Mother Brain rainbow sequence has no body function yet.");

        if (baby.Animation != state.BabyAppliedAnimation)
            SetBabyMetroidCutsceneAnimation(state, slot, baby.Animation);
        CopyBabyMetroidActorToPhysicalSlot(slot, baby, _motherBrainEncounter.Colours);
        ApplyBabyMetroidFrameEffects(step);
    }

    /// <summary>Copies cutscene position, property, palette, tile, and health state into the live enemy record.</summary>
    private static void CopyBabyMetroidActorToPhysicalSlot(
        RoomEnemySlot slot,
        BabyMetroidCutsceneState baby,
        MotherBrainColours colours)
    {
        slot.Position = baby.Position;
        slot.Contact = baby.Contact;
        slot.Processing = baby.Processing;
        slot.Presence = baby.Presence;
        slot.Shading = baby.HurtFlashShown ? EnemyShades.HurtFlashLook : colours.BabyShading;
        slot.Health = baby.Health;
    }

    /// <summary>Applies one step's music, sounds, dust, palette, attack-tile, and recovery-light requests to room systems.</summary>
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
            _projectiles.SpawnRoomGraphicsDustExplosion(dust.Position, dust.Animation);

        if (step.DeathExplosion is { } explosion)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(explosion.Position, explosion.Animation);
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
    internal void RunBabyMetroidCutsceneMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMotherBrainBabyMetroidMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>
    /// Plays the Baby's animation where the shared enemy animation runs: the frame expires on the update its
    /// timer would, and a newly shown frame raises the new-frame flag. A held pose is the script's sleep: the timer
    /// wraps from zero and the last frame stays.
    /// </summary>
    internal void AdvanceBabyMetroidCutsceneAnimation(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The cutscene Baby's animation ran without Mother Brain's encounter state.");
        if (!slot.AnimationClock.Expire())
            return;
        AnimationPlayback<MotherBrainBabySegment> playback = state.BabyPlayback;
        EnemyAnimationPlayback.Play(slot, MotherBrainBabyAnimations.Segments, ref playback, BabyMetroidVisualDefinitions.ShownFrames.Of,
            cue => throw new InvalidOperationException($"The cutscene Baby's animations raise no event {cue}."));
        state.BabyPlayback = playback;
    }

    /// <summary>Restarts the Baby's slot on <paramref name="segment"/>, as starting an enemy's animation does.</summary>
    private static void SetBabyMetroidCutsceneAnimation(MotherBrainEnemyState state, RoomEnemySlot slot, MotherBrainBabySegment segment)
    {
        state.BabyPlayback = MotherBrainBabyAnimations.Starting(segment);
        state.BabyAppliedAnimation = segment;
        slot.AnimationClock.Restart();
    }
}
