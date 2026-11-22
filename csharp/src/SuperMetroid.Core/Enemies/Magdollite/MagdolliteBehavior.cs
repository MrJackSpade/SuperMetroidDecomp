using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The Magdollite species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class MagdolliteBehavior : IEnemySpecies, IEnemyLoadedFactory<MagdolliteSpawnSettings>, IRoomGlow
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Magdollite];

    /// <summary>
    /// A Magdollite's head, which faces the loading Samus and offers the body and overlay its room lists right after it the
    /// creation that builds each of them in its Magdollite (#2342, #2474, #2554). Only the room's population load places a
    /// Magdollite's parts, and a body or overlay no head offered is refused.
    /// </summary>
    /// <exception cref="InvalidDataException">The placement names a body or an overlay with no head listed before it.</exception>
    public EnemyOccupant CreateLoaded(RoomEnemyOccupant occupant, MagdolliteSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
    {
        if (settings is not MagdolliteHeadSpawnSettings headSettings)
            throw new InvalidDataException($"A Magdollite {settings} in enemy place {Ensure.NotNull(place).Ordinal} is listed with no head before it.");
        var head = new MagdolliteHeadEntity(this, occupant, place, common, surroundings, headSettings);
        _partOffers.OfferLoaded<MagdolliteSpawnSettings>(EnemyDefinitionId.Magdollite, new PartJoining(this, head.Partners).CreateLater);
        return EnemyOccupant.Running(head);
    }

    /// <summary>
    /// The creation a head offers the body and overlay its room lists right after it (#2554): each in the head's Magdollite;
    /// a later head is built as any head is.
    /// </summary>
    /// <param name="species">The Magdollite species owner.</param>
    /// <param name="partners">The head's Magdollite, which a later body or overlay joins.</param>
    private sealed partial class PartJoining(MagdolliteBehavior species, MagdollitePartners partners)
    {
        /// <summary>A later entry: the body or overlay in the head's Magdollite, or another head.</summary>
        /// <exception cref="InvalidOperationException">The settings name no Magdollite part.</exception>
        internal EnemyOccupant CreateLater(RoomEnemyOccupant occupant, MagdolliteSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
            EnemyPopulationSurroundings surroundings) => settings switch
        {
            MagdolliteHeadSpawnSettings => species.CreateLoaded(occupant, settings, place, common, surroundings),
            MagdolliteBodySpawnSettings => EnemyOccupant.Running(new MagdolliteBodyEntity(species, occupant, JoiningPart(place, partners.Composite.Body, "body"), common, partners)),
            MagdolliteOverlaySpawnSettings => EnemyOccupant.Running(new MagdolliteOverlayEntity(species, occupant, JoiningOverlay(place, partners), common, partners)),
            var unknown => throw new InvalidOperationException($"Undefined Magdollite part {unknown}."),
        };
    }

    /// <summary>Where a head offers the body and overlay listed after it the creation that builds each in its Magdollite.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>
    /// The place of a part joining its head's Magdollite, which must be where the head found the part as it was placed.
    /// </summary>
    /// <exception cref="InvalidDataException">The room lists the part elsewhere than right after its head, in order.</exception>
    private static RoomEnemyPlace JoiningPart(RoomEnemyPlace place, RoomEnemyPlace expected, string part) =>
        place == expected
            ? place
            : throw new InvalidDataException($"A Magdollite {part} in enemy place {place.Ordinal} is listed apart from its head's, which lists it in enemy place {expected.Ordinal}.");

    /// <summary>
    /// The place of an overlay joining its head's Magdollite, which completes the Magdollite: the composite is checked
    /// once then.
    /// </summary>
    /// <exception cref="InvalidDataException">The overlay is listed elsewhere, or its body is not a Magdollite's.</exception>
    private static RoomEnemyPlace JoiningOverlay(RoomEnemyPlace place, MagdollitePartners partners)
    {
        JoiningPart(place, partners.Composite.Overlay, "overlay");
        if (partners.Composite.Body.Entity is not MagdolliteBodyEntity)
            throw new InvalidDataException("A Magdollite overlay does not complete a head and a body listed in that order.");
        return place;
    }

    /// <summary>Supplies the room's apex overreads.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the lava shots emitted by the overlay's animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each Magdollite head finds its parts and each part its follow-up's recipients.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The places of the Magdollite parts placed this room, each once, which show the room's lava glow.</summary>
    private readonly List<RoomEnemyPlace> _placedParts = [];
    /// <summary>Runs common contact damage before the species reaction.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Creates the item drop when a thrown lava projectile is destroyed.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the Magdollite AI, projectile, combat and glow services.</summary>
    internal MagdolliteBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyAudioQueue audio,
        EnemyPartOffers partOffers)
    {
        _partOffers = partOffers;
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
        _places = places;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// The room's lava glow (#2439): the frame every Magdollite and its lava show, and the updates it has left. A room
    /// starts it on its first frame, stopped.
    /// </summary>
    private readonly ShowingAnimationPlayer<MagdolliteGlowSegment, MagdolliteGlow, NoAnimationCue> _glow =
        ShowingAnimationPlayer<MagdolliteGlowSegment, MagdolliteGlow, NoAnimationCue>.StartShowing(MagdolliteGlowAnimations.Segments, MagdolliteGlowAnimations.Glowing);

    /// <summary>Whether the room's lava glow runs: a Magdollite part's placement starts it, and until one has, it does not step.</summary>
    internal bool GlowRuns { get; private set; }

    /// <summary>The lava glow every Magdollite in the room, and the lava they throw, shows now (#1512).</summary>
    internal PictureShading Glow => PictureShading.Shaded(MagdolliteShades.Glow(_glow.Showing));

    /// <summary>
    /// The two enemies, at most, the part in <paramref name="place"/>'s follow-up to a hit reaches
    /// (<see cref="FollowUpAfterStruckPart"/>), nearest first: as many as the room places after the part (#2558), which the
    /// part finds as it is built.
    /// </summary>
    internal IReadOnlyList<RoomEnemyPlace> FollowUpRecipientsOf(RoomEnemyPlace place) =>
        FollowUpAfterStruckPart.RecipientsWithin(_places.AllPlacesAfter(place), MagdollitePartListingDefinitions.FollowUpReach);

    /// <summary>Records <paramref name="place"/> among the room's Magdollite parts, which show its lava glow.</summary>
    private void PlaceMagdollitePart(RoomEnemyPlace place)
    {
        if (!_placedParts.Contains(place))
            _placedParts.Add(place);
    }

    /// <summary>
    /// Restarts the room's lava glow once a part is placed: the room has one lava glow, not one per part, and each part's
    /// placement restarts it, so the last part placed sets its phase.
    /// </summary>
    private void FinishPlacingMagdollitePart(RoomEnemyPlace placing) => RestartGlow(placing);


    /// <summary>
    /// Restarts the room's lava glow at its first frame, which every Magdollite and its lava show at once, holding it
    /// for <see cref="MagdolliteMotionDefinitions.GlowFrameDuration"/> updates.
    /// </summary>
    private void RestartGlow(RoomEnemyPlace placing)
    {
        _glow.Show(MagdolliteGlowAnimations.Glowing);
        GlowRuns = true;
        // The part being placed holds its place once its entity is built, so it shows the glow with the parts already there.
        ShowGlow(part => part == placing || part.Entity is MagdollitePartEntity);
    }

    /// <summary>
    /// Places the head and starts it in its idle animation, facing Samus: it finds the body and overlay its room lists right
    /// after it and begins the states they hold as each is built, after it (#2554).
    /// </summary>
    /// <returns>The Magdollite the head and its parts hold from their setups.</returns>
    internal MagdollitePartners InitializeMagdolliteHead(
        RoomEnemyOccupant occupant,
        MagdolliteHeadSpawnSettings settings,
        RoomEnemyPlace place,
        SamusState samus)
    {
        RoomEnemyPlace Partner(MagdolliteMember partner) =>
            _places.Member(place, MagdollitePartListingDefinitions.Listing, MagdolliteMember.Head, partner);
        PlaceMagdollitePart(place);
        var state = new MagdolliteHeadState(MagdolliteHeadAction.Waiting);
        var partners = new MagdollitePartners(new MagdolliteComposite(place, Partner(MagdolliteMember.Body), Partner(MagdolliteMember.Overlay)),
            occupant, settings, state, new MagdolliteBodyState(MagdolliteBodyAction.Dormant), new MagdolliteOverlayState(MagdolliteOverlayAction.WaitingForAttack));
        state.OriginY = place.Position.Y.Whole;

        HorizontalDirection samusSide = (samus.Position.Whole - place.Position.Whole).X < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        StartMagdolliteAnimation(place, state, samusSide == HorizontalDirection.Left ? MagdolliteHeadSegment.LeftIdle : MagdolliteHeadSegment.RightIdle);
        FinishPlacingMagdollitePart(place);
        return partners;
    }

    /// <summary>
    /// Places the rising body, hidden below its origin (<see cref="MagdolliteMotionDefinitions.HiddenDepth"/>) until the head
    /// triggers an attack, with the state its head began for it.
    /// </summary>
    /// <returns>The state the body holds from its setup.</returns>
    internal MagdolliteBodyState InitializeMagdolliteBody(RoomEnemyPlace place, MagdollitePartners partners)
    {
        PlaceMagdollitePart(place);
        MagdolliteBodyState state = Ensure.NotNull(partners).BodyState;
        state.OriginY = place.Position.Y.Whole;
        StartMagdolliteAnimation(place, state, MagdolliteRiseStage.Setup.Phase.BodyAnimation);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        FinishPlacingMagdollitePart(place);
        return state;
    }

    /// <summary>
    /// Places the projectile-throwing overlay, hidden below its origin, and starts its pillar-cap animation, with the state
    /// its head began for it.
    /// </summary>
    /// <returns>The state the overlay holds from its setup.</returns>
    internal MagdolliteOverlayState InitializeMagdolliteOverlay(RoomEnemyPlace place, MagdollitePartners partners)
    {
        PlaceMagdollitePart(place);
        MagdolliteOverlayState state = Ensure.NotNull(partners).OverlayState;
        state.OriginY = place.Position.Y.Whole;
        state.AttackCooldown = UpdateCountdown.For(MagdolliteMotionDefinitions.AttackPollWait);
        StartMagdolliteAnimation(place, state, MagdolliteOverlaySegment.PillarCap);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        FinishPlacingMagdollitePart(place);
        return state;
    }

    /// <summary>What <see cref="RunMagdolliteHead"/> does for each <see cref="MagdolliteHeadAction"/> member, each member's own arm.</summary>
    private static IdentityTable<MagdolliteHeadAction, Action<MagdolliteHeadSpawnSettings, RoomEnemyPlace, MagdolliteHeadState, MagdollitePartners, SamusState>> RunMagdolliteHeadForMagdolliteHeadAction => field ??= new(new Dictionary<MagdolliteHeadAction, Action<MagdolliteHeadSpawnSettings, RoomEnemyPlace, MagdolliteHeadState, MagdollitePartners, SamusState>>
    {
        [MagdolliteHeadAction.Waiting] = (settings, place, state, partners, samus) => RunMagdolliteHeadWaiting(settings, place, state, partners, samus),
        [MagdolliteHeadAction.WaitingForAttackAnimation] = (_, _, _, _, _) => { },
        [MagdolliteHeadAction.WaitingForBodyLanding] = (_, _, _, _, _) => { },
        [MagdolliteHeadAction.WaitingForReturnAnimation] = (_, _, _, _, _) => { },
        [MagdolliteHeadAction.Submerged] = (_, _, state, partners, _) => ReleaseMagdolliteArm(state, partners),
        [MagdolliteHeadAction.ArmLanded] = (_, place, state, _, samus) => EmergeMagdolliteHead(place, state, samus),
        [MagdolliteHeadAction.Surfaced] = (_, place, state, _, samus) => IdleMagdolliteHead(place, state, samus),
    });

    /// <summary>Runs the head's current action.</summary>
    internal static void RunMagdolliteHead(MagdolliteHeadSpawnSettings magdolliteHeadSpawnSettings, RoomEnemyPlace place, MagdolliteHeadState state, MagdollitePartners partners, SamusState samus)
    {
        RunMagdolliteHeadForMagdolliteHeadAction[state.Action](magdolliteHeadSpawnSettings, place, state, partners, samus);
    }

    /// <summary>What <see cref="RunMagdolliteBody"/> does for each <see cref="MagdolliteBodyAction"/> member, each member's own arm.</summary>
    private IdentityTable<MagdolliteBodyAction, Action<RoomEnemyPlace, MagdolliteBodyState, MagdollitePartners, SamusState>> RunMagdolliteBodyForMagdolliteBodyAction => field ??= new(new Dictionary<MagdolliteBodyAction, Action<RoomEnemyPlace, MagdolliteBodyState, MagdollitePartners, SamusState>>
    {
        [MagdolliteBodyAction.Dormant] = (_, _, _, _) => { },
        [MagdolliteBodyAction.Released] = (place, state, _, _) => StartMagdolliteBodyRise(place, state),
        [MagdolliteBodyAction.Rising] = (place, state, partners, samus) => RunMagdolliteBodyRising(place, state, partners, samus),
        [MagdolliteBodyAction.HoldingAtApex] = (_, _, _, _) => { },
        [MagdolliteBodyAction.Falling] = (place, state, partners, _) => RunMagdolliteBodyFalling(place, state, partners),
    });

    /// <summary>Runs the rising body's current action.</summary>
    internal void RunMagdolliteBody(RoomEnemyPlace place, MagdolliteBodyState state, MagdollitePartners partners, SamusState samus)
    {
        RunMagdolliteBodyForMagdolliteBodyAction[state.Action](place, state, partners, samus);
    }

    /// <summary>What <see cref="RunMagdolliteOverlay"/> does for each <see cref="MagdolliteOverlayAction"/> member, each member's own arm.</summary>
    private static IdentityTable<MagdolliteOverlayAction, Action<RoomEnemyPlace, MagdolliteOverlayState, MagdollitePartners, SamusState>> RunMagdolliteOverlayForMagdolliteOverlayAction => field ??= new(new Dictionary<MagdolliteOverlayAction, Action<RoomEnemyPlace, MagdolliteOverlayState, MagdollitePartners, SamusState>>
    {
        [MagdolliteOverlayAction.WaitingForAttack] = (_, state, partners, _) => RunMagdolliteOverlayWaiting(state, partners),
        [MagdolliteOverlayAction.WaitingForThrowAnimation] = (place, _, partners, _) => UpdateMagdolliteHeadRadius(place, partners),
        [MagdolliteOverlayAction.Thrown] = (place, state, partners, _) => FinishMagdolliteOverlayThrow(place, state, partners),
        [MagdolliteOverlayAction.TrackingRisingBody] = (place, state, partners, samus) => RunMagdolliteOverlayTrackingRise(place, state, partners, samus),
        [MagdolliteOverlayAction.TrackingFallingBody] = (place, state, partners, _) => RunMagdolliteOverlayTrackingFall(place, state, partners),
    });

    /// <summary>Counts down the attack cooldown and runs the tracking overlay's current action.</summary>
    internal static void RunMagdolliteOverlay(RoomEnemyPlace place, MagdolliteOverlayState state, MagdollitePartners partners, SamusState samus)
    {
        state.AttackCooldown.Step();
        RunMagdolliteOverlayForMagdolliteOverlayAction[state.Action](place, state, partners, samus);
    }

    /// <summary>Tracks Samus while idle and begins submerging when the overlay cooldown expires within the head's horizontal range.</summary>
    private static void RunMagdolliteHeadWaiting(
        MagdolliteHeadSpawnSettings magdolliteHeadSpawnSettings, RoomEnemyPlace head,
        MagdolliteHeadState state,
        MagdollitePartners partners,
        SamusState samus)
    {
        HorizontalDirection samusSide = SamusSide(samus, head);
        StartMagdolliteAnimation(head, state, samusSide == HorizontalDirection.Right ? MagdolliteHeadSegment.RightIdle : MagdolliteHeadSegment.LeftIdle);

        (_, _, RoomEnemyPlace overlay) = partners.Composite;
        MagdolliteOverlayState overlayState = partners.OverlayState;
        if (overlayState.AttackCooldown.IsRunning)
            return;

        overlayState.AttackCooldown = UpdateCountdown.For(MagdolliteMotionDefinitions.AttackPollWait);
        int range = magdolliteHeadSpawnSettings.Range;
        if (Math.Abs(head.Position.X.Whole - samus.Position.X.Whole) >= range)
            return;

        StartMagdolliteAnimation(head, state, samusSide == HorizontalDirection.Right ? MagdolliteHeadSegment.RightSubmerge : MagdolliteHeadSegment.LeftSubmerge);
        state.Action = MagdolliteHeadAction.WaitingForAttackAnimation;
    }

    /// <summary>Releases the arm to rise once the head has submerged, and waits for it to land.</summary>
    private static void ReleaseMagdolliteArm(
        MagdolliteHeadState state,
        MagdollitePartners partners)
    {
        (_, RoomEnemyPlace body, _) = partners.Composite;
        partners.BodyState.Release();
        state.Action = MagdolliteHeadAction.WaitingForBodyLanding;
    }

    /// <summary>Starts the Samus-facing emerge animation once the arm has landed.</summary>
    private static void EmergeMagdolliteHead(
        RoomEnemyPlace head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        StartMagdolliteAnimation(head, state, SamusSide(samus, head) == HorizontalDirection.Right ? MagdolliteHeadSegment.RightEmerge : MagdolliteHeadSegment.LeftEmerge);
        state.Action = MagdolliteHeadAction.WaitingForReturnAnimation;
    }

    /// <summary>Restores the appropriate idle animation once the head has surfaced.</summary>
    private static void IdleMagdolliteHead(
        RoomEnemyPlace head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        StartMagdolliteAnimation(head, state, SamusSide(samus, head) == HorizontalDirection.Right ? MagdolliteHeadSegment.RightIdle : MagdolliteHeadSegment.LeftIdle);
        state.Action = MagdolliteHeadAction.Waiting;
    }

    /// <summary>Returns the hidden body to its origin and starts it rising, as its head released it.</summary>
    private static void StartMagdolliteBodyRise(
        RoomEnemyPlace body,
        MagdolliteBodyState state)
    {
        state.Action = MagdolliteBodyAction.Rising;
        state.RiseDisplacementY = 0;
        state.BodyPhase = MagdolliteRiseStage.OneSegment;
        body.Position = body.Position.WithWholeY(state.OriginY);
    }

    /// <summary>Rises using the preceding head's velocity, advances growth phases at their thresholds, and stops at the apex or above Samus.</summary>
    internal void RunMagdolliteBodyRising(
        RoomEnemyPlace body,
        MagdolliteBodyState state,
        MagdollitePartners partners,
        SamusState samus)
    {
        // The arm rises at the speed its head's spawn settings name (#2474).
        state.RiseDisplacementY += AddMagdolliteY(body, -partners.HeadRise.Speed);

        int upwardDistance = -state.RiseDisplacementY;
        MagdollitePhaseDefinition phase = state.BodyPhase.Phase;
        bool roseMaximum = upwardDistance >= MagdollitePhaseDefinitions.MaximumRise;
        bool rosePastSamus = body.Position.Y.Whole - phase.OverlayYOffset < samus.Position.Y.Whole;
        // At full height whether the arm grows its last segment depends on the place it occupies
        // (MagdolliteApexGrowth); the Samus test compares with the phase's own threshold.
        bool growsAPhase = roseMaximum
            ? _room.Session.Overreads.MagdolliteApexGrowth.GrowsFinalSegment(body)
            : upwardDistance >= phase.DistanceThreshold;

        if (roseMaximum || rosePastSamus)
        {
            state.Action = MagdolliteBodyAction.HoldingAtApex;
            if (!growsAPhase)
                return;
        }
        else if (!growsAPhase)
        {
            return;
        }

        state.BodyPhase = state.BodyPhase.Higher;
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + MagdollitePhaseDefinitions.BodyShiftPerPhase);
        StartMagdolliteAnimation(body, state, state.BodyPhase.Phase.BodyAnimation);
    }

    /// <summary>Retraces the body's growth phases and, landing at its origin, lets the head emerge.</summary>
    internal static void RunMagdolliteBodyFalling(
        RoomEnemyPlace body,
        MagdolliteBodyState state,
        MagdollitePartners partners)
    {
        (RoomEnemyPlace head, _, _) = partners.Composite;
        MagdolliteHeadState headState = partners.HeadState;
        state.RiseDisplacementY += AddMagdolliteY(body, partners.HeadRise.Speed);

        if (state.RiseDisplacementY >= 0)
        {
            state.Action = MagdolliteBodyAction.Dormant;
            headState.ArmLanded();
            return;
        }

        int downwardDistanceRemaining = -state.RiseDisplacementY;
        int threshold = state.BodyPhase.Lower.Phase.DistanceThreshold;
        if (downwardDistanceRemaining >= threshold)
            return;

        state.BodyPhase = state.BodyPhase.Lower;
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole - MagdollitePhaseDefinitions.BodyShiftPerPhase);
        StartMagdolliteAnimation(body, state, state.BodyPhase.Phase.BodyAnimation);
    }

    /// <summary>Begins following the rising body when the head enters its attack animation.</summary>
    private static void RunMagdolliteOverlayWaiting(
        MagdolliteOverlayState state,
        MagdollitePartners partners)
    {
        (RoomEnemyPlace head, _, _) = partners.Composite;
        if (partners.HeadState.IsSubmerging)
            state.Action = MagdolliteOverlayAction.TrackingRisingBody;
    }

    /// <summary>Once the throw has finished, restores the overlay, starts the body's descent and holds the head radius.</summary>
    private static void FinishMagdolliteOverlayThrow(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state,
        MagdollitePartners partners)
    {
        (_, RoomEnemyPlace body, _) = partners.Composite;
        MagdolliteBodyState bodyState = partners.BodyState;
        StartMagdolliteAnimation(overlay, state, MagdolliteOverlaySegment.PillarCap);
        bodyState.Action = MagdolliteBodyAction.Falling;
        state.Action = MagdolliteOverlayAction.TrackingFallingBody;
        overlay.Position = overlay.Position.WithWhole(state.ThrowOrigin);
        UpdateMagdolliteHeadRadius(overlay, partners);
    }

    /// <summary>Positions the overlay over each rising body phase, then starts its Samus-facing throw at the apex.</summary>
    internal static void RunMagdolliteOverlayTrackingRise(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state,
        MagdollitePartners partners,
        SamusState samus)
    {
        (_, RoomEnemyPlace body, _) = partners.Composite;
        MagdolliteBodyState bodyState = partners.BodyState;
        if (bodyState.Action == MagdolliteBodyAction.HoldingAtApex)
        {
            state.ThrowDirection = SamusSide(samus, overlay);
            StartMagdolliteAnimation(overlay, state, state.ThrowDirection == HorizontalDirection.Right ? MagdolliteOverlaySegment.RightThrow : MagdolliteOverlaySegment.LeftThrow);
            state.Action = MagdolliteOverlayAction.WaitingForThrowAnimation;
            state.ThrowOrigin = overlay.Position.Whole;
        }
        else
        {
            int offset = bodyState.BodyPhase.Phase.OverlayYOffset;
            overlay.Position = overlay.Position.WithWholeY(body.Position.Y.Whole - offset);
        }

        UpdateMagdolliteHeadRadius(overlay, partners);
    }

    /// <summary>Follows the descending body until the head returns to idle, while maintaining its collision radius.</summary>
    private static void RunMagdolliteOverlayTrackingFall(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state,
        MagdollitePartners partners)
    {
        (RoomEnemyPlace head, RoomEnemyPlace body, _) = partners.Composite;
        MagdolliteHeadState headState = partners.HeadState;
        MagdolliteBodyState bodyState = partners.BodyState;
        if (headState.Action == MagdolliteHeadAction.Waiting)
        {
            state.Action = MagdolliteOverlayAction.WaitingForAttack;
        }
        else
        {
            int offset = bodyState.BodyPhase.Phase.OverlayYOffset;
            overlay.Position = overlay.Position.WithWholeY(body.Position.Y.Whole - offset);
        }

        UpdateMagdolliteHeadRadius(overlay, partners);
    }

    /// <summary>
    /// Stretches the head's collision height down to the tracking overlay (#1823): the head's vertical radius becomes
    /// the distance from the overlay up to the head plus its reach past the overlay, and never less than its smallest
    /// radius, so the head's hitbox
    /// reaches the overlay as the arm grows beneath it.
    /// </summary>
    private static void UpdateMagdolliteHeadRadius(RoomEnemyPlace overlay, MagdollitePartners partners)
    {
        (RoomEnemyPlace head, _, _) = partners.Composite;
        int radius = head.Position.Y.Whole - overlay.Position.Y.Whole + MagdolliteMotionDefinitions.HeadReachPastOverlay;
        head.Radius = head.Radius.WithY(Math.Max(radius, MagdolliteMotionDefinitions.SmallestHeadRadius));
    }

    /// <summary>
    /// Adds a Y velocity, carrying the fraction into the whole, and returns the whole-pixel vertical
    /// move it made.
    /// </summary>
    private static int AddMagdolliteY(
        RoomEnemyPlace place,
        SignedSixteenSixteen velocity)
    {
        int before = place.Position.Y.Whole;
        place.Position = place.Position.AdvanceY(velocity);
        return place.Position.Y.Whole - before;
    }

    /// <summary>Starts <paramref name="segment"/>'s animation only when it differs from the one the head plays (#2145).</summary>
    private static void StartMagdolliteAnimation(RoomEnemyPlace place, MagdolliteHeadState state, MagdolliteHeadSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, MagdolliteHeadAnimations.StartingAt[segment]);

    /// <summary>Starts <paramref name="segment"/>'s animation only when it differs from the one the body plays (#2145).</summary>
    private static void StartMagdolliteAnimation(RoomEnemyPlace place, MagdolliteBodyState state, MagdolliteBodySegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, MagdolliteBodyAnimations.StartingAt[segment]);

    /// <summary>Starts <paramref name="segment"/>'s animation only when it differs from the one the overlay plays (#2145).</summary>
    private static void StartMagdolliteAnimation(RoomEnemyPlace place, MagdolliteOverlayState state, MagdolliteOverlaySegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, MagdolliteOverlayAnimations.StartingAt[segment]);

    /// <summary>
    /// Counts down the room's lava glow once a Magdollite has started it, after the enemies' update; when the frame's
    /// time is up the glow steps to its next frame.
    /// </summary>
    public void StepGlow()
    {
        if (!GlowRuns || _glow.StepToNewFrame() is null)
            return;

        ShowGlow();
    }

    /// <summary>
    /// Shows the glow's current frame on every Magdollite part still in the place the species set it up in, and on the
    /// lava they have thrown (#2306).
    /// </summary>
    private void ShowGlow() => ShowGlow(static part => part.Entity is MagdollitePartEntity);

    /// <summary>Shows the glow's current frame on each part <paramref name="showsGlow"/> picks and on the lava thrown.</summary>
    private void ShowGlow(Func<RoomEnemyPlace, bool> showsGlow)
    {
        PictureShading glow = Glow;
        foreach (RoomEnemyPlace part in _placedParts)
        {
            // A place whose part has left it, and any enemy that took the place since, shows no glow.
            if (showsGlow(part))
                part.Shading = glow;
        }
        foreach (EnemyProjectile lava in _projectiles.EnemyProjectiles)
        {
            if (lava.Behavior is { ShowsThrowersGlow: true })
                lava.Graphics = lava.Graphics.In(glow);
        }
    }

    /// <summary>
    /// Carries out one cue of the head's animations: a move of the head or of the head with the body's pillar, or a signal
    /// (<see cref="MagdolliteHeadCueKind"/>). Every cue runs on to the next step, so none redirects.
    /// </summary>
    private static void ApplyMagdolliteHeadCue(RoomEnemyPlace place, MagdolliteHeadState state, MagdollitePartners partners, MagdolliteHeadCue cue)
    {
        switch (cue)
        {
            case MagdolliteHeadCue.Move move:
                place.Position += move.Offset;
                break;
            case MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.FinishSubmerge }:
                state.FinishSubmerge();
                break;
            case MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.FinishEmerge }:
                state.FinishEmerge();
                break;
            case MagdolliteHeadCue.MoveWithPillar moveWithPillar:
            {
                // The head's submerge and emerge move it with the body's pillar.
                (_, RoomEnemyPlace body, _) = partners.Composite;
                place.Position += moveWithPillar.Offset;
                body.Position += moveWithPillar.Offset;
                break;
            }
            case MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.SinkAndShowParts }:
            {
                (_, RoomEnemyPlace body, RoomEnemyPlace overlay) = partners.Composite;
                int sunkY = state.OriginY + MagdolliteMotionDefinitions.SunkDepth;
                place.Position = place.Position.WithWholeY(sunkY);
                body.Position = body.Position.WithWholeY(sunkY);
                body.Presence = body.Presence.Without(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.Without(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.RestoreOrigins }:
            {
                (_, RoomEnemyPlace body, _) = partners.Composite;
                place.Position = place.Position.WithWholeY(state.OriginY);
                body.Position = body.Position.WithWholeY(state.OriginY);
                break;
            }
            case MagdolliteHeadCue.Signal { Kind: MagdolliteHeadCueKind.SurfaceAndHideParts }:
            {
                (_, RoomEnemyPlace body, RoomEnemyPlace overlay) = partners.Composite;
                int surfacedY = state.OriginY + MagdolliteMotionDefinitions.SurfacedDepth;
                place.Position = place.Position.WithWholeY(surfacedY);
                body.Position = body.Position.WithWholeY(surfacedY);
                body.Presence = body.Presence.With(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.With(EnemyPresence.Hidden);
                break;
            }
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteHeadCue)} {cue}.");
        }
    }

    /// <summary>
    /// Carries out one cue of the tracking overlay's animations: a sound heard while the overlay is in view, a shift from
    /// the throw's origin, or a signal (<see cref="MagdolliteOverlayCueKind"/>). Every cue runs on to the next step, so
    /// none redirects.
    /// </summary>
    private void ApplyMagdolliteOverlayCue(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        MagdolliteOverlayState state,
        MagdolliteOverlayCue cue,
        Position16<RoomPixels> camera)
    {
        switch (cue)
        {
            case MagdolliteOverlayCue.PlaySoundIfOnScreen playSound:
            {
                if (EnemyView.Contains(place.Position.Whole, camera))
                    _audio.QueueEnemySound(playSound.Sound, priority: SoundRequestPriority.Normal);
                break;
            }
            case MagdolliteOverlayCue.Signal { Kind: MagdolliteOverlayCueKind.FinishThrow }:
                state.FinishThrow();
                break;
            case MagdolliteOverlayCue.Signal { Kind: MagdolliteOverlayCueKind.SpawnLava }:
                SpawnMagdolliteLava(occupant, place, state.ThrowDirection);
                break;
            case MagdolliteOverlayCue.ShiftFromThrowOrigin shift:
                place.Position = place.Position.WithWhole(state.ThrowOrigin + shift.Offset);
                break;
            case MagdolliteOverlayCue.Signal { Kind: MagdolliteOverlayCueKind.StartCooldown }:
                state.AttackCooldown = UpdateCountdown.Lasting(MagdolliteMotionDefinitions.AttackCooldown);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteOverlayCue)} {cue}.");
        }
    }

    /// <summary>
    /// Passes a struck part's fate to the two enemies listed after it, once any hit's ordinary damage is done: a part
    /// killed deletes them, and a frozen part freezes them for as long.
    /// </summary>
    internal static void ResolveMagdolliteCombatAfterCommon(RoomEnemyPlace current, IReadOnlyList<RoomEnemyPlace> recipients)
    {
        // The follow-up reaches the two enemies listed after the part that was hit (FollowUpAfterStruckPart): a power
        // bomb also kills the rising body and the tracking overlay, whose follow-ups reach the enemies listed after the
        // Magdollite.

        if (current.Health == 0)
        {
            foreach (RoomEnemyPlace following in recipients)
                following.Presence = following.Presence.With(EnemyPresence.Deleted);
        }

        if (current.Freeze.IsOver)
            return;
        foreach (RoomEnemyPlace following in recipients)
        {
            following.Freeze.Restart(current.Freeze.UpdatesLeft);
            following.MarkFrozen();
        }
    }

    /// <summary>Magdollite's reaction to a bomb: after its ordinary damage, the struck part's fate passes to the enemies listed after it.</summary>
    /// <param name="part">The part the reaction is built for, whose own method passes its fate on.</param>
    internal static EnemyBombReaction MagdolliteBombReaction(MagdollitePartEntity part) => new()
    {
        AfterCommon = Ensure.NotNull(part).PassFateOn,
    };

    /// <summary>Magdollite's thrown lava (#1760).</summary>
    internal LavaBehavior Lava => field ??= new LavaBehavior(_deathsAndDrops);

    /// <summary>
    /// Thrown lava: it flies sideways until it leaves the view, handles its animations' cues, and a shot ball runs its
    /// typed shot animation, the animation its definition names.
    /// </summary>
    /// <param name="deathsAndDrops">The drops a shot ball leaves.</param>
    internal sealed partial class LavaBehavior(EnemyDeathsAndDrops deathsAndDrops) : SingleMotionProjectileBehavior(FlyLava),
        IEnemyProjectileCues<MagdolliteLavaCue>
    {
        /// <summary>Flies thrown lava sideways until it leaves the view.</summary>
        private static void FlyLava(EnemyProjectileMotionContext context) => FlyMagdolliteLava(context.Projectile, context.Camera);

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new MagdolliteLavaAnimation(this, MagdolliteLavaSegment.Shot);

        /// <summary>What <see cref="RunCue"/> does for each <see cref="MagdolliteLavaCueKind"/> member, each member's own arm.</summary>
        private IdentityTable<MagdolliteLavaCueKind, Action<EnemyProjectileCueContext, EnemyProjectile>> RunCueForMagdolliteLavaCueKind => field ??= new(new Dictionary<MagdolliteLavaCueKind, Action<EnemyProjectileCueContext, EnemyProjectile>>
        {
            [MagdolliteLavaCueKind.Delete] = (context, projectile) => projectile.Delete(),
            [MagdolliteLavaCueKind.SpawnDrops] = (context, projectile) =>
            {
                // The shot animation's drop step: the ball leaves Magdollite's drop.
                deathsAndDrops.SpawnEnemyDropFromDefinition(
                    projectile.Position.Whole,
                    EnemyDefinitionId.Magdollite,
                    context.Samus);
            },
        });
        /// <inheritdoc/>
        /// <remarks>Thrown lava glows with the Magdollites, whatever Magdollite threw it.</remarks>
        internal override bool ShowsThrowersGlow => true;

        /// <summary>Carries out one cue of the thrown lava's animations.</summary>
        public void RunCue(EnemyProjectileCueContext context, MagdolliteLavaCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            RunCueForMagdolliteLavaCueKind[cue.Kind](context, projectile);
        }
    }

    /// <summary>Advances the head's typed animation, carrying out its cues.</summary>
    internal static void AdvanceMagdolliteHeadAnimation(RoomEnemyPlace place, MagdolliteHeadState head, MagdollitePartners partners) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, head, MagdolliteHeadAnimations.Segments,
            static (MagdolliteHeadState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyMagdolliteHeadCue(place, state, partners, cue));

    /// <summary>Advances the rising body's cue-free typed animation.</summary>
    internal static void AdvanceMagdolliteBodyAnimation(RoomEnemyPlace place, MagdolliteBodyState body) =>
        EnemyAnimationPlayback.Advance(place, body, MagdolliteBodyAnimations.Segments,
            static (MagdolliteBodyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Magdollite body animation raised {cue}."));

    /// <summary>Advances the tracking overlay's typed animation, carrying out its cues.</summary>
    internal void AdvanceMagdolliteOverlayAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, MagdolliteOverlayState overlay, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, overlay, MagdolliteOverlayAnimations.Segments,
            static (MagdolliteOverlayState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyMagdolliteOverlayCue(occupant, place, state, cue, context.Camera));

    /// <summary>Propagates common power-bomb death and freezing effects to the composite's following parts.</summary>
    /// <param name="part">The part the reaction is built for, whose own method passes its fate on.</param>
    internal static EnemyPowerBombReaction MagdollitePowerBombReaction(MagdollitePartEntity part) => new()
    {
        AfterCommon = Ensure.NotNull(part).PassFateOn,
    };

    /// <summary>Propagates common shot death and freezing effects to the composite's following parts.</summary>
    /// <param name="part">The part the reaction is built for, whose own method passes its fate on.</param>
    internal static EnemyShotReaction MagdolliteShotReaction(MagdollitePartEntity part) => new()
    {
        AfterCommon = Ensure.NotNull(part).PassFateOn,
    };

    /// <summary>Applies normal Samus contact damage before mirroring the enemy's death or freeze state.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="recipients">The enemies the touched part's follow-up reaches.</param>
    internal void ReactToMagdolliteTouch(EnemyTouchContext context, IReadOnlyList<RoomEnemyPlace> recipients)
    {
        _collision.ResolveOrdinaryTouch(context);
        ResolveMagdolliteCombatAfterCommon(context.Place, recipients);
    }

    /// <summary>The side of <paramref name="part"/> Samus is on: right unless she is to its left (#2338).</summary>
    private static HorizontalDirection SamusSide(SamusState samus, RoomEnemyPlace part) =>
        samus.Position.X.Whole < part.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right;

    /// <summary>Spawns and initializes a Magdollite lava ball thrown <paramref name="thrown"/>.</summary>
    internal void SpawnMagdolliteLava(RoomEnemyOccupant occupant, RoomEnemyPlace source, HorizontalDirection thrown)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.LavaThrownByMagdollite, Lava, SpriteGraphics.SpawnedBy(occupant, source.Shading), new MagdolliteLavaAnimation(Lava, thrown == HorizontalDirection.Right ? MagdolliteLavaSegment.Right : MagdolliteLavaSegment.Left));
        if (projectile is null)
            return;


        // The lava starts where the throwing overlay stands, with its fractions of a pixel, a drop below it, and flies
        // sideways the way the overlay faces.
        projectile.Position = source.Position.WithWholeY(source.Position.Y.Whole + MagdolliteMotionDefinitions.LavaDrop);
        projectile.Velocity = new(
            MagdolliteMotionDefinitions.LavaThrowSpeed.Toward(thrown),
            SignedEightEight.Zero);
    }

    /// <summary>Flies the lava one update: it flies sideways at its speed until it leaves the view.</summary>
    internal static void FlyMagdolliteLava(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());

        // The lava leaves once past the view after its horizontal move, the view's right and bottom edges already
        // outside. The lava never moves vertically.
        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }
}
