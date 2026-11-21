using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The branches Bomb and Golden Torizo's animations take on Samus's position, and their walk steps.</summary>
[RoomLifetime]
internal sealed class TorizoChoices
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves body movement against room blocks and slopes.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoChoices(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>
    /// Whether a Torizo attack choice falls in its first attack's phase (#1619): the frame counter plus Samus's X halved,
    /// rounding up, read through <see cref="TorizoCadenceDefinitions.FirstAttackPhase"/>.
    /// </summary>
    /// <param name="frameCounter">The frame counter on the update the choice is made.</param>
    /// <param name="samusX">Samus's X, in whole pixels.</param>
    internal static bool InFirstAttackPhase(int frameCounter, int samusX) =>
        TorizoCadenceDefinitions.FirstAttackPhase.IsInSecondHalfAt(frameCounter + TwoPixelStepsRoundedUp(samusX));

    /// <summary>
    /// <paramref name="x"/> counted in two-pixel steps, rounded up (#2555): an odd X reads as the next step, toward
    /// increasing coordinates for a negative X too.
    /// </summary>
    private static int TwoPixelStepsRoundedUp(int x) => (int)Math.Ceiling(x / 2.0);

    /// <summary>Branches to the grounded-ball response only when Samus is behind the statue, between <see cref="TorizoDecisionDefinitions.MorphedBehindNearest"/> and <see cref="TorizoDecisionDefinitions.MorphedBehindFarthest"/> away.</summary>
    internal static AnimationContinuation<TorizoBodySegment>? SelectGoldenTorizoMorphBallBranch(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        TorizoAnimation target)
    {
        int distance = Math.Abs(samus.Position.X.Whole - torizo.Position.X.Whole);
        bool isGroundedBall = samus.Pose is
            SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose;
        if (TorizoFacing.FacesSamus(torizo, samus) ||
            distance < TorizoDecisionDefinitions.MorphedBehindNearest || distance >= TorizoDecisionDefinitions.MorphedBehindFarthest || !isGroundedBall)
        {
            return null;
        }

        state.DecisionCounter = 0;
        return TorizoAnimations.ContinueWith(target);
    }

    /// <summary>Uses the room RNG to interrupt the walk with an attack forward at medium horizontal range, keeping the walk's place.</summary>
    internal AnimationContinuation<TorizoBodySegment>? SelectGoldenTorizoMediumRangeBranch(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        AnimationPlayback<TorizoBodySegment> continuation,
        TorizoAnimation target)
    {
        int distance = Math.Abs(samus.Position.X.Whole - torizo.Position.X.Whole);
        if (!TorizoFacing.FacesSamus(torizo, samus) ||
            distance < TorizoDecisionDefinitions.MediumRangeNearest || distance >= TorizoDecisionDefinitions.MediumRangeFarthest ||
            !TorizoRandomRolls.MediumRangeAttack.IsTakenBy(_room.Random.Generate()))
        {
            return null;
        }

        state.InterruptedWalk = continuation;
        return TorizoAnimations.ContinueWith(target);
    }

    /// <summary>Launches the forward leap when distant Samus movement or sustained space jumping breaks the wait.</summary>
    internal AnimationContinuation<TorizoBodySegment>? SelectGoldenTorizoSpaceJumpCounter(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        GameplayAction controllerInput,
        TorizoAnimation target)
    {
        int distance = Math.Abs(samus.Position.X.Whole - torizo.Position.X.Whole);
        if (distance < TorizoDecisionDefinitions.ForwardLeapDistance || !TorizoFacing.FacesSamus(torizo, samus))
            return null;
        if (state.SamusSpaceJumpFrames <= TorizoDecisionDefinitions.SpaceJumpLeapFrames &&
            (!controllerInput.HasAny(GameplayActions.HorizontalDirections) ||
                TorizoRandomDefinitions.ForwardLeap.IsTakenBy(_room.Random.Generate())))
        {
            return null;
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoForwardJump(torizo, state);
        return TorizoAnimations.ContinueWith(target);
    }

    /// <summary>Starts a backward jump after enough decisions or when Samus closes inside <see cref="TorizoDecisionDefinitions.CloseInFrontDistance"/>.</summary>
    internal static AnimationContinuation<TorizoBodySegment>? SelectGoldenTorizoRepeatedJump(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        TorizoAnimation target)
    {
        int distance = Math.Abs(samus.Position.X.Whole - torizo.Position.X.Whole);
        if (state.DecisionCounter < TorizoDecisionDefinitions.RepeatedJumpSteps &&
            (distance >= TorizoDecisionDefinitions.CloseInFrontDistance || !TorizoFacing.FacesSamus(torizo, samus)))
        {
            return null;
        }

        state.DecisionCounter = 0;
        StartBackwardJump(torizo, state);
        return TorizoAnimations.ContinueWith(target);
    }

    /// <summary>
    /// Moves the Torizo by the walking step's <paramref name="stepPixels"/>, turns it about as <paramref name="walker"/> does
    /// at a wall in its way, and otherwise arms its turn toward a Samus behind it (#2335).
    /// </summary>
    internal AnimationContinuation<TorizoBodySegment>? ProcessTorizoWalkStep(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        RoomLevelData level,
        TorizoWalker walker,
        int stepPixels)
    {
        state.Velocity = state.Velocity.WithX(TorizoWalkLeftVelocity.Of(stepPixels));
        SignedSixteenSixteen displacement = SignedSixteenSixteen.FromWholePixels(stepPixels);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.TurnTowardSamus = null;
            return TorizoAnimations.ContinueWith(walker.TurnAbout(torizo));
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (FacesAwayFromSamus(torizo, samus))
            state.TurnTowardSamus ??= BossCountdown.Lasting(walker.TurnDelay);
        return null;
    }

    /// <summary>Sets the facing-dependent forward leap velocity; the leap's crouch, which the decision continues in, times its frame.</summary>
    private static void StartGoldenTorizoForwardJump(
        RoomEnemyPlace torizo,
        TorizoEnemyState state)
    {
        SignedEightEight side = TorizoMotionDefinitions.ForwardLeapSideSpeed;
        state.Velocity = new(side.Toward(TorizoFacing.Posture(torizo).Facing), TorizoMotionDefinitions.ForwardLeapRise);
        state.JumpAcceleration = TorizoMotionDefinitions.LeapGravity;
    }

    /// <summary>
    /// Launches a jump back, away from the way the Torizo faces (#2472): Bomb Torizo's from a Samus close in front and Golden
    /// Torizo's repeated jump alike.
    /// </summary>
    private static void StartBackwardJump(
        RoomEnemyPlace torizo,
        TorizoEnemyState state)
    {
        SignedEightEight side = TorizoMotionDefinitions.BackwardJumpSideSpeed;
        state.Velocity = new(side.Toward(TorizoFacing.Posture(torizo).Facing.Reversed()), TorizoMotionDefinitions.BackwardJumpRise);
        state.JumpAcceleration = TorizoMotionDefinitions.LeapGravity;
    }

    /// <summary>Interrupts the walk with the swipe, keeping the walk's place, when Samus is within <see cref="TorizoDecisionDefinitions.CloseBehindDistance"/> behind the statue.</summary>
    internal static AnimationContinuation<TorizoBodySegment>? SelectBombTorizoCloseBehindBranch(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        AnimationPlayback<TorizoBodySegment> continuation,
        TorizoAnimation target)
    {
        if (Math.Abs(samus.Position.X.Whole - torizo.Position.X.Whole) >= TorizoDecisionDefinitions.CloseBehindDistance)
            return null;
        if (FacesAwayFromSamus(torizo, samus))
            return null;
        state.InterruptedWalk = continuation;
        return TorizoAnimations.ContinueWith(target);
    }

    /// <summary>Launches the reverse jump when Samus is within <see cref="TorizoDecisionDefinitions.CloseInFrontDistance"/> in the selected facing half-plane.</summary>
    internal static AnimationContinuation<TorizoBodySegment>? SelectBombTorizoCloseFrontJump(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        SamusState samus,
        TorizoAnimation target)
    {
        if (Math.Abs(samus.Position.X.Whole - torizo.Position.X.Whole) >= TorizoDecisionDefinitions.CloseInFrontDistance ||
            !TorizoFacing.FacesSamus(torizo, samus))
        {
            return null;
        }

        StartBackwardJump(torizo, state);
        return TorizoAnimations.ContinueWith(target);
    }

    /// <summary>
    /// Whether the Torizo faces away from Samus, as its walking steps and close-behind branch test it: facing right with
    /// Samus to its left, or facing left with Samus to its right or level with it. It is not the negation of
    /// <see cref="TorizoFacing.FacesSamus"/>: by the two tests, a Samus level with the Torizo is both in front of
    /// and behind it while it faces left, and neither while it faces right.
    /// </summary>
    private static bool FacesAwayFromSamus(
        RoomEnemyPlace torizo,
        SamusState samus) =>
        TorizoFacing.FacesRight(torizo) == samus.Position.X.Whole < torizo.Position.X.Whole;
}
