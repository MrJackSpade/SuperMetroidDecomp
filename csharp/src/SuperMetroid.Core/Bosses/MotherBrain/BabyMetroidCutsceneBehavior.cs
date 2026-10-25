using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid cutscene species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    /// <summary>Queues healing, death, and cutscene cues with native queue limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Allocates the Baby in the first free native enemy slot.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies Baby and Mother Brain color changes to live CGRAM.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies the enemy-frame counter used to gate incremental healing sound.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns release dust and Baby death explosions.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Shares physical body, head, rainbow sequence, and requested music state with Mother Brain.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Connects the cutscene Baby to its Mother Brain encounter and room services.</summary>
    internal BabyMetroidCutsceneBehavior(BabyMetroidCutsceneBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
    }

    /// <summary>Allocates the Baby in the first free native slot, runs its initializer, and preserves the post-init empty map until its first scheduled turn.</summary>
    internal void SpawnMotherBrainBabyMetroid(MotherBrainEnemyState state)
    {
        if (state.BabyMetroidSlot is { DefinitionId: not 0 })
        {
            throw new InvalidOperationException(
                "Mother Brain attempted to allocate the cutscene Baby more than once.");
        }

        // `$A0:9275` searches from native slot zero for the first consecutive free run.
        // The Baby header declares one part, so the first zero ID is the exact result.
        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
            return; // Generic SpawnEnemy returns $FFFF and X=$0800 when the pool is full.

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        if (population.DefinitionId != EnemyDefinitionId.BabyMetroidCutscene)
        {
            throw new InvalidDataException(
                "Mother Brain Baby spawn record " +
                $"names enemy {population.DefinitionId}, not BabyMetroidCutscene.");
        }

        RoomEnemySlot babySlot = _slotTable.Slots[slotIndex];
        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(population.DefinitionId);
        _dispatch.InitializeSlotFromDefinition(babySlot, population, definition);
        _dispatch.RunInitializationAi(babySlot, EnemySpawnSurroundings.None);

        // SpawnEnemy installs the canonical empty map after init when property $2000 is set.
        // The actor's list is still live and produces its first visible map on its first
        // scheduled turn; the active-index array was frozen before this allocation.
        babySlot.Frame = babySlot.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? EnemyFrameId.Empty
            : null;
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * EnemySlotTable.EnemyIndexStride));
    }

    /// <summary>Creates the physical Baby only after the rainbow sequence exists and installs its first list and fifteen nontransparent palette colors.</summary>
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
        baby.Initialize(slot.Position, slot.Properties);
        state.BabyMetroidSlot = slot;
        state.BabyMetroid = baby;
        state.BabyAppliedProgram = baby.Program;
        state.BabyCursor = AnimationProgramCursor<MotherBrainBabyProgram>.Start(baby.Program);

        CopyBabyMetroidActorToPhysicalSlot(slot, baby);
        slot.InstructionTimer = baby.InstructionTimer;
        slot.Timer = WrappingCountdown.Start(0);

        // The writer receives byte index $01E2 and count $000F. Color zero is deliberately
        // skipped at source `$94D2+2`, preserving the room's transparent backdrop entry.
        LoadBabyMetroidCutsceneInitialPalette();
    }

    /// <summary>Copies installed Baby start colors while leaving CGRAM's transparent entry untouched.</summary>
    internal void LoadBabyMetroidCutsceneInitialPalette()
    {
        var colors = _bossArtwork.Artwork?.BabyMetroidCutsceneColors ?? throw new InvalidOperationException(
            "Baby Metroid cutscene requires installed colors.");
        for (int color = 0; color < BabyMetroidCutsceneColorDefinitions.InitialColorCount; color++)
            _presentation.BossColours().BabyMetroidCutscene.Write(color, colors.InitialColor(color));
    }

    /// <summary>Advances the later Baby slot, publishes cries/healing, and writes body and Baby list changes for their next ordered enemy turns.</summary>
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

        slot.ShakeTimer = WrappingCountdown.Start(0);
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.IdleUntilNoHealth)
        {
            // `$A9:C7B7` clears the complete shared word and queues only one `$72`, even
            // when several rings incremented it before this later enemy slot ran.
            state.PendingBabyCryCount = 0;
            state.LastSoundEffect = ShitroidSoundDefinitions.DrainCry;
        }
        BabyMetroidCutsceneStepResult step = baby.Step(
            target,
            sequence,
            state.Head.Position.Whole,
            layer1: camera,
            enemyFrameCounter: slot.FrameCounter,
            randomNumber: _room.Random.Read());
        // Both outcomes of the native heal helper call the sound helper after the
        // energy write. Use the phase that ran, including its final full-health call.
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            target.Health >= MotherBrainHealthSounds.MinimumEnergy &&
            (_frameContext.RandomEnemyCounter & MotherBrainHealthSounds.EnemyClockMask) == 0)
            _audio.QueueEnemySound(Library3Sound.IncrementalEnergy, maximumQueued: SoundQueueLimit.Max3);

        // `$A9:C879` writes the body enemy's instruction words from this later Baby slot.
        // The reusable sequence retains the requested list, but the physical body already
        // completed its AI/instruction pass earlier this frame; install the list here so it
        // begins on the next increasing-slot enemy pass exactly like the cross-WRAM write.
        if (step.BodyStumbleRequested)
        {
            MotherBrainBodyProgram bodyProgram = sequence.RequestedBodyProgram ?? throw new InvalidDataException(
                "Baby requested Mother Brain body movement without a body program.");
            MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, bodyProgram);
        }

        // Cross-enemy writes happen during the later Baby slot and become visible in the
        // body record immediately, though body AI cannot execute them until the next frame.
        state.Function = sequence.State.Phase ?? throw new InvalidDataException("The live Mother Brain rainbow sequence has no body function yet.");

        if (baby.Program != state.BabyAppliedProgram)
            SetBabyMetroidCutsceneProgram(state, slot, baby.Program);
        CopyBabyMetroidActorToPhysicalSlot(slot, baby);
        ApplyBabyMetroidFrameEffects(state, step);
    }

    /// <summary>Copies cutscene position, property, palette, tile, and health state into the live enemy record.</summary>
    private static void CopyBabyMetroidActorToPhysicalSlot(
        RoomEnemySlot slot,
        BabyMetroidCutsceneState baby)
    {
        slot.Position = baby.Position;
        slot.Properties = baby.Properties;
        slot.PaletteIndex = baby.HitFlashPaletteShown ? (ushort)0 : EnemyPaletteBits.Palette7;
        slot.TileIndexBase = baby.GraphicsOffset.Raw;
        slot.Health = baby.Health;
    }

    /// <summary>Applies one step's music, sounds, dust, palette, attack-tile, and recovery-light requests to room systems.</summary>
    internal void ApplyBabyMetroidFrameEffects(
        MotherBrainEnemyState state,
        BabyMetroidCutsceneStepResult step)
    {
        if (step.FatalBlowStarted)
        {
            _audio.QueueEnemySound(Library3Sound.BabyMetroidDeath, maximumQueued: SoundQueueLimit.Max6);
            SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRoomColorPresentation.ApplyBabyMetroidDeathBlackout(_presentation.Colours);
            state.RequestMusic(MusicRequest.Stop, MusicQueueDelay.Shortest);
        }
        if (step.SamusThemeStarted)
        {
            state.RequestMusic(MusicRequest.LoadDataSet(MusicDataSet.SamusThemeUpperCrateria),
                MusicQueueDelay.Shortest);
            state.RequestMusic(MusicRequest.SelectTrack(MotherBrainBabyMetroidDefinitions.SamusThemeMusicTrack),
                MusicQueueDelay.Shortest);
        }
        if (step.LatchSoundQueued)
            state.LastSoundEffectLibrary1 = MotherBrainSoundDefinitions.StartRainbowDrain;
        if (step.AmbientCrySoundQueued)
            state.LastSoundEffect = ShitroidSoundDefinitions.RemorseCry;

        foreach (BabyMetroidReleaseDustRequest dust in step.ReleaseDustClouds)
            _projectiles.SpawnRoomGraphicsDustExplosion(dust.Position, dust.Animation);

        if (step.DeathExplosion is { } explosion)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(explosion.Position, explosion.Animation);
            // $A9:CDF4 queues the explosion through QueueSound_Lib3_Max3.
            state.LastSoundEffectLibrary3 = explosion.SoundEffect;
        }

        if (step.BabyFadeShade is int shade)
            LoadBabyMetroidCutsceneFadePalette(shade);

        if (step.AttackPage is { } attackPage)
            _motherBrainEncounter.ShowSpritePage(attackPage);

        if (step.RecoveryLightsFrame is int lightsFrame)
            LoadMotherBrainRecoveryLights(lightsFrame);
    }

    /// <summary>Shows room-light recovery frame <paramref name="frame"/> (0..6) from the installed room colours.</summary>
    /// <exception cref="InvalidDataException">The frame is not one of the seven authored images.</exception>
    internal void LoadMotherBrainRecoveryLights(int frame) =>
        (_bossArtwork.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain recovery requires installed room colors."))
            .ApplyRecoveryLights(_presentation.Colours, frame);

    /// <summary>Shows the Baby's black-fade shade <paramref name="shade"/> (1..6) from its installed colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shade is not one of the six authored images.</exception>
    internal void LoadBabyMetroidCutsceneFadePalette(int shade)
    {
        var colors = _bossArtwork.Artwork?.BabyMetroidCutsceneColors ?? throw new InvalidOperationException(
            "Baby Metroid fade requires installed colors.");
        for (int color = 0; color < BabyMetroidCutsceneColorDefinitions.FadeColorCount; color++)
            _presentation.BossColours().BabyMetroidCutscene.Write(color, colors.FadeColor(shade, color));
    }

    /// <summary>Ports BabyMetroidCutscene's main AI.</summary>
    internal void RunBabyMetroidCutsceneMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMotherBrainBabyMetroidMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>
    /// Runs the Baby's program where the enemy instruction interpreter would: the frame expires on the update its
    /// native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvanceBabyMetroidCutsceneAnimation(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The cutscene Baby's program ran without Mother Brain's encounter state.");
        CountdownStep instructionTimer = slot.InstructionTimer.Decrement();
        slot.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<MotherBrainBabyProgram> cursor = state.BabyCursor;
        WrappingCountdown counter = slot.Timer;
        AnimationProgramAdvance<BabyMetroidFrame> advance = MotherBrainBabyAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => throw new InvalidOperationException($"The cutscene Baby's programs publish no cue {cue}."));
        slot.Timer = counter;
        state.BabyCursor = cursor;
        switch (advance)
        {
            case AnimationProgramAdvance<BabyMetroidFrame>.Show shown:
                slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                slot.Frame = BabyMetroidVisualDefinitions.ShownFrames.Of(shown.Frame);
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvance<BabyMetroidFrame>.Hold:
                // The native sleep: the timer wraps from zero and the last frame stays.
                return;
            default:
                throw new InvalidOperationException($"The cutscene Baby's programs have no {advance} step.");
        }
    }

    /// <summary>Restarts the Baby's slot on <paramref name="program"/>, as the native set-list helper does.</summary>
    private static void SetBabyMetroidCutsceneProgram(MotherBrainEnemyState state, RoomEnemySlot slot, MotherBrainBabyProgram program)
    {
        state.BabyCursor = AnimationProgramCursor<MotherBrainBabyProgram>.Start(program);
        state.BabyAppliedProgram = program;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }
}
