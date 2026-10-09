using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Enemy definition word selecting Ceres Ridley's slot-zero AI and state extension.</summary>
    private const ushort CeresRidleyDefinition = EnemyDefinitionPointers.CeresRidley;

    /// <summary>
    /// Runs Ceres Ridley's bank-$A6 shot-overlap seam against the five ordinary Samus
    /// projectile slots. The native shot handler increments a dedicated counter and never
    /// subtracts enemy health; the projectile owner performs its normal explosion change.
    /// </summary>
    public int ResolveCeresRidleyProjectileHits(
        ISnesAddressSpace bus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(projectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);

        if (_ridleyState is null || CeresStatus != 0)
            return 0;

        RoomEnemySlot slot = _slots[0];
        if (slot.EnemyDefinitionPointer != CeresRidleyDefinition ||
            slot.Properties.HasAny(
                EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return 0;
        }

        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            if (!projectile.HasEnemyCollisionPayload ||
                !ExtendedSpritemapOverlapsRectangle(
                    slot,
                    projectile.XPosition,
                    projectile.YPosition,
                    projectile.XRadius,
                    projectile.YRadius))
            {
                continue;
            }
            if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
                continue;

            ResolveCeresRidleyShotAfterCollision(slot);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Executes the non-area-two half of <c>EnemyShot_Ridley</c> at $A6:DF8A after bank
    /// $A0 has accepted and marked an overlapping projectile. Both the ordinary-shot and
    /// physical normal-bomb walkers dispatch this exact callback; keeping the reaction here
    /// prevents those two collision owners from acquiring subtly different cinematic rules.
    /// </summary>
    private void ResolveCeresRidleyShotAfterCollision(RoomEnemySlot slot)
    {
        RidleyEnemyState state = RequireRidley(slot);
        if (slot.EnemyDefinitionPointer != CeresRidleyDefinition)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} entered Ceres Ridley's shot branch as " +
                $"definition ${slot.EnemyDefinitionPointer:X4}.");
        }

        // `$DF99-$DFA9` starts with Y=13, shifts the old nonzero timer once, and selects
        // 14 only when that shift produced carry. In host terms this is precisely old bit
        // zero; testing bit one (or simply toggling the new value) changes repeated-hit
        // timing and eventually moves the escape threshold relative to the hurt palette.
        slot.FlashTimer = slot.FlashTimer != 0 && (slot.FlashTimer & 1) != 0
            ? (ushort)14
            : (ushort)13;
        state.HitCounter = unchecked((ushort)(state.HitCounter + 1));

        // The cartridge draws the hurt palette from the enemy frame that just completed.
        // Collision runs afterward, so publish that same pre-increment frame immediately
        // for standalone renderers inspecting CGRAM before the next enemy dispatcher call.
        UpdateRidleyHurtFlashPalettes(
            slot,
            state,
            unchecked((ushort)(slot.FrameCounter - 1)));
        UpdateCeresRidleyHealthPalette(state);
    }

    /// <summary>
    /// Ports the Ceres branch of <c>CeresRidley_Init</c> at $A6:A0F5. The shared routine also
    /// initializes Norfair Ridley ($E17F), but selecting that branch here would be fiction:
    /// this loader currently has no saved boss-bit input and the playable path is fresh Ceres.
    /// </summary>
    private void InitializeCeresRidley(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Ceres Ridley requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }

        // $A6:A10B-A12E clears the shared boss tilemap workspace, disables the minimap, and
        // marks the room explored. Those consumers are not yet represented by RoomEnemySystem;
        // every actor-owned write that follows is retained below.
        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.Initial);
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native bits $1000 and $0400 are proven by the initializer, but $1000 has not yet
        // been given a cross-enemy semantic name. Preserve it raw; $0400 already has the
        // verified IgnoreSamusCollision identity used by the scheduler.
        slot.Properties = slot.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.XPosition = 0x00ba;
        slot.YPosition = 0x00a9;
        CeresStatus = 0;

        _ridleyState = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = 0,
            HitCounter = 0,
            SpritemapPaletteIndex = EnemyPaletteBits.Palette7,
            CommonDrawPaletteIndex = EnemyPaletteBits.Palette7,
            MovementAnimationEnabled = 0,
            FacingDirection = 0,
            IdleTailWhipEnabled = 1,
            TailDamage = 0x000f,
            HorizontalVelocity = 0,
            VerticalVelocity = 0,
            MinimumY = 0xffe0,
            MaximumY = 0x00b0,
            MinimumX = 0x0028,
            MaximumX = 0x00e0,
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyFunction = 0xbe9c,
            BabyCurrentSpritemap = 0,
            BabyXPosition = unchecked((ushort)(slot.XPosition - 16)),
            BabyYPosition = unchecked((ushort)(slot.YPosition + 22)),
            WingFrame = 5,
            WingAnimationTimer = 0,
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = 0,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            TailWhipTargetClockwiseAngle = 0xffff,
            TailWhipTargetCounterClockwiseAngle = 0xffff,
            TailWhipRequest = 0,
            TailExtensionSpeed = 0x00f0,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentAngle,
            TailSegments = CreateInitialRidleyTailSegments(),
        };
        // `$A6:A280` ends Ceres Ridley's initialization by queuing music zero (stop).
        QueueInitializationMusicDelayed8(MusicCommand.Stop);

        // WriteColorsToTargetPalette($140, $A6:E16F, $20) installs the Ceres door and Baby
        // Metroid container palettes. This runtime exposes the final target directly in
        // CGRAM, matching the established Ceres-door palette seam.
        (CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley requires installed start colors.")).ApplyStart(_cgram!);

        // The native loop clears target-palette byte offsets $1E2..$1FE: OBJ palette seven,
        // colors one through fifteen. Color zero belongs to the shared transparent backdrop
        // and is intentionally left untouched.
        for (int color = 0x1e2 / 2; color <= 0x1fe / 2; color++)
            _cgram!.SetColor(color, 0);
    }

    /// <summary>
    /// Ports the Ceres dispatcher reached from $A6:A288 through the end of the battle. The
    /// reveal, liftoff, hover, two cartridge-authored termination conditions, upward retreat,
    /// palette handoff, and <c>ceres_status = 1</c> publication all retain their ROM addresses.
    /// Ridley's later Mode-7 getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    private void RunCeresRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus,
        VramWriteQueue? vramWriteQueue)
    {
        RidleyEnemyState state = RequireCeresRidley(slot);
        slot.Health = 0x7fff;

        // $A6:A29F runs after the boss dispatcher and before movement. None of the
        // translated dispatcher branches mutate the flash timer, so selecting the two
        // draw palettes here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled != 0 && CeresStatus == 0)
            UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);

        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                // A35B waits even though enemy visual instructions run during fade.
                if (EnemyDoorTransitionActive)
                    break;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = 512;
                TickCeresRidleyInitialDelay(state);
                break;

            case RidleyAiFunction.InitialDelay:
                TickCeresRidleyInitialDelay(state);
                break;

            case RidleyAiFunction.FadeInEyes:
                TickCeresRidleyEyeFade(state);
                break;

            case RidleyAiFunction.FadeInBody:
                TickCeresRidleyBodyFade(slot, state);
                break;

            case RidleyAiFunction.WaitBeforeRoar:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.OpeningRoar);
                    state.FunctionTimer = 252;
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                break;

            case RidleyAiFunction.WaitBeforeLiftoff:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    state.FadePaletteOffset = 0;
                    SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.TransitionToFlying);
                    // Function_Ridley_RoarBeforeFly at $A6:A4B4-A4CB primes the wing
                    // timer, enables every tail segment, and selects neutral tail AI in
                    // the same transition that installs instruction list $E91D.
                    state.WingAnimationTimer = 8;
                    state.WingAnimationTimerDelta = 8;
                    foreach (RidleyTailSegment segment in state.TailSegments)
                        segment.Active = true;
                    state.TailFunctionIndex = 1;
                    state.Function = RidleyAiFunction.ClearVelocity;
                }
                break;

            case RidleyAiFunction.ClearVelocity:
                state.HorizontalVelocity = 0;
                state.VerticalVelocity = 0;
                break;

            case RidleyAiFunction.CeresLiftoffAccelerating:
                // $A6:A6AF adds -16 to the 8.8 Y velocity before the common movement pass.
                state.VerticalVelocity = unchecked((ushort)(state.VerticalVelocity - 16));
                if (unchecked((short)(slot.YPosition - 112)) < 0)
                {
                    state.Function = RidleyAiFunction.CeresLiftoffDecelerating;
                    TickCeresRidleyLiftoffDecelerating(slot, state);
                }
                break;

            case RidleyAiFunction.CeresLiftoffDecelerating:
                TickCeresRidleyLiftoffDecelerating(slot, state);
                break;

            case RidleyAiFunction.CeresHovering:
                TickCeresRidleyHover(slot, state, samus);
                break;

            case RidleyAiFunction.CeresFireballMoveToPosition:
                TickCeresRidleyFireballMove(slot, state);
                break;

            case RidleyAiFunction.CeresFireballShooting:
                TickCeresRidleyFireballShooting(slot, state);
                break;

            case RidleyAiFunction.CeresLungeSetup:
                SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.CeresLunge);
                state.Function = RidleyAiFunction.CeresLungeMain;
                state.FunctionTimer = 64;
                TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresLungeMain:
                TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresSwoopSetup:
                state.Function = RidleyAiFunction.CeresSwoopMoveToPosition;
                state.FunctionTimer = 10;
                state.SwoopAngleAccumulator = 0;
                state.IdleTailWhipEnabled = 0;
                TickCeresRidleySwoopMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresSwoopMoveToPosition:
                TickCeresRidleySwoopMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresSwoopDescendingAimingDown:
                TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -32,
                    targetAngle: -1024,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopDescendingAimingLeft,
                    nextTimer: 36);
                break;

            case RidleyAiFunction.CeresSwoopDescendingAimingLeft:
                TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: -16384,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopAscendingAimingUp,
                    nextTimer: 28);
                if (state.Function == RidleyAiFunction.CeresSwoopAscendingAimingUp)
                {
                    // `$A6:A91B-$A91E` requests one aimed tail fling at the exact bottom
                    // of the swoop. Neutral tail AI consumes and clears it later this same
                    // enemy-main pass; treating it as a persistent animation mode makes the
                    // tail lag an entire attack behind Ridley.
                    state.TailWhipRequest = 1;
                }
                break;

            case RidleyAiFunction.CeresSwoopAscendingAimingUp:
                TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: -30720,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopAscendingAimingUpFaster,
                    nextTimer: 1);
                break;

            case RidleyAiFunction.CeresSwoopAscendingAimingUpFaster:
                UpdateRidleySwoopVelocity(
                    state, angleDelta: -768, targetAngle: -30720, targetMagnitude: 768);
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    state.Function = RidleyAiFunction.CeresHovering;
                    state.HoverCounter = 0;
                    state.IdleTailWhipEnabled = 1;
                }
                break;

            case RidleyAiFunction.CeresFakeRetreatMoveToPosition:
                TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresFakeRetreatRising:
                TickCeresRidleyFakeRetreatRising(slot, state);
                break;

            case RidleyAiFunction.CeresWaitBeforeRetrievingBaby:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.RetrieveBabyMetroid);
                    state.Function = RidleyAiFunction.CeresRetrieveBaby;
                    TickCeresRidleyRetrieveBaby(slot, state);
                }
                break;

            case RidleyAiFunction.CeresRetrieveBaby:
                TickCeresRidleyRetrieveBaby(slot, state);
                break;

            case RidleyAiFunction.CeresRealRetreatRising:
                TickCeresRidleyRetreat(slot, state);
                break;

            case RidleyAiFunction.CeresRetreatDelay:
                TickCeresRidleyRetreatDelay(state);
                break;

            case RidleyAiFunction.CeresPublishEscapeHandoff:
                // $A6:AA11 installs a null dispatcher and publishes the room-main cutscene
                // state. The native renderer stops emitting Ridley's ordinary composite at
                // this point because Mode 7 owns his departure; hide the ordinary actor so
                // it cannot remain parked over that separately scoped presentation seam.
                state.Function = RidleyAiFunction.CeresInactive;
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                CeresStatus = 1;
                SetupCeresRidleyMode7(state);
                break;

            case RidleyAiFunction.CeresInactive:
                // $A6:AA4F is an RTS; room main owns the getaway.
                return;

            case RidleyAiFunction.CeresActivateSelfDestruct:
                TickCeresRidleySelfDestruct(state, vramWriteQueue);
                return;

            case RidleyAiFunction.CeresSelfDestructPaletteOnly:
                UpdateCeresSelfDestructPalette(state, slot.FrameCounter);
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres Ridley function $A6:{(ushort)state.Function:X4} is not translated.");
        }

        // Once the eye fade enables composite animation, CeresRidley_Main performs the
        // common Ridley 8.8 movement pass after every dispatcher call. Keeping that order
        // matters at A6AF/A6C8: velocity changes affect position in the very same frame.
        if (state.MovementAnimationEnabled != 0 && CeresStatus == 0)
        {
            IntegrateRidleyMovement(slot, state, ceresWallImpact: true);
            TickRidleyWingAnimation(state);
            RandomlyUpdateCeresRidleyTailCurliness(state);
            TickRidleyTail(slot, state, samus);
            UpdateCeresRidleyHealthPalette(state);
        }
        if (CeresStatus == 0)
            TickCeresBaby(slot, state);
    }

    /// <summary>
    /// <c>RandomlyUpdateRidleyTailCurliness</c> ($A6:A2BD): when the live RNG word is at
    /// least $FF00 (sampled, not advanced), the ideal inter-segment tail angle becomes its
    /// low nibble plus seven plus the compare's set carry, so $08..$17.
    /// </summary>
    private void RandomlyUpdateCeresRidleyTailCurliness(RidleyEnemyState state)
    {
        ushort random = _readRandomNumber!();
        if (random < 0xff00)
            return;
        state.IdealInterSegmentTailAngle = (ushort)((random & 0x000f) + 7 + 1);
    }

    /// <summary>
    /// Ports <c>RidleyHurtFlashHandling</c> at $A6:D4DA and the common extended-enemy
    /// palette override at $A0:9497. Ridley's composite is split between those two writers:
    /// the ordinary body uses the current execution counter, while the manual tail/wings
    /// deliberately test the counter after an increment.
    /// </summary>
    private static void UpdateRidleyHurtFlashPalettes(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        ushort enemyMainExecutionCounter)
    {
        state.CommonDrawPaletteIndex = slot.FlashTimer != 0 &&
            (enemyMainExecutionCounter & 2) != 0
                ? (ushort)0
                : slot.PaletteIndex;

        state.SpritemapPaletteIndex = slot.FlashTimer > 1 &&
            (unchecked((ushort)(enemyMainExecutionCounter + 1)) & 2) != 0
                ? (ushort)0
                : EnemyPaletteBits.Palette7;
    }

    /// <summary>
    /// Ports $A6:D4B5-$D4D7. Ceres uses shot count in place of health, including the
    /// original missing branch after the 90-shot comparison: 50-69 selects palette zero,
    /// and every value from 70 onward selects palette two.
    /// </summary>
    private void UpdateCeresRidleyHealthPalette(RidleyEnemyState state)
    {
        if (state.FightMode == 0 || state.HitCounter < 50)
            return;

        int paletteIndex = state.HitCounter < 70
            ? CeresRidleyPaletteRomData.HealthMidRow
            : CeresRidleyPaletteRomData.HealthLateRow;
        (CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley damage requires installed health colors."))
            .ApplyHealth(_cgram!, paletteIndex);
    }

    /// <summary>Applies the deceleration phase after liftoff crosses its vertical threshold, then switches to hover.</summary>
    /// <param name="slot">Ridley slot supplying the height threshold.</param>
    /// <param name="state">State whose vertical velocity and AI function are advanced.</param>
    private static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // The fallthrough from A6AF deliberately applies both -16 and +20 on the crossing
        // frame. Do not combine these into a single acceleration outside this state.
        state.VerticalVelocity = unchecked((ushort)(state.VerticalVelocity + 20));
        if (unchecked((short)(slot.YPosition - 80)) < 0)
        {
            state.Function = RidleyAiFunction.CeresHovering;
            state.FightMode = 1;
        }
    }

    /// <summary>Runs the hover cycle, selecting the retreat, lunge, swoop, or fireball branch from combat state.</summary>
    /// <param name="slot">Ridley slot used for range checks and movement targets.</param>
    /// <param name="state">Combat counters and dispatch state updated by the hover cycle.</param>
    /// <param name="samus">Active player state used when a lunge direction must be chosen.</param>
    private void TickCeresRidleyHover(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        // Ceres Ridley never loses the ordinary enemy-health word. His shot AI increments
        // this separate counter and A6E8 treats 100 contacts as the scripted win condition.
        if (state.HitCounter >= 100)
        {
            state.FightMode = 0;
            state.Function = RidleyAiFunction.CeresFakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
            return;
        }

        // The other original termination route is Samus energy below 30. A synthetic room
        // fixture may omit Samus, in which case only the shot-counter route is meaningful.
        // $A6:A706 jumps into the real retreat, so its first acceleration runs this call.
        if (samus is not null && unchecked((short)(samus.Health - 30)) < 0)
        {
            state.FightMode = 0;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(slot, state);
            return;
        }

        // A6:A763 is the shared cooldown/return-to-hover movement: accelerate toward
        // ($00C0,$0064) using divisor table entry zero.
        AccelerateRidleyToward(slot, state, targetX: 192, targetY: 100, divisorIndex: 0);
        if (!IsWithinRidleyRectangle(slot, 192, 100, 8, 8))
        {
            // Shitroid_Func_2 returns zero on rectangle overlap. $A6:A6E8 therefore waits
            // only while Ridley is still outside the hover box, with $7C as the authentic
            // timeout. The previous translation inverted this branch and selected attacks
            // while he was still above the arena, making every fireball route hit its
            // intentional 48-frame bailout before the mouth animation could begin.
            state.HoverCounter = unchecked((ushort)(state.HoverCounter + 1));
            if (state.HoverCounter < 124)
                return;
        }

        // A6:A743 contains sixteen literal function pointers: six fireball routes, five
        // lunges, and five swoops. The runtime's random provider is the same room-load seam
        // already consumed by Ceres steam initialization.
        int choice = _nextRandom!() & 0x0f;
        state.Function = choice switch
        {
            0 or 1 or 2 or 3 or 9 or 15 => RidleyAiFunction.CeresFireballMoveToPosition,
            4 or 5 or 6 or 7 or 8 => RidleyAiFunction.CeresLungeSetup,
            _ => RidleyAiFunction.CeresSwoopSetup,
        };
        state.HoverCounter = 0;
    }

    /// <summary>Moves Ridley into the firing position while keeping the vertical velocity at its native minimum magnitude.</summary>
    /// <param name="slot">Ridley slot whose position determines the approach phase.</param>
    /// <param name="state">Movement state and hover counter updated during the approach.</param>
    private void TickCeresRidleyFireballMove(RoomEnemySlot slot, RidleyEnemyState state)
    {
        int verticalVelocity = unchecked((short)state.VerticalVelocity);
        int magnitude = Math.Max(128, Math.Abs(verticalVelocity));
        state.VerticalVelocity = unchecked((ushort)(verticalVelocity < 0 ? -magnitude : magnitude));
        AccelerateRidleyToward(slot, state, slot.XPosition, 88, 0);

        if (unchecked((short)(slot.YPosition - 80)) < 0)
        {
            state.HoverCounter = unchecked((ushort)(state.HoverCounter + 1));
            if (state.HoverCounter >= 48)
                state.Function = RidleyAiFunction.CeresSwoopSetup;
            return;
        }
        if (unchecked((short)(slot.YPosition - 128)) >= 0)
            return;

        state.FireballBaseXPosition = slot.XPosition;
        state.FireballBaseYPosition = slot.YPosition;
        // Function $A782 installs the complete retail mouth/fireball instruction stream.
        // Its $E84D/$E904/$E909 commands calculate and spawn bank-$86 actors; leaving this
        // assignment out produces correct boss motion with a conspicuously silent attack.
        SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.Fireballing);
        state.Function = RidleyAiFunction.CeresFireballShooting;
        state.FunctionTimer = 224;
        TickCeresRidleyFireballShooting(slot, state);
    }

    /// <summary>Applies shared-RNG horizontal jitter while Ridley holds position for the fireball attack.</summary>
    /// <param name="slot">Ridley slot providing the current attack position.</param>
    /// <param name="state">Attack target and movement state used to aim the shooting phase.</param>
    private void TickCeresRidleyFireballShooting(RoomEnemySlot slot, RidleyEnemyState state)
    {
        ushort random = _nextRandom!();
        int jitter = random & 7;
        if ((random & 0x8000) != 0)
            jitter = -jitter;
        AccelerateRidleyToward(
            slot,
            state,
            unchecked((ushort)(state.FireballBaseXPosition + jitter)),
            unchecked((ushort)(state.FireballBaseYPosition + jitter)),
            0);
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((short)state.FunctionTimer < 0)
        {
            state.HoverCounter = 0;
            state.Function = RidleyAiFunction.CeresHovering;
        }
    }

    /// <summary>Accelerates Ridley through the aimed lunge and transitions when its attack phase completes.</summary>
    /// <param name="slot">Ridley slot receiving movement and used for attack-boundary checks.</param>
    /// <param name="state">Lunge timer and velocity state.</param>
    /// <param name="samus">Required active player position for selecting the lunge target.</param>
    /// <exception cref="InvalidOperationException">The lunge is run without an active Samus actor.</exception>
    private static void TickCeresRidleyLunge(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Ceres Ridley lunge requires the active Samus actor.");

        ushort targetY = unchecked((short)(samus.YPosition - 132)) < 0
            ? (ushort)64
            : unchecked((ushort)(samus.YPosition - 68));
        AccelerateRidleyToward(slot, state, samus.XPosition, targetY, 13);
        bool reached = IsWithinRidleyRectangle(slot, samus.XPosition, targetY, 2, 2);
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if (reached || (short)state.FunctionTimer < 0)
        {
            state.HoverCounter = 0;
            state.Function = RidleyAiFunction.CeresHovering;
        }
    }

    /// <summary>Steers Ridley to the swoop start point before initializing the descending phase.</summary>
    /// <param name="slot">Ridley slot used for position checks.</param>
    /// <param name="state">Movement and swoop phase state.</param>
    private static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        AccelerateRidleyToward(slot, state, 192, 80, 1);
        if (unchecked((short)(slot.YPosition - 96)) < 0)
        {
            state.Function = RidleyAiFunction.CeresSwoopDescendingAimingDown;
            state.FunctionTimer = 10;
            state.SwoopAngleAccumulator = 0;
        }
    }

    /// <summary>Advances one timed swoop arc segment and switches to its configured next phase at timer underflow.</summary>
    /// <param name="state">Ridley state containing the angle, speed magnitude, timer, and current function.</param>
    /// <param name="angleDelta">Signed angular adjustment applied for this update.</param>
    /// <param name="targetAngle">Angle at which this segment clamps its sweep.</param>
    /// <param name="targetMagnitude">Speed magnitude approached during the segment.</param>
    /// <param name="nextFunction">Function selected when the segment timer expires.</param>
    /// <param name="nextTimer">Timer value installed for the next phase.</param>
    private static void TickCeresRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        ushort nextTimer)
    {
        UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((short)state.FunctionTimer < 0)
        {
            state.Function = nextFunction;
            state.FunctionTimer = nextTimer;
        }
    }

    /// <summary>Approaches the phase's speed magnitude and angle, then derives signed axis velocities from the trigonometry table.</summary>
    /// <param name="state">Ridley movement state receiving the updated magnitude, angle, and velocities.</param>
    /// <param name="angleDelta">Signed angular adjustment for this update.</param>
    /// <param name="targetAngle">Terminal angle for the current swoop segment.</param>
    /// <param name="targetMagnitude">Magnitude approached in increments of at most 32.</param>
    private static void UpdateRidleySwoopVelocity(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude)
    {
        int magnitude = state.SwoopSpeedMagnitude;
        if (magnitude < targetMagnitude)
            magnitude = Math.Min(targetMagnitude, magnitude + 32);
        else if (magnitude > targetMagnitude)
            magnitude = Math.Max(targetMagnitude, magnitude - 32);
        state.SwoopSpeedMagnitude = unchecked((ushort)magnitude);

        ushort angle = unchecked((ushort)(state.SwoopAngleAccumulator + angleDelta));
        // Native CMP branches on N, not signed less-than (N xor V). Preserve word
        // wrapping, particularly the $8000 recovery target after a rightward swoop.
        bool negativeDifference = unchecked((short)(angle - targetAngle)) < 0;
        if (angleDelta < 0 ? negativeDifference : !negativeDifference)
            angle = unchecked((ushort)targetAngle);
        state.SwoopAngleAccumulator = angle;
        byte phase = (byte)(angle >> 8);
        state.HorizontalVelocity = EnemyTrigonometryTables.MultiplySignedSine((ushort)magnitude, phase);
        state.VerticalVelocity = EnemyTrigonometryTables.MultiplySignedSine((ushort)magnitude, unchecked((byte)(phase + 64)));
    }

    /// <summary>Starts the real retreat by selecting upward movement and lowering Ridley's minimum Y bound.</summary>
    /// <param name="state">Ridley state whose retreat function and vertical range are changed.</param>
    private static void BeginCeresRidleyRetreat(RidleyEnemyState state)
    {
        state.Function = RidleyAiFunction.CeresRealRetreatRising;
        state.MinimumY = unchecked((ushort)-192);
    }

    /// <summary>Steers the high-hit-count fake retreat toward its staging point before beginning the rise.</summary>
    /// <param name="slot">Ridley slot used to test arrival at the staging X coordinate.</param>
    /// <param name="state">Movement state advanced toward the fake-retreat staging point.</param>
    private static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // Shared Ridley function $BD9A steers toward ($C0,$80). Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(slot, state, 192, 128, 1);
        if (unchecked((short)(slot.XPosition - 192)) >= 0)
            state.Function = RidleyAiFunction.CeresFakeRetreatRising;
    }

    /// <summary>Raises Ridley during the fake retreat and starts the Baby's fall and retrieval delay at the height threshold.</summary>
    /// <param name="slot">Ridley slot supplying the rise threshold.</param>
    /// <param name="state">Ridley and Baby state updated for the fake retreat transition.</param>
    private static void TickCeresRidleyFakeRetreatRising(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // $BD:BC raises Ridley past Y=$20 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent falling function and a
        // 21-underflow wait before Ridley changes to the retrieval pose.
        state.MinimumY = unchecked((ushort)-192);
        AccelerateRidleyToward(slot, state, 192, unchecked((ushort)-128), 1);
        if (unchecked((short)(slot.YPosition - 32)) < 0)
        {
            state.BabyFunction = 0xbeca;
            state.Function = RidleyAiFunction.CeresWaitBeforeRetrievingBaby;
            state.FunctionTimer = 21;
        }
    }

    /// <summary>Moves Ridley's hand toward the falling Baby and begins the real retreat when their native rectangles overlap.</summary>
    /// <param name="slot">Ridley slot defining the moving hand position.</param>
    /// <param name="state">Baby position and function plus Ridley's movement state.</param>
    private static void TickCeresRidleyRetrieveBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $BE03 flies Ridley's hand toward the falling Baby. The native collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        ushort handTargetX = unchecked((ushort)(state.BabyXPosition - 10));
        ushort handTargetY = unchecked((ushort)(state.BabyYPosition - 56));
        AccelerateRidleyToward(slot, state, handTargetX, handTargetY, 12);

        ushort handX = unchecked((ushort)(slot.XPosition + 14));
        ushort handY = unchecked((ushort)(slot.YPosition + 66));
        if (Math.Abs(unchecked((short)(state.BabyXPosition - handX))) >= 8 ||
            Math.Abs(unchecked((short)(state.BabyYPosition - handY))) >= 8)
        {
            return;
        }

        state.BabyFunction = 0xbeb3;
        state.VerticalVelocity = unchecked((ushort)-512);
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Updates the attached Baby position or its falling motion according to the Baby's native function word.</summary>
    /// <param name="slot">Ridley slot used as the Baby's carried-position anchor.</param>
    /// <param name="state">Combined Ridley extension holding Baby position, velocity, and function.</param>
    /// <exception cref="InvalidDataException">The Baby function word has no translated behavior.</exception>
    private static void TickCeresBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        switch (state.BabyFunction)
        {
            case CeresEnemyCodePointers.UpdateBabyMetroidPosition_CarriedInArms:
                // Before the fake retreat, the Baby remains at Ridley's original left-hand
                // anchor. This function is also why the retrieval target begins coherent
                // even though the Baby is not a separate RoomEnemySlot.
                state.BabyXPosition = unchecked((ushort)(slot.XPosition - 16));
                state.BabyYPosition = unchecked((ushort)(slot.YPosition + 22));
                return;

            case CeresEnemyCodePointers.DropBabyMetroid:
                state.BabyYSubposition = 0;
                state.BabyVerticalVelocity = 0;
                state.BabyFunction = 0xbedc;
                goto case CeresEnemyCodePointers.BabyMetroidDropped;

            case CeresEnemyCodePointers.BabyMetroidDropped:
                state.BabyVerticalVelocity = unchecked((ushort)(state.BabyVerticalVelocity + 8));
                (state.BabyYPosition, state.BabyYSubposition) = IntegrateUnclampedAxis(
                    state.BabyYPosition,
                    state.BabyYSubposition,
                    state.BabyVerticalVelocity);
                if (unchecked((short)(state.BabyYPosition - 192)) >= 0)
                {
                    state.BabyYPosition = 192;
                    state.BabyFunction = CeresEnemyCodePointers.RTS_A6BF19;
                }
                return;

            case CeresEnemyCodePointers.UpdateBabyMetroidPosition_CarriedInFeet:
                // Once the hand rectangles overlap, the Baby follows the grasp point for
                // the real retreat and disappears with Ridley at the status-one handoff.
                state.BabyXPosition = unchecked((ushort)(slot.XPosition + 14));
                state.BabyYPosition = unchecked((ushort)(slot.YPosition + 66));
                return;

            case CeresEnemyCodePointers.RTS_A6BF19:
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres Baby Metroid function $A6:{state.BabyFunction:X4} is not translated.");
        }
    }

    /// <summary>Accelerates Ridley upward until he clears the off-screen threshold, then starts the retreat delay.</summary>
    /// <param name="slot">Ridley slot whose Y position determines when the delay begins.</param>
    /// <param name="state">Retreat movement and timer state.</param>
    private void TickCeresRidleyRetreat(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $A6:A971 accelerates toward the off-screen point ($00C0,$FF80). The signed test
        // is performed before the common movement pass; once the origin is above Y=-128
        // the 64-frame delay is installed and $A6:A99D falls through into its first tick.
        AccelerateRidleyToward(slot, state, 192, unchecked((ushort)-128), 1);
        if (unchecked((short)(slot.YPosition + 128)) < 0)
        {
            state.Function = RidleyAiFunction.CeresRetreatDelay;
            state.FunctionTimer = 64;
            TickCeresRidleyRetreatDelay(state);
        }
    }

    /// <summary><c>Function_RidleyCeres_RealRetreat_WritePalettesSpawnWalls</c> ($A6:A9A0).</summary>
    private void TickCeresRidleyRetreatDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((short)state.FunctionTimer >= 0)
            return;

        // $A6:A9A5 spawns the two $E23F wall actors before the following
        // dispatcher switches BG mode. They are not decorative bookkeeping:
        // Mode 7 has no BG2, so these OBJ strips are what keep the chamber
        // visible around the rotating Ridley/Baby image.
        SpawnCeresRidleyMode7Walls();
        state.HorizontalVelocity = 0;
        state.VerticalVelocity = 0;
        state.TailFunctionIndex = 0;
        state.Function = RidleyAiFunction.CeresPublishEscapeHandoff;

        // $A6:A9E3 replaces colors 1..15 of BG palette five. $A6:AA01 is
        // copied to colors 1..8 of BG palette two and OBJ palette seven.
        (CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley retreat requires installed colors.")).ApplyRetreat(_cgram!);
    }

    /// <summary>Adjusts each signed axis velocity toward a target using the selected cartridge inertia divisor.</summary>
    /// <param name="slot">Current Ridley position used to determine each axis error.</param>
    /// <param name="state">Velocity state updated independently for X and Y.</param>
    /// <param name="targetX">Target room-space X coordinate.</param>
    /// <param name="targetY">Target room-space Y coordinate.</param>
    /// <param name="divisorIndex">Index into the native inertia-divisor definition.</param>
    private static void AccelerateRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        ushort targetX,
        ushort targetY,
        int divisorIndex)
    {
        // Division truncates like the SNES hardware quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        ushort divisor = RidleyInertiaDefinitions.Divisor(divisorIndex);

        state.HorizontalVelocity = AccelerateAxis(
            state.HorizontalVelocity, slot.XPosition, targetX, divisor);
        state.VerticalVelocity = AccelerateAxis(
            state.VerticalVelocity, slot.YPosition, targetY, divisor);
    }

    /// <summary>
    /// One axis of <c>$A6:D62F</c> (Y) / <c>$A6:D6A6</c> (X). The SEC/SBC distance test's
    /// carry feeds the first ADC/SBC of the velocity chain, and each later ADC/SBC consumes
    /// the previous one's carry, so crossing zero while reversing adds one more unit.
    /// </summary>
    private static ushort AccelerateAxis(ushort velocity, ushort position, ushort target, ushort divisor)
    {
        ushort difference = unchecked((ushort)(position - target));
        if (difference == 0)
            return velocity;
        bool carry = position >= target;
        bool targetBelow = (difference & 0x8000) != 0;
        ushort magnitude = targetBelow ? unchecked((ushort)-difference) : difference;
        // Hardware quotient; a zero quotient is promoted to one.
        ushort step = (ushort)Math.Max(1, magnitude / divisor);

        if (targetBelow)
        {
            if ((velocity & 0x8000) != 0)
            {
                Add(step);
                Add(step);
            }
            Add(step);
            // CMP #$0500 : BMI keeps any value whose signed difference is negative.
            if (unchecked((short)(velocity - 0x0500)) >= 0)
                velocity = 0x0500;
        }
        else
        {
            if ((velocity & 0x8000) == 0)
            {
                Subtract(step);
                Subtract(step);
            }
            Subtract(step);
            // CMP #$FB00 : BPL keeps any value whose signed difference is non-negative.
            if (unchecked((short)(velocity - 0xfb00)) < 0)
                velocity = 0xfb00;
        }
        return velocity;

        void Add(ushort operand)
        {
            int result = velocity + operand + (carry ? 1 : 0);
            velocity = unchecked((ushort)result);
            carry = result > ushort.MaxValue;
        }
        void Subtract(ushort operand)
        {
            int result = velocity - operand - (carry ? 0 : 1);
            velocity = unchecked((ushort)result);
            carry = result >= 0;
        }
    }

    /// <summary>Integrates Ridley's 8.8 velocity into room position, clamps to movement bounds, and optionally emits left-wall impact.</summary>
    /// <param name="slot">Enemy slot whose integer and fractional coordinates are advanced.</param>
    /// <param name="state">Velocity and movement bounds used and updated by integration.</param>
    /// <param name="ceresWallImpact">Whether the Ceres liftoff wall-impact condition is enabled for this pass.</param>
    private void IntegrateRidleyMovement(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        bool ceresWallImpact = false)
    {
        // $A6:D86B calls the impact routine only when proposed X is strictly below the
        // left bound, before zeroing X velocity. Vertical speed can meet its threshold
        // even when horizontal motion is slow; the right and vertical clamps do not call it.
        var proposedX = IntegrateUnclampedAxis(slot.XPosition, slot.XSubposition, state.HorizontalVelocity);
        var proposedY = IntegrateUnclampedAxis(slot.YPosition, slot.YSubposition, state.VerticalVelocity);
        state.HitRoomBoundary = unchecked((short)(proposedX.Position - state.MinimumX)) < 0 ||
            unchecked((short)(proposedX.Position - state.MaximumX)) >= 0 ||
            unchecked((short)(proposedY.Position - state.MinimumY)) < 0 ||
            unchecked((short)(proposedY.Position - state.MaximumY)) >= 0;
        if (ceresWallImpact && unchecked((short)(proposedX.Position - state.MinimumX)) < 0 &&
            Math.Max(Math.Abs((int)unchecked((short)state.HorizontalVelocity)),
                Math.Abs((int)unchecked((short)state.VerticalVelocity))) >= RidleyWallImpactDefinitions.MinimumSpeed)
        {
            EarthquakeType = RidleyWallImpactDefinitions.EarthquakeType;
            EarthquakeTimer = RidleyWallImpactDefinitions.Duration;
        }
        (slot.XPosition, slot.XSubposition, state.HorizontalVelocity) = IntegrateAxis(
            slot.XPosition,
            slot.XSubposition,
            state.HorizontalVelocity,
            state.MinimumX,
            state.MaximumX);
        (slot.YPosition, slot.YSubposition, state.VerticalVelocity) = IntegrateAxis(
            slot.YPosition,
            slot.YSubposition,
            state.VerticalVelocity,
            state.MinimumY,
            state.MaximumY);
    }

    /// <summary>Integrates one 8.8 axis and clamps at either bound, zeroing velocity on contact.</summary>
    /// <param name="position">Current integer coordinate.</param>
    /// <param name="subposition">Current fractional coordinate whose high byte accumulates velocity fraction.</param>
    /// <param name="velocityWord">Signed 8.8 velocity word.</param>
    /// <param name="minimum">Inclusive lower movement bound.</param>
    /// <param name="maximum">Upper movement bound at which motion is clamped.</param>
    /// <returns>Updated integer position, fractional position, and velocity.</returns>
    private static (ushort Position, ushort Subposition, ushort Velocity) IntegrateAxis(
        ushort position,
        ushort subposition,
        ushort velocityWord,
        ushort minimum,
        ushort maximum)
    {
        // Ridley's velocities are signed 8.8 values, but the original routine accumulates
        // their fractional byte into the *high* byte of the ordinary 16-bit subposition.
        // Reproduce the byte carry explicitly; a generic 16.16 helper would be off by 256.
        int fraction = (subposition >> 8) + (velocityWord & 0xff);
        subposition = unchecked((ushort)((subposition & 0x00ff) | ((fraction & 0xff) << 8)));
        ushort next = unchecked((ushort)(position + (sbyte)(velocityWord >> 8) + (fraction >> 8)));

        if (unchecked((short)(next - minimum)) < 0)
            return (minimum, subposition, 0);
        if (unchecked((short)(next - maximum)) >= 0)
            return (maximum, subposition, 0);
        return (next, subposition, velocityWord);
    }

    /// <summary>Integrates one 8.8 axis without applying movement bounds.</summary>
    /// <param name="position">Current integer coordinate.</param>
    /// <param name="subposition">Current fractional coordinate.</param>
    /// <param name="velocityWord">Signed 8.8 velocity word.</param>
    /// <returns>Updated integer and fractional coordinates with the cartridge's byte carry.</returns>
    private static (ushort Position, ushort Subposition) IntegrateUnclampedAxis(
        ushort position,
        ushort subposition,
        ushort velocityWord)
    {
        int fraction = (subposition >> 8) + (velocityWord & 0xff);
        ushort nextSubposition = unchecked((ushort)(
            (subposition & 0x00ff) | ((fraction & 0xff) << 8)));
        ushort nextPosition = unchecked((ushort)(
            position + (sbyte)(velocityWord >> 8) + (fraction >> 8)));
        return (nextPosition, nextSubposition);
    }

    /// <summary>Tests inclusive rectangle overlap between Ridley and a target, accounting for both actor radii.</summary>
    /// <param name="slot">Ridley slot whose center and radii define one rectangle.</param>
    /// <param name="centerX">Target rectangle center in room coordinates.</param>
    /// <param name="centerY">Target rectangle center in room coordinates.</param>
    /// <param name="radiusX">Target half-width.</param>
    /// <param name="radiusY">Target half-height.</param>
    /// <returns><see langword="true"/> when the rectangles overlap, including their boundaries.</returns>
    private static bool IsWithinRidleyRectangle(
        RoomEnemySlot slot,
        ushort centerX,
        ushort centerY,
        ushort radiusX,
        ushort radiusY) =>
        // $A9:EF06 tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        Math.Abs(unchecked((short)(slot.XPosition - centerX))) < unchecked((ushort)(slot.XRadius + radiusX + 1)) &&
            Math.Abs(unchecked((short)(slot.YPosition - centerY))) < unchecked((ushort)(slot.YRadius + radiusY + 1));

    /// <summary>Creates seven inactive tail links at their authored rest lengths and initial angles.</summary>
    /// <returns>Tail segments in root-to-tip order, ready for the liftoff transition to activate.</returns>
    private static RidleyTailSegment[] CreateInitialRidleyTailSegments()
    {
        // Each successive link begins one ideal angular separation beyond its predecessor.
        // The chosen base/shaft/tip lengths remain required inputs in the tail catalog.
        var segments = new RidleyTailSegment[7];
        for (int index = 0; index < segments.Length; index++)
        {
            segments[index] = new RidleyTailSegment
            {
                MovementDirection = 0x8000,
                Distance = RidleyTailDefinitions.RestDistance(index),
                Angle = RidleyTailDefinitions.InitialAngle(index),
                StaggerAngle = 0x0011,
            };
        }
        return segments;
    }

    /// <summary>Counts down the post-door delay and switches from waiting to the eye fade on signed underflow.</summary>
    /// <param name="state">Ridley state containing the dispatcher timer and fade phase.</param>
    private static void TickCeresRidleyInitialDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((short)state.FunctionTimer >= 0)
            return;

        state.Function = RidleyAiFunction.FadeInEyes;
        state.FadePaletteOffset = 0;
        state.FunctionTimer = 0;
    }

    /// <summary>Applies the next authored eye-palette fade row, then enables movement when the fade sequence completes.</summary>
    /// <param name="state">Ridley state whose fade offset and movement-enable flag are advanced.</param>
    private void TickCeresRidleyEyeFade(RidleyEnemyState state)
    {
        // The original INC/BNE sequence advances once per AI call for every reachable
        // counter value, resetting the scratch timer immediately afterward.
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 1));
        if (state.FunctionTimer == 0)
            return;
        state.FunctionTimer = 0;

        CeresRidleyEyeFadeStep fadeStep =
            CeresRidleyEyeFadeDefinitions.Get(state.FadePaletteOffset);
        if (fadeStep.IsComplete)
        {
            state.FadePaletteOffset = 0;
            state.Function = RidleyAiFunction.FadeInBody;
            state.MovementAnimationEnabled = 1;
            return;
        }

        (CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley eye fade requires installed colors."))
            .ApplyEyeFade(_cgram!, fadeStep.PaletteRow);
        state.FadePaletteOffset = unchecked((ushort)(state.FadePaletteOffset + 1));
    }

    /// <summary>Applies body palette rows on alternating calls and begins the roar wait after the last row.</summary>
    /// <param name="slot">Ridley slot whose collision property is restored when the fade completes.</param>
    /// <param name="state">Fade offset, dispatcher timer, and music request state.</param>
    /// <exception cref="InvalidOperationException">The current fade offset does not map to an installed body-palette row.</exception>
    private void TickCeresRidleyBodyFade(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 1));
        if (state.FunctionTimer < 2)
            return;
        state.FunctionTimer = 0;

        int row = state.FadePaletteOffset /
            (CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort));
        if (CeresRidleyColors is { } bodyColors &&
            state.FadePaletteOffset %
                (CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort)) == 0 &&
            row < CeresRidleyPaletteRomData.BodyFadeRowCount)
            bodyColors.ApplyBodyFade(_cgram!, row);
        else throw new InvalidOperationException(
            $"Ceres Ridley body-fade offset {state.FadePaletteOffset} is outside the installed color rows.");
        state.FadePaletteOffset = unchecked((ushort)(state.FadePaletteOffset +
            CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort)));
        if (state.FadePaletteOffset < CeresRidleyPaletteRomData.BodyFadeRowCount *
            CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort))
            return;

        // CeresRidley_Func_5 clears tangible bit $0400 after the final palette row, waits
        // five dispatcher calls, then begins the opening-roar list.
        slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.FadePaletteOffset = 0;
        state.Function = RidleyAiFunction.WaitBeforeRoar;
        state.FunctionTimer = 4;
        // `$A6:A449` queues track five immediately after the final body-palette row.
        // This is the Ceres battle/escape track; it intentionally continues after Ridley
        // retreats, so omitting this single publication silences both scenes.
        state.MusicRequest = MusicCommand.SelectTrack(5);
    }

    /// <summary>Installs an instruction-list pointer and resets its interpreter timers.</summary>
    /// <param name="slot">Ridley slot whose animation interpreter is updated.</param>
    /// <param name="instruction">Instruction-list address selected for the next animation update.</param>
    private static void SetRidleyInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Validates that a slot is the initialized slot-zero Ceres Ridley and returns its extension state.</summary>
    /// <param name="slot">Enemy slot entering a Ceres Ridley routine.</param>
    /// <returns>The initialized Ceres Ridley state.</returns>
    /// <exception cref="InvalidOperationException">The slot is not definition $E13F in slot zero or its state is absent.</exception>
    private RidleyEnemyState RequireCeresRidley(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != CeresRidleyDefinition ||
            slot.SlotIndex != 0 ||
            _ridleyState is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed a Ceres Ridley routine without " +
                "the $E13F slot-zero state extension.");
        }

        return _ridleyState;
    }
}
