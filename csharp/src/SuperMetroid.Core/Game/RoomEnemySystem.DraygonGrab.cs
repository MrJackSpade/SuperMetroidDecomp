namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's complete goop-to-grab route from bank <c>$A5:8E19-$9184</c>. This file owns
/// only the boss half of the interaction. <see cref="SamusDraygonGrabbedState"/> remains
/// authoritative for Samus's poses, escape counter, release velocities, and collision mode.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DraygonChaseSpeed = 2;
    private const ushort DraygonSpiralTargetX = 0x0100;
    private const ushort DraygonSpiralTargetY = 0x0180;
    private const ushort DraygonInitialSpiralAngle = 0x00c0;
    private const ushort DraygonInitialSpiralAngleDelta = 0x0800;
    private const ushort DraygonMaximumSpiralRadius = 0x00a0;
    private const ushort DraygonMinimumSpiralCenterY = 0x0040;
    private const ushort DraygonTailWhipDuration = 0x0040;
    private const ushort DraygonTailWhipStartFrame = 0x003f;

    /// <summary>
    /// Ports <c>Function_DraygonBody_ChaseSamus</c> at <c>$A5:8E19</c>. The speed-divisor
    /// test is the real communication channel from bank-$86 goop; no host-only encounter
    /// flag or proximity shortcut is used to start this branch.
    /// </summary>
    private void ChaseAndTryToGrabSamus(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
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

        short clawOffset = state.FacingRight ? (short)8 : (short)-8;
        ushort clawX = unchecked((ushort)(body.XPosition + clawOffset));
        bool insideGrabWindow =
            IsWithinStrictModularDistance(clawX, samus.XPosition, 8) &&
            IsWithinStrictModularDistance(body.YPosition, samus.YPosition, 8);
        if (!insideGrabWindow)
        {
            MoveDraygonToward(body, samus.XPosition, samus.YPosition, DraygonChaseSpeed);
            return;
        }

        InstallDraygonInstruction(state.Arms!, state.FacingRight ? (ushort)0x9c38 : (ushort)0x9845);

        // Samus command $0D returns nonzero whenever the grapple-beam function is anything
        // other than its inactive entry. In that case Draygon drops the beam and retreats;
        // it must not enter a grabbed pose and then undo it with a bespoke correction.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            state.Function = DraygonAiFunction.GrabbedSamus;
            return;
        }

        samus.DraygonGrabbed.Begin(_bus!, samus, state.FacingRight);
        state.SpiralCenterX = DraygonSpiralTargetX;
        state.SpiralCenterY = DraygonSpiralTargetY;
        state.SpiralCenterYSubposition = 0;
        state.SpiralXRadius = 0;
        state.SpiralXSubradius = 0;
        state.SpiralAngle = DraygonInitialSpiralAngle;
        state.SpiralAngleDelta = DraygonInitialSpiralAngleDelta;
        state.Function = DraygonAiFunction.CarrySamus;
        state.SuccessfulGrabs++;
    }

    /// <summary>Ports the one-frame grapple rejection at <c>$A5:8F10</c>.</summary>
    private static void RepelDraygonWithGrapple(DraygonEnemyState state, SamusState? samus)
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
    private void CarrySamusToDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        RoomEnemySlot body = state.Body;
        bool reachedTarget =
            IsWithinStrictModularDistance(body.XPosition, DraygonSpiralTargetX, 2) &&
            IsWithinStrictModularDistance(body.YPosition, DraygonSpiralTargetY, 2);
        if (reachedTarget)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            InstallDraygonInstruction(body, state.FacingRight ? (ushort)0x9cb4 : (ushort)0x9922);
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        MoveDraygonToward(body, DraygonSpiralTargetX, DraygonSpiralTargetY, DraygonChaseSpeed);
        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>
    /// Ports the expanding, rising spiral at <c>$A5:8FD6</c>. Radius growth, angular
    /// deceleration, and center lift stay as separate native fixed-point word pairs.
    /// </summary>
    private void CarrySamusInDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        // The current RNG word is sampled without advancing it. Turret cadence and other
        // actors are the producers; a low byte of zero inserts one 64-frame tail whip.
        if (((_readRandomNumber?.Invoke() ?? 0) & 0x00ff) == 0)
        {
            state.TailWhipTimer = DraygonTailWhipDuration;
            state.Function = DraygonAiFunction.TailWhipWithSamus;
            return;
        }

        RoomEnemySlot body = state.Body;
        body.XPosition = unchecked((ushort)(
            state.SpiralCenterX +
            ReadEightBitCosineProduct(state.SpiralAngle, state.SpiralXRadius)));
        body.YPosition = unchecked((ushort)(
            state.SpiralCenterY +
            ReadEightBitNegativeSineProduct(
                state.SpiralAngle,
                unchecked((ushort)(state.SpiralXRadius >> 2)))));

        if ((body.FrameCounter & 7) == 0)
        {
            ushort foamX = unchecked((ushort)(
                body.XPosition + (state.FacingRight ? 0x0020 : -0x0020)));
            SpawnRoomSpriteObject(
                foamX,
                unchecked((ushort)(body.YPosition - 0x0010)),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphicsIndex: 0);
        }

        uint radius = ((uint)state.SpiralXRadius << 16) | state.SpiralXSubradius;
        radius = unchecked(radius + 0x00002000u);
        state.SpiralXRadius = unchecked((ushort)(radius >> 16));
        state.SpiralXSubradius = unchecked((ushort)radius);
        if (unchecked((short)(state.SpiralXRadius - DraygonMaximumSpiralRadius)) >= 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        state.SpiralAngleDelta = unchecked((ushort)(state.SpiralAngleDelta - 1));
        state.SpiralAngle = unchecked((byte)(
            state.SpiralAngle + (state.SpiralAngleDelta >> 8)));

        uint centerY = ((uint)state.SpiralCenterY << 16) | state.SpiralCenterYSubposition;
        centerY = unchecked(centerY - 0x00004000u);
        state.SpiralCenterY = unchecked((ushort)(centerY >> 16));
        state.SpiralCenterYSubposition = unchecked((ushort)centerY);
        if (unchecked((short)(state.SpiralCenterY - DraygonMinimumSpiralCenterY)) < 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>Ports the incidental 64-frame tail-whip pause at <c>$A5:90D4</c>.</summary>
    private void RunDraygonTailWhip(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        state.TailWhipTimer = unchecked((ushort)(state.TailWhipTimer - 1));
        if (state.TailWhipTimer == 0)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            return;
        }

        if (state.TailWhipTimer == DraygonTailWhipStartFrame)
        {
            InstallDraygonInstruction(
                state.Tail!,
                state.FacingRight ? (ushort)0x9ea1 : (ushort)0x9ae8);
        }
    }

    /// <summary>Installs the four-repeat finishing whip list at <c>$A5:9105</c>.</summary>
    private void BeginDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        InstallDraygonInstruction(
            state.Tail!,
            state.FacingRight ? (ushort)0x9e21 : (ushort)0x9a68);
        state.Function = DraygonAiFunction.FinalTailWhipsWait;
    }

    /// <summary>
    /// Ports <c>$A5:9124</c>. The tail's cartridge list owns the duration and eventually
    /// executes opcode <c>$9F57</c>, which changes the body function to release Samus.
    /// </summary>
    private void WaitForDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus) =>
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));

    /// <summary>Ports release, collision-bit cleanup, and tail flail at <c>$A5:9128</c>.</summary>
    private void ReleaseSamusFromDraygon(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon release requires the active Samus actor.");

        // Grapple rejection reaches this function without ever installing a grabbed pose.
        // The normal route does own one, so invoke the bank-$90 release only in that case.
        if (samus.DraygonGrabbed.IsActive)
        {
            samus.DraygonGrabbed.Release(_bus!, samus);
            // ReleaseSamusFromDraygon publishes bit one, and $A5:9128 immediately clears
            // the complete native grapple-flags word. Consume the host projection here so
            // a later encounter cannot observe a stale owner-release request.
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
        }

        state.Function = DraygonAiFunction.FlyStraightUp;
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        InstallDraygonInstruction(
            state.Tail!,
            state.FacingRight ? (ushort)0x9f15 : (ushort)0x9b5a);
    }

    /// <summary>Ports the regional four-pixel upward retreat at <c>$A5:9154</c>.</summary>
    private void FlyDraygonStraightUp(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        RoomEnemySlot body = state.Body;
        body.YPosition = unchecked((ushort)(body.YPosition - 4));
        if (unchecked((short)body.YPosition) >= 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Function = DraygonAiFunction.SwoopRightSetup;
        body.VariableB = 0;
        body.XPosition = state.LeftSideResetXPosition;
        body.YPosition = state.ResetYPosition;
    }

    /// <summary>
    /// Exact owner-position seam at <c>$A5:94A9</c>. Bank-$90 publishes escape as a
    /// one-shot owner signal after changing Samus back to an ordinary pose; consume that
    /// signal before asking the grabbed-pose object to accept another owner coordinate.
    /// </summary>
    private static void MoveSamusWithDraygon(DraygonEnemyState state, SamusState samus)
    {
        if (samus.DraygonGrabbed.ConsumeOwnerReleaseSignal())
        {
            state.Function = DraygonAiFunction.FlyStraightUp;
            return;
        }

        if (!samus.DraygonGrabbed.IsActive)
        {
            state.Function = DraygonAiFunction.FlyStraightUp;
            return;
        }

        samus.DraygonGrabbed.ApplyOwnerPosition(
            samus,
            state.Body.XPosition,
            state.Body.YPosition,
            state.FacingRight);
    }

    /// <summary>
    /// Implements the shared <c>ConvertAngleToXy</c> plus
    /// <c>MoveEnemyAccordingToAngleAndXYSpeeds</c> pair used by chase and carry.
    /// </summary>
    private void MoveDraygonToward(
        RoomEnemySlot body,
        ushort targetX,
        ushort targetY,
        ushort speed)
    {
        short deltaX = unchecked((short)(targetX - body.XPosition));
        short deltaY = unchecked((short)(targetY - body.YPosition));
        byte cartridgeAngle = CalculateCartridgeAngle(deltaX, deltaY);
        byte movementAngle = unchecked((byte)(0x40 - cartridgeAngle));

        uint xMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x40));
        uint yMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x80));

        (body.XPosition, body.XSubposition) = AddDraygonAngleMagnitude(
            body.XPosition,
            body.XSubposition,
            xMagnitude,
            subtract: ((movementAngle + 0x40) & 0x80) != 0);
        (body.YPosition, body.YSubposition) = AddDraygonAngleMagnitude(
            body.YPosition,
            body.YSubposition,
            yMagnitude,
            subtract: ((movementAngle + 0x80) & 0x80) != 0);
    }

    private static (ushort Position, ushort Subposition) AddDraygonAngleMagnitude(
        ushort position,
        ushort subposition,
        uint magnitude,
        bool subtract)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        fixedPosition = subtract
            ? unchecked(fixedPosition - magnitude)
            : unchecked(fixedPosition + magnitude);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private static bool IsDraygonGrappleConnected(SamusState samus) =>
        samus.Grapple.Phase is GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab;

    private static void MarkDraygonGrappleShock(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.FlashTimer = unchecked((ushort)(body.HurtAiTime + 8));
        body.AiHandlerBits = unchecked((ushort)(body.AiHandlerBits | 2));
    }

    private static SamusState RequireDraygonGrabbedSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon carry AI requires the active Samus actor.");
        return samus;
    }

    /// <summary>Ports tail instruction <c>$A5:9B9A</c>, including suit damage.</summary>
    private void ApplyDraygonTailWhipHit(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon's tail-hit instruction requires Samus state.");

        state.SwoopYAcceleration = 0x0018;
        ushort rawDamage = state.Body.Definition.Damage;
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(rawDamage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(rawDamage >> 1))
                : rawDamage;
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));

        state.TailWhipHits++;
        state.LastTailWhipDamage = damage;
        SpawnRoomSpriteObject(
            samus.XPosition,
            unchecked((ushort)(samus.YPosition + 0x0010)),
            RoomSpriteObjectKind.DustCloud,
            graphicsIndex: 0);
    }
}
