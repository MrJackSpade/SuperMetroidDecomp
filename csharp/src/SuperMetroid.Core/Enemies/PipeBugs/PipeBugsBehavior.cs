using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Compatibility;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The pipe bugs species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PipeBugsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PipeBugsPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PipeBugsEntity(this, slot, common);

    /// <summary>The room's enemy places, where each Norfair formation leader finds its formation when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>
    /// The five-bug formation each Norfair leader found when it was placed: itself, then the four bugs its room lists
    /// right after it, each in its role (#1840).
    /// </summary>
    private readonly EnemyPartLinks<NorfairPipeBugFormation> _formations = new();
    /// <summary>
    /// The places set up as Norfair Pipe Bugs this room. A formation member killed and not yet respawned holds no state,
    /// and its place is still the formation's.
    /// </summary>
    private readonly HashSet<RoomEnemySlot> _norfairPlaces = [];

    /// <summary>Binds the shared enemy pool needed to validate Norfair formation membership.</summary>
    internal PipeBugsBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Pixels a Brinstar pipe bug rises above its stored spawn Y before flying.</summary>
    private const int BrinstarPipeBugEmergenceHeight = 16;
    /// <summary>Strict horizontal range that wakes a hidden Brinstar pipe bug.</summary>
    private const int BrinstarPipeBugTriggerWidth = 64;
    /// <summary>Vertical band above a Brinstar bug in which Samus can trigger emergence.</summary>
    private const int BrinstarPipeBugTriggerTop = 96;
    /// <summary>Wait before an offscreen Brinstar bug may emerge again.</summary>
    private const int BrinstarPipeBugRespawnFrames = 48;

    /// <summary>Forward horizontal range that wakes a yellow pipe bug.</summary>
    private const int YellowPipeBugTriggerDistance = 192;
    /// <summary>Strict vertical distance allowed by the yellow bug's Samus trigger.</summary>
    private const int YellowPipeBugTriggerHeight = 48;
    /// <summary>Frames between a yellow bug becoming visible and starting its straight flight.</summary>
    private const int YellowPipeBugEmergenceDelayFrames = 24;
    /// <summary>Distance past Samus at which a yellow bug begins its one-time arc.</summary>
    private const int YellowPipeBugArcTriggerDistance = 48;
    /// <summary>The place along the quadratic speed curve, mark 40, the yellow bug's vertical arc starts from.</summary>
    private static readonly QuadraticSpeedCurvePlace YellowPipeBugArcCounterStart = QuadraticSpeedCurvePlace.FromMarks(40);

    /// <summary>Initializes the Brinstar Pipe Bug.</summary>
    internal static void InitializeBrinstarPipeBug(RoomEnemySlot slot)
    {
        var state = new BrinstarPipeBugState();
        slot.SetSpeciesState(state);
        state.SpawnPosition = slot.Position.Whole;
        state.EmergenceTop = state.SpawnPosition + new Offset16<RoomPixels>(0, -BrinstarPipeBugEmergenceHeight);
        state.Action = PipeBugAction.BrinstarWaitUntilOnScreen;
        state.RespawnDelay = BrinstarPipeBugRespawnFrames;
        state.InstalledAnimation = PipeBugAnimationChoices.Rising(IsStrong(slot), HorizontalDirection.Left);
        state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(state.InstalledAnimation), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Whether the Brinstar Pipe Bug in <paramref name="slot"/> is the strong Zebbo rather than the ordinary Zeb.</summary>
    private static bool IsStrong(RoomEnemySlot slot) =>
        slot.SpawnSettings<BrinstarPipeBugSpawnSettings>().Kind == BrinstarPipeBugKind.Strong;

    /// <summary>Initializes the Norfair Pipe Bug.</summary>
    internal void InitializeNorfairPipeBug(RoomEnemySlot slot)
    {
        // Only the formation's first bug has a trigger width; it leads the four its room lists after it.
        if (slot.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth != 0)
            _formations.Place(slot, NorfairPipeBugFormation.LedBy(_slotTable, slot));
        _ = _norfairPlaces.Add(slot);
        var state = new NorfairPipeBugState();
        slot.SetSpeciesState(state);
        state.SpawnPosition = slot.Position.Whole;
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        SignedSixteenSixteen flightSpeed = slot.SpawnSettings<NorfairPipeBugSpawnSettings>().Speed;
        state.FlightSpeeds = new EnemyLinearSpeed(flightSpeed);
        state.FormationCounter = 0;
        state.Playback = EnemyAnimationPlayback.Start(slot, PipeBugAnimations.StartingWith(PipeBugSegment.GametRisingLeft));
        state.Action = PipeBugAction.NorfairWaitForFormation;
    }

    /// <summary>Initializes the yellow Brinstar Pipe Bug.</summary>
    internal static void InitializeYellowPipeBug(RoomEnemySlot slot)
    {
        var state = new YellowPipeBugState();
        slot.SetSpeciesState(state);
        state.SpawnPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        state.Playback = EnemyAnimationPlayback.Start(slot, PipeBugAnimations.StartingWith(
            slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight));

        // The spawn settings name the speed the bug flies at.
        state.FlightSpeed = slot.SpawnSettings<YellowPipeBugSpawnSettings>().Speed;
        state.Action = PipeBugAction.YellowWaitForSamus;
        state.ArcCompleted = false;
    }

    /// <summary>Runs a Brinstar Pipe Bug's action.</summary>
    private static void RunBrinstarPipeBug(
        RoomEnemySlot slot,
        BrinstarPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case PipeBugAction.BrinstarWaitUntilOnScreen:
                // The game's on-screen check returns zero when the
                // origin is visible. The wait branches on that zero result, so this initial
                // state arms the proximity check on-screen, not after a despawn boundary.
                if (EnemyProcessingWindow.IsWithinProcessingWindow(slot, camera))
                    state.Action = PipeBugAction.BrinstarWaitForSamus;
                return;
            case PipeBugAction.BrinstarWaitForSamus:
                RunBrinstarPipeBugWaiting(slot, state, samus);
                return;
            case PipeBugAction.BrinstarEmerge:
                RunBrinstarPipeBugEmergence(slot, state, samus);
                return;
            case PipeBugAction.BrinstarFlyHorizontally:
                RunBrinstarPipeBugFlight(slot, state, camera.X);
                return;
            case PipeBugAction.BrinstarRespawnDelay:
                if (--state.RespawnDelay == 0)
                    state.Action = PipeBugAction.BrinstarWaitForSamus;
                return;
            // The other families' actions are never this Brinstar bug's.
            case PipeBugAction.NorfairWaitForFormation:
            case PipeBugAction.NorfairWaitForSamus:
            case PipeBugAction.NorfairRise:
            case PipeBugAction.NorfairLeaderStagger:
            case PipeBugAction.NorfairUpperNearStagger:
            case PipeBugAction.NorfairUpperFarStagger:
            case PipeBugAction.NorfairLowerNearStagger:
            case PipeBugAction.NorfairLowerFarStagger:
            case PipeBugAction.NorfairFlyLeft:
            case PipeBugAction.NorfairFlyRight:
            case PipeBugAction.NorfairWaitForStagger:
            case PipeBugAction.YellowWaitForSamus:
            case PipeBugAction.YellowEmergenceDelay:
            case PipeBugAction.YellowFlyLeft:
            case PipeBugAction.YellowFlyRight:
            case PipeBugAction.YellowArcLeft:
            case PipeBugAction.YellowArcRight:
            default:
                throw new InvalidDataException(
                    $"Brinstar Pipe Bug has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Runs a Norfair Pipe Bug's action.</summary>
    private void RunNorfairPipeBug(
        RoomEnemySlot slot,
        NorfairPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case PipeBugAction.NorfairWaitForFormation:
                RunNorfairPipeBugFormationWait(slot, state);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairWaitForSamus:
                RunNorfairPipeBugSamusWait(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairRise:
                RunNorfairPipeBugRise(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairLeaderStagger:
                state.CountFormationTick();
                state.Action = PipeBugAction.NorfairWaitForStagger;
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairUpperNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -16);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairUpperFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -32);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairLowerNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 16);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairLowerFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 32);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairWaitForStagger:
                RunNorfairPipeBugStaggerWait(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairFlyLeft:
                AddPipeBugLinearVelocity(slot, axis: Axis.Horizontal, state.FlightSpeeds.Negative);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugAction.NorfairFlyRight:
                AddPipeBugLinearVelocity(slot, axis: Axis.Horizontal, state.FlightSpeeds.Positive);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            // The other families' actions are never this Norfair bug's.
            case PipeBugAction.BrinstarWaitUntilOnScreen:
            case PipeBugAction.BrinstarWaitForSamus:
            case PipeBugAction.BrinstarEmerge:
            case PipeBugAction.BrinstarFlyHorizontally:
            case PipeBugAction.BrinstarRespawnDelay:
            case PipeBugAction.YellowWaitForSamus:
            case PipeBugAction.YellowEmergenceDelay:
            case PipeBugAction.YellowFlyLeft:
            case PipeBugAction.YellowFlyRight:
            case PipeBugAction.YellowArcLeft:
            case PipeBugAction.YellowArcRight:
            default:
                throw new InvalidDataException(
                    $"Norfair Pipe Bug has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Runs a yellow Pipe Bug's action.</summary>
    private static void RunYellowPipeBug(
        RoomEnemySlot slot,
        YellowPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case PipeBugAction.YellowWaitForSamus:
                RunYellowPipeBugWaiting(slot, state, samus);
                return;
            case PipeBugAction.YellowEmergenceDelay:
                RunYellowPipeBugDelay(slot, state);
                return;
            case PipeBugAction.YellowFlyLeft:
                RunYellowPipeBugStraightFlight(slot, state, samus, camera, direction: HorizontalDirection.Left);
                return;
            case PipeBugAction.YellowFlyRight:
                RunYellowPipeBugStraightFlight(slot, state, samus, camera, direction: HorizontalDirection.Right);
                return;
            case PipeBugAction.YellowArcLeft:
                RunYellowPipeBugArc(slot, state, camera, direction: HorizontalDirection.Left);
                return;
            case PipeBugAction.YellowArcRight:
                RunYellowPipeBugArc(slot, state, camera, direction: HorizontalDirection.Right);
                return;
            // The other families' actions are never this yellow bug's.
            case PipeBugAction.BrinstarWaitUntilOnScreen:
            case PipeBugAction.BrinstarWaitForSamus:
            case PipeBugAction.BrinstarEmerge:
            case PipeBugAction.BrinstarFlyHorizontally:
            case PipeBugAction.BrinstarRespawnDelay:
            case PipeBugAction.NorfairWaitForFormation:
            case PipeBugAction.NorfairWaitForSamus:
            case PipeBugAction.NorfairRise:
            case PipeBugAction.NorfairLeaderStagger:
            case PipeBugAction.NorfairUpperNearStagger:
            case PipeBugAction.NorfairUpperFarStagger:
            case PipeBugAction.NorfairLowerNearStagger:
            case PipeBugAction.NorfairLowerFarStagger:
            case PipeBugAction.NorfairFlyLeft:
            case PipeBugAction.NorfairFlyRight:
            case PipeBugAction.NorfairWaitForStagger:
            default:
                throw new InvalidDataException(
                    $"Yellow Pipe Bug has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Wakes a hidden Brinstar bug only when Samus enters its narrow horizontal and upper vertical band.</summary>
    private static void RunBrinstarPipeBugWaiting(
        RoomEnemySlot slot,
        BrinstarPipeBugState state,
        SamusState samus)
    {
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole);
        if (deltaY >= 0 || CoordinateAxis.AsSigned(deltaY + BrinstarPipeBugTriggerTop) < 0)
            return;

        state.FliesLeft = (samus.Position.Whole - slot.Position.Whole).X < 0;
        if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) >=
            BrinstarPipeBugTriggerWidth)
        {
            return;
        }

        state.Action = PipeBugAction.BrinstarEmerge;
        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        slot.AnimationClock.ClearRepeats();
        SelectBrinstarPipeBugAnimation(slot, state, PipeBugAnimationChoices.Rising(
            IsStrong(slot), state.FliesLeft ? HorizontalDirection.Left : HorizontalDirection.Right));
    }

    /// <summary>Rises (<see cref="PipeBugEmergenceRise"/>) until the bug is above Samus, then switches to horizontal flight.</summary>
    private static void RunBrinstarPipeBugEmergence(
        RoomEnemySlot slot,
        BrinstarPipeBugState state,
        SamusState samus)
    {
        slot.Position = slot.Position.WithY(PipeBugEmergenceRise.Risen(slot.Position.Y));
        if ((state.EmergenceTop - slot.Position.Whole).Y < 0 ||
            slot.Position.Y.Whole >= samus.Position.Y.Whole)
        {
            return;
        }

        SelectBrinstarPipeBugAnimation(slot, state, PipeBugAnimationChoices.ShootingWithSameFacing(state.InstalledAnimation));
        state.Action = PipeBugAction.BrinstarFlyHorizontally;
    }

    /// <summary>Moves two pixels per frame, then restores the stored spawn position and starts an invisible respawn wait offscreen.</summary>
    private static void RunBrinstarPipeBugFlight(
        RoomEnemySlot slot,
        BrinstarPipeBugState state,
        int cameraX)
    {
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole +
            (state.FliesLeft ? -2 : 2));
        // The despawn tests only the horizontal screen extent.
        if (!EnemyProcessingWindow.EnemyIsHorizontallyOffScreen(slot, cameraX))
            return;

        slot.Position = slot.Position.WithWhole(state.SpawnPosition).WithFractionX(0)
            .WithY(PipeBugEmergenceRise.Respawned(state.SpawnPosition.Y));
        SelectBrinstarPipeBugAnimation(slot, state, PipeBugAnimationChoices.Rising(IsStrong(slot), HorizontalDirection.Left));
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        state.RespawnDelay = BrinstarPipeBugRespawnFrames;
        state.Action = PipeBugAction.BrinstarRespawnDelay;
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one already installed, which keeps playing.</summary>
    internal static void SelectBrinstarPipeBugAnimation(
        RoomEnemySlot slot,
        BrinstarPipeBugState state,
        PipeBugSegment animation)
    {
        if (animation == state.InstalledAnimation)
            return;
        state.InstalledAnimation = animation;
        state.Playback = EnemyAnimationPlayback.Start(slot, PipeBugAnimations.StartingWith(animation));
    }

    /// <summary>Arms the Norfair leader only after all five consecutive formation members remain dormant.</summary>
    private void RunNorfairPipeBugFormationWait(RoomEnemySlot slot, NorfairPipeBugState state)
    {
        // Only the first Pipe Bug listed has a trigger width. It is the formation controller and may
        // arm itself only while all four Pipe Bugs listed after it are still at the
        // dormant action.
        if (slot.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth == 0)
            return;
        NorfairPipeBugFormation formation = RequireNorfairPipeBugFormation(slot);
        if (formation.Members.All(member => KilledGametHoldsFormation.Waits(
                member.Place.SpeciesState<NorfairPipeBugState>(),
                static state => state.Action == PipeBugAction.NorfairWaitForFormation)))
        {
            state.Action = PipeBugAction.NorfairWaitForSamus;
        }
    }

    /// <summary>Launches the five-bug rise when Samus enters the leader's trigger, retaining follower frame timers.</summary>
    internal void RunNorfairPipeBugSamusWait(
        RoomEnemySlot leader,
        NorfairPipeBugState leaderState,
        SamusState samus)
    {
        int triggerWidth = leader.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth;
        if (!EnemyMath.IsWithinStrictModularDistance(leader.Position.X.Whole, samus.Position.X.Whole, triggerWidth) ||
            CoordinateAxis.Difference(leader.Position.Y.Whole, samus.Position.Y.Whole) < 0)
        {
            return;
        }

        leaderState.CountFormationTick();
        // The formation tick resets only the current (leader) slot. The following
        // formation stores change every animation, but leave follower
        // timers intact, including zero timers in cleared respawn placeholders.
        leader.AnimationClock.Restart();
        bool samusIsRight = CoordinateAxis.Difference(samus.Position.X.Whole, leader.Position.X.Whole) >= 0;
        NorfairPipeBugFormation formation = RequireNorfairPipeBugFormation(leader);
        // A member killed since the formation armed has nothing to launch: it respawns waiting, as a new bug.
        foreach (NorfairPipeBugMember member in formation.Members)
        {
            if (member.Place.SpeciesState<NorfairPipeBugState>() is not { } memberState)
                continue;
            memberState.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(samusIsRight
                ? PipeBugSegment.GametRisingRight
                : PipeBugSegment.GametRisingLeft), AnimationPlace.StartOfAnimation);
            memberState.Action = PipeBugAction.NorfairRise;
            memberState.StaggerTarget = member.Role.StaggerTarget;
            memberState.PostRiseAction = member.Role.PostRiseAction;
        }
    }

    /// <summary>Raises one Norfair bug until it passes Samus's Y and selects its authored post-rise action.</summary>
    private static void RunNorfairPipeBugRise(
        RoomEnemySlot slot,
        NorfairPipeBugState state,
        SamusState samus)
    {
        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        AddPipeBugLinearVelocity(slot, axis: Axis.Vertical, -OnePixelPerFrame);
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, samus.Position.Y.Whole) >= 0)
            return;

        state.Action = state.PostRiseAction
            ?? throw new InvalidOperationException("A Norfair Pipe Bug finished rising without a post-rise action from its formation.");
        state.RiseEnd = slot.Position.Whole;
        state.Playback = StartPipeBugAnimation(slot,
            CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0
                ? PipeBugSegment.GametRisingRight
                : PipeBugSegment.GametRisingLeft);
    }

    /// <summary>Offsets a rising Norfair member by its signed stagger target before its timed flight wait.</summary>
    private static void RunNorfairPipeBugVerticalStagger(
        RoomEnemySlot slot,
        NorfairPipeBugState state,
        int signedOffset)
    {
        state.CountFormationTick();
        // Negative offsets move upward and positive offsets downward, one pixel per frame. The
        // game clamps exactly at the emergence Y +/- 16/32.
        AddPipeBugLinearVelocity(
            slot,
            axis: Axis.Vertical,
            signedOffset < 0 ? -OnePixelPerFrame : OnePixelPerFrame);
        Position16<RoomPixels> target = state.RiseEnd + new Offset16<RoomPixels>(0, signedOffset);
        int distancePastTarget = (slot.Position.Whole - target).Y;
        bool reached = signedOffset < 0
            ? distancePastTarget < 0
            : distancePastTarget >= 0;
        if (!reached)
            return;
        slot.Position = slot.Position.WithY(new(target.Y, 0));
        state.Action = PipeBugAction.NorfairWaitForStagger;
    }

    /// <summary>Waits for a member's stagger counter and then chooses left or right flight from Samus's live X.</summary>
    private static void RunNorfairPipeBugStaggerWait(
        RoomEnemySlot slot,
        NorfairPipeBugState state,
        SamusState samus)
    {
        state.CountFormationTick();
        if (!state.HasReachedStaggerTarget)
            return;

        state.FormationCounter = 0;
        slot.AnimationClock.Restart();
        if (CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0)
        {
            state.Action = PipeBugAction.NorfairFlyRight;
            state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(PipeBugSegment.GametFlyingRight), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Action = PipeBugAction.NorfairFlyLeft;
            state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(PipeBugSegment.GametFlyingLeft), AnimationPlace.StartOfAnimation);
        }
    }

    /// <summary>
    /// The off-screen reset tests the bare enemy center against the screen, not the sprite-padded visibility check.
    /// </summary>
    private static void ResetNorfairPipeBugIfOffScreen(
        RoomEnemySlot slot,
        NorfairPipeBugState state,
        Position16<RoomPixels> camera)
    {
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
            return;
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        state.Action = PipeBugAction.NorfairWaitForFormation;
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
    }

    /// <summary>
    /// The formation the leader found when it was placed, rejecting an incomplete or unrelated formation: every member's
    /// place was set up as a Norfair Pipe Bug, and holds one or, killed, none until it respawns
    /// (<see cref="KilledGametHoldsFormation"/>).
    /// </summary>
    private NorfairPipeBugFormation RequireNorfairPipeBugFormation(RoomEnemySlot leader)
    {
        NorfairPipeBugFormation formation = _formations.Of(leader);
        foreach (NorfairPipeBugMember member in formation.Members)
        {
            RoomEnemySlot place = member.Place;
            bool killed = place.SpeciesState<NorfairPipeBugState>() is null && place.DefinitionId == EnemyDefinitionId.None;
            if (!_norfairPlaces.Contains(place) ||
                (!killed && place.DefinitionId != EnemyDefinitionId.Gamet))
            {
                throw new InvalidDataException(
                    $"Norfair Pipe Bug formation slot {place.SlotIndex} holds {place.DefinitionId}, " +
                    $"not a {EnemyDefinitionId.Gamet} or a killed one's place.");
            }
        }
        return formation;
    }

    /// <summary>Wakes a yellow bug when Samus is ahead on its authored side and within the narrow vertical band.</summary>
    private static void RunYellowPipeBugWaiting(
        RoomEnemySlot slot,
        YellowPipeBugState state,
        SamusState samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole);
        bool inFront = slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
            ? deltaX < 0 && CoordinateAxis.AsSigned(deltaX + YellowPipeBugTriggerDistance) >= 0
            : deltaX >= 0 && CoordinateAxis.Difference(deltaX, YellowPipeBugTriggerDistance) < 0;
        if (!inFront ||
            !EnemyMath.IsWithinStrictModularDistance(slot.Position.Y.Whole, samus.Position.Y.Whole, YellowPipeBugTriggerHeight))
        {
            return;
        }

        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        state.EmergenceDelay = YellowPipeBugEmergenceDelayFrames;
        state.Action = PipeBugAction.YellowEmergenceDelay;
    }

    /// <summary>Counts down the visible emergence delay before installing directional straight-flight art.</summary>
    private static void RunYellowPipeBugDelay(RoomEnemySlot slot, YellowPipeBugState state)
    {
        if (--state.EmergenceDelay != 0)
            return;
        slot.AnimationClock.Restart();
        if (slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left)
        {
            state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(PipeBugSegment.GeegaFlyingLeft), AnimationPlace.StartOfAnimation);
            state.Action = PipeBugAction.YellowFlyLeft;
        }
        else
        {
            state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(PipeBugSegment.GeegaFlyingRight), AnimationPlace.StartOfAnimation);
            state.Action = PipeBugAction.YellowFlyRight;
        }
    }

    /// <summary>Advances yellow flight and begins its single quadratic arc after passing Samus by the threshold.</summary>
    private static void RunYellowPipeBugStraightFlight(
        RoomEnemySlot slot,
        YellowPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        AddYellowPipeBugHorizontalVelocity(slot, state, direction == HorizontalDirection.Left);
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        if (state.ArcCompleted)
            return;

        bool passedSamus = direction == HorizontalDirection.Left
            ? CoordinateAxis.Difference(slot.Position.X.Whole - samus.Position.X.Whole, YellowPipeBugArcTriggerDistance) < 0
            : CoordinateAxis.Difference(samus.Position.X.Whole - slot.Position.X.Whole, YellowPipeBugArcTriggerDistance) < 0;
        if (!passedSamus)
            return;

        state.Action = direction == HorizontalDirection.Left
            ? PipeBugAction.YellowArcLeft
            : PipeBugAction.YellowArcRight;
        state.ArcCounter = YellowPipeBugArcCounterStart;
        state.ArcRising = true;
        state.Playback = StartPipeBugAnimation(slot,
            direction == HorizontalDirection.Left
                ? PipeBugSegment.GeegaArcingLeft
                : PipeBugSegment.GeegaArcingRight);
    }

    /// <summary>Applies the two quadratic vertical phases until the bug returns to its spawn height.</summary>
    private static void RunYellowPipeBugArc(
        RoomEnemySlot slot,
        YellowPipeBugState state,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(slot, state, direction == HorizontalDirection.Left);
        if (state.ArcRising)
        {
            // The game moves the place back and ends the rise when it would pass the curve's start, holding it there; the
            // place never passes the curve's start, so the end is tested before the step.
            if (state.ArcCounter == QuadraticSpeedCurvePlace.Start)
            {
                state.ArcRising = false;
            }
            else
            {
                state.ArcCounter -= QuadraticSpeedCurvePlace.OneMark;
                AddPipeBugDisplacement(slot, axis: Axis.Vertical,
                    state.ArcCounter.Speed.Positive);
            }
            return;
        }

        state.ArcCounter += QuadraticSpeedCurvePlace.OneMark;
        AddPipeBugDisplacement(slot, axis: Axis.Vertical,
            state.ArcCounter.Speed.Negative);
        bool returnedToSpawnY = direction == HorizontalDirection.Left
            ? CoordinateAxis.Difference(slot.Position.Y.Whole, state.SpawnPosition.Y) < 0
            : CoordinateAxis.Difference(state.SpawnPosition.Y, slot.Position.Y.Whole) >= 0;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.ArcRising = true;
        state.Action = direction == HorizontalDirection.Left
            ? PipeBugAction.YellowFlyLeft
            : PipeBugAction.YellowFlyRight;
        state.Playback = StartPipeBugAnimation(slot,
            direction == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight);
    }

    /// <summary>Hides an offscreen yellow bug, restores its spawn point, and makes its arc available again.</summary>
    private static bool ResetYellowPipeBugIfOffScreen(
        RoomEnemySlot slot,
        YellowPipeBugState state,
        Position16<RoomPixels> camera)
    {
        // Every Geega flight action resets through the bare center's on-screen test.
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
            return false;
        slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
        state.Action = PipeBugAction.YellowWaitForSamus;
        state.ArcCompleted = false;
        state.Playback = EnemyAnimationPlayback.Start(slot, PipeBugAnimations.StartingWith(
            slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight));
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        return true;
    }

    /// <summary>Applies the separately authored positive or negative 1/65536-pixel flight speed.</summary>
    private static void AddYellowPipeBugHorizontalVelocity(
        RoomEnemySlot slot,
        YellowPipeBugState state,
        bool movingLeft)
    {
        slot.Position = slot.Position.AdvanceX(movingLeft ? -state.FlightSpeed : state.FlightSpeed);
    }

    /// <summary>The speed a Norfair bug rises and staggers at, one pixel per frame.</summary>
    private static readonly SignedSixteenSixteen OnePixelPerFrame = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>Adds a linear-speed velocity to one axis, carrying the fraction into the whole.</summary>
    private static void AddPipeBugLinearVelocity(
        RoomEnemySlot slot,
        Axis axis,
        SignedSixteenSixteen velocity)
    {
        slot.Position = axis == Axis.Horizontal ? slot.Position.AdvanceX(velocity) : slot.Position.AdvanceY(velocity);
    }

    /// <summary>Adds a wrapped signed 1/65536-pixel displacement to one position axis.</summary>
    private static void AddPipeBugDisplacement(
        RoomEnemySlot slot,
        Axis axis,
        SignedSixteenSixteen displacement)
    {
        if (axis == Axis.Horizontal)
        {
            slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(displacement));
        }
        else
        {
            slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(displacement));
        }
    }

    /// <summary>The playback of <paramref name="segment"/>'s animation restarted at timer one, its animation frame timer cleared.</summary>
    internal static AnimationPlayback<PipeBugSegment> StartPipeBugAnimation(RoomEnemySlot slot, PipeBugSegment segment) =>
        EnemyAnimationPlayback.Start(slot, PipeBugAnimations.StartingWith(segment));

    /// <summary>Advances the Pipe Bug's typed animation, held in its family's state.</summary>
    internal static void AdvancePipeBugsTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        switch (PipeBugsPartDefinitions.Parts[slot.DefinitionId])
        {
            case PipeBugsPart.Zeb or PipeBugsPart.Zebbo:
                EnemyAnimationPlayback.Advance(slot, PipeBugAnimations.Segments,
                    static (BrinstarPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));
                return;
            case PipeBugsPart.Gamet:
                EnemyAnimationPlayback.Advance(slot, PipeBugAnimations.Segments,
                    static (NorfairPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));
                return;
            case PipeBugsPart.Geega:
                EnemyAnimationPlayback.Advance(slot, PipeBugAnimations.Segments,
                    static (YellowPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));
                return;
            default:
                throw new InvalidOperationException($"Pipe Bug animation advanced for {slot.DefinitionId}.");
        }
    }

    /// <summary>Rejects a cue: no Pipe Bug animation raises one.</summary>
    private static AnimationContinuation<PipeBugSegment>? RaisesNoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"A Pipe Bug animation raised {cue}, though its lists raise no events.");

    /// <summary>Runs the Pipe Bug's main AI for its family.</summary>
    internal void RunPipeBugsMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        switch (PipeBugsPartDefinitions.Parts[slot.DefinitionId])
        {
            case PipeBugsPart.Zeb or PipeBugsPart.Zebbo:
                RunBrinstarPipeBug(slot, slot.RequireSpeciesState<BrinstarPipeBugState>(), frame.Samus, frame.Camera);
                return;
            case PipeBugsPart.Gamet:
                RunNorfairPipeBug(slot, slot.RequireSpeciesState<NorfairPipeBugState>(), frame.Samus, frame.Camera);
                return;
            case PipeBugsPart.Geega:
                RunYellowPipeBug(slot, slot.RequireSpeciesState<YellowPipeBugState>(), frame.Samus, frame.Camera);
                return;
            default:
                throw new InvalidOperationException($"Pipe Bug main AI ran for {slot.DefinitionId}.");
        }
    }
}
