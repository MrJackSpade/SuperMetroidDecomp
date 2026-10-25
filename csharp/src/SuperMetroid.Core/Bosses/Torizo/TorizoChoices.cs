using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The branches Bomb and Golden Torizo's programs take on Samus's position, and their walk steps.</summary>
[RoomLifetime]
internal sealed class TorizoChoices
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves body movement against room blocks and slopes.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoChoices(EnemyRoomBinding room, EnemyTerrain terrain, TorizoEncounterAccess access)
    {
        _room = room;
        _terrain = terrain;
        _access = access;
    }

    /// <summary>Branches to the grounded-ball response only when Samus is four to 39 pixels away behind the statue.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoMorphBallBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        bool isGroundedBall = samus.Pose is
            SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallMovingRightPose or
            SamusPoseId.MorphBallMovingLeftPose or SamusPoseId.SpringBallGroundRightPose or
            SamusPoseId.SpringBallGroundLeftPose or SamusPoseId.SpringBallMovingRightPose or
            SamusPoseId.SpringBallMovingLeftPose;
        if (_access.BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 4 || distance >= 40 || !isGroundedBall)
        {
            return null;
        }

        state.DecisionCounter = 0;
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Uses the room RNG to select a forward-facing attack at medium horizontal range.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoMediumRangeBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        AnimationProgramCursor<TorizoBodyProgram> continuation,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (!_access.BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 32 || distance >= 96 || !TorizoRandomDefinitions.MediumRangeAttack.AllClear(_room.Random.Generate()))
        {
            return null;
        }

        state.ReturnCursor = continuation;
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Launches the forward leap when distant Samus movement or sustained space jumping breaks the wait.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoSpaceJumpCounter(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ControllerButton controllerInput,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (distance < 112 || !_access.BombTorizoFunction12IsNonNegative(torizo, samus))
            return null;
        ControllerButton heldButtons = controllerInput;
        if (state.SamusSpaceJumpFrames <= 360 &&
            (!heldButtons.HasAny(ControllerButtons.HorizontalDirections) ||
                TorizoRandomDefinitions.ForwardLeap.AllClear(_room.Random.Generate())))
        {
            return null;
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoForwardJump(torizo, state);
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Starts a backward jump after enough decisions or when Samus closes inside 32 pixels.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? SelectGoldenTorizoRepeatedJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        TorizoBodyProgram target)
    {
        if (samus is null)
            return null;
        int distance = Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)));
        if (state.DecisionCounter < 8 &&
            (distance >= 32 || !_access.BombTorizoFunction12IsNonNegative(torizo, samus)))
        {
            return null;
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoBackwardJump(torizo, state);
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Moves by the Golden walk table, branches on wall collision, and arms a sixteen-frame air transition near Samus.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? ProcessGoldenTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        int tableOffset)
    {
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(GoldenTorizoWalkDefinitions.Velocity(tableOffset)));
        int displacement = SnesSixteenSixteen.FromWhole(state.Velocity.X.SignedRawValue);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = WrappingCountdown.Start(0);
            return AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(torizo)
                ? TorizoBodyProgram.GoldenTurningLeft
                : TorizoBodyProgram.GoldenTurningRight);
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = WrappingCountdown.Start(16);
        }
        return null;
    }

    /// <summary>Sets the facing-dependent forward leap velocity and restarts its instruction timer.</summary>
    private void StartGoldenTorizoForwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.Velocity = new EightEightVector<RoomPixels>(
            _access.FacesRight(torizo) ? (short)512 : (short)-512, -1472);
        state.JumpAcceleration = new EightEightVector<RoomPixels>(0, 40);
        torizo.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Sets the faster reverse leap velocity used by Golden Torizo's repeated-jump branch.</summary>
    private void StartGoldenTorizoBackwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.Velocity = new EightEightVector<RoomPixels>(
            _access.FacesRight(torizo) ? (short)-768 : (short)768, -1152);
        state.JumpAcceleration = new EightEightVector<RoomPixels>(0, 40);
        torizo.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Uses the walk table's velocity and wall branches, arming a 72-frame air transition when Samus crosses the facing test.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? ProcessBombTorizoWalkInstruction(RoomEnemySlot torizo, TorizoEnemyState state, SamusState? samus, RoomLevelData level, TorizoWalkStep step)
    {
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(BombTorizoMovementDefinitions.WalkVelocity(step.TableOffset)));
        int displacement = SnesSixteenSixteen.FromWhole(state.Velocity.X.SignedRawValue);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = WrappingCountdown.Start(0);
            return AnimationProgramCursor<TorizoBodyProgram>.Start(_access.FacesRight(torizo)
                ? step.CollisionFacingRight
                : step.CollisionFacingLeft);
        }

        EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer.IsZero)
        {
            state.AirTransitionTimer = WrappingCountdown.Start(72);
        }
        return null;
    }

    /// <summary>Saves the next instruction and branches when Samus is within 56 pixels behind the statue.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? SelectBombTorizoCloseBehindBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        AnimationProgramCursor<TorizoBodyProgram> continuation,
        TorizoBodyProgram target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) >= 56)
            return null;
        if (BombTorizoParameterMatchesSamusDelta(torizo, samus))
            return null;
        state.ReturnCursor = continuation;
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>Launches the reverse jump when Samus is within 32 pixels in the selected facing half-plane.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? SelectBombTorizoCloseFrontJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        TorizoBodyProgram target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole))) >= 32 ||
            !_access.BombTorizoFunction12IsNonNegative(torizo, samus))
        {
            return null;
        }

        // $AA:C22D launches away from the currently faced direction at 3 px/frame and
        // starts with -4.5 px/frame vertical speed, adding 40/256 px/frame each tick.
        state.Velocity = new EightEightVector<RoomPixels>(
            _access.FacesRight(torizo) ? (short)-768 : (short)768, -1152);
        state.JumpAcceleration = new EightEightVector<RoomPixels>(0, 40);
        torizo.InstructionTimer = WrappingCountdown.Start(1);
        return AnimationProgramCursor<TorizoBodyProgram>.Start(target);
    }

    /// <summary>
    /// Compares the facing bit with Samus-X minus enemy-X for walking branches; the
    /// cartridge's other facing helper deliberately uses the inverse subtraction.
    /// </summary>
    private bool BombTorizoParameterMatchesSamusDelta(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // Instructions $C470/$C4E5/$C567 compare the facing bit against the sign of SamusX-enemyX.
        // Keep that subtraction direction explicit: Torizo_Func_12 below deliberately uses
        // the inverse delta, and collapsing both tests into one "faces Samus" helper silently
        // reverses one family of ROM branches.
        return _access.FacesRight(torizo) == unchecked((short)(samus.Position.X.Whole - torizo.Position.X.Whole)) < 0;
    }
}
