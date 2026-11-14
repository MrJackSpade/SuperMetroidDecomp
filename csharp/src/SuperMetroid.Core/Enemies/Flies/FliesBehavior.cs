using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The Flies species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class FliesBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Mella, EnemyDefinitionId.Mellow, EnemyDefinitionId.Menu];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FliesEntity(this, slot, common);

    /// <summary>Provides the shared room RNG whose advance is part of each fly AI update.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Attaches fly AI to the room's enemy and random-state services.</summary>
    internal FliesBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Maximum horizontal Samus distance, in pixels, that triggers an attack from circular flight.</summary>
    private const int FlyAttackHorizontalRange = 112;

    /// <summary>Starts a newly loaded fly at angle zero with its flight animation and clockwise motion.</summary>
    internal static void InitializeFly(RoomEnemySlot slot)
    {
        var state = new FlyEnemyState
        {
            Angle = RotationAngle.Zero,
            Action = FlyAction.ClockwiseCircle,
        };
        slot.SetSpeciesState(state);
        state.Playback = new(FlyAnimations.StartingWith(FlySegment.Flight), AnimationPlace.StartOfAnimation);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Advances room RNG and dispatches a fly's circle, attack, or retreat phase.</summary>
    private void RunFlyMain(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Fly AI requires the active Samus actor.");

        // The flies advance the global RNG even though this family does not consume the
        // returned number. The side effect matters when several random enemies share a room.
        _room.Random.Generate();
        FlyEnemyState state = slot.RequireSpeciesState<FlyEnemyState>();
        switch (state.Action)
        {
            case FlyAction.ClockwiseCircle:
                RunFlyCircle(slot, state, samus, rotation: RotationDirection.Clockwise);
                return;
            case FlyAction.AntiClockwiseCircle:
                RunFlyCircle(slot, state, samus, rotation: RotationDirection.Counterclockwise);
                return;
            case FlyAction.AttackSamus:
                RunFlyAttack(slot, state);
                return;
            case FlyAction.Retreat:
                MoveFlyAccordingToVelocities(slot, state);
                if (--state.RetreatTimer < 0)
                {
                    state.RetreatTimer = 24;
                    state.Action = FlyAction.ClockwiseCircle;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Fly has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Rotates a fly around its flight path, reversing direction each revolution or diving when Samus enters range.</summary>
    private static void RunFlyCircle(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus,
        RotationDirection rotation)
    {
        if (state.RetreatTimer != 0)
        {
            state.RetreatTimer--;
        }
        else if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) <
            FlyAttackHorizontalRange)
        {
            SetFlyToAttackSamus(slot, state, samus);
            return;
        }

        MoveFlyAccordingToAngle(slot, state);
        state.AdvanceCircle(rotation == RotationDirection.Clockwise);
        if (state.Angle == RotationAngle.Zero)
        {
            state.Action = rotation == RotationDirection.Clockwise
                ? FlyAction.AntiClockwiseCircle
                : FlyAction.ClockwiseCircle;
        }
    }

    /// <summary>Aims an asymmetric horizontal and vertical dive toward Samus's current position.</summary>
    private static void SetFlyToAttackSamus(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus)
    {
        TrigonometryAngle angle = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole))).ToMirroredTrigonometryAngle();
        state.Velocity = DiveVelocity(angle);
        state.TargetHeight = new Position16<RoomPixels>(0, samus.Position.Y.Whole);
        state.Action = FlyAction.AttackSamus;
    }

    /// <summary>Continues the dive until it crosses Samus's captured height, then reverses vertical speed into retreat.</summary>
    private static void RunFlyAttack(RoomEnemySlot slot, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(slot, state);
        state.RetreatTimer++;

        bool crossedTarget = state.Velocity.Y.IsNegative
            ? slot.Position.Y.Whole < state.TargetHeight.Y
            : slot.Position.Y.Whole >= state.TargetHeight.Y;
        if (!crossedTarget)
            return;

        state.Velocity = state.Velocity.ReversedY();
        state.Action = FlyAction.Retreat;
    }

    /// <summary>
    /// The dive toward Samus along <paramref name="angle"/>, the mirrored trigonometry angle of her bearing: two pixels per update
    /// along its sine across and four along its sine a quarter turn behind (the negative cosine) down.
    /// </summary>
    internal static EightEightVector<RoomPixels> DiveVelocity(TrigonometryAngle angle) => new(
        EnemyMath.SineVelocity(angle, 2),
        EnemyMath.SineVelocity(angle.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn), 4));

    /// <summary>
    /// One update's step around the circle at <paramref name="angle"/>: one pixel along its sine across and one along its
    /// sine a quarter turn behind (the negative cosine) down.
    /// </summary>
    internal static EightEightVector<RoomPixels> CircleStep(TrigonometryAngle angle) => new(
        EnemyMath.SineVelocity(angle, 1),
        EnemyMath.SineVelocity(angle.TurnedBySteps(-EnemyTrigonometryAngleDefinitions.QuarterTurn), 1));

    /// <summary>Steps the fly around its circle at its angle, across before down.</summary>
    private static void MoveFlyAccordingToAngle(RoomEnemySlot slot, FlyEnemyState state)
    {
        EightEightVector<RoomPixels> step = CircleStep(state.Angle.ToTrigonometryAngle());
        slot.Position = slot.Position.AdvanceX(step.X.ToSixteenSixteenDelta());
        slot.Position = slot.Position.AdvanceY(step.Y.ToSixteenSixteenDelta());
    }

    /// <summary>Applies the fixed-point dive or retreat velocity to the enemy position.</summary>
    private static void MoveFlyAccordingToVelocities(RoomEnemySlot slot, FlyEnemyState state)
    {
        slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
    }

    /// <summary>Advances Flies's typed animation.</summary>
    internal static void AdvanceFliesTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        FlyEnemyState state = slot.RequireSpeciesState<FlyEnemyState>();
        AnimationPlayback<FlySegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, FlyAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free fly animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs Flies's main AI.</summary>
    internal void RunFliesMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunFlyMain(slot, frame.Samus);
    }
}
