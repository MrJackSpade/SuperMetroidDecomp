using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The Phantoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PhantoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PhantoonPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PhantoonEntity(this, slot, common);

    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the body, eye, mouth, and tentacle slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies body palettes and BG2 tilemap changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Phantoon's entrance: the starting flames and its wavy fade-in.</summary>
    internal PhantoonOpening Opening { get; }

    /// <summary>Phantoon's flames as projectiles, its hitboxes and body touch, and what Samus's weapons do to it.</summary>
    internal PhantoonCombat Combat { get; }

    /// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
    internal PhantoonPresentation Presentation { get; }

    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    internal PhantoonPatterns Patterns { get; }

    /// <summary>Phantoon's death: the fatal swoop, the fades, explosions and wavy mosaic, and Wrecked Ship's power returning.</summary>
    internal PhantoonDeath Death { get; }


    /// <summary>Binds room and enemy services for the four-part boss actor.</summary>
    internal PhantoonBehavior(PhantoonBehaviorDependencies dependencies)
    {
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        Presentation = new PhantoonPresentation(_room, _presentation, dependencies.FrameContext);
        Opening = new PhantoonOpening(dependencies.Projectiles, _audio, Presentation);
        Combat = new PhantoonCombat(_room, dependencies.Projectiles, dependencies.Earthquake, _audio, _slotTable, dependencies.Collision, dependencies.DeathsAndDrops);
        Patterns = new PhantoonPatterns(_room, dependencies.Projectiles, _audio, _presentation, dependencies.FrameContext, Presentation);
        Death = new PhantoonDeath(_room, dependencies.Projectiles, _presentation, dependencies.FrameContext, dependencies.DeathsAndDrops, Presentation, Patterns);
    }

    /// <summary>Native per-update increase in introductory wave amplitude.</summary>
    internal const ushort PhantoonIntroAmplitudeDelta = 64;
    /// <summary>Wave amplitude at which the intro advances to the next phase.</summary>
    internal const ushort PhantoonIntroMaximumAmplitude = 3072;
    /// <summary>Per-update phase increment for the wavy BG2 distortion.</summary>
    internal const ushort PhantoonWavyPhaseDelta = 8;

    /// <summary>Ports <c>InitAI_PhantoonBody</c> at $A7:CDF3.</summary>
    internal void InitializePhantoonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Phantoon body requires native slot zero, not slot {body.SlotIndex}.");
        }

        Combat.Phantoon = new PhantoonEnemyState(body)
        {
            BackgroundTilemapPrepared = true,
        };

        // Native clears all $1000 bytes of the enemy BG2 working image to tile $0338. Phantoon owns that image
        // (#1353): ProcessExtendedTilemap draws the body maps into it and it publishes as his body on BG2.
        _presentation.AttachBg2Image(new EnemyBg2Image(PhantoonBg2FrameDefinitions.BlankTile));

        // Target BG palette seven is black at fight start. In the software PPU CGRAM is the
        // renderer-visible incarnation of native current/target palette storage; clearing
        // these exact sixteen words makes the later health-palette interpolation observable.
        _presentation.BossPalettes.PhantoonBody.Fill(Bgr555.Black);

        Combat.Part(body).FunctionTimer = PhantoonTimerDefinitions.InitialFlameDelayFrames;
        Combat.Part(body).StartingFlameIndex = 0;
        Combat.Part(body).StartingFlamesActive = false;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        InitializePhantoonPart(body, PhantoonPartProgram.InvulnerableBody);
    }

    /// <summary>Ports the shared part initializer at $A7:CE55.</summary>
    internal void InitializePhantoonPart(RoomEnemySlot part)
    {
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        PhantoonPartProgram program = part.Spawn.Population.Parameter2 switch
        {
            1 => PhantoonPartProgram.EyeClosed,
            2 => PhantoonPartProgram.Tentacles,
            3 => PhantoonPartProgram.MouthInitial,
            _ => throw new InvalidDataException(
                $"Phantoon part slot {part.SlotIndex} has invalid part index ${part.Spawn.Population.Parameter2:X4}."),
        };
        InitializePhantoonPart(part, program);

        switch (part.Spawn.Population.Parameter2)
        {
            case 1:
                state.Eye = part;
                break;
            case 2:
                state.Tentacles = part;
                break;
            case 3:
                state.Mouth = part;
                break;
            default:
                throw new InvalidOperationException($"Phantoon part slot {part.SlotIndex} has invalid part index ${part.Spawn.Population.Parameter2:X4}.");
        }
    }

    /// <summary>Copies body presentation state into a physical part and starts its chosen program.</summary>
    private void InitializePhantoonPart(RoomEnemySlot part, PhantoonPartProgram program)
    {
        PhantoonEnemyState state = Combat.Phantoon ??
            throw new InvalidOperationException("Phantoon body must initialize before its parts.");
        part.Frame = EnemyFrameId.Empty;
        part.InstructionTimer = NativeCountdown.Start(1);
        part.Timer = NativeCountdown.Start(0);
        part.PaletteIndex = state.Body.PaletteIndex;
        part.VramTilesIndex = state.Body.VramTilesIndex;
        state.PartProgram(part) = AnimationProgramCursor<PhantoonPartProgram>.Start(program);
        PhantoonPartState words = state.Parts[part.SlotIndex];
        words.InstallSpawnParameters(part.Spawn.Population);
        part.SetSpeciesState(words, words.ClearWithRecord);
        state.Parts[part.SlotIndex].Function = PhantoonAiFunction.SpawnStartingFlames;

        // Every call writes these shared aliases, including the body fallthrough. Slot three
        // owns the casual-flame pattern index/control words used much later in the fight.
        if (_slotTable.Slots.Count > 3)
        {
            state.Parts[3].BlendingControl = 0;
            state.Parts[3].CasualFlameCount = PhantoonCasualFlameDefinitions.NoActiveSchedule;
        }
    }

    /// <summary>Ports <c>MainAI_Phantoon</c> at $A7:CEA6.</summary>
    private void RunPhantoonMain(
        RoomEnemySlot body,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(body);
        state.LastMaterializationSound = null;
        PhantoonAiFunction dispatched = state.Parts[body.SlotIndex].Function;

        switch (dispatched)
        {
            case PhantoonAiFunction.SpawnStartingFlames:
                Opening.RunPhantoonStartingFlameSpawner(body, state);
                break;
            case PhantoonAiFunction.WaitBeforeActivatingStartingFlames:
                PhantoonOpening.RunPhantoonStartingFlamePause(state.Parts[body.SlotIndex]);
                break;
            case PhantoonAiFunction.WaitForStartingFlamesToDisappear:
                PhantoonOpening.RunPhantoonStartingFlameOrbitWait(body, state);
                break;
            case PhantoonAiFunction.WavyFadeIn:
                Opening.RunPhantoonWavyFadeIn(body, state);
                break;
            case PhantoonAiFunction.PickFirstRoundPattern:
                Patterns.RunPhantoonPickFirstRoundPattern(body, state);
                break;
            case PhantoonAiFunction.MoveInFigureEightThenOpenEye:
                Patterns.RunPhantoonFirstRoundFigureEight(body, state);
                break;
            case PhantoonAiFunction.EyeTracksSamus:
                PhantoonPatterns.RunPhantoonEyeTracking(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.BecomeSolidAndSwoop:
                PhantoonPatterns.BeginPhantoonVulnerableSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.Swooping:
                PhantoonPatterns.RunPhantoonSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.FadeOutWhileSwooping:
                Patterns.RunPhantoonFadeOutWhileSwooping(
                    body,
                    state,
                    RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.WaitAfterFadeOut:
                PhantoonPatterns.RunPhantoonHiddenWait(state.Parts[body.SlotIndex]);
                break;
            case PhantoonAiFunction.PickNextAppearance:
                Patterns.PlacePhantoonForNextFigureEight(body, state);
                break;
            case PhantoonAiFunction.FadeInBeforeFigureEight:
                Patterns.RunPhantoonFadeInBeforeFigureEight(body, state);
                break;
            case PhantoonAiFunction.BecomeSolidAfterFlameRain:
                PhantoonPatterns.BeginPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonAiFunction.FadeInDuringFlameRain:
                Patterns.RunPhantoonFlameRainFadeIn(body, state);
                break;
            case PhantoonAiFunction.TrackSamusDuringFlameRain:
                PhantoonPatterns.RunPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonAiFunction.FadeOutDuringFlameRain:
                Patterns.RunPhantoonFlameRainFadeOut(body, state);
                break;
            case PhantoonAiFunction.SpawnFlameRain:
                Patterns.RunPhantoonHiddenFlameRain(body, state);
                break;
            case PhantoonAiFunction.FadeOutBeforeFirstFlameRain:
                Patterns.RunPhantoonInitialFlameRain(body, state);
                break;
            case PhantoonAiFunction.FadeOutBeforeRage:
                Patterns.RunPhantoonFadeOutBeforeRage(body, state);
                break;
            case PhantoonAiFunction.MoveToTopCenterForRage:
                PhantoonPatterns.RunPhantoonRageHiddenWait(body, state);
                break;
            case PhantoonAiFunction.FadeInForRage:
                Patterns.RunPhantoonRageFadeIn(body, state);
                break;
            case PhantoonAiFunction.Enraged:
                Patterns.RunPhantoonRage(body, state);
                break;
            case PhantoonAiFunction.FadeOutAfterRage:
                Patterns.RunPhantoonRageFadeOut(body, state);
                break;
            case PhantoonAiFunction.FinishFatalSwoop:
                PhantoonDeath.RunPhantoonFatalSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonAiFunction.DyingFadeInOut:
                Death.RunPhantoonDyingFadeCycles(body, state);
                break;
            case PhantoonAiFunction.DyingExplosions:
                Death.RunPhantoonDyingExplosions(body, state);
                break;
            case PhantoonAiFunction.BeginFinalWavyDeath:
                PhantoonDeath.BeginPhantoonWavyMosaicDeath(body, state);
                break;
            case PhantoonAiFunction.DyingFadeOut:
                Death.RunPhantoonWavyMosaicDeath(body, state);
                break;
            case PhantoonAiFunction.AlmostDead:
                Death.ClearPhantoonDeathGraphics(body, state);
                break;
            case PhantoonAiFunction.Dead:
                Death.ActivateWreckedShipAfterPhantoon(body, state);
                break;
            case PhantoonAiFunction.NoOperation:
                break;
            default:
                throw new InvalidDataException(
                    $"Phantoon function {state.Parts[body.SlotIndex].Function} is not translated.");
        }

        // The source contains a documented X-register bug that makes only the slot-zero
        // invocation run this synchronization block. Our main dispatcher likewise calls it
        // only for the body, then copies the updated point into the three drawing records.
        // $A7:CEB2 tests the X the function returned with: $DAD7 leaves it at the VRAM
        // write-stack index, so that frame skips the part sync and BG2 scroll entirely.
        if (dispatched == PhantoonAiFunction.AlmostDead)
            return;
        state.Mouth.Position = state.Mouth.Position.WithWholeX(body.Position.X.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Mouth.Position = state.Mouth.Position.WithWholeY(body.Position.Y.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
        if (state.Parts[state.Eye.SlotIndex].WaveMode == 0)
        {
            state.Bg2Scroll = new(
                unchecked((ushort)(camera.X - body.Position.X.Whole + 40)),
                unchecked((ushort)(camera.Y - body.Position.Y.Whole + 40)));
        }
    }

    /// <summary>Requires the body's encounter state; each part property requires its attached slot when read.</summary>
    internal PhantoonEnemyState RequireCompletePhantoonState(RoomEnemySlot body) => Combat.RequirePhantoonState(body);

    /// <summary>Requires a live Samus state for Phantoon movement and eye targeting.</summary>
    private static SamusState RequirePhantoonSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Phantoon's eye and swoop programs require the active Samus actor.");

    /// <summary>Accumulated damage required to alter Phantoon's vulnerable-eye phase.</summary>
    internal const ushort PhantoonDamageThreshold = 300;

    /// <summary>Ports Phantoon's palette-only hurt AI at $A7:DD3F.</summary>
    internal void ApplyPhantoonHurt(RoomEnemySlot body, PhantoonEnemyState state)
    {
        PhantoonPartState tentacles = state.Parts[state.Tentacles.SlotIndex];
        bool restoreHealthPalette = body.FlashTimer.RawValue == 8 ||
            (body.FrameCounter & 2) == 0 && tentacles.HurtFlashShown;
        if (restoreHealthPalette)
        {
            CopyPhantoonHealthPalette(body);
            tentacles.HurtFlashShown = false;
            return;
        }

        if ((body.FrameCounter & 2) == 0 || tentacles.HurtFlashShown)
            return;

        _presentation.BossPalettes.PhantoonBody.Fill(Bgr555.White);
        tentacles.HurtFlashShown = true;
    }

    /// <summary>Copies the selected health colors into the visible body palette.</summary>
    internal void CopyPhantoonHealthPalette(RoomEnemySlot body)
    {
        int healthBand = Math.Min(7, Math.Max(0, (body.Health - 1) / 312));
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
            _presentation.BossPalettes.PhantoonBody.Write(color, Presentation.ReadPhantoonHealthColor(healthBand, color));
    }
    /// <summary>Per-update wave increment during the wavy mosaic death animation.</summary>
    internal const ushort PhantoonDeathWaveDelta = 256;
    /// <summary>Wave ceiling reached before the death animation advances.</summary>
    internal const ushort PhantoonDeathWaveMaximum = 61440;

    /// <summary>
    /// Runs a Phantoon part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its native timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvancePhantoonPartAnimation(RoomEnemySlot part)
    {
        if (!EnemyProgramAnimation.Expire(part))
            return;
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        AnimationProgramCursor<PhantoonPartProgram> cursor = state.PartProgram(part);
        // Writing the cursor back after the cues lets the program's own sleep overwrite a cue's install on this
        // part, as the native Sleep does. Phantoon's part lists load no loop counter.
        EnemyProgramAnimation.Run(part, PhantoonAnimations.Programs, ref cursor, PhantoonPartFrameSpritemaps.ShownFrames.Of,
            (cue, continuation) =>
            {
                ApplyPhantoonPartCue(part, cue);
                return null;
            });
        state.PartProgram(part) = cursor;
    }

    /// <summary>Carries out one cue of a Phantoon part program.</summary>
    internal void ApplyPhantoonPartCue(
        RoomEnemySlot part,
        PhantoonPartCue cue)
    {
        PhantoonEnemyState state = RequireCompletePhantoonStateForPart(part);
        switch (cue)
        {
            case PhantoonPartCue.PlayMaterializationSound:
                state.LastMaterializationSound =
                    PhantoonSoundDefinitions.MaterializationSound(
                        state.MaterializationSoundIndex);
                state.MaterializationSoundIndex++;
                if (state.MaterializationSoundIndex >= PhantoonSoundDefinitions.MaterializationSoundCount)
                    state.MaterializationSoundIndex = 0;
                return;

            case PhantoonPartCue.BeginEyeTracking:
                BeginPhantoonEyeTracking(state);
                // The advance resumes its own cursor after the cue, so the program's hold overwrites the
                // eye program this installs, as the native Sleep does; the body's program survives.
                return;

            case PhantoonPartCue.PickNewPattern:
                Patterns.PickPhantoonSecondRoundPattern(state);
                return;

            case PhantoonPartCue.SpawnCasualFlame:
                Patterns.SpawnPhantoonDestroyableFlame(state.Body, PhantoonFlameSpawnType.Casual, 0);
                _audio.QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlameQueueCapacity);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(PhantoonPartCue)} {cue}.");
        }
    }

    /// <summary>Starts the eye's Samus-tracking list for the current attack phase.</summary>
    internal void BeginPhantoonEyeTracking(PhantoonEnemyState state)
    {
        state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
        RoomEnemySlot body = state.Body;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.EyeHitboxBody);
        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Parts[body.SlotIndex].FunctionTimer = PhantoonTimerDefinitions.VulnerableWindow[_room.Random.Generate() & 7];
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.EyeTracksSamus;
        PhantoonPresentation.InstallPhantoonProgram(state, state.Eye, PhantoonPartProgram.EyeballCentered);
    }

    /// <summary>Requires the initialized four-slot encounter state for a part callback.</summary>
    private PhantoonEnemyState RequireCompletePhantoonStateForPart(RoomEnemySlot stateSlot) =>
        RequireCompletePhantoonState(stateSlot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody
            ? stateSlot
            : _slotTable.Slots[0]);
    /// <summary>Initial flame callback that holds position before orbit activation.</summary>
    internal const EnemyProjectilePreInstruction PhantoonStartingFlameWaitingPreInstruction =
        EnemyProjectilePreInstruction.PhantoonStartingFlames;
    /// <summary>Activated flame callback that advances its orbit around the body.</summary>
    internal const EnemyProjectilePreInstruction PhantoonStartingFlameOrbitPreInstruction =
        EnemyProjectilePreInstruction.PhantoonStartingFlames_Activated;

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Combat.Phantoon is not null)
        {
            Combat.Phantoon.LastMaterializationSound = null;
            Combat.Phantoon.LastCombatSoundEffect = null;
            Combat.Phantoon.MusicRequest = null;
        }
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (Combat.Phantoon?.LastMaterializationSound is { } phantoonMaterialization)
            audio.QueueEnemySound(phantoonMaterialization, maximumQueued: 6);
        if (Combat.Phantoon?.LastCombatSoundEffect is { } phantoonCombat)
            audio.QueueEnemySound(phantoonCombat, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(Combat.Phantoon?.MusicRequest, MusicCommandDelay.EightFrames);
    }

    /// <summary>Ports PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPhantoonMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>Binds the native hurt callback used after a vulnerable-eye hit.</summary>
    /// <remarks>$A7:DD3F owns every Phantoon hurt frame. It alternates palette seven between white and the new health
    /// band; body movement/main AI resumes only after common flash timing clears handler bit two.</remarks>
    internal void RegisterPhantoonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody,
            (slot, _) => ApplyPhantoonHurt(slot, RequireCompletePhantoonState(slot)));
}
