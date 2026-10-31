using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
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
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Restricts camera scrolling while Draygon controls the arena.</summary>
    private readonly SamusScrollCap _scrollCap;
    /// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonGrab(BossFrameResultBuilder frameResults, EnemyRoomBinding room, RoomSpriteObjectPool spriteObjects, SamusScrollCap scrollCap, DraygonParts parts, DraygonTurrets turrets)
    {
        _frameResults = frameResults;
        _room = room;
        _spriteObjects = spriteObjects;
        _scrollCap = scrollCap;
        _parts = parts;
        _turrets = turrets;
    }

    /// <summary>
    /// Ports <c>Function_DraygonBody_ChaseSamus</c> at <c>$A5:8E19</c>. The speed-divisor
    /// test is the real communication channel from bank-$86 goop; no host-only encounter
    /// flag or proximity shortcut is used to start this branch.
    /// </summary>
    internal void ChaseAndTryToGrabSamus(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus is null)
            throw new InvalidOperationException("Draygon chase AI requires the active Samus actor.");

        // Attached goop actors own this word. Once the final actor disappears, retail
        // abandons the chase immediately and exits through the same upward flight used
        // after a completed grab.
        if (samus.XSpeedDivisor == 0)
        {
            state.Function = DraygonAiFunction.FlyStraightUp;
            return;
        }

        // Despite older labels calling $0400 “tangible”, this bit tells the common enemy
        // collision pass to ignore Samus. Draygon's claw-window test below replaces the
        // ordinary body collision while he is deliberately chasing the gooped player.
        RoomEnemySlot body = state.Body;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);

        int clawOffset = state.FacingRight ? 8 : -8;
        bool insideGrabWindow =
            EnemyMath.IsWithinStrictModularDistance(unchecked((ushort)(body.Position.X.Whole + clawOffset)), samus.Position.X.Whole, 8) &&
            EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, samus.Position.Y.Whole, 8);
        if (!insideGrabWindow)
        {
            MoveDraygonToward(body, samus.Position.Whole, DraygonBehavior.DraygonChaseSpeed);
            return;
        }

        _parts.StartDraygonProgram(
            state.Arms,
            state.FacingRight
                ? DraygonProgram.ArmsFacingRightGrab
                : DraygonProgram.ArmsFacingLeftGrab);

        // Samus command $0D returns nonzero whenever the grapple-beam function is anything
        // other than its inactive entry. In that case Draygon drops the beam and retreats;
        // it must not enter a grabbed pose and then undo it with a bespoke correction.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            state.Function = DraygonAiFunction.GrabbedSamus;
            return;
        }

        samus.DraygonGrabbed.Begin(samus, state.FacingRight);
        _frameResults.DraygonDiscardedSamusPoseRequests = true;
        // `$A5:8EDB-$8EE5` set the whole spiral center only; the Y fraction (`$7E:7814`) carries over from
        // the previous spiral of this fight, so the first lift after a re-grab borrows on the native frame.
        state.SpiralCenter = state.SpiralCenter.WithWhole(DraygonBehavior.DraygonSpiralTarget);
        state.SpiralXRadius = SignedSixteenSixteen.Zero;
        state.SpiralAngle = DraygonBehavior.DraygonInitialSpiralAngle;
        state.SpiralAngleDelta = DraygonBehavior.DraygonInitialSpiralAngleDelta;
        state.Function = DraygonAiFunction.CarrySamus;
        state.SuccessfulGrabs++;
    }

    /// <summary>Ports the one-frame grapple rejection at <c>$A5:8F10</c>.</summary>
    internal static void RepelDraygonWithGrapple(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon grapple rejection requires Samus state.");

        samus.Grapple.Phase = GrapplePhase.Dropped;
        state.Function = DraygonAiFunction.ReleaseSamus;
    }

    /// <summary>
    /// Ports <c>$A5:8F1E</c>: fly the grabbed pair toward the authored spiral center, then
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
            _parts.StartDraygonProgram(
                body,
                state.FacingRight
                    ? DraygonProgram.BodyFacingRightRoar
                    : DraygonProgram.BodyFacingLeftRoar);
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        MoveDraygonToward(body, DraygonBehavior.DraygonSpiralTarget, DraygonBehavior.DraygonChaseSpeed);
        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>
    /// Ports the expanding, rising spiral at <c>$A5:8FD6</c>. Radius growth, angular
    /// deceleration, and center lift stay as separate native fixed-point word pairs.
    /// </summary>
    internal void CarrySamusInDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        // The current RNG word is sampled without advancing it. Turret cadence and other
        // actors are the producers; a low byte of zero inserts one 64-frame tail whip.
        if (SharedRandomState.LowerByte(_room.Random.Read()) == 0)
        {
            state.TailWhipTimer = WrappingCountdown.Start(DraygonBehavior.DraygonTailWhipDuration);
            state.Function = DraygonAiFunction.TailWhipWithSamus;
            return;
        }

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(state.SpiralCenter.Whole + new Offset16<RoomPixels>(
            unchecked((short)EnemyMath.ReadEightBitCosineProduct(state.SpiralAngle.ToByteAngle(), state.SpiralXRadius.WholeWord)),
            unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(
                state.SpiralAngle.ToByteAngle(),
                unchecked((ushort)(state.SpiralXRadius.WholeWord >> 2))))));

        if ((body.FrameCounter & 7) == 0)
        {
            _spriteObjects.SpawnRoomSpriteObject(
                body.Position.Whole + new Offset16<RoomPixels>((short)(state.FacingRight ? 32 : -32), -16),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphics: default);
        }

        state.SpiralXRadius += DraygonBehavior.DraygonSpiralRadiusGrowth;
        if (unchecked((short)(state.SpiralXRadius.WholeWord - DraygonBehavior.DraygonMaximumSpiralRadius)) >= 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        state.SpiralAngleDelta = state.SpiralAngleDelta.AddRaw(-1);
        state.SpiralAngle = state.SpiralAngle.AddTableUnits(state.SpiralAngleDelta.TableIndex);

        state.SpiralCenter = state.SpiralCenter.AdvanceY(DraygonBehavior.DraygonSpiralCenterLift);
        if (unchecked((short)(state.SpiralCenter.Y.Whole - DraygonBehavior.DraygonMinimumSpiralCenterY)) < 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>Ports the incidental 64-frame tail-whip pause at <c>$A5:90D4</c>.</summary>
    internal void RunDraygonTailWhip(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        state.TailWhipTimer = state.TailWhipTimer.Decrement().Remaining;
        if (state.TailWhipTimer.IsZero)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            return;
        }

        if (state.TailWhipTimer.RawValue == DraygonBehavior.DraygonTailWhipStartFrame)
        {
            _parts.StartDraygonProgram(
                state.Tail,
                state.FacingRight
                    ? DraygonProgram.TailFacingRightWhip
                    : DraygonProgram.TailFacingLeftWhip);
        }
    }

    /// <summary>Installs the four-repeat finishing whip list at <c>$A5:9105</c>.</summary>
    internal void BeginDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        _parts.StartDraygonProgram(
            state.Tail,
            state.FacingRight
                ? DraygonProgram.TailFacingRightFinalWhips
                : DraygonProgram.TailFacingLeftFinalWhips);
        state.Function = DraygonAiFunction.FinalTailWhipsWait;
    }

    /// <summary>
    /// Ports <c>$A5:9124</c>. The tail's cartridge list owns the duration and eventually
    /// executes opcode <c>$9F57</c>, which changes the body function to release Samus.
    /// </summary>
    internal void WaitForDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus) =>
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));

    /// <summary>Ports release, collision-bit cleanup, and tail flail at <c>$A5:9128</c>.</summary>
    internal void ReleaseSamusFromDraygon(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon release requires the active Samus actor.");

        // Grapple rejection reaches this function without ever installing a grabbed pose.
        // The normal route does own one, so invoke the bank-$90 release only in that case.
        if (samus.DraygonGrabbed.IsActive)
        {
            ReleaseSamusDuringEnemyMain(samus);
            // ReleaseSamusFromDraygon publishes bit one, and $A5:9128 immediately clears
            // the complete native grapple-flags word. Consume the host projection here so
            // a later encounter cannot observe a stale owner-release request.
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
        }

        state.Function = DraygonAiFunction.FlyStraightUp;
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        _parts.StartDraygonProgram(
            state.Tail,
            state.FacingRight
                ? DraygonProgram.TailFacingRightFlail
                : DraygonProgram.TailFacingLeftFlail);
    }

    /// <summary>
    /// <c>MoveSamusWithDraygon</c> ($A5:94A9). It places Samus without checking that she is
    /// still held, and only then tests the escape bit bank $90 publishes. The first carry
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
    /// Implements the shared <c>ConvertAngleToXy</c> plus
    /// <c>MoveEnemyAccordingToAngleAndXYSpeeds</c> pair used by chase and carry.
    /// </summary>
    private static void MoveDraygonToward(
        RoomEnemySlot body,
        Position16<RoomPixels> target,
        int speed)
    {
        ByteAngle movementAngle = EnemyMath.CalculateBearing(target - body.Position.Whole).ToTableAngle();

        uint xMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            checked((ushort)speed),
            angleOffset: RotationAngle.QuarterTurn.TableIndex));
        uint yMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            checked((ushort)speed),
            angleOffset: RotationAngle.HalfTurn.TableIndex));

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
        body.FlashTimer = WrappingCountdown.Start(body.HurtAiTime + 8);
        body.AiStates = body.AiStates | EnemyAiStates.Hurt;
    }

    /// <summary>
    /// Runs <c>ReleaseSamusFromDraygon</c> ($90:E2DE) from Draygon's AI and publishes it so the
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
