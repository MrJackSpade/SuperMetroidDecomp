using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>The Magdollite species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MagdolliteBehavior : IEnemySpecies, IEnemySettingsFactory<MagdolliteSpawnSettings>, IRoomGlow
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Magdollite];

    /// <summary>
    /// The Magdollite part the placement names: its head, rising body or tracking overlay (#2342, #2474), each part's settings
    /// building their own part (#2554).
    /// </summary>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, MagdolliteSpawnSettings settings) =>
        Ensure.NotNull(settings).Create(this, place, common, setup);

    /// <summary>Supplies the room's apex overreads.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the lava shots emitted by the overlay's animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each Magdollite part finds the others when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The partners each Magdollite part found when it was placed.</summary>
    private readonly EnemyPartLinks<MagdollitePartners> _partners = new();

    /// <summary>The parts' states each head made as it was placed, which its body and overlay take as each is placed (#2554).</summary>
    private readonly EnemyPartLinks<MagdolliteStates> _states = new();
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

    /// <summary>
    /// The room's lava glow (#2439): the frame every Magdollite and its lava show, and the updates it has left. A room
    /// starts it on its first frame, stopped.
    /// </summary>
    private readonly ShowingAnimationPlayer<MagdolliteGlowSegment, MagdolliteGlow, NoAnimationCue> _glow =
        ShowingAnimationPlayer<MagdolliteGlowSegment, MagdolliteGlow, NoAnimationCue>.StartShowing(MagdolliteGlowAnimations.Segments, MagdolliteGlowAnimations.Glowing);

    /// <summary>Whether the room's lava glow runs: a Magdollite part's placement starts it, and until one has, it does not step.</summary>
    internal bool GlowRuns { get; private set; }

    /// <summary>The lava glow every Magdollite in the room, and the lava they throw, shows now (#1512).</summary>
    private PictureShading Glow => PictureShading.Shaded(MagdolliteShades.Glow(_glow.Showing));

    /// <summary>
    /// Finds the partners of a part <paramref name="placesAfterHead"/> places after its head. The room lists each
    /// Magdollite's head, body and overlay in a row; each part finds the others and the two enemies its follow-up to a hit
    /// reaches. The overlay, placed last, completes the composite, which is checked once then.
    /// </summary>
    /// <exception cref="InvalidDataException">The overlay does not follow a head and a body.</exception>
    /// <returns>The states of the part's Magdollite, which its head made as it was placed.</returns>
    /// <param name="place">The part's place.</param>
    /// <param name="placesAfterHead">How many places after its head the room lists the part.</param>
    /// <param name="statesFor">The states of the part's Magdollite, given its body's and overlay's places.</param>
    private MagdolliteStates PlaceMagdollitePart(RoomEnemyPlace place, int placesAfterHead,
        Func<RoomEnemyPlace, RoomEnemyPlace, MagdolliteStates> statesFor) =>
        PlaceMagdollitePart(place, placesAfterHead, _partners.Of(_places.ListedBefore(place, placesAfterHead)).Head);

    /// <summary>
    /// Places a part <paramref name="placesAfterHead"/> places after the head <paramref name="head"/>, which the head
    /// hands itself to as it is placed first and its body and overlay find among its partners.
    /// </summary>
    /// <exception cref="InvalidDataException">The overlay does not follow a head and a body.</exception>
    private void PlaceMagdollitePart(RoomEnemyPlace place, int placesAfterHead, RoomEnemyOccupant head)
    {
        RoomEnemyPlace head = placesAfterHead == 0 ? place : _places.ListedBefore(place, placesAfterHead);
        RoomEnemyPlace body = _places.ListedAfter(head, MagdollitePartListingDefinitions.BodyPlacesAfterHead);
        RoomEnemyPlace overlay = _places.ListedAfter(head, MagdollitePartListingDefinitions.OverlayPlacesAfterHead);
        MagdolliteStates states = statesFor(body, overlay);
        var composite = new MagdolliteComposite(head, body, overlay, states);
        _partners.Place(place, new MagdollitePartners(
            Composite: composite,
            Head: head,
            FollowUpRecipients: FollowUpAfterStruckPart.RecipientsWithin(_places.AllListedAfter(place), MagdollitePartListingDefinitions.FollowUpReach)));
        if (place == composite.Overlay &&
            (composite.Head.Entity is not MagdolliteHeadEntity || composite.Body.Entity is not MagdolliteBodyEntity))
        {
            throw new InvalidDataException(
                $"Magdollite overlay in place {place.Ordinal} does not follow a head and a body listed in that order.");
        }
        return states;
    }

    /// <summary>
    /// Restarts the room's lava glow once a part is placed: the room has one lava glow, not one per part, and each part's
    /// placement restarts it, so the last part placed sets its phase.
    /// </summary>
    private void FinishPlacingMagdollitePart() => RestartGlow();


    /// <summary>
    /// Restarts the room's lava glow at its first frame, which every Magdollite and its lava show at once, holding it
    /// for <see cref="MagdolliteMotionDefinitions.GlowFrameDuration"/> updates.
    /// </summary>
    private void RestartGlow()
    {
        _glow.Show(MagdolliteGlowAnimations.Glowing);
        GlowRuns = true;
        ShowGlow();
    }

    /// <summary>
    /// The states the head, placed first, makes for all three parts, rising as <paramref name="settings"/> name; its body and
    /// overlay take theirs as each is placed.
    /// </summary>
    private MagdolliteStates MakeMagdolliteStates(RoomEnemyOccupant occupant,
        RoomEnemyPlace body, RoomEnemyPlace overlay, MagdolliteHeadSpawnSettings settings)
    {
        var states = new MagdolliteStates(
            new MagdolliteHeadState(MagdolliteHeadAction.Waiting, settings),
            new MagdolliteBodyState(MagdolliteBodyAction.Dormant),
            new MagdolliteOverlayState(MagdolliteOverlayAction.WaitingForAttack));
        _states.Place(body, states);
        _states.Place(overlay, states);
        return states;
    }

    /// <summary>Places the head and starts it in its idle animation, facing Samus.</summary>
    internal MagdolliteHeadState InitializeMagdolliteHead(
        RoomEnemyPlace place,
        SamusState samus,
        MagdolliteHeadSpawnSettings settings)
    {
        MagdolliteHeadState state = PlaceMagdollitePart(place, placesAfterHead: 0, (body, overlay) => MakeMagdolliteStates(body, overlay, settings)).Head;
        state.OriginY = place.Position.Y.Whole;

        HorizontalDirection samusSide = (samus.Position.Whole - place.Position.Whole).X < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        StartMagdolliteAnimation(place, state, samusSide == HorizontalDirection.Left ? MagdolliteSegment.LeftIdle : MagdolliteSegment.RightIdle);
        FinishPlacingMagdollitePart();
        return state;
    }

    /// <summary>Places the rising body, hidden below its origin (<see cref="MagdolliteMotionDefinitions.HiddenDepth"/>) until the head triggers an attack.</summary>
    internal MagdolliteBodyState InitializeMagdolliteBody(RoomEnemyPlace place)
    {
        MagdolliteBodyState state = PlaceMagdollitePart(place, MagdollitePartListingDefinitions.BodyPlacesAfterHead, (_, _) => _states.Of(place)).Body;
        state.OriginY = place.Position.Y.Whole;
        StartMagdolliteAnimation(place, state, MagdolliteRiseStage.Setup.Phase.BodyAnimation);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        FinishPlacingMagdollitePart();
        return state;
    }

    /// <summary>Places the projectile-throwing overlay, hidden below its origin, and starts its pillar-cap animation.</summary>
    internal MagdolliteOverlayState InitializeMagdolliteOverlay(RoomEnemyPlace place)
    {
        MagdolliteOverlayState state = PlaceMagdollitePart(place, MagdollitePartListingDefinitions.OverlayPlacesAfterHead, (_, _) => _states.Of(place)).Overlay;
        state.OriginY = place.Position.Y.Whole;
        state.AttackCooldown = UpdateCountdown.For(MagdolliteMotionDefinitions.AttackPollWait);
        StartMagdolliteAnimation(place, state, MagdolliteSegment.PillarCap);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        FinishPlacingMagdollitePart();
        return state;
    }

    /// <summary>Runs the head's current action.</summary>
    internal void RunMagdolliteHead(RoomEnemyOccupant occupant, RoomEnemyPlace place, MagdolliteHeadState state, SamusState samus)
    {
        switch (state.Action)
        {
            case MagdolliteHeadAction.Waiting:
                RunMagdolliteHeadWaiting(occupant, place, state, samus);
                return;
            case MagdolliteHeadAction.WaitingForAttackAnimation:
            case MagdolliteHeadAction.WaitingForBodyLanding:
            case MagdolliteHeadAction.WaitingForReturnAnimation:
                return;
            case MagdolliteHeadAction.Submerged:
                ReleaseMagdolliteArm(place, state);
                return;
            case MagdolliteHeadAction.ArmLanded:
                EmergeMagdolliteHead(place, state, samus);
                return;
            case MagdolliteHeadAction.Surfaced:
                IdleMagdolliteHead(place, state, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite head has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the rising body's current action.</summary>
    internal void RunMagdolliteBody(RoomEnemyPlace place, MagdolliteBodyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case MagdolliteBodyAction.Dormant:
                return;
            case MagdolliteBodyAction.Released:
                StartMagdolliteBodyRise(place, state);
                return;
            case MagdolliteBodyAction.Rising:
                RunMagdolliteBodyRising(place, state, samus);
                return;
            case MagdolliteBodyAction.HoldingAtApex:
                return;
            case MagdolliteBodyAction.Falling:
                RunMagdolliteBodyFalling(place, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite body has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Counts down the attack cooldown and runs the tracking overlay's current action.</summary>
    internal void RunMagdolliteOverlay(RoomEnemyPlace place, MagdolliteOverlayState state, SamusState samus)
    {
        state.AttackCooldown.Step();
        switch (state.Action)
        {
            case MagdolliteOverlayAction.WaitingForAttack:
                RunMagdolliteOverlayWaiting(place, state);
                return;
            case MagdolliteOverlayAction.WaitingForThrowAnimation:
                UpdateMagdolliteHeadRadius(place);
                return;
            case MagdolliteOverlayAction.Thrown:
                FinishMagdolliteOverlayThrow(place, state);
                return;
            case MagdolliteOverlayAction.TrackingRisingBody:
                RunMagdolliteOverlayTrackingRise(place, state, samus);
                return;
            case MagdolliteOverlayAction.TrackingFallingBody:
                RunMagdolliteOverlayTrackingFall(place, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite overlay has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Tracks Samus while idle and begins submerging when the overlay cooldown expires within the head's horizontal range.</summary>
    private void RunMagdolliteHeadWaiting(
        RoomEnemyOccupant occupant, RoomEnemyPlace head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        HorizontalDirection samusSide = SamusSide(samus, head);
        StartMagdolliteAnimation(head, state, samusSide == HorizontalDirection.Right ? MagdolliteSegment.RightIdle : MagdolliteSegment.LeftIdle);

        (_, _, RoomEnemyPlace overlay, MagdolliteStates parts) = RequireMagdolliteComposite(head);
        MagdolliteOverlayState overlayState = parts.Overlay;
        if (overlayState.AttackCooldown.IsRunning)
            return;

        overlayState.AttackCooldown = UpdateCountdown.For(MagdolliteMotionDefinitions.AttackPollWait);
        int range = state.Settings.Range;
        if (Math.Abs(head.Position.X.Whole - samus.Position.X.Whole) >= range)
            return;

        StartMagdolliteAnimation(head, state, samusSide == HorizontalDirection.Right ? MagdolliteSegment.RightSubmerge : MagdolliteSegment.LeftSubmerge);
        state.Action = MagdolliteHeadAction.WaitingForAttackAnimation;
    }

    /// <summary>Releases the arm to rise once the head has submerged, and waits for it to land.</summary>
    private void ReleaseMagdolliteArm(
        RoomEnemyPlace head,
        MagdolliteHeadState state)
    {
        (_, RoomEnemyPlace body, _, MagdolliteStates parts) = RequireMagdolliteComposite(head);
        parts.Body.Release();
        state.Action = MagdolliteHeadAction.WaitingForBodyLanding;
    }

    /// <summary>Starts the Samus-facing emerge animation once the arm has landed.</summary>
    private static void EmergeMagdolliteHead(
        RoomEnemyPlace head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        StartMagdolliteAnimation(head, state, SamusSide(samus, head) == HorizontalDirection.Right ? MagdolliteSegment.RightEmerge : MagdolliteSegment.LeftEmerge);
        state.Action = MagdolliteHeadAction.WaitingForReturnAnimation;
    }

    /// <summary>Restores the appropriate idle animation once the head has surfaced.</summary>
    private static void IdleMagdolliteHead(
        RoomEnemyPlace head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        StartMagdolliteAnimation(head, state, SamusSide(samus, head) == HorizontalDirection.Right ? MagdolliteSegment.RightIdle : MagdolliteSegment.LeftIdle);
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
    private void RunMagdolliteBodyRising(
        RoomEnemyPlace body,
        MagdolliteBodyState state,
        SamusState samus)
    {
        // The arm rises at the speed its head's spawn settings name (#2474).
        (_, _, _, MagdolliteStates parts) = RequireMagdolliteComposite(body);
        state.RiseDisplacementY += AddMagdolliteY(body, -parts.Head.Settings.Speed);

        int upwardDistance = -state.RiseDisplacementY;
        MagdollitePhaseDefinition phase = state.BodyPhase.Phase;
        bool roseMaximum = upwardDistance >= MagdollitePhaseDefinitions.MaximumRise;
        bool rosePastSamus = body.Position.Y.Whole - phase.OverlayYOffset < samus.Position.Y.Whole;
        // At full height whether the arm grows its last segment depends on the place it occupies
        // (MagdolliteApexGrowth); the Samus test compares with the phase's own threshold.
        bool growsAPhase = roseMaximum
            ? _room.Session.Overreads.MagdolliteApexGrowth.GrowsFinalSegment(body.Ordinal)
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
    private void RunMagdolliteBodyFalling(
        RoomEnemyPlace body,
        MagdolliteBodyState state)
    {
        (RoomEnemyPlace head, _, _, MagdolliteStates parts) = RequireMagdolliteComposite(body);
        MagdolliteHeadState headState = parts.Head;
        state.RiseDisplacementY += AddMagdolliteY(body, headState.Settings.Speed);

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
    private void RunMagdolliteOverlayWaiting(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state)
    {
        (RoomEnemyPlace head, _, _, MagdolliteStates parts) = RequireMagdolliteComposite(overlay);
        if (parts.Head.IsSubmerging)
            state.Action = MagdolliteOverlayAction.TrackingRisingBody;
    }

    /// <summary>Once the throw has finished, restores the overlay, starts the body's descent and holds the head radius.</summary>
    private void FinishMagdolliteOverlayThrow(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state)
    {
        (_, RoomEnemyPlace body, _, MagdolliteStates parts) = RequireMagdolliteComposite(overlay);
        MagdolliteBodyState bodyState = parts.Body;
        StartMagdolliteAnimation(overlay, state, MagdolliteSegment.PillarCap);
        bodyState.Action = MagdolliteBodyAction.Falling;
        state.Action = MagdolliteOverlayAction.TrackingFallingBody;
        overlay.Position = overlay.Position.WithWhole(state.ThrowOrigin);
        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Positions the overlay over each rising body phase, then starts its Samus-facing throw at the apex.</summary>
    private void RunMagdolliteOverlayTrackingRise(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state,
        SamusState samus)
    {
        (_, RoomEnemyPlace body, _, MagdolliteStates parts) = RequireMagdolliteComposite(overlay);
        MagdolliteBodyState bodyState = parts.Body;
        if (bodyState.Action == MagdolliteBodyAction.HoldingAtApex)
        {
            state.ThrowDirection = SamusSide(samus, overlay);
            StartMagdolliteAnimation(overlay, state, state.ThrowDirection == HorizontalDirection.Right ? MagdolliteSegment.RightThrow : MagdolliteSegment.LeftThrow);
            state.Action = MagdolliteOverlayAction.WaitingForThrowAnimation;
            state.ThrowOrigin = overlay.Position.Whole;
        }
        else
        {
            int offset = bodyState.BodyPhase.Phase.OverlayYOffset;
            overlay.Position = overlay.Position.WithWholeY(body.Position.Y.Whole - offset);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Follows the descending body until the head returns to idle, while maintaining its collision radius.</summary>
    private void RunMagdolliteOverlayTrackingFall(
        RoomEnemyPlace overlay,
        MagdolliteOverlayState state)
    {
        (RoomEnemyPlace head, RoomEnemyPlace body, _, MagdolliteStates parts) = RequireMagdolliteComposite(overlay);
        MagdolliteHeadState headState = parts.Head;
        MagdolliteBodyState bodyState = parts.Body;
        if (headState.Action == MagdolliteHeadAction.Waiting)
        {
            state.Action = MagdolliteOverlayAction.WaitingForAttack;
        }
        else
        {
            int offset = bodyState.BodyPhase.Phase.OverlayYOffset;
            overlay.Position = overlay.Position.WithWholeY(body.Position.Y.Whole - offset);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>
    /// Stretches the head's collision height down to the tracking overlay (#1823): the head's vertical radius becomes
    /// the distance from the overlay up to the head plus its reach past the overlay, and never less than its smallest
    /// radius, so the head's hitbox
    /// reaches the overlay as the arm grows beneath it.
    /// </summary>
    private void UpdateMagdolliteHeadRadius(RoomEnemyPlace overlay)
    {
        (RoomEnemyPlace head, _, _, _) = RequireMagdolliteComposite(overlay);
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

    /// <summary>Starts <paramref name="segment"/>'s animation only when it differs from the one the part plays (#2145).</summary>
    private static void StartMagdolliteAnimation(
        RoomEnemyPlace place,
        MagdollitePartState state,
        MagdolliteSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, MagdolliteAnimations.StartingWith(segment));

    /// <summary>The head, body and overlay, listed in that order, that a part found when it was placed.</summary>
    private MagdolliteComposite RequireMagdolliteComposite(RoomEnemyPlace member) => _partners.Of(member).Composite;

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
    private void ShowGlow()
    {
        PictureShading glow = Glow;
        foreach (RoomEnemyPlace part in _partners.Placed)
        {
            // A place whose part has left it, and any enemy that took the place since, shows no glow.
            if (part.Entity is MagdollitePartEntity)
                part.Shading = glow;
        }
        foreach (EnemyProjectile lava in _projectiles.EnemyProjectiles)
        {
            if (lava.Kind == RoomEnemyProjectileKind.LavaThrownByMagdollite)
                lava.Graphics = lava.Graphics.In(glow);
        }
    }

    /// <summary>
    /// Carries out one cue of a Magdollite part's animations: a sound heard while the part is in view, a move of the part or
    /// of the head with the body's pillar, a shift from the throw's origin, or a signal (<see cref="MagdolliteCueKind"/>).
    /// Every cue runs on to the next step, so none redirects.
    /// </summary>
    private void ApplyMagdolliteCue(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        MagdollitePartState state,
        MagdolliteCue cue,
        Position16<RoomPixels> camera)
    {
        switch (cue)
        {
            case MagdolliteCue.PlaySoundIfOnScreen playSound:
            {
                if (EnemyView.Contains(place.Position.Whole, camera))
                    _audio.QueueEnemySound(playSound.Sound, priority: SoundRequestPriority.Normal);
                break;
            }
            case MagdolliteCue.Move move:
                place.Position += move.Offset;
                break;
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.FinishSubmerge } when state is MagdolliteHeadState head:
                head.FinishSubmerge();
                break;
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.FinishEmerge } when state is MagdolliteHeadState head:
                head.FinishEmerge();
                break;
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.FinishThrow } when state is MagdolliteOverlayState overlayState:
                overlayState.FinishThrow();
                break;
            case MagdolliteCue.MoveWithPillar moveWithPillar:
            {
                // Only the head's submerge and emerge raise it, moving the head with the body's pillar.
                (_, RoomEnemyPlace body, _, _) = RequireMagdolliteComposite(place);
                place.Position += moveWithPillar.Offset;
                body.Position += moveWithPillar.Offset;
                break;
            }
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.SinkAndShowParts }:
            {
                (_, RoomEnemyPlace body, RoomEnemyPlace overlay, _) = RequireMagdolliteComposite(place);
                int sunkY = state.OriginY + MagdolliteMotionDefinitions.SunkDepth;
                place.Position = place.Position.WithWholeY(sunkY);
                body.Position = body.Position.WithWholeY(sunkY);
                body.Presence = body.Presence.Without(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.Without(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.RestoreOrigins }:
            {
                (_, RoomEnemyPlace body, _, _) = RequireMagdolliteComposite(place);
                place.Position = place.Position.WithWholeY(state.OriginY);
                body.Position = body.Position.WithWholeY(state.OriginY);
                break;
            }
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.SurfaceAndHideParts }:
            {
                (_, RoomEnemyPlace body, RoomEnemyPlace overlay, _) = RequireMagdolliteComposite(place);
                int surfacedY = state.OriginY + MagdolliteMotionDefinitions.SurfacedDepth;
                place.Position = place.Position.WithWholeY(surfacedY);
                body.Position = body.Position.WithWholeY(surfacedY);
                body.Presence = body.Presence.With(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.With(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.SpawnLava } when state is MagdolliteOverlayState magdolliteOverlayState:
                SpawnMagdolliteLava(occupant, place, magdolliteOverlayState.ThrowDirection);
                break;
            case MagdolliteCue.ShiftFromThrowOrigin shift when state is MagdolliteOverlayState magdolliteOverlayState:
                place.Position = place.Position.WithWhole(magdolliteOverlayState.ThrowOrigin + shift.Offset);
                break;
            case MagdolliteCue.Signal { Kind: MagdolliteCueKind.StartCooldown } when state is MagdolliteOverlayState magdolliteOverlayState:
                magdolliteOverlayState.AttackCooldown = UpdateCountdown.Lasting(MagdolliteMotionDefinitions.AttackCooldown);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteCue)} {cue}.");
        }
    }

    /// <summary>
    /// Passes a struck part's fate to the two enemies listed after it, once any hit's ordinary damage is done: a part
    /// killed deletes them, and a frozen part freezes them for as long.
    /// </summary>
    private void ResolveMagdolliteCombatAfterCommon(RoomEnemyPlace current)
    {
        // The follow-up reaches the two enemies listed after the part that was hit (FollowUpAfterStruckPart): a power
        // bomb also kills the rising body and the tracking overlay, whose follow-ups reach the enemies listed after the
        // Magdollite.
        IReadOnlyList<RoomEnemyPlace> recipients = _partners.Of(current).FollowUpRecipients;

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
            following.AiStates = following.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>Magdollite's reaction to a bomb: after its ordinary damage, the struck part's fate passes to the enemies listed after it.</summary>
    internal EnemyBombReaction MagdolliteBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveMagdolliteCombatAfterCommon(c.Enemy),
    };

    /// <summary>Magdollite's thrown lava (#1760).</summary>
    private LavaBehavior Lava => ThrownLava;

    /// <summary>The thrown lava's behaviour, which owns its animations' cues.</summary>
    private LavaBehavior ThrownLava => field ??= new LavaBehavior(_deathsAndDrops);

    /// <summary>
    /// Thrown lava: it flies sideways until it leaves the view, handles its animations' cues, and a shot ball runs its
    /// typed shot animation, the animation its definition names.
    /// </summary>
    /// <param name="deathsAndDrops">The drops a shot ball leaves.</param>
    private sealed class LavaBehavior(EnemyDeathsAndDrops deathsAndDrops) : SingleMotionProjectileBehavior(
        context => FlyMagdolliteLava(context.Projectile, context.Camera)), IEnemyProjectileCues<MagdolliteLavaCue>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new MagdolliteLavaAnimation(MagdolliteLavaSegment.Shot, this);

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
                        EnemyDefinitionId.Magdollite,
                        context.Samus);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MagdolliteLavaCueKind)} {cue.Kind}.");
            }
        }
    }

    /// <summary>Advances the typed animation of the Magdollite part whose state is <paramref name="state"/>.</summary>
    internal void AdvanceMagdolliteTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, MagdollitePartState state, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, state, MagdolliteAnimations.Segments,
            static (MagdollitePartState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyMagdolliteCue(occupant, place, state, cue, context.Camera);
                return null;
            });

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
        _collision.ResolveOrdinaryTouch(context);
        ResolveMagdolliteCombatAfterCommon(context.Place);
    }

    /// <summary>The side of <paramref name="part"/> Samus is on: right unless she is to its left (#2338).</summary>
    private static HorizontalDirection SamusSide(SamusState samus, RoomEnemyPlace part) =>
        samus.Position.X.Whole < part.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right;

    /// <summary>Spawns and initializes a Magdollite lava ball thrown <paramref name="thrown"/>.</summary>
    private void SpawnMagdolliteLava(RoomEnemyOccupant occupant, RoomEnemyPlace source, HorizontalDirection thrown)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.LavaThrownByMagdollite, Lava, SpriteGraphics.SpawnedBy(occupant, source.Shading));
        if (projectile is null)
            return;


        // The lava starts where the throwing overlay stands, with its fractions of a pixel, a drop below it, and flies
        // sideways the way the overlay faces.
        projectile.Position = source.Position.WithWholeY(source.Position.Y.Whole + MagdolliteMotionDefinitions.LavaDrop);
        projectile.Velocity = new(
            MagdolliteMotionDefinitions.LavaThrowSpeed.Toward(thrown),
            SignedEightEight.Zero);
        projectile.InstallAnimation(new MagdolliteLavaAnimation(Lava, thrown == HorizontalDirection.Right ? MagdolliteLavaSegment.Right : MagdolliteLavaSegment.Left, ThrownLava));
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
