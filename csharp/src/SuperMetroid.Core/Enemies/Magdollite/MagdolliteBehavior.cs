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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MagdolliteEntity(this, slot, common);

    /// <summary>Supplies the room's apex overreads.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the lava shots emitted by the overlay's animation.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each Magdollite part finds the others when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The partners each Magdollite part found when it was placed.</summary>
    private readonly EnemyPartLinks<MagdollitePartners> _partners = new();
    /// <summary>Runs common contact damage before the species reaction.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Creates the item drop when a thrown lava projectile is destroyed.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the Magdollite AI, projectile, combat and glow services.</summary>
    internal MagdolliteBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
        _slotTable = slotTable;
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
    internal void InitializeMagdollite(RoomEnemySlot slot, SamusState samus)
    {
        MagdollitePart part = slot.SpawnSettings<MagdolliteSpawnSettings>().Part;
        // The room lists each Magdollite's head, body and overlay in a row; each part finds the others, the enemy
        // listed after it and the two its follow-up to a hit reaches.
        RoomEnemySlot head = part == MagdollitePart.Head ? slot : _slotTable.ListedBefore(slot, SlotsAfterHead(part));
        _partners.Place(slot, new MagdollitePartners(
            Composite: new(head, _slotTable.ListedAfter(head, 1), _slotTable.ListedAfter(head, 2)),
            Next: _slotTable.ListedAfterOrNone(slot, 1),
            FollowUpRecipients: FollowUpAfterStruckPart.Recipients(distance => _slotTable.ListedAfterOrNone(slot, distance), reach: 2)));
        switch (part)
        {
            case MagdollitePart.Head:
                InitializeMagdolliteHead(slot, samus);
                break;
            case MagdollitePart.RisingBody:
                InitializeMagdolliteBody(slot);
                break;
            case MagdollitePart.TrackingOverlay:
                InitializeMagdolliteOverlay(slot);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdollitePart)} {part}.");
        }

        // The room has one lava glow, not one per part: each part's initialization restarts it, so the last part placed
        // sets its phase.
        RestartGlow();
    }

    /// <summary>How the Magdollite head in <paramref name="head"/> rises, from its spawn settings.</summary>
    /// <exception cref="InvalidDataException">The head's placement names no rise.</exception>
    private static MagdolliteHeadRise HeadRise(RoomEnemySlot head) =>
        head.SpawnSettings<MagdolliteSpawnSettings>().Rise
            ?? throw new InvalidDataException($"The Magdollite head in enemy slot {head.SlotIndex} names no rise.");

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
        RoomEnemySlot slot,
        SamusState samus)
    {
        // The head's spawn settings name the speed it and its arm rise and sink at; the arm reads the head's.
        var state = new MagdolliteHeadState(MagdolliteHeadAction.Waiting) { Speed = HeadRise(slot).Speed };
        slot.SetSpeciesState<MagdollitePartState>(state);
        state.InstalledAnimation = null;
        state.HeadWaitsForBodyLanding = false;
        state.OriginY = slot.Position.Y.Whole;

        bool samusIsLeft = (samus.Position.Whole - slot.Position.Whole).X < 0;
        state.DesiredAnimation = samusIsLeft
            ? MagdolliteSegment.LeftIdle
            : MagdolliteSegment.RightIdle;
        StartMagdolliteAnimation(slot, state);
    }

    /// <summary>Hides the rising body 32 pixels below its origin until the head triggers an attack.</summary>
    private static void InitializeMagdolliteBody(RoomEnemySlot slot)
    {
        var state = new MagdolliteBodyState(MagdolliteBodyAction.Dormant);
        slot.SetSpeciesState<MagdollitePartState>(state);
        state.OriginY = slot.Position.Y.Whole;
        state.InstalledAnimation = null;
        state.BodyReachedApex = false;
        state.BodyWakeRequested = true;
        state.DesiredAnimation = MagdolliteRiseStage.Setup.Phase.BodyAnimation;
        StartMagdolliteAnimation(slot, state);
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Hides the projectile-throwing overlay below its origin and starts its pillar-cap animation.</summary>
    private static void InitializeMagdolliteOverlay(RoomEnemySlot slot)
    {
        var state = new MagdolliteOverlayState(MagdolliteOverlayAction.WaitingForAttack);
        slot.SetSpeciesState<MagdollitePartState>(state);
        state.OriginY = slot.Position.Y.Whole;
        state.InstalledAnimation = null;
        state.AttackTimer = WrappingCountdown.Start(0);
        state.DesiredAnimation = MagdolliteSegment.PillarCap;
        StartMagdolliteAnimation(slot, state);
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + MagdolliteMotionDefinitions.HiddenDepth);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs the Magdollite part's main AI: the head's, the body's or the overlay's actions.</summary>
    private void RunMagdolliteMain(
        RoomEnemySlot slot,
        SamusState samus)
    {
        switch (slot.RequireSpeciesState<MagdollitePartState>())
        {
            case MagdolliteHeadState head:
                RunMagdolliteHead(slot, head, samus);
                return;
            case MagdolliteBodyState body:
                RunMagdolliteBody(slot, body, samus);
                return;
            case MagdolliteOverlayState overlay:
                RunMagdolliteOverlay(slot, overlay, samus);
                return;
            case var other:
                throw new InvalidDataException($"Magdollite slot {slot.SlotIndex} holds an unknown part {other.Part}.");
        }
    }

    /// <summary>Runs the head's current action.</summary>
    private void RunMagdolliteHead(RoomEnemySlot slot, MagdolliteHeadState state, SamusState samus)
    {
        switch (state.Action)
        {
            case MagdolliteHeadAction.Waiting:
                RunMagdolliteHeadWaiting(slot, state, samus);
                return;
            case MagdolliteHeadAction.WaitingForAttackAnimation:
                RunMagdolliteHeadWaitingForAttackAnimation(slot, state);
                return;
            case MagdolliteHeadAction.WaitingForBodyLanding:
                RunMagdolliteHeadWaitingForBodyLanding(slot, state, samus);
                return;
            case MagdolliteHeadAction.WaitingForReturnAnimation:
                RunMagdolliteHeadWaitingForReturnAnimation(slot, state, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite head has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the rising body's current action.</summary>
    private void RunMagdolliteBody(RoomEnemySlot slot, MagdolliteBodyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case MagdolliteBodyAction.Dormant:
                RunMagdolliteBodyDormant(slot, state);
                return;
            case MagdolliteBodyAction.Rising:
                RunMagdolliteBodyRising(slot, state, samus);
                return;
            case MagdolliteBodyAction.HoldingAtApex:
                return;
            case MagdolliteBodyAction.Falling:
                RunMagdolliteBodyFalling(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite body has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Counts down the attack cooldown and runs the tracking overlay's current action.</summary>
    private void RunMagdolliteOverlay(RoomEnemySlot slot, MagdolliteOverlayState state, SamusState samus)
    {
        state.AttackTimer = state.AttackTimer.Decrement().Remaining;
        switch (state.Action)
        {
            case MagdolliteOverlayAction.WaitingForAttack:
                RunMagdolliteOverlayWaiting(slot, state);
                return;
            case MagdolliteOverlayAction.WaitingForThrowAnimation:
                RunMagdolliteOverlayAnimation(slot, state);
                return;
            case MagdolliteOverlayAction.TrackingRisingBody:
                RunMagdolliteOverlayTrackingRise(slot, state, samus);
                return;
            case MagdolliteOverlayAction.TrackingFallingBody:
                RunMagdolliteOverlayTrackingFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Magdollite overlay has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>The part state of <paramref name="slot"/>, which must be the <typeparamref name="TPart"/> its composite places there.</summary>
    private static TPart PartState<TPart>(RoomEnemySlot slot) where TPart : MagdollitePartState =>
        slot.RequireSpeciesState<MagdollitePartState>() as TPart
            ?? throw new InvalidDataException($"Magdollite slot {slot.SlotIndex} does not hold its composite's {typeof(TPart).Name}.");

    /// <summary>Tracks Samus while idle and begins submerging when the overlay cooldown expires within the head's horizontal range.</summary>
    private void RunMagdolliteHeadWaiting(
        RoomEnemySlot head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        bool samusIsRight = !CoordinateAxis.Precedes(samus.Position.X.Whole, head.Position.X.Whole);
        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightIdle
            : MagdolliteSegment.LeftIdle;
        StartMagdolliteAnimation(head, state);

        (_, _, RoomEnemySlot overlay) = RequireMagdolliteComposite(head);
        MagdolliteOverlayState overlayState = PartState<MagdolliteOverlayState>(overlay);
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
        state.Action = MagdolliteHeadAction.WaitingForAttackAnimation;
    }

    /// <summary>Wakes the adjacent body only after the head's submerge animation has cleared its busy flag.</summary>
    private void RunMagdolliteHeadWaitingForAttackAnimation(
        RoomEnemySlot head,
        MagdolliteHeadState state)
    {
        if (state.AnimationBusy)
            return;

        (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(head);
        state.HeadWaitsForBodyLanding = true;
        PartState<MagdolliteBodyState>(body).BodyWakeRequested = false;
        state.Action = MagdolliteHeadAction.WaitingForBodyLanding;
    }

    /// <summary>Starts the Samus-facing emerge animation once the falling body releases the head's landing latch.</summary>
    private static void RunMagdolliteHeadWaitingForBodyLanding(
        RoomEnemySlot head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        if (state.HeadWaitsForBodyLanding)
            return;

        bool samusIsRight = !CoordinateAxis.Precedes(samus.Position.X.Whole, head.Position.X.Whole);
        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightEmerge
            : MagdolliteSegment.LeftEmerge;
        StartMagdolliteAnimation(head, state);
        state.Action = MagdolliteHeadAction.WaitingForReturnAnimation;
    }

    /// <summary>Restores the appropriate idle animation after the emerge animation finishes.</summary>
    private static void RunMagdolliteHeadWaitingForReturnAnimation(
        RoomEnemySlot head,
        MagdolliteHeadState state,
        SamusState samus)
    {
        if (state.AnimationBusy)
            return;

        bool samusIsRight = !CoordinateAxis.Precedes(samus.Position.X.Whole, head.Position.X.Whole);
        state.DesiredAnimation = samusIsRight
            ? MagdolliteSegment.RightIdle
            : MagdolliteSegment.LeftIdle;
        StartMagdolliteAnimation(head, state);
        state.Action = MagdolliteHeadAction.Waiting;
    }

    /// <summary>Returns the hidden body to its origin and enters rising once the head clears its wake latch.</summary>
    private static void RunMagdolliteBodyDormant(
        RoomEnemySlot body,
        MagdolliteBodyState state)
    {
        if (state.BodyWakeRequested)
            return;

        state.Action = MagdolliteBodyAction.Rising;
        state.RiseDisplacementY = 0;
        state.BodyWakeRequested = false;
        state.BodyPhase = MagdolliteRiseStage.FirstPillar;
        body.Position = body.Position.WithWholeY(state.OriginY);
    }

    /// <summary>Rises using the preceding head's velocity, advances growth phases at their thresholds, and stops at the apex or above Samus.</summary>
    internal void RunMagdolliteBodyRising(
        RoomEnemySlot body,
        MagdolliteBodyState state,
        SamusState samus)
    {
        // The arm rises at its head's speed.
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(body);
        MagdolliteHeadState speedOwner = PartState<MagdolliteHeadState>(head);
        state.RiseDisplacementY += AddMagdolliteY(body, -speedOwner.Speed);

        int upwardDistance = -state.RiseDisplacementY;
        MagdollitePhaseDefinition phase = state.BodyPhase.Phase;
        bool roseMaximum = !CoordinateAxis.Precedes(upwardDistance, MagdollitePhaseDefinitions.MaximumRise);
        bool rosePastSamus = CoordinateAxis.Precedes(body.Position.Y.Whole - phase.OverlayYOffset, samus.Position.Y.Whole);
        // At full height whether the arm grows its last segment depends on the slot it occupies
        // (MagdolliteApexGrowth); the Samus test compares with the phase's own threshold.
        bool growsAPhase = roseMaximum
            ? _room.Session.Overreads.MagdolliteApexGrowth.GrowsFinalSegment(body.SlotIndex)
            : !CoordinateAxis.Precedes(upwardDistance, phase.DistanceThreshold);

        if (roseMaximum || rosePastSamus)
        {
            state.Action = MagdolliteBodyAction.HoldingAtApex;
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
        RoomEnemySlot body,
        MagdolliteBodyState state)
    {
        (RoomEnemySlot speedHead, _, _) = RequireMagdolliteComposite(body);
        MagdolliteHeadState speedOwner = PartState<MagdolliteHeadState>(speedHead);
        state.RiseDisplacementY += AddMagdolliteY(body, speedOwner.Speed);

        if (state.RiseDisplacementY >= 0)
        {
            state.BodyWakeRequested = true;
            (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(body);
            PartState<MagdolliteHeadState>(head).HeadWaitsForBodyLanding = false;
            state.Action = MagdolliteBodyAction.Dormant;
            return;
        }

        int downwardDistanceRemaining = -state.RiseDisplacementY;
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
        RoomEnemySlot overlay,
        MagdolliteOverlayState state)
    {
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(overlay);
        if (PartState<MagdolliteHeadState>(head).Action == MagdolliteHeadAction.WaitingForAttackAnimation)
            state.Action = MagdolliteOverlayAction.TrackingRisingBody;
    }

    /// <summary>Holds the head radius while throwing, then restores the overlay and starts the body's descent.</summary>
    private void RunMagdolliteOverlayAnimation(
        RoomEnemySlot overlay,
        MagdolliteOverlayState state)
    {
        if (!state.AnimationBusy)
        {
            (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
            MagdolliteBodyState bodyState = PartState<MagdolliteBodyState>(body);
            state.DesiredAnimation = MagdolliteSegment.PillarCap;
            StartMagdolliteAnimation(overlay, state);
            bodyState.Action = MagdolliteBodyAction.Falling;
            state.Action = MagdolliteOverlayAction.TrackingFallingBody;
            bodyState.BodyReachedApex = false;
            overlay.Position = overlay.Position.WithWhole(state.ThrowOrigin);
        }

        UpdateMagdolliteHeadRadius(overlay);
    }

    /// <summary>Positions the overlay over each rising body phase, then starts its Samus-facing throw at the apex.</summary>
    internal void RunMagdolliteOverlayTrackingRise(
        RoomEnemySlot overlay,
        MagdolliteOverlayState state,
        SamusState samus)
    {
        (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteBodyState bodyState = PartState<MagdolliteBodyState>(body);
        if (bodyState.BodyReachedApex)
        {
            bool samusIsRight = !CoordinateAxis.Precedes(samus.Position.X.Whole, overlay.Position.X.Whole);
            state.DesiredAnimation = samusIsRight
                ? MagdolliteSegment.RightThrow
                : MagdolliteSegment.LeftThrow;
            state.ThrowsRight = samusIsRight;
            StartMagdolliteAnimation(overlay, state);
            state.Action = MagdolliteOverlayAction.WaitingForThrowAnimation;
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
        RoomEnemySlot overlay,
        MagdolliteOverlayState state)
    {
        (RoomEnemySlot head, RoomEnemySlot body, _) = RequireMagdolliteComposite(overlay);
        MagdolliteHeadState headState = PartState<MagdolliteHeadState>(head);
        MagdolliteBodyState bodyState = PartState<MagdolliteBodyState>(body);
        if (headState.Action == MagdolliteHeadAction.Waiting)
        {
            state.Action = MagdolliteOverlayAction.WaitingForAttack;
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
    /// the distance from the overlay up to the head plus its reach past the overlay, and never less than its smallest
    /// radius, so the head's hitbox
    /// reaches the overlay as the arm grows beneath it.
    /// </summary>
    private void UpdateMagdolliteHeadRadius(RoomEnemySlot overlay)
    {
        (RoomEnemySlot head, _, _) = RequireMagdolliteComposite(overlay);
        int radius = CoordinateAxis.Wrap(head.Position.Y.Whole - overlay.Position.Y.Whole + MagdolliteMotionDefinitions.HeadReachPastOverlay);
        int smallest = MagdolliteMotionDefinitions.SmallestHeadRadius;
        head.Radius = head.Radius.WithY(CoordinateAxis.Precedes(radius, smallest) ? smallest : radius);
    }

    /// <summary>
    /// Adds a signed 1/65536-pixel Y velocity, carrying the fraction into the whole, and returns the whole-pixel vertical
    /// move it made, by the wrapped difference.
    /// </summary>
    private static int AddMagdolliteY(
        RoomEnemySlot slot,
        SignedSixteenSixteen velocity)
    {
        int before = slot.Position.Y.Whole;
        slot.Position = slot.Position.AdvanceY(velocity);
        return CoordinateAxis.Difference(slot.Position.Y.Whole, before);
    }

    /// <summary>Restarts the animation only when the desired segment differs from the installed one.</summary>
    private static void StartMagdolliteAnimation(
        RoomEnemySlot slot,
        MagdollitePartState state)
    {
        if (state.DesiredAnimation == state.InstalledAnimation)
            return;
        state.Playback = new(MagdolliteAnimations.StartingWith(state.DesiredAnimation), AnimationPlace.StartOfAnimation);
        state.InstalledAnimation = state.DesiredAnimation;
        slot.AnimationClock.Restart();
    }

    /// <summary>Validates the retail-ordered head, body and overlay any one component found when it was placed.</summary>
    private MagdolliteComposite RequireMagdolliteComposite(RoomEnemySlot member)
    {
        (RoomEnemySlot head, RoomEnemySlot body, RoomEnemySlot overlay) = _partners.Of(member).Composite;
        if (head.DefinitionId != EnemyDefinitionId.Magdollite || head.SpawnSettings<MagdolliteSpawnSettings>().Part != MagdollitePart.Head ||
            body.DefinitionId != EnemyDefinitionId.Magdollite || body.SpawnSettings<MagdolliteSpawnSettings>().Part != MagdollitePart.RisingBody ||
            overlay.DefinitionId != EnemyDefinitionId.Magdollite || overlay.SpawnSettings<MagdolliteSpawnSettings>().Part != MagdollitePart.TrackingOverlay)
        {
            throw new InvalidDataException(
                $"Magdollite slot {member.SlotIndex} is not inside a retail-ordered head, body and overlay composite.");
        }
        return new(head, body, overlay);
    }

    /// <summary>How many places after its composite's head the room lists a part: the head, then the body, then the overlay.</summary>
    private static int SlotsAfterHead(MagdollitePart part) => part switch
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
        foreach (RoomEnemySlot part in _slotTable.Slots)
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
        RoomEnemySlot slot,
        MagdolliteCue cue,
        Position16<RoomPixels> camera)
    {
        MagdollitePartState state = slot.RequireSpeciesState<MagdollitePartState>();
        switch (cue.Kind)
        {
            case MagdolliteCueKind.PlaySoundIfOnScreen:
            {
                if (EnemyView.Contains(slot.Position.Whole, camera))
                    _audio.QueueEnemySound(cue.Sound ?? throw new InvalidDataException("A Magdollite sound cue carries no sound."), priority: SoundRequestPriority.Normal);
                break;
            }
            case MagdolliteCueKind.MoveDown2:
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 2);
                break;
            case MagdolliteCueKind.MoveUp2:
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 2);
                break;
            case MagdolliteCueKind.SetAnimationBusy:
                state.AnimationBusy = true;
                break;
            case MagdolliteCueKind.ClearAnimationBusy:
                state.AnimationBusy = false;
                break;
            case MagdolliteCueKind.MoveWithPillarUp1:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole - 1);
                next.Position = next.Position.WithWrappedWholeY(next.Position.Y.Whole - 1);
                break;
            }
            case MagdolliteCueKind.MoveWithPillarDown1:
            {
                RoomEnemySlot next = NextMagdolliteSlot(slot);
                slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 1);
                next.Position = next.Position.WithWrappedWholeY(next.Position.Y.Whole + 1);
                break;
            }
            case MagdolliteCueKind.SinkAndShowParts:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                int sunkY = CoordinateAxis.Wrap(state.OriginY + MagdolliteMotionDefinitions.SunkDepth);
                slot.Position = slot.Position.WithWholeY(sunkY);
                body.Position = body.Position.WithWholeY(sunkY);
                body.Presence = body.Presence.Without(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.Without(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteCueKind.RestoreOrigins:
            {
                (_, RoomEnemySlot body, _) = RequireMagdolliteComposite(slot);
                slot.Position = slot.Position.WithWholeY(state.OriginY);
                body.Position = body.Position.WithWholeY(state.OriginY);
                break;
            }
            case MagdolliteCueKind.SurfaceAndHideParts:
            {
                (_, RoomEnemySlot body, RoomEnemySlot overlay) = RequireMagdolliteComposite(slot);
                int surfacedY = CoordinateAxis.Wrap(state.OriginY + MagdolliteMotionDefinitions.SurfacedDepth);
                slot.Position = slot.Position.WithWholeY(surfacedY);
                body.Position = body.Position.WithWholeY(surfacedY);
                body.Presence = body.Presence.With(EnemyPresence.Hidden);
                overlay.Presence = overlay.Presence.With(EnemyPresence.Hidden);
                break;
            }
            case MagdolliteCueKind.SpawnLava:
                SpawnMagdolliteLava(slot, PartState<MagdolliteOverlayState>(slot).ThrowsRight);
                break;
            case MagdolliteCueKind.ShiftRightUp4:
                slot.Position = slot.Position.WithWhole(PartState<MagdolliteOverlayState>(slot).ThrowOrigin + new Offset16<RoomPixels>(8, -4));
                break;
            case MagdolliteCueKind.ShiftLeftUp4:
                slot.Position = slot.Position.WithWhole(PartState<MagdolliteOverlayState>(slot).ThrowOrigin + new Offset16<RoomPixels>(-8, -4));
                break;
            case MagdolliteCueKind.ShiftRightUp8:
                slot.Position = slot.Position.WithWhole(PartState<MagdolliteOverlayState>(slot).ThrowOrigin + new Offset16<RoomPixels>(8, -8));
                break;
            case MagdolliteCueKind.StartCooldown:
                PartState<MagdolliteOverlayState>(slot).AttackTimer = WrappingCountdown.Start(MagdolliteMotionDefinitions.AttackCooldown);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MagdolliteCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>The enemy listed after the part, which it found when it was placed, used by the paired pillar cues.</summary>
    private RoomEnemySlot NextMagdolliteSlot(RoomEnemySlot slot) =>
        _partners.Of(slot).Next ?? throw new InvalidDataException($"Magdollite slot {slot.SlotIndex} has no following component.");

    /// <summary>Runs the shared post-combat tail.</summary>
    private void ResolveMagdolliteCombatAfterCommon(RoomEnemySlot current)
    {
        // The follow-up reaches the two enemies listed after the part that was hit (FollowUpAfterStruckPart): a power
        // bomb also kills the rising body and the tracking overlay, whose follow-ups reach the enemies listed after the
        // Magdollite.
        IReadOnlyList<RoomEnemySlot?> recipients = _partners.Of(current).FollowUpRecipients;
        RoomEnemySlot? following = recipients[0];
        RoomEnemySlot? secondFollowing = recipients[1];

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
    internal void RunMagdolliteInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeMagdollite(slot, surroundings.IntroducedSamus);
    }

    /// <summary>Runs Magdollite's main AI.</summary>
    internal void RunMagdolliteMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMagdolliteMain(slot, frame.Samus);
    }

    /// <summary>Advances Magdollite's typed animation.</summary>
    internal void AdvanceMagdolliteTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(slot, MagdolliteAnimations.Segments,
            static (MagdollitePartState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyMagdolliteCue(slot, cue, context.Camera) is { } target
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
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
        ResolveMagdolliteCombatAfterCommon(context.Slot);
    }
    /// <summary>Sideways speed of thrown lava, three pixels per update, negated for a leftward throw.</summary>
    private static readonly SignedEightEight MagdolliteLavaHorizontalSpeed = SignedEightEight.FromParts(3, 0);

    /// <summary>Spawns and initializes a Magdollite lava ball.</summary>
    internal void SpawnMagdolliteLava(RoomEnemySlot source, bool throwsRight)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.LavaThrownByMagdollite,
            Lava,
            SpriteGraphics.SpawnedBy(source));

        // The initializer copies both subpixels from the throwing overlay, starts the lava's drop below it, and throws the
        // lava sideways in the overlay's facing.
        projectile.Position = new(new(source.Position.X.Whole, source.Position.X.Fraction), new(CoordinateAxis.Wrap(source.Position.Y.Whole + MagdolliteMotionDefinitions.LavaDrop), source.Position.Y.Fraction));
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

        // The lava leaves once past the view after its horizontal move, the view's right and bottom edges already
        // outside. The lava never moves vertically.
        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }
}
