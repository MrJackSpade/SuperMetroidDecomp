using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The Phantoon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PhantoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonEye, EnemyDefinitionId.PhantoonMouth, EnemyDefinitionId.PhantoonTentacles];

    /// <summary>Phantoon's body, eye, tentacles or mouth (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Phantoon definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.PhantoonBody)
            return new PhantoonBodyEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.PhantoonEye)
            return new PhantoonEyeEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.PhantoonTentacles)
            return new PhantoonTentaclesEntity(this, place, common, _bossArtwork);
        if (definition == EnemyDefinitionId.PhantoonMouth)
            return new PhantoonMouthEntity(this, place, common, _bossArtwork);
        throw new InvalidOperationException($"The Phantoon species creates no {place.DefinitionId}.");
    }

    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Applies body colours and the background changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The encounters of the room's bosses, to which his body's setup attaches his (#2194).</summary>
    private readonly RoomBossEncounters _encounters;
    /// <summary>The room's enemy places, which list his eye, tentacles and mouth right after his body (#2252).</summary>
    private readonly EnemyPlaces _places;
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
        _encounters = dependencies.Encounters;
        _places = dependencies.Places;
        Presentation = new PhantoonPresentation(dependencies.FrameContext);
        Combat = new PhantoonCombat(_room, dependencies.Earthquake, _audio, dependencies.Collision, dependencies.DeathsAndDrops);
        Opening = new PhantoonOpening(dependencies.Projectiles, _audio, Presentation, dependencies.FrameResults, Combat);
        Patterns = new PhantoonPatterns(_room, dependencies.Projectiles, _audio, dependencies.FrameContext, Presentation, Combat);
        Death = new PhantoonDeath(new PhantoonDeathDependencies { Room = _room, Projectiles = dependencies.Projectiles, Presentation = _presentation, FrameContext = dependencies.FrameContext, DeathsAndDrops = dependencies.DeathsAndDrops, PhantoonPresentation = Presentation, Patterns = Patterns, Audio = _audio, FrameResults = dependencies.FrameResults });
    }

    /// <summary>
    /// Sets Phantoon's body up when the room spawns it: it begins the encounter with his eye, tentacles and mouth, which
    /// his room lists right after his body (#2252), and holds its part of it.
    /// </summary>
    internal void InitializePhantoonBody(RoomEnemyPlace body)
    {
        RoomEnemyPlace[] parts = _places.ListedRunAfter(body, count: 3);
        var state = new PhantoonEnemyState(body, eye: parts[0], tentacles: parts[1], mouth: parts[2]);
        // The room steps his body wave and visibility and its display latches them from the room from here (#2194).
        _encounters.Attach(state);

        // Phantoon shows his body, eye, tentacle and mouth frames on his body (#1353, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Phantoon, state));
        _presentation.BossShadings.Show(BossShadingLayer.Phantoon, state.Colours);

        // His body is black as the fight starts, which his later fades start from (#1512).
        state.Colours.Show(PhantoonBodyLook.Black);

        state.PhaseTimer = PhantoonTimerDefinitions.InitialFlameDelayFrames;
        state.StartingFlames.NextFlame = 0;
        state.StartingFlames.Orbiting = false;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        StartPhantoonPart(state.BodyPart, PhantoonPartSegment.InvulnerableBody);
        // The body starts asking for no flame rain; no reader consumes it before a phase sets it (#1472).
        state.FlameRainRequested = false;
        body.SetSpeciesState(state.BodyPart);
        ResetPhantoonMouthControls(state);
    }

    /// <summary>
    /// The encounter a part joins as it sets up, which his body, listed <paramref name="placesAfterBody"/> places before
    /// the part, began (#2252).
    /// </summary>
    private PhantoonEnemyState JoiningEncounter(RoomEnemyPlace part, int placesAfterBody) =>
        _places.ListedBefore(part, placesAfterBody).RequireSpeciesState<PhantoonPart>().Phantoon;

    /// <summary>Sets Phantoon's eye up, listed right after his body, closed; its start ends any wave.</summary>
    internal void InitializePhantoonEye(RoomEnemyPlace eye)
    {
        PhantoonEnemyState state = JoiningEncounter(eye, placesAfterBody: 1);
        StartPhantoonPart(state.EyePart, PhantoonPartSegment.EyeClosed);
        state.Wave.Length = null;
        JoinPhantoon(state.EyePart);
    }

    /// <summary>Sets Phantoon's tentacles up, listed after his eye.</summary>
    internal void InitializePhantoonTentacles(RoomEnemyPlace tentacles)
    {
        PhantoonEnemyState state = JoiningEncounter(tentacles, placesAfterBody: 2);
        StartPhantoonPart(state.TentaclesPart, PhantoonPartSegment.Tentacles);
        JoinPhantoon(state.TentaclesPart);
    }

    /// <summary>Sets Phantoon's mouth up, listed after his tentacles.</summary>
    internal void InitializePhantoonMouth(RoomEnemyPlace mouth)
    {
        PhantoonEnemyState state = JoiningEncounter(mouth, placesAfterBody: 3);
        StartPhantoonPart(state.MouthPart, PhantoonPartSegment.MouthInitial);
        JoinPhantoon(state.MouthPart);
    }

    /// <summary>Ends a drawing part's setup: the part holds its part of the encounter, and its setup resets the mouth's controls.</summary>
    private static void JoinPhantoon(PhantoonPart part)
    {
        part.Place.SetSpeciesState(part);
        ResetPhantoonMouthControls(part.Phantoon);
    }

    /// <summary>Gives a drawing part the body's colours and starts its chosen animation.</summary>
    private static void StartPhantoonPart(PhantoonPart part, PhantoonPartSegment segment)
    {
        RoomEnemyPlace place = part.Place;
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();
        place.Shading = part.Phantoon.Body.Shading;
        part.Playback = PhantoonPart.Starting(segment);
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
        PhantoonEnemyState state,
        SamusState samus)
    {
        RoomEnemyPlace body = state.Body;
        PhantoonPhase phase = state.Phase;

        switch (phase)
        {
            case PhantoonPhase.SpawnStartingFlames:
                Opening.RunPhantoonStartingFlameSpawner(state);
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
                PhantoonPatterns.BeginPhantoonFlameRainVulnerableWindow(state);
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
                Patterns.RunPhantoonRage(state);
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
                Death.BeginPhantoonWavyPixelatedDeath(state);
                break;
            case PhantoonPhase.DyingFadeOut:
                Death.RunPhantoonWavyPixelatedDeath(state);
                break;
            case PhantoonPhase.AlmostDead:
                Death.ClearPhantoonDeathGraphics(state);
                break;
            case PhantoonPhase.Dead:
                Death.ActivateWreckedShipAfterPhantoon(body, state, samus);
                break;
            case PhantoonPhase.WaitingOnEye:
                break;
            default:
                throw new InvalidDataException(
                    $"Phantoon has no behaviour for phase {state.Phase}.");
        }

        // The three drawing parts follow the body to its new place, except on the update the almost-dead phase runs.
        if (phase == PhantoonPhase.AlmostDead)
            return;
        state.Mouth.Position = state.Mouth.Position.WithWholeX(body.Position.X.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Mouth.Position = state.Mouth.Position.WithWholeY(body.Position.Y.Whole);
        state.Tentacles.Position = state.Tentacles.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
    }


    /// <summary>Accumulated damage required to alter Phantoon's vulnerable-eye phase.</summary>
    internal const int PhantoonDamageThreshold = 300;

    /// <summary>Runs Phantoon's hurt reaction, which only flashes his colours.</summary>
    internal static void ApplyPhantoonHurt(RoomEnemyPlace body, PhantoonEnemyState state)
    {
        PhantoonRoundState round = state.Round;
        // His health colours return on his last hurt update, as the enemy pass is about to clear his hurt state.
        bool showHealthColours = body.FlashTimer == EnemyCollisionDefinitions.HurtStateMinimumFlashUpdates ||
            !PhantoonCadenceDefinitions.HurtFlashShown.IsSetIn(body.FrameCounter) && round.HurtFlashShown;
        if (showHealthColours)
        {
            ShowPhantoonHealthColours(state);
            round.HurtFlashShown = false;
            return;
        }

        if (!PhantoonCadenceDefinitions.HurtFlashShown.IsSetIn(body.FrameCounter) || round.HurtFlashShown)
            return;

        state.Colours.Show(PhantoonBodyLook.White);
        round.HurtFlashShown = true;
    }

    /// <summary>Shows his body in the colours of his health band.</summary>
    internal static void ShowPhantoonHealthColours(PhantoonEnemyState state) =>
        state.Colours.Show(PhantoonBodyLook.Health(PhantoonPresentation.HealthBand(state.Body)));

    /// <summary>
    /// Runs a Phantoon part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and a newly shown frame raises the new-frame flag.
    /// </summary>
    internal void AdvancePhantoonPartAnimation(PhantoonPart part)
    {
        RoomEnemyPlace place = part.Place;
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<PhantoonPartSegment> playback = part.Playback;
        // Writing the playback back after the events lets the animation's own hold overwrite an event's install on
        // this part, as a rest does. Phantoon's part animations count no plays.
        EnemyAnimationPlayback.Play(place, PhantoonAnimations.Segments, ref playback,
            cue =>
            {
                ApplyPhantoonPartCue(part.Phantoon, cue);
                return null;
            });
        part.Playback = playback;
    }

    /// <summary>Carries out one cue of a Phantoon part animation.</summary>
    internal void ApplyPhantoonPartCue(
        PhantoonEnemyState state,
        PhantoonPartCue cue)
    {
        switch (cue)
        {
            case PhantoonPartCue.PlayMaterializationSound:
                _audio.RequestPhantoonMaterializationSound(state.NextMaterializationSound);
                state.NextMaterializationSound = PhantoonSounds.MaterializationSoundAfter(state.NextMaterializationSound);
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
        state.BodyPart.StartAnimation(PhantoonPartSegment.EyeHitboxBody);
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.PhaseTimer = PhantoonTimerDefinitions.VulnerableWindow[_room.Random.Next(PhantoonRandomDefinitions.VulnerableWindowChoice)];
        state.Phase = PhantoonPhase.EyeTracksSamus;
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeballCentered);
    }

    /// <summary>Runs PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(PhantoonEnemyState state, IEnemyAiFrame frame)
    {
        RunPhantoonMain(state, frame.Samus);
    }
}
