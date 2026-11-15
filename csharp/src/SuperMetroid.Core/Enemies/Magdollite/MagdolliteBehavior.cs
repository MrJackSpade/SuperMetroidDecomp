using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The Magdollite species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MagdolliteBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Magdollite];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new MagdolliteEntity(this, place, common);

    /// <summary>Supplies the room's apex overreads.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the lava shots emitted by the overlay's animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each Magdollite part finds the others when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The partners each Magdollite part found when it was placed.</summary>
    private readonly EnemyPartLinks<MagdollitePartners> _partners = new();
    /// <summary>Runs common contact damage before the species reaction.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Creates the item drop when a thrown lava projectile is destroyed.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the Magdollite AI, projectile, combat and glow services.</summary>
    internal MagdolliteBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
        _places = places;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>The updates each frame of the room's lava glow holds for.</summary>
    private const int GlowFrameUpdates = 8;

    /// <summary>Updates left before the room's lava glow steps to its next frame.</summary>
    internal int GlowStepCountdown { get; private set; }
    /// <summary>
    /// The frame of the room's lava glow every Magdollite shows, 0 through <see cref="MagdolliteShades.GlowFrames"/>
    /// minus one; each step advances it around the cycle.
    /// </summary>
    internal int GlowFrame { get; private set; }
    /// <summary>Whether a Magdollite has started the room's lava glow; until one has, the glow does not step.</summary>
    private bool _glowStarted;

    /// <summary>The lava glow every Magdollite in the room, and the lava they throw, shows now (#1512).</summary>
    internal PictureShading Glow => PictureShading.Shaded(MagdolliteShades.Glow(GlowFrame));

    /// <summary>Initializes the Magdollite.</summary>
    internal void InitializeMagdollite(RoomEnemyPlace place, SamusState samus)
    {
        MagdollitePart part = place.SpawnSettings<MagdolliteSpawnSettings>().Part;
        // The room lists each Magdollite's head, body and overlay in a row; each part finds the others, the enemy
        // listed after it and the two its follow-up to a hit reaches.
        RoomEnemyPlace head = part == MagdollitePart.Head ? place : _places.ListedBefore(place, PlacesAfterHead(part));
        _partners.Place(place, new MagdollitePartners(
            Composite: new(head, _places.ListedAfter(head, 1), _places.ListedAfter(head, 2)),
            Next: _places.ListedAfterOrNone(place, 1),
            FollowUpRecipients: FollowUpAfterStruckPart.Recipients(distance => _places.ListedAfterOrNone(place, distance), reach: 2)));
        var state = new MagdolliteEnemyState(part);
        place.SetSpeciesState(state);

        switch (state.Part)
        {
            case MagdollitePart.Head:
                InitializeMagdolliteHead(place, state, samus);
                break;
            case MagdollitePart.RisingBody:
                InitializeMagdolliteBody(place, state);
                break;
            case MagdollitePart.TrackingOverlay:
                InitializeMagdolliteOverlay(place, state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdollitePart)} {state.Part}.");
        }

        // The head's spawn settings name the speed it and its arm rise and sink at; the arm reads the head's.
        if (state.Part == MagdollitePart.Head)
            state.Speed = HeadRise(place).Speed;

        // The room has one lava glow, not one per part: each part's initialization restarts it, so the last part placed
        // sets its phase.
        RestartGlow();
    }

    /// <summary>How the Magdollite head in <paramref name="head"/> rises, from its spawn settings.</summary>
    /// <exception cref="InvalidDataException">The head's placement names no rise.</exception>
    private static MagdolliteHeadRise HeadRise(RoomEnemyPlace head) =>
        head.SpawnSettings<MagdolliteSpawnSettings>().Rise
            ?? throw new InvalidDataException($"The Magdollite head in enemy place {head.Ordinal} names no rise.");

    /// <summary>
    /// Restarts the room's lava glow at its first frame, which every Magdollite and its lava show at once, holding it
    /// for <see cref="GlowFrameUpdates"/> updates.
    /// </summary>
    internal void RestartGlow()
    {
        _glowStarted = true;
        GlowStepCountdown = GlowFrameUpdates;
        GlowFrame = 0;
        ShowGlow();
    }

    /// <summary>Starts the head in its Samus-facing idle animation, retaining the initializer's reversed marker convention until the first tick.</summary>
    private static void InitializeMagdolliteHead(
        RoomEnemyPlace place,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        state.InstalledAnimation = null;
        state.HeadWaitsForBodyLanding = false;
        state.Origin = place.Position.Whole;

        bool samusIsLeft = samus is not null && (samus.Position.Whole - place.Position.Whole).X < 0;
        state.DesiredAnimation = samusIsLeft
            ? MagdolliteSegment.LeftIdle
            : MagdolliteSegment.RightIdle;
        StartMagdolliteAnimation(place, state);
        state.Action = MagdolliteAction.HeadWaiting;
    }

    /// <summary>Hides the rising body 32 pixels below its origin until the head triggers an attack.</summary>
    private static void InitializeMagdolliteBody(
        RoomEnemyPlace place,
        MagdolliteEnemyState state)
    {
        state.Origin = place.Position.Whole;
        state.InstalledAnimation = null;
        state.BodyReachedApex = false;
        state.BodyWakeRequested = true;
        state.DesiredAnimation = MagdolliteRiseStage.Setup.Phase.BodyAnimation;
        StartMagdolliteAnimation(place, state);
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        state.Action = MagdolliteAction.BodyDormant;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Hides the projectile-throwing overlay below its origin and starts its pillar-cap animation.</summary>
    private static void InitializeMagdolliteOverlay(
        RoomEnemyPlace place,
        MagdolliteEnemyState state)
    {
        state.Origin = place.Position.Whole;
        state.InstalledAnimation = null;
        state.AttackTimer = WrappingCountdown.Start(0);
        state.DesiredAnimation = MagdolliteSegment.PillarCap;
        StartMagdolliteAnimation(place, state);
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        state.Action = MagdolliteAction.OverlayWaitingForAttack;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs the Magdollite's main AI and its actions.</summary>
    private void RunMagdolliteMain(
        RoomEnemyPlace place,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        state.AttackTimer = state.AttackTimer.Decrement().Remaining;
        switch (state.Action)
        {
            case MagdolliteAction.HeadWaiting:
                RunMagdolliteHeadWaiting(place, state, samus);
                return;
            case MagdolliteAction.HeadWaitingForAttackAnimation:
                RunMagdolliteHeadWaitingForAttackAnimation(place, state);
                return;
            case MagdolliteAction.HeadWaitingForBodyLanding:
                RunMagdolliteHeadWaitingForBodyLanding(place, state, samus);
                return;
            case MagdolliteAction.HeadWaitingForReturnAnimation:
                RunMagdolliteHeadWaitingForReturnAnimation(place, state, samus);
                return;
            case MagdolliteAction.BodyDormant:
                RunMagdolliteBodyDormant(place, state);
                return;
            case MagdolliteAction.BodyRising:
                RunMagdolliteBodyRising(place, state, samus);
                return;
            case MagdolliteAction.HoldingAtApex:
                return;
            case MagdolliteAction.BodyFalling:
                RunMagdolliteBodyFalling(place, state);
                return;
            case MagdolliteAction.OverlayWaitingForAttack:
                RunMagdolliteOverlayWaiting(place, state);
                return;
            case MagdolliteAction.OverlayWaitingForThrowAnimation:
                RunMagdolliteOverlayAnimation(place, state);
                return;
            case MagdolliteAction.OverlayTrackingRisingBody:
                RunMagdolliteOverlayTrackingRise(place, state, samus);
                return;
            case MagdolliteAction.OverlayTrackingFallingBody:
                RunMagdolliteOverlayTrackingFall(place, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Tracks Samus while idle and begins submerging when the overlay cooldown expires within the head's horizontal range.</summary>
    private void RunMagdolliteHeadWaiting(
        RoomEnemyPlace head,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        bool samusIsRight = !CoordinateAxis.Precedes(samus.Position.X.Whole, head.Position.X.Whole);
        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightIdle
            : MagdolliteSegment.LeftIdle;
        StartMagdolliteAnimation(head, state);

        (_, _, RoomEnemyPlace overlay) = RequireMagdolliteComposite(head);
        MagdolliteEnemyState overlayState = overlay.RequireSpeciesState<MagdolliteEnemyState>();
        if (!overlayState.AttackTimer.IsNegative)
            return;

        overlayState.AttackTimer = WrappingCountdown.Start(0);
        int range = HeadRise(head).Range;
        if (!EnemyMath.IsWithinStrictModularDistance(head.Position.X.Whole, samus.Position.X.Whole, range))
            return;

        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightSubmerge
            : MagdolliteSegment.LeftSubmerge;
        StartMagdolliteAnimation(head, state);
        state.Action = MagdolliteAction.HeadWaitingForAttackAnimation;
    }

    /// <summary>Wakes the adjacent body only after the head's submerge animation has cleared its busy flag.</summary>
    private void RunMagdolliteHeadWaitingForAttackAnimation(
        RoomEnemyPlace head,
        MagdolliteEnemyState state)
    {
        if (state.AnimationBusy)
            return;

        (_, RoomEnemyPlace body, _) = RequireMagdolliteComposite(head);
        state.HeadWaitsForBodyLanding = true;
        body.RequireSpeciesState<MagdolliteEnemyState>().BodyWakeRequested = false;
        state.Action = MagdolliteAction.HeadWaitingForBodyLanding;
    }

    /// <summary>Starts the Samus-facing emerge animation once the falling body releases the head's landing latch.</summary>
    private static void RunMagdolliteHeadWaitingForBodyLanding(
        RoomEnemyPlace head,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        if (state.HeadWaitsForBodyLanding)
            return;

        bool samusIsRight = samus is not null &&
            !CoordinateAxis.Precedes(samus.Position.X.Whole, head.Position.X.Whole);
        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightEmerge
            : MagdolliteSegment.LeftEmerge;
        StartMagdolliteAnimation(head, state);
        state.Action = MagdolliteAction.HeadWaitingForReturnAnimation;
    }

    /// <summary>Restores the appropriate idle animation after the emerge animation finishes.</summary>
    private static void RunMagdolliteHeadWaitingForReturnAnimation(
        RoomEnemyPlace head,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        if (state.AnimationBusy)
            return;

        bool samusIsRight = samus is not null &&
            !CoordinateAxis.Precedes(samus.Position.X.Whole, head.Position.X.Whole);
        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightIdle
            : MagdolliteSegment.LeftIdle;
        StartMagdolliteAnimation(head, state);
        state.Action = MagdolliteAction.HeadWaiting;
    }

    /// <summary>Returns the hidden body to its origin and enters rising once the head clears its wake latch.</summary>
    private static void RunMagdolliteBodyDormant(
        RoomEnemyPlace body,
        MagdolliteEnemyState state)
    {
        if (state.BodyWakeRequested)
            return;

        state.Action = MagdolliteAction.BodyRising;
        state.RiseDisplacement = Offset16<RoomPixels>.Zero;
        state.BodyWakeRequested = false;
        state.BodyPhase = MagdolliteRiseStage.FirstPillar;
        body.Position = body.Position.WithWholeY(state.Origin.Y);
    }

    /// <summary>Rises using the preceding head's velocity, advances growth phases at their thresholds, and stops at the apex or above Samus.</summary>
    internal void RunMagdolliteBodyRising(
        RoomEnemyPlace body,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        // The arm rises at its head's speed.
        (RoomEnemyPlace head, _, _) = RequireMagdolliteComposite(body);
        MagdolliteEnemyState speedOwner = head.RequireSpeciesState<MagdolliteEnemyState>();
        state.RiseDisplacement += AddMagdolliteY(body, -speedOwner.Speed);

        int upwardDistance = -state.RiseDisplacement.Y;
        MagdollitePhaseDefinition phase = state.BodyPhase.Phase;
        bool roseMaximum = !CoordinateAxis.Precedes(upwardDistance, MagdollitePhaseDefinitions.MaximumRise);
        bool rosePastSamus = samus is not null &&
            CoordinateAxis.Precedes(body.Position.Y.Whole - phase.OverlayYOffset, samus.Position.Y.Whole);
        // At full height whether the arm grows its last segment depends on the place it occupies
        // (MagdolliteApexGrowth); the Samus test compares with the phase's own threshold.
        bool growsAPhase = roseMaximum
            ? _room.Session.Overreads.MagdolliteApexGrowth.GrowsFinalSegment(body.Ordinal)
            : !CoordinateAxis.Precedes(upwardDistance, phase.DistanceThreshold);

        if (roseMaximum || rosePastSamus)
        {
            state.Action = MagdolliteAction.HoldingAtApex;
            state.BodyReachedApex = true;
            if (!growsAPhase)
                return;
        }
        else if (!growsAPhase)
        {
            return;
        }

        state.BodyPhase = state.BodyPhase.Higher;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + MagdollitePhaseDefinitions.BodyShiftPerPhase);
        state.DesiredAnimation = state.BodyPhase.Phase.BodyAnimation;
        StartMagdolliteAnimation(body, state);
    }

    /// <summary>Retraces the body's growth phases and releases the head's landing latch at its origin.</summary>
    internal void RunMagdolliteBodyFalling(
        RoomEnemyPlace body,
        MagdolliteEnemyState state)
    {
        (RoomEnemyPlace speedHead, _, _) = RequireMagdolliteComposite(body);
        MagdolliteEnemyState speedOwner = speedHead.RequireSpeciesState<MagdolliteEnemyState>();
        state.RiseDisplacement += AddMagdolliteY(body, speedOwner.Speed);

        if (state.RiseDisplacement.Y >= 0)
        {
            state.BodyWakeRequested = true;
            (RoomEnemyPlace head, _, _) = RequireMagdolliteComposite(body);
            head.RequireSpeciesState<MagdolliteEnemyState>().HeadWaitsForBodyLanding = false;
            state.Action = MagdolliteAction.BodyDormant;
            return;
        }

        int downwardDistanceRemaining = -state.RiseDisplacement.Y;
        int threshold = state.BodyPhase.Lower.Phase.DistanceThreshold;
        if (!CoordinateAxis.Precedes(downwardDistanceRemaining, threshold))
            return;

        state.BodyPhase = state.BodyPhase.Lower;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - MagdollitePhaseDefinitions.BodyShiftPerPhase);
        state.DesiredAnimation = state.BodyPhase.Phase.BodyAnimation;
        StartMagdolliteAnimation(body, state);
    }

    /// <summary>Begins following the rising body when the head enters its attack animation.</summary>
    private void RunMagdolliteOverlayWaiting(
        RoomEnemyPlace overlay,
        MagdolliteEnemyState state)
    {
        (RoomEnemyPlace head, _, _) = RequireMagdolliteComposite(overlay);
        if (head.RequireSpeciesState<MagdolliteEnemyState>().Action ==
            MagdolliteAction.HeadWaitingForAttackAnimation)
        {
            state.Action = MagdolliteAction.OverlayTrackingRisingBody;
        }
    }

    /// <summary>Holds the head radius while throwing, then restores the overlay and starts the body's descent.</summary>
    private void RunMagdolliteOverlayAnimation(
        RoomEnemyPlace overlay,
        MagdolliteEnemyState state)
    {
        if (!state.AnimationBusy)
        {
            (_, RoomEnemyPlace body, _) = RequireMagdolliteComposite(overlay);
            MagdolliteEnemyState bodyState = body.RequireSpeciesState<MagdolliteEnemyState>();
            state.DesiredAnimation = MagdolliteSegment.PillarCap;
            StartMagdolliteAnimation(overlay, state);
            bodyState.Action = MagdolliteAction.BodyFalling;
            state.Action = MagdolliteAction.OverlayTrackingFallingBody;
            bodyState.BodyReachedApex = false;
            overlay.Position = overlay.Position.WithWhole(state.ThrowOrigin);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Positions the overlay over each rising body phase, then starts its Samus-facing throw at the apex.</summary>
    internal void RunMagdolliteOverlayTrackingRise(
        RoomEnemyPlace overlay,
        MagdolliteEnemyState state,
        SamusState samus)
    {
        (_, RoomEnemyPlace body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteEnemyState bodyState = body.RequireSpeciesState<MagdolliteEnemyState>();
        if (bodyState.BodyReachedApex)
        {
            bool samusIsRight = samus is not null &&
                !CoordinateAxis.Precedes(samus.Position.X.Whole, overlay.Position.X.Whole);
            state.DesiredAnimation = samusIsRight
                ? MagdolliteSegment.RightThrow
                : MagdolliteSegment.LeftThrow;
            state.ThrowsRight = samusIsRight;
            StartMagdolliteAnimation(overlay, state);
            state.Action = MagdolliteAction.OverlayWaitingForThrowAnimation;
            state.ThrowOrigin = overlay.Position.Whole;
        }
        else
        {
            int offset = bodyState.BodyPhase.Phase.OverlayYOffset;
            overlay.Position = overlay.Position.WithWrappedWholeY(body.Position.Y.Whole - offset);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Follows the descending body until the head returns to idle, while maintaining its collision radius.</summary>
    private void RunMagdolliteOverlayTrackingFall(
        RoomEnemyPlace overlay,
        MagdolliteEnemyState state)
    {
        (RoomEnemyPlace head, RoomEnemyPlace body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteEnemyState headState = head.RequireSpeciesState<MagdolliteEnemyState>();
        MagdolliteEnemyState bodyState = body.RequireSpeciesState<MagdolliteEnemyState>();
        if (headState.Action == MagdolliteAction.HeadWaiting)
        {
            state.Action = MagdolliteAction.OverlayWaitingForAttack;
        }
        else
        {
            int offset = bodyState.BodyPhase.Phase.OverlayYOffset;
            overlay.Position = overlay.Position.WithWrappedWholeY(body.Position.Y.Whole - offset);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>
    /// Stretches the head's collision height down to the tracking overlay (#1823): the head's vertical radius becomes
    /// the distance from the overlay up to the head plus two pixels, and never less than eight, so the head's hitbox
    /// reaches the overlay as the arm grows beneath it.
    /// </summary>
    private void UpdateMagdolliteHeadRadius(RoomEnemyPlace overlay)
    {
        (RoomEnemyPlace head, _, _) = RequireMagdolliteComposite(overlay);
        int radius = CoordinateAxis.Wrap(head.Position.Y.Whole - overlay.Position.Y.Whole + 2);
        head.Radius = head.Radius.WithY(CoordinateAxis.Precedes(radius, 8) ? 8 : radius);
    }

    /// <summary>
    /// Adds a signed 1/65536-pixel Y velocity, carrying the fraction into the whole, and returns the whole-pixel move it made
    /// (only Y is meaningful).
    /// </summary>
    private static Offset16<RoomPixels> AddMagdolliteY(
        RoomEnemyPlace place,
        SignedSixteenSixteen velocity)
    {
        Position16<RoomPixels> before = place.Position.Whole;
        place.Position = place.Position.AdvanceY(velocity);
        return place.Position.Whole - before;
    }

    /// <summary>Restarts the animation only when the desired segment differs from the installed one.</summary>
    private static void StartMagdolliteAnimation(
        RoomEnemyPlace place,
        MagdolliteEnemyState state)
    {
        if (state.DesiredAnimation == state.InstalledAnimation)
            return;
        state.Playback = new(MagdolliteAnimations.StartingWith(state.DesiredAnimation), AnimationPlace.StartOfAnimation);
        state.InstalledAnimation = state.DesiredAnimation;
        place.AnimationClock.Restart();
    }

    /// <summary>Validates the retail-ordered head, body and overlay any one component found when it was placed.</summary>
    private MagdolliteComposite RequireMagdolliteComposite(RoomEnemyPlace member)
    {
        (RoomEnemyPlace head, RoomEnemyPlace body, RoomEnemyPlace overlay) = _partners.Of(member).Composite;
        if (head.DefinitionId != EnemyDefinitionId.Magdollite || head.SpawnSettings<MagdolliteSpawnSettings>().Part != MagdollitePart.Head ||
            body.DefinitionId != EnemyDefinitionId.Magdollite || body.SpawnSettings<MagdolliteSpawnSettings>().Part != MagdollitePart.RisingBody ||
            overlay.DefinitionId != EnemyDefinitionId.Magdollite || overlay.SpawnSettings<MagdolliteSpawnSettings>().Part != MagdollitePart.TrackingOverlay)
        {
            throw new InvalidDataException(
                $"Magdollite place {member.Ordinal} is not inside a retail-ordered head, body and overlay composite.");
        }
        return new(head, body, overlay);
    }

    /// <summary>How many places after its composite's head the room lists a part: the head, then the body, then the overlay.</summary>
    private static int PlacesAfterHead(MagdollitePart part) => part switch
    {
        MagdollitePart.Head => 0,
        MagdollitePart.RisingBody => 1,
        MagdollitePart.TrackingOverlay => 2,
        _ => throw new InvalidOperationException($"Undefined {nameof(MagdollitePart)} {part}."),
    };

    /// <summary>
    /// Counts down the room's lava glow once a Magdollite has started it, after the enemies' update; when the frame's
    /// time is up the glow steps to its next frame.
    /// </summary>
    internal void StepGlow()
    {
        if (!_glowStarted)
            return;

        if (--GlowStepCountdown != 0)
            return;

        GlowStepCountdown = GlowFrameUpdates;
        GlowFrame = (GlowFrame + 1) % MagdolliteShades.GlowFrames;
        ShowGlow();
    }

    /// <summary>Shows the glow's current frame on every Magdollite part in the room and on the lava they have thrown.</summary>
    private void ShowGlow()
    {
        PictureShading glow = Glow;
        foreach (RoomEnemyPlace part in _places.InUpdateOrder)
        {
            if (part.DefinitionId == EnemyDefinitionId.Magdollite)
                part.Shading = glow;
        }
        foreach (EnemyProjectile lava in _projectiles.EnemyProjectiles)
        {
            if (lava.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite)
                lava.Graphics = lava.Graphics.In(glow);
        }
    }

    /// <summary>
    /// Carries out one cue of a Magdollite part's animations: the sixteen private cues the ten
    /// authored animations reference. Every cue runs on to the next step, so none redirects.
    /// </summary>
    private MagdolliteSegment? ApplyMagdolliteCue(
        RoomEnemyPlace place,
        MagdolliteCue cue,
        Position16<RoomPixels> camera)
    {
        MagdolliteEnemyState state = place.RequireSpeciesState<MagdolliteEnemyState>();
        switch (cue.Kind)
        {
            case MagdolliteCueKind.PlaySoundIfOnScreen:
            {
                bool offScreen = CoordinateAxis.Precedes(place.Position.X.Whole, camera.X) ||
                    CoordinateAxis.Precedes(camera.X + 256, place.Position.X.Whole) ||
                    CoordinateAxis.Precedes(place.Position.Y.Whole, camera.Y) ||
                    CoordinateAxis.Precedes(camera.Y + 256, place.Position.Y.Whole);
                if (!offScreen)
                    _audio.QueueEnemySound(cue.Sound ?? throw new InvalidDataException("A Magdollite sound cue carries no sound."), priority: SoundRequestPriority.Normal);
                break;
            }
            case MagdolliteCueKind.MoveDown2:
                place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + 2);
                break;
            case MagdolliteCueKind.MoveUp2:
                place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole - 2);
                break;
            case MagdolliteCueKind.SetAnimationBusy:
                state.AnimationBusy = true;
                break;
            case MagdolliteCueKind.ClearAnimationBusy:
                state.AnimationBusy = false;
                break;
            case MagdolliteCueKind.MoveWithPillarUp1:
            {
                RoomEnemyPlace next = NextMagdollitePlace(place);
                place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole - 1);
                next.Position = next.Position.WithWrappedWholeY(next.Position.Y.Whole - 1);
                break;
            }
            case MagdolliteCueKind.MoveWithPillarDown1:
            {
                RoomEnemyPlace next = NextMagdollitePlace(place);
                place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + 1);
                next.Position = next.Position.WithWrappedWholeY(next.Position.Y.Whole + 1);
                break;
            }
            case MagdolliteCueKind.SinkAndShowParts:
            {
                (_, RoomEnemyPlace body, RoomEnemyPlace overlay) = RequireMagdolliteComposite(place);
                Position16<RoomPixels> sunk = state.Origin + new Offset16<RoomPixels>(0, MagdolliteMotionDefinitions.SunkDepth);
                place.Position = place.Position.WithWholeY(sunk.Y);
                body.Position = body.Position.WithWholeY(sunk.Y);
                body.Presence = body.Presence.Without(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.Without(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteCueKind.RestoreOrigins:
            {
                (_, RoomEnemyPlace body, _) = RequireMagdolliteComposite(place);
                place.Position = place.Position.WithWholeY(state.Origin.Y);
                body.Position = body.Position.WithWholeY(state.Origin.Y);
                break;
            }
            case MagdolliteCueKind.SurfaceAndHideParts:
            {
                (_, RoomEnemyPlace body, RoomEnemyPlace overlay) = RequireMagdolliteComposite(place);
                Position16<RoomPixels> surfaced = state.Origin + new Offset16<RoomPixels>(0, MagdolliteMotionDefinitions.SurfacedDepth);
                place.Position = place.Position.WithWholeY(surfaced.Y);
                body.Position = body.Position.WithWholeY(surfaced.Y);
                body.Presence = body.Presence.With(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.With(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteCueKind.SpawnLava:
                SpawnMagdolliteLava(place, state.ThrowsRight);
                break;
            case MagdolliteCueKind.ShiftRightUp4:
                place.Position = place.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(8, -4));
                break;
            case MagdolliteCueKind.ShiftLeftUp4:
                place.Position = place.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(-8, -4));
                break;
            case MagdolliteCueKind.ShiftRightUp8:
                place.Position = place.Position.WithWhole(state.ThrowOrigin + new Offset16<RoomPixels>(8, -8));
                break;
            case MagdolliteCueKind.StartCooldown:
                state.AttackTimer = WrappingCountdown.Start(MagdolliteMotionDefinitions.AttackCooldown);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>The enemy listed after the part, which it found when it was placed, used by the paired pillar cues.</summary>
    private RoomEnemyPlace NextMagdollitePlace(RoomEnemyPlace place) =>
        _partners.Of(place).Next ?? throw new InvalidDataException($"Magdollite place {place.Ordinal} has no following component.");

    /// <summary>Runs the shared post-combat tail.</summary>
    private void ResolveMagdolliteCombatAfterCommon(RoomEnemyPlace current)
    {
        // The follow-up reaches the two enemies listed after the part that was hit (FollowUpAfterStruckPart): a power
        // bomb also kills the rising body and the tracking overlay, whose follow-ups reach the enemies listed after the
        // Magdollite.
        IReadOnlyList<RoomEnemyPlace?> recipients = _partners.Of(current).FollowUpRecipients;
        RoomEnemyPlace? following = recipients[0];
        RoomEnemyPlace? secondFollowing = recipients[1];

        if (current.Health == 0)
        {
            if (following is not null)
                following.Presence = following.Presence.With(EnemyPresence.Deleted);
            if (secondFollowing is not null)
                secondFollowing.Presence = secondFollowing.Presence.With(EnemyPresence.Deleted);
        }

        if (current.FrozenTimer == 0)
            return;
        if (following is not null)
        {
            following.FrozenTimer = current.FrozenTimer;
            following.AiStates = following.AiStates | EnemyAiStates.Frozen;
        }
        if (secondFollowing is not null)
        {
            secondFollowing.FrozenTimer = current.FrozenTimer;
            secondFollowing.AiStates = secondFollowing.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>Attaches the species tail after common bomb damage when the shot AI selects Magdollite's reaction.</summary>
    internal EnemyBombReaction MagdolliteBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveMagdolliteCombatAfterCommon(c.Enemy),
    };

    /// <summary>Magdollite's thrown lava (#1760).</summary>
    internal EnemyProjectileBehavior Lava => field ??= new LavaBehavior(_deathsAndDrops);

    /// <summary>
    /// Thrown lava: it flies sideways until it leaves the view, handles its animations' cues, and a shot ball runs its
    /// typed shot animation, the animation its definition names.
    /// </summary>
    /// <param name="deathsAndDrops">The drops a shot ball leaves.</param>
    private sealed class LavaBehavior(EnemyDeathsAndDrops deathsAndDrops) : SingleMotionProjectileBehavior(
        context => FlyMagdolliteLava(context.Projectile, context.Camera),
        shot: () => new MagdolliteLavaAnimation(MagdolliteLavaSegment.Shot)), IEnemyProjectileCues<MagdolliteLavaCue>
    {
        /// <summary>Carries out one cue of the thrown lava's animations.</summary>
        public void RunCue(EnemyProjectileCueContext context, MagdolliteLavaCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue.Kind)
            {
                case MagdolliteLavaCueKind.Delete:
                    projectile.Delete();
                    break;
                case MagdolliteLavaCueKind.SpawnDrops:
                    // The shot animation's drop step: the ball leaves Magdollite's drop.
                    deathsAndDrops.SpawnEnemyDropFromDefinition(
                        projectile.Position.Whole,
                        EnemyDefinitionId.Magdollite);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MagdolliteLavaCueKind)} {cue.Kind}.");
            }
        }
    }

    /// <summary>Runs Magdollite's initialization AI.</summary>
    internal void RunMagdolliteInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeMagdollite(place, surroundings.IntroducedSamus);
    }

    /// <summary>Runs Magdollite's main AI.</summary>
    internal void RunMagdolliteMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMagdolliteMain(place, place.RequireSpeciesState<MagdolliteEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Magdollite's typed animation.</summary>
    internal void AdvanceMagdolliteTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, MagdolliteAnimations.Segments,
            static (MagdolliteEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyMagdolliteCue(place, cue, context.Camera) is { } target
                ? new AnimationContinuation<MagdolliteSegment>(MagdolliteAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Propagates common power-bomb death and freezing effects to the composite's following parts.</summary>
    internal EnemyPowerBombReaction MagdollitePowerBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveMagdolliteCombatAfterCommon(c.Enemy),
    };

    /// <summary>Propagates common shot death and freezing effects to the composite's following parts.</summary>
    internal EnemyShotReaction MagdolliteShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveMagdolliteCombatAfterCommon(c.Enemy),
    };

    /// <summary>Applies normal Samus contact damage before mirroring the enemy's death or freeze state.</summary>
    internal void ReactToMagdolliteTouch(EnemyTouchContext context)
    {
        _collision.ResolveNormalEnemyTouch(context.Place, context.Samus);
        ResolveMagdolliteCombatAfterCommon(context.Place);
    }
    /// <summary>Sideways speed of thrown lava, three pixels per update, negated for a leftward throw.</summary>
    private static readonly SignedEightEight MagdolliteLavaHorizontalSpeed = SignedEightEight.FromParts(3, 0);

    /// <summary>Spawns and initializes a Magdollite lava ball.</summary>
    internal void SpawnMagdolliteLava(RoomEnemyPlace source, bool throwsRight)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.LavaThrownByMagdollite,
            Lava,
            SpriteGraphics.SpawnedBy(source));

        // The initializer copies both subpixels from the throwing overlay, starts two pixels below it, and throws the
        // lava sideways in the overlay's facing.
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(CoordinateAxis.Wrap(source.Position.Y.Whole + 2), source.Position.Y.Fraction));
        projectile.Velocity = new(
            throwsRight ? MagdolliteLavaHorizontalSpeed : -MagdolliteLavaHorizontalSpeed,
            SignedEightEight.Zero);
        projectile.InstallAnimation(new MagdolliteLavaAnimation(throwsRight ? MagdolliteLavaSegment.Right : MagdolliteLavaSegment.Left));
    }

    /// <summary>Flies the lava one update: it flies sideways at its speed until it leaves the view.</summary>
    internal static void FlyMagdolliteLava(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());

        // The lava is deleted outside the inclusive 256x256 camera square after its
        // horizontal move. The lava never moves vertically.
        if (CoordinateAxis.Wrap(projectile.Position.X.Whole - camera.X) >= 256 ||
            CoordinateAxis.Wrap(projectile.Position.Y.Whole - camera.Y) >= 256)
        {
            projectile.Delete();
        }
    }
}
