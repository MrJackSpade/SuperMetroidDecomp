using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Lower Norfair Ridley ($E17F). The Ceres encounter and the real boss intentionally share
/// initialization, composition, movement, and instruction code in bank $A6; this file owns
/// only the area-two branches and combat dispatcher beginning at $A6:B227.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Enemy header $A0:E17F, selecting the Lower Norfair boss branches of Ridley's shared bank-$A6 implementation.</summary>
    public const ushort NorfairRidleyDefinition = 0xe17f;

    /// <summary>Identifies the two enemy headers that enter the shared Ridley implementation.</summary>
    /// <param name="definitionPointer">The enemy definition pointer read from a room slot.</param>
    /// <returns><see langword="true"/> for the Ceres or Lower Norfair Ridley header.</returns>
    private static bool IsRidleyDefinition(ushort definitionPointer) =>
        definitionPointer is CeresRidleyDefinition or NorfairRidleyDefinition;

    /// <summary>
    /// Ports the area-two branch of the shared initializer at $A6:A0F5. A defeated Ridley
    /// is deleted before any shared workspace or palette side effects, matching the native
    /// boss-bit gate; a live fight begins in layer five below the visible arena.
    /// </summary>
    private void InitializeNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Lower Norfair Ridley requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }

        if (RequireAreaBossDefeated())
        {
            slot.Properties = slot.Properties.With(
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);
            return;
        }

        slot.Parameter1 = 0;
        slot.Parameter2 = 0;
        SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.Initial);
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native property $1000 blocks Plasma Beam penetration. Its meaning is established
        // only for this boss, so preserve the raw bit instead of widening the shared enum.
        slot.Properties = slot.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.XPosition = 96;
        slot.YPosition = 394;
        slot.Layer = 5;

        _ridleyState = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = 0,
            SpritemapPaletteIndex = EnemyPaletteBits.Palette7,
            CommonDrawPaletteIndex = EnemyPaletteBits.Palette7,
            MovementAnimationEnabled = 1,
            FacingDirection = 2,
            IdleTailWhipEnabled = 0,
            TailDamage = 120,
            MinimumY = 64,
            MaximumY = 416,
            MinimumX = 64,
            MaximumX = 224,
            WingFrame = 0,
            WingAnimationTimer = 0,
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = 0,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            TailWhipTargetClockwiseAngle = 0xffff,
            TailWhipTargetCounterClockwiseAngle = 0xffff,
            TailExtensionSpeed = 0x00f0,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentAngle,
            TailSegments = CreateInitialRidleyTailSegments(),
        };

        // WriteColorsToTargetPalette($140, $A6:E1CF, $20) installs both body and manual
        // tail/wing source palettes. The following loops clear the two fifteen-color OBJ
        // ranges that the reveal gradually fills; color zero remains transparent.
        (TileArtwork?.NorfairRidleyColors ?? throw new InvalidOperationException(
            "Norfair Ridley requires installed colors."))
            .ApplyInitial(_cgram!);
        for (int color = 113; color <= 127; color++)
            _cgram!.SetColor(color, 0);
        for (int color = 241; color <= 255; color++)
            _cgram!.SetColor(color, 0);
    }

    /// <summary>
    /// Ports <c>Ridley_Main</c> at $A6:B227 and the area-two half of the shared reveal
    /// dispatcher. Composition is advanced after the selected function, exactly like the
    /// cartridge, so a velocity change affects the same rendered frame.
    /// </summary>
    private void RunNorfairRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort controllerInput,
        RoomLevelData? level = null,
        SamusProjectileSystem? samusProjectiles = null,
        SamusBombProjectileSystem? sharedProjectiles = null,
        ushort cameraX = 0,
        ushort cameraY = 0)
    {
        RidleyEnemyState state = RequireNorfairRidley(slot);
        state.HurtMovementClamp = unchecked((ushort)Math.Max(
            0,
            unchecked((short)state.HurtMovementClamp) - 4));

        UpdateNorfairRidleyIntangibility(slot, state, cameraX, cameraY);
        PrepareNorfairRidleyCombatFrame(slot, state, sharedProjectiles ?? throw new InvalidOperationException(
            "Ridley's power-bomb check ($A6:BD2C) requires the shared projectile owner."));

        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);

        if (state.MovementAnimationEnabled != 0)
        {
            UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);
            IntegrateRidleyMovement(slot, state);
            TickRidleyWingAnimation(state);
            TickRidleyTail(slot, state, samus);
            if (samusProjectiles is not null)
                ResolveRidleyTailProjectileHits(slot, state, samusProjectiles);
            // Native Main places carried Samus after body/tail movement.
            if (state.GrabState != 0 && samus is not null)
                UpdateNorfairRidleyGrabbedSamus(slot, state, samus);
        }

        UpdateNorfairRidleyHealthPalette(slot, state);
    }

    /// <summary>Executes one Lower Norfair Ridley AI function, including any same-frame transition setup.</summary>
    /// <param name="slot">The room enemy slot whose position, health, and animation are being updated.</param>
    /// <param name="state">Ridley's persistent encounter state.</param>
    /// <param name="samus">Samus state used by attacks and grab decisions, when present.</param>
    /// <param name="controllerInput">The controller word available to the encounter this frame.</param>
    /// <param name="level">Room collision data used by terrain-sensitive attacks, when available.</param>
    private void RunNorfairRidleyFunction(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        ushort controllerInput,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                if (EnemyDoorTransitionActive)
                    return;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = 170;
                TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.InitialDelay:
                TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.FadeInEyes:
                TickCeresRidleyEyeFade(state);
                return;

            case RidleyAiFunction.FadeInBody:
                TickCeresRidleyBodyFade(slot, state);
                if (state.Function == RidleyAiFunction.WaitBeforeRoar)
                    slot.Layer = 2;
                return;

            case RidleyAiFunction.WaitBeforeRoar:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if ((short)state.FunctionTimer < 0)
                {
                    SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.OpeningRoar);
                    state.FunctionTimer = 0;
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                return;

            case RidleyAiFunction.WaitBeforeLiftoff:
                TickNorfairRidleyArenaReveal(slot, state);
                return;

            case RidleyAiFunction.ClearVelocity:
                state.HorizontalVelocity = 0;
                state.VerticalVelocity = 0;
                return;

            case RidleyAiFunction.NorfairEnterArena:
                MoveNorfairRidleyToward(slot, state, 64, 256, divisorIndex: 14);
                if (IsWithinRidleyRectangle(slot, 64, 256, 8, 8))
                {
                    state.FightMode = 1;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                    SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                }
                return;

            case RidleyAiFunction.NorfairSelectAttack:
                SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                return;

            case RidleyAiFunction.NorfairHoverSetup:
                state.Function = RidleyAiFunction.NorfairHover;
                state.FunctionTimer = 128;
                TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairHover:
                TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopSetup:
                state.Function = RidleyAiFunction.NorfairSwoopMoveToStart;
                state.SwoopPhaseTimer = 10;
                state.SwoopAngleAccumulator = 0;
                TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopMoveToStart:
                TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopAimDown:
                TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != 0 ? 32 : -32,
                    state.FacingDirection != 0 ? 512 : -512,
                    1152,
                    RidleyAiFunction.NorfairSwoopAimSideways,
                    20);
                return;

            case RidleyAiFunction.NorfairSwoopAimSideways:
                if (TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != 0 ? 320 : -320,
                        state.FacingDirection != 0 ? 0x4000 : -0x4000,
                        1280,
                        RidleyAiFunction.NorfairSwoopAimUp,
                        16))
                {
                    state.TailWhipRequest = 1;
                }
                return;

            case RidleyAiFunction.NorfairSwoopAimUp:
                TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != 0 ? 512 : -512,
                    state.FacingDirection != 0 ? 0x7800 : -0x7800,
                    768,
                    RidleyAiFunction.NorfairSwoopClimb,
                    32);
                return;

            case RidleyAiFunction.NorfairSwoopClimb:
                if (TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != 0 ? 1024 : -1024,
                        state.FacingDirection != 0 ? 0x7800 : -0x7800,
                        768,
                        RidleyAiFunction.NorfairSwoopRecover,
                        32))
                {
                    SelectNorfairRidleyFacingInstruction(slot, state);
                }
                return;

            case RidleyAiFunction.NorfairSwoopRecover:
                UpdateRidleySwoopVelocity(state, 0, short.MinValue, 448);
                if (state.SwoopPhaseTimer != 0)
                {
                    state.SwoopPhaseTimer--;
                    return;
                }
                state.Function = SamusMovementUsesRidleyGrab(samus)
                    ? RidleyAiFunction.NorfairGrabApproach
                    : RidleyAiFunction.NorfairSelectAttack;
                return;

            case RidleyAiFunction.NorfairPogoSetup:
                state.IdealInterSegmentTailAngle = 11;
                state.TailExtensionSpeed = 0x180;
                state.Function = RidleyAiFunction.NorfairPogoDescending;
                state.FunctionTimer = unchecked((ushort)((RequireRandomNumber() & 0x1f) + 32));
                TickNorfairRidleyPogo(slot, state, samus, descending: true);
                return;

            case RidleyAiFunction.NorfairPogoDescending:
                TickNorfairRidleyPogo(slot, state, samus, descending: true);
                return;

            case RidleyAiFunction.NorfairPogoAscending:
                TickNorfairRidleyPogo(slot, state, samus, descending: false);
                return;

            case RidleyAiFunction.NorfairFireballSetup:
                BeginNorfairRidleyGroundAttack(state);
                return;

            case RidleyAiFunction.NorfairFireballMoveToSide:
                TickNorfairRidleyGroundAttackMoveToSide(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairFireballMoveToHeight:
                TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairFireballAttack:
                TickNorfairRidleyGroundAttack(slot, state, samus, level);
                return;

            case RidleyAiFunction.NorfairFireballRecover:
                TickNorfairRidleyGroundAttackRecovery(state, samus);
                return;

            case RidleyAiFunction.NorfairGrabApproach:
                TickNorfairRidleyGrabApproach(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairReturnToArena:
                TickNorfairRidleyPowerBombDodge(slot, state);
                return;

            case RidleyAiFunction.NorfairCarrySetup:
                BeginNorfairRidleyCarry(slot, state);
                return;

            case RidleyAiFunction.NorfairCarryMoveToAnchor:
                MoveNorfairRidleyToward(slot, state, state.TargetX, state.TargetY, 0);
                if (TickRidleyFunctionTimer(state))
                {
                    state.Function = RidleyAiFunction.NorfairCarryRise;
                    state.FunctionTimer = 32;
                }
                return;

            case RidleyAiFunction.NorfairCarryRise:
                if (TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.CarryReleaseInterSegmentAngle;
                    state.TailExtensionSpeed = RidleyTailDefinitions.CarryReleaseExtensionSpeed;
                    ReleaseNorfairRidleyGrab(state, samus);
                    state.Function = RidleyAiFunction.NorfairCarryRelease;
                    state.FunctionTimer = 64;
                }
                else
                    MoveNorfairRidleyToward(slot, state, state.TargetX, 256, 0);
                return;

            case RidleyAiFunction.NorfairCarryRelease:
                if (TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentAngle;
                    state.TailExtensionSpeed = RidleyTailDefinitions.CarryReleaseExtensionSpeed;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                }
                else
                {
                    ushort releaseX = RidleyMovementTargets.CarryReleaseX(Math.Min(state.FacingDirection, (ushort)2));
                    MoveNorfairRidleyToward(slot, state, releaseX, 224, 0);
                }
                return;

            case RidleyAiFunction.NorfairReleaseSamus:
                TickNorfairRidleyMoveToDeathSpot(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathStart:
                BeginNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathExplosions:
                TickNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathFall:
                TickNorfairRidleyDeathExplosions(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairDeathImpact:
                BeginNorfairRidleyBreakup(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathWait:
                TickNorfairRidleyBreakupWait(state);
                return;

            case RidleyAiFunction.NorfairDeathFinish:
                TickNorfairRidleyDeathFinish(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathComplete:
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Ridley function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// $A6:A478 advances an area-two-only fourteen-color fade every third dispatcher call.
    /// A zero pointer terminates the table and activates the shared tail/wing composition.
    /// </summary>
    private void TickNorfairRidleyArenaReveal(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        if ((short)state.FunctionTimer >= 0)
            return;

        state.FunctionTimer = 2;
        ushort row = state.FadePaletteOffset;
        bool hasPalette;
        var colors = TileArtwork?.NorfairRidleyColors ?? throw new InvalidOperationException(
            "Norfair Ridley reveal requires installed colors.");
        if (row > NorfairRidleyPaletteRomData.RevealRowCount)
            throw new InvalidDataException($"Norfair Ridley reveal row {row} escaped its native terminator.");
        hasPalette = row < NorfairRidleyPaletteRomData.RevealRowCount;
        if (hasPalette)
            colors.ApplyReveal(_cgram!, row);
        state.FadePaletteOffset = unchecked((ushort)(state.FadePaletteOffset + 1));
        if (hasPalette)
            return;

        PublishRidleyLiquidMotion(state, RidleyLiquidRomData.BattleHeight,
            RidleyLiquidRomData.RiseVelocity, RidleyLiquidRomData.RiseDelay);
        state.FadePaletteOffset = 0;
        SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.TransitionToFlying);
        state.WingAnimationTimer = 8;
        state.WingAnimationTimerDelta = 8;
        foreach (RidleyTailSegment segment in state.TailSegments)
            segment.Active = true;
        state.TailFunctionIndex = 1;
        state.Function = RidleyAiFunction.ClearVelocity;
    }

    /// <summary>Chooses the next attack from Samus's movement, Ridley's health, and the encounter RNG byte, then starts it immediately.</summary>
    /// <param name="slot">The boss slot, whose current health and position inform the choice.</param>
    /// <param name="state">The AI state whose selected function is replaced.</param>
    /// <param name="samus">Samus movement and position used to classify the attack situation.</param>
    /// <param name="controllerInput">Input forwarded if the chosen function consumes it during its setup frame.</param>
    /// <param name="level">Room collision data forwarded to the selected function when it needs terrain.</param>
    private void SelectNorfairRidleyAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        ushort controllerInput,
        RoomLevelData? level)
    {
        RidleyAttackSituation situation;
        if (samus?.ReadMovementType(_bus!) == SamusMovementType.SpinJumping)
        {
            situation = RidleyAttackSituation.SpinJumping;
        }
        else if (slot.Health == 0)
        {
            situation = RidleyAttackSituation.ZeroHealth;
            state.ZeroHealthLungeCount = unchecked((ushort)(state.ZeroHealthLungeCount + 1));
        }
        else if (slot.Health < 14400)
        {
            situation = RidleyAttackSituation.BelowHalfHealth;
        }
        else if (samus is not null && samus.YPosition >= 352)
        {
            situation = RidleyAttackSituation.PogoZone;
        }
        else if (SamusMovementUsesRidleyGrab(samus))
        {
            situation = RidleyAttackSituation.DamageBoosting;
        }
        else
        {
            situation = slot.Health < 9000 ? RidleyAttackSituation.AboveHalfHealth : RidleyAttackSituation.BelowHalfHealth;
        }

        int choice = _nextRandom!() & 7;
        state.Function = RidleyAttackChoices.Resolve(situation, choice);

        // The native selector tail-calls the chosen routine. Retain that same-frame setup
        // so timers, instruction changes, and velocity all begin on the selected frame.
        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);
    }

    /// <summary>Moves Ridley toward the health-stage hover point and returns to attack selection on arrival or timeout.</summary>
    /// <param name="slot">The boss slot whose position is steered.</param>
    /// <param name="state">The hover timer, health stage, facing, and velocities.</param>
    private void TickNorfairRidleyHover(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (TickRidleyFunctionTimer(state))
        {
            state.Function = RidleyAiFunction.NorfairSelectAttack;
            return;
        }

        ushort targetX = state.FacingDirection != 0 ? (ushort)96 : (ushort)192;
        MoveNorfairRidleyToward(
            slot,
            state,
            targetX,
            256,
            ReadRidleyHealthMovementDivisorIndex(state));
        if (IsWithinRidleyRectangle(slot, targetX, 256, 8, 8))
            state.Function = RidleyAiFunction.NorfairSelectAttack;
    }

    /// <summary>Positions Ridley at the facing-dependent start of a swoop before beginning its downward arc.</summary>
    /// <param name="slot">The boss slot to move toward the swoop start point.</param>
    /// <param name="state">The facing, velocity, and AI phase state updated by the transition.</param>
    private static void TickNorfairRidleySwoopMoveToStart(RoomEnemySlot slot, RidleyEnemyState state)
    {
        ushort targetX = state.FacingDirection != 0 ? (ushort)64 : (ushort)192;
        MoveNorfairRidleyToward(slot, state, targetX, 128, divisorIndex: 1);
        if (!IsWithinRidleyRectangle(slot, targetX, 128, 8, 8))
            return;

        state.Function = RidleyAiFunction.NorfairSwoopAimDown;
        state.SwoopPhaseTimer = 32;
        state.SwoopAngleAccumulator = 0;
    }

    /// <summary>Advances a timed swoop arc and installs its next phase when the current phase timer expires.</summary>
    /// <param name="state">The accumulated angle, velocity, timer, and current AI function.</param>
    /// <param name="angleDelta">The signed angular adjustment applied this update.</param>
    /// <param name="targetAngle">The target angle passed to the swoop velocity calculation.</param>
    /// <param name="targetMagnitude">The velocity magnitude approached during the phase.</param>
    /// <param name="nextFunction">The AI phase to select when the timer has elapsed.</param>
    /// <param name="nextTimer">The duration assigned to that next phase.</param>
    /// <returns><see langword="true"/> when the phase changed; otherwise <see langword="false"/>.</returns>
    private static bool TickNorfairRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        ushort nextTimer)
    {
        UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        if (state.SwoopPhaseTimer != 0)
        {
            state.SwoopPhaseTimer--;
            return false;
        }

        state.Function = nextFunction;
        state.SwoopPhaseTimer = nextTimer;
        return true;
    }

    /// <summary>
    /// Ports $B5E5/$B613. These two states alternate side targets while Samus is spin
    /// jumping; any other movement type immediately advances into the low arena attack.
    /// </summary>
    private void TickNorfairRidleyPogo(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        bool descending)
    {
        int facing = Math.Min(state.FacingDirection, (ushort)2);
        ushort targetX = descending ? RidleyMovementTargets.DescendingPogoX(facing)
            : RidleyMovementTargets.AscendingPogoX(facing);
        ushort targetY = samus is null ? (ushort)352 : Math.Min(samus.YPosition, (ushort)352);
        MoveNorfairRidleyToward(
            slot,
            state,
            targetX,
            targetY,
            ReadRidleyHealthMovementDivisorIndex(state));
        state.TailWhipRequest = 1;

        if (samus?.ReadMovementType(_bus!) != SamusMovementType.SpinJumping)
        {
            BeginNorfairRidleyGroundAttack(state);
            return;
        }

        // $A6:B669 uses the existing RNG byte; it does not advance the generator.
        if ((RequireRandomNumber() & 0xff) >= 0x80 && !state.Roaring && state.FacingDirection != 1)
            SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.Fireballing);

        if (!TickRidleyFunctionTimer(state))
            return;

        state.Function = descending
            ? RidleyAiFunction.NorfairPogoAscending
            : RidleyAiFunction.NorfairPogoDescending;
        state.FunctionTimer = 128;
        SelectNorfairRidleyFacingInstruction(slot, state);
    }

    /// <summary>Configures the tail and switches the AI into its ground-attack positioning sequence.</summary>
    /// <param name="state">The encounter state whose tail mode and function are initialized.</param>
    private static void BeginNorfairRidleyGroundAttack(RidleyEnemyState state)
    {
        state.TailExtensionSpeed = 0xf0;
        state.IdealInterSegmentTailAngle = 16;
        state.TailFunctionIndex = RidleyTailDefinitions.Neutral;
        state.Function = RidleyAiFunction.NorfairFireballMoveToSide;
    }

    /// <summary>Moves Ridley to the facing-dependent horizontal launch point, then begins height alignment.</summary>
    /// <param name="slot">The boss slot being positioned.</param>
    /// <param name="state">The function, timer, and movement state for this attack.</param>
    /// <param name="samus">Samus state passed to the height-alignment tail update.</param>
    private void TickNorfairRidleyGroundAttackMoveToSide(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (unchecked((short)(slot.YPosition - 288)) < 0)
        {
            SelectNorfairRidleyFacingInstruction(slot, state);
            state.Function = RidleyAiFunction.NorfairFireballMoveToHeight;
            state.FunctionTimer = 32;
            TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
            return;
        }

        ushort targetX = RidleyMovementTargets.GroundAttackX(Math.Min(state.FacingDirection, (ushort)2));
        MoveNorfairRidleyToward(slot, state, targetX, 288, divisorIndex: 0);
    }

    /// <summary>Holds the selected horizontal point while rising or falling to the attack height, then starts the pogo strike.</summary>
    /// <param name="slot">The boss slot whose vertical position is adjusted.</param>
    /// <param name="state">The timer, tail, and movement state for the attack setup.</param>
    /// <param name="samus">Samus state used while updating the tail during setup.</param>
    private void TickNorfairRidleyGroundAttackMoveToHeight(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        MoveNorfairRidleyToward(slot, state, slot.XPosition, 288, divisorIndex: 0);
        if (!TickRidleyFunctionTimer(state))
            return;

        state.TailFunctionIndex = RidleyTailDefinitions.PogoSetup;
        TickRidleyPogoTail(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.FunctionTimer = unchecked((ushort)((RequireRandomNumber() & 0x3f) + 128));
    }

    /// <summary>Runs the descending tail strike, handling a claw grab first and bouncing when the tail contacts terrain.</summary>
    /// <param name="slot">The boss slot used for claw geometry and impact effects.</param>
    /// <param name="state">Ridley's movement, tail, and attack state.</param>
    /// <param name="samus">Samus state used for grab detection and bounce direction.</param>
    /// <param name="level">Collision map sampled beneath the tail tip and joints.</param>
    private void TickNorfairRidleyGroundAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is not null && SamusMovementUsesRidleyGrab(samus) &&
            RidleyClawOverlapsSamus(slot, state, samus, radiusX: 4, radiusY: 4))
        {
            state.VerticalVelocity = unchecked((ushort)-Math.Max(
                512,
                Math.Abs((int)unchecked((short)state.VerticalVelocity))));
            state.TailFunctionIndex = RidleyTailDefinitions.Neutral;
            state.TailAngleDelta = 1;
            BeginNorfairRidleyGrab(slot, state, samus);
            return;
        }

        int nextVertical = unchecked((short)state.VerticalVelocity) +
            unchecked((short)state.PogoDownwardAcceleration);
        state.VerticalVelocity = unchecked((ushort)Math.Min(nextVertical, 1536));

        if (!RidleyTailTouchesTerrain(state, level))
            return;

        RidleyTailSegment tip = state.TailSegments[6];
        SpawnRidleyDust(tip.XPosition, unchecked((ushort)(tip.YPosition + 12)), variant: 9);
        QueueEnemySound(SoundEffectLibrary2Sounds.RidleyTailTerrainImpact, maximumQueued: 6);
        EarthquakeType = 13;
        EarthquakeTimer = 4;
        SetRidleyPogoHorizontalDirection(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        for (int index = 0; index < state.TailSegments.Length; index++)
        {
            state.TailSegments[index].Distance = RidleyTailDefinitions.RestDistance(index);
            state.TailSegments[index].TargetDistance = RidleyTailDefinitions.BounceDistance;
        }
        state.TailFunctionIndex = RidleyTailDefinitions.Pogo;
        state.PogoBounceCount = unchecked((ushort)(state.PogoBounceCount + 1));
        if (state.PogoBounceCount >= 2)
        {
            state.PogoBounceCount = 0;
            if (state.FacingDirection != 1)
                SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.Fireballing);
        }
        state.Function = RidleyAiFunction.NorfairFireballRecover;
    }

    /// <summary>Ends the bounce recovery when its timer expires or Samus leaves the low arena, otherwise lifts Ridley back toward the strike.</summary>
    /// <param name="state">The recovery timer, vertical velocity, and tail mode.</param>
    /// <param name="samus">Samus position determining whether the low-arena pogo continues.</param>
    private static void TickNorfairRidleyGroundAttackRecovery(
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || samus.YPosition < 352 || TickRidleyFunctionTimer(state))
        {
            state.TailFunctionIndex = RidleyTailDefinitions.Neutral;
            state.TailAngleDelta = 1;
            state.Function = RidleyAiFunction.NorfairSelectAttack;
            return;
        }

        int nextVertical = unchecked((short)state.VerticalVelocity) +
            unchecked((short)state.PogoUpwardAcceleration);
        if (nextVertical >= 0)
        {
            state.VerticalVelocity = 0;
            state.Function = RidleyAiFunction.NorfairFireballAttack;
        }
        else
        {
            state.VerticalVelocity = unchecked((ushort)nextVertical);
        }
    }

    /// <summary>
    /// Ports the four ROM-selected velocity tables consumed by $A6:B90F. The signedness
    /// of the previous horizontal velocity chooses direction; magnitudes and both vertical
    /// accelerations are compiled cartridge definitions indexed by Ridley's health stage.
    /// </summary>
    private void InitializeNorfairRidleyPogoVelocity(RidleyEnemyState state)
    {
        // The native routine samples the current word; it does not advance shared RNG.
        int randomIndex = RequireRandomNumber() & 3;
        var definition = RidleyPogoDefinitions.Read(randomIndex, Math.Min(state.HealthStage, (ushort)3) + 2);
        state.PogoUpwardAcceleration = definition.UpwardAcceleration;
        state.PogoDownwardAcceleration = definition.DownwardAcceleration;
        state.VerticalVelocity = definition.Y;
        ushort horizontalMagnitude = definition.X;
        state.HorizontalVelocity = unchecked((short)state.HorizontalVelocity < 0)
            ? unchecked((ushort)-(short)horizontalMagnitude)
            : horizontalMagnitude;
    }

    /// <summary>Checks the tail tip and lower joints against nonempty room collision blocks using the encounter's pixel probes.</summary>
    /// <param name="state">The tail segments whose coordinates are sampled.</param>
    /// <param name="level">The room map; absence of collision data means no terrain contact is reported.</param>
    /// <returns><see langword="true"/> if any in-bounds probe lands in a collidable block.</returns>
    private static bool RidleyTailTouchesTerrain(
        RidleyEnemyState state,
        RoomLevelData? level)
    {
        if (level is null || state.TailSegments.Length != 7)
            return false;

        // $B7E7 samples the tip and the next four joints with the exact +16/+18 Y probes.
        int tipIndex = state.TailSegments.Length - 1;
        for (int index = tipIndex; index >= 2; index--)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            ushort x = segment.XPosition;
            ushort y = unchecked((ushort)(segment.YPosition + (index == tipIndex ? 16 : 18)));
            if ((x >> 4) >= level.WidthInBlocks || (y >> 4) >= level.HeightInBlocks)
                continue;
            if (level.GetCollisionBlockAtPixel(x, y).CollisionType != 0)
                return true;
        }
        return false;
    }

    /// <summary>Steers a grab lunge toward Samus and converts a valid claw overlap into a grab; missed or passed targets end the lunge.</summary>
    /// <param name="slot">The boss slot used for relative-position and claw calculations.</param>
    /// <param name="state">The facing, movement, and function state of the lunge.</param>
    /// <param name="samus">The target; a missing or ineligible target makes the lunge miss.</param>
    private void TickNorfairRidleyGrabApproach(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !SamusMovementUsesRidleyGrab(samus))
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        // The native lunge is a passing attack, not indefinite pursuit. Test the
        // previous movement's boundary result before applying another acceleration.
        short deltaX = unchecked((short)(slot.XPosition - samus.XPosition));
        bool passedSamus = state.FacingDirection == 2 ? deltaX >= 0 : deltaX < 0;
        if (state.HitRoomBoundary ||
            (passedSamus && unchecked((short)(Math.Abs((int)deltaX) - RidleyLungeDefinitions.PassDistance)) >= 0) ||
            unchecked((short)(slot.YPosition + RidleyLungeDefinitions.ClawHeight - samus.YPosition)) >= 0)
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        int sideOffset = state.FacingDirection != 0 ? 16 : -16;
        ushort targetY = unchecked((ushort)(samus.YPosition - 4));
        int divisorIndex = RidleyMovementTargets.GrabDivisorIndexes(Math.Min(state.HealthStage, (ushort)3));
        MoveNorfairRidleyToward(
            slot,
            state,
            unchecked((ushort)(samus.XPosition + sideOffset)),
            targetY,
            divisorIndex);

        if (RidleyClawOverlapsSamus(slot, state, samus, radiusX: 8, radiusY: 12))
        {
            // $A6:BB56-$BB5D reverses the lunge before the carry setup accelerates.
            state.VerticalVelocity = unchecked((ushort)-state.VerticalVelocity);
            BeginNorfairRidleyGrab(slot, state, samus);
        }
    }

    /// <summary>Attaches Samus to Ridley's claw, applies the control and collision locks, and enters carry or the zero-health death route.</summary>
    /// <param name="slot">The boss slot receiving the Samus-collision property update.</param>
    /// <param name="state">Ridley's grab and encounter state.</param>
    /// <param name="samus">The Samus instance to lock and position relative to the claw.</param>
    private static void BeginNorfairRidleyGrab(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        ushort clawX = GetNorfairRidleyClawX(slot, state);
        ushort clawY = GetNorfairRidleyClawY(slot, state);
        state.GrabXOffset = unchecked((ushort)(samus.XPosition - clawX));
        state.GrabYOffset = unchecked((ushort)(samus.YPosition - clawY));
        state.GrabState = 1;
        samus.SetStationaryScriptControlLock(true);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (slot.Health == 0)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            // $A6:BB8C tail-jumps to $C538 on the same successful zero-health grab.
            TickNorfairRidleyMoveToDeathSpot(slot, state);
            return;
        }
        // $BB8F falls through $BBC4 on the grabbing update.
        BeginNorfairRidleyCarry(slot, state);
    }

    /// <summary>Sets a facing-dependent carry anchor and begins steering Ridley toward it.</summary>
    /// <param name="slot">The boss slot whose current position establishes the target height.</param>
    /// <param name="state">The carry target, timer, function, and initial movement values.</param>
    private static void BeginNorfairRidleyCarry(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.TargetX = RidleyMovementTargets.CarryAnchorX(Math.Min(state.FacingDirection, (ushort)2));
        state.TargetY = unchecked((short)(slot.YPosition - 320)) < 0
            ? (ushort)256
            : unchecked((ushort)(slot.YPosition - 64));
        state.Function = RidleyAiFunction.NorfairCarryMoveToAnchor;
        state.FunctionTimer = 32;
        MoveNorfairRidleyToward(slot, state, state.TargetX, state.TargetY, 0);
        TickRidleyFunctionTimer(state);
    }

    /// <summary>Detaches Samus and restores the movement-specific release invulnerability while the fight remains active.</summary>
    /// <param name="state">The grab state and timer fields updated for release.</param>
    /// <param name="samus">The player whose stationary-script lock is removed, when present.</param>
    private void ReleaseNorfairRidleyGrab(RidleyEnemyState state, SamusState? samus)
    {
        state.GrabState = 0;
        samus?.SetStationaryScriptControlLock(false);
        state.TailWhipRequest = 1;
        state.TailFunctionIndex = 1;
        // $A6:BC8F-BC93 leaves the timer untouched once death owns the fight.
        if (unchecked((short)state.FightMode) >= 0)
        {
            SamusMovementType movement = samus?.ReadMovementType(_bus!) ?? SamusMovementType.Standing;
            state.IntangibilityTimer = RidleySamusInteractionDefinitions.ReleaseIntangibilityFrames(movement);
        }
    }

    /// <summary>Places carried Samus at the claw, easing the initial capture offset toward zero on each update.</summary>
    /// <param name="slot">The boss slot anchoring the claw position.</param>
    /// <param name="state">Ridley's grab offsets and facing-related geometry.</param>
    /// <param name="samus">The captured player whose world coordinates are updated.</param>
    private static void UpdateNorfairRidleyGrabbedSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        state.GrabXOffset = DecayRidleyGrabOffset(state.GrabXOffset);
        state.GrabYOffset = DecayRidleyGrabOffset(state.GrabYOffset);
        samus.XPosition = unchecked((ushort)(
            GetNorfairRidleyClawX(slot, state) + unchecked((short)state.GrabXOffset)));
        samus.YPosition = unchecked((ushort)(
            GetNorfairRidleyClawY(slot, state) + unchecked((short)state.GrabYOffset)));
    }

    /// <summary>Moves a signed claw-to-player offset four pixels toward zero without crossing it.</summary>
    /// <param name="offsetWord">The two's-complement offset stored in an unsigned 16-bit word.</param>
    /// <returns>The signed offset after one frame of easing, represented in the original word format.</returns>
    private static ushort DecayRidleyGrabOffset(ushort offsetWord)
    {
        int offset = unchecked((short)offsetWord);
        if (offset > 0)
            offset = Math.Max(0, offset - 4);
        else if (offset < 0)
            offset = Math.Min(0, offset + 4);
        return unchecked((ushort)offset);
    }

    /// <summary>Tests the claw point against Samus's collision radii expanded by the supplied horizontal and vertical margins.</summary>
    /// <param name="slot">The boss slot anchoring the claw.</param>
    /// <param name="state">The facing and body geometry used to locate the claw.</param>
    /// <param name="samus">The player collision box to test.</param>
    /// <param name="radiusX">Additional horizontal reach beyond Samus's X radius.</param>
    /// <param name="radiusY">Additional vertical reach beyond Samus's Y radius.</param>
    /// <returns><see langword="true"/> when both axis distances are strictly inside their expanded radii.</returns>
    private static bool RidleyClawOverlapsSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus,
        ushort radiusX,
        ushort radiusY) =>
        Math.Abs(unchecked((short)(samus.XPosition - GetNorfairRidleyClawX(slot, state)))) <
            samus.Kinematics.XRadius + radiusX &&
        Math.Abs(unchecked((short)(samus.YPosition - GetNorfairRidleyClawY(slot, state)))) <
            samus.Kinematics.YRadius + radiusY;

    /// <summary>Returns the claw's world X coordinate using the facing-specific offset.</summary>
    /// <param name="slot">The boss body position.</param>
    /// <param name="state">The facing direction selecting the claw offset.</param>
    /// <returns>The claw X position in room pixels, wrapped to the native 16-bit coordinate word.</returns>
    private static ushort GetNorfairRidleyClawX(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.XPosition + RidleyClawOffsets.ReadX(state.FacingDirection)));

    /// <summary>Returns the claw's world Y coordinate using the current feet-distance index.</summary>
    /// <param name="slot">The boss body position.</param>
    /// <param name="state">The feet-distance index selecting the vertical claw offset.</param>
    /// <returns>The claw Y position in room pixels, wrapped to the native 16-bit coordinate word.</returns>
    private static ushort GetNorfairRidleyClawY(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.YPosition + RidleyClawOffsets.ReadY(state.FeetDistanceIndex)));

    /// <summary>Decrements the unsigned AI timer and reports when its signed interpretation has expired.</summary>
    /// <param name="state">The encounter state whose function timer is advanced.</param>
    /// <returns><see langword="true"/> once decrementing reaches the native negative sentinel.</returns>
    private static bool TickRidleyFunctionTimer(RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return (short)state.FunctionTimer < 0;
    }

    /// <summary>Starts a turn animation only when Ridley's facing points away from the arena half containing him.</summary>
    /// <param name="slot">The slot whose instruction and animation timer may change.</param>
    /// <param name="state">The current facing direction used to choose the turn instruction.</param>
    private static void SelectNorfairRidleyFacingInstruction(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.FacingDirection == 1)
            return;
        // D955 reads the word at Enemy.XPosition-1: its sign is bit 7 of
        // the low X byte, not the sign of the full coordinate. Keep an inward
        // facing actor's current animation and timer untouched.
        bool rightHalf = (slot.XPosition & 0x0080) != 0;
        if (state.FacingDirection == 0 ? rightHalf : !rightHalf)
            return;
        SetRidleyInstruction(
            slot,
            state.FacingDirection == 0
                ? RidleyInstructionProgramDefinitions.TurnFromLeftToRight
                : RidleyInstructionProgramDefinitions.TurnFromRightToLeft);
        slot.InstructionTimer = 2;
    }

    /// <summary>
    /// Ports $A6:D3F9. A facing instruction restores the seven rest distances before
    /// reflecting angles around $8000 and setting clockwise movement. The offset words
    /// remain untouched until the following tail controller update.
    /// </summary>
    private static void MirrorRidleyTail(RidleyEnemyState state)
    {
        for (int index = 0; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            segment.Distance = RidleyTailDefinitions.RestDistance(index);
            segment.Angle = unchecked((ushort)(0x8000 - segment.Angle));
            segment.MovementDirection |= 0x8000;
        }
    }

    /// <summary>Maps the clamped health stage to the divisor row used by hover movement.</summary>
    /// <param name="state">Ridley's health stage established by the health palette update.</param>
    /// <returns>The movement divisor table index associated with that stage.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "Preserve the transitive diagnostic instance entry points during this table-only migration.")]
    private int ReadRidleyHealthMovementDivisorIndex(RidleyEnemyState state) =>
        RidleyMovementTargets.HoverDivisorIndexes(Math.Min(state.HealthStage, (ushort)3));

    /// <summary>Reports whether the current Samus movement mode is eligible for Ridley's grab behavior.</summary>
    /// <param name="samus">The player state to inspect; no player is treated as ineligible.</param>
    /// <returns><see langword="true"/> for movement types accepted by the encounter's grab rules.</returns>
    private bool SamusMovementUsesRidleyGrab(SamusState? samus)
    {
        if (samus is null)
            return false;
        SamusMovementType movement = samus.ReadMovementType(_bus!);
        return RidleySamusInteractionDefinitions.CanGrab(movement);
    }

    /// <summary>Accelerates both signed movement axes toward a target using one inertia divisor and optional reversal boost.</summary>
    /// <param name="slot">The body coordinates used to determine each axis's direction and distance.</param>
    /// <param name="state">The horizontal and vertical velocity words updated in place.</param>
    /// <param name="targetX">Target room X coordinate.</param>
    /// <param name="targetY">Target room Y coordinate.</param>
    /// <param name="divisorIndex">Index into the encounter's inertia-divisor definitions.</param>
    /// <param name="reversalBoost">Extra acceleration applied when reversing an axis already moving away from its target.</param>
    private static void MoveNorfairRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        ushort targetX,
        ushort targetY,
        int divisorIndex,
        ushort reversalBoost = 0)
    {
        ushort divisor = RidleyInertiaDefinitions.Divisor(divisorIndex);

        state.HorizontalVelocity = AccelerateNorfairRidleyAxis(
            state.HorizontalVelocity,
            slot.XPosition, targetX,
            divisor,
            reversalBoost);
        state.VerticalVelocity = AccelerateNorfairRidleyAxis(
            state.VerticalVelocity,
            slot.YPosition, targetY,
            divisor,
            reversalBoost);
    }

    /// <summary>Applies the native signed-axis acceleration sequence, preserving its 16-bit carry behavior and velocity limits.</summary>
    /// <param name="velocityWord">Current signed velocity stored as a 16-bit word.</param>
    /// <param name="position">Current coordinate on the axis.</param>
    /// <param name="target">Destination coordinate on the axis.</param>
    /// <param name="divisor">Positive inertia divisor used to scale distance into an acceleration step.</param>
    /// <param name="reversalBoost">Additional adjustment used when velocity points away from the target.</param>
    /// <returns>The updated signed velocity encoded as an unsigned word.</returns>
    private static ushort AccelerateNorfairRidleyAxis(
        ushort velocityWord,
        ushort position,
        ushort target,
        ushort divisor,
        ushort reversalBoost)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return velocityWord;
        ushort step = (ushort)Math.Max(1, Math.Abs((int)distance) / divisor);
        bool carry = position >= target;
        ushort velocity = velocityWord;

        // $A6:D559..D5A5 and D5CF..D61B retain carry/borrow across the chained
        // ADC/SBC instructions. Crossing zero can add one more acceleration unit;
        // combining these operations into integer arithmetic changes the trajectory.
        if (distance > 0)
        {
            if ((short)velocity >= 0)
            {
                carry = true;
                Subtract(reversalBoost);
                carry = true;
                Subtract(RidleyInertiaDefinitions.ReversalAcceleration);
                Subtract(step);
            }
            Subtract(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MinimumVelocity)) < 0) velocity = RidleyInertiaDefinitions.MinimumVelocity;
        }
        else
        {
            if ((short)velocity < 0)
            {
                carry = false;
                Add(reversalBoost);
                carry = false;
                Add(RidleyInertiaDefinitions.ReversalAcceleration);
                Add(step);
            }
            Add(step);
            if (unchecked((short)(velocity - RidleyInertiaDefinitions.MaximumVelocity)) >= 0) velocity = RidleyInertiaDefinitions.MaximumVelocity;
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

    /// <summary>Derives the encounter health stage from current HP and applies the matching shared Ridley damage palette.</summary>
    /// <param name="slot">The boss slot whose health selects a stage.</param>
    /// <param name="state">The health-stage field updated even when no damage palette is needed.</param>
    private void UpdateNorfairRidleyHealthPalette(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.HealthStage = slot.Health switch
        {
            < 1800 => 3,
            < 5400 => 2,
            < 9000 => 1,
            _ => 0,
        };
        if (state.HealthStage == 0)
            return;

        // Norfair and Ceres select the same authored $A6:E46A health rows. Keep
        // Norfair's health thresholds here, but use the installed palette so
        // editing that one asset updates both encounters consistently.
        int row = state.HealthStage - 1;
        (CeresRidleyColors ?? throw new InvalidOperationException(
            "Ridley damage requires installed health colors."))
            .ApplyHealth(_cgram!, row);
    }

    /// <summary>Returns the shared Ridley state after validating the supported header and native slot-zero ownership.</summary>
    /// <param name="slot">The room enemy slot requesting access to shared Ridley state.</param>
    /// <returns>The initialized state extension associated with the Ridley encounter.</returns>
    /// <exception cref="InvalidOperationException">The slot is not a supported Ridley header in slot zero, or the extension is absent.</exception>
    private RidleyEnemyState RequireRidley(RoomEnemySlot slot)
    {
        if (!IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
            slot.SlotIndex != 0 ||
            _ridleyState is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed shared Ridley code without an " +
                "$E13F/$E17F slot-zero state extension.");
        }
        return _ridleyState;
    }

    /// <summary>Validates the Lower Norfair header before returning the shared Ridley state.</summary>
    /// <param name="slot">The room enemy slot expected to host the Lower Norfair boss.</param>
    /// <returns>The initialized state extension for Lower Norfair Ridley.</returns>
    /// <exception cref="InvalidOperationException">The slot uses another definition pointer or fails the shared Ridley ownership checks.</exception>
    private RidleyEnemyState RequireNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != NorfairRidleyDefinition)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed Lower Norfair Ridley code as " +
                $"definition ${slot.EnemyDefinitionPointer:X4}.");
        }
        return RequireRidley(slot);
    }
}
