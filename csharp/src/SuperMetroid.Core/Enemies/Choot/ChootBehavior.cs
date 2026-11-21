using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The Choot species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ChootBehavior : IEnemySpecies, IEnemySpawnFactory<ChootSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Choot];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, ChootSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new ChootEntity(occupant, place, common, settings));

    /// <summary>Initializes the Choot, selecting a falling pattern and calculating ascent speed from parameters.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static ChootEnemyState InitializeChoot(ChootSpawnSettings chootSpawnSettings, RoomEnemyPlace place)
    {
        ChootFallingPath fallingPath = chootSpawnSettings.FallingPath;
        var state = new ChootEnemyState(ChootAction.WaitingForSamus);

        SetChootAnimation(place, state, ChootSegment.Idle);
        state.SpawnPosition = place.Position.Whole;

        // The jump rises as far as its loops of the falling path fall: one to seven loops of under 256 pixels in every
        // room, so the height stays well within the room (#1620).
        int jumpHeight = chootSpawnSettings.PatternLoops * ChootFalls.LoopYDistance(fallingPath);
        state.InitialYSpeedCurvePlace = CalculateInitialChootYSpeedCurvePlace(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(state.SpawnPosition.Y - jumpHeight);
        state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
        return state;
    }

    /// <summary>What <see cref="RunChootMain"/> does for each <see cref="ChootAction"/> member, each member's own arm.</summary>
    private static IdentityTable<ChootAction, Action<ChootSpawnSettings, RoomEnemyPlace, ChootEnemyState, SamusState>> RunChootMainForChootAction => field ??= new(new Dictionary<ChootAction, Action<ChootSpawnSettings, RoomEnemyPlace, ChootEnemyState, SamusState>>
    {
        [ChootAction.WaitingForSamus] = static (settings, place, state, samus) => WaitForChootActivation(settings, place, state, samus),
        [ChootAction.PreparingJump] = static (_, place, state, samus) => PrepareChootJump(place, state),
        [ChootAction.Jumping] = static (settings, place, state, samus) => RunChootJump(settings, place, state),
        [ChootAction.Falling] = static (settings, place, state, samus) => RunChootFall(settings, place, state),
    });

    /// <summary>Runs the Choot's wait, preparation, jump or fall.</summary>
    private static void RunChootMain(
        ChootSpawnSettings chootSpawnSettings, RoomEnemyPlace place,
        ChootEnemyState state,
        SamusState samus)
    {
        RunChootMainForChootAction[state.Action](chootSpawnSettings, place, state, samus);
    }

    /// <summary>Arms the jump delay when Samus comes within the activation distance horizontally.</summary>
    private static void WaitForChootActivation(
        ChootSpawnSettings chootSpawnSettings, RoomEnemyPlace place,
        ChootEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= ChootMotionDefinitions.ActivationDistance)
            return;

        state.JumpDelayTimer = UpdateCountdown.For(chootSpawnSettings.JumpDelay + EnemyWaitDefinitions.RunOutUpdate);
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
    private static void RunChootJump(ChootSpawnSettings chootSpawnSettings, RoomEnemyPlace place, ChootEnemyState state)
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
        state.FallingLoop = ChootFallLoop.Starting(chootSpawnSettings.FallingPath);
        // A population loop count of zero falls one loop, as a count of one does.
        state.FallingPatternLoopCounter = Math.Max(1, chootSpawnSettings.PatternLoops);
        SetChootAnimation(place, state, ChootSegment.Falling);
        state.Action = ChootAction.Falling;
    }

    /// <summary>Follows authored path offsets, advancing its origin at each loop's end and restoring the exact spawn state after the final loop.</summary>
    private static void RunChootFall(ChootSpawnSettings chootSpawnSettings, RoomEnemyPlace place, ChootEnemyState state)
    {
        if (state.FallingLoop is ChootFallLoop.AtSample at)
        {
            place.Position = place.Position.WithWhole(state.FallingOrigin + at.Offset);
            state.FallingLoop = at.Counted();
            return;
        }

        ChootFallingPath fallingPath = chootSpawnSettings.FallingPath;
        state.FallingOrigin = state.FallingOrigin.WithY(state.FallingOrigin.Y + ChootFalls.LoopYDistance(fallingPath));
        state.FallingLoop = ChootFallLoop.Starting(fallingPath);
        if (--state.FallingPatternLoopCounter > 0)
            return;

        // The last loop's end restores the exact spawn position, with no fraction of a pixel, restarts
        // the idle animation, and rearms the same calculated ascent for the next activation.
        state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
        place.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
        SetChootAnimation(place, state, ChootSegment.Idle);
        state.Action = ChootAction.WaitingForSamus;
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
    internal static void AdvanceChootTypedAnimation(RoomEnemyPlace place, ChootEnemyState chootEnemyState) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, chootEnemyState, ChootAnimations.Segments,
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
    internal static void RunChootMainAi(ChootSpawnSettings chootSpawnSettings, RoomEnemyPlace place, ChootEnemyState chootEnemyState, IEnemyAiFrame frame)
    {
        RunChootMain(chootSpawnSettings, place, chootEnemyState, frame.Samus);
    }
}
