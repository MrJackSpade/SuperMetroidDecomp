using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
[RoomLifetime]
internal sealed class DraygonGrab
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Restricts camera scrolling while Draygon controls the arena.</summary>
    private readonly SamusScrollCap _scrollCap;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonGrab(EnemyPassEvents passEvents, EnemyRoomBinding room, RoomEffectActors effectActors, SamusScrollCap scrollCap, DraygonTurrets turrets)
    {
        _passEvents = passEvents;
        _room = room;
        _effectActors = effectActors;
        _scrollCap = scrollCap;
        _turrets = turrets;
    }

    /// <summary>
    /// Chases Samus and tries to grab her. Draygon chases her once goop has slowed her (her speed divisor), and nothing
    /// else, neither an encounter flag nor her nearness, starts the chase.
    /// </summary>
    internal void ChaseAndTryToGrabSamus(
        DraygonEnemyState state,
        SamusState samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(samus, frameCounter);
        // Attached goop actors own this divisor. Once the final actor disappears, Draygon
        // abandons the chase immediately and exits through the same upward flight used
        // after a completed grab.
        if (samus.SlowdownLevel == 0)
        {
            state.Phase = DraygonPhase.FlyStraightUp;
            return;
        }

        // Draygon ignores Samus's ordinary body contact while he chases the gooped Samus. Draygon's claw-window test below replaces the
        // ordinary body collision while he is deliberately chasing the gooped player.
        RoomEnemyPlace body = state.Body;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

        int clawOffset = DraygonArenaDefinitions.ClawReach.Toward(state.Facing);
        bool insideGrabWindow =
            DistanceAcrossRoomEdge.Between(body.Position.X.Whole + clawOffset, samus.Position.X.Whole) < DraygonArenaDefinitions.GrabWindow &&
            DistanceAcrossRoomEdge.Between(body.Position.Y.Whole, samus.Position.Y.Whole) < DraygonArenaDefinitions.GrabWindow;
        if (!insideGrabWindow)
        {
            MoveDraygonToward(body, samus.Position.Whole, DraygonGrabDefinitions.ChaseSpeed);
            return;
        }

        state.ArmsPart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonSegment.ArmsFacingRightGrab
                : DraygonSegment.ArmsFacingLeftGrab);

        // While Samus's grapple beam is out in any phase,
        // Draygon drops the beam and retreats;
        // it must not enter a grabbed pose and then undo it with a bespoke correction.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            state.Phase = DraygonPhase.RepelGrapple;
            return;
        }

        samus.DraygonGrabbed.Begin(samus, state.Facing);
        _passEvents.Publish(new PoseRequestsDiscarded());
        state.SpiralCenter = state.SpiralCenter.StartSpiral(DraygonGrabDefinitions.SpiralTarget);
        state.SpiralXRadius = SignedSixteenSixteen.Zero;
        state.SpiralAngle = DraygonGrabDefinitions.InitialSpiralAngle;
        state.SpiralAngleDelta = DraygonGrabDefinitions.InitialSpiralAngleDelta;
        state.Phase = DraygonPhase.CarrySamus;
    }

    /// <summary>Runs the one-frame grapple rejection.</summary>
    internal static void RepelDraygonWithGrapple(DraygonEnemyState state, SamusState samus)
    {
        samus.Grapple.Phase = GrapplePhase.Dropped;
        state.Phase = DraygonPhase.ReleaseSamus;
    }

    /// <summary>
    /// Flies Draygon and the grabbed Samus toward the authored spiral centre, then roars facing his way before the
    /// expanding orbit begins.
    /// </summary>
    internal void CarrySamusToDraygonSpiral(DraygonEnemyState state, SamusState samus)
    {
        if (IsDraygonGrappleConnected(samus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        RoomEnemyPlace body = state.Body;
        // The spiral's target and its tolerance lie inside the arena, so a place within the tolerance is so plainly.
        bool reachedTarget =
            Math.Abs(body.Position.X.Whole - DraygonGrabDefinitions.SpiralTarget.X) < DraygonArenaDefinitions.SpiralTargetTolerance &&
            Math.Abs(body.Position.Y.Whole - DraygonGrabDefinitions.SpiralTarget.Y) < DraygonArenaDefinitions.SpiralTargetTolerance;
        if (reachedTarget)
        {
            state.Phase = DraygonPhase.FlailWithSamus;
            state.BodyPart.StartAnimation(state.Facing == HorizontalDirection.Right
                    ? DraygonSegment.BodyFacingRightRoar
                    : DraygonSegment.BodyFacingLeftRoar);
            body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
            return;
        }

        MoveDraygonToward(body, DraygonGrabDefinitions.SpiralTarget, DraygonGrabDefinitions.ChaseSpeed);
        MoveSamusWithDraygon(state, samus);
    }

    /// <summary>
    /// Runs the expanding, rising spiral. Radius growth, angular
    /// deceleration, and center lift each keep their own fraction of a pixel.
    /// </summary>
    internal void CarrySamusInDraygonSpiral(DraygonEnemyState state, SamusState samus)
    {
        if (IsDraygonGrappleConnected(samus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        // The current random number is read without drawing a new one: turret cadence and other
        // actors draw them. One roll in 256 pauses the spiral for a tail whip from the next update.
        if (DraygonRandomDefinitions.TailWhips.IsTakenBy(_room.Random.Read()))
        {
            state.Phase = DraygonPhase.BeginTailWhipWithSamus;
            return;
        }

        RoomEnemyPlace body = state.Body;
        // The radius starts at none and grows until it reaches its maximum, so its whole pixels are never
        // negative.
        body.Position = body.Position.WithWhole(state.SpiralCenter.Center.Whole + new Offset16<RoomPixels>(
            EnemyMath.CircleCosinePixels(state.SpiralAngle.ToTrigonometryAngle(), state.SpiralXRadius.WholePixels),
            EnemyMath.CircleNegativeSinePixels(
                state.SpiralAngle.ToTrigonometryAngle(),
                state.SpiralXRadius.WholePixels / DraygonArenaDefinitions.SpiralWidthToHeight)));

        if (DraygonCadenceDefinitions.GrabFoam.IsDue(body.FrameCounter))
        {
            _effectActors.SpawnRoomEffectActor(
                body.Position.Whole + DraygonParts.TowardFacing(DraygonArenaDefinitions.MouthFacingRight, state.Facing),
                RoomEffectActorKind.DraygonFoam,
                graphics: SpriteGraphics.SharedStandard);
        }

        state.SpiralXRadius += DraygonGrabDefinitions.SpiralRadiusGrowth;
        if (state.SpiralXRadius.WholePixels >= DraygonGrabDefinitions.MaximumSpiralRadius)
        {
            state.Phase = DraygonPhase.FinalTailWhips;
            return;
        }

        state.SpiralAngleDelta -= DraygonGrabDefinitions.SpiralTurnSlowing;
        state.SpiralAngle += state.SpiralAngleDelta.WholeStep;

        state.SpiralCenter = state.SpiralCenter.Lifted(DraygonGrabDefinitions.SpiralCenterLift);
        // Every spiral starts at its target, inside the arena, and rises a quarter pixel an update until it passes the
        // minimum height, so its centre never reaches the room's top.
        if (state.SpiralCenter.Center.Y.Whole < DraygonGrabDefinitions.MinimumSpiralCenterY)
        {
            state.Phase = DraygonPhase.FinalTailWhips;
            return;
        }

        MoveSamusWithDraygon(state, samus);
    }

    /// <summary>
    /// Runs the tail-whip pause's first update (#2538): the pause starts, the update runs as the pause's others do, and
    /// his tail starts its whip.
    /// </summary>
    internal void BeginDraygonTailWhip(DraygonEnemyState state, SamusState samus)
    {
        state.TailWhipPause.LastFor(DraygonGrabDefinitions.TailWhipUpdates);
        state.Phase = DraygonPhase.TailWhipWithSamus;
        RunDraygonTailWhip(state, samus);
        state.TailPart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonSegment.TailFacingRightWhip
                : DraygonSegment.TailFacingLeftWhip);
    }

    /// <summary>Holds the grabbed pair together through the tail-whip pause; the spiral resumes on its last update.</summary>
    internal void RunDraygonTailWhip(DraygonEnemyState state, SamusState samus)
    {
        MoveSamusWithDraygon(state, samus);
        if (state.TailWhipPause.StepRunsOut())
            state.Phase = DraygonPhase.FlailWithSamus;
    }

    /// <summary>Starts the four-repeat finishing whip animation.</summary>
    internal void BeginDraygonFinalTailWhips(DraygonEnemyState state, SamusState samus)
    {
        MoveSamusWithDraygon(state, samus);
        state.TailPart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonAnimations.TailFacingRightFinalWhips
                : DraygonAnimations.TailFacingLeftFinalWhips);
        state.Phase = DraygonPhase.FinalTailWhipsWait;
    }

    /// <summary>
    /// Waits for the final tail whips. The tail's animation owns the duration and eventually
    /// executes its release step, which changes the body phase to release Samus.
    /// </summary>
    internal void WaitForDraygonFinalTailWhips(DraygonEnemyState state, SamusState samus) =>
        MoveSamusWithDraygon(state, samus);

    /// <summary>Releases Samus, restores ordinary contact, and starts the tail flail.</summary>
    internal void ReleaseSamusFromDraygon(DraygonEnemyState state, SamusState samus)
    {
        // Grapple rejection reaches this phase without ever installing a grabbed pose.
        // The normal route does own one, so invoke the release only in that case.
        if (samus.DraygonGrabbed.IsActive)
        {
            // Draygon sees his own release, so no later carry or encounter finds an escape waiting.
            ReleaseSamusDuringEnemyUpdate(samus);
        }

        state.Phase = DraygonPhase.FlyStraightUp;
        state.Body.Contact = state.Body.Contact.Without(EnemyContact.IgnoresSamus);
        state.TailPart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonSegment.TailFacingRightFlail
                : DraygonSegment.TailFacingLeftFlail);
    }

    /// <summary>
    /// Carries Samus with Draygon. It places Samus without checking that she is
    /// still held, and the carry reports whether she has broken free since his last. The first carrying
    /// update after an escape therefore drags the released Samus once more before Draygon
    /// switches to flying straight up.
    /// </summary>
    private void MoveSamusWithDraygon(DraygonEnemyState state, SamusState samus)
    {
        bool escaped = samus.DraygonGrabbed.CarryTo(
            samus,
            state.Body.Position.Whole,
            state.Facing);
        _scrollCap.CapSamusScrollingSpeed(samus);
        if (escaped)
            state.Phase = DraygonPhase.FlyStraightUp;
    }

    /// <summary>
    /// Turns an angle into X and Y speeds and moves Draygon along them, as the chase
    /// and carry both do.
    /// </summary>
    private static void MoveDraygonToward(
        RoomEnemyPlace body,
        Position16<RoomPixels> target,
        int speed)
    {
        DraygonMovement.MoveDraygonAtAngle(body, EnemyMath.CalculateAimAngle(target - body.Position.Whole), speed);
    }

    /// <summary>Recognizes grapple phases that let Samus electrocute a connected Draygon.</summary>
    internal static bool IsDraygonGrappleConnected(SamusState samus) =>
        samus.Grapple.Phase is GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab;

    /// <summary>Starts the hurt flash and marks Draygon hurt after a grapple shock.</summary>
    private static void MarkDraygonGrappleShock(DraygonEnemyState state)
    {
        RoomEnemyPlace body = state.Body;
        body.HurtFlash.Restart(body.HurtFlashFrames + DraygonArenaDefinitions.GrappleShockFlashExtension);
        body.AiStates = body.AiStates | EnemyAiStates.Hurt;
    }

    /// <summary>
    /// Releases Samus (<see cref="ReleaseSamusFromDraygon"/>) from Draygon's AI and publishes it so the
    /// runtime clears the prospective poses the input phase already chose, as the grab does.
    /// </summary>
    internal void ReleaseSamusDuringEnemyUpdate(SamusState samus)
    {
        samus.DraygonGrabbed.ReleaseByDraygon(samus);
        _passEvents.Publish(new PoseRequestsDiscarded());
    }
}
