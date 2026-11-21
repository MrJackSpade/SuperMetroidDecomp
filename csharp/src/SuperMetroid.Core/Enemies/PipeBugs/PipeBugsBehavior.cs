using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The pipe bugs species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PipeBugsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>A Brinstar Pipe Bug (Zeb or Zebbo), a Norfair one (Gamet) or a yellow one (Geega) (#2146).</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void Register(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<BrinstarPipeBugSpawnSettings>([EnemyDefinitionId.Zeb, EnemyDefinitionId.Zebbo], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new BrinstarPipeBugEntity(occupant, place, common, settings)));
        registrar.Spawns<NorfairPipeBugSpawnSettings>([EnemyDefinitionId.Gamet], (occupant, settings, place, common) =>
            settings is NorfairPipeBugLeaderSpawnSettings leader
                ? EnemyOccupant.Running(new NorfairPipeBugLeaderEntity(this, occupant, place, common, leader))
                : EnemyOccupant.Running(new NorfairPipeBugFollowerEntity(this, occupant, place, common, settings)));
        registrar.Spawns<YellowPipeBugSpawnSettings>([EnemyDefinitionId.Geega], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new YellowPipeBugEntity(occupant, place, common, settings)));
    }

    /// <summary>The room's enemy places, where each Norfair formation leader finds its formation when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>
    /// The five-bug formation each Norfair leader found when it was placed, under the leader's place: itself, then the four
    /// bugs its room lists right after it, each in its role (#1840). The last member's placement checks the formation once.
    /// </summary>
    private readonly Dictionary<RoomEnemyPlace, NorfairPipeBugFormation> _formations = [];

    /// <summary>Binds the shared enemy pool needed to validate Norfair formation membership.</summary>
    internal PipeBugsBehavior(EnemyPlaces places)
    {
        _places = places;
    }

    /// <summary>Initializes the Brinstar Pipe Bug.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static BrinstarPipeBugState InitializeBrinstarPipeBug(RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings, RoomEnemyPlace place)
    {
        var state = new BrinstarPipeBugState(BrinstarPipeBugAction.WaitUntilOnScreen);
        state.SpawnPosition = place.Position.Whole;
        state.EmergenceTopY = state.SpawnPosition.Y - PipeBugDefinitions.BrinstarEmergenceHeight;
        state.RespawnDelay = UpdateCountdown.For(PipeBugDefinitions.BrinstarRespawnWait);
        state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(
            PipeBugAnimationChoices.Rising(KindOf(occupant, brinstarPipeBugSpawnSettings), HorizontalDirection.Left)), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>Which Brinstar Pipe Bug <paramref name="occupant"/> is: the ordinary Zeb or the strong Zebbo.</summary>
    private static BrinstarPipeBugKind KindOf(RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings) =>
        brinstarPipeBugSpawnSettings.Kind;

    /// <summary>
    /// Initializes the Norfair Pipe Bug. The last member of a leader's formation, placed last, completes the formation,
    /// which is checked once then.
    /// </summary>
    /// <exception cref="InvalidDataException">The formation's last member completes a formation whose members are not all Norfair Pipe Bugs.</exception>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal NorfairPipeBugState InitializeNorfairPipeBug(RoomEnemyPlace place)
    {
        CheckNorfairPipeBugFormationCompletedBy(place);
        var state = new NorfairPipeBugState(NorfairPipeBugAction.WaitForFormation);
        state.SpawnPosition = place.Position.Whole;
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        state.Playback = EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(PipeBugSegment.GametRisingLeft));
        return state;
    }

    /// <summary>
    /// The formation the leader in <paramref name="leader"/> leads, which it finds as it is placed: itself and the four
    /// bugs its room lists after it, the leader's settings alone having a trigger width.
    /// </summary>
    internal NorfairPipeBugFormation LeadFormation(RoomEnemyPlace leader)
    {
        NorfairPipeBugFormation formation = NorfairPipeBugFormation.LedBy(_places, leader);
        _formations[leader] = formation;
        return formation;
    }

    /// <summary>Initializes the yellow Brinstar Pipe Bug.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static YellowPipeBugState InitializeYellowPipeBug(RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place)
    {
        var state = new YellowPipeBugState(YellowPipeBugAction.WaitForSamus);
        state.SpawnPosition = place.Position.Whole;
        place.Position = place.Position.WithoutFractions();
        state.Playback = EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(
            yellowPipeBugSpawnSettings.Facing == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight));
        return state;
    }

    /// <summary>Runs a Brinstar Pipe Bug's action.</summary>
    private static void RunBrinstarPipeBug(
        RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings, RoomEnemyPlace place,
        BrinstarPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case BrinstarPipeBugAction.WaitUntilOnScreen:
                // The bug starts watching for Samus once its pipe is on screen.
                if (EnemyProcessingWindow.IsWithinProcessingWindow(place, camera))
                    state.Action = BrinstarPipeBugAction.WaitForSamus;
                return;
            case BrinstarPipeBugAction.WaitForSamus:
                RunBrinstarPipeBugWaiting(occupant, brinstarPipeBugSpawnSettings, place, state, samus);
                return;
            case BrinstarPipeBugAction.Emerge:
                RunBrinstarPipeBugEmergence(occupant, brinstarPipeBugSpawnSettings, place, state, samus);
                return;
            case BrinstarPipeBugAction.FlyHorizontally:
                RunBrinstarPipeBugFlight(occupant, brinstarPipeBugSpawnSettings, place, state, camera.X);
                return;
            case BrinstarPipeBugAction.RespawnDelay:
                if (state.RespawnDelay.StepEnds())
                    state.Action = BrinstarPipeBugAction.WaitForSamus;
                return;
            default:
                throw new InvalidDataException(
                    $"Brinstar Pipe Bug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Runs a Norfair Pipe Bug formation leader's action: it waits for its formation and then for Samus, which only the
    /// leader does, and otherwise acts as every member does.
    /// </summary>
    private void RunNorfairPipeBugLeader(
        NorfairPipeBugLeaderSpawnSettings settings, NorfairPipeBugFormation formation, RoomEnemyPlace place,
        NorfairPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case NorfairPipeBugAction.WaitForFormation:
                RunNorfairPipeBugFormationWait(formation, state);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.WaitForSamus:
                RunNorfairPipeBugSamusWait(settings, formation, place, state, samus);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            default:
                RunNorfairPipeBug(settings, place, state, samus, camera);
                return;
        }
    }

    /// <summary>
    /// Runs a Norfair Pipe Bug's action as every member of a formation runs it; a follower waits for its leader to launch
    /// it, and only the leader waits for Samus (<see cref="RunNorfairPipeBugLeader"/>).
    /// </summary>
    private void RunNorfairPipeBug(
        NorfairPipeBugSpawnSettings norfairPipeBugSpawnSettings, RoomEnemyPlace place,
        NorfairPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case NorfairPipeBugAction.WaitForFormation:
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
                RunNorfairPipeBugVerticalStagger(place, state, state.Role is NorfairPipeBugRole.Follower { Stagger: var stagger }
                    ? stagger
                    : throw new InvalidOperationException("A Norfair Pipe Bug staggers in a role that holds its height."));
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.WaitForStagger:
                RunNorfairPipeBugStaggerWait(place, state, samus);
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            case NorfairPipeBugAction.Fly:
                // A departed member flies at its spawn settings' speed toward Samus's side.
                SignedSixteenSixteen flightSpeed = norfairPipeBugSpawnSettings.Speed;
                AddPipeBugLinearVelocity(place, axis: Axis.Horizontal,
                    flightSpeed.Toward(state.FlightDirection));
                ResetNorfairPipeBugIfOffScreen(place, state, camera);
                return;
            default:
                throw new InvalidDataException(
                    $"Norfair Pipe Bug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs a yellow Pipe Bug's action.</summary>
    private static void RunYellowPipeBug(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
        YellowPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case YellowPipeBugAction.WaitForSamus:
                RunYellowPipeBugWaiting(occupant, yellowPipeBugSpawnSettings, place, state, samus);
                return;
            case YellowPipeBugAction.EmergenceDelay:
                RunYellowPipeBugDelay(occupant, yellowPipeBugSpawnSettings, place, state);
                return;
            case YellowPipeBugAction.Fly:
                RunYellowPipeBugStraightFlight(occupant, yellowPipeBugSpawnSettings, place, state, samus, camera, YellowPipeBugFacing(occupant, yellowPipeBugSpawnSettings));
                return;
            case YellowPipeBugAction.ArcDipping:
                RunYellowPipeBugArcDip(occupant, yellowPipeBugSpawnSettings, place, state, camera, YellowPipeBugFacing(occupant, yellowPipeBugSpawnSettings));
                return;
            case YellowPipeBugAction.ArcClimbing:
                RunYellowPipeBugArcClimb(occupant, yellowPipeBugSpawnSettings, place, state, camera, YellowPipeBugFacing(occupant, yellowPipeBugSpawnSettings));
                return;
            default:
                throw new InvalidDataException(
                    $"Yellow Pipe Bug has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Wakes a hidden Brinstar bug only when Samus enters its narrow horizontal and upper vertical band.</summary>
    private static void RunBrinstarPipeBugWaiting(
        RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings, RoomEnemyPlace place,
        BrinstarPipeBugState state,
        SamusState samus)
    {
        int deltaY = samus.Position.Y.Whole - place.Position.Y.Whole;
        if (deltaY is >= 0 or < -PipeBugDefinitions.BrinstarTriggerTop)
            return;

        state.Flight = (samus.Position.Whole - place.Position.Whole).X < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        if (Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) >=
            PipeBugDefinitions.BrinstarTriggerWidth)
        {
            return;
        }

        state.Action = BrinstarPipeBugAction.Emerge;
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        SelectBrinstarPipeBugAnimation(place, state, PipeBugAnimationChoices.Rising(
            KindOf(occupant, brinstarPipeBugSpawnSettings), state.Flight));
    }

    /// <summary>Rises (<see cref="PipeBugEmergenceRise"/>) until the bug is above Samus, then switches to horizontal flight.</summary>
    private static void RunBrinstarPipeBugEmergence(
        RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings, RoomEnemyPlace place,
        BrinstarPipeBugState state,
        SamusState samus)
    {
        place.Position = place.Position.WithY(PipeBugEmergenceRise.Risen(place.Position.Y));
        if (state.EmergenceTopY < place.Position.Y.Whole ||
            place.Position.Y.Whole >= samus.Position.Y.Whole)
        {
            return;
        }

        // The bug shoots facing the way it rose: toward the side Samus was on as it emerged.
        SelectBrinstarPipeBugAnimation(place, state, PipeBugAnimationChoices.Shooting(
            KindOf(occupant, brinstarPipeBugSpawnSettings), state.Flight));
        state.Action = BrinstarPipeBugAction.FlyHorizontally;
    }

    /// <summary>Moves its flight step each frame, then restores the stored spawn position and starts an invisible respawn wait offscreen.</summary>
    private static void RunBrinstarPipeBugFlight(
        RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings, RoomEnemyPlace place,
        BrinstarPipeBugState state,
        int cameraX)
    {
        // The flight runs until the bug leaves the screen sideways. Flying left with the camera at the room's left edge, it
        // crosses the edge and continues from the far end of the room's coordinates, which the despawn test reads as
        // left of the room, so this room wrap is the flight's end (#2373).
        place.Position = OffRoomPlaces.WithWholeX(place.Position, place.Position.X.Whole +
            (PipeBugDefinitions.BrinstarFlightStep.Toward(state.Flight)));
        // The despawn tests only the horizontal screen extent.
        if (!EnemyProcessingWindow.EnemyIsHorizontallyOffScreen(place, cameraX))
            return;

        place.Position = place.Position.WithWhole(state.SpawnPosition).WithFractionX(PixelFraction.None)
            .WithY(PipeBugEmergenceRise.Respawned(state.SpawnPosition.Y));
        SelectBrinstarPipeBugAnimation(place, state, PipeBugAnimationChoices.Rising(KindOf(occupant, brinstarPipeBugSpawnSettings), HorizontalDirection.Left));
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        state.RespawnDelay = UpdateCountdown.For(PipeBugDefinitions.BrinstarRespawnWait);
        state.Action = BrinstarPipeBugAction.RespawnDelay;
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one playing, which keeps playing (#2145).</summary>
    private static void SelectBrinstarPipeBugAnimation(
        RoomEnemyPlace place,
        BrinstarPipeBugState state,
        PipeBugSegment animation) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, PipeBugAnimations.StartingWith(animation));

    /// <summary>Arms the Norfair leader only after all five consecutive formation members remain dormant.</summary>
    private static void RunNorfairPipeBugFormationWait(NorfairPipeBugFormation formation, NorfairPipeBugState state)
    {
        // Only the formation's leader has a trigger width. It leads the formation and may
        // arm itself only while all four Pipe Bugs listed after it are still at the
        // dormant action. It reads each member's place whatever it holds: a member killed and not yet respawned has a
        // cleared place, which never reads as waiting, so it holds the formation back until its respawn sets it up again.
        if (formation.Members.All(static member =>
                member.Place.FoundEntity is NorfairPipeBugEntity { State.Action: NorfairPipeBugAction.WaitForFormation }))
        {
            state.Action = NorfairPipeBugAction.WaitForSamus;
        }
    }

    /// <summary>Launches the five-bug rise when Samus enters the leader's trigger, retaining follower frame timers.</summary>
    private static void RunNorfairPipeBugSamusWait(
        NorfairPipeBugLeaderSpawnSettings norfairPipeBugLeaderSpawnSettings, NorfairPipeBugFormation formation, RoomEnemyPlace leader,
        NorfairPipeBugState leaderState,
        SamusState samus)
    {
        int triggerWidth = norfairPipeBugLeaderSpawnSettings.TriggerWidth;
        if (Math.Abs(leader.Position.X.Whole - samus.Position.X.Whole) >= triggerWidth ||
            leader.Position.Y.Whole < samus.Position.Y.Whole)
        {
            return;
        }

        // Only the leader's animation clock restarts. The formation then changes every member's animation but leaves
        // the followers' frame timers as they are, a killed member's included.
        leader.AnimationClock.Restart();
        bool samusIsRight = samus.Position.X.Whole >= leader.Position.X.Whole;
        // A member killed since the formation armed has nothing to launch: it respawns waiting, as a new bug.
        foreach (NorfairPipeBugMember member in formation.Members)
        {
            if (member.Place.Entity is not NorfairPipeBugEntity { State: var memberState })
                continue;
            memberState.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(samusIsRight
                ? PipeBugSegment.GametRisingRight
                : PipeBugSegment.GametRisingLeft), AnimationPlace.StartOfAnimation);
            memberState.Action = NorfairPipeBugAction.Rise;
            memberState.Role = member.Role;
        }
        // The launch counts one formation update of the leader's, once its role gives its wait its length.
        leaderState.CountFormationTick();
    }

    /// <summary>Raises one Norfair bug until it passes Samus's Y and selects its authored post-rise action.</summary>
    private static void RunNorfairPipeBugRise(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        SamusState samus)
    {
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        AddPipeBugLinearVelocity(place, axis: Axis.Vertical, -PipeBugDefinitions.NorfairRiseSpeed);
        if (place.Position.Y.Whole >= samus.Position.Y.Whole)
            return;

        state.Action = state.Role.PostRiseAction;
        state.RiseEndY = place.Position.Y.Whole;
        state.Playback = StartPipeBugAnimation(place,
            samus.Position.X.Whole >= place.Position.X.Whole
                ? PipeBugSegment.GametRisingRight
                : PipeBugSegment.GametRisingLeft);
    }

    /// <summary>Moves a risen Norfair member to its place in the formation before its timed flight wait (#2473).</summary>
    private static void RunNorfairPipeBugVerticalStagger(
        RoomEnemyPlace place,
        NorfairPipeBugState state,
        NorfairPipeBugStagger stagger)
    {
        state.CountFormationTick();
        // The member moves its stagger's way at its rise speed. The game clamps exactly at the distance from where its
        // rise ended: one moving up stops once above it, one moving down once at or below it.
        bool up = stagger.Direction == VerticalDirection.Up;
        AddPipeBugLinearVelocity(
            place,
            axis: Axis.Vertical,
            PipeBugDefinitions.NorfairRiseSpeed.Toward(stagger.Direction));
        int targetY = up ? state.RiseEndY - stagger.Distance : state.RiseEndY + stagger.Distance;
        int distancePastTarget = place.Position.Y.Whole - targetY;
        bool reached = up
            ? distancePastTarget < 0
            : distancePastTarget >= 0;
        if (!reached)
            return;
        place.Position = place.Position.WithY(FixedCoordinate.FromWhole(targetY));
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

        state.RestartDeparture();
        place.AnimationClock.Restart();
        state.Action = NorfairPipeBugAction.Fly;
        if (samus.Position.X.Whole >= place.Position.X.Whole)
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
    /// Checks, as <paramref name="place"/> is placed, the formation it completes when it is the last member of a leader's
    /// formation: every member, placed before it, is a Norfair Pipe Bug. A killed member's place, empty until the bug
    /// respawns there, stays the formation's, so the last member's own respawn passes it.
    /// </summary>
    /// <exception cref="InvalidDataException">A member of the formation <paramref name="place"/> completes is not a Norfair Pipe Bug.</exception>
    private void CheckNorfairPipeBugFormationCompletedBy(RoomEnemyPlace place)
    {
        foreach (NorfairPipeBugFormation formation in _formations.Values)
        {
            if (formation.Members[^1].Place != place)
                continue;
            foreach (NorfairPipeBugMember member in formation.Members)
            {
                if (member.Place != place && member.Place.Entity is not NorfairPipeBugEntity && member.Place.Occupant is { } found)
                {
                    throw new InvalidDataException(
                        $"Norfair Pipe Bug formation place {member.Place.Ordinal} holds {found.DefinitionId}, " +
                        $"not a {EnemyDefinitionId.Gamet}.");
                }
            }
        }
    }

    /// <summary>Wakes a yellow bug when Samus is ahead on its authored side and within the narrow vertical band.</summary>
    private static void RunYellowPipeBugWaiting(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
        YellowPipeBugState state,
        SamusState samus)
    {
        int deltaX = samus.Position.X.Whole - place.Position.X.Whole;
        bool inFront = YellowPipeBugFacing(occupant, yellowPipeBugSpawnSettings) == HorizontalDirection.Left
            ? deltaX is < 0 and >= -PipeBugDefinitions.YellowTriggerDistance
            : deltaX is >= 0 and < PipeBugDefinitions.YellowTriggerDistance;
        if (!inFront ||
            Math.Abs(place.Position.Y.Whole - samus.Position.Y.Whole) >= PipeBugDefinitions.YellowTriggerHeight)
        {
            return;
        }

        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        state.EmergenceDelay = UpdateCountdown.For(PipeBugDefinitions.YellowEmergenceDelay);
        state.Action = YellowPipeBugAction.EmergenceDelay;
    }

    /// <summary>Counts down the visible emergence delay, then flies straight toward the bug's facing with its flying animation.</summary>
    private static void RunYellowPipeBugDelay(RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place, YellowPipeBugState state)
    {
        if (!state.EmergenceDelay.StepEnds())
            return;
        place.AnimationClock.Restart();
        state.Playback = new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(YellowPipeBugFacing(occupant, yellowPipeBugSpawnSettings) == HorizontalDirection.Left
            ? PipeBugSegment.GeegaFlyingLeft
            : PipeBugSegment.GeegaFlyingRight), AnimationPlace.StartOfAnimation);
        state.Action = YellowPipeBugAction.Fly;
    }

    /// <summary>Advances yellow flight and begins its single quadratic arc after passing Samus by the threshold.</summary>
    private static void RunYellowPipeBugStraightFlight(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
        YellowPipeBugState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        AddYellowPipeBugHorizontalVelocity(occupant, yellowPipeBugSpawnSettings, place, direction);
        if (ResetYellowPipeBugIfOffScreen(occupant, yellowPipeBugSpawnSettings, place, state, camera))
            return;
        if (state.ArcCompleted)
            return;

        bool passedSamus = direction == HorizontalDirection.Left
            ? place.Position.X.Whole - samus.Position.X.Whole < PipeBugDefinitions.YellowArcTriggerDistance
            : samus.Position.X.Whole - place.Position.X.Whole < PipeBugDefinitions.YellowArcTriggerDistance;
        if (!passedSamus)
            return;

        state.Action = YellowPipeBugAction.ArcDipping;
        state.ArcCurvePlace = PipeBugDefinitions.YellowArcStart;
        state.Playback = StartPipeBugAnimation(place,
            direction == HorizontalDirection.Left
                ? PipeBugSegment.GeegaArcingLeft
                : PipeBugSegment.GeegaArcingRight);
    }

    /// <summary>Dips the bug along the arc's first half, slowing, until its place on the speed curve would pass the start.</summary>
    private static void RunYellowPipeBugArcDip(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
        YellowPipeBugState state,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        if (ResetYellowPipeBugIfOffScreen(occupant, yellowPipeBugSpawnSettings, place, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(occupant, yellowPipeBugSpawnSettings, place, direction);
        // The game moves the place back and ends the dip when it would pass the curve's start, holding it there; the
        // place never passes the curve's start, so the end is tested before the step.
        if (state.ArcCurvePlace == QuadraticSpeedCurvePlace.Start)
        {
            state.Action = YellowPipeBugAction.ArcClimbing;
            return;
        }

        state.ArcCurvePlace -= QuadraticSpeedCurvePlace.OneMark;
        AddPipeBugDisplacement(place, axis: Axis.Vertical,
            state.ArcCurvePlace.Speed.Toward(VerticalDirection.Down));
    }

    /// <summary>Climbs the bug back along the arc's second half, speeding up, until it returns above its spawn height.</summary>
    private static void RunYellowPipeBugArcClimb(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
        YellowPipeBugState state,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        if (ResetYellowPipeBugIfOffScreen(occupant, yellowPipeBugSpawnSettings, place, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(occupant, yellowPipeBugSpawnSettings, place, direction);
        state.ArcCurvePlace += QuadraticSpeedCurvePlace.OneMark;
        AddPipeBugDisplacement(place, axis: Axis.Vertical,
            state.ArcCurvePlace.Speed.Toward(VerticalDirection.Up));
        bool returnedToSpawnY = direction == HorizontalDirection.Left
            ? place.Position.Y.Whole < state.SpawnPosition.Y
            : state.SpawnPosition.Y >= place.Position.Y.Whole;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.Action = YellowPipeBugAction.Fly;
        state.Playback = StartPipeBugAnimation(place,
            direction == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight);
    }

    /// <summary>Hides an offscreen yellow bug, restores its spawn point, and makes its arc available again.</summary>
    private static bool ResetYellowPipeBugIfOffScreen(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
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
            yellowPipeBugSpawnSettings.Facing == HorizontalDirection.Left
                ? PipeBugSegment.GeegaFlyingLeft
                : PipeBugSegment.GeegaFlyingRight));
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        return true;
    }

    /// <summary>
    /// The way a yellow Pipe Bug faces, waits, flies and arcs (#2474): its placement's facing, which its flight never
    /// changes.
    /// </summary>
    private static HorizontalDirection YellowPipeBugFacing(RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings) =>
        yellowPipeBugSpawnSettings.Facing;

    /// <summary>Moves the bug its spawn settings' flight speed toward <paramref name="direction"/>.</summary>
    private static void AddYellowPipeBugHorizontalVelocity(
        RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place,
        HorizontalDirection direction)
    {
        SignedSixteenSixteen flightSpeed = yellowPipeBugSpawnSettings.Speed;
        place.Position = place.Position.AdvanceX(flightSpeed.Toward(direction));
    }

    /// <summary>Adds a linear-speed velocity to one axis, carrying the fraction into the whole.</summary>
    private static void AddPipeBugLinearVelocity(
        RoomEnemyPlace place,
        Axis axis,
        SignedSixteenSixteen velocity)
    {
        place.Position = axis == Axis.Horizontal ? place.Position.AdvanceX(velocity) : place.Position.AdvanceY(velocity);
    }

    /// <summary>Moves the bug along one axis by a displacement.</summary>
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

    /// <summary>The playback of <paramref name="segment"/>'s animation started afresh, its first step playing on the next update.</summary>
    private static AnimationPlayback<PipeBugSegment> StartPipeBugAnimation(RoomEnemyPlace place, PipeBugSegment segment) =>
        EnemyAnimationPlayback.Start(place, PipeBugAnimations.StartingWith(segment));

    /// <summary>Advances a Brinstar Pipe Bug's typed animation.</summary>
    internal static void AdvanceBrinstarPipeBugTypedAnimation(RoomEnemyPlace place, BrinstarPipeBugState brinstarPipeBugState) =>
        EnemyAnimationPlayback.Advance(place, brinstarPipeBugState, PipeBugAnimations.Segments,
            static (BrinstarPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));

    /// <summary>Advances a Norfair Pipe Bug's typed animation.</summary>
    internal static void AdvanceNorfairPipeBugTypedAnimation(RoomEnemyPlace place, NorfairPipeBugState norfairPipeBugState) =>
        EnemyAnimationPlayback.Advance(place, norfairPipeBugState, PipeBugAnimations.Segments,
            static (NorfairPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));

    /// <summary>Advances a yellow Pipe Bug's typed animation.</summary>
    internal static void AdvanceYellowPipeBugTypedAnimation(RoomEnemyPlace place, YellowPipeBugState yellowPipeBugState) =>
        EnemyAnimationPlayback.Advance(place, yellowPipeBugState, PipeBugAnimations.Segments,
            static (YellowPipeBugState state) => state.Playback, static (state, playback) => state.Playback = playback, static (_, cue) => RaisesNoCues(cue));

    /// <summary>Rejects a cue: no Pipe Bug animation raises one.</summary>
    private static AnimationContinuation<PipeBugSegment>? RaisesNoCues(NoAnimationCue cue) =>
        throw new InvalidOperationException($"A Pipe Bug animation raised {cue}, though no Pipe Bug animation has a cue.");

    /// <summary>Runs a Brinstar Pipe Bug's main AI.</summary>
    internal static void RunBrinstarPipeBugMainAi(RoomEnemyOccupant occupant, BrinstarPipeBugSpawnSettings brinstarPipeBugSpawnSettings, RoomEnemyPlace place, BrinstarPipeBugState brinstarPipeBugState, IEnemyAiFrame frame) =>
        RunBrinstarPipeBug(occupant, brinstarPipeBugSpawnSettings, place, brinstarPipeBugState, frame.Samus, frame.Camera);

    /// <summary>Runs a Norfair Pipe Bug formation follower's main AI.</summary>
    internal void RunNorfairPipeBugMainAi(NorfairPipeBugSpawnSettings norfairPipeBugSpawnSettings, RoomEnemyPlace place, NorfairPipeBugState norfairPipeBugState, IEnemyAiFrame frame) =>
        RunNorfairPipeBug(norfairPipeBugSpawnSettings, place, norfairPipeBugState, frame.Samus, frame.Camera);

    /// <summary>Runs a Norfair Pipe Bug formation leader's main AI.</summary>
    internal void RunNorfairPipeBugLeaderMainAi(NorfairPipeBugLeaderSpawnSettings settings, NorfairPipeBugFormation formation, RoomEnemyPlace place,
        NorfairPipeBugState state, IEnemyAiFrame frame) =>
        RunNorfairPipeBugLeader(settings, formation, place, state, frame.Samus, frame.Camera);

    /// <summary>Runs a yellow Pipe Bug's main AI.</summary>
    internal static void RunYellowPipeBugMainAi(RoomEnemyOccupant occupant, YellowPipeBugSpawnSettings yellowPipeBugSpawnSettings, RoomEnemyPlace place, YellowPipeBugState yellowPipeBugState, IEnemyAiFrame frame) =>
        RunYellowPipeBug(occupant, yellowPipeBugSpawnSettings, place, yellowPipeBugState, frame.Samus, frame.Camera);
}
