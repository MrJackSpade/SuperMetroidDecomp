using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The Choot species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ChootBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Choot];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ChootEntity(place, common);

    /// <summary>Samus must be within 80 horizontal pixels for a waiting Choot to arm its jump timer.</summary>
    private const int ChootActivationDistance = 80;

    /// <summary>
    /// How far the ascent moves back along the quadratic speed curve each jumping update, two whole marks; the starting
    /// place is found by moving on by the same step.
    /// </summary>
    private static readonly QuadraticSpeedCurvePlace ChootAscentStep = QuadraticSpeedCurvePlace.FromMarks(2);

    /// <summary>Initializes the Choot, selecting a falling pattern and calculating ascent speed from parameters.</summary>
    internal static void InitializeChoot(RoomEnemyPlace place)
    {
        ChootFallingPath fallingPath = place.SpawnSettings<ChootSpawnSettings>().FallingPath;
        var state = new ChootEnemyState(ChootAction.WaitingForSamus, fallingPath);
        place.SetSpeciesState(state);

        SetChootAnimation(place, state, ChootSegment.Idle);
        state.SpawnPosition = place.Position.Whole;

        state.FallingPatternYDistance = ChootFalls.LoopYDistance(fallingPath);

        // The jump rises as far as its loops of the falling path fall: one to seven loops of under 256 pixels in every
        // retail room, so the height stays well within the room (#1620).
        int jumpHeight = place.SpawnSettings<ChootSpawnSettings>().PatternLoops * state.FallingPatternYDistance;
        state.InitialYSpeedCurvePlace = CalculateInitialChootYSpeedCurvePlace(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(CoordinateAxis.Wrap(
            state.SpawnPosition.Y - jumpHeight));
        state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
    }

    /// <summary>Runs the Choot's main AI and its four wait, prepare, jump, and fall targets.</summary>
    private static void RunChootMain(
        RoomEnemyPlace place,
        ChootEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case ChootAction.WaitingForSamus:
                WaitForChootActivation(place, state, samus);
                return;
            case ChootAction.PreparingJump:
                PrepareChootJump(place, state);
                return;
            case ChootAction.Jumping:
                RunChootJump(place, state);
                return;
            case ChootAction.Falling:
                RunChootFall(place, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Choot has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Arms the jump delay when Samus comes within the activation distance horizontally.</summary>
    private static void WaitForChootActivation(
        RoomEnemyPlace place,
        ChootEnemyState state,
        SamusState samus)
    {
        if (CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) >= ChootActivationDistance)
            return;

        state.JumpDelayTimer = place.SpawnSettings<ChootSpawnSettings>().JumpDelay;
        state.Action = ChootAction.PreparingJump;
    }

    /// <summary>Decrements the delay until it turns negative, then starts the jumping animation.</summary>
    internal static void PrepareChootJump(RoomEnemyPlace place, ChootEnemyState state)
    {
        if (--state.JumpDelayTimer >= 0)
            return;

        SetChootAnimation(place, state, ChootSegment.Jumping);
        state.Action = ChootAction.Jumping;
    }

    /// <summary>Applies quadratic ascent without room collision and snaps whole coordinates to the calculated falling origin at the apex.</summary>
    internal static void RunChootJump(RoomEnemyPlace place, ChootEnemyState state)
    {
        // Choot applies the curve's separately stored negative pair, including its fractional
        // negation bug, without consulting room collision during this authored leap.
        SignedSixteenSixteen upwardDisplacement = state.YSpeedCurvePlace.Speed.Negative;
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(upwardDisplacement));

        state.YSpeedCurvePlace -= ChootAscentStep;
        if (!state.YSpeedCurvePlace.IsNegative)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // subpositions deliberately survive until the final spawn reset, just as in the game.
        place.Position = place.Position.WithWhole(state.InitialFallingPosition);
        state.FallingOrigin = state.InitialFallingPosition;
        state.FallingPatternFrame = 0;
        // A population loop count of zero starts below zero, so the fall ends at the first loop's end as a count of one does.
        state.FallingPatternLoopCounter = place.SpawnSettings<ChootSpawnSettings>().PatternLoops - 1;
        SetChootAnimation(place, state, ChootSegment.Falling);
        state.Action = ChootAction.Falling;
    }

    /// <summary>Follows authored path offsets, advancing its origin at each loop's end and restoring the exact spawn state after the final loop.</summary>
    internal static void RunChootFall(RoomEnemyPlace place, ChootEnemyState state)
    {
        Offset16<RoomPixels>? offset = ChootFalls.At(state.FallingPath, state.FallingPatternFrame);
        if (offset is not { } pathOffset)
        {
            state.FallingOrigin = state.FallingOrigin.WithY(CoordinateAxis.Wrap(
                state.FallingOrigin.Y + state.FallingPatternYDistance));
            state.FallingPatternFrame = 0;
            if (--state.FallingPatternLoopCounter >= 0)
                return;

            // The last loop's end restores exact spawn coordinates/subpositions, reinstalls
            // the idle animation, and rearms the same calculated ascent for the next activation.
            state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
            place.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
            SetChootAnimation(place, state, ChootSegment.Idle);
            state.Action = ChootAction.WaitingForSamus;
            return;
        }

        place.Position = place.Position.WithWhole(state.FallingOrigin + pathOffset);
        state.FallingPatternFrame++;
    }

    /// <summary>
    /// Calculates the Choot's initial jump speed, integrating every second quadratic step until
    /// they cover the requested height. The curve ends at its last mark, 94, so the result is an even mark from 2 through 94.
    /// </summary>
    private static QuadraticSpeedCurvePlace CalculateInitialChootYSpeedCurvePlace(int jumpHeight)
    {
        FixedCoordinate accumulatedDistance = default;
        for (QuadraticSpeedCurvePlace curvePlace = ChootAscentStep; curvePlace <= QuadraticSpeedCurvePlace.LastMark; curvePlace += ChootAscentStep)
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
    internal static void AdvanceChootTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, ChootAnimations.Segments,
            static (ChootEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyChootCue(place, cue) is { } target
                ? new AnimationContinuation<ChootSegment>(ChootAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Carries out one cue of Choot's animation: each switches off-screen processing on or off.</summary>
    private static ChootSegment? ApplyChootCue(RoomEnemyPlace place, ChootCue cue)
    {
        place.Processing = cue.Kind switch
        {
            ChootCueKind.ProcessOffScreen => place.Processing.With(EnemyProcessing.OffScreen),
            ChootCueKind.StopProcessingOffScreen => place.Processing.Without(EnemyProcessing.OffScreen),
            _ => throw new InvalidOperationException($"Undefined {nameof(ChootCueKind)} {cue.Kind}."),
        };
        return null;
    }

    /// <summary>Runs Choot's main AI.</summary>
    internal static void RunChootMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunChootMain(place, place.RequireSpeciesState<ChootEnemyState>(), frame.Samus);
    }
}
