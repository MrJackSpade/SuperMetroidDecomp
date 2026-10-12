using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's complete goop-to-grab route from bank <c>$A5:8E19-$9184</c>. This file owns
/// only the boss half of the interaction. <see cref="SamusDraygonGrabbedState"/> remains
/// authoritative for Samus's poses, escape counter, release velocities, and collision mode.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DraygonChaseSpeed = 2;
    /// <summary>The carry spiral's target and initial center, ($100, $180).</summary>
    private static readonly Position16<RoomPixels> DraygonSpiralTarget = new(0x0100, 0x0180);
    private const ushort DraygonInitialSpiralAngle = 0x00c0;
    private const ushort DraygonInitialSpiralAngleDelta = 0x0800;
    private const ushort DraygonMaximumSpiralRadius = 0x00a0;
    private const ushort DraygonMinimumSpiralCenterY = 0x0040;
    /// <summary>The spiral center's per-frame 16.16 lift, -$0000.4000.</summary>
    private static readonly SnesSignedSixteenSixteen DraygonSpiralCenterLift = SnesSignedSixteenSixteen.FromRaw(-0x4000);
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
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
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
        ushort clawX = unchecked((ushort)(body.Position.X.Whole + clawOffset));
        bool insideGrabWindow =
            IsWithinStrictModularDistance(clawX, samus.Position.X.Whole, 8) &&
            IsWithinStrictModularDistance(body.Position.Y.Whole, samus.Position.Y.Whole, 8);
        if (!insideGrabWindow)
        {
            MoveDraygonToward(body, samus.Position.Whole, DraygonChaseSpeed);
            return;
        }

        InstallDraygonInstruction(
            state.Arms!,
            state.FacingRight
                ? DraygonInstructionProgramDefinitions.ArmsFacingRightGrab
                : DraygonInstructionProgramDefinitions.ArmsFacingLeftGrab);

        // Samus command $0D returns nonzero whenever the grapple-beam function is anything
        // other than its inactive entry. In that case Draygon drops the beam and retreats;
        // it must not enter a grabbed pose and then undo it with a bespoke correction.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            state.Function = DraygonAiFunction.GrabbedSamus;
            return;
        }

        samus.DraygonGrabbed.Begin(_bus!, samus, state.FacingRight);
        state.SpiralCenter = FixedPosition<RoomPixels>.FromWhole(DraygonSpiralTarget);
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
            IsWithinStrictModularDistance(body.Position.X.Whole, DraygonSpiralTarget.X, 2) &&
            IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonSpiralTarget.Y, 2);
        if (reachedTarget)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            InstallDraygonInstruction(
                body,
                state.FacingRight
                    ? DraygonInstructionProgramDefinitions.BodyFacingRightRoar
                    : DraygonInstructionProgramDefinitions.BodyFacingLeftRoar);
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        MoveDraygonToward(body, DraygonSpiralTarget, DraygonChaseSpeed);
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
        if ((SnesLittleEndian.LowByte(RequireRandomNumber())) == 0)
        {
            state.TailWhipTimer = DraygonTailWhipDuration;
            state.Function = DraygonAiFunction.TailWhipWithSamus;
            return;
        }

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(state.SpiralCenter.Whole + new Offset16<RoomPixels>(
            unchecked((short)ReadEightBitCosineProduct(state.SpiralAngle, state.SpiralXRadius)),
            unchecked((short)ReadEightBitNegativeSineProduct(
                state.SpiralAngle,
                unchecked((ushort)(state.SpiralXRadius >> 2))))));

        if ((body.FrameCounter & 7) == 0)
        {
            ushort foamX = unchecked((ushort)(
                body.Position.X.Whole + (state.FacingRight ? 0x0020 : -0x0020)));
            SpawnRoomSpriteObject(
                new(foamX, unchecked((ushort)(body.Position.Y.Whole - 0x0010))),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphicsIndex: 0);
        }

        uint radius = SnesSixteenSixteen.Compose(state.SpiralXRadius, state.SpiralXSubradius);
        radius = unchecked(radius + 0x00002000u);
        state.SpiralXRadius = SnesFixedPosition.WholeOf(unchecked((int)radius));
        state.SpiralXSubradius = unchecked((ushort)radius);
        if (unchecked((short)(state.SpiralXRadius - DraygonMaximumSpiralRadius)) >= 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        state.SpiralAngleDelta = unchecked((ushort)(state.SpiralAngleDelta - 1));
        state.SpiralAngle = unchecked((byte)(
            state.SpiralAngle + SnesUnsignedEightEight.Whole(state.SpiralAngleDelta)));

        state.SpiralCenter = state.SpiralCenter.AdvanceY(DraygonSpiralCenterLift);
        if (unchecked((short)(state.SpiralCenter.Y.Whole - DraygonMinimumSpiralCenterY)) < 0)
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
                state.FacingRight
                    ? DraygonInstructionProgramDefinitions.TailFacingRightWhip
                    : DraygonInstructionProgramDefinitions.TailFacingLeftWhip);
        }
    }

    /// <summary>Installs the four-repeat finishing whip list at <c>$A5:9105</c>.</summary>
    private void BeginDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        InstallDraygonInstruction(
            state.Tail!,
            state.FacingRight
                ? DraygonInstructionProgramDefinitions.TailFacingRightFinalWhips
                : DraygonInstructionProgramDefinitions.TailFacingLeftFinalWhips);
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
            ReleaseSamusDuringEnemyMain(samus);
            // ReleaseSamusFromDraygon publishes bit one, and $A5:9128 immediately clears
            // the complete native grapple-flags word. Consume the host projection here so
            // a later encounter cannot observe a stale owner-release request.
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
        }

        state.Function = DraygonAiFunction.FlyStraightUp;
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        InstallDraygonInstruction(
            state.Tail!,
            state.FacingRight
                ? DraygonInstructionProgramDefinitions.TailFacingRightFlail
                : DraygonInstructionProgramDefinitions.TailFacingLeftFlail);
    }

    /// <summary>Ports the regional four-pixel upward retreat at <c>$A5:9154</c>.</summary>
    private void FlyDraygonStraightUp(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 4)));
        if (unchecked((short)body.Position.Y.Whole) >= 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Function = DraygonAiFunction.SwoopRightSetup;
        body.VariableB = 0;
        body.Position = body.Position.WithWhole(new(state.LeftSideResetXPosition, state.ResetYPosition));
    }

    /// <summary>
    /// <c>MoveSamusWithDraygon</c> ($A5:94A9). It places Samus without checking that she is
    /// still held, and only then tests the escape bit bank $90 publishes. The first carry
    /// call after an escape therefore drags the released Samus once more before Draygon
    /// switches to flying straight up.
    /// </summary>
    private void MoveSamusWithDraygon(DraygonEnemyState state, SamusState samus)
    {
        samus.DraygonGrabbed.ApplyOwnerPosition(
            samus,
            state.Body.Position.Whole,
            state.FacingRight);
        CapSamusScrollingSpeed(samus);
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
        ushort speed)
    {
        byte cartridgeAngle = CalculateCartridgeAngle(target - body.Position.Whole);
        byte movementAngle = unchecked((byte)(0x40 - cartridgeAngle));

        uint xMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x40));
        uint yMagnitude = unchecked((uint)ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: 0x80));

        body.Position = body.Position.WithX(AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: ((movementAngle + 0x40) & 0x80) != 0));
        body.Position = body.Position.WithY(AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: ((movementAngle + 0x80) & 0x80) != 0));
    }

    private static SnesFixedPosition AddDraygonAngleMagnitude(
        SnesFixedPosition position,
        uint magnitude,
        bool subtract)
    {
        uint fixedPosition = position.RawValue;
        fixedPosition = subtract
            ? unchecked(fixedPosition - magnitude)
            : unchecked(fixedPosition + magnitude);
        return SnesFixedPosition.FromRaw(fixedPosition);
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

    /// <summary>
    /// Runs <c>ReleaseSamusFromDraygon</c> ($90:E2DE) from Draygon's AI and publishes it so the
    /// runtime clears the prospective poses alpha already chose.
    /// </summary>
    private void ReleaseSamusDuringEnemyMain(SamusState samus)
    {
        samus.DraygonGrabbed.Release(_bus!, samus);
        SamusReleasedByDraygonThisFrame = true;
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

        // `$A5:9B9A` also publishes the same 32-frame, type-seven quake used by the
        // cartridge's grabbed-tail impact. Keeping it on the shared room-enemy surface lets
        // the ordinary camera renderer observe the hit instead of reducing it to HP loss.
        EarthquakeTimer = 32;
        EarthquakeType = 7;

        state.TailWhipHits++;
        state.LastTailWhipDamage = damage;
        SpawnRoomSpriteObject(
            new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + 0x0010))),
            RoomSpriteObjectKind.DustCloud,
            graphicsIndex: 0);
    }
}
