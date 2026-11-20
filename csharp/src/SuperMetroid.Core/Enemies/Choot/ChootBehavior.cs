using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The Choot species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ChootBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Choot], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new ChootEntity(occupant, place, common));

    /// <summary>Initializes the Choot, selecting a falling pattern and calculating ascent speed from parameters.</summary>
    internal static void InitializeChoot(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        ChootFallingPath fallingPath = occupant.SpawnSettings<ChootSpawnSettings>().FallingPath;
        var state = new ChootEnemyState(ChootAction.WaitingForSamus);
        place.SetSpeciesState(state);

        SetChootAnimation(place, state, ChootSegment.Idle);
        state.SpawnPosition = place.Position.Whole;

        // The jump rises as far as its loops of the falling path fall: one to seven loops of under 256 pixels in every
        // room, so the height stays well within the room (#1620).
        int jumpHeight = occupant.SpawnSettings<ChootSpawnSettings>().PatternLoops * ChootFalls.LoopYDistance(fallingPath);
        state.InitialYSpeedCurvePlace = CalculateInitialChootYSpeedCurvePlace(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(state.SpawnPosition.Y - jumpHeight);
        state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
    }

    /// <summary>What <c>RunChootMain</c> does for each <see cref="ChootAction"/> member, each member's own arm.</summary>
    private static IdentityTable<ChootAction, Action<RoomEnemyOccupant, RoomEnemyPlace, ChootEnemyState, SamusState>> RunChootMainForChootAction => field ??= new(new Dictionary<ChootAction, Action<RoomEnemyOccupant, RoomEnemyPlace, ChootEnemyState, SamusState>>
    {
        [ChootAction.WaitingForSamus] = static (occupant, place, state, samus) => WaitForChootActivation(occupant, place, state, samus),
        [ChootAction.PreparingJump] = static (_, place, state, samus) => PrepareChootJump(place, state),
        [ChootAction.Jumping] = static (occupant, place, state, samus) => RunChootJump(occupant, place, state),
        [ChootAction.Falling] = static (occupant, place, state, samus) => RunChootFall(occupant, place, state),
    });

    /// <summary>Runs the Choot's wait, preparation, jump or fall.</summary>
    private static void RunChootMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        ChootEnemyState state,
        SamusState samus)
    {
        RunChootMainForChootAction[state.Action](occupant, place, state, samus);
    }

    /// <summary>Arms the jump delay when Samus comes within the activation distance horizontally.</summary>
    private static void WaitForChootActivation(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        ChootEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= ChootMotionDefinitions.ActivationDistance)
            return;

        state.JumpDelayTimer = UpdateCountdown.For(occupant.SpawnSettings<ChootSpawnSettings>().JumpDelay + EnemyWaitDefinitions.RunOutUpdate);
        state.Action = ChootAction.PreparingJump;
    }

    /// <summary>Waits out the jump delay, then starts the jumping animation.</summary>
    private static void PrepareChootJump(RoomEnemyPlace place, ChootEnemyState state)
    {
        if (!state.JumpDelayTimer.Step())
            return;

        SetChootAnimation(place, state, ChootSegment.Jumping);
        state.Action = ChootAction.Jumping;
    }

    /// <summary>Applies quadratic ascent without room collision and snaps whole coordinates to the calculated falling origin at the apex.</summary>
    private static void RunChootJump(RoomEnemyOccupant occupant, RoomEnemyPlace place, ChootEnemyState state)
    {
        // The Choot rises at its place's speed on the curve, passing through the room's blocks during this leap.
        SignedSixteenSixteen upwardDisplacement = state.YSpeedCurvePlace.Speed.Negative;
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(upwardDisplacement));

        state.YSpeedCurvePlace -= ChootMotionDefinitions.AscentStep;
        if (!state.YSpeedCurvePlace.IsNegative)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // where it lies within a pixel stays until the final spawn reset, just as in the game.
        place.Position = place.Position.WithWhole(state.InitialFallingPosition);
        state.FallingOrigin = state.InitialFallingPosition;
        state.FallingLoop = ChootFallLoop.Of(occupant.SpawnSettings<ChootSpawnSettings>().FallingPath);
        // A population loop count of zero falls one loop, as a count of one does.
        state.FallingPatternLoopCounter = Math.Max(1, occupant.SpawnSettings<ChootSpawnSettings>().PatternLoops);
        SetChootAnimation(place, state, ChootSegment.Falling);
        state.Action = ChootAction.Falling;
    }

    /// <summary>Follows authored path offsets, advancing its origin at each loop's end and restoring the exact spawn state after the final loop.</summary>
    private static void RunChootFall(RoomEnemyOccupant occupant, RoomEnemyPlace place, ChootEnemyState state)
    {
        ChootFallingPath fallingPath = occupant.SpawnSettings<ChootSpawnSettings>().FallingPath;
        if (!state.FallingLoop.TryShowNext(out Offset16<RoomPixels> pathOffset))
        {
            state.FallingOrigin = state.FallingOrigin.WithY(state.FallingOrigin.Y + ChootFalls.LoopYDistance(fallingPath));
            state.FallingLoop = ChootFallLoop.Of(fallingPath);
            if (--state.FallingPatternLoopCounter > 0)
                return;

            // The last loop's end restores the exact spawn position, with no fraction of a pixel, restarts the idle
            // animation, and rearms the same calculated ascent for the next activation.
            state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
            place.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
            SetChootAnimation(place, state, ChootSegment.Idle);
            state.Action = ChootAction.WaitingForSamus;
            return;
        }

        place.Position = place.Position.WithWhole(state.FallingOrigin + pathOffset);
    }

    /// <summary>
    /// Calculates the Choot's initial jump speed, integrating the quadratic steps the ascent takes
    /// (<see cref="ChootMotionDefinitions.AscentStep"/>) until they cover the requested height, at most the curve's last mark.
    /// </summary>
    private static QuadraticSpeedCurvePlace CalculateInitialChootYSpeedCurvePlace(int jumpHeight)
    {
        FixedCoordinate accumulatedDistance = default;
        for (QuadraticSpeedCurvePlace curvePlace = ChootMotionDefinitions.AscentStep; curvePlace <= QuadraticSpeedCurvePlace.LastMark; curvePlace += ChootMotionDefinitions.AscentStep)
        {
            accumulatedDistance = accumulatedDistance.MovedBy(curvePlace.Speed.Positive);
            if (accumulatedDistance.Whole >= jumpHeight)
                return curvePlace;
        }

        throw new InvalidDataException(
            $"Choot jump-height calculation did not reach {jumpHeight} pixels.");
    }

    /// <summary>Installs the idle, jumping, or falling animation and restarts its frame timers.</summary>
    private static void SetChootAnimation(
        RoomEnemyPlace place,
        ChootEnemyState state,
        ChootSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, ChootAnimations.StartingWith(segment));
    }

    /// <summary>Advances Choot's typed animation.</summary>
    internal static void AdvanceChootTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, ChootAnimations.Segments,
            static (ChootEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (ChootEnemyState _, ChootCue cue) => ApplyChootCue(place, cue));

    /// <summary>Carries out one cue of Choot's animation: each switches off-screen processing on or off.</summary>
    private static void ApplyChootCue(RoomEnemyPlace place, ChootCue cue)
    {
        place.Processing = cue.Kind switch
        {
            ChootCueKind.ProcessOffScreen => place.Processing.With(EnemyProcessing.OffScreen),
            ChootCueKind.StopProcessingOffScreen => place.Processing.Without(EnemyProcessing.OffScreen),
            _ => throw new InvalidOperationException($"Undefined {nameof(ChootCueKind)} {cue.Kind}."),
        };
    }

    /// <summary>Runs Choot's main AI.</summary>
    internal static void RunChootMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunChootMain(occupant, place, place.RequireSpeciesState<ChootEnemyState>(), frame.Samus);
    }
}
