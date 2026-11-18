using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The Flies species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FliesBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Mella, EnemyDefinitionId.Mellow, EnemyDefinitionId.MaridiaFly];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new FliesEntity(this, place, common);

    /// <summary>Provides the shared room RNG whose advance is part of each fly AI update.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Attaches fly AI to the room's enemy and random-state services.</summary>
    internal FliesBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }


    /// <summary>Starts a newly loaded fly at angle zero with its flight animation and clockwise motion.</summary>
    internal static void InitializeFly(RoomEnemyPlace place)
    {
        var state = new FlyEnemyState(FlyAction.Circling)
        {
            Angle = RotationAngle.Zero,
            Rotation = RotationDirection.Clockwise,
        };
        place.SetSpeciesState(state);
        state.Playback = new(FlyAnimations.StartingWith(FlySegment.Flight), AnimationPlace.StartOfAnimation);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Advances room RNG and runs a fly's circle, attack or retreat phase.</summary>
    private void RunFlyMain(RoomEnemyPlace place, SamusState samus)
    {
        // The flies advance the global RNG even though this family does not consume the
        // returned number. The side effect matters when several random enemies share a room.
        _room.Random.Generate();
        FlyEnemyState state = place.RequireSpeciesState<FlyEnemyState>();
        switch (state.Action)
        {
            case FlyAction.Circling:
                RunFlyCircle(place, state, samus);
                return;
            case FlyAction.AttackSamus:
                RunFlyAttack(place, state);
                return;
            case FlyAction.Retreat:
                MoveFlyAccordingToVelocities(place, state);
                if (--state.RetreatUpdatesLeft == 0)
                {
                    state.CooldownUpdatesLeft = FlyMotionDefinitions.AttackCooldown;
                    state.Action = FlyAction.Circling;
                    state.Rotation = RotationDirection.Clockwise;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Fly has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Rotates a fly around its flight path, reversing direction each revolution or diving when Samus enters range.</summary>
    private static void RunFlyCircle(
        RoomEnemyPlace place,
        FlyEnemyState state,
        SamusState samus)
    {
        if (state.CooldownUpdatesLeft != 0)
        {
            state.CooldownUpdatesLeft--;
        }
        else if (Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) <
            FlyMotionDefinitions.AttackHorizontalRange)
        {
            SetFlyToAttackSamus(place, state, samus);
            return;
        }

        MoveFlyAccordingToAngle(place, state);
        state.AdvanceCircle();
        if (state.Angle == RotationAngle.Zero)
        {
            state.Rotation = state.Rotation == RotationDirection.Clockwise
                ? RotationDirection.Counterclockwise
                : RotationDirection.Clockwise;
        }
    }

    /// <summary>Aims an asymmetric horizontal and vertical dive toward Samus's current position.</summary>
    private static void SetFlyToAttackSamus(
        RoomEnemyPlace place,
        FlyEnemyState state,
        SamusState samus)
    {
        // A fly retreats one update longer than it dives, so each attack leaves it a step higher, and nothing stops it
        // rising through the ceiling: one that has climbed above the room's top keeps a wrapped height and dives down
        // the short way to Samus, as CoordinateAxis.Difference gives it. Across, a dive ends above Samus and a circle is a few pixels wide, so
        // it never passes the room's left edge.
        TrigonometryAngle angle = EnemyMath.CalculateBearing(
            new(samus.Position.X.Whole - place.Position.X.Whole, CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole))).ToMirroredTrigonometryAngle();
        state.Velocity = DiveVelocity(angle);
        state.TargetY = samus.Position.Y.Whole;
        state.DiveUpdates = 0;
        state.Action = FlyAction.AttackSamus;
    }

    /// <summary>Continues the dive until it crosses Samus's captured height, then reverses vertical speed into retreat.</summary>
    private static void RunFlyAttack(RoomEnemyPlace place, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(place, state);
        state.DiveUpdates++;

        bool crossedTarget = state.Velocity.Y.IsNegative
            ? place.Position.Y.Whole < state.TargetY
            : place.Position.Y.Whole >= state.TargetY;
        if (!crossedTarget)
            return;

        state.Velocity = state.Velocity.ReversedY();
        state.RetreatUpdatesLeft = state.DiveUpdates + FlyMotionDefinitions.RetreatBeyondDive;
        state.Action = FlyAction.Retreat;
    }

    /// <summary>
    /// The dive toward Samus along <paramref name="angle"/>, the mirrored trigonometry angle of her bearing: the dive's speed
    /// across along its sine and its speed down along its sine a quarter turn behind (the negative cosine).
    /// </summary>
    internal static EightEightVector<RoomPixels> DiveVelocity(TrigonometryAngle angle) => new(
        EnemyMath.SineVelocity(angle, FlyMotionDefinitions.DiveAcrossSpeed),
        EnemyMath.SineVelocity(angle - TrigonometryAngle.QuarterTurn, FlyMotionDefinitions.DiveDownSpeed));

    /// <summary>
    /// One update's step around the circle at <paramref name="angle"/>: one pixel along its sine across and one along its
    /// sine a quarter turn behind (the negative cosine) down.
    /// </summary>
    internal static EightEightVector<RoomPixels> CircleStep(TrigonometryAngle angle) => new(
        EnemyMath.SineVelocity(angle, 1),
        EnemyMath.SineVelocity(angle - TrigonometryAngle.QuarterTurn, 1));

    /// <summary>Steps the fly around its circle at its angle, across before down.</summary>
    private static void MoveFlyAccordingToAngle(RoomEnemyPlace place, FlyEnemyState state)
    {
        EightEightVector<RoomPixels> step = CircleStep(state.Angle.ToTrigonometryAngle());
        place.Position = place.Position.AdvanceX(step.X.ToSixteenSixteenDelta());
        place.Position = place.Position.AdvanceY(step.Y.ToSixteenSixteenDelta());
    }

    /// <summary>Applies the fixed-point dive or retreat velocity to the enemy position.</summary>
    private static void MoveFlyAccordingToVelocities(RoomEnemyPlace place, FlyEnemyState state)
    {
        place.Position = place.Position.Advance(state.Velocity.ToSixteenSixteen());
    }

    /// <summary>Advances Flies's typed animation.</summary>
    internal static void AdvanceFliesTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        FlyEnemyState state = place.RequireSpeciesState<FlyEnemyState>();
        AnimationPlayback<FlySegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, FlyAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free fly animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs Flies's main AI.</summary>
    internal void RunFliesMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunFlyMain(place, frame.Samus);
    }
}
