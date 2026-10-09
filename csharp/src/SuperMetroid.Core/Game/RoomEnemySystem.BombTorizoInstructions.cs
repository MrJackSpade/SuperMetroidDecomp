using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Dispatches Bomb Torizo's bank-$AA instruction callbacks. <paramref name="cursor"/>
    /// points at the opcode on entry and at the next opcode/frame/branch target on return.
    /// A true <paramref name="pauseInterpreter"/> reproduces callbacks that return a null
    /// instruction pointer after scheduling a multi-frame delay in the common enemy slot.
    /// <paramref name="frameCounterLow"/> is the low byte of <c>NMI_FrameCounter</c> ($05B6),
    /// which both attack choosers ($AA:C5B9, $AA:D53B) add; only its bit 3 survives.
    /// </summary>
    private bool TryProcessBombTorizoInstruction(
        RoomEnemySlot torizo,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor,
        ushort controllerInput,
        byte frameCounterLow,
        out bool pauseInterpreter)
    {
        pauseInterpreter = false;
        if (torizo.EnemyDefinitionPointer is not (
                BombTorizoDefinition or GoldenTorizoDefinition))
            return false;

        TorizoEnemyState state = RequireBombTorizoState(torizo);
        // A large fraction of bank-$AA callbacks have no operands. Reading the
        // following word before dispatch also reads the next list after a
        // return, and makes a cartridge-free run depend on an unrelated list.
        // Capture this address before any branch mutates the ref cursor.
        ushort operandAddress = unchecked((ushort)(cursor + 2));
        ushort operand0 = 0;
        bool operand0Read = false;
        ushort ReadOperand0()
        {
            if (!operand0Read)
            {
                operand0 = ReadEnemyInstructionMechanicsWord(torizo, operandAddress);
                operand0Read = true;
            }
            return operand0;
        }

        switch (opcode)
        {
            case TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY:
                state.PreInstruction = ReadOperand0();
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_CommonAA3_SetEnemy0FB2ToRTS:
                state.PreInstruction = TorizoPreInstructionIdle;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_FunctionInY:
                state.Function = ReadOperand0();
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_MarkBTGutBlownUp_Spawn6BTDroolProjectiles:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0x8000));
                for (int piece = 0; piece < 6; piece++)
                    SpawnBombTorizoLowHealthDrool(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_MarkBombTorizoFaceBlownUp:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0x4000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetAsVisible:
                torizo.Properties = torizo.Properties.Without(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetAsInvisible:
                torizo.Properties = torizo.Properties.With(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetupPaletteTransitionToBlack:
                SetTorizoPaletteTarget(state, new ushort[32]);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops:
                FinishBombTorizoDeath(state);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange:
                GetTorizoPaletteTransition(state).Step(_cgram!, TorizoPaletteDefinitions.BodyPaletteMask);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetupPaletteTransitionToNormalTorizo:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Normal);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_StartFightMusic_BombTorizoBellyPaletteFX:
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.RTL_AAC2C8:
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetAnimationLock:
                state.ShotGuard = BombTorizoShotGuardValue;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock:
                state.ShotGuard = 0;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoY_IfFaceBlownUp_ElseGotoY2_IfGolden:
            {
                ushort operand1 = ReadEnemyInstructionMechanicsWord(
                    torizo, unchecked((ushort)(cursor + 4)));
                if ((torizo.Parameter2 & 0x4000) != 0)
                    cursor = ReadOperand0();
                else if (state.IsGolden)
                    cursor = operand1;
                else
                    cursor = unchecked((ushort)(cursor + 6)); // Bomb variant skips both targets.
                return true;
            }

            case TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY:
                state.ReturnInstruction = ReadOperand0();
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_Return:
                cursor = state.ReturnInstruction;
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoGutExplosionLinkInstruction:
                cursor = state.InterruptedInstruction;
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_Spawn5LowHealthExplosion_SleepFor28Frames:
                for (int explosion = 0; explosion < 6; explosion++)
                    SpawnBombTorizoLowHealthExplosion(torizo, ReadOperand0());
                torizo.CurrentInstruction = unchecked((ushort)(cursor + 4));
                torizo.FlashTimer = 40;
                torizo.InstructionTimer = 40;
                pauseInterpreter = true;
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoDeathExplosion_SleepFor1IFrame:
                SpawnBombTorizoDeathExplosion(torizo);
                torizo.CurrentInstruction = unchecked((ushort)(cursor + 2));
                torizo.FlashTimer = 1;
                torizo.InstructionTimer = 1;
                pauseInterpreter = true;
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnTorizoLandingDustClouds:
                SpawnBombTorizoLandingDust(torizo, rightFoot: true);
                SpawnBombTorizoLandingDust(torizo, rightFoot: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnLowHealthInitialDroolIfHealthIsLow:
                if (torizo.Health < BombTorizoHeadExplosionHealth)
                    SpawnBombTorizoInitialDrool(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetTorizoTurningAroundFlag:
                torizo.Parameter1 = unchecked((ushort)(torizo.Parameter1 | 0x4000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithLeftFootState:
                torizo.Parameter1 = unchecked((ushort)(torizo.Parameter1 & 0x1fff));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetSteppedRightWithRightFootState:
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0x8000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetSteppedLeftWithRightFootState:
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0x2000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetSteppedRightWithLeftFootState:
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0xa000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY:
                ApplyBombTorizoMapOffset(torizo, ReadOperand0(), subtract: false);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SittingDownMovement_IndexInY:
                ApplyBombTorizoMapOffset(torizo, ReadOperand0(), subtract: true);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY:
                cursor = ProcessBombTorizoWalkInstruction(
                    torizo,
                    state,
                    samus,
                    RequireBombTorizoLevel(level),
                    cursor,
                    ReadOperand0(),
                    collisionFacingRight: 0xb962,
                    collisionFacingLeft: 0xbdd8);
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY:
                cursor = ProcessBombTorizoWalkInstruction(
                    torizo,
                    state,
                    samus,
                    RequireBombTorizoLevel(level),
                    cursor,
                    ReadOperand0(),
                    collisionFacingRight: 0xbd0e,
                    collisionFacingLeft: 0xc188);
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising:
                cursor = unchecked((short)state.VerticalVelocity) < 0
                    ? ReadOperand0()
                    : unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront:
                cursor = SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels:
                cursor = SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack:
            {
                if (samus is null)
                    throw new InvalidOperationException("Bomb Torizo attack selection requires Samus state.");
                ushort operand1 = ReadEnemyInstructionMechanicsWord(
                    torizo, unchecked((ushort)(cursor + 4)));
                state.ReturnInstruction = unchecked((ushort)(cursor + 6));
                bool chooseFirst = samus.Missiles < 5 ||
                    ((frameCounterLow + (samus.XPosition & 1) + (samus.XPosition >> 1)) & 8) != 0;
                cursor = chooseFirst ? ReadOperand0() : operand1;
                return true;
            }

            case TorizoInstructionCodes.Instruction_Torizo_SpawnBombTorizosChozoOrbs:
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY:
                SpawnBombTorizoSonicBoom(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY:
                SpawnGoldenTorizoSonicBoom(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY:
                SpawnBombTorizoExplosiveSwipe(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX:
                LastBombTorizoSoundEffect = 0x0027;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0027), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX:
                LastBombTorizoSoundEffect = 0x004b;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x004b), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoY_IfNotHitGround:
                cursor = torizo.YPosition == 375
                    ? unchecked((ushort)(cursor + 4))
                    : ReadOperand0();
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_LoadGoldenTorizoPalettes:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Golden);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX:
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                torizo.XRadius = 18;
                torizo.YRadius = 48;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_ClearCaughtSuperMissileFlag:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 & ~0x1000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnGoldenTorizoEgg:
                SpawnGoldenTorizoEgg(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_EyeBeamAttack_0:
                cursor = EnemyProjectiles.Any(projectile =>
                        projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg)
                    ? ReadOperand0()
                    : unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_DisableEyeBeamExplosions:
                state.AttackFlags = unchecked((ushort)(state.AttackFlags & ~0x8000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_EnableEyeBeamExplosions:
                state.AttackFlags = unchecked((ushort)(state.AttackFlags | 0x8000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_UnmarkStunned:
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 &
                    ~GoldenTorizoBehavioralProperties.Stunned));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_QueueEggReleasedSFX:
                LastBombTorizoSoundEffect = 0x0034;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0034), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_QueueLaserSFX:
                LastBombTorizoSoundEffect = 0x0067;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0067), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_QueueSonicBoomSFX:
                LastBombTorizoSoundEffect = 0x0048;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0048), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnSuperMissile:
                SpawnGoldenTorizoSuperMissile(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_IfSamusIsMorphedBehindTorizo:
                cursor = SelectGoldenTorizoMorphBallBranch(
                    torizo,
                    state,
                    samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnEyeBeam:
                SpawnGoldenTorizoEyeBeam(torizo, ReadOperand0());
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo:
                cursor = SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789:
                if (torizo.Health > 0x0788 || (_nextRandom!() & 0x0102) != 0)
                    cursor = unchecked((ushort)(cursor + 4));
                else
                {
                    state.DecisionCounter = 0;
                    state.ReturnInstruction = unchecked((ushort)(cursor + 4));
                    cursor = ReadOperand0();
                }
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31:
                if (torizo.Health <= 0x2a30 ||
                    (torizo.Parameter2 & GoldenTorizoBehavioralProperties.Stunned) == 0)
                    cursor = unchecked((ushort)(cursor + 4));
                else
                {
                    state.ReturnInstruction = unchecked((ushort)(cursor + 4));
                    cursor = ReadOperand0();
                }
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel:
                cursor = SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    samus,
                    controllerInput,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs:
                SpawnGoldenTorizoChozoOrb(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpBack_IfLessThan20Pixels:
                cursor = SelectGoldenTorizoRepeatedJump(
                    torizo,
                    state,
                    samus,
                    cursor,
                    ReadOperand0());
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack:
            {
                SamusState activeSamus = RequireGoldenTorizoSamus(samus);
                ushort operand1 = ReadEnemyInstructionMechanicsWord(
                    torizo, unchecked((ushort)(cursor + 4)));
                state.ReturnInstruction = unchecked((ushort)(cursor + 6));
                bool chooseFirst = activeSamus.Missiles < 0x20 ||
                    ((frameCounterLow + (activeSamus.XPosition & 1) +
                        (activeSamus.XPosition >> 1)) & 8) != 0;
                cursor = chooseFirst ? ReadOperand0() : operand1;
                return true;
            }

            case TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY:
                cursor = ProcessGoldenTorizoWalkInstruction(
                    torizo,
                    state,
                    samus,
                    RequireBombTorizoLevel(level),
                    cursor,
                    ReadOperand0());
                return true;

            default:
                return false;
        }
    }

    /// <summary>Selects the morph-ball branch for a grounded ball 4–39 pixels away when the native facing-sign test is negative.</summary>
    /// <param name="torizo">Golden Torizo actor whose position and facing are tested.</param>
    /// <param name="state">Boss state whose decision counter is cleared when the branch is taken.</param>
    /// <param name="samus">Active player state; absence advances past the branch operands.</param>
    /// <param name="cursor">Instruction address of the branch opcode.</param>
    /// <param name="target">Branch target encoded by the first operand.</param>
    /// <returns>The target when the grounded morph-ball condition passes; otherwise the next instruction.</returns>
    private static ushort SelectGoldenTorizoMorphBallBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.XPosition - torizo.XPosition)));
        bool isGroundedBall = samus.Pose is
            0x1d or 0x1e or 0x1f or 0x79 or 0x7a or 0x7b or 0x7c;
        if (BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 4 || distance >= 0x28 || !isGroundedBall)
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        return target;
    }

    /// <summary>Calls the medium-range attack target when Samus is 32–95 pixels away and the native direction and random tests pass.</summary>
    /// <param name="torizo">Golden Torizo actor whose position is compared with Samus.</param>
    /// <param name="state">Boss state receiving the return instruction when the call is taken.</param>
    /// <param name="samus">Active player state; absence skips the branch.</param>
    /// <param name="cursor">Instruction address of the conditional call.</param>
    /// <param name="target">Address of the attack routine to call when its conditions pass.</param>
    /// <returns>The target for a taken call, or the next instruction when skipped.</returns>
    private ushort SelectGoldenTorizoMediumRangeBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.XPosition - torizo.XPosition)));
        if (!BombTorizoFunction12IsNonNegative(torizo, samus) ||
            distance < 0x20 || distance >= 0x60 || (_nextRandom!() & 0x0110) != 0)
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.ReturnInstruction = unchecked((ushort)(cursor + 4));
        return target;
    }

    /// <summary>Starts a forward jump when Samus is at least 112 pixels away, passes the native direction test, and meets the held-input or Space Jump condition.</summary>
    /// <param name="torizo">Golden Torizo actor whose facing determines the jump direction.</param>
    /// <param name="state">Boss state supplying the Space Jump frame counter and receiving movement velocities.</param>
    /// <param name="samus">Active player state used for range and direction checks.</param>
    /// <param name="controllerInput">Current controller word tested for held horizontal directions.</param>
    /// <param name="cursor">Instruction address of the conditional branch.</param>
    /// <param name="target">Target instruction selected after a qualifying jump begins.</param>
    /// <returns>The target after starting the jump, or the next instruction when the condition fails.</returns>
    private ushort SelectGoldenTorizoSpaceJumpCounter(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort controllerInput,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.XPosition - torizo.XPosition)));
        if (distance < 0x70 || !BombTorizoFunction12IsNonNegative(torizo, samus))
            return unchecked((ushort)(cursor + 4));
        SnesButton heldButtons = SnesButtons.FromRaw(
            controllerInput,
            "Golden Torizo jump decision");
        if (state.SamusSpaceJumpFrames <= 0x0168 &&
            (!heldButtons.HasAny(SnesButtons.HorizontalDirections) ||
                (_nextRandom!() & 0x0101) == 0))
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoForwardJump(torizo, state);
        return target;
    }

    /// <summary>Starts a backward jump when the repeat counter permits it or Samus is close and in the native facing relation.</summary>
    /// <param name="torizo">Golden Torizo actor whose facing selects the jump direction.</param>
    /// <param name="state">Boss state containing and resetting the repeated-jump decision counter.</param>
    /// <param name="samus">Active player state used for the close-range and direction checks.</param>
    /// <param name="cursor">Instruction address of the conditional branch.</param>
    /// <param name="target">Target instruction selected after starting the jump.</param>
    /// <returns>The target after the jump starts, or the next instruction when the branch is skipped.</returns>
    private static ushort SelectGoldenTorizoRepeatedJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null)
            return unchecked((ushort)(cursor + 4));
        int distance = Math.Abs(unchecked((short)(samus.XPosition - torizo.XPosition)));
        if (state.DecisionCounter < 8 &&
            (distance >= 0x20 || !BombTorizoFunction12IsNonNegative(torizo, samus)))
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoBackwardJump(torizo, state);
        return target;
    }

    /// <summary>Applies one Golden Torizo walking step, returning to the facing-specific path on collision.</summary>
    /// <param name="torizo">Actor whose movement and instruction flow are updated.</param>
    /// <param name="state">Boss state receiving the selected horizontal velocity and transition timer.</param>
    /// <param name="samus">Optional player state used to begin the airborne transition delay.</param>
    /// <param name="level">Room collision data for movement and slope alignment.</param>
    /// <param name="cursor">Instruction address of the walking opcode.</param>
    /// <param name="tableOffset">Native movement-table offset selecting the walking speed.</param>
    /// <returns>The facing-specific collision branch address or the next instruction.</returns>
    private ushort ProcessGoldenTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        ushort cursor,
        ushort tableOffset)
    {
        state.HorizontalVelocity = GoldenTorizoWalkDefinitions.Velocity(tableOffset);
        int displacement = unchecked((short)state.HorizontalVelocity) << 16;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = 0;
            return (torizo.Parameter1 & 0x8000) != 0
                ? (ushort)0xd203
                : (ushort)0xd2bf;
        }

        AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer == 0)
        {
            state.AirTransitionTimer = 16;
        }
        return unchecked((ushort)(cursor + 4));
    }

    /// <summary>Initializes the forward jump's facing-dependent horizontal speed and shared vertical arc.</summary>
    /// <param name="torizo">Actor whose facing chooses the horizontal direction and whose instruction timer is reset.</param>
    /// <param name="state">Boss state receiving the jump velocities and acceleration.</param>
    private static void StartGoldenTorizoForwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.HorizontalVelocity = (torizo.Parameter1 & 0x8000) != 0
            ? (ushort)512
            : unchecked((ushort)-512);
        state.VerticalVelocity = unchecked((ushort)-1472);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = 1;
    }

    /// <summary>Initializes the backward jump's faster facing-opposite speed and its vertical arc.</summary>
    /// <param name="torizo">Actor whose facing determines the backward horizontal direction and timer reset.</param>
    /// <param name="state">Boss state receiving the jump velocities and acceleration.</param>
    private static void StartGoldenTorizoBackwardJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state)
    {
        state.HorizontalVelocity = (torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)-768)
            : (ushort)768;
        state.VerticalVelocity = unchecked((ushort)-1152);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = 1;
    }

    /// <summary>Requires the active player state for instruction branches whose native decision reads Samus.</summary>
    /// <param name="samus">Player state supplied to the current enemy update.</param>
    /// <returns>The supplied active player state.</returns>
    /// <exception cref="InvalidOperationException">No player state was supplied for the Golden Torizo decision.</exception>
    private static SamusState RequireGoldenTorizoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Golden Torizo instruction selection requires the active Samus actor.");

    /// <summary>Applies a posture-table displacement to the actor's position in the selected direction.</summary>
    /// <param name="torizo">Actor whose coordinates are adjusted.</param>
    /// <param name="tableOffset">Native posture-table offset selecting the displacement.</param>
    /// <param name="subtract"><see langword="true"/> to reverse the table displacement; otherwise apply it forward.</param>
    private static void ApplyBombTorizoMapOffset(
        RoomEnemySlot torizo,
        ushort tableOffset,
        bool subtract)
    {
        BombTorizoPostureDisplacement displacement =
            BombTorizoMovementDefinitions.Posture(tableOffset);
        torizo.XPosition = unchecked((ushort)(
            torizo.XPosition + (subtract ? -displacement.X : displacement.X)));
        torizo.YPosition = unchecked((ushort)(
            torizo.YPosition + (subtract ? -displacement.Y : displacement.Y)));
    }

    /// <summary>Moves Bomb Torizo using the selected walking speed and returns a facing-specific branch on collision.</summary>
    /// <param name="torizo">Actor whose position and instruction flow are updated.</param>
    /// <param name="state">Boss state receiving the walking speed and air-transition timer.</param>
    /// <param name="samus">Optional player state used for the proximity transition test.</param>
    /// <param name="level">Room collision data used by movement and slope alignment.</param>
    /// <param name="cursor">Instruction address of the walking opcode.</param>
    /// <param name="tableOffset">Native movement-table offset selecting the speed.</param>
    /// <param name="collisionFacingRight">Branch target used for collision while facing right.</param>
    /// <param name="collisionFacingLeft">Branch target used for collision while facing left.</param>
    /// <returns>The collision branch address or the next instruction.</returns>
    private ushort ProcessBombTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        ushort cursor,
        ushort tableOffset,
        ushort collisionFacingRight,
        ushort collisionFacingLeft)
    {
        state.HorizontalVelocity = BombTorizoMovementDefinitions.WalkVelocity(tableOffset);
        int displacement = unchecked((short)state.HorizontalVelocity) << 16;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, torizo, displacement))
        {
            state.AirTransitionTimer = 0;
            return (torizo.Parameter1 & 0x8000) != 0
                ? collisionFacingRight
                : collisionFacingLeft;
        }

        AlignEnemyYWithNonSquareSlope(level, torizo);
        if (samus is not null && BombTorizoParameterMatchesSamusDelta(torizo, samus) &&
            state.AirTransitionTimer == 0)
        {
            state.AirTransitionTimer = 72;
        }
        return unchecked((ushort)(cursor + 4));
    }

    /// <summary>Calls the close-range branch when Samus is within 56 pixels and lies in front of the actor's facing.</summary>
    /// <param name="torizo">Actor whose position and facing define the relative direction.</param>
    /// <param name="state">Boss state receiving the return address when the branch is taken.</param>
    /// <param name="samus">Active player state; absence skips the branch.</param>
    /// <param name="cursor">Instruction address of the conditional call.</param>
    /// <param name="target">Attack routine address selected by the native operand.</param>
    /// <returns>The target for a taken call, or the next instruction when skipped.</returns>
    private static ushort SelectBombTorizoCloseBehindBranch(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.XPosition - torizo.XPosition))) >= 0x38)
            return unchecked((ushort)(cursor + 4));
        if (BombTorizoParameterMatchesSamusDelta(torizo, samus))
            return unchecked((ushort)(cursor + 4));
        state.ReturnInstruction = unchecked((ushort)(cursor + 4));
        return target;
    }

    /// <summary>Starts a jump away from Samus when she is within 32 pixels and satisfies the native direction test.</summary>
    /// <param name="torizo">Actor whose facing determines the jump direction.</param>
    /// <param name="state">Boss state receiving jump velocities and acceleration.</param>
    /// <param name="samus">Active player state used for range and direction checks.</param>
    /// <param name="cursor">Instruction address of the conditional branch.</param>
    /// <param name="target">Instruction target selected after the jump starts.</param>
    /// <returns>The target when the jump starts, or the next instruction when conditions fail.</returns>
    private static ushort SelectBombTorizoCloseFrontJump(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        ushort cursor,
        ushort target)
    {
        if (samus is null || Math.Abs(unchecked((short)(samus.XPosition - torizo.XPosition))) >= 0x20 ||
            !BombTorizoFunction12IsNonNegative(torizo, samus))
        {
            return unchecked((ushort)(cursor + 4));
        }

        // $AA:C22D launches away from the currently faced direction at 3 px/frame and
        // starts with -4.5 px/frame vertical speed, adding 40/256 px/frame each tick.
        state.HorizontalVelocity = (torizo.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)-768)
            : (ushort)768;
        state.VerticalVelocity = unchecked((ushort)-1152);
        state.VerticalAcceleration = 40;
        torizo.InstructionTimer = 1;
        return target;
    }

    /// <summary>Tests whether the actor's facing bit agrees with the sign of Samus X minus enemy X.</summary>
    /// <param name="torizo">Actor supplying the facing bit and horizontal position.</param>
    /// <param name="samus">Player supplying the horizontal position.</param>
    /// <returns><see langword="true"/> when the facing bit matches that signed delta test.</returns>
    private static bool BombTorizoParameterMatchesSamusDelta(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // Instructions $C470/$C4E5/$C567 compare the facing bit against SamusX-enemyX.
        // Keep that subtraction direction explicit: Torizo_Func_12 below deliberately uses
        // the inverse delta, and collapsing both tests into one "faces Samus" helper silently
        // reverses one family of ROM branches.
        return (((short)torizo.Parameter1 ^ unchecked((short)(samus.XPosition - torizo.XPosition))) &
            short.MinValue) == 0;
    }

    /// <summary>Reproduces Torizo_Func_12's sign test using enemy X minus Samus X.</summary>
    /// <param name="torizo">Actor supplying the facing parameter and horizontal position.</param>
    /// <param name="samus">Player supplying the horizontal position.</param>
    /// <returns><see langword="true"/> when the XOR result has a clear sign bit.</returns>
    private static bool BombTorizoFunction12IsNonNegative(
        RoomEnemySlot torizo,
        SamusState samus)
    {
        // $AA:C58B calls Torizo_Func_12, whose operand is parameter1 XOR
        // (enemyX-SamusX), rather than the SamusX-enemyX test used by the walkers.
        return (((short)torizo.Parameter1 ^ unchecked((short)(torizo.XPosition - samus.XPosition))) &
            short.MinValue) == 0;
    }
}
