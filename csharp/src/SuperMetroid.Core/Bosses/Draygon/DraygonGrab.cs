using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
[RoomLifetime]
internal sealed class DraygonGrab
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Restricts camera scrolling while Draygon controls the arena.</summary>
    private readonly SamusScrollCap _scrollCap;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonGrab(BossFrameResultBuilder frameResults, EnemyRoomBinding room, RoomSpriteObjects spriteObjects, SamusScrollCap scrollCap, DraygonParts parts, DraygonTurrets turrets)
    {
        _frameResults = frameResults;
        _room = room;
        _spriteObjects = spriteObjects;
        _scrollCap = scrollCap;
        _parts = parts;
        _turrets = turrets;
    }

    /// <summary>
    /// Chases Samus and tries to grab her. The speed-divisor
    /// test is the real communication channel from goop; no invented encounter
    /// flag or proximity shortcut is used to start this branch.
    /// </summary>
    internal void ChaseAndTryToGrabSamus(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounter)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounter);
        if (samus is null)
            throw new InvalidOperationException("Draygon chase AI requires the active Samus actor.");

        // Attached goop actors own this divisor. Once the final actor disappears, retail
        // abandons the chase immediately and exits through the same upward flight used
        // after a completed grab.
        if (samus.XSpeedDivisor == 0)
        {
            state.Function = DraygonAiFunction.FlyStraightUp;
            return;
        }

        // Despite older labels calling the intangible property “tangible”, this bit tells the common enemy
        // collision pass to ignore Samus. Draygon's claw-window test below replaces the
        // ordinary body collision while he is deliberately chasing the gooped player.
        RoomEnemySlot body = state.Body;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

        int clawOffset = state.FacingRight ? 8 : -8;
        bool insideGrabWindow =
            EnemyMath.IsWithinStrictModularDistance(CoordinateAxis.Wrap(body.Position.X.Whole + clawOffset), samus.Position.X.Whole, 8) &&
            EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, samus.Position.Y.Whole, 8);
        if (!insideGrabWindow)
        {
            MoveDraygonToward(body, samus.Position.Whole, DraygonBehavior.DraygonChaseSpeed);
            return;
        }

        _parts.StartDraygonAnimation(
            state.Arms,
            state.FacingRight
                ? DraygonSegment.ArmsFacingRightGrab
                : DraygonSegment.ArmsFacingLeftGrab);

        // Samus's grapple query returns nonzero whenever the grapple-beam function is anything
        // other than its inactive entry. In that case Draygon drops the beam and retreats;
        // it must not enter a grabbed pose and then undo it with a bespoke correction.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            state.Function = DraygonAiFunction.GrabbedSamus;
            return;
        }

        samus.DraygonGrabbed.Begin(samus, state.FacingRight);
        _frameResults.DraygonDiscardedSamusPoseRequests = true;
        // The spiral setup sets the whole spiral center only; the Y fraction carries over from
        // the previous spiral of this fight, so the first lift after a re-grab borrows on the frame.
        state.SpiralCenter = state.SpiralCenter.WithWhole(DraygonBehavior.DraygonSpiralTarget);
        state.SpiralXRadius = SignedSixteenSixteen.Zero;
        state.SpiralAngle = DraygonBehavior.DraygonInitialSpiralAngle;
        state.SpiralAngleDelta = DraygonBehavior.DraygonInitialSpiralAngleDelta;
        state.Function = DraygonAiFunction.CarrySamus;
    }

    /// <summary>Runs the one-frame grapple rejection.</summary>
    internal static void RepelDraygonWithGrapple(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon grapple rejection requires Samus state.");

        samus.Grapple.Phase = GrapplePhase.Dropped;
        state.Function = DraygonAiFunction.ReleaseSamus;
    }

    /// <summary>
    /// Flies the grabbed pair toward the authored spiral center, then
    /// install the facing-specific roar before the expanding orbit begins.
    /// </summary>
    internal void CarrySamusToDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        RoomEnemySlot body = state.Body;
        bool reachedTarget =
            EnemyMath.IsWithinStrictModularDistance(body.Position.X.Whole, DraygonBehavior.DraygonSpiralTarget.X, 2) &&
            EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonBehavior.DraygonSpiralTarget.Y, 2);
        if (reachedTarget)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            _parts.StartDraygonAnimation(
                body,
                state.FacingRight
                    ? DraygonSegment.BodyFacingRightRoar
                    : DraygonSegment.BodyFacingLeftRoar);
            body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
            return;
        }

        MoveDraygonToward(body, DraygonBehavior.DraygonSpiralTarget, DraygonBehavior.DraygonChaseSpeed);
        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>
    /// Runs the expanding, rising spiral. Radius growth, angular
    /// deceleration, and center lift stay separate fixed-point values.
    /// </summary>
    internal void CarrySamusInDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        // The current random number is read without drawing a new one: turret cadence and other
        // actors draw them. A tail-whip roll of zero inserts one 64-frame tail whip.
        if (DraygonRandomDefinitions.TailWhips.IsTakenBy(_room.Random.Read()))
        {
            state.TailWhipTimer = DraygonBehavior.DraygonTailWhipDuration;
            state.Function = DraygonAiFunction.TailWhipWithSamus;
            return;
        }

        RoomEnemySlot body = state.Body;
        // The radius starts at none and grows until it reaches its maximum of 160 pixels, so its whole pixels are never
        // negative.
        body.Position = body.Position.WithWhole(state.SpiralCenter.Whole + new Offset16<RoomPixels>(
            EnemyMath.ReadEightBitCosineProduct(state.SpiralAngle.ToTrigonometryAngle(), state.SpiralXRadius.WholePixels),
            EnemyMath.ReadEightBitNegativeSineProduct(
                state.SpiralAngle.ToTrigonometryAngle(),
                state.SpiralXRadius.WholePixels / 4)));

        if (DraygonCadenceDefinitions.GrabFoamEveryEighthUpdate.IsDue(body.FrameCounter))
        {
            _spriteObjects.SpawnRoomSpriteObject(
                body.Position.Whole + new Offset16<RoomPixels>(state.FacingRight ? 32 : -32, -16),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphics: default);
        }

        state.SpiralXRadius += DraygonBehavior.DraygonSpiralRadiusGrowth;
        if (state.SpiralXRadius.WholePixels >= DraygonBehavior.DraygonMaximumSpiralRadius)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        state.SpiralAngleDelta -= DraygonBehavior.DraygonSpiralTurnSlowing;
        state.SpiralAngle += state.SpiralAngleDelta.WholeStep;

        state.SpiralCenter = state.SpiralCenter.AdvanceY(DraygonBehavior.DraygonSpiralCenterLift);
        if (CoordinateAxis.Difference(state.SpiralCenter.Y.Whole, DraygonBehavior.DraygonMinimumSpiralCenterY) < 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>Runs the incidental 64-frame tail-whip pause.</summary>
    internal void RunDraygonTailWhip(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        if (--state.TailWhipTimer == 0)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            return;
        }

        if (state.TailWhipTimer == DraygonBehavior.DraygonTailWhipStartFrame)
        {
            _parts.StartDraygonAnimation(
                state.Tail,
                state.FacingRight
                    ? DraygonSegment.TailFacingRightWhip
                    : DraygonSegment.TailFacingLeftWhip);
        }
    }

    /// <summary>Starts the four-repeat finishing whip animation.</summary>
    internal void BeginDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        _parts.StartDraygonAnimation(
            state.Tail,
            state.FacingRight
                ? DraygonSegment.TailFacingRightFinalWhips
                : DraygonSegment.TailFacingLeftFinalWhips);
        state.Function = DraygonAiFunction.FinalTailWhipsWait;
    }

    /// <summary>
    /// Waits for the final tail whips. The tail's animation owns the duration and eventually
    /// executes its release step, which changes the body function to release Samus.
    /// </summary>
    internal void WaitForDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus) =>
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));

    /// <summary>Runs release, collision-bit cleanup, and tail flail.</summary>
    internal void ReleaseSamusFromDraygon(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon release requires the active Samus actor.");

        // Grapple rejection reaches this function without ever installing a grabbed pose.
        // The normal route does own one, so invoke the release only in that case.
        if (samus.DraygonGrabbed.IsActive)
        {
            ReleaseSamusDuringEnemyMain(samus);
            // ReleaseSamusFromDraygon publishes the owner-release flag, and the release immediately clears
            // all of Samus's grapple flags. Consume that release signal here so
            // a later encounter cannot observe a stale owner-release request.
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
        }

        state.Function = DraygonAiFunction.FlyStraightUp;
        state.Body.Contact = state.Body.Contact.Without(EnemyContact.IgnoresSamus);
        _parts.StartDraygonAnimation(
            state.Tail,
            state.FacingRight
                ? DraygonSegment.TailFacingRightFlail
                : DraygonSegment.TailFacingLeftFlail);
    }

    /// <summary>
    /// <c>MoveSamusWithDraygon</c>. It places Samus without checking that she is
    /// still held, and only then tests the escape flag Samus's grapple publishes. The first carry
    /// call after an escape therefore drags the released Samus once more before Draygon
    /// switches to flying straight up.
    /// </summary>
    internal void MoveSamusWithDraygon(DraygonEnemyState state, SamusState samus)
    {
        samus.DraygonGrabbed.ApplyOwnerPosition(
            samus,
            state.Body.Position.Whole,
            state.FacingRight);
        _scrollCap.CapSamusScrollingSpeed(samus);
        if (samus.DraygonGrabbed.ConsumeOwnerReleaseSignal())
            state.Function = DraygonAiFunction.FlyStraightUp;
    }

    /// <summary>
    /// Turns an angle into X and Y speeds and moves Draygon along them, as the chase
    /// and carry both do.
    /// </summary>
    private static void MoveDraygonToward(
        RoomEnemySlot body,
        Position16<RoomPixels> target,
        int speed)
    {
        TrigonometryAngle movementAngle = EnemyMath.CalculateBearing(target - body.Position.Whole).ToTableAngle();

        UnsignedSixteenSixteen xMagnitude = DraygonMovement.AngleMagnitude(movementAngle, speed, RotationAngle.QuarterTurn);
        UnsignedSixteenSixteen yMagnitude = DraygonMovement.AngleMagnitude(movementAngle, speed, RotationAngle.HalfTurn);

        body.Position = body.Position.WithX(DraygonMovement.AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: EnemyMath.CosineIsNegative(movementAngle)));
        body.Position = body.Position.WithY(DraygonMovement.AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: EnemyMath.SineIsNegative(movementAngle)));
    }

    /// <summary>Recognizes grapple phases that let Samus electrocute a connected Draygon.</summary>
    internal static bool IsDraygonGrappleConnected(SamusState samus) =>
        samus.Grapple.Phase is GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab;

    /// <summary>Sets the hurt-flash timer and handler bit after a grapple shock.</summary>
    private static void MarkDraygonGrappleShock(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.FlashTimer = body.HurtAiTime + 8;
        body.AiStates = body.AiStates | EnemyAiStates.Hurt;
    }

    /// <summary>
    /// Runs <c>ReleaseSamusFromDraygon</c> from Draygon's AI and publishes it so the
    /// runtime clears the prospective poses alpha already chose, as the grab does.
    /// </summary>
    internal void ReleaseSamusDuringEnemyMain(SamusState samus)
    {
        samus.DraygonGrabbed.Release(samus);
        _frameResults.DraygonDiscardedSamusPoseRequests = true;
    }

    /// <summary>Requires the active Samus actor for Draygon's carry and release phases.</summary>
    private static SamusState RequireDraygonGrabbedSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon carry AI requires the active Samus actor.");
        return samus;
    }
}
