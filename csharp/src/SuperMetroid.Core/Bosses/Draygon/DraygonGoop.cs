using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's goop passes; <see cref="DraygonShots"/> owns the goop and turret shots as projectiles.</summary>
[RoomLifetime]
internal sealed class DraygonGoop
{
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;
    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    private readonly DraygonMovement _movement;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonGoop(EnemyRoomBinding room, DraygonTurrets turrets, DraygonMovement movement)
    {
        _room = room;
        _turrets = turrets;
        _movement = movement;
    }

    /// <summary>Runs the two mirrored goop setups.</summary>
    internal void SetupDraygonGoopPass(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState samus,
        int frameCounter)
    {
        // Only the rightward setup fires a wall turret; the leftward setup omits it.
        // That asymmetry changes the random seed and is therefore material.
        if (direction == HorizontalDirection.Right)
            _turrets.ObserveDraygonTurretCadence(samus, frameCounter);

        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithWhole(new(direction == HorizontalDirection.Right ? DraygonBehavior.DraygonOffscreenLeftX : state.RightSideReset.X, 384));
        state.SwoopVelocity = new UnsignedSixteenSixteen(1, 0);
        state.GoopYOscillationAngle = RotationAngle.Zero;
        // Going left, the setup first starts the arms' right-facing apex animation; the body's reset cue then replaces
        // all four parts' animations.
        DraygonParts.StartDraygonAnimation(state.Arms, DraygonSegment.ArmsFacingRightNearSwoopApex);
        state.Phase = DraygonPhase.GoopApproach;
        DraygonParts.StartDraygonAnimation(
            body,
            direction == HorizontalDirection.Right
                ? DraygonSegment.BodyFacingRightReset
                : DraygonSegment.BodyFacingLeftReset);
        state.FacingRight = direction == HorizontalDirection.Right;
    }

    /// <summary>Runs the oscillating approach.</summary>
    internal void ApproachSamusForDraygonGoop(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        if (CoordinateAxis.Distance(state.Body.Position.X.Whole, samus.Position.X.Whole) < 208)
        {
            state.Phase = DraygonPhase.GoopTail;
            state.GoopCounter = 16;
            return;
        }
        MoveDraygonAlongGoopPath(state, direction == HorizontalDirection.Right);
    }

    /// <summary>Runs the firing and off-screen selection.</summary>
    internal void FireDraygonGoop(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState samus,
        int frameCounter)
    {
        // The right-facing shot runs the turret's step; the left-facing one does not.
        if (direction == HorizontalDirection.Right)
            _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        if (samus.XSpeedDivisor != 0)
        {
            state.Phase = DraygonPhase.TryGrabSamus;
            return;
        }

        if (DraygonRandomDefinitions.GoopVolleySteps.IsTakenBy(_room.Random.Read()))
        {
            if (--state.GoopCounter == 0)
            {
                FinishDraygonGoopFiring(state, direction == HorizontalDirection.Right);
                return;
            }
            DraygonParts.StartDraygonAnimation(
                state.Body,
                direction == HorizontalDirection.Right
                    ? DraygonSegment.BodyFacingRightFireGoop
                    : DraygonSegment.BodyFacingLeftFireGoop);
        }

        MoveDraygonAlongGoopPath(state, direction == HorizontalDirection.Right);
        int x = state.Body.Position.X.Whole;
        bool crossedExit = direction == HorizontalDirection.Right
            ? CoordinateAxis.AsSigned(x) >= 0 && x >= 672
            : CoordinateAxis.AsSigned(x) < 0 && CoordinateAxis.Difference(x, DraygonBehavior.DraygonOffscreenLeftX) < 0;
        if (crossedExit)
            FinishDraygonGoopFiring(state, direction == HorizontalDirection.Right);
    }

    /// <summary>Places Draygon offscreen and selects the next swoop after a goop pass exits.</summary>
    private static void FinishDraygonGoopFiring(DraygonEnemyState state, bool movingRight)
    {
        DraygonParts.StartDraygonAnimation(
            state.Arms,
            movingRight
                ? DraygonSegment.ArmsFacingRightIdle
                : DraygonSegment.ArmsFacingLeftIdle);
        state.Phase = DraygonPhase.GoopRecovery;
    }

    /// <summary>Runs the final off-screen movement.</summary>
    internal void ExitDraygonGoopPass(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState samus)
    {
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        // Only the leftward recovery rechecks whether Samus became gooped after firing.
        if (direction != HorizontalDirection.Right && samus.XSpeedDivisor != 0)
        {
            state.Phase = DraygonPhase.TryGrabSamus;
            return;
        }

        MoveDraygonAlongGoopPath(state, direction == HorizontalDirection.Right);
        int x = state.Body.Position.X.Whole;
        bool offScreen = direction == HorizontalDirection.Right
            ? CoordinateAxis.AsSigned(x) >= 0 && x >= 672
            : CoordinateAxis.AsSigned(x) < 0 && CoordinateAxis.Difference(x, DraygonBehavior.DraygonOffscreenLeftX) < 0;
        if (!offScreen)
            return;

        state.SwoopEntry = DraygonSwoops.PathOf(state).EntryCount;
        if (direction == HorizontalDirection.Right)
        {
            state.Phase = DraygonPhase.SwoopSetup;
            state.PassDirection = HorizontalDirection.Left;
            state.Body.Position = state.Body.Position.WithWholeX(592);
        }
        else
        {
            state.Phase = DraygonPhase.SwoopSetup;
            state.PassDirection = HorizontalDirection.Right;
            state.Body.Position = state.Body.Position.WithWholeX(DraygonBehavior.DraygonOffscreenLeftX);
        }
        state.Body.Position = state.Body.Position.WithWholeY(DraygonBehavior.DraygonOffscreenAboveY);
    }

    /// <summary>Applies the goop pass's horizontal motion and one-step Y oscillation.</summary>
    internal static void MoveDraygonAlongGoopPath(DraygonEnemyState state, bool movingRight)
    {
        state.Body.Position = state.Body.Position.WithWholeY(
            384 + EnemyMath.CircleCosinePixels(state.GoopYOscillationAngle.ToTrigonometryAngle(), 32));
        state.GoopYOscillationAngle = state.GoopYOscillationAngle.TurnedBySteps(1);
        DraygonMovement.MoveDraygonHorizontally(state, movingRight);
    }
}
