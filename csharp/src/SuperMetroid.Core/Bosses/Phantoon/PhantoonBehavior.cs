using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new PhantoonEntity(this, place, common, _bossArtwork);

    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Applies body colours and the background changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Phantoon's entrance: the starting flames and its wavy fade-in.</summary>
    internal PhantoonOpening Opening { get; }

    /// <summary>Phantoon's flames as projectiles, its hitboxes and body touch, and what Samus's weapons do to it.</summary>
    internal PhantoonCombat Combat { get; }

    /// <summary>Phantoon's fades, health colors, wave amplitude, animations and phase timer, shared by every phase of the fight.</summary>
    internal PhantoonPresentation Presentation { get; }

    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    internal PhantoonPatterns Patterns { get; }

    /// <summary>Phantoon's death: the fatal swoop, the fades, explosions and wavy pixelation, and Wrecked Ship's power returning.</summary>
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
        Combat = new PhantoonCombat(_room, dependencies.Earthquake, _audio, dependencies.Collision, dependencies.DeathsAndDrops);
        Opening = new PhantoonOpening(dependencies.Projectiles, _audio, Presentation, dependencies.FrameResults, Combat);
        Patterns = new PhantoonPatterns(_room, dependencies.Projectiles, _audio, dependencies.FrameContext, Presentation, Combat);
        Death = new PhantoonDeath(new PhantoonDeathDependencies { Room = _room, Projectiles = dependencies.Projectiles, Presentation = _presentation, FrameContext = dependencies.FrameContext, DeathsAndDrops = dependencies.DeathsAndDrops, PhantoonPresentation = Presentation, Patterns = Patterns, Audio = _audio, FrameResults = dependencies.FrameResults });
    }

    /// <summary>Sets Phantoon's body up when the room spawns it.</summary>
    internal void InitializePhantoonBody(RoomEnemyPlace body)
    {
        Combat.Phantoon = new PhantoonEnemyState(body);

        // Phantoon shows his body, eye, tentacle and mouth frames on his body (#1353, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Phantoon));

        // His body is black as the fight starts, which his later fades start from (#1512).
        Combat.Phantoon.Colours.Show(PhantoonBodyLook.Black);

        PhantoonEnemyState state = Combat.Phantoon;
        state.PhaseTimer = PhantoonTimerDefinitions.InitialFlameDelayFrames;
        state.StartingFlames.NextFlame = 0;
        state.StartingFlames.Orbiting = false;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        StartPhantoonPart(body, PhantoonPartSegment.InvulnerableBody);
        // The body starts asking for no flame rain; no reader consumes it before a phase sets it (#1472).
        state.FlameRainRequested = false;
        body.SetSpeciesState(state);
        ResetPhantoonMouthControls(Combat.Phantoon);
    }

    /// <summary>Runs the shared part initializer.</summary>
    internal void InitializePhantoonPart(RoomEnemyPlace part)
    {
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        // Each part is its own enemy, so the part's identity selects its animation; every part belongs to the encounter.
        if (part.DefinitionId == EnemyDefinitionId.PhantoonEye)
        {
            state.Eye = part;
            StartPhantoonPart(part, PhantoonPartSegment.EyeClosed);
            // The eye's start ends any wave.
            state.Wave.Length = null;
        }
        else if (part.DefinitionId == EnemyDefinitionId.PhantoonTentacles)
        {
            state.Tentacles = part;
            StartPhantoonPart(part, PhantoonPartSegment.Tentacles);
        }
        else if (part.DefinitionId == EnemyDefinitionId.PhantoonMouth)
        {
            state.Mouth = part;
            StartPhantoonPart(part, PhantoonPartSegment.MouthInitial);
        }
        else
        {
            throw new InvalidDataException($"Phantoon part place {part.Ordinal} holds {part.DefinitionId}, not a Phantoon part.");
        }
        part.SetSpeciesState(state);
        ResetPhantoonMouthControls(state);
    }

    /// <summary>Copies body presentation state into a physical part and starts its chosen animation.</summary>
    private void StartPhantoonPart(RoomEnemyPlace part, PhantoonPartSegment segment)
    {
        PhantoonEnemyState state = Combat.Phantoon ??
            throw new InvalidOperationException("Phantoon body must initialize before its parts.");
        part.Frame = EnemyFrameId.Empty;
        part.AnimationClock.Restart();
        part.Shading = state.Body.Shading;
        state.PartPlayback(part) = PhantoonEnemyState.Starting(segment);
    }

    /// <summary>
    /// Every part's setup, the body's included, asks to hide Phantoon's body and ends any casual-flame burst, which the
    /// fight only starts much later.
    /// </summary>
    private static void ResetPhantoonMouthControls(PhantoonEnemyState state)
    {
        state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.Hide;
        state.CasualFlames.Remaining = null;
    }

    /// <summary>Runs Phantoon's encounter phase for one update.</summary>
    private void RunPhantoonMain(
        RoomEnemyPlace body,
        SamusState samus)
    {
        PhantoonEnemyState state = RequireCompletePhantoonState(body);
        PhantoonPhase phase = state.Phase;

        switch (phase)
        {
            case PhantoonPhase.SpawnStartingFlames:
                Opening.RunPhantoonStartingFlameSpawner(body, state);
                break;
            case PhantoonPhase.WaitBeforeActivatingStartingFlames:
                PhantoonOpening.RunPhantoonStartingFlamePause(state);
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
                PhantoonPatterns.RunPhantoonEyeTracking(body, state, samus);
                break;
            case PhantoonPhase.BecomeSolidAndSwoop:
                PhantoonPatterns.BeginPhantoonVulnerableSwoop(body, state, samus);
                break;
            case PhantoonPhase.Swooping:
                PhantoonPatterns.RunPhantoonSwoop(body, state, samus);
                break;
            case PhantoonPhase.FadeOutWhileSwooping:
                Patterns.RunPhantoonFadeOutWhileSwooping(
                    body,
                    state,
                    samus);
                break;
            case PhantoonPhase.WaitAfterFadeOut:
                PhantoonPatterns.RunPhantoonHiddenWait(state);
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
                PhantoonDeath.RunPhantoonFatalSwoop(body, state, samus);
                break;
            case PhantoonPhase.DyingFadeInOut:
                Death.RunPhantoonDyingFadeCycles(body, state);
                break;
            case PhantoonPhase.DyingExplosions:
                Death.RunPhantoonDyingExplosions(body, state);
                break;
            case PhantoonPhase.BeginFinalWavyDeath:
                Death.BeginPhantoonWavyPixelatedDeath(body, state);
                break;
            case PhantoonPhase.DyingFadeOut:
                Death.RunPhantoonWavyPixelatedDeath(state);
                break;
            case PhantoonPhase.AlmostDead:
                Death.ClearPhantoonDeathGraphics(body, state);
                break;
            case PhantoonPhase.Dead:
                Death.ActivateWreckedShipAfterPhantoon(body, state);
                break;
            case PhantoonPhase.WaitingOnEye:
                break;
            default:
                throw new InvalidDataException(
                    $"Phantoon has no behaviour for phase {state.Phase}.");
        }

        // This synchronization runs only for the body's invocation, then copies the updated point into the three
        // drawing parts. The almost-dead phase skips the part sync entirely on that frame.
        if (phase == PhantoonPhase.AlmostDead)
            return;
        state.Mouth.Position = state.Mouth.Position.WithWholeX(body.Position.X.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Mouth.Position = state.Mouth.Position.WithWholeY(body.Position.Y.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
    }

    /// <summary>Requires the body's encounter state; each part property requires its attached place when read.</summary>
    internal PhantoonEnemyState RequireCompletePhantoonState(RoomEnemyPlace body) => Combat.RequirePhantoonState(body);

    /// <summary>Accumulated damage required to alter Phantoon's vulnerable-eye phase.</summary>
    internal const int PhantoonDamageThreshold = 300;

    /// <summary>Runs Phantoon's hurt reaction, which only flashes his colours.</summary>
    internal void ApplyPhantoonHurt(RoomEnemyPlace body, PhantoonEnemyState state)
    {
        PhantoonRoundState round = state.Round;
        bool showHealthColours = body.FlashTimer == 8 ||
            !PhantoonCadenceDefinitions.HurtFlashShown.IsSetIn(body.FrameCounter) && round.HurtFlashShown;
        if (showHealthColours)
        {
            ShowPhantoonHealthColours(body);
            round.HurtFlashShown = false;
            return;
        }

        if (!PhantoonCadenceDefinitions.HurtFlashShown.IsSetIn(body.FrameCounter) || round.HurtFlashShown)
            return;

        state.Colours.Show(PhantoonBodyLook.White);
        round.HurtFlashShown = true;
    }

    /// <summary>Shows his body in the colours of his health band.</summary>
    internal void ShowPhantoonHealthColours(RoomEnemyPlace body) =>
        (Combat.Phantoon ?? throw new InvalidOperationException("Phantoon's health colours show once Phantoon is loaded."))
            .Colours.Show(PhantoonBodyLook.Health(PhantoonPresentation.HealthBand(body)));

    /// <summary>
    /// Runs a Phantoon part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvancePhantoonPartAnimation(RoomEnemyPlace part)
    {
        if (!part.AnimationClock.Expire())
            return;
        PhantoonEnemyState state = Combat.RequirePhantoonState(part);
        AnimationPlayback<PhantoonPartSegment> playback = state.PartPlayback(part);
        // Writing the playback back after the events lets the animation's own hold overwrite an event's install on
        // this part, as a rest does. Phantoon's part animations count no plays.
        EnemyAnimationPlayback.Play(part, PhantoonAnimations.Segments, ref playback,
            cue =>
            {
                ApplyPhantoonPartCue(part, cue);
                return null;
            });
        state.PartPlayback(part) = playback;
    }

    /// <summary>Carries out one cue of a Phantoon part animation.</summary>
    internal void ApplyPhantoonPartCue(
        RoomEnemyPlace part,
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
                // eye animation this installs, as a rest does; the body's animation survives.
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
        state.Swoop.Requested = false;
        RoomEnemyPlace body = state.Body;
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.EyeHitboxBody);
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.PhaseTimer = PhantoonTimerDefinitions.VulnerableWindow[_room.Random.Next(PhantoonRandomDefinitions.VulnerableWindowChoice)];
        state.Phase = PhantoonPhase.EyeTracksSamus;
        PhantoonPresentation.StartPhantoonAnimation(state, state.Eye, PhantoonPartSegment.EyeballCentered);
    }

    /// <summary>Runs PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunPhantoonMain(place, frame.Samus);
    }
}
