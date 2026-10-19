using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ceres Ridley species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CeresRidleyBehavior
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <summary>Invokes the enemy AI table for Ridley's shared movement callbacks.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Offsets the private Baby/door drawing hook during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves the door and Mode-7 wall enemy records beside Ridley.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Performs Samus and projectile contact against Ridley's extended body.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Advances Ridley's manually drawn tail segments.</summary>
    private readonly RidleyTail _ridleyTail;
    /// <summary>Publishes Ceres escape and self-destruct room status.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Connects Ceres Ridley's enemy AI, drawing, collision and room-main collaborators.</summary>
    internal CeresRidleyBehavior(CeresRidleyBehaviorDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _ridleyTail = dependencies.RidleyTail;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _collision = dependencies.Collision;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _roomStatus = dependencies.RoomStatus;
    }

    /// <summary>
    /// Runs Ceres Ridley's bank-$A6 shot-overlap seam against the five ordinary Samus
    /// projectile slots. The native shot handler increments a dedicated counter and never
    /// subtracts enemy health; the projectile owner performs its normal explosion change.
    /// </summary>
    public int ResolveCeresRidleyProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);

        if (_ridleyEncounter.Ridley is null || _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
            return 0;

        RoomEnemySlot slot = _slotTable._slots[0];
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.RidleyCeres ||
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
                    projectile.Position.Whole,
                    projectile.Radius))
            {
                continue;
            }
            if (!projectiles.EnemyImpacts.TryStartEnemyImpact(sharedProjectiles, projectile.SlotIndex))
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
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.RidleyCeres)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} entered Ceres Ridley's shot branch as " +
                $"definition ${(int)slot.EnemyDefinitionPointer:X4}.");
        }

        // `$DF99-$DFA9` starts with Y=13, shifts the old nonzero timer once, and selects
        // 14 only when that shift produced carry. In host terms this is precisely old bit
        // zero; testing bit one (or simply toggling the new value) changes repeated-hit
        // timing and eventually moves the escape threshold relative to the hurt palette.
        slot.FlashTimer = NativeCountdown.FromRaw(!slot.FlashTimer.IsZero && (slot.FlashTimer.RawValue & 1) != 0
            ? (ushort)14
            : (ushort)13);
        state.HitCounter++;

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
    internal void InitializeCeresRidley(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Ceres Ridley requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }

        // $A6:A10B-A12E clears the shared boss tilemap workspace, disables the minimap, and
        // marks the room explored. Those consumers are not yet represented by RoomEnemySystem;
        // every actor-owned write that follows is retained below.
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native bits $1000 and $0400 are proven by the initializer, but $1000 has not yet
        // been given a cross-enemy semantic name. Preserve it raw; $0400 already has the
        // verified IgnoreSamusCollision identity used by the scheduler.
        slot.Properties = slot.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.Position = slot.Position.WithWhole(new(186, 169));
        _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            HitCounter = 0,
            SpritemapPaletteIndex = SnesObjPalettes.Index7,
            CommonDrawPaletteIndex = SnesObjPalettes.Index7,
            MovementAnimationEnabled = false,
            FacingDirection = RidleyFacing.Left,
            IdleTailWhipEnabled = true,
            TailDamage = 15,
            Velocity = new EightEightVector<RoomPixels>(0, 0),
            MovementMinimum = new Position16<RoomPixels>(40, unchecked((ushort)-32)),
            MovementMaximum = new Position16<RoomPixels>(224, 176),
            BabyProgram = AnimationProgramCursor<CeresBabyProgram>.Start(CeresBabyProgram.Initial),
            BabyInstructionTimer = 1,
            BabyFunction = CeresBabyFunction.CarriedInArms,
            BabyCurrentPose = null,
            BabyPosition = FixedPosition<RoomPixels>.FromWhole(slot.Position.Whole + CeresBabyArmsAnchor),
            WingFrame = 5,
            WingAnimationTimer = NativeCountdown.Start(0),
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = RidleyTailFunction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailWhipRequest = 0,
            TailExtensionSpeed = 240,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentAngle,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        StartRidleyProgram(_ridleyEncounter.Ridley, slot, RidleyBodyProgram.Initial);
        // `$A6:A280` ends Ceres Ridley's initialization by queuing music zero (stop).
        _audio.QueueInitializationMusicDelayed8(MusicCommand.Stop);

        // WriteColorsToTargetPalette($140, $A6:E16F, $20) installs the Ceres door and Baby
        // Metroid container palettes. This runtime exposes the final target directly in
        // CGRAM, matching the established Ceres-door palette seam.
        (_presentation.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley requires installed start colors.")).ApplyStart(_presentation.Cgram);

        // The native loop clears target-palette byte offsets $1E2..$1FE: OBJ palette seven,
        // colors one through fifteen. Color zero belongs to the shared transparent backdrop
        // and is intentionally left untouched.
        _presentation.BossPalettes.CeresRidleyRevealedObjects.Fill(Bgr555.Black);
    }

    /// <summary>
    /// Ports the Ceres dispatcher reached from $A6:A288 through the end of the battle. The
    /// reveal, liftoff, hover, two cartridge-authored termination conditions, upward retreat,
    /// palette handoff, and <c>ceres_status = 1</c> publication all retain their ROM addresses.
    /// Ridley's later Mode-7 getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    internal void RunCeresRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus,
        VramWriteQueue? vramWriteQueue)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireCeresRidley(slot);
        slot.Health = short.MaxValue;

        // $A6:A29F runs after the boss dispatcher and before movement. None of the
        // translated dispatcher branches mutate the flash timer, so selecting the two
        // draw palettes here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);

        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                // A35B waits even though enemy visual instructions run during fade.
                if (_roomStatus.EnemyDoorTransitionActive)
                    break;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = NativeCountdown.Start(512);
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
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    StartRidleyProgram(state, slot, RidleyBodyProgram.OpeningRoar);
                    state.FunctionTimer = NativeCountdown.Start(252);
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                break;

            case RidleyAiFunction.WaitBeforeLiftoff:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    state.FadePaletteOffset = 0;
                    StartRidleyProgram(state, slot, RidleyBodyProgram.TransitionToFlying);
                    // Function_Ridley_RoarBeforeFly at $A6:A4B4-A4CB primes the wing
                    // timer, enables every tail segment, and selects neutral tail AI in
                    // the same transition that installs instruction list $E91D.
                    state.WingAnimationTimer = NativeCountdown.Start(8);
                    state.WingAnimationTimerDelta = 8;
                    foreach (RidleyTailSegment segment in state.TailSegments)
                        segment.Active = true;
                    state.TailFunctionIndex = RidleyTailFunction.Neutral;
                    state.Function = RidleyAiFunction.ClearVelocity;
                }
                break;

            case RidleyAiFunction.ClearVelocity:
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                break;

            case RidleyAiFunction.CeresLiftoffAccelerating:
                // $A6:A6AF adds -16 to the 8.8 Y velocity before the common movement pass.
                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - 16))));
                if (unchecked((short)(slot.Position.Y.Whole - 112)) < 0)
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
                StartRidleyProgram(state, slot, RidleyBodyProgram.CeresLunge);
                state.Function = RidleyAiFunction.CeresLungeMain;
                state.FunctionTimer = NativeCountdown.Start(64);
                TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresLungeMain:
                TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresSwoopSetup:
                state.Function = RidleyAiFunction.CeresSwoopMoveToPosition;
                state.FunctionTimer = NativeCountdown.Start(10);
                state.SwoopAngleAccumulator = SnesAngle.Zero;
                state.IdleTailWhipEnabled = false;
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
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    state.Function = RidleyAiFunction.CeresHovering;
                    state.HoverCounter = 0;
                    state.IdleTailWhipEnabled = true;
                }
                break;

            case RidleyAiFunction.CeresFakeRetreatMoveToPosition:
                TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresFakeRetreatRising:
                TickCeresRidleyFakeRetreatRising(slot, state);
                break;

            case RidleyAiFunction.CeresWaitBeforeRetrievingBaby:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    StartRidleyProgram(state, slot, RidleyBodyProgram.RetrieveBabyMetroid);
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
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyEscaped;
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
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            IntegrateRidleyMovement(slot, state, ceresWallImpact: true);
            TickRidleyWingAnimation(state);
            _ridleyTail.RandomlyUpdateCeresRidleyTailCurliness(state);
            _ridleyTail.TickRidleyTail(slot, state, samus);
            UpdateCeresRidleyHealthPalette(state);
        }
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            TickCeresBaby(slot, state);
    }

    /// <summary>OBJ palette zero, which the tail and wings select on hurt-flash updates.</summary>
    private static readonly SnesObjAttributeWord HurtFlashSpritemapPalette =
        SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 0, priority: 0);

    /// <summary>
    /// Ports <c>RidleyHurtFlashHandling</c> at $A6:D4DA and the common extended-enemy
    /// palette override at $A0:9497. Ridley's composite is split between those two writers:
    /// the ordinary body uses the current execution counter, while the manual tail/wings
    /// deliberately test the counter after an increment.
    /// </summary>
    internal static void UpdateRidleyHurtFlashPalettes(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        int enemyMainExecutionCounter)
    {
        state.CommonDrawPaletteIndex = new SnesObjAttributeWord(!slot.FlashTimer.IsZero &&
            (enemyMainExecutionCounter & 2) != 0
                ? (ushort)0
                : checked((ushort)slot.PaletteIndex));

        state.SpritemapPaletteIndex = slot.FlashTimer.RawValue > 1 &&
            (unchecked((ushort)(enemyMainExecutionCounter + 1)) & 2) != 0
                ? HurtFlashSpritemapPalette
                : SnesObjPalettes.Index7;
    }

    /// <summary>
    /// Ports $A6:D4B5-$D4D7. Ceres uses shot count in place of health, including the
    /// original missing branch after the 90-shot comparison: 50-69 selects palette zero,
    /// and every value from 70 onward selects palette two.
    /// </summary>
    internal void UpdateCeresRidleyHealthPalette(RidleyEnemyState state)
    {
        if (state.FightMode == RidleyFightMode.NotFighting || state.HitCounter < 50)
            return;

        int paletteIndex = state.HitCounter < 70
            ? CeresRidleyPaletteRomData.HealthMidRow
            : CeresRidleyPaletteRomData.HealthLateRow;
        (_presentation.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley damage requires installed health colors."))
            .ApplyHealth(_presentation.Cgram, paletteIndex);
    }

    /// <summary>Applies the crossing-frame -16 then +20 velocity changes before shared Ridley motion.</summary>
    private static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // The fallthrough from A6AF deliberately applies both -16 and +20 on the crossing
        // frame. Do not combine these into a single acceleration outside this state.
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue + 20))));
        if (unchecked((short)(slot.Position.Y.Whole - 80)) < 0)
        {
            state.Function = RidleyAiFunction.CeresHovering;
            state.FightMode = RidleyFightMode.Fighting;
        }
    }

    /// <summary>Tests the scripted shot-count and Samus-energy exits, then admits attacks only inside the hover box.</summary>
    private void TickCeresRidleyHover(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        // Ceres Ridley never loses the ordinary enemy-health word. His shot AI increments
        // this separate counter and A6E8 treats 100 contacts as the scripted win condition.
        if (state.HitCounter >= 100)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            state.Function = RidleyAiFunction.CeresFakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
            return;
        }

        // The other original termination route is Samus energy below 30. A synthetic room
        // fixture may omit Samus, in which case only the shot-counter route is meaningful.
        // $A6:A706 jumps into the real retreat, so its first acceleration runs this call.
        if (samus is not null && samus.Health < 30)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(slot, state);
            return;
        }

        // A6:A763 is the shared cooldown/return-to-hover movement: accelerate toward
        // ($00C0,$0064) using divisor table entry zero.
        AccelerateRidleyToward(slot, state, target: new(192, 100), divisorIndex: 0);
        if (!IsWithinRidleyRectangle(slot, new(192, 100), new(8, 8)))
        {
            // Shitroid_Func_2 returns zero on rectangle overlap. $A6:A6E8 therefore waits
            // only while Ridley is still outside the hover box, with $7C as the authentic
            // timeout. The previous translation inverted this branch and selected attacks
            // while he was still above the arena, making every fireball route hit its
            // intentional 48-frame bailout before the mouth animation could begin.
            state.HoverCounter++;
            if (state.HoverCounter < 124)
                return;
        }

        state.Function = CeresRidleyAttackDefinitions.SelectAttack(_room.Random.Read());
        state.HoverCounter = 0;
    }

    /// <summary>Steers Ridley into his fireball position and installs the mouth/fireball instruction stream.</summary>
    private void TickCeresRidleyFireballMove(RoomEnemySlot slot, RidleyEnemyState state)
    {
        int verticalVelocity = state.Velocity.Y.SignedRawValue;
        int magnitude = Math.Max(128, Math.Abs(verticalVelocity));
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(verticalVelocity < 0 ? -magnitude : magnitude))));
        AccelerateRidleyToward(slot, state, new(slot.Position.X.Whole, 88), 0);

        if (unchecked((short)(slot.Position.Y.Whole - 80)) < 0)
        {
            state.HoverCounter++;
            if (state.HoverCounter >= 48)
                state.Function = RidleyAiFunction.CeresSwoopSetup;
            return;
        }
        if (unchecked((short)(slot.Position.Y.Whole - 128)) >= 0)
            return;

        state.FireballBasePosition = slot.Position.Whole;
        // Function $A782 installs the complete retail mouth/fireball instruction stream.
        // Its $E84D/$E904/$E909 commands calculate and spawn bank-$86 actors; leaving this
        // assignment out produces correct boss motion with a conspicuously silent attack.
        StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);
        state.Function = RidleyAiFunction.CeresFireballShooting;
        state.FunctionTimer = NativeCountdown.Start(224);
        TickCeresRidleyFireballShooting(slot, state);
    }

    /// <summary>Uses the current RNG word and native carry arithmetic to select the next fireball target.</summary>
    private void TickCeresRidleyFireballShooting(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $A6:A7F9 reads RandomNumberSeed without generating a number.
        int random = _room.Random.Read();
        ushort jitter = (ushort)(random & 7);
        if (unchecked((short)random) < 0)
            jitter = unchecked((ushort)-jitter);
        // Both targets use ADC without CLC. Carry is clear on entry (the enemy AI dispatcher's
        // CLC/ADC at $A0:9083 and $A6:A7BA's in-range compare), so the X sum carries into Y.
        bool carry = false;
        ushort x = SnesCarry.AddWithCarry(checked((ushort)state.FireballBasePosition.X), jitter, ref carry);
        ushort y = SnesCarry.AddWithCarry(checked((ushort)state.FireballBasePosition.Y), jitter, ref carry);
        AccelerateRidleyToward(slot, state, new(x, y), 0);
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (state.FunctionTimer.IsNegative)
        {
            state.HoverCounter = 0;
            state.Function = RidleyAiFunction.CeresHovering;
        }
    }

    /// <summary>Advances the attack lunge until its position threshold hands control to the swoop.</summary>
    private static void TickCeresRidleyLunge(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Ceres Ridley lunge requires the active Samus actor.");

        ushort targetY = unchecked((short)(samus.Position.Y.Whole - 132)) < 0
            ? (ushort)64
            : unchecked((ushort)(samus.Position.Y.Whole - 68));
        AccelerateRidleyToward(slot, state, new(samus.Position.X.Whole, targetY), 13);
        bool reached = IsWithinRidleyRectangle(slot, new(samus.Position.X.Whole, targetY), new(2, 2));
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (reached || state.FunctionTimer.IsNegative)
        {
            state.HoverCounter = 0;
            state.Function = RidleyAiFunction.CeresHovering;
        }
    }

    /// <summary>Steers Ridley toward the swoop start point with the shared acceleration routine.</summary>
    private static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        AccelerateRidleyToward(slot, state, new(192, 80), 1);
        if (unchecked((short)(slot.Position.Y.Whole - 96)) < 0)
        {
            state.Function = RidleyAiFunction.CeresSwoopDescendingAimingDown;
            state.FunctionTimer = NativeCountdown.Start(10);
            state.SwoopAngleAccumulator = SnesAngle.Zero;
        }
    }

    /// <summary>Runs the swoop arc and publishes the one-frame tail-fling request at its bottom.</summary>
    private static void TickCeresRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        ushort nextTimer)
    {
        UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (state.FunctionTimer.IsNegative)
        {
            state.Function = nextFunction;
            state.FunctionTimer = NativeCountdown.FromRaw(nextTimer);
        }
    }

    /// <summary>Applies native signed-word comparisons while reversing Ridley's swoop velocity.</summary>
    internal static void UpdateRidleySwoopVelocity(
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
        state.SwoopSpeedMagnitude = magnitude;

        SnesAngle angle = state.SwoopAngleAccumulator.AddRaw(angleDelta);
        // Native CMP branches on N, not signed less-than (N xor V). The heading wraps around the
        // turn, particularly onto the half-turn recovery target after a rightward swoop.
        bool negativeDifference = unchecked((short)(angle.RawValue - targetAngle)) < 0;
        if (angleDelta < 0 ? negativeDifference : !negativeDifference)
            angle = SnesAngle.NormalizeRaw(targetAngle);
        state.SwoopAngleAccumulator = angle;
        byte phase = angle.TableIndex;
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(EnemyTrigonometryTables.MultiplySignedSine((ushort)magnitude, phase)));
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(EnemyTrigonometryTables.MultiplySignedSine((ushort)magnitude, unchecked((byte)(phase + 64)))));
    }

    /// <summary>Switches the encounter into the initial scripted retreat.</summary>
    private static void BeginCeresRidleyRetreat(RidleyEnemyState state)
    {
        state.Function = RidleyAiFunction.CeresRealRetreatRising;
        state.MovementMinimum = state.MovementMinimum.WithY(unchecked((ushort)-192));
    }

    /// <summary>Moves the 100-shot retreat toward the Baby retrieval staging point.</summary>
    private static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // Shared Ridley function $BD9A steers toward ($C0,$80). Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(slot, state, new(192, 128), 1);
        if (unchecked((short)(slot.Position.X.Whole - 192)) >= 0)
            state.Function = RidleyAiFunction.CeresFakeRetreatRising;
    }

    /// <summary>Raises Ridley above Y=$20 before releasing the Baby into its independent fall.</summary>
    private static void TickCeresRidleyFakeRetreatRising(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // $BD:BC raises Ridley past Y=$20 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent falling function and a
        // 21-underflow wait before Ridley changes to the retrieval pose.
        state.MovementMinimum = state.MovementMinimum.WithY(unchecked((ushort)-192));
        AccelerateRidleyToward(slot, state, new(192, unchecked((ushort)-128)), 1);
        if (unchecked((short)(slot.Position.Y.Whole - 32)) < 0)
        {
            state.BabyFunction = CeresBabyFunction.Drop;
            state.Function = RidleyAiFunction.CeresWaitBeforeRetrievingBaby;
            state.FunctionTimer = NativeCountdown.Start(21);
        }
    }

    /// <summary>Steers Ridley's hand toward the falling Baby and accepts overlapping radius-four grasp rectangles.</summary>
    private static void TickCeresRidleyRetrieveBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $BE03 flies Ridley's hand toward the falling Baby. The native collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        AccelerateRidleyToward(slot, state, state.BabyPosition.Whole - CeresRidleyHandToBaby, 12);

        Offset16<RoomPixels> fromHand = state.BabyPosition.Whole - (slot.Position.Whole + CeresBabyFeetAnchor);
        if (Math.Abs(fromHand.X) >= 8 || Math.Abs(fromHand.Y) >= 8)
        {
            return;
        }

        state.BabyFunction = CeresBabyFunction.CarriedInFeet;
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-512)));
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Keeps the Baby at Ridley's arm anchor, falls independently, then follows the grasp point on retrieval.</summary>
    private static void TickCeresBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        switch (state.BabyFunction)
        {
            case CeresBabyFunction.CarriedInArms:
                // Before the fake retreat, the Baby remains at Ridley's original left-hand
                // anchor. This function is also why the retrieval target begins coherent
                // even though the Baby is not a separate RoomEnemySlot.
                state.BabyPosition = state.BabyPosition.WithWhole(slot.Position.Whole + CeresBabyArmsAnchor);
                return;

            case CeresBabyFunction.Drop:
                state.BabyPosition = state.BabyPosition.WithFractionY(0);
                state.BabyVerticalVelocity = 0;
                state.BabyFunction = CeresBabyFunction.Dropped;
                goto case CeresBabyFunction.Dropped;

            case CeresBabyFunction.Dropped:
                state.BabyVerticalVelocity += 8;
                // The falling speed only grows from zero and the fall ends at Y=192, so it is always a
                // nonnegative 8.8 velocity well inside the signed range the integrator accepts.
                state.BabyPosition = state.BabyPosition.WithY(
                    state.BabyPosition.Y.AddEightEightToHighFraction(checked((ushort)state.BabyVerticalVelocity)));
                if (unchecked((short)(state.BabyPosition.Y.Whole - 192)) >= 0)
                {
                    state.BabyPosition = state.BabyPosition.WithWholeY(192);
                    state.BabyFunction = CeresBabyFunction.Idle;
                }
                return;

            case CeresBabyFunction.CarriedInFeet:
                // Once the hand rectangles overlap, the Baby follows the grasp point for
                // the real retreat and disappears with Ridley at the status-one handoff.
                state.BabyPosition = state.BabyPosition.WithWhole(slot.Position.Whole + CeresBabyFeetAnchor);
                return;

            case CeresBabyFunction.Idle:
                return;

            case CeresBabyFunction.None:
                throw new InvalidOperationException("The Ceres Baby Metroid has no behavior function.");

            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresBabyFunction)} {(int)state.BabyFunction:X4}.");
        }
    }

    /// <summary>Accelerates Ridley toward the offscreen retreat point and begins the 64-frame getaway delay.</summary>
    private void TickCeresRidleyRetreat(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $A6:A971 accelerates toward the off-screen point ($00C0,$FF80). The signed test
        // is performed before the common movement pass; once the origin is above Y=-128
        // the 64-frame delay is installed and $A6:A99D falls through into its first tick.
        AccelerateRidleyToward(slot, state, new(192, unchecked((ushort)-128)), 1);
        if (unchecked((short)(slot.Position.Y.Whole + 128)) < 0)
        {
            state.Function = RidleyAiFunction.CeresRetreatDelay;
            state.FunctionTimer = NativeCountdown.Start(64);
            TickCeresRidleyRetreatDelay(state);
        }
    }

    /// <summary><c>Function_RidleyCeres_RealRetreat_WritePalettesSpawnWalls</c> ($A6:A9A0).</summary>
    private void TickCeresRidleyRetreatDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        // $A6:A9A5 spawns the two $E23F wall actors before the following
        // dispatcher switches BG mode. They are not decorative bookkeeping:
        // Mode 7 has no BG2, so these OBJ strips are what keep the chamber
        // visible around the rotating Ridley/Baby image.
        SpawnCeresRidleyMode7Walls();
        state.Velocity = new EightEightVector<RoomPixels>(0, 0);
        state.TailFunctionIndex = RidleyTailFunction.None;
        state.Function = RidleyAiFunction.CeresPublishEscapeHandoff;

        // $A6:A9E3 replaces colors 1..15 of BG palette five. $A6:AA01 is
        // copied to colors 1..8 of BG palette two and OBJ palette seven.
        (_presentation.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley retreat requires installed colors.")).ApplyRetreat(_presentation.Cgram);
    }

    /// <summary>The Ceres Baby's anchor in Ridley's arms before the fake retreat.</summary>
    private static Offset16<RoomPixels> CeresBabyArmsAnchor => new(-16, 22);

    /// <summary>The Ceres Baby's anchor in Ridley's feet once retrieved, also the grasp point the retrieval compares.</summary>
    private static Offset16<RoomPixels> CeresBabyFeetAnchor => new(14, 66);

    /// <summary>The retrieval steering target relative to the falling Baby (<c>$BE03</c>).</summary>
    private static Offset16<RoomPixels> CeresRidleyHandToBaby => new(10, 56);

    /// <summary>Steers both 8.8 velocity axes toward a target, promoting zero hardware quotients to one.</summary>
    internal static void AccelerateRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisorIndex)
    {
        // Division truncates like the SNES hardware quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        ushort divisor = RidleyInertiaDefinitions.Divisor(divisorIndex);

        state.Velocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(AccelerateAxis(state.Velocity.X.RawValue, slot.Position.X.Whole, target.X, divisor)),
            new SnesSignedEightEight(AccelerateAxis(state.Velocity.Y.RawValue, slot.Position.Y.Whole, target.Y, divisor)));
    }

    /// <summary>The fastest 8.8 speed the acceleration clamps to toward a lower target: five pixels per update.</summary>
    private const ushort CeresRidleyMaximumSpeed = 1280;
    /// <summary>The fastest 8.8 speed toward a higher target, as the native two's-complement word.</summary>
    private const ushort CeresRidleyMinimumSpeed = unchecked((ushort)-1280);

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
        bool carry = SnesCarry.AfterCompare(position, target);
        bool targetBelow = unchecked((short)difference) < 0;
        ushort magnitude = targetBelow ? unchecked((ushort)-difference) : difference;
        // Hardware quotient; a zero quotient is promoted to one.
        ushort step = (ushort)Math.Max(1, magnitude / divisor);

        if (targetBelow)
        {
            if (unchecked((short)velocity) < 0)
            {
                Add(step);
                Add(step);
            }
            Add(step);
            // CMP #$0500 : BMI keeps any value whose signed difference is negative.
            if (unchecked((short)(velocity - CeresRidleyMaximumSpeed)) >= 0)
                velocity = CeresRidleyMaximumSpeed;
        }
        else
        {
            if (unchecked((short)velocity) >= 0)
            {
                Subtract(step);
                Subtract(step);
            }
            Subtract(step);
            // CMP #$FB00 : BPL keeps any value whose signed difference is non-negative.
            if (unchecked((short)(velocity - CeresRidleyMinimumSpeed)) < 0)
                velocity = CeresRidleyMinimumSpeed;
        }
        return velocity;

        void Add(ushort operand) => velocity = SnesCarry.AddWithCarry(velocity, operand, ref carry);
        void Subtract(ushort operand) => velocity = SnesCarry.SubtractWithBorrow(velocity, operand, ref carry);
    }

    /// <summary>Applies Ridley's 8.8 motion and native arena clamps, invoking impact only at the strict left bound.</summary>
    internal void IntegrateRidleyMovement(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        bool ceresWallImpact = false)
    {
        // $A6:D86B calls the impact routine only when proposed X is strictly below the
        // left bound, before zeroing X velocity. Vertical speed can meet its threshold
        // even when horizontal motion is slow; the right and vertical clamps do not call it.
        var proposedX = IntegrateUnclampedAxis(slot.Position.X.Whole, slot.Position.X.Fraction, state.Velocity.X.RawValue);
        var proposedY = IntegrateUnclampedAxis(slot.Position.Y.Whole, slot.Position.Y.Fraction, state.Velocity.Y.RawValue);
        state.HitRoomBoundary = unchecked((short)(proposedX.Position - state.MovementMinimum.X)) < 0 ||
            unchecked((short)(proposedX.Position - state.MovementMaximum.X)) >= 0 ||
            unchecked((short)(proposedY.Position - state.MovementMinimum.Y)) < 0 ||
            unchecked((short)(proposedY.Position - state.MovementMaximum.Y)) >= 0;
        if (ceresWallImpact && unchecked((short)(proposedX.Position - state.MovementMinimum.X)) < 0 &&
            Math.Max(Math.Abs((int)state.Velocity.X.SignedRawValue),
                Math.Abs((int)state.Velocity.Y.SignedRawValue)) >= RidleyWallImpactDefinitions.MinimumSpeed)
        {
            _earthquake.EarthquakeType = RidleyWallImpactDefinitions.EarthquakeType;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(RidleyWallImpactDefinitions.Duration);
        }
        (SnesFixedPosition x, ushort xVelocity) = IntegrateAxis(
            slot.Position.X,
            state.Velocity.X.RawValue,
            state.MovementMinimum.X,
            state.MovementMaximum.X);
        state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(xVelocity));
        slot.Position = slot.Position.WithX(x);
        (SnesFixedPosition y, ushort yVelocity) = IntegrateAxis(
            slot.Position.Y,
            state.Velocity.Y.RawValue,
            state.MovementMinimum.Y,
            state.MovementMaximum.Y);
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(yVelocity));
        slot.Position = slot.Position.WithY(y);
    }

    /// <summary>Integrates one 8.8 velocity into the high byte of a 16-bit subposition and clips to an arena bound.</summary>
    private static (SnesFixedPosition Position, ushort Velocity) IntegrateAxis(
        SnesFixedPosition position,
        ushort velocityWord,
        ushort minimum,
        ushort maximum)
    {
        // Ridley's velocities are signed 8.8 values that the original routine accumulates into the
        // *high* byte of the ordinary 16-bit subposition.
        SnesFixedPosition moved = position.AddEightEightToHighFraction(velocityWord);
        ushort subposition = moved.Fraction;
        ushort next = moved.Whole;

        if (unchecked((short)(next - minimum)) < 0)
            return (new(minimum, subposition), 0);
        if (unchecked((short)(next - maximum)) >= 0)
            return (new(maximum, subposition), 0);
        return (new(next, subposition), velocityWord);
    }

    /// <summary>Adds an 8.8 velocity to a native whole/subpixel pair before boundary checks.</summary>
    private static (ushort Position, ushort Subposition) IntegrateUnclampedAxis(
        ushort position,
        ushort subposition,
        ushort velocityWord)
    {
        SnesFixedPosition moved = new SnesFixedPosition(position, subposition).AddEightEightToHighFraction(velocityWord);
        return (moved.Whole, moved.Fraction);
    }

    /// <summary>Tests an inclusive actor rectangle for hover, lunge or retrieval handoffs.</summary>
    internal static bool IsWithinRidleyRectangle(
        RoomEnemySlot slot,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        // $A9:EF06 tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        Math.Abs(unchecked((short)(slot.Position.X.Whole - center.X))) < unchecked((ushort)(slot.Radius.X + radius.X + 1)) &&
            Math.Abs(unchecked((short)(slot.Position.Y.Whole - center.Y))) < unchecked((ushort)(slot.Radius.Y + radius.Y + 1));

    /// <summary>Advances the initial battle hold before Ridley's eye fade begins.</summary>
    internal static void TickCeresRidleyInitialDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        state.Function = RidleyAiFunction.FadeInEyes;
        state.FadePaletteOffset = 0;
        state.FunctionTimer = NativeCountdown.Start(0);
    }

    /// <summary>Advances eye palette rows on each AI call before enabling the body fade.</summary>
    internal void TickCeresRidleyEyeFade(RidleyEnemyState state)
    {
        // The original INC/BNE sequence advances once per AI call for every reachable
        // counter value, resetting the scratch timer immediately afterward.
        state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
        if (state.FunctionTimer.IsZero)
            return;
        state.FunctionTimer = NativeCountdown.Start(0);

        CeresRidleyEyeFadeStep fadeStep =
            CeresRidleyEyeFadeDefinitions.Get(state.FadePaletteOffset);
        if (fadeStep.IsComplete)
        {
            state.FadePaletteOffset = 0;
            state.Function = RidleyAiFunction.FadeInBody;
            state.MovementAnimationEnabled = true;
            return;
        }

        (_presentation.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley eye fade requires installed colors."))
            .ApplyEyeFade(_presentation.Cgram, fadeStep.PaletteRow);
        state.FadePaletteOffset++;
    }

    /// <summary>Reveals body palette rows, clears intangible collision and queues the battle track at the final row.</summary>
    internal void TickCeresRidleyBodyFade(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
        if (state.FunctionTimer.RawValue < 2)
            return;
        state.FunctionTimer = NativeCountdown.Start(0);

        // The cartridge steps a colour offset one row of colours at a time; the row it reaches is the fade's progress.
        int row = state.FadePaletteOffset;
        if (_presentation.CeresRidleyColors is { } bodyColors &&
            row < CeresRidleyPaletteRomData.BodyFadeRowCount)
            bodyColors.ApplyBodyFade(_presentation.Cgram, row);
        else throw new InvalidOperationException(
            $"Ceres Ridley body-fade row {row} is outside the installed color rows.");
        state.FadePaletteOffset++;
        if (state.FadePaletteOffset < CeresRidleyPaletteRomData.BodyFadeRowCount)
            return;

        // CeresRidley_Func_5 clears tangible bit $0400 after the final palette row, waits
        // five dispatcher calls, then begins the opening-roar list.
        slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.FadePaletteOffset = 0;
        state.Function = RidleyAiFunction.WaitBeforeRoar;
        state.FunctionTimer = NativeCountdown.Start(4);
        // `$A6:A449` queues track five immediately after the final body-palette row.
        // This is the Ceres battle/escape track; it intentionally continues after Ridley
        // retreats, so omitting this single publication silences both scenes.
        state.MusicRequest = MusicCommand.SelectTrack(5);
    }

    /// <summary>Restarts Ridley's native enemy instruction cursor and frame timer at a selected list.</summary>
    internal static void StartRidleyProgram(RidleyEnemyState state, RoomEnemySlot slot, RidleyBodyProgram program)
    {
        state.BodyProgram = AnimationProgramCursor<RidleyBodyProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Routes exploding normal bombs to Ceres Ridley's shot counter after common bank-$A0 overlap.</summary>
    internal void RegisterCeresRidleyBombReactions(EnemySharedReactions table)
    {
        // `$A6:DF8A` branches on area index before it ever reaches common vulnerability damage. Ceres therefore
        // treats an exploding normal bomb exactly like an ordinary shot for its cinematic counter: the bank-$A0
        // walker has already marked the physical bomb, while Ridley alternates hurt timing and advances the 100-hit
        // escape condition.
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres &&
                                  c.SelectedShotAi == EnemyInteractionCallback.RidleyShot,
            c => ResolveCeresRidleyShotAfterCollision(c.Enemy));
    }

    /// <summary>Ports Ceres Ridley's initialization AI (the bank-$A6 Ridley initializer for the Ceres header).</summary>
    internal void RunCeresRidleyInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3) =>
        InitializeCeresRidley(slot);

    /// <summary>Ports Ceres Ridley's main dispatcher.</summary>
    internal void RunCeresRidleyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunCeresRidleyMain(slot, frame.Samus, frame.VramWriteQueue);

    /// <summary>Native wing frame deltas selected from Ridley's movement speed.</summary>
    private static readonly ushort[] CeresRidleyWingAnimationDeltas =
        [12, 14, 16, 18, 28, 32, 40, 48];

    /// <summary>Ports $A6:D97D-$D9EC after the common Ridley movement pass.</summary>
    internal static void TickRidleyWingAnimation(RidleyEnemyState state)
    {
        int absoluteXVelocity = Math.Abs(state.Velocity.X.SignedRawValue);
        int absoluteYVelocity = Math.Abs(state.Velocity.Y.SignedRawValue);
        int largestAxisVelocity = Math.Max(absoluteXVelocity, absoluteYVelocity);
        if (largestAxisVelocity == 0)
        {
            state.WingAnimationTimerDelta = 0;
        }
        else
        {
            // $A6:D9CF isolates bits 8..11 after multiplying the 8.8 velocity by four,
            // converts the resulting word-table byte index, and clamps that index to $E.
            int tableIndex = Math.Min(7, SnesEightEight.WholeOf(largestAxisVelocity << 2) % 16);
            ushort delta = CeresRidleyWingAnimationDeltas[tableIndex];
            if (state.Velocity.Y.SignedRawValue >= 0)
                delta >>= 1;
            state.WingAnimationTimerDelta = delta;
        }

        state.WingAnimationTimer = NativeCountdown.FromRaw(unchecked((ushort)(
            state.WingAnimationTimer.RawValue - state.WingAnimationTimerDelta)));
        if (!state.WingAnimationTimer.IsNegative)
            return;

        state.WingAnimationTimer = NativeCountdown.Start(32);
        state.WingFrame = (state.WingFrame + 1) % 10;
    }

    /// <summary>Ports DrawRidleyTail $A6:DB2A and DrawRidleyWings $A6:DAD8.</summary>
    internal void DrawRidleySupplementalSprites(
        OamBuffer oam,
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        if ((slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres && _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped) ||
            !state.MovementAnimationEnabled)
            return;

        if (state.TailSegments.Length == 7)
        {
            RidleyTailSegment tip = state.TailSegments[6];
            // The tip's sixteen poses split the byte angle into sixteenths.
            int tipIndex = unchecked((byte)(
                tip.Angle + state.TailSegments[5].Angle + 8)) / 16;
            EnemyFrameId tipFrame = RidleySupplementalVisualDefinitions.TailTipFrames.Of(
                tipIndex);
            DrawRidleyWorldSpritemap(
                oam,
                state.SpritemapPaletteIndex,
                tip,
                tipFrame,
                camera);

            // $A6:DB59-$DBBF walks back from segment five to the base after emitting the
            // tip. Lower OAM indices win equal-priority overlap, so reversing this visually
            // changes the curl even though every individual piece is otherwise correct.
            for (int index = 5; index >= 0; index--)
                DrawRidleyWorldSpritemap(
                    oam,
                    state.SpritemapPaletteIndex,
                    state.TailSegments[index],
                    RidleySupplementalVisualDefinitions.SegmentFrames.Of(index),
                    camera);
        }

        if (state.FacingDirection == RidleyFacing.Turning)
            return;
        int wingPointerIndex = (state.FacingDirection == RidleyFacing.Left ? 0 : 10) + state.WingFrame;
        EnemyFrameId wingFrame = RidleySupplementalVisualDefinitions.WingFrames.Of(
            wingPointerIndex);
        _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Frame = wingFrame, Origin = new(unchecked((ushort)(slot.Position.X.Whole - camera.X)), unchecked((ushort)(slot.Position.Y.Whole - camera.Y))), PaletteBits = state.SpritemapPaletteIndex.PaletteBits, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = unchecked((ushort)(slot.Position.Y.Whole - camera.Y)) < 256 });
    }

    /// <summary>
    /// Replays the direct branch to <c>$A6:A2E3</c> taken while Ridley's movement and
    /// animation word is zero. This is deliberately separate from the deferred hook below:
    /// the cartridge executes this path inside Ridley's enemy-main call, before the ordinary
    /// drawing queues, so the Baby appears with the room and before Ridley's composite.
    /// </summary>
    public void DrawCeresRidleyImmediateBabyAndDoor(
        OamBuffer oam,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(oam);
        _room.EnsureLoaded();
        // The shared Ridley state also exists in Lower Norfair. Only Ceres
        // Ridley's initializer installs this private Baby/door draw path.
        if (_ridleyEncounter.Ridley is null ||
            _slotTable._slots[0].EnemyDefinitionPointer != EnemyDefinitionId.RidleyCeres ||
            _ridleyEncounter.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(oam, camera);
    }

    /// <summary>
    /// Replays the enemy-graphics-drawn hook installed by <c>$A6:A2E5</c>. The cartridge
    /// installs it only after Ridley's animation word becomes nonzero; it then emits the
    /// same private Baby instruction list after the ordinary enemy layer loop.
    /// </summary>
    public void DrawCeresRidleyPostEnemyHook(
        OamBuffer oam,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(oam);
        _room.EnsureLoaded();
        if (_ridleyEncounter.Ridley is null ||
            _slotTable._slots[0].EnemyDefinitionPointer != EnemyDefinitionId.RidleyCeres ||
            !_ridleyEncounter.Ridley.MovementAnimationEnabled)
            return;

        DrawCeresRidleyBabyAndDoor(oam, camera);
    }

    /// <summary>Emits the private Baby map and hidden door overlay with the native secondary quake offset.</summary>
    private void DrawCeresRidleyBabyAndDoor(
        OamBuffer oam,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.Ridley
            ?? throw new InvalidOperationException("Ceres Ridley Baby draw requires active state.");

        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            if (AdvanceCeresBabyDrawInstruction(state) is { } babyFrame)
            {
                _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Frame = babyFrame, Origin = state.BabyPosition.Whole.ToScreen(camera).ToOam(), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = state.BabyPosition.Whole.ToScreen(camera).Y < 256 });
            }
        }

        // Enemy one is the ordinary Ridley-room door. Its $F69F instruction hands its
        // drawing to Ridley's scene when the common enemy pass intentionally hides the actor;
        // $A6:A2FA then draws this fixed overlay after the Baby. This is a native cross-slot
        // draw hook, not a replacement for the door actor's own instruction interpreter.
        RoomEnemySlot door = _slotTable._slots[1];
        if (door.Entity is IDrawnByCeresRidleyScene { HandsDrawingToRidley: true })
        {
            // The private Ceres hook does not call WriteEnemyOAM and therefore owns a
            // second, cartridge-authored quake adjustment at `$A6:A2F2-$A314`. Its `Y`
            // index is the low two bits of the earthquake timer, but the table contains
            // words and the assembly omits an ASL. Preserve that retail byte-index read:
            // bytes 00,00,FC,FF become signed X offsets 0,0,-4,-1.
            int quakeXOffset = CeresDoorQuakeDefinitions.XOffset(_earthquake.EarthquakeTimer.RawValue);
            _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Frame = CeresDoorFrameDefinitions.RidleyPrivateOverlayFrame, Origin = new(unchecked((ushort)(door.Position.X.Whole - camera.X + quakeXOffset)), unchecked((ushort)(door.Position.Y.Whole - camera.Y))), PaletteBits = EnemyPaletteBits.Palette2, BaseTileIndex = 0 });
        }
    }

    /// <summary>
    /// Runs the Baby's typed cutscene program one draw update and returns its current spritemap. $A6:DBE7 counts the
    /// elapsed updates up and advances once they reach the pose's duration; the next pose starts at one.
    /// </summary>
    internal EnemyFrameId? AdvanceCeresBabyDrawInstruction(RidleyEnemyState state)
    {
        if (state.BabyPoseDuration != 0 && state.BabyPoseDuration != state.BabyInstructionTimer)
        {
            state.BabyInstructionTimer++;
            return CeresBabyFrame(state.BabyCurrentPose);
        }

        AnimationProgramCursor<CeresBabyProgram> cursor = state.BabyProgram;
        NativeCountdown counter = NativeCountdown.Start(0);
        AnimationProgramAdvance<CeresBabyPose> advance = CeresBabyAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, _) => ApplyCeresBabyCue(state, cue));
        state.BabyProgram = cursor;
        if (advance.Kind != AnimationProgramAdvanceKind.Show)
            throw new InvalidOperationException($"The Ceres Baby's program ended a draw update with {advance.Kind}.");
        state.BabyPoseDuration = advance.Duration;
        state.BabyInstructionTimer = 1;
        state.BabyCurrentPose = advance.Frame;
        return CeresBabyFrame(state.BabyCurrentPose);
    }

    /// <summary>The frame the draw shows for <paramref name="pose"/>, or none before the Baby's first pose.</summary>
    private static EnemyFrameId? CeresBabyFrame(CeresBabyPose? pose) =>
        pose is { } shown ? CeresBabyFrameDefinitions.ShownFrames.Of(shown) : null;

    /// <summary>Carries out one cue of the Baby's program, returning the program a branch continues at, or null to run on.</summary>
    private CeresBabyProgram? ApplyCeresBabyCue(RidleyEnemyState state, CeresBabyCue cue)
    {
        switch (cue.Kind)
        {
            case CeresBabyCueKind.CryOrGoTo:
                // $A6:BFC9: while the baby is falling ($880C nonzero) it always cries and
                // continues. Otherwise it samples the live RNG word's low bit without
                // calling GenerateRandomNumber; a set bit jumps silently ($A6:BFD5).
                if (state.BabyVerticalVelocity == 0 && (_room.Random.Read() & 1) != 0)
                    return cue.Target;
                _audio.QueueEnemySound(SoundEffectLibrary3Sounds.CeresBabyCry, maximumQueued: 6);
                return null;
            case CeresBabyCueKind.UpdateColors:
                if (_presentation.CeresRidleyColors is { } babyColors)
                    babyColors.ApplyBaby(_presentation.Cgram, cue.PaletteRow);
                else throw new InvalidOperationException(
                    "Ceres Ridley requires installed Baby Metroid colors.");
                return null;
            case CeresBabyCueKind.GoToIfMoving:
                return state.BabyVerticalVelocity != 0 ? cue.Target : null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresBabyCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Allocates two real enemy records for the OBJ wall strips shown during Mode 7.</summary>
    private void SpawnCeresRidleyMode7Walls()
    {
        // SpawnEnemy consumes the first two free native slots and initializes the literal
        // population records embedded at $A6:AA2F/$AA3F. Keeping these as real slots makes
        // their layer-two OAM order, off-screen processing, and $F7A5 visibility dispatcher
        // flow through the same scheduler as the cartridge instead of painting host panels.
        SpawnCeresRidleyMode7Wall(xPosition: 8, parameter1: 5);
        SpawnCeresRidleyMode7Wall(xPosition: 248, parameter1: 6);
    }

    /// <summary>Initializes one native wall actor at its authored X coordinate and variant parameter.</summary>
    private void SpawnCeresRidleyMode7Wall(ushort xPosition, ushort parameter1)
    {
        int slotIndex = Array.FindIndex(_slotTable._slots, slot => slot.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            throw new InvalidOperationException("Ceres Ridley getaway has no free enemy slot for a Mode-7 wall.");

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.CeresDoor);
        RoomEnemyPopulationRecord population = new(
            EnemyDefinitionId.CeresDoor,
            Position: new(xPosition, 127),
            InitializationParameter: 0,
            Properties: new EnemyPropertyWord(
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen),
            ExtraProperties: new EnemyExtraPropertyWord(EnemyExtraProperties.None),
            Parameter1: parameter1,
            Parameter2: 0);
        RoomEnemySlot wall = _slotTable._slots[slotIndex];
        _dispatch.InitializeSlotFromDefinition(wall, population, definition);
        _dispatch.RunInitializationAi(wall);
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * RoomEnemySystem.NativeSlotSize));
    }

    /// <summary>Projects a Ridley component's world spritemap into OAM using the current camera.</summary>
    private void DrawRidleyWorldSpritemap(
        OamBuffer oam,
        SnesObjAttributeWord paletteIndex,
        RidleyTailSegment segment,
        EnemyFrameId frame,
        Position16<RoomPixels> camera)
    {
        Position16<ScreenPixels> screen = segment.Position.ToScreen(camera);
        _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Frame = frame, Origin = screen.ToOam(), PaletteBits = paletteIndex.PaletteBits, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = screen.Y < 256 });
    }

    /// <summary>Tests Samus against Ridley's extended body and applies contact response after native invincibility gating.</summary>
    internal bool ResolveRidleyBodySamusContact(SamusState samus)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        // $A0:9A8B-$9A98 gates ordinary contact only on the contact-damage mode and
        // Samus's invincibility timer. It does not inspect the bank-$90 knockback-active
        // word. The extra host guard previously made a stale/ongoing aerial knockback
        // suppress every later Ridley overlap even after invincibility had expired.
        RoomEnemySlot slot = _slotTable._slots[0];
        if (_ridleyEncounter.Ridley is null ||
            !RidleyBehavior.IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
            slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres && _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ||
            !samus.InvincibilityTimer.IsZero)
        {
            return false;
        }

        if (slot.Frame is null ||
            slot.Properties.HasAny(
                EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return false;
        }

        if (!_collision.TryFindExtendedHitboxCallback(
                slot,
                samus.Position.Whole,
                samus.Kinematics.Radius,
                selectShotCallback: false,
                out EnemyInteractionCallback? touchAi))
        {
            return false;
        }

        if (touchAi != EnemyInteractionCallback.RidleyExtendedTouch)
        {
            throw new InvalidDataException(
                $"Ridley extended body selected touch AI {touchAi?.ToString() ?? "(unnamed)"}, expected " +
                $"{EnemyInteractionCallback.RidleyExtendedTouch} from map {slot.Frame}.");
        }

        // $A6:DF59 enters the common no-death-check handler. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        _collision.ResolveNormalEnemyTouch(
            slot,
            samus,
            skipDeathAnimation: true);
        return true;
    }

    /// <summary>
    /// Shared port of the rectangle walk used by $A0:9A5A (Samus contact) and $A0:9B7F
    /// (projectile contact). Every component offset and every rectangle comes from the
    /// active extended spritemap; the tiny 8x8 radius in Ridley's enemy header is not his
    /// body and must never be substituted for this data.
    /// </summary>
    internal static bool ExtendedSpritemapOverlapsRectangle(
        RoomEnemySlot slot,
        Position16<RoomPixels> subject,
        Extent16<RoomPixels> subjectRadius)
    {
        int subjectLeft = unchecked((ushort)(subject.X - subjectRadius.X));
        int subjectRight = unchecked((ushort)(subject.X + subjectRadius.X));
        int subjectTop = unchecked((ushort)(subject.Y - subjectRadius.Y));
        int subjectBottom = unchecked((ushort)(subject.Y + subjectRadius.Y));
        var components =
            RidleyCollisionDefinitions.ComponentsAt(EnemyFrames.Spritemap(slot.Frame));
        foreach (RidleyCollisionComponent component in components)
        {
            Position16<RoomPixels> componentOrigin = slot.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (RidleyCollisionHitbox hitbox in component.Hitboxes)
            {
                int left = unchecked((ushort)(componentX + hitbox.Left));
                int top = unchecked((ushort)(componentY + hitbox.Top));
                int right = unchecked((ushort)(componentX + hitbox.Right));
                int bottom = unchecked((ushort)(componentY + hitbox.Bottom));

                // $A0:9B14/$9B20/$9B2C/$9B38 use signed comparisons and treat touching
                // right/bottom edges as non-overlap. Ceres coordinates remain in the low
                // positive room range, so these integer comparisons are the same result.
                if (unchecked((short)(left - subjectRight)) <= 0 &&
                    unchecked((short)(right - subjectLeft)) > 0 &&
                    unchecked((short)(top - subjectBottom)) <= 0 &&
                    unchecked((short)(bottom - subjectTop)) > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>A signed 8.8 coefficient of one, unit scale, as the getaway's setup installs it.</summary>
    private static readonly SnesSignedEightEight Mode7UnitCoefficient = SnesSignedEightEight.FromSignedRaw(256);

    /// <summary>
    /// Ports $A6:AA54. Every value below is a literal PPU/actor word installed by the retail
    /// setup routine; in particular B and C intentionally begin at $0100 before the first
    /// getaway-table entry replaces the matrix.
    /// </summary>
    private static void SetupCeresRidleyMode7(RidleyEnemyState state)
    {
        state.Mode7Active = true;
        state.Mode7Finished = false;
        state.Mode7TableIndex = 0;
        state.Mode7Angle = SnesAngle.FromRaw(1);
        state.Mode7Offset = new Offset16<Mode7Pixels>(-128, 32);
        state.Mode7Zoom = 2048;
        state.Mode7MatrixA = Mode7UnitCoefficient;
        state.Mode7MatrixB = Mode7UnitCoefficient;
        state.Mode7MatrixC = Mode7UnitCoefficient;
        state.Mode7MatrixD = Mode7UnitCoefficient;
        state.Mode7Center = new Position16<Mode7Pixels>(64, 64);
        state.Mode7BabyFrame = 0;
        state.Mode7WingFrame = 0;
    }

    /// <summary>
    /// Room main <c>MainASM_HandleCeresRidleyGetawayCutscene</c> ($8F:E571) and
    /// <c>HandleCeresRidleyGetawayCutsceneIfActive</c> ($A6:AAAF). It runs after the
    /// frame's enemy AI and Samus processing, so the getaway's first step shares the frame
    /// in which Ridley's escape handoff publishes Ceres status one.
    /// </summary>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, int nmiFrameCounter)
    {
        if (_ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyEscaped)
            return;
        RidleyEnemyState state = _ridleyEncounter.Ridley ?? throw new InvalidOperationException(
            "Ceres status one requires the Ceres Ridley getaway state.");
        if (state.Mode7Finished)
            return;
        TickCeresRidleyMode7Getaway(state, samus, nmiFrameCounter);
    }

    /// <summary>
    /// Ports room-main function $A6:AABD through its $FFFF table terminator. This routine
    /// owns presentation only: ordinary Ridley has already been hidden, while Samus remains
    /// in room-world coordinates exactly as she does during the boss-room getaway.
    /// </summary>
    internal void TickCeresRidleyMode7Getaway(
        RidleyEnemyState state,
        SamusState? samus,
        int nmiFrameCounter)
    {
        int tableIndex = state.Mode7TableIndex;
        state.Mode7TableIndex = tableIndex + 1;

        // HandleCeresRidleyGetawayCutscene queues the departure sound on the first table
        // entry only ($A6:AABD -> QueueSfx2_Max6($4E)). Tying the request to the native
        // table index preserves the same one-shot behavior after saves/replays and pauses.
        if (tableIndex == 0)
            _audio.QueueEnemySound(SoundEffectLibrary2Sounds.CeresRidleyGetaway, maximumQueued: 6);

        // At frame $68 (byte index $D0), `$90:E119` replaces Samus's movement/hack handlers so the
        // rotating boss image cannot overlap her from the following frame onward.
        if (tableIndex == 104 && samus is not null)
            samus.CeresRidleyEjection.Request(samus);

        var frame = CeresRidleyGetawayDefinitions.FromIndex(tableIndex);
        ushort zoom = checked((ushort)frame.Zoom);
        if (zoom == CeresRidleyGetawayDefinitions.Finished)
        {
            // $A6:AB2E restores fake BGMODE=$09 and clears all Mode 7/scroll registers.
            // Preserve that clean handoff explicitly so the renderer cannot accidentally
            // keep sampling Mode 7 VRAM for the self-destruct text scene.
            state.Mode7Finished = true;
            state.Mode7Active = false;
            state.Mode7Offset = Offset16<Mode7Pixels>.Zero;
            state.Mode7MatrixA = default;
            state.Mode7MatrixB = default;
            state.Mode7MatrixC = default;
            state.Mode7MatrixD = default;
            state.Mode7Center = Position16<Mode7Pixels>.Zero;
            // $A6:AB55 hands the same extended actor workspace to $A6:C04E. Samus's
            // special push/fall handler remains responsible for restoring normal input;
            // ending Mode 7 itself does not unlock her in the cartridge.
            state.Function = RidleyAiFunction.CeresActivateSelfDestruct;
            state.FunctionTimer = NativeCountdown.Start(0);
            return;
        }

        state.Mode7Zoom = zoom;
        // The getaway frame's velocity is added to the vertical word and subtracted from the
        // horizontal word.
        state.Mode7Offset = new Offset16<Mode7Pixels>(
            unchecked((short)(state.Mode7Offset.X - frame.Velocity.X)),
            unchecked((short)(state.Mode7Offset.Y + frame.Velocity.Y)));

        UpdateCeresRidleyMode7Palette(zoom);
        state.Mode7Angle = state.Mode7Angle.AddRaw(48);
        UpdateCeresRidleyMode7Matrix(state);

        // Both animations queue into this frame's Mode 7 transfer queue, the capsule first, so
        // the wing descriptors start after the capsule's. Their bytes outlive the transfer.
        var queue = new Mode7TransferQueue(_room.Mode7QueueResidue ?? throw new InvalidOperationException(
            "The Ceres Ridley getaway requires the session's Mode 7 transfer queue residue."));

        // $A6:ACBC advances the Baby capsule on every fourth NMI using 0,1,2,1. The transfer
        // lists patch only the two two-byte rows occupied by that animation in the map.
        if ((nmiFrameCounter & 3) == 0)
        {
            state.Mode7BabyFrame = (state.Mode7BabyFrame + 1) % 4;
            ApplyMode7TransferList(CeresMode7TransferDefinitions.BabyFrameForPhase(state.Mode7BabyFrame), ref queue);
        }

        // $A6:AD27 alternates the six sparse wing rows on every eighth NMI.
        if ((nmiFrameCounter & 7) == 0)
        {
            state.Mode7WingFrame = (state.Mode7WingFrame + 1) % 2;
            ApplyMode7TransferList(state.Mode7WingFrame == 0
                ? CeresMode7TransferList.WingFrame0
                : CeresMode7TransferList.WingFrame1, ref queue);
        }
        queue.CompleteTransfers();
    }

    /// <summary>
    /// Negates a signed 8.8 coefficient the way the getaway's two's-complement negate does: the most negative value,
    /// which has no positive counterpart, negates to itself.
    /// </summary>
    internal static SnesSignedEightEight NegateMode7Coefficient(SnesSignedEightEight coefficient) =>
        SnesSignedEightEight.FromSignedRaw(unchecked((short)-coefficient.SignedRawValue));

    /// <summary>Updates the getaway's Mode-7 transform matrix from its zoom and rotation words.</summary>
    private static void UpdateCeresRidleyMode7Matrix(RidleyEnemyState state)
    {
        byte angle = state.Mode7Angle.TableIndex;
        ushort zoom = checked((ushort)state.Mode7Zoom);
        var diagonal = new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(zoom, unchecked((byte)(angle + 64))));
        var offDiagonal = new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(zoom, angle));
        state.Mode7MatrixA = diagonal;
        state.Mode7MatrixB = offDiagonal;
        state.Mode7MatrixC = NegateMode7Coefficient(offDiagonal);
        state.Mode7MatrixD = diagonal;
    }

    /// <summary>Selects the zoom-dependent fifteen-color Ceres getaway palette without replacing transparent color zero.</summary>
    internal void UpdateCeresRidleyMode7Palette(ushort zoom)
    {
        // SetCeresRidleyPaletteAccordingToZoomLevel uses this wrapped bank-$A6 expression.
        // There are fifteen colors; palette entry zero remains the shared transparent color.
        int zoomHighByte = SnesLittleEndian.HighByte(zoom);
        if (_presentation.CeresRidleyMode7Colors is { } installed &&
            zoomHighByte < CeresRidleyPaletteRomData.Mode7ZoomRowCount)
        {
            installed.Apply(_presentation.Cgram, zoomHighByte);
            return;
        }
        throw new InvalidOperationException(
            $"Ceres Ridley Mode 7 zoom row {zoomHighByte} is not in the installed palette catalog.");
    }

    /// <summary>
    /// Replays one compiled $80:8B4F Mode-7 transfer list synchronously and queues its native
    /// descriptors. Its source bytes are immutable cartridge definitions, so gameplay needs no
    /// bank-$A6 read.
    /// </summary>
    private void ApplyMode7TransferList(CeresMode7TransferList pointer, ref Mode7TransferQueue queue)
    {
        CeresMode7TransferDefinitions.ApplyTo(_presentation.Vram, pointer);
        foreach (CeresMode7Transfer transfer in CeresMode7TransferDefinitions.Get(pointer))
        {
            queue.QueueVramTilemapTransfer(
                new SnesAddress(Mode7TransferQueueLayout.CeresSourceBank, checked((ushort)transfer.SourceOffset)),
                (ushort)transfer.TileNumbers.Count, checked((ushort)transfer.DestinationWord));
        }
    }

    /// <summary>
    /// Ports the shared Ridley function at $A6:C04E for the Ceres branch. The phase word
    /// is the actor's native variable F: even values select the exact C062/C08E/C09F/C0BB/
    /// C0F5/C104/C117 entries. English skips only the language-specific phase eight and
    /// enters phase ten to type the cartridge's three-line English warning. Japanese enters
    /// phase eight first, installs its subtitle tilemap, and then uses the same typewriter.
    /// </summary>
    private void TickCeresRidleySelfDestruct(
        RidleyEnemyState state,
        VramWriteQueue? vramWriteQueue)
    {
        switch (state.FunctionTimer.RawValue)
        {
            case 0:
                // $A6:C062 copies four already-live colors before the warning graphics
                // replace their tiles. These are palette indices, not byte offsets.
                CopyCeresWarningColors(_presentation.CgramRows.BackgroundRow(0), _presentation.CgramRows.BackgroundRow(6));
                CopyCeresWarningColors(_presentation.CgramRows.BackgroundRow(1), _presentation.CgramRows.BackgroundRow(5));
                state.CeresEscapeTransfers = CeresEscapeTransferCursor.TimerSprites;
                state.FunctionTimer = NativeCountdown.Start(2);
                goto case 2;

            case 2:
                if (QueueNextCeresEscapeTransfer(state, vramWriteQueue))
                {
                    state.CeresEscapeTransfers = CeresEscapeTransferCursor.TimerBackgrounds;
                    state.FunctionTimer = NativeCountdown.Start(4);
                    goto case 4;
                }
                return;

            case 4:
                if (QueueNextCeresEscapeTransfer(state, vramWriteQueue))
                {
                    // `$A6:C0AA` calls DrawEmergencyText immediately after the final
                    // BG1/BG2 graphics record. The word was missing from the earlier
                    // translation, so the newly uploaded letter tiles existed but no
                    // tilemap entries ever referenced them on screen.
                    QueueCeresEmergencyText(vramWriteQueue);
                    // $A6:C09F also queues music track seven after installing the final
                    // static warning tilemap page. The frontend consumes this typed request
                    // through the same bank-$80 queue used by every other music producer.
                    state.MusicRequest = MusicCommand.SelectTrack(7);
                    state.FunctionTimer = NativeCountdown.Start(6);
                    state.CeresEscapeTextDelayTimer = NativeCountdown.Start(128);
                }
                return;

            case 6:
                UpdateCeresSelfDestructPalette(state, _slotTable._slots[0].FrameCounter);
                NativeWordCounterStep delay = state.CeresEscapeTextDelayTimer.Decrement();
                state.CeresEscapeTextDelayTimer = delay.Remaining;
                if (!delay.IsZero)
                    return;

                state.CeresEscapeTypewriter = _presentation.EscapeTypewriterPresentation is { } presentation
                    ? new(presentation.Get(EscapeTypewriterProgramId.Ceres), EscapeTypewriterDefinitions.CeresTileBase)
                    : null;

                // `$A6:C0E3-$A6:C0F1` advances the even-byte dispatch index twice for
                // English before executing the common final increment: 6 -> 8 -> 10.
                // Japanese skips the first increment and reaches phase 8. The previous
                // translation incorrectly counted the two INC instructions as four phase
                // entries and jumped English directly to phase 12, suppressing every
                // character below "EMERGENCY".
                state.FunctionTimer = NativeCountdown.FromRaw(_roomStatus.JapaneseText ? (ushort)8 : (ushort)10);
                return;

            case 8:
                UpdateCeresSelfDestructPalette(state, _slotTable._slots[0].FrameCounter);
                NativeWordCounterStep japaneseDelay = state.CeresEscapeTextDelayTimer.Decrement();
                state.CeresEscapeTextDelayTimer = japaneseDelay.Remaining;
                if (japaneseDelay.IsZero)
                {
                    state.FunctionTimer = NativeCountdown.Start(10);
                    QueueCeresTransferList(CeresEscapeVramTransferDefinitions.JapaneseOverlay,
                        vramWriteQueue);
                }
                if (StepCeresEscapeTypewriter(state))
                    state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 2)));
                return;

            case 10:
                UpdateCeresSelfDestructPalette(state, _slotTable._slots[0].FrameCounter);
                if (StepCeresEscapeTypewriter(state))
                    state.FunctionTimer = NativeCountdown.Start(12);
                return;

            case 12:
                UpdateCeresSelfDestructPalette(state, _slotTable._slots[0].FrameCounter);
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                state.FunctionTimer = NativeCountdown.Start(0);
                state.Function = RidleyAiFunction.CeresSelfDestructPaletteOnly;
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
                // $A6:C131 sets the area-boss bit inside Ridley's AI, so later slots (the
                // room's exit door at $A6:F562) observe it in this same EnemyMain pass.
                _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
                _frameResults.CeresEscapeStarted = true;
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres self-destruct phase ${state.FunctionTimer.RawValue:X4} is not a native even dispatcher value.");
        }
    }

    /// <summary>$A6:C062: colours one and three of <paramref name="source"/> replace those of <paramref name="destination"/>.</summary>
    private static void CopyCeresWarningColors(CgramPalette source, CgramPalette destination)
    {
        destination.Write(1, source[1]);
        destination.Write(3, source[3]);
    }

    /// <summary>Ports DrawEmergencyText at $A6:C136 exactly.</summary>
    internal void QueueCeresEmergencyText([NotNull] VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Ceres emergency text requires a VRAM write queue.");
        CeresEscapeOverlayTilemapDefinition page =
            CeresEscapeOverlayTilemapDefinitions.Emergency;
        ushort byteCount = checked((ushort)(page.WordCount * sizeof(ushort)));
        if (_room.Artwork is { } installed &&
            installed.CeresEscapeOverlayTilemaps?.TryResolve(
                page.SourceAddress, byteCount, out _) != true)
            throw new InvalidDataException(
                "Installed Ceres escape has no editable EMERGENCY tilemap.");
        vramWriteQueue.Enqueue(
            byteCount,
            page.SourceAddress,
            EnemyVram.CeresEmergencyTextDma);
    }

    /// <summary>Ports ProcessSpriteTilesTransfers at $A6:C26E one record per actor call.</summary>
    [PpuLayoutBoundary("codec: Ceres escape VRAM transfer records' native destinations ($A6:C26E)")]
    internal bool QueueNextCeresEscapeTransfer(
        RidleyEnemyState state,
        VramWriteQueue? vramWriteQueue)
    {
        CeresEscapeTransferCursor cursor = state.CeresEscapeTransfers;
        ushort byteCount;
        int sourceAddress;
        ushort destination;
        if (!cursor.IsStarted)
            throw new InvalidDataException("The Ceres escape graphics dispatcher was never given a transfer list.");
        if (cursor.IsFinished)
            return true;
        CeresEscapeVramTransferDefinition transfer = cursor.Current;
        byteCount = transfer.ByteCount;
        sourceAddress = transfer.SourceAddress;
        destination = transfer.DestinationWord;
        if (vramWriteQueue is null)
        {
            throw new InvalidOperationException(
                "Ceres escape graphics require the runtime's native VRAM write queue.");
        }
        if (_room.Artwork is not null && _presentation.EscapeTimerArtwork is null &&
            EscapeTimerTilePages.IsSource(sourceAddress))
            throw new InvalidDataException(
                "Installed Ceres escape timer has no editable sprite tile artwork.");
        if (_room.Artwork is { } installed &&
            CeresEscapeTileArtworkDefinitions.Contains(sourceAddress, byteCount) &&
            installed.CeresEscapeTiles?.TryResolve(sourceAddress, byteCount, out _) != true)
            throw new InvalidDataException(
                $"Installed Ceres escape has no editable character page for ${sourceAddress:X6}.");

        if (_presentation.EscapeTimerArtwork?.TryQueueNativeTransfer(
            vramWriteQueue, sourceAddress, byteCount, destination) != true)
            vramWriteQueue.Enqueue(byteCount, sourceAddress, VramDmaDestination.FromNative(destination));

        state.CeresEscapeTransfers = cursor.Advance();
        return state.CeresEscapeTransfers.IsFinished;
    }

    /// <summary>Queues every record in the one-shot Japanese overlay list at $A6:C3B8.</summary>
    [PpuLayoutBoundary("codec: Ceres Japanese overlay transfer records' native destinations ($A6:C3B8)")]
    internal void QueueCeresTransferList(ushort pointer, VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Ceres Japanese text requires a VRAM write queue.");

        for (int record = 0; record < 32; record++, pointer = unchecked((ushort)(pointer + 7)))
        {
            if (CeresEscapeVramTransferDefinitions.IsTerminator(pointer))
                return;
            if (!CeresEscapeVramTransferDefinitions.TryGet(pointer, out var transfer))
                throw new InvalidDataException(
                    $"Ceres Japanese transfer ${pointer:X4} has no compiled record.");
            if (_room.Artwork?.CeresEscapeOverlayTilemaps?.TryResolve(
                    transfer.SourceAddress, transfer.ByteCount, out _) != true)
                throw new InvalidDataException(
                    $"Installed Ceres Japanese overlay ${pointer:X4} has no editable tilemap.");
            vramWriteQueue.Enqueue(transfer.ByteCount, transfer.SourceAddress,
                VramDmaDestination.FromNative(transfer.DestinationWord));
        }

        throw new InvalidDataException("Ceres Japanese transfer list exceeded 32 records.");
    }

    /// <summary>Ports the byte-oriented command stream consumed by $A6:C2A7.</summary>
    private bool StepCeresEscapeTypewriter(RidleyEnemyState state)
    {
        var installed = state.CeresEscapeTypewriter ?? throw new InvalidDataException(
            "Ceres warning text requires installed typewriter artwork.");
        bool completed = installed.Step(_presentation.Vram);
        state.CeresEscapeTextDelayTimer = installed.DelayTimer;
        if (installed.ClickRequested)
            _audio.QueueEnemySound(EscapeTypewriterRomData.CeresClick, maximumQueued: 3);
        return completed;
    }

    /// <summary>Ports the three-color alarm palette cycle at $A6:C19C.</summary>
    internal void UpdateCeresSelfDestructPalette(RidleyEnemyState state, int frameCounter)
    {
        if ((frameCounter & 3) != 0)
            return;

        state.CeresEscapePaletteFrame = (state.CeresEscapePaletteFrame + 1) % 16;
        if (_presentation.CeresRidleyColors is { } colors)
            colors.ApplyAlarm(_presentation.Cgram, state.CeresEscapePaletteFrame);
        else throw new InvalidOperationException(
            "Ceres alarm requires installed Ridley color rows.");
    }
}
