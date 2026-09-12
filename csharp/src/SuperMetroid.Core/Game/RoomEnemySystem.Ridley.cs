using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Lower Norfair Ridley ($E17F). The Ceres encounter and the real boss intentionally share
/// initialization, composition, movement, and instruction code in bank $A6; this file owns
/// only the area-two branches and combat dispatcher beginning at $A6:B227.
/// </summary>
public sealed partial class RoomEnemySystem
{
    public const ushort NorfairRidleyDefinition = 0xe17f;

    private static readonly byte[] RidleySamusMovementFlags =
    [
        0x80, 0x80, 0x80, 0x00, 0xff, 0x80, 0x80, 0xff,
        0xff, 0xff, 0x80, 0x00, 0x00, 0x80, 0x80, 0x80,
        0x80, 0xff, 0xff, 0xff, 0x80, 0x80, 0x00, 0x80,
        0x80, 0x00, 0x00, 0x80,
    ];

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
        SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E538);
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
            TailMinimumClockwiseAngle = 0x3fc0,
            TailMaximumCounterClockwiseAngle = 0x4010,
            TailWhipTargetClockwiseAngle = 0xffff,
            TailWhipTargetCounterClockwiseAngle = 0xffff,
            TailExtensionSpeed = 0x00f0,
            IdealInterSegmentTailAngle = 0x0010,
            TailSegments = CreateInitialRidleyTailSegments(),
        };

        // WriteColorsToTargetPalette($140, $A6:E1CF, $20) installs both body and manual
        // tail/wing source palettes. The following loops clear the two fifteen-color OBJ
        // ranges that the reveal gradually fills; color zero remains transparent.
        _cgram!.LoadFromBus(
            _bus!,
            EnemyRomTablePointers.Ridley.InitialPaletteWords,
            colorCount: 32,
            destinationIndex: 0x140 / 2);
        for (int color = 113; color <= 127; color++)
            _cgram.SetColor(color, 0);
        for (int color = 241; color <= 255; color++)
            _cgram.SetColor(color, 0);
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
        SamusProjectileSystem? samusProjectiles = null)
    {
        RidleyEnemyState state = RequireNorfairRidley(slot);
        state.HurtMovementClamp = unchecked((ushort)Math.Max(
            0,
            unchecked((short)state.HurtMovementClamp) - 4));

        PrepareNorfairRidleyCombatFrame(slot, state);

        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);

        if (state.GrabState != 0 && samus is not null)
            UpdateNorfairRidleyGrabbedSamus(slot, state, samus);

        if (state.MovementAnimationEnabled != 0)
        {
            UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);
            IntegrateRidleyMovement(slot, state);
            TickRidleyWingAnimation(state);
            TickRidleyTail(slot, state, samus);
            if (samusProjectiles is not null)
                ResolveRidleyTailProjectileHits(slot, state, samusProjectiles);
        }

        UpdateNorfairRidleyHealthPalette(slot, state);
    }

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
                    SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E690);
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
                state.FunctionTimer = 10;
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
                if (state.FunctionTimer != 0)
                {
                    state.FunctionTimer--;
                    return;
                }
                state.Function = SamusMovementUsesRidleyGrab(samus)
                    ? RidleyAiFunction.NorfairGrabApproach
                    : RidleyAiFunction.NorfairSelectAttack;
                return;

            case RidleyAiFunction.NorfairPogoSetup:
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
                TickNorfairRidleyGroundAttackMoveToSide(slot, state);
                return;

            case RidleyAiFunction.NorfairFireballMoveToHeight:
                TickNorfairRidleyGroundAttackMoveToHeight(slot, state);
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
                MoveNorfairRidleyToward(slot, state, state.TargetX, 256, 0);
                if (TickRidleyFunctionTimer(state))
                {
                    ReleaseNorfairRidleyGrab(state, samus);
                    state.Function = RidleyAiFunction.NorfairCarryRelease;
                    state.FunctionTimer = 64;
                }
                return;

            case RidleyAiFunction.NorfairCarryRelease:
                ushort releaseX = RidleyMovementTargets.CarryReleaseX[Math.Min(state.FacingDirection, (ushort)2)];
                MoveNorfairRidleyToward(slot, state, releaseX, 224, 0);
                if (TickRidleyFunctionTimer(state))
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
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
        ushort sourcePointer = ReadWord(
            _bus!,
            EnemyRomTablePointers.Ridley.RevealPaletteSourcePointers +
            state.FadePaletteOffset * 2);
        state.FadePaletteOffset = unchecked((ushort)(state.FadePaletteOffset + 1));
        if (sourcePointer != 0)
        {
            _cgram!.LoadFromBus(
                _bus!,
                0xa60000 | sourcePointer,
                colorCount: 14,
                destinationIndex: 0x00e2 / 2);
            return;
        }

        PublishRidleyLiquidMotion(state, RidleyLiquidRomData.BattleHeight,
            RidleyLiquidRomData.RiseVelocity, RidleyLiquidRomData.RiseDelay);
        state.FadePaletteOffset = 0;
        SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E91D);
        state.WingAnimationTimer = 8;
        state.WingAnimationTimerDelta = 8;
        foreach (RidleyTailSegment segment in state.TailSegments)
            segment.Active = true;
        state.TailFunctionIndex = 1;
        state.Function = RidleyAiFunction.ClearVelocity;
    }

    private void SelectNorfairRidleyAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        ushort controllerInput,
        RoomLevelData? level)
    {
        ReadOnlySpan<RidleyAiFunction> choices;
        if (samus?.ReadMovementType(_bus!) == SamusMovementType.SpinJumping)
        {
            choices = RidleyAttackChoices.SpinJumping;
        }
        else if (slot.Health == 0)
        {
            choices = RidleyAttackChoices.ZeroHealth;
            state.ZeroHealthLungeCount = unchecked((ushort)(state.ZeroHealthLungeCount + 1));
        }
        else if (slot.Health < 14400)
        {
            choices = RidleyAttackChoices.BelowHalfHealth;
        }
        else if (samus is not null && samus.YPosition >= 352)
        {
            choices = RidleyAttackChoices.PogoZone;
        }
        else if (SamusMovementUsesRidleyGrab(samus))
        {
            choices = RidleyAttackChoices.DamageBoosting;
        }
        else
        {
            choices = slot.Health < 9000 ? RidleyAttackChoices.AboveHalfHealth : RidleyAttackChoices.BelowHalfHealth;
        }

        int choice = _nextRandom!() & 7;
        state.Function = choices[choice];

        // The native selector tail-calls the chosen routine. Retain that same-frame setup
        // so timers, instruction changes, and velocity all begin on the selected frame.
        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);
    }

    private void TickNorfairRidleyHover(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.FunctionTimer != 0)
            state.FunctionTimer--;
        else
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

    private static void TickNorfairRidleySwoopMoveToStart(RoomEnemySlot slot, RidleyEnemyState state)
    {
        ushort targetX = state.FacingDirection != 0 ? (ushort)64 : (ushort)192;
        MoveNorfairRidleyToward(slot, state, targetX, 128, divisorIndex: 1);
        if (!IsWithinRidleyRectangle(slot, targetX, 128, 8, 8))
            return;

        state.Function = RidleyAiFunction.NorfairSwoopAimDown;
        state.FunctionTimer = 32;
        state.SwoopAngleAccumulator = 0;
    }

    private static bool TickNorfairRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        ushort nextTimer)
    {
        UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        if (state.FunctionTimer != 0)
        {
            state.FunctionTimer--;
            return false;
        }

        state.Function = nextFunction;
        state.FunctionTimer = nextTimer;
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
        ReadOnlySpan<ushort> targets = descending
            ? RidleyMovementTargets.DescendingPogoX : RidleyMovementTargets.AscendingPogoX;
        ushort targetX = targets[Math.Min(state.FacingDirection, (ushort)2)];
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

        if (!TickRidleyFunctionTimer(state))
            return;

        state.Function = descending
            ? RidleyAiFunction.NorfairPogoAscending
            : RidleyAiFunction.NorfairPogoDescending;
        state.FunctionTimer = 128;
        SelectNorfairRidleyFacingInstruction(slot, state);
    }

    private static void BeginNorfairRidleyGroundAttack(RidleyEnemyState state)
    {
        state.Function = RidleyAiFunction.NorfairFireballMoveToSide;
    }

    private void TickNorfairRidleyGroundAttackMoveToSide(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        if (unchecked((short)(slot.YPosition - 288)) < 0)
        {
            SelectNorfairRidleyFacingInstruction(slot, state);
            state.Function = RidleyAiFunction.NorfairFireballMoveToHeight;
            state.FunctionTimer = 32;
            TickNorfairRidleyGroundAttackMoveToHeight(slot, state);
            return;
        }

        ushort targetX = RidleyMovementTargets.GroundAttackX[Math.Min(state.FacingDirection, (ushort)2)];
        MoveNorfairRidleyToward(slot, state, targetX, 288, divisorIndex: 0);
    }

    private void TickNorfairRidleyGroundAttackMoveToHeight(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        MoveNorfairRidleyToward(slot, state, slot.XPosition, 288, divisorIndex: 0);
        if (!TickRidleyFunctionTimer(state))
            return;

        InitializeNorfairRidleyPogoVelocity(state);
        state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.FunctionTimer = unchecked((ushort)((RequireRandomNumber() & 0x3f) + 128));
    }

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
            BeginNorfairRidleyGrab(slot, state, samus);
            return;
        }

        int nextVertical = unchecked((short)state.VerticalVelocity) +
            unchecked((short)state.PogoDownwardAcceleration);
        state.VerticalVelocity = unchecked((ushort)Math.Min(nextVertical, 1536));

        if (!RidleyTailTouchesTerrain(state, level))
            return;

        EarthquakeType = 13;
        EarthquakeTimer = 4;
        InitializeNorfairRidleyPogoVelocity(state);
        state.PogoBounceCount = unchecked((ushort)(state.PogoBounceCount + 1));
        if (state.PogoBounceCount >= 2)
        {
            state.PogoBounceCount = 0;
            if (state.FacingDirection != 1)
                SetRidleyInstruction(slot, RidleyInstructionLists.Ilist_E73A);
        }
        state.Function = RidleyAiFunction.NorfairFireballRecover;
    }

    private static void TickNorfairRidleyGroundAttackRecovery(
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || samus.YPosition < 352 || TickRidleyFunctionTimer(state))
        {
            state.TailWhipRequest = 0;
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

    private static bool RidleyTailTouchesTerrain(
        RidleyEnemyState state,
        RoomLevelData? level)
    {
        if (level is null || state.TailSegments.Length != 7)
            return false;

        // $B7E7 samples the tip and the next four joints with the exact +16/+18 Y probes.
        int[] segmentIndexes = [6, 5, 4, 3, 2];
        int[] yOffsets = [16, 18, 18, 18, 18];
        for (int index = 0; index < segmentIndexes.Length; index++)
        {
            RidleyTailSegment segment = state.TailSegments[segmentIndexes[index]];
            ushort x = segment.XPosition;
            ushort y = unchecked((ushort)(segment.YPosition + yOffsets[index]));
            if ((x >> 4) >= level.WidthInBlocks || (y >> 4) >= level.HeightInBlocks)
                continue;
            if (level.GetCollisionBlockAtPixel(x, y).CollisionType != 0)
                return true;
        }
        return false;
    }

    private void TickNorfairRidleyGrabApproach(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !SamusMovementUsesRidleyGrab(samus))
        {
            state.Function = RidleyAiFunction.NorfairHoverSetup;
            return;
        }

        int sideOffset = state.FacingDirection != 0 ? 16 : -16;
        ushort targetY = unchecked((ushort)(samus.YPosition - 4));
        int divisorIndex = RidleyMovementTargets.GrabDivisorIndexes[Math.Min(state.HealthStage, (ushort)3)];
        MoveNorfairRidleyToward(
            slot,
            state,
            unchecked((ushort)(samus.XPosition + sideOffset)),
            targetY,
            divisorIndex);

        if (RidleyClawOverlapsSamus(slot, state, samus, radiusX: 8, radiusY: 12))
            BeginNorfairRidleyGrab(slot, state, samus);
    }

    private void BeginNorfairRidleyGrab(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        ushort clawX = GetNorfairRidleyClawX(slot, state);
        ushort clawY = GetNorfairRidleyClawY(slot, state);
        state.GrabXOffset = unchecked((ushort)(samus.XPosition - clawX));
        state.GrabYOffset = unchecked((ushort)(samus.YPosition - clawY));
        state.GrabState = 1;
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (slot.Health == 0)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            return;
        }
        state.Function = RidleyAiFunction.NorfairCarrySetup;
    }

    private static void BeginNorfairRidleyCarry(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.TargetX = RidleyMovementTargets.CarryAnchorX[Math.Min(state.FacingDirection, (ushort)2)];
        state.TargetY = unchecked((short)(slot.YPosition - 320)) < 0
            ? (ushort)256
            : unchecked((ushort)(slot.YPosition - 64));
        state.Function = RidleyAiFunction.NorfairCarryMoveToAnchor;
        state.FunctionTimer = 32;
        MoveNorfairRidleyToward(slot, state, state.TargetX, state.TargetY, 0);
    }

    private void ReleaseNorfairRidleyGrab(RidleyEnemyState state, SamusState? samus)
    {
        state.GrabState = 0;
        state.TailWhipRequest = 1;
        state.TailFunctionIndex = 1;
        SamusMovementType movement = samus?.ReadMovementType(_bus!) ?? SamusMovementType.Standing;
        bool shortRelease = (byte)movement < RidleySamusMovementFlags.Length &&
            (RidleySamusMovementFlags[(byte)movement] & 0x40) != 0;
        state.IntangibilityTimer = shortRelease ? (ushort)6 : (ushort)10;
    }

    private void UpdateNorfairRidleyGrabbedSamus(
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

    private static ushort DecayRidleyGrabOffset(ushort offsetWord)
    {
        int offset = unchecked((short)offsetWord);
        if (offset > 0)
            offset = Math.Max(0, offset - 4);
        else if (offset < 0)
            offset = Math.Min(0, offset + 4);
        return unchecked((ushort)offset);
    }

    private bool RidleyClawOverlapsSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus,
        ushort radiusX,
        ushort radiusY) =>
        Math.Abs(unchecked((short)(samus.XPosition - GetNorfairRidleyClawX(slot, state)))) <
            samus.Kinematics.XRadius + radiusX &&
        Math.Abs(unchecked((short)(samus.YPosition - GetNorfairRidleyClawY(slot, state)))) <
            samus.Kinematics.YRadius + radiusY;

    private ushort GetNorfairRidleyClawX(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.XPosition + unchecked((short)ReadWord(
            _bus!,
            EnemyRomTablePointers.Ridley.ClawXOffsetWords +
            Math.Min(state.FacingDirection, (ushort)2) * 2))));

    private ushort GetNorfairRidleyClawY(RoomEnemySlot slot, RidleyEnemyState state)
    {
        int index = Math.Min(state.FeetDistanceIndex >> 1, (ushort)8);
        return unchecked((ushort)(slot.YPosition + unchecked((short)ReadWord(
            _bus!,
            EnemyRomTablePointers.Ridley.ClawYOffsetWords + index * 2))));
    }

    private static bool TickRidleyFunctionTimer(RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return (short)state.FunctionTimer < 0;
    }

    private static void SelectNorfairRidleyFacingInstruction(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.FacingDirection == 1)
            return;
        SetRidleyInstruction(
            slot,
            state.FacingDirection == 0
                ? RidleyInstructionLists.Ilist_E6F0
                : RidleyInstructionLists.Ilist_E706);
        slot.InstructionTimer = 2;
    }

    /// <summary>
    /// Ports the shared tail half of $A6:D3F9. A facing instruction reflects every polar
    /// angle around $8000 and flips its movement direction; failing to mirror this state is
    /// the classic cause of Ridley's tail appearing several frames late after a turn.
    /// </summary>
    private static void MirrorRidleyTail(RidleyEnemyState state)
    {
        foreach (RidleyTailSegment segment in state.TailSegments)
        {
            segment.Angle = unchecked((ushort)(0x8000 - segment.Angle));
            segment.MovementDirection |= 0x8000;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static",
        Justification = "Preserve the transitive diagnostic instance entry points during this table-only migration.")]
    private int ReadRidleyHealthMovementDivisorIndex(RidleyEnemyState state) =>
        RidleyMovementTargets.HoverDivisorIndexes[Math.Min(state.HealthStage, (ushort)3)];

    private bool SamusMovementUsesRidleyGrab(SamusState? samus)
    {
        if (samus is null)
            return false;
        SamusMovementType movement = samus.ReadMovementType(_bus!);
        return (byte)movement < RidleySamusMovementFlags.Length &&
            (RidleySamusMovementFlags[(byte)movement] & 0x80) != 0;
    }

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
            unchecked((short)(slot.XPosition - targetX)),
            divisor,
            reversalBoost);
        state.VerticalVelocity = AccelerateNorfairRidleyAxis(
            state.VerticalVelocity,
            unchecked((short)(slot.YPosition - targetY)),
            divisor,
            reversalBoost);
    }

    private static ushort AccelerateNorfairRidleyAxis(
        ushort velocityWord,
        short distance,
        ushort divisor,
        ushort reversalBoost)
    {
        if (distance == 0)
            return velocityWord;

        int step = Math.Max(1, Math.Abs((int)distance) / divisor);
        int velocity = unchecked((short)velocityWord);
        if (distance > 0)
        {
            if (velocity >= 0)
                velocity -= reversalBoost + 8 + step;
            velocity -= step;
        }
        else
        {
            if (velocity < 0)
                velocity += reversalBoost + 8 + step;
            velocity += step;
        }

        return unchecked((ushort)Math.Clamp(velocity, -1280, 1280));
    }

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

        _cgram!.LoadFromBus(
            _bus!,
            EnemyRomTablePointers.Ridley.HealthPaletteWords +
            (state.HealthStage - 1) * 28,
            colorCount: 14,
            destinationIndex: 0x01e2 / 2);
    }

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
