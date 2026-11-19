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
        body.Position = body.Position.WithWhole(new(direction == HorizontalDirection.Right ? OffRoomPlaces.Place(DraygonArenaDefinitions.OffscreenLeftX) : state.RightSideResetX, DraygonArenaDefinitions.SwimLineY));
        state.SwoopVelocity = DraygonSwoopDefinitions.GoopApproachSpeed.Toward(direction);
        state.GoopYOscillationAngle = RotationAngle.Zero;
        state.Phase = DraygonPhase.GoopApproach;
        state.BodyPart.StartAnimation(direction == HorizontalDirection.Right
                ? DraygonSegment.BodyFacingRightReset
                : DraygonSegment.BodyFacingLeftReset);
        state.Facing = direction;
    }

    /// <summary>Runs the oscillating approach.</summary>
    internal void ApproachSamusForDraygonGoop(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        if (DistanceAcrossRoomEdge.Between(state.Body.Position.X.Whole, samus.Position.X.Whole) < DraygonArenaDefinitions.GoopTailRange)
        {
            state.Phase = DraygonPhase.GoopTail;
            state.GoopShotsLeft.LastFor(DraygonArenaDefinitions.GoopsPerPass);
            return;
        }
        MoveDraygonAlongGoopPath(state);
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
        if (samus.SlowdownLevel != 0)
        {
            state.Phase = DraygonPhase.TryGrabSamus;
            return;
        }

        if (DraygonRandomDefinitions.GoopVolleySteps.IsTakenBy(_room.Random.Read()))
        {
            if (state.GoopShotsLeft.StepRunsOut())
            {
                FinishDraygonGoopFiring(state, direction);
                return;
            }
            state.BodyPart.StartAnimation(direction == HorizontalDirection.Right
                    ? DraygonSegment.BodyFacingRightFireGoop
                    : DraygonSegment.BodyFacingLeftFireGoop);
        }

        MoveDraygonAlongGoopPath(state);
        int x = state.Body.Position.X.Whole;
        bool crossedExit = direction == HorizontalDirection.Right
            ? !OffRoomPlaces.LiesBeforeRoom(x) && x >= DraygonArenaDefinitions.RightSideResetDistance
            : OffRoomPlaces.LiesBeforeRoom(x) && DistanceAcrossRoomEdge.LiesBefore(x, DraygonArenaDefinitions.OffscreenLeftX);
        if (crossedExit)
            FinishDraygonGoopFiring(state, direction);
    }

    /// <summary>Places Draygon offscreen and selects the next swoop after a goop pass exits.</summary>
    private static void FinishDraygonGoopFiring(DraygonEnemyState state, HorizontalDirection direction)
    {
        state.ArmsPart.StartAnimation(direction == HorizontalDirection.Right
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
        if (direction != HorizontalDirection.Right && samus.SlowdownLevel != 0)
        {
            state.Phase = DraygonPhase.TryGrabSamus;
            return;
        }

        MoveDraygonAlongGoopPath(state);
        int x = state.Body.Position.X.Whole;
        bool offScreen = direction == HorizontalDirection.Right
            ? !OffRoomPlaces.LiesBeforeRoom(x) && x >= DraygonArenaDefinitions.RightSideResetDistance
            : OffRoomPlaces.LiesBeforeRoom(x) && DistanceAcrossRoomEdge.LiesBefore(x, DraygonArenaDefinitions.OffscreenLeftX);
        if (!offScreen)
            return;

        if (direction == HorizontalDirection.Right)
        {
            state.Phase = DraygonPhase.SwoopSetup;
            state.PassDirection = HorizontalDirection.Left;
            state.Body.Position = state.Body.Position.WithWholeX(DraygonArenaDefinitions.RightSwoopSetupX);
        }
        else
        {
            state.Phase = DraygonPhase.SwoopSetup;
            state.PassDirection = HorizontalDirection.Right;
            state.Body.Position = OffRoomPlaces.WithWholeX(state.Body.Position, DraygonArenaDefinitions.OffscreenLeftX);
        }
        state.Body.Position = OffRoomPlaces.WithWholeY(state.Body.Position, DraygonArenaDefinitions.OffscreenAboveY);
    }

    /// <summary>Applies the goop pass's horizontal motion and one-step Y oscillation.</summary>
    internal static void MoveDraygonAlongGoopPath(DraygonEnemyState state)
    {
        state.Body.Position = state.Body.Position.WithWholeY(
            DraygonArenaDefinitions.SwimLineY + EnemyMath.CircleCosinePixels(state.GoopYOscillationAngle.ToTrigonometryAngle(), DraygonArenaDefinitions.GoopSwayRadius));
        state.GoopYOscillationAngle = state.GoopYOscillationAngle.TurnedBySteps(1);
        DraygonMovement.MoveDraygonHorizontally(state);
    }
}
