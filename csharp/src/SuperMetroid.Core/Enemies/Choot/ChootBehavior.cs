using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The Choot species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ChootBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Choot];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ChootEntity(slot, common);

    /// <summary>Samus must be within 80 horizontal pixels for a waiting Choot to arm its jump timer.</summary>
    private const int ChootActivationDistance = 80;

    /// <summary>
    /// How far the ascent moves back along the quadratic speed table each jumping update, two whole rows; the starting
    /// place is found by moving on by the same step.
    /// </summary>
    private static readonly EnemyQuadraticSpeedIndex ChootRowsPerUpdate = EnemyQuadraticSpeedIndex.FromRows(2);

    /// <summary>Initializes the Choot, selecting a falling pattern and calculating ascent speed from parameters.</summary>
    internal static void InitializeChoot(RoomEnemySlot slot)
    {
        var state = new ChootEnemyState();
        slot.SetSpeciesState(state);

        SetChootAnimation(slot, state, ChootSegment.Idle);
        state.Action = ChootAction.WaitingForSamus;
        state.SpawnPosition = slot.Position.Whole;

        ChootFallingPath fallingPath = slot.SpawnSettings<ChootSpawnSettings>().FallingPath;
        state.FallingPath = fallingPath;
        state.FallingPatternYDistance = ChootFalls.LoopYDistance(fallingPath);

        // The jump rises as far as its loops of the falling path fall: one to seven loops of under 256 pixels in every
        // retail room, so the height stays well within the room (#1620).
        int jumpHeight = slot.SpawnSettings<ChootSpawnSettings>().PatternLoops * state.FallingPatternYDistance;
        state.InitialYSpeedCurvePlace = CalculateInitialChootYSpeedCurvePlace(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(CoordinateAxis.Wrap(
            state.SpawnPosition.Y - jumpHeight));
        state.YSpeedCurvePlace = state.InitialYSpeedCurvePlace;
    }

    /// <summary>Runs the Choot's main AI and its four wait, prepare, jump, and fall targets.</summary>
    private static void RunChootMain(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        switch (state.Action)
        {
            case ChootAction.WaitingForSamus:
                WaitForChootActivation(slot, state, samus);
                return;
            case ChootAction.PreparingJump:
                PrepareChootJump(slot, state);
                return;
            case ChootAction.Jumping:
                RunChootJump(slot, state);
                return;
            case ChootAction.Falling:
                RunChootFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Choot has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Uses wrapped signed horizontal distance to arm the jump delay when Samus enters range.</summary>
    private static void WaitForChootActivation(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Choot proximity AI requires the active Samus actor.");

        // The distance is the magnitude of the wrapped signed difference, up to 32768, and the farthest one reads as
        // negative in the game's signed comparison with the activation distance.
        int distance = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole);
        if (CoordinateAxis.Difference(distance, ChootActivationDistance) >= 0)
            return;

        state.JumpDelayTimer = slot.SpawnSettings<ChootSpawnSettings>().JumpDelay;
        state.Action = ChootAction.PreparingJump;
    }

    /// <summary>Decrements the delay until it turns negative, then starts the jumping animation.</summary>
    internal static void PrepareChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        if (--state.JumpDelayTimer >= 0)
            return;

        SetChootAnimation(slot, state, ChootSegment.Jumping);
        state.Action = ChootAction.Jumping;
    }

    /// <summary>Applies quadratic ascent without room collision and snaps whole coordinates to the calculated falling origin at the apex.</summary>
    internal static void RunChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        // Choot applies the table's separately stored negative pair, including its fractional
        // negation bug, without consulting room collision during this authored leap.
        SignedSixteenSixteen upwardDisplacement = state.YSpeedCurvePlace.Row.Negative;
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(upwardDisplacement));

        state.YSpeedCurvePlace -= ChootRowsPerUpdate;
        if (!state.YSpeedCurvePlace.IsNegative)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // subpositions deliberately survive until the final spawn reset, just as in the game.
        slot.Position = slot.Position.WithWhole(state.InitialFallingPosition);
        state.FallingOrigin = state.InitialFallingPosition;
        state.FallingPatternFrame = 0;
        // A population loop count of zero starts below zero, so the fall ends at the first loop's end as a count of one does.
        state.FallingPatternLoopCounter = slot.SpawnSettings<ChootSpawnSettings>().PatternLoops - 1;
        SetChootAnimation(slot, state, ChootSegment.Falling);
        state.Action = ChootAction.Falling;
    }

    /// <summary>Follows authored path offsets, advancing its origin at each loop's end and restoring the exact spawn state after the final loop.</summary>
    internal static void RunChootFall(RoomEnemySlot slot, ChootEnemyState state)
    {
        Offset16<RoomPixels>? offset = ChootFalls.At(
            state.FallingPath ?? throw new InvalidDataException("A falling Choot has no falling pattern."),
            state.FallingPatternFrame);
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
            slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
            SetChootAnimation(slot, state, ChootSegment.Idle);
            state.Action = ChootAction.WaitingForSamus;
            return;
        }

        slot.Position = slot.Position.WithWhole(state.FallingOrigin + pathOffset);
        state.FallingPatternFrame++;
    }

    /// <summary>
    /// Calculates the Choot's initial jump speed, integrating every second quadratic step until
    /// they cover the requested height. The table ends at row 94, so the result is an even index from 2 to 94.
    /// </summary>
    private static EnemyQuadraticSpeedIndex CalculateInitialChootYSpeedCurvePlace(int jumpHeight)
    {
        EnemyQuadraticSpeedIndex curvePlace = EnemyQuadraticSpeedIndex.Start;
        FixedCoordinate accumulatedDistance = default;
        for (int iteration = 0; iteration < 128; iteration++)
        {
            curvePlace += ChootRowsPerUpdate;
            accumulatedDistance = accumulatedDistance.MovedBy(curvePlace.Row.Positive);
            int wholeDistance = accumulatedDistance.Whole;
            if (CoordinateAxis.Difference(wholeDistance, jumpHeight) >= 0)
                return curvePlace;
        }

        throw new InvalidDataException(
            $"Choot jump-height calculation did not reach {jumpHeight} pixels.");
    }

    /// <summary>Installs the idle, jumping, or falling animation and restarts its frame timers.</summary>
    private static void SetChootAnimation(
        RoomEnemySlot slot,
        ChootEnemyState state,
        ChootSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, ChootAnimations.StartingWith(segment));
    }

    /// <summary>Advances Choot's typed animation.</summary>
    internal static void AdvanceChootTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, ChootAnimations.Segments,
            static (ChootEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyChootCue(slot, cue) is { } target
                ? new AnimationContinuation<ChootSegment>(ChootAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Carries out one cue of Choot's animation: each switches off-screen processing on or off.</summary>
    private static ChootSegment? ApplyChootCue(RoomEnemySlot slot, ChootCue cue)
    {
        slot.Processing = cue.Kind switch
        {
            ChootCueKind.ProcessOffScreen => slot.Processing.With(EnemyProcessing.OffScreen),
            ChootCueKind.StopProcessingOffScreen => slot.Processing.Without(EnemyProcessing.OffScreen),
            _ => throw new InvalidOperationException($"Undefined {nameof(ChootCueKind)} {cue.Kind}."),
        };
        return null;
    }

    /// <summary>Runs Choot's main AI.</summary>
    internal static void RunChootMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunChootMain(slot, slot.RequireSpeciesState<ChootEnemyState>(), frame.Samus);
    }
}
