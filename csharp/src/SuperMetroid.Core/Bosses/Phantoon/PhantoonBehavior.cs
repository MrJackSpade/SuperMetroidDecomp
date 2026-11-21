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
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.PhantoonBody] = (occupant, place, common) => EnemyOccupant.Of(new PhantoonBodyEntity(this, occupant, place, common, _bossArtwork)),
        [EnemyDefinitionId.PhantoonEye] = (occupant, place, common) => EnemyOccupant.Of(new PhantoonEyeEntity(this, occupant, place, common, _bossArtwork)),
        [EnemyDefinitionId.PhantoonMouth] = (occupant, place, common) => EnemyOccupant.Of(new PhantoonMouthEntity(this, occupant, place, common, _bossArtwork)),
        [EnemyDefinitionId.PhantoonTentacles] = (occupant, place, common) => EnemyOccupant.Of(new PhantoonTentaclesEntity(this, occupant, place, common, _bossArtwork)),
    };

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

    /// <summary>Phantoon's fades, health colors, wave amplitude and animations, shared by every phase of the fight.</summary>
    private PhantoonPresentation Presentation { get; }

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
        Opening = new PhantoonOpening(dependencies.Projectiles, _audio, Presentation, dependencies.PassEvents, Combat);
        Patterns = new PhantoonPatterns(_room, dependencies.Projectiles, _audio, dependencies.FrameContext, Presentation, Combat);
        Death = new PhantoonDeath(new PhantoonDeathDependencies { Room = _room, Projectiles = dependencies.Projectiles, Presentation = _presentation, FrameContext = dependencies.FrameContext, DeathsAndDrops = dependencies.DeathsAndDrops, PhantoonPresentation = Presentation, Patterns = Patterns, Audio = _audio, PassEvents = dependencies.PassEvents });
    }

    /// <summary>
    /// Sets Phantoon's body up when the room spawns it: it begins the encounter with his eye, tentacles and mouth, which
    /// his room lists right after his body (#2252), and holds its part of it.
    /// </summary>
    internal void InitializePhantoonBody(RoomEnemyOccupant occupant, RoomEnemyPlace body)
    {
        var state = new PhantoonEnemyState(occupant, body, eye: Partner(body, EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonEye),
            tentacles: Partner(body, EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonTentacles),
            mouth: Partner(body, EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonMouth));
        // The room steps his body wave and visibility and its display latches them from the room from here (#2194).
        _encounters.Attach(state);

        // Phantoon shows his body, eye, tentacle and mouth frames on his body (#1353, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Phantoon, state));
        _presentation.BossShadings.Show(BossShadingLayer.Phantoon, state.Colours);

        // His body is black as the fight starts, which his later fades start from (#1512).
        state.Colours.Show(PhantoonBodyLook.Black);

        state.PhaseWait.LastFor(PhantoonTimerDefinitions.InitialFlameDelayFrames);
        state.StartingFlames.NextFlame = PhantoonStartingFlamesState.First;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        StartPhantoonPart(state.BodyPart);
        body.SetSpeciesState(state.BodyPart);
        // The encounter starts asking to hide his body, with no casual-flame burst, which the fight only starts much later;
        // his parts set up in the same pass, before anything reads either.
        state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.Hide;
        state.CasualFlames.NextFlame = null;
    }

    /// <summary>The member of Phantoon's that plays <paramref name="role"/>, as his room lists them (#2252, #2556).</summary>
    private RoomEnemyPlace Partner(RoomEnemyPlace member, EnemyDefinitionId memberRole, EnemyDefinitionId role) =>
        _places.Member(member, PhantoonPartListingDefinitions.Listing, memberRole, role);

    /// <summary>The encounter a part playing <paramref name="role"/> joins as it sets up, which his body began (#2252).</summary>
    private PhantoonEnemyState JoiningEncounter(RoomEnemyPlace part, EnemyDefinitionId role) =>
        Partner(part, role, EnemyDefinitionId.PhantoonBody).RequireSpeciesState<PhantoonPart>().Phantoon;

    /// <summary>Sets Phantoon's eye up, listed right after his body, closed; its start ends any wave.</summary>
    internal void InitializePhantoonEye(RoomEnemyPlace eye)
    {
        PhantoonEnemyState state = JoiningEncounter(eye, EnemyDefinitionId.PhantoonEye);
        StartPhantoonPart(state.EyePart);
        state.Wave.End();
        JoinPhantoon(state.EyePart);
    }

    /// <summary>Sets Phantoon's tentacles up, listed after his eye.</summary>
    internal void InitializePhantoonTentacles(RoomEnemyPlace tentacles)
    {
        PhantoonEnemyState state = JoiningEncounter(tentacles, EnemyDefinitionId.PhantoonTentacles);
        StartPhantoonPart(state.TentaclesPart);
        JoinPhantoon(state.TentaclesPart);
    }

    /// <summary>Sets Phantoon's mouth up, listed after his tentacles.</summary>
    internal void InitializePhantoonMouth(RoomEnemyPlace mouth)
    {
        PhantoonEnemyState state = JoiningEncounter(mouth, EnemyDefinitionId.PhantoonMouth);
        StartPhantoonPart(state.MouthPart);
        JoinPhantoon(state.MouthPart);
    }

    /// <summary>Ends a drawing part's setup: the part holds its part of the encounter.</summary>
    private static void JoinPhantoon(PhantoonPart part) => part.Place.SetSpeciesState(part);

    /// <summary>
    /// Gives a drawing part the body's colours and runs the animation it was made playing (#2319), its first step on the
    /// next update.
    /// </summary>
    private static void StartPhantoonPart(PhantoonPart part)
    {
        RoomEnemyPlace place = part.Place;
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.Restart();
        place.Shading = part.Phantoon.Body.Shading;
    }

    /// <summary>Runs Phantoon's encounter phase for one update.</summary>
    private void RunPhantoonMain(
        PhantoonEnemyState state,
        SamusState samus)
    {
        RoomEnemyPlace body = state.Body;
        PhantoonPhase phase = state.Phase;

        phase.Run(this, state, samus);

        // The three drawing parts follow the body to its new place, except on the update the almost-dead phase runs.
        if (phase == PhantoonPhase.AlmostDead)
            return;
        foreach (PhantoonPart part in state.CarriedParts)
            part.Place.Position = part.Place.Position.WithWhole(body.Position.Whole);
    }

    /// <summary>Runs Phantoon's hurt reaction, which only flashes his colours.</summary>
    internal static void ApplyPhantoonHurt(RoomEnemyPlace body, PhantoonEnemyState state)
    {
        PhantoonRoundState round = state.Round;
        // His health colours return on his last hurt update, as the enemy pass is about to clear his hurt state.
        bool showHealthColours = body.HurtFlash.UpdatesLeft == EnemyCollisionDefinitions.HurtStateMinimumFlashUpdates ||
            !PhantoonCadenceDefinitions.HurtFlashShown.IsInSecondHalfAt(body.FrameCounter) && round.HurtFlashShown;
        if (showHealthColours)
        {
            ShowPhantoonHealthColours(state);
            round.HurtFlashShown = false;
            return;
        }

        if (!PhantoonCadenceDefinitions.HurtFlashShown.IsInSecondHalfAt(body.FrameCounter) || round.HurtFlashShown)
            return;

        state.Colours.Show(PhantoonBodyLook.White);
        round.HurtFlashShown = true;
    }

    /// <summary>Shows his body in the colours of his health band.</summary>
    private static void ShowPhantoonHealthColours(PhantoonEnemyState state) =>
        state.Colours.Show(PhantoonBodyLook.Health(PhantoonPresentation.HealthBand(state.Body)));

    /// <summary>
    /// Runs a Phantoon part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, and the clock notes each newly shown frame.
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

    /// <summary>What <see cref="ApplyPhantoonPartCue"/> does for each <see cref="PhantoonPartCue"/> member, each member's own arm.</summary>
    private IdentityTable<PhantoonPartCue, Action<PhantoonEnemyState>> ApplyPhantoonPartCueForPhantoonPartCue => field ??= new(new Dictionary<PhantoonPartCue, Action<PhantoonEnemyState>>
    {
        [PhantoonPartCue.PlayMaterializationSound] = state =>
        {
            _audio.RequestPhantoonMaterializationSound(state.NextMaterializationSound.Entry);
            state.NextMaterializationSound = state.NextMaterializationSound.NextInCycle;
        },
        [PhantoonPartCue.BeginEyeTracking] = state =>
        {
            BeginPhantoonEyeTracking(state);
            // The eye shows its tracking animation from the next update; the body keeps its own animation.
        },
        [PhantoonPartCue.PickNewPattern] = state => Patterns.PickPhantoonSecondRoundPattern(state),
        [PhantoonPartCue.SpawnCasualFlame] = state =>
        {
            Patterns.SpawnPhantoonCasualFlame(state.BodyOccupant, state.Body);
            _audio.QueueEnemySound(PhantoonSoundDefinitions.CasualFlame, PhantoonSoundDefinitions.CasualFlamePriority);
        },
    });

    /// <summary>Carries out one cue of a Phantoon part animation.</summary>
    private void ApplyPhantoonPartCue(
        PhantoonEnemyState state,
        PhantoonPartCue cue)
    {
        ApplyPhantoonPartCueForPhantoonPartCue[cue](state);
    }

    /// <summary>Starts the eye's Samus-tracking animation for the current attack phase.</summary>
    private void BeginPhantoonEyeTracking(PhantoonEnemyState state)
    {
        state.WindowEnd = PhantoonWindowEnd.EyeCloses;
        RoomEnemyPlace body = state.Body;
        state.BodyPart.StartAnimation(PhantoonPartSegment.EyeHitboxBody);
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.PhaseWait.LastFor(PhantoonRandomDefinitions.VulnerableWindow.Choose(_room.Random.Generate()));
        state.Phase = PhantoonPhase.EyeTracksSamus;
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeballCentered);
    }

    /// <summary>Runs PhantoonBody's main AI.</summary>
    internal void RunPhantoonBodyMainAi(PhantoonEnemyState state, IEnemyAiFrame frame)
    {
        RunPhantoonMain(state, frame.Samus);
    }
}
