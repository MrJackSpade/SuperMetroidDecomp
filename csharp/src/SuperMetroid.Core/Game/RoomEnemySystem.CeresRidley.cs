using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const ushort CeresRidleyDefinition = 0xe13f;

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
            if (!projectile.IsActive ||
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
        SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E538);
        slot.PaletteIndex = 0x0e00;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native bits $1000 and $0400 are proven by the initializer, but $1000 has not yet
        // been given a cross-enemy semantic name. Preserve it raw; $0400 already has the
        // verified IgnoreSamusCollision identity used by the scheduler.
        slot.Properties = unchecked((ushort)(slot.Properties | 0x1000));
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.XPosition = 0x00ba;
        slot.YPosition = 0x00a9;
        CeresStatus = 0;

        _ridleyState = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = 0,
            HitCounter = 0,
            SpritemapPaletteIndex = 0x0e00,
            CommonDrawPaletteIndex = 0x0e00,
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
            BabyInstruction = 0xbf31,
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
            TailMinimumClockwiseAngle = 0x3ff0,
            TailMaximumCounterClockwiseAngle = 0x4040,
            TailWhipTargetClockwiseAngle = 0xffff,
            TailWhipTargetCounterClockwiseAngle = 0xffff,
            TailWhipRequest = 0,
            TailExtensionSpeed = 0x00f0,
            IdealInterSegmentTailAngle = 0x0010,
            TailSegments = CreateInitialRidleyTailSegments(),
        };

        // WriteColorsToTargetPalette($140, $A6:E16F, $20) installs the Ceres door and Baby
        // Metroid container palettes. This runtime exposes the final target directly in
        // CGRAM, matching the established Ceres-door palette seam.
        _cgram!.LoadFromBus(_bus!, 0xa6e16f, colorCount: 32, destinationIndex: 0x140 / 2);

        // The native loop clears target-palette byte offsets $1E2..$1FE: OBJ palette seven,
        // colors one through fifteen. Color zero belongs to the shared transparent backdrop
        // and is intentionally left untouched.
        for (int color = 0x1e2 / 2; color <= 0x1fe / 2; color++)
            _cgram.SetColor(color, 0);
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
                // Enemy AI cannot run until the runtime has completed room loading, so the
                // native door_transition_flag_enemies test is necessarily clear here.
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
                    SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E690);
                    state.FunctionTimer = 252;
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                break;

            case RidleyAiFunction.WaitBeforeLiftoff:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    state.FadePaletteOffset = 0;
                    SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E91D);
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
                SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E548);
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
                    SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E658);
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
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    // $A6:A9A5 spawns the two $E23F wall actors before the following
                    // dispatcher switches BG mode. They are not decorative bookkeeping:
                    // Mode 7 has no BG2, so these OBJ strips are what keep the chamber
                    // visible around the rotating Ridley/Baby image.
                    SpawnCeresRidleyMode7Walls();
                    state.HorizontalVelocity = 0;
                    state.VerticalVelocity = 0;
                    state.Function = RidleyAiFunction.CeresPublishEscapeHandoff;

                    // $A6:A9E3 replaces colors 1..15 of BG palette five. $A6:AA01 is
                    // copied to colors 1..8 of BG palette two and OBJ palette seven.
                    _cgram!.LoadFromBus(_bus!, 0xa6a9e3, 15, 0x00a2 / 2);
                    _cgram.LoadFromBus(_bus!, 0xa6aa01, 8, 0x0042 / 2);
                    _cgram.LoadFromBus(_bus!, 0xa6aa01, 8, 0x01e2 / 2);
                }
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
                if (CeresStatus == 1 && state.Mode7Active)
                    TickCeresRidleyMode7Getaway(state, samus, slot.FrameCounter);
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
            IntegrateRidleyMovement(slot, state);
            TickRidleyWingAnimation(state);
            TickRidleyTail(slot, state, samus);
            UpdateCeresRidleyHealthPalette(state);
        }
        if (CeresStatus == 0)
            TickCeresBaby(slot, state);
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
                : (ushort)0x0e00;
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

        int paletteIndex = state.HitCounter < 70 ? 0 : 2;
        const int colorsPerHealthPalette = 14;
        _cgram!.LoadFromBus(
            _bus!,
            0xa6e46a + paletteIndex * colorsPerHealthPalette * 2,
            colorCount: colorsPerHealthPalette,
            destinationIndex: 0x01e2 / 2);
    }

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
        if (samus is not null && unchecked((short)(samus.Health - 30)) < 0)
        {
            state.FightMode = 0;
            BeginCeresRidleyRetreat(state);
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
        SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E73A);
        state.Function = RidleyAiFunction.CeresFireballShooting;
        state.FunctionTimer = 224;
        TickCeresRidleyFireballShooting(slot, state);
    }

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

    private void TickCeresRidleyLunge(
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

    private void TickCeresRidleySwoopMoveToPosition(
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

        int angle = unchecked((short)state.SwoopAngleAccumulator);
        angle = angleDelta < 0
            ? Math.Max(targetAngle, angle + angleDelta)
            : Math.Min(targetAngle, angle + angleDelta);
        state.SwoopAngleAccumulator = unchecked((ushort)angle);

        // Math_MultBySin/Cos uses the signed 8-bit sine table with the high byte of the
        // 16-bit angle accumulator. Rounding to the nearest signed table byte reproduces
        // the table's integer amplitude before its magnitude multiply/truncate.
        int phase = unchecked((byte)(state.SwoopAngleAccumulator >> 8));
        state.HorizontalVelocity = unchecked((ushort)MultiplyBySineTable(magnitude, phase));
        state.VerticalVelocity = unchecked((ushort)MultiplyBySineTable(magnitude, phase + 64));
    }

    private static short MultiplyBySineTable(int magnitude, int phase)
    {
        int sine = (int)Math.Round(
            Math.Sin((phase & 0xff) * (Math.PI * 2 / 256)) * 127,
            MidpointRounding.AwayFromZero);
        return unchecked((short)(Math.Sign(sine) * (magnitude * Math.Abs(sine) >> 8)));
    }

    private static void BeginCeresRidleyRetreat(RidleyEnemyState state)
    {
        state.Function = RidleyAiFunction.CeresRealRetreatRising;
        state.MinimumY = unchecked((ushort)-192);
    }

    private void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // Shared Ridley function $BD9A steers toward ($C0,$80). Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(slot, state, 192, 128, 1);
        if (unchecked((short)(slot.XPosition - 192)) >= 0)
            state.Function = RidleyAiFunction.CeresFakeRetreatRising;
    }

    private void TickCeresRidleyFakeRetreatRising(
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

    private void TickCeresRidleyRetrieveBaby(RoomEnemySlot slot, RidleyEnemyState state)
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

    private void TickCeresRidleyRetreat(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $A6:A971 accelerates toward the off-screen point ($00C0,$FF80). The signed test
        // is performed before the common movement pass, so the 64-frame delay begins on
        // the call after Ridley's integrated origin first crosses Y=-128.
        AccelerateRidleyToward(slot, state, 192, unchecked((ushort)-128), 1);
        if (unchecked((short)(slot.YPosition + 128)) < 0)
        {
            state.Function = RidleyAiFunction.CeresRetreatDelay;
            state.FunctionTimer = 64;
        }
    }

    private void AccelerateRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        ushort targetX,
        ushort targetY,
        int divisorIndex)
    {
        // g_byte_A6D712 is the exact acceleration divisor table consumed by Ridley_Func_106.
        // Division truncates like the SNES hardware quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        ushort divisor = _bus!.ReadByte(0xa6d712 + divisorIndex);
        if (divisor == 0)
            throw new InvalidDataException($"Ridley acceleration divisor {divisorIndex} is zero.");

        state.HorizontalVelocity = AccelerateAxis(
            state.HorizontalVelocity,
            unchecked((short)(slot.XPosition - targetX)),
            divisor);
        state.VerticalVelocity = AccelerateAxis(
            state.VerticalVelocity,
            unchecked((short)(slot.YPosition - targetY)),
            divisor);
    }

    private static ushort AccelerateAxis(ushort velocityWord, short distance, ushort divisor)
    {
        if (distance == 0)
            return velocityWord;

        int step = Math.Max(1, Math.Abs((int)distance) / divisor);
        int velocity = unchecked((short)velocityWord);
        if (distance > 0)
        {
            if (velocity >= 0)
                velocity -= step * 2;
            velocity -= step;
        }
        else
        {
            if (velocity < 0)
                velocity += step * 2;
            velocity += step;
        }

        return unchecked((ushort)Math.Clamp(velocity, -1280, 1280));
    }

    private static void IntegrateRidleyMovement(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
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

    private static bool IsWithinRidleyRectangle(
        RoomEnemySlot slot,
        ushort centerX,
        ushort centerY,
        ushort radiusX,
        ushort radiusY) =>
        Math.Abs(unchecked((short)(slot.XPosition - centerX))) < radiusX &&
            Math.Abs(unchecked((short)(slot.YPosition - centerY))) < radiusY;

    private static RidleyTailSegment[] CreateInitialRidleyTailSegments()
    {
        // InitializeTailParts at $A6:D2D6 copies these five seven-word ROM tables into
        // the bank-$7E tail workspace. Keeping the data together makes the otherwise odd
        // initial $4000,$4010... phase staggering directly auditable against the cartridge.
        ushort[] distances = [0x0200, 0x0800, 0x0800, 0x0800, 0x0800, 0x0800, 0x0500];
        ushort[] angles = [0x4000, 0x4010, 0x4020, 0x4030, 0x4040, 0x4050, 0x4060];
        var segments = new RidleyTailSegment[7];
        for (int index = 0; index < segments.Length; index++)
        {
            segments[index] = new RidleyTailSegment
            {
                MovementDirection = 0x8000,
                Distance = distances[index],
                Angle = angles[index],
                StaggerAngle = 0x0011,
            };
        }
        return segments;
    }

    private static void TickCeresRidleyInitialDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((short)state.FunctionTimer >= 0)
            return;

        state.Function = RidleyAiFunction.FadeInEyes;
        state.FadePaletteOffset = 0;
        state.FunctionTimer = 0;
    }

    private void TickCeresRidleyEyeFade(RidleyEnemyState state)
    {
        // The original INC/BNE sequence advances once per AI call for every reachable
        // counter value, resetting the scratch timer immediately afterward.
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 1));
        if (state.FunctionTimer == 0)
            return;
        state.FunctionTimer = 0;

        byte colorStep = _bus!.ReadByte(0xa6e269 + state.FadePaletteOffset);
        if (colorStep == 0xff)
        {
            state.FadePaletteOffset = 0;
            state.Function = RidleyAiFunction.FadeInBody;
            state.MovementAnimationEnabled = 1;
            return;
        }

        int source = 0xa6e2aa + colorStep * 6;
        _cgram!.LoadFromBus(_bus, source, colorCount: 3, destinationIndex: 252);
        state.FadePaletteOffset = unchecked((ushort)(state.FadePaletteOffset + 1));
    }

    private void TickCeresRidleyBodyFade(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 1));
        if (state.FunctionTimer < 2)
            return;
        state.FunctionTimer = 0;

        int source = 0xa6e30a + state.FadePaletteOffset;
        _cgram!.LoadFromBus(_bus!, source, colorCount: 11, destinationIndex: 0x122 / 2);
        _cgram.LoadFromBus(_bus!, source, colorCount: 11, destinationIndex: 0x1e2 / 2);
        state.FadePaletteOffset = unchecked((ushort)(state.FadePaletteOffset + 22));
        if (state.FadePaletteOffset < 0x0160)
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

    private static void SetRidleyInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

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
