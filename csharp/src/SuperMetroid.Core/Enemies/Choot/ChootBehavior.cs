using SuperMetroid.Core.Programs;
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

    /// <summary>Initializes the Choot, selecting a falling pattern and calculating ascent speed from parameters.</summary>
    internal static void InitializeChoot(RoomEnemySlot slot)
    {
        var state = new ChootEnemyState();
        slot.SetSpeciesState(state);

        SetChootAnimation(slot, state, ChootSegment.Idle);
        state.Function = ChootEnemyFunction.WaitingForSamus;
        state.SpawnPosition = slot.Position.Whole;

        ChootFallingPath fallingPath = slot.SpawnSettings<ChootSpawnSettings>().FallingPath;
        state.FallingPath = fallingPath;
        state.FallingPatternYDistance = ChootFalls.LoopYDistance(fallingPath);

        // The jump rises as far as its loops of the falling path fall: one to seven loops of under 256 pixels in every
        // retail room, so the height stays well within the room (#1620).
        int jumpHeight = slot.SpawnSettings<ChootSpawnSettings>().PatternLoops * state.FallingPatternYDistance;
        state.InitialYSpeedTableIndex = CalculateInitialChootYSpeedTableIndex(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(CoordinateAxis.Wrap(
            state.SpawnPosition.Y - jumpHeight));
        state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
    }

    /// <summary>Runs the Choot's main AI and its four wait, prepare, jump, and fall targets.</summary>
    private static void RunChootMain(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case ChootEnemyFunction.WaitingForSamus:
                WaitForChootActivation(slot, state, samus);
                return;
            case ChootEnemyFunction.PreparingJump:
                PrepareChootJump(slot, state);
                return;
            case ChootEnemyFunction.Jumping:
                RunChootJump(slot, state);
                return;
            case ChootEnemyFunction.Falling:
                RunChootFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Choot function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Uses wrapped signed horizontal distance to arm the parameter-two jump delay when Samus enters range.</summary>
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
        state.Function = ChootEnemyFunction.PreparingJump;
    }

    /// <summary>Decrements the delay until it turns negative, then starts the jumping program.</summary>
    internal static void PrepareChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        if (--state.JumpDelayTimer >= 0)
            return;

        SetChootAnimation(slot, state, ChootSegment.Jumping);
        state.Function = ChootEnemyFunction.Jumping;
    }

    /// <summary>Applies quadratic ascent without room collision and snaps whole coordinates to the calculated falling origin at the apex.</summary>
    internal static void RunChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        // Choot applies the table's separately stored negative pair, including its fractional
        // negation bug, without consulting room collision during this authored leap.
        SignedSixteenSixteen upwardDisplacement = EnemyQuadraticSpeeds.Row(state.YSpeedTableIndex).Negative;
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(upwardDisplacement));

        state.YSpeedTableIndex -= 2;
        if (state.YSpeedTableIndex >= 0)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // subpositions deliberately survive until the final spawn reset, just as in the game.
        slot.Position = slot.Position.WithWhole(state.InitialFallingPosition);
        state.FallingOrigin = state.InitialFallingPosition;
        state.FallingPatternFrame = 0;
        // A population loop count of zero starts below zero, so the fall ends at the first loop's end as a count of one does.
        state.FallingPatternLoopCounter = slot.SpawnSettings<ChootSpawnSettings>().PatternLoops - 1;
        SetChootAnimation(slot, state, ChootSegment.Falling);
        state.Function = ChootEnemyFunction.Falling;
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
            // the idle program, and rearms the same calculated ascent for the next activation.
            state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
            slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
            SetChootAnimation(slot, state, ChootSegment.Idle);
            state.Function = ChootEnemyFunction.WaitingForSamus;
            return;
        }

        slot.Position = slot.Position.WithWhole(state.FallingOrigin + pathOffset);
        state.FallingPatternFrame++;
    }

    /// <summary>
    /// Calculates the Choot's initial jump speed, integrating every second quadratic step until
    /// they cover the requested height. The table ends at record 94, so the result is an even index from 2 to 94.
    /// </summary>
    private static int CalculateInitialChootYSpeedTableIndex(int jumpHeight)
    {
        int tableIndex = 0;
        FixedCoordinate accumulatedDistance = default;
        for (int iteration = 0; iteration < 128; iteration++)
        {
            tableIndex += 2;
            accumulatedDistance = accumulatedDistance.MovedBy(
                EnemyQuadraticSpeeds.Row(tableIndex).Positive);
            int wholeDistance = accumulatedDistance.Whole;
            if (CoordinateAxis.Difference(wholeDistance, jumpHeight) >= 0)
                return tableIndex;
        }

        throw new InvalidDataException(
            $"Choot jump-height calculation did not reach {jumpHeight} pixels.");
    }

    /// <summary>Installs the idle, jumping, or falling program and restarts its frame timers.</summary>
    private static void SetChootAnimation(
        RoomEnemySlot slot,
        ChootEnemyState state,
        ChootSegment program)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, ChootAnimations.StartingWith(program));
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
