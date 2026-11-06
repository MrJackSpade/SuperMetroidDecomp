using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PhantoonEntity(this, slot, common, _bossArtwork);

    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the body, eye, mouth, and tentacle slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies body palettes and the background tilemap changes.</summary>
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
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Binds room and enemy services for the four-part boss actor.</summary>
    internal PhantoonBehavior(PhantoonBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        Presentation = new PhantoonPresentation(dependencies.FrameContext);
        Opening = new PhantoonOpening(dependencies.Projectiles, _audio, Presentation);
        Combat = new PhantoonCombat(_room, dependencies.Projectiles, dependencies.Earthquake, _audio, _slotTable, dependencies.Collision, dependencies.DeathsAndDrops);
        Patterns = new PhantoonPatterns(_room, dependencies.Projectiles, _audio, dependencies.FrameContext, Presentation);
        Death = new PhantoonDeath(new PhantoonDeathDependencies { Room = _room, Projectiles = dependencies.Projectiles, Presentation = _presentation, FrameContext = dependencies.FrameContext, DeathsAndDrops = dependencies.DeathsAndDrops, PhantoonPresentation = Presentation, Patterns = Patterns, Audio = _audio });
    }

    /// <summary>Per-update increase in introductory wave amplitude.</summary>
    internal const int PhantoonIntroAmplitudeDelta = 64;
    /// <summary>Wave amplitude at which the intro advances to the next phase.</summary>
    internal const int PhantoonIntroMaximumAmplitude = 3072;
    /// <summary>Per-update phase increment for the wavy background distortion.</summary>
    internal const int PhantoonWavyPhaseDelta = 8;

    /// <summary>Ports <c>InitAI_PhantoonBody</c>.</summary>
    internal void InitializePhantoonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Phantoon body requires the first enemy slot, not slot {body.SlotIndex}.");
        }

        Combat.Phantoon = new PhantoonEnemyState(body);

        // Phantoon shows his body, eye, tentacle and mouth frames on his body (#1353, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts());

        // His body is black as the fight starts, which his later fades start from (#1512).
        Combat.Phantoon.Colours.Show(PhantoonBodyLook.Black);

        Combat.Part(body).FunctionTimer = PhantoonTimerDefinitions.InitialFlameDelayFrames;
        Combat.Part(body).StartingFlameIndex = 0;
        Combat.Part(body).StartingFlamesActive = false;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        InitializePhantoonPart(body, PhantoonPartProgram.InvulnerableBody);
    }

    /// <summary>Ports the shared part initializer.</summary>
    internal void InitializePhantoonPart(RoomEnemySlot part)
    {
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        // Each part is its own enemy, so the part's identity selects its program and its place in the state.
        if (part.DefinitionId == EnemyDefinitionId.PhantoonEye)
        {
            InitializePhantoonPart(part, PhantoonPartProgram.EyeClosed);
            state.Eye = part;
        }
        else if (part.DefinitionId == EnemyDefinitionId.PhantoonTentacles)
        {
            InitializePhantoonPart(part, PhantoonPartProgram.Tentacles);
            state.Tentacles = part;
        }
        else if (part.DefinitionId == EnemyDefinitionId.PhantoonMouth)
        {
            InitializePhantoonPart(part, PhantoonPartProgram.MouthInitial);
            state.Mouth = part;
        }
        else
        {
            throw new InvalidDataException($"Phantoon part slot {part.SlotIndex} holds {part.DefinitionId}, not a Phantoon part.");
        }
    }

    /// <summary>Copies body presentation state into a physical part and starts its chosen program.</summary>
    private void InitializePhantoonPart(RoomEnemySlot part, PhantoonPartProgram program)
    {
        PhantoonEnemyState state = Combat.Phantoon ??
            throw new InvalidOperationException("Phantoon body must initialize before its parts.");
        part.Frame = EnemyFrameId.Empty;
        part.FrameHold = WrappingCountdown.Start(1);
        part.Timer = WrappingCountdown.Start(0);
        part.Shading = state.Body.Shading;
        state.PartProgram(part) = AnimationProgramCursor<PhantoonPartProgram>.Start(program);
        PhantoonPartState partState = state.Parts[part.SlotIndex];
        partState.StartPart();
        part.SetSpeciesState(partState, partState.ClearWithRecord);
        state.Parts[part.SlotIndex].Function = PhantoonAiFunction.SpawnStartingFlames;

        // Every part setup writes the mouth's layer control and casual-flame count, including the body's. Slot three
        // owns the casual-flame schedule used much later in the fight.
        if (_slotTable.Slots.Count > 3)
        {
            state.Parts[3].LayerControl = PhantoonLayerControl.Hidden;
            state.Parts[3].CasualFlameCount = PhantoonCasualFlameDefinitions.NoActiveSchedule;
        }
    }

    /// <summary>Ports <c>MainAI_Phantoon</c>.</summary>
    private void RunPhantoonMain(
        RoomEnemySlot body,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(body);
        _audio.CancelPhantoonMaterializationSound();
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
                Opening.RunPhantoonStartingFlameOrbitWait(body, state);
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
                Death.BeginPhantoonWavyMosaicDeath(body, state);
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
        // The synchronization tests the X the function returned with: the wave setup leaves it at the background
        // write-stack index, so that frame skips the part sync and body-layer scroll entirely.
        if (dispatched == PhantoonAiFunction.AlmostDead)
            return;
        state.Mouth.Position = state.Mouth.Position.WithWholeX(body.Position.X.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Mouth.Position = state.Mouth.Position.WithWholeY(body.Position.Y.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
        if (state.Parts[state.Eye.SlotIndex].WaveLength is null)
        {
            state.BodyLayerPlacement = new(
                CoordinateAxis.Wrap(camera.X - body.Position.X.Whole + 40),
                CoordinateAxis.Wrap(camera.Y - body.Position.Y.Whole + 40));
        }
    }

    /// <summary>Requires the body's encounter state; each part property requires its attached slot when read.</summary>
    internal PhantoonEnemyState RequireCompletePhantoonState(RoomEnemySlot body) => Combat.RequirePhantoonState(body);

    /// <summary>Requires a live Samus state for Phantoon movement and eye targeting.</summary>
    private static SamusState RequirePhantoonSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Phantoon's eye and swoop programs require the active Samus actor.");

    /// <summary>Accumulated damage required to alter Phantoon's vulnerable-eye phase.</summary>
    internal const int PhantoonDamageThreshold = 300;

    /// <summary>Ports Phantoon's palette-only hurt AI.</summary>
    internal void ApplyPhantoonHurt(RoomEnemySlot body, PhantoonEnemyState state)
    {
        PhantoonPartState tentacles = state.Parts[state.Tentacles.SlotIndex];
        bool restoreHealthPalette = body.FlashTimer == 8 ||
            (body.FrameCounter & 2) == 0 && tentacles.HurtFlashShown;
        if (restoreHealthPalette)
        {
            CopyPhantoonHealthPalette(body);
            tentacles.HurtFlashShown = false;
            return;
        }

        if ((body.FrameCounter & 2) == 0 || tentacles.HurtFlashShown)
            return;

        state.Colours.Show(PhantoonBodyLook.White);
        tentacles.HurtFlashShown = true;
    }

    /// <summary>Copies the selected health colors into the visible body palette.</summary>
    internal void CopyPhantoonHealthPalette(RoomEnemySlot body) =>
        (Combat.Phantoon ?? throw new InvalidOperationException("Phantoon's health colours show once Phantoon is loaded."))
            .Colours.Show(PhantoonBodyLook.Health(PhantoonPresentation.HealthBand(body)));
    /// <summary>Per-update wave increment during the wavy mosaic death animation.</summary>
    internal const int PhantoonDeathWaveDelta = 256;
    /// <summary>Wave ceiling reached before the death animation advances.</summary>
    internal const int PhantoonDeathWaveMaximum = 61440;

    /// <summary>
    /// Runs a Phantoon part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvancePhantoonPartAnimation(RoomEnemySlot part)
    {
        if (!EnemyProgramAnimation.Expire(part))
            return;
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        AnimationProgramCursor<PhantoonPartProgram> cursor = state.PartProgram(part);
        // Writing the cursor back after the cues lets the program's own sleep overwrite a cue's install on this
        // part, as the Sleep does. Phantoon's part lists load no loop counter.
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
                _audio.RequestPhantoonMaterializationSound(
                    PhantoonSounds.MaterializationSound(
                        state.MaterializationSoundIndex));
                state.MaterializationSoundIndex++;
                if (state.MaterializationSoundIndex >= PhantoonSoundDefinitions.MaterializationSoundCount)
                    state.MaterializationSoundIndex = 0;
                return;

            case PhantoonPartCue.BeginEyeTracking:
                BeginPhantoonEyeTracking(state);
                // The advance resumes its own cursor after the cue, so the program's hold overwrites the
                // eye program this installs, as the Sleep does; the body's program survives.
                return;

            case PhantoonPartCue.PickNewPattern:
                Patterns.PickPhantoonSecondRoundPattern(state);
                return;

            case PhantoonPartCue.SpawnCasualFlame:
                Patterns.SpawnPhantoonDestroyableFlame(state.Body, PhantoonFlameSpawnType.Casual, 0);
                _audio.QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlamePriority);
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
        RequireCompletePhantoonState(stateSlot.DefinitionId == EnemyDefinitionId.PhantoonBody
            ? stateSlot
            : _slotTable.Slots[0]);
    /// <summary>Initial flame callback that holds position before orbit activation.</summary>
    internal const EnemyProjectileFrameBehavior PhantoonStartingFlameWaitingFrameBehavior =
        EnemyProjectileFrameBehavior.PhantoonIntroFlameWaiting;
    /// <summary>Activated flame callback that advances its orbit around the body.</summary>
    internal const EnemyProjectileFrameBehavior PhantoonStartingFlameOrbitFrameBehavior =
        EnemyProjectileFrameBehavior.PhantoonIntroFlameOrbit;

    /// <summary>Ports PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPhantoonMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>Binds the hurt callback used after a vulnerable-eye hit.</summary>
    /// <remarks>Phantoon's hurt AI owns every Phantoon hurt frame. It alternates palette seven between white and the new health
    /// band; body movement/main AI resumes only after common flash timing clears handler bit two.</remarks>
    internal void AddPhantoonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.PhantoonBody,
            (slot, _) => ApplyPhantoonHurt(slot, RequireCompletePhantoonState(slot)));
}
