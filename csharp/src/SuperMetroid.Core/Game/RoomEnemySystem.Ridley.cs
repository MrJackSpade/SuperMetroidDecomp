using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Lower Norfair Ridley ($E17F). The Ceres encounter and the real boss intentionally share
/// initialization, composition, movement, and instruction code in bank $A6; this file owns
/// only the area-two branches and combat dispatcher beginning at $A6:B227.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Executes the private animation instructions shared by Ceres and Lower Norfair Ridley.</summary>
    private bool TryProcessRidleyInstruction(RoomEnemySlot slot, SamusState? samus, ushort word, ref ushort cursor)
    {
        if (!IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((RidleyInstruction)word))
            return false;

        switch ((RidleyInstruction)word)
        {
            case RidleyInstruction.Roar:
                RequireRidley(slot).Roaring = true;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, RidleyInstructionSounds.Roar), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.ClearRoaringFlag:
                RequireRidley(slot).Roaring = false;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.GotoYIfNotNorfairAndSamusHasLowEnergy:
                if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Ridley)
                {
                    cursor = unchecked((ushort)(cursor + 4));
                    return true;
                }
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres Ridley fireball branch requires the active Samus actor.");
                }
                if (unchecked((short)(samus.Health - 30)) < 0)
                {
                    // $A6:E4E2 stores 8 to the Ridley timer at $7E:7800 before the goto,
                    // cutting the fireball hover short.
                    RequireRidley(slot).FunctionTimer = 8;
                    cursor = ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                }
                else
                {
                    cursor = unchecked((ushort)(cursor + 4));
                }
                return true;
            case RidleyInstruction.GotoYIfNotHoldingBaby:
            {
                RidleyEnemyState grabbedBranch = RequireRidley(slot);
                ushort branchOperand = grabbedBranch.GrabState != 0
                    ? (ushort)2
                    : (ushort)4;
                cursor = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + branchOperand)));
                return true;
            }
            case RidleyInstruction.GotoYIfHoldingBaby:
            {
                RidleyEnemyState carryBranch = RequireRidley(slot);
                cursor = carryBranch.GrabState != 0
                    ? unchecked((ushort)(cursor + 4))
                    : ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                return true;
            }
            case RidleyInstruction.CeresFeetDistanceIndexInY:
                RequireRidley(slot).FeetDistanceIndex =
                    ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case RidleyInstruction.GotoYIfNotFacingLeft:
            {
                RidleyEnemyState ridley = RequireRidley(slot);
                cursor = ridley.FacingDirection != 0
                    ? ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)))
                    : unchecked((ushort)(cursor + 4));
                return true;
            }
            case RidleyInstruction.MoveWithArgsInY:
                slot.XPosition = unchecked((ushort)(
                    slot.XPosition +
                    ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)))));
                slot.YPosition = unchecked((ushort)(
                    slot.YPosition +
                    ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 4)))));
                cursor = unchecked((ushort)(cursor + 6));
                return true;
            case RidleyInstruction.FlipLeft:
                MirrorRidleyTail(RequireRidley(slot));
                RequireRidley(slot).FacingDirection = 0;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.FaceForward:
                RequireRidley(slot).FacingDirection = 1;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.FlipRight:
                MirrorRidleyTail(RequireRidley(slot));
                RequireRidley(slot).FacingDirection = 2;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.CalculateFireballXYVelocities:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres Ridley fireball aim requires the active Samus actor.");
                }
                CalculateRidleyFireballVelocity(slot, samus);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.SpawnFireballWithAfterburn:
                SpawnRidleyFireball(slot, spawnAfterburn: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.SpawnFireballWithoutAfterburn:
                SpawnRidleyFireball(slot, spawnAfterburn: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case RidleyInstruction.CeresStartLiftoff:
            {
                RidleyEnemyState liftoff = RequireCeresRidley(slot);
                liftoff.Function = RidleyAiFunction.CeresLiftoffAccelerating;
                liftoff.VerticalVelocity = unchecked((ushort)-352);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case RidleyInstruction.StartLiftoff:
            {
                // The shared roar/liftoff list hands control to the real fight at
                // $B2F3 and supplies the initial upward 8.8 velocity in the same tick.
                RidleyEnemyState norfairLiftoff = RequireNorfairRidley(slot);
                norfairLiftoff.Function = RidleyAiFunction.NorfairEnterArena;
                norfairLiftoff.VerticalVelocity = unchecked((ushort)-352);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            default:
                throw new InvalidOperationException(
                    $"Ridley does not own instruction ${word:X4}.");
        }
    }


    private static bool IsRidleyDefinition(EnemyDefinitionId definitionPointer) =>
        definitionPointer is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley;

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

        Ridley = new RidleyEnemyState
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
            TailFunctionIndex = RidleyTailFunction.None,
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
            _cgram!.SetColor(color, Bgr555.Black);
        for (int color = 241; color <= 255; color++)
            _cgram!.SetColor(color, Bgr555.Black);
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
        PrepareNorfairRidleyCombatFrame(state, sharedProjectiles ?? throw new InvalidOperationException(
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
                    ushort releaseX = RidleyMovementTargets.CarryReleaseX(RidleyMovementTargets.Facing(state.FacingDirection));
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
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.Function = RidleyAiFunction.ClearVelocity;
    }

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

    private static void TickNorfairRidleyHover(RoomEnemySlot slot, RidleyEnemyState state)
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
        RidleyFacing facing = RidleyMovementTargets.Facing(state.FacingDirection);
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

    private static void BeginNorfairRidleyGroundAttack(RidleyEnemyState state)
    {
        state.TailExtensionSpeed = 0xf0;
        state.IdealInterSegmentTailAngle = 16;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.Function = RidleyAiFunction.NorfairFireballMoveToSide;
    }

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

        ushort targetX = RidleyMovementTargets.GroundAttackX(RidleyMovementTargets.Facing(state.FacingDirection));
        MoveNorfairRidleyToward(slot, state, targetX, 288, divisorIndex: 0);
    }

    private void TickNorfairRidleyGroundAttackMoveToHeight(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        MoveNorfairRidleyToward(slot, state, slot.XPosition, 288, divisorIndex: 0);
        if (!TickRidleyFunctionTimer(state))
            return;

        state.TailFunctionIndex = RidleyTailFunction.PogoSetup;
        TickRidleyPogoTail(slot, state, samus);
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
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
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
        state.TailFunctionIndex = RidleyTailFunction.Pogo;
        state.PogoBounceCount = unchecked((ushort)(state.PogoBounceCount + 1));
        if (state.PogoBounceCount >= 2)
        {
            state.PogoBounceCount = 0;
            if (state.FacingDirection != 1)
                SetRidleyInstruction(slot, RidleyInstructionProgramDefinitions.Fireballing);
        }
        state.Function = RidleyAiFunction.NorfairFireballRecover;
    }

    private static void TickNorfairRidleyGroundAttackRecovery(
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || samus.YPosition < 352 || TickRidleyFunctionTimer(state))
        {
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
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

    private static void BeginNorfairRidleyCarry(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.TargetX = RidleyMovementTargets.CarryAnchorX(RidleyMovementTargets.Facing(state.FacingDirection));
        state.TargetY = unchecked((short)(slot.YPosition - 320)) < 0
            ? (ushort)256
            : unchecked((ushort)(slot.YPosition - 64));
        state.Function = RidleyAiFunction.NorfairCarryMoveToAnchor;
        state.FunctionTimer = 32;
        MoveNorfairRidleyToward(slot, state, state.TargetX, state.TargetY, 0);
        TickRidleyFunctionTimer(state);
    }

    private void ReleaseNorfairRidleyGrab(RidleyEnemyState state, SamusState? samus)
    {
        state.GrabState = 0;
        samus?.SetStationaryScriptControlLock(false);
        state.TailWhipRequest = 1;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        // $A6:BC8F-BC93 leaves the timer untouched once death owns the fight.
        if (unchecked((short)state.FightMode) >= 0)
        {
            SamusMovementType movement = samus?.ReadMovementType(_bus!) ?? SamusMovementType.Standing;
            state.IntangibilityTimer = RidleySamusInteractionDefinitions.ReleaseIntangibilityFrames(movement);
        }
    }

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

    private static ushort DecayRidleyGrabOffset(ushort offsetWord)
    {
        int offset = unchecked((short)offsetWord);
        if (offset > 0)
            offset = Math.Max(0, offset - 4);
        else if (offset < 0)
            offset = Math.Min(0, offset + 4);
        return unchecked((ushort)offset);
    }

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

    private static ushort GetNorfairRidleyClawX(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.XPosition + RidleyClawOffsets.ReadX(state.FacingDirection)));

    private static ushort GetNorfairRidleyClawY(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.YPosition + RidleyClawOffsets.ReadY(state.FeetDistanceIndex)));

    private static bool TickRidleyFunctionTimer(RidleyEnemyState state)
    {
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
        return (short)state.FunctionTimer < 0;
    }

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

    private static int ReadRidleyHealthMovementDivisorIndex(RidleyEnemyState state) =>
        RidleyMovementTargets.HoverDivisorIndexes(Math.Min(state.HealthStage, (ushort)3));

    private bool SamusMovementUsesRidleyGrab(SamusState? samus)
    {
        if (samus is null)
            return false;
        SamusMovementType movement = samus.ReadMovementType(_bus!);
        return RidleySamusInteractionDefinitions.CanGrab(movement);
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
            slot.XPosition, targetX,
            divisor,
            reversalBoost);
        state.VerticalVelocity = AccelerateNorfairRidleyAxis(
            state.VerticalVelocity,
            slot.YPosition, targetY,
            divisor,
            reversalBoost);
    }

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

    private RidleyEnemyState RequireRidley(RoomEnemySlot slot)
    {
        if (!IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
            slot.SlotIndex != 0 ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed shared Ridley code without an " +
                "$E13F/$E17F slot-zero state extension.");
        }
        return Ridley;
    }

    private RidleyEnemyState RequireNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Ridley)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed Lower Norfair Ridley code as " +
                $"definition ${(int)slot.EnemyDefinitionPointer:X4}.");
        }
        return RequireRidley(slot);
    }
}
