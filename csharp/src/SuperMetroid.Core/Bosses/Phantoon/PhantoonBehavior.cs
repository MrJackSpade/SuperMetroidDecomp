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

/// <summary>The Phantoon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PhantoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PhantoonPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PhantoonEntity(this, slot, common, _bossArtwork);

    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Applies body palettes and the background changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Phantoon's entrance: the starting flames and its wavy fade-in.</summary>
    internal PhantoonOpening Opening { get; }

    /// <summary>Phantoon's flames as projectiles, its hitboxes and body touch, and what Samus's weapons do to it.</summary>
    internal PhantoonCombat Combat { get; }

    /// <summary>Phantoon's fades, health colors, wave amplitude, animations and phase timer, shared by every phase of the fight.</summary>
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
        _presentation = dependencies.Presentation;
        Presentation = new PhantoonPresentation(dependencies.FrameContext);
        Opening = new PhantoonOpening(dependencies.Projectiles, _audio, Presentation, dependencies.FrameResults);
        Combat = new PhantoonCombat(_room, dependencies.Projectiles, dependencies.Earthquake, _audio, dependencies.Collision, dependencies.DeathsAndDrops);
        Patterns = new PhantoonPatterns(_room, dependencies.Projectiles, _audio, dependencies.FrameContext, Presentation);
        Death = new PhantoonDeath(new PhantoonDeathDependencies { Room = _room, Projectiles = dependencies.Projectiles, Presentation = _presentation, FrameContext = dependencies.FrameContext, DeathsAndDrops = dependencies.DeathsAndDrops, PhantoonPresentation = Presentation, Patterns = Patterns, Audio = _audio, FrameResults = dependencies.FrameResults });
    }

    /// <summary>Per-update increase in introductory wave amplitude.</summary>
    internal const int PhantoonIntroAmplitudeDelta = 64;
    /// <summary>Wave amplitude at which the intro advances to the next phase.</summary>
    internal const int PhantoonIntroMaximumAmplitude = 3072;
    /// <summary>Per-update phase increment for the wavy background distortion.</summary>
    internal const int PhantoonWavyPhaseDelta = 8;

    /// <summary>Sets Phantoon's body up when the room spawns it.</summary>
    internal void InitializePhantoonBody(RoomEnemySlot body)
    {
        Combat.Phantoon = new PhantoonEnemyState(body);

        // Phantoon shows his body, eye, tentacle and mouth frames on his body (#1353, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Phantoon));

        // His body is black as the fight starts, which his later fades start from (#1512).
        Combat.Phantoon.Colours.Show(PhantoonBodyLook.Black);

        PhantoonBodyState bodyState = Combat.Phantoon.BodyState;
        bodyState.PhaseTimer = PhantoonTimerDefinitions.InitialFlameDelayFrames;
        bodyState.StartingFlameIndex = 0;
        bodyState.StartingFlamesActive = false;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        StartPhantoonPart(body, PhantoonPartSegment.InvulnerableBody);
        bodyState.StartPart();
        body.SetSpeciesState(bodyState);
        bodyState.Phase = PhantoonPhase.SpawnStartingFlames;
        ResetPhantoonMouthControls(Combat.Phantoon);
    }

    /// <summary>Runs the shared part initializer.</summary>
    internal void InitializePhantoonPart(RoomEnemySlot part)
    {
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        // Each part is its own enemy, so the part's identity selects its animation and its own state, and attaches it.
        if (part.DefinitionId == EnemyDefinitionId.PhantoonEye)
        {
            state.Eye = part;
            StartPhantoonPart(part, PhantoonPartSegment.EyeClosed);
            state.EyeState.StartPart();
            part.SetSpeciesState(state.EyeState);
        }
        else if (part.DefinitionId == EnemyDefinitionId.PhantoonTentacles)
        {
            state.Tentacles = part;
            StartPhantoonPart(part, PhantoonPartSegment.Tentacles);
            part.SetSpeciesState(state.TentaclesState);
        }
        else if (part.DefinitionId == EnemyDefinitionId.PhantoonMouth)
        {
            state.Mouth = part;
            StartPhantoonPart(part, PhantoonPartSegment.MouthInitial);
            part.SetSpeciesState(state.MouthState);
        }
        else
        {
            throw new InvalidDataException($"Phantoon part slot {part.SlotIndex} holds {part.DefinitionId}, not a Phantoon part.");
        }
        ResetPhantoonMouthControls(state);
    }

    /// <summary>Copies body presentation state into a physical part and starts its chosen animation.</summary>
    private void StartPhantoonPart(RoomEnemySlot part, PhantoonPartSegment segment)
    {
        PhantoonEnemyState state = Combat.Phantoon ??
            throw new InvalidOperationException("Phantoon body must initialize before its parts.");
        part.Frame = EnemyFrameId.Empty;
        part.AnimationClock.Restart();
        part.Shading = state.Body.Shading;
        state.PartPlayback(part) = PhantoonEnemyState.Starting(segment);
    }

    /// <summary>
    /// Every part's setup, the body's included, hides Phantoon's body and empties the casual-flame schedule on the
    /// mouth's state, which owns the casual-flame schedule used much later in the fight.
    /// </summary>
    private static void ResetPhantoonMouthControls(PhantoonEnemyState state)
    {
        state.MouthState.BodyLayer = PhantoonBodyLayer.Hidden;
        state.MouthState.CasualFlameCount = null;
    }

    /// <summary>Runs Phantoon's per-frame behaviour.</summary>
    private void RunPhantoonMain(
        RoomEnemySlot body,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(body);
        PhantoonPhase dispatched = state.BodyState.Phase;

        switch (dispatched)
        {
            case PhantoonPhase.SpawnStartingFlames:
                Opening.RunPhantoonStartingFlameSpawner(body, state);
                break;
            case PhantoonPhase.WaitBeforeActivatingStartingFlames:
                PhantoonOpening.RunPhantoonStartingFlamePause(state.BodyState);
                break;
            case PhantoonPhase.WaitForStartingFlamesToDisappear:
                Opening.RunPhantoonStartingFlameOrbitWait(state);
                break;
            case PhantoonPhase.WavyFadeIn:
                Opening.RunPhantoonWavyFadeIn(body, state);
                break;
            case PhantoonPhase.PickFirstRoundPattern:
                Patterns.RunPhantoonPickFirstRoundPattern(state);
                break;
            case PhantoonPhase.MoveInFigureEightThenOpenEye:
                Patterns.RunPhantoonFirstRoundFigureEight(body, state);
                break;
            case PhantoonPhase.EyeTracksSamus:
                PhantoonPatterns.RunPhantoonEyeTracking(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonPhase.BecomeSolidAndSwoop:
                PhantoonPatterns.BeginPhantoonVulnerableSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonPhase.Swooping:
                PhantoonPatterns.RunPhantoonSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonPhase.FadeOutWhileSwooping:
                Patterns.RunPhantoonFadeOutWhileSwooping(
                    body,
                    state,
                    RequirePhantoonSamus(samus));
                break;
            case PhantoonPhase.WaitAfterFadeOut:
                PhantoonPatterns.RunPhantoonHiddenWait(state.BodyState);
                break;
            case PhantoonPhase.PickNextAppearance:
                Patterns.PlacePhantoonForNextFigureEight(body, state);
                break;
            case PhantoonPhase.FadeInBeforeFigureEight:
                Patterns.RunPhantoonFadeInBeforeFigureEight(body, state);
                break;
            case PhantoonPhase.BecomeSolidAfterFlameRain:
                PhantoonPatterns.BeginPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonPhase.FadeInDuringFlameRain:
                Patterns.RunPhantoonFlameRainFadeIn(body, state);
                break;
            case PhantoonPhase.TrackSamusDuringFlameRain:
                PhantoonPatterns.RunPhantoonFlameRainVulnerableWindow(body, state);
                break;
            case PhantoonPhase.FadeOutDuringFlameRain:
                Patterns.RunPhantoonFlameRainFadeOut(state);
                break;
            case PhantoonPhase.SpawnFlameRain:
                Patterns.RunPhantoonHiddenFlameRain(body, state);
                break;
            case PhantoonPhase.FadeOutBeforeFirstFlameRain:
                Patterns.RunPhantoonInitialFlameRain(body, state);
                break;
            case PhantoonPhase.FadeOutBeforeRage:
                Patterns.RunPhantoonFadeOutBeforeRage(state);
                break;
            case PhantoonPhase.MoveToTopCenterForRage:
                PhantoonPatterns.RunPhantoonRageHiddenWait(body, state);
                break;
            case PhantoonPhase.FadeInForRage:
                Patterns.RunPhantoonRageFadeIn(body, state);
                break;
            case PhantoonPhase.Enraged:
                Patterns.RunPhantoonRage(body, state);
                break;
            case PhantoonPhase.FadeOutAfterRage:
                Patterns.RunPhantoonRageFadeOut(state);
                break;
            case PhantoonPhase.FinishFatalSwoop:
                PhantoonDeath.RunPhantoonFatalSwoop(body, state, RequirePhantoonSamus(samus));
                break;
            case PhantoonPhase.DyingFadeInOut:
                Death.RunPhantoonDyingFadeCycles(body, state);
                break;
            case PhantoonPhase.DyingExplosions:
                Death.RunPhantoonDyingExplosions(body, state);
                break;
            case PhantoonPhase.BeginFinalWavyDeath:
                Death.BeginPhantoonWavyMosaicDeath(body, state);
                break;
            case PhantoonPhase.DyingFadeOut:
                Death.RunPhantoonWavyMosaicDeath(state);
                break;
            case PhantoonPhase.AlmostDead:
                Death.ClearPhantoonDeathGraphics(body, state);
                break;
            case PhantoonPhase.Dead:
                Death.ActivateWreckedShipAfterPhantoon(body, state);
                break;
            case PhantoonPhase.NoOperation:
                break;
            default:
                throw new InvalidDataException(
                    $"Phantoon has no behaviour for phase {state.BodyState.Phase}.");
        }

        // This synchronization runs only for the body's invocation, then copies the updated point into the three
        // drawing parts. The almost-dead phase skips the part sync and body-layer scroll entirely on that frame.
        if (dispatched == PhantoonPhase.AlmostDead)
            return;
        state.Mouth.Position = state.Mouth.Position.WithWholeX(body.Position.X.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Mouth.Position = state.Mouth.Position.WithWholeY(body.Position.Y.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
        if (state.EyeState.WaveLength is null)
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

    /// <summary>Runs Phantoon's palette-only hurt AI.</summary>
    internal void ApplyPhantoonHurt(RoomEnemySlot body, PhantoonEnemyState state)
    {
        PhantoonTentaclesState tentacles = state.TentaclesState;
        bool restoreHealthPalette = body.FlashTimer == 8 ||
            !PhantoonCadenceDefinitions.HurtFlashShown.IsSetIn(body.FrameCounter) && tentacles.HurtFlashShown;
        if (restoreHealthPalette)
        {
            CopyPhantoonHealthPalette(body);
            tentacles.HurtFlashShown = false;
            return;
        }

        if (!PhantoonCadenceDefinitions.HurtFlashShown.IsSetIn(body.FrameCounter) || tentacles.HurtFlashShown)
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
    /// Runs a Phantoon part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvancePhantoonPartAnimation(RoomEnemySlot part)
    {
        if (!part.AnimationClock.Expire())
            return;
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        AnimationPlayback<PhantoonPartSegment> playback = state.PartPlayback(part);
        // Writing the playback back after the events lets the animation's own hold overwrite an event's install on
        // this part, as the Sleep does. Phantoon's part animations count no plays.
        EnemyAnimationPlayback.Play(part, PhantoonAnimations.Segments, ref playback, PhantoonPartFrames.ShownFrames.Of,
            cue =>
            {
                ApplyPhantoonPartCue(part, cue);
                return null;
            });
        state.PartPlayback(part) = playback;
    }

    /// <summary>Carries out one cue of a Phantoon part animation.</summary>
    internal void ApplyPhantoonPartCue(
        RoomEnemySlot part,
        PhantoonPartCue cue)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(part);
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
                // The advance resumes its own cursor after the cue, so the animation's hold overwrites the
                // eye animation this installs, as the Sleep does; the body's animation survives.
                return;

            case PhantoonPartCue.PickNewPattern:
                Patterns.PickPhantoonSecondRoundPattern(state);
                return;

            case PhantoonPartCue.SpawnCasualFlame:
                Patterns.SpawnPhantoonCasualFlame(state.Body);
                _audio.QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlamePriority);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(PhantoonPartCue)} {cue}.");
        }
    }

    /// <summary>Starts the eye's Samus-tracking animation for the current attack phase.</summary>
    internal void BeginPhantoonEyeTracking(PhantoonEnemyState state)
    {
        state.TentaclesState.SwoopRequested = false;
        RoomEnemySlot body = state.Body;
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.EyeHitboxBody);
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.BodyState.PhaseTimer = PhantoonTimerDefinitions.VulnerableWindow[_room.Random.Next(PhantoonRandomDefinitions.VulnerableWindowChoice)];
        state.BodyState.Phase = PhantoonPhase.EyeTracksSamus;
        PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeballCentered);
    }

    /// <summary>Initial flame behaviour that holds position before orbit activation.</summary>
    internal const EnemyProjectileFrameBehavior PhantoonStartingFlameWaitingFrameBehavior =
        EnemyProjectileFrameBehavior.PhantoonIntroFlameWaiting;
    /// <summary>Activated flame behaviour that advances its orbit around the body.</summary>
    internal const EnemyProjectileFrameBehavior PhantoonStartingFlameOrbitFrameBehavior =
        EnemyProjectileFrameBehavior.PhantoonIntroFlameOrbit;

    /// <summary>Runs PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPhantoonMain(slot, frame.Samus, frame.Camera);
    }

    /// <summary>Binds the hurt behaviour used after a vulnerable-eye hit.</summary>
    /// <remarks>Phantoon's hurt AI owns every Phantoon hurt frame. It alternates the body's colours between white and the
    /// new health band; body movement/main AI resumes only after common flash timing clears the hurt state.</remarks>
    internal void AddPhantoonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.PhantoonBody,
            (slot, _) => ApplyPhantoonHurt(slot, RequireCompletePhantoonState(slot)));
}
