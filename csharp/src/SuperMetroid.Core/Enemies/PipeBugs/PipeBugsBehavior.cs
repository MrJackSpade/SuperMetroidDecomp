using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The pipe bugs species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PipeBugsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Gamet, EnemyDefinitionId.Geega, EnemyDefinitionId.Zeb, EnemyDefinitionId.Zebbo];

    /// <summary>A Brinstar Pipe Bug (Zeb or Zebbo), a Norfair one (Gamet) or a yellow one (Geega) (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Pipe Bugs definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition is EnemyDefinitionId.Zeb or EnemyDefinitionId.Zebbo)
            return new BrinstarPipeBugEntity(place, common);
        if (definition == EnemyDefinitionId.Gamet)
            return new NorfairPipeBugEntity(this, place, common);
        if (definition == EnemyDefinitionId.Geega)
            return new YellowPipeBugEntity(place, common);
        throw new InvalidOperationException($"The Pipe Bugs species creates no {place.DefinitionId}.");
    }

    /// <summary>The room's enemy places, where each Norfair formation leader finds its formation when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>
    /// The five-bug formation each Norfair leader found when it was placed: itself, then the four bugs its room lists
    /// right after it, each in its role (#1840).
    /// </summary>
    private readonly EnemyPartLinks<NorfairPipeBugFormation> _formations = new();
    /// <summary>
    /// The places set up as Norfair Pipe Bugs this room. A formation member killed and not yet respawned holds no state,
    /// and its place is still the formation's.
    /// </summary>
    private readonly HashSet<RoomEnemyPlace> _norfairPlaces = [];

    /// <summary>Binds the shared enemy pool needed to validate Norfair formation membership.</summary>
    internal PipeBugsBehavior(EnemyPlaces places)
    {
        _places = places;
    }

    /// <summary>Initializes the Brinstar Pipe Bug.</summary>
    internal static void InitializeBrinstarPipeBug(RoomEnemyPlace place)
    {
        var state = new BrinstarPipeBugState(BrinstarPipeBugAction.WaitUntilOnScreen);
        place.SetSpeciesState(state);
        state.SpawnPosition = place.Position.Whole;
        state.EmergenceTopY = CoordinateAxis.Wrap(state.SpawnPosition.Y - PipeBugDefinitions.BrinstarEmergenceHeight);
        state.RespawnDelay = PipeBugDefinitions.BrinstarRespawnWait;
        state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(
            PipeBugAnimationChoices.Rising(IsStrong(place), HorizontalDirection.Left)), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Whether the Brinstar Pipe Bug in <paramref name="place"/> is the strong Zebbo rather than the ordinary Zeb.</summary>
    private static bool IsStrong(RoomEnemyPlace place) =>
        place.SpawnSettings<BrinstarPipeBugSpawnSettings>().Kind == BrinstarPipeBugKind.Strong;

    /// <summary>Initializes the Norfair Pipe Bug.</summary>
    internal void InitializeNorfairPipeBug(RoomEnemyPlace place)
    {
        // Only the formation's first bug has a trigger width; it leads the four its room lists after it.
        if (place.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth != 0)
            _formations.Place(place, NorfairPipeBugFormation.LedBy(_places, place));
        _ = _norfairPlaces.Add(place);
        var state = new NorfairPipeBugState(NorfairPipeBugAction.WaitForFormation);
        place.SetSpeciesState(state);
        state.SpawnPosition = place.Position.Whole;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        SignedSixteenSixteen flightSpeed = place.SpawnSettings<NorfairPipeBugSpawnSettings>().Speed;
        state.FlightSpeeds = new EnemyLinearSpeed(flightSpeed);
        state.FormationCounter = 0;
        state.Playback = EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(PipeBugSegment.GametRisingLeft));
    }

    /// <summary>Initializes the yellow Brinstar Pipe Bug.</summary>
    internal static void InitializeYellowPipeBug(RoomEnemyPlace place)
    {
        var state = new YellowPipeBugState(YellowPipeBugAction.WaitForSamus);
        place.SetSpeciesState(state);
        state.SpawnPosition = place.Position.Whole;
        place.Position = place.Position.WithFractionX(0).WithFractionY(0);
        state.Playback = EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(
            place.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight));

        // The spawn settings name the speed the bug flies at.
        state.FlightSpeed = place.SpawnSettings<YellowPipeBugSpawnSettings>().Speed;
        state.ArcCompleted = false;
    }

    /// <summary>Runs a Brinstar Pipe Bug's action.</summary>
    private static void RunBrinstarPipeBug(
        RoomEnemyPlace place,
        BrinstarPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case BrinstarPipeBugAction.WaitUntilOnScreen:
                // The game's on-screen check returns zero when the
                // origin is visible. The wait branches on that zero result, so this initial
                // state arms the proximity check on-screen, not after a despawn boundary.
                if (EnemyProcessingWindow.IsWithinProcessingWindow(place, camera))
                    state.Action = BrinstarPipeBugAction.WaitForSamus;
                return;
            case BrinstarPipeBugAction.WaitForSamus:
                RunBrinstarPipeBugWaiting(place, state, samus);
                return;
            case BrinstarPipeBugAction.Emerge:
                RunBrinstarPipeBugEmergence(place, state, samus);
                return;
            case BrinstarPipeBugAction.FlyHorizontally:
                RunBrinstarPipeBugFlight(place, state, camera.X);
                return;
            case BrinstarPipeBugAction.RespawnDelay:
                if (--state.RespawnDelay == 0)
                    state.Action = BrinstarPipeBugAction.WaitForSamus;
                return;
            default:
                throw new InvalidDataException(
                    $"Brinstar Pipe Bug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs a Norfair Pipe Bug's action.</summary>
    private void RunNorfairPipeBug(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case NorfairPipeBugAction.WaitForFormation:
                RunNorfairPipeBugFormationWait(place, state);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.WaitForSamus:
                RunNorfairPipeBugSamusWait(place, state, samus);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.Rise:
                RunNorfairPipeBugRise(place, state, samus);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.LeaderStagger:
                state.CountFormationTick();
                state.Action = NorfairPipeBugAction.WaitForStagger;
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.Stagger:
                RunNorfairPipeBugVerticalStagger(place, state, state.StaggerOffset);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.WaitForStagger:
                RunNorfairPipeBugStaggerWait(place, state, samus);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.Fly:
                AddPipeBugLinearVelocity(place, axis: Axis.Horizontal,
                    state.FlightDirection == HorizontalDirection.Left ? state.FlightSpeeds.Negative : state.FlightSpeeds.Positive);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            default:
                throw new InvalidDataException(
                    $"Norfair Pipe Bug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs a yellow Pipe Bug's action.</summary>
    private static void RunYellowPipeBug(
        RoomEnemyPlace place,
        YellowPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case YellowPipeBugAction.WaitForSamus:
                RunYellowPipeBugWaiting(place, state, samus);
                return;
            case YellowPipeBugAction.EmergenceDelay:
                RunYellowPipeBugDelay(place, state);
                return;
            case YellowPipeBugAction.Fly:
                RunYellowPipeBugStraightFlight(place, state, samus, camera, state.FlightDirection);
                return;
            case YellowPipeBugAction.Arc:
                RunYellowPipeBugArc(place, state, camera, state.FlightDirection);
                return;
            default:
                throw new InvalidDataException(
                    $"Yellow Pipe Bug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Wakes a hidden Brinstar bug only when Samus enters its narrow horizontal and upper vertical band.</summary>
    private static void RunBrinstarPipeBugWaiting(
        RoomEnemyPlace place,
        BrinstarPipeBugState state,
        SamusState samus)
    {
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole);
        if (deltaY >= 0 || CoordinateAxis.AsSigned(deltaY + PipeBugDefinitions.BrinstarTriggerTop) < 0)
            return;

        state.FliesLeft = (samus.Position.Whole - place.Position.Whole).X < 0;
        if (CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole) >=
            PipeBugDefinitions.BrinstarTriggerWidth)
        {
            return;
        }

        state.Action = BrinstarPipeBugAction.Emerge;
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        SelectBrinstarPipeBugAnimation(place, state, PipeBugAnimationChoices.Rising(
            IsStrong(place), state.FliesLeft ? HorizontalDirection.Left : HorizontalDirection.Right));
    }

    /// <summary>Rises (<see cref="PipeBugEmergenceRise"/>) until the bug is above Samus, then switches to horizontal flight.</summary>
    private static void RunBrinstarPipeBugEmergence(
        RoomEnemyPlace place,
        BrinstarPipeBugState state,
        SamusState samus)
    {
        place.Position = place.Position.WithY(PipeBugEmergenceRise.Risen(place.Position.Y));
        if (CoordinateAxis.Precedes(state.EmergenceTopY, place.Position.Y.Whole) ||
            place.Position.Y.Whole >= samus.Position.Y.Whole)
        {
            return;
        }

        // The bug shoots facing the way it rose: toward the side Samus was on as it emerged.
        SelectBrinstarPipeBugAnimation(place, state, PipeBugAnimationChoices.Shooting(
            IsStrong(place), state.FliesLeft ? HorizontalDirection.Left : HorizontalDirection.Right));
        state.Action = BrinstarPipeBugAction.FlyHorizontally;
    }

    /// <summary>Moves its flight step each frame, then restores the stored spawn position and starts an invisible respawn wait offscreen.</summary>
    private static void RunBrinstarPipeBugFlight(
        RoomEnemyPlace place,
        BrinstarPipeBugState state,
        int cameraX)
    {
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole +
            (state.FliesLeft ? -PipeBugDefinitions.BrinstarFlightStep : PipeBugDefinitions.BrinstarFlightStep));
        // The despawn tests only the horizontal screen extent.
        if (!EnemyProcessingWindow.EnemyIsHorizontallyOffScreen(place, cameraX))
            return;

        place.Position = place.Position.WithWhole(state.SpawnPosition).WithFractionX(0)
            .WithY(PipeBugEmergenceRise.Respawned(state.SpawnPosition.Y));
        SelectBrinstarPipeBugAnimation(place, state, PipeBugAnimationChoices.Rising(IsStrong(place), HorizontalDirection.Left));
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        state.RespawnDelay = PipeBugDefinitions.BrinstarRespawnWait;
        state.Action = BrinstarPipeBugAction.RespawnDelay;
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one playing, which keeps playing (#2145).</summary>
    internal static void SelectBrinstarPipeBugAnimation(
        RoomEnemyPlace place,
        BrinstarPipeBugState state,
        PipeBugSegment animation) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, PipeBugAnimations.StartingWith(animation));

    /// <summary>Arms the Norfair leader only after all five consecutive formation members remain dormant.</summary>
    private void RunNorfairPipeBugFormationWait(RoomEnemyPlace place, NorfairPipeBugState state)
    {
        // Only the first Pipe Bug listed has a trigger width. It is the formation controller and may
        // arm itself only while all four Pipe Bugs listed after it are still at the
        // dormant action. It reads each member's place whatever it holds: a member killed and not yet respawned has a
        // cleared place, which never reads as waiting, so it holds the formation back until its respawn sets it up again.
        if (place.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth == 0)
            return;
        NorfairPipeBugFormation formation = RequireNorfairPipeBugFormation(place);
        if (formation.Members.All(static member =>
                member.Place.FoundState<NorfairPipeBugState>() is { Action: NorfairPipeBugAction.WaitForFormation }))
        {
            state.Action = NorfairPipeBugAction.WaitForSamus;
        }
    }

    /// <summary>Launches the five-bug rise when Samus enters the leader's trigger, retaining follower frame timers.</summary>
    internal void RunNorfairPipeBugSamusWait(
        RoomEnemyPlace leader,
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
        // The formation tick resets only the current (leader) place. The following
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
            memberState.Action = NorfairPipeBugAction.Rise;
            memberState.StaggerTarget = member.Role.StaggerTarget;
            memberState.PostRiseAction = member.Role.PostRiseAction;
            memberState.StaggerOffset = member.Role.StaggerOffset;
        }
    }

    /// <summary>Raises one Norfair bug until it passes Samus's Y and selects its authored post-rise action.</summary>
    private static void RunNorfairPipeBugRise(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        SamusState samus)
    {
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        AddPipeBugLinearVelocity(place, axis: Axis.Vertical, -PipeBugDefinitions.NorfairRiseSpeed);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, samus.Position.Y.Whole) >= 0)
            return;

        state.Action = state.PostRiseAction
            ?? throw new InvalidOperationException("A Norfair Pipe Bug finished rising without a post-rise action from its formation.");
        state.RiseEndY = place.Position.Y.Whole;
        state.Playback = StartPipeBugAnimation(place,
            CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) >= 0
                ? PipeBugSegment.GametRisingRight
                : PipeBugSegment.GametRisingLeft);
    }

    /// <summary>Offsets a rising Norfair member by its signed stagger target before its timed flight wait.</summary>
    private static void RunNorfairPipeBugVerticalStagger(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        int signedOffset)
    {
        state.CountFormationTick();
        // Negative offsets move upward and positive offsets downward, one pixel per frame. The
        // game clamps exactly at the emergence Y plus the offset.
        AddPipeBugLinearVelocity(
            place,
            axis: Axis.Vertical,
            signedOffset < 0 ? -PipeBugDefinitions.NorfairRiseSpeed : PipeBugDefinitions.NorfairRiseSpeed);
        int targetY = CoordinateAxis.Wrap(state.RiseEndY + signedOffset);
        int distancePastTarget = CoordinateAxis.Difference(place.Position.Y.Whole, targetY);
        bool reached = signedOffset < 0
            ? distancePastTarget < 0
            : distancePastTarget >= 0;
        if (!reached)
            return;
        place.Position = place.Position.WithY(new(targetY, 0));
        state.Action = NorfairPipeBugAction.WaitForStagger;
    }

    /// <summary>Waits for a member's stagger counter and then chooses left or right flight from Samus's live X.</summary>
    private static void RunNorfairPipeBugStaggerWait(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        SamusState samus)
    {
        state.CountFormationTick();
        if (!state.HasReachedStaggerTarget)
            return;

        state.FormationCounter = 0;
        place.AnimationClock.Restart();
        state.Action = NorfairPipeBugAction.Fly;
        if (CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) >= 0)
        {
            state.FlightDirection = HorizontalDirection.Right;
            state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(PipeBugSegment.GametFlyingRight), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.FlightDirection = HorizontalDirection.Left;
            state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(PipeBugSegment.GametFlyingLeft), AnimationPlace.StartOfAnimation);
        }
    }

    /// <summary>
    /// The off-screen reset tests the bare enemy center against the screen, not the sprite-padded visibility check.
    /// </summary>
    private static void ResetNorfairPipeBugIfOffScreen(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        Position16<RoomPixels> camera)
    {
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(place, camera))
            return;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        state.Action = NorfairPipeBugAction.WaitForFormation;
        place.Position = place.Position.WithWhole(state.SpawnPosition);
    }

    /// <summary>
    /// The formation the leader found when it was placed, rejecting an incomplete or unrelated formation: every member's
    /// place was set up as a Norfair Pipe Bug, and holds one or, killed, none until it respawns
    /// (<see cref="RoomEnemyPlace.FoundState{T}"/>).
    /// </summary>
    private NorfairPipeBugFormation RequireNorfairPipeBugFormation(RoomEnemyPlace leader)
    {
        NorfairPipeBugFormation formation = _formations.Of(leader);
        foreach (NorfairPipeBugMember member in formation.Members)
        {
            RoomEnemyPlace place = member.Place;
            bool killed = place.SpeciesState<NorfairPipeBugState>() is null && place.DefinitionId == EnemyDefinitionId.None;
            if (!_norfairPlaces.Contains(place) ||
                (!killed && place.DefinitionId != EnemyDefinitionId.Gamet))
            {
                throw new InvalidDataException(
                    $"Norfair Pipe Bug formation place {place.Ordinal} holds {place.DefinitionId}, " +
                    $"not a {EnemyDefinitionId.Gamet} or a killed one's place.");
            }
        }
        return formation;
    }

    /// <summary>Wakes a yellow bug when Samus is ahead on its authored side and within the narrow vertical band.</summary>
    private static void RunYellowPipeBugWaiting(
        RoomEnemyPlace place,
        YellowPipeBugState state,
        SamusState samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole);
        bool inFront = place.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
            ? deltaX < 0 && CoordinateAxis.AsSigned(deltaX + PipeBugDefinitions.YellowTriggerDistance) >= 0
            : deltaX >= 0 && CoordinateAxis.Difference(deltaX, PipeBugDefinitions.YellowTriggerDistance) < 0;
        if (!inFront ||
            !EnemyMath.IsWithinStrictModularDistance(place.Position.Y.Whole, samus.Position.Y.Whole, PipeBugDefinitions.YellowTriggerHeight))
        {
            return;
        }

        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        state.EmergenceDelay = PipeBugDefinitions.YellowEmergenceDelay;
        state.Action = YellowPipeBugAction.EmergenceDelay;
    }

    /// <summary>Counts down the visible emergence delay before installing directional straight-flight art.</summary>
    private static void RunYellowPipeBugDelay(RoomEnemyPlace place, YellowPipeBugState state)
    {
        if (--state.EmergenceDelay != 0)
            return;
        place.AnimationClock.Restart();
        state.FlightDirection = place.SpawnSettings<YellowPipeBugSpawnSettings>().Facing;
        state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(state.FlightDirection == HorizontalDirection.Left
            ? PipeBugSegment.GeegaFlyingLeft
            : PipeBugSegment.GeegaFlyingRight), AnimationPlace.StartOfAnimation);
        state.Action = YellowPipeBugAction.Fly;
    }

    /// <summary>Advances yellow flight and begins its single quadratic arc after passing Samus by the threshold.</summary>
    private static void RunYellowPipeBugStraightFlight(
        RoomEnemyPlace place,
        YellowPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        AddYellowPipeBugHorizontalVelocity(place, state, direction == HorizontalDirection.Left);
        if (ResetYellowPipeBugIfOffScreen(place, state, camera))
            return;
        if (state.ArcCompleted)
            return;

        bool passedSamus = direction == HorizontalDirection.Left
            ? CoordinateAxis.Difference(place.Position.X.Whole - samus.Position.X.Whole, PipeBugDefinitions.YellowArcTriggerDistance) < 0
            : CoordinateAxis.Difference(samus.Position.X.Whole - place.Position.X.Whole, PipeBugDefinitions.YellowArcTriggerDistance) < 0;
        if (!passedSamus)
            return;

        state.Action = YellowPipeBugAction.Arc;
        state.ArcCounter = PipeBugDefinitions.YellowArcStart;
        state.ArcRising = true;
        state.Playback = StartPipeBugAnimation(place,
            direction == HorizontalDirection.Left
                ? PipeBugSegment.GeegaArcingLeft
                : PipeBugSegment.GeegaArcingRight);
    }

    /// <summary>Applies the two quadratic vertical phases until the bug returns to its spawn height.</summary>
    private static void RunYellowPipeBugArc(
        RoomEnemyPlace place,
        YellowPipeBugState state,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        if (ResetYellowPipeBugIfOffScreen(place, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(place, state, direction == HorizontalDirection.Left);
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
                AddPipeBugDisplacement(place, axis: Axis.Vertical,
                    state.ArcCounter.Speed.Positive);
            }
            return;
        }

        state.ArcCounter += QuadraticSpeedCurvePlace.OneMark;
        AddPipeBugDisplacement(place, axis: Axis.Vertical,
            state.ArcCounter.Speed.Negative);
        bool returnedToSpawnY = direction == HorizontalDirection.Left
            ? CoordinateAxis.Difference(place.Position.Y.Whole, state.SpawnPosition.Y) < 0
            : CoordinateAxis.Difference(state.SpawnPosition.Y, place.Position.Y.Whole) >= 0;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.ArcRising = true;
        state.Action = YellowPipeBugAction.Fly;
        state.Playback = StartPipeBugAnimation(place,
            direction == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight);
    }

    /// <summary>Hides an offscreen yellow bug, restores its spawn point, and makes its arc available again.</summary>
    private static bool ResetYellowPipeBugIfOffScreen(
        RoomEnemyPlace place,
        YellowPipeBugState state,
        Position16<RoomPixels> camera)
    {
        // Every Geega flight action resets through the bare center's on-screen test.
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(place, camera))
            return false;
        place.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
        state.Action = YellowPipeBugAction.WaitForSamus;
        state.ArcCompleted = false;
        state.Playback = EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(
            place.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight));
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        return true;
    }

    /// <summary>Applies the separately authored positive or negative 1/65536-pixel flight speed.</summary>
    private static void AddYellowPipeBugHorizontalVelocity(
        RoomEnemyPlace place,
        YellowPipeBugState state,
        bool movingLeft)
    {
        place.Position = place.Position.AdvanceX(movingLeft ? -state.FlightSpeed : state.FlightSpeed);
    }

    /// <summary>Adds a linear-speed velocity to one axis, carrying the fraction into the whole.</summary>
    private static void AddPipeBugLinearVelocity(
        RoomEnemyPlace place,
        Axis axis,
        SignedSixteenSixteen velocity)
    {
        place.Position = axis == Axis.Horizontal ? place.Position.AdvanceX(velocity) : place.Position.AdvanceY(velocity);
    }

    /// <summary>Adds a wrapped signed 1/65536-pixel displacement to one position axis.</summary>
    private static void AddPipeBugDisplacement(
        RoomEnemyPlace place,
        Axis axis,
        SignedSixteenSixteen displacement)
    {
        if (axis == Axis.Horizontal)
        {
            place.Position = place.Position.WithX(place.Position.X.MovedBy(displacement));
        }
        else
        {
            place.Position = place.Position.WithY(place.Position.Y.MovedBy(displacement));
        }
    }

    /// <summary>The playback of <paramref name="segment"/>'s animation restarted at timer one, its animation frame timer cleared.</summary>
    internal static AnimationPlayback<PipeBugSegment> StartPipeBugAnimation(RoomEnemyPlace place, PipeBugSegment segment) =>
        EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(segment));

    /// <summary>Advances a Brinstar Pipe Bug's typed animation.</summary>
    internal static void AdvanceBrinstarPipeBugTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, PipeBugAnimations.Segments,
            static (BrinstarPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));

    /// <summary>Advances a Norfair Pipe Bug's typed animation.</summary>
    internal static void AdvanceNorfairPipeBugTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, PipeBugAnimations.Segments,
            static (NorfairPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));

    /// <summary>Advances a yellow Pipe Bug's typed animation.</summary>
    internal static void AdvanceYellowPipeBugTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, PipeBugAnimations.Segments,
            static (YellowPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));

    /// <summary>Rejects a cue: no Pipe Bug animation raises one.</summary>
    private static AnimationContinuation<PipeBugSegment>? RaisesNoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"A Pipe Bug animation raised {cue}, though its lists raise no events.");

    /// <summary>Runs a Brinstar Pipe Bug's main AI.</summary>
    internal static void RunBrinstarPipeBugMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunBrinstarPipeBug(place, place.RequireSpeciesState<BrinstarPipeBugState>(), frame.Samus, frame.Camera);

    /// <summary>Runs a Norfair Pipe Bug's main AI.</summary>
    internal void RunNorfairPipeBugMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunNorfairPipeBug(place, place.RequireSpeciesState<NorfairPipeBugState>(), frame.Samus, frame.Camera);

    /// <summary>Runs a yellow Pipe Bug's main AI.</summary>
    internal static void RunYellowPipeBugMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunYellowPipeBug(place, place.RequireSpeciesState<YellowPipeBugState>(), frame.Samus, frame.Camera);
}
