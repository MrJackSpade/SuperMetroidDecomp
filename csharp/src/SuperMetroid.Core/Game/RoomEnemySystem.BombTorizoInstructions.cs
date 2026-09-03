using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Dispatches Bomb Torizo's bank-$AA instruction callbacks. <paramref name="cursor"/>
    /// points at the opcode on entry and at the next opcode/frame/branch target on return.
    /// A true <paramref name="pauseInterpreter"/> reproduces callbacks that return a null
    /// instruction pointer after scheduling a multi-frame delay in the common enemy slot.
    /// </summary>
    private bool TryProcessBombTorizoInstruction(
        RoomEnemySlot torizo,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor,
        ushort controllerInput,
        byte nmiFrameCounter8,
        out bool pauseInterpreter)
    {
        pauseInterpreter = false;
        if (torizo.EnemyDefinitionPointer is not (
                BombTorizoDefinition or GoldenTorizoDefinition))
            return false;

        TorizoEnemyState state = RequireBombTorizoState(torizo);
        int operandAddress = 0xaa0000 | unchecked((ushort)(cursor + 2));
        ushort operand0 = ReadWord(_bus!, operandAddress);

        switch (opcode)
        {
            case TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY:
                state.PreInstruction = operand0;
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_CommonAA3_SetEnemy0FB2ToRTS:
                state.PreInstruction = TorizoPreInstructionIdle;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_FunctionInY:
                state.Function = operand0;
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
                BlackOutBombTorizoPalette();
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetBossBit_QueueElevatorMusic_SpawnDrops:
                FinishBombTorizoDeath(state);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange:
                EarthquakeType = 4;
                EarthquakeTimer = 32;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SetupPaletteTransitionToNormalTorizo:
                LoadTorizoDeathPalette();
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
                ushort operand1 = ReadWord(_bus!, 0xaa0000 |
                    unchecked((ushort)(cursor + 4)));
                if ((torizo.Parameter2 & 0x4000) != 0)
                    cursor = operand0;
                else if (state.IsGolden)
                    cursor = operand1;
                else
                    cursor = unchecked((ushort)(cursor + 6)); // Bomb variant skips both targets.
                return true;
            }

            case TorizoInstructionCodes.Instruction_Torizo_LinkInstructionInY:
                state.ReturnInstruction = operand0;
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
                    SpawnBombTorizoLowHealthExplosion(torizo, operand0);
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
                ApplyBombTorizoMapOffset(torizo, operand0, subtract: false);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SittingDownMovement_IndexInY:
                ApplyBombTorizoMapOffset(torizo, operand0, subtract: true);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY:
                cursor = ProcessBombTorizoWalkInstruction(
                    torizo,
                    state,
                    samus,
                    RequireBombTorizoLevel(level),
                    cursor,
                    operand0,
                    velocityTable: 0xaac4bd,
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
                    operand0,
                    velocityTable: 0xaac532,
                    collisionFacingRight: 0xbd0e,
                    collisionFacingLeft: 0xc188);
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoY_IfRising:
                cursor = unchecked((short)state.VerticalVelocity) < 0
                    ? operand0
                    : unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_CallYIfSamusIsLessThan38PixelsInFront:
                cursor = SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    samus,
                    cursor,
                    operand0);
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_GotoYAndJumpBackwardsIfLessThan20Pixels:
                cursor = SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    samus,
                    cursor,
                    operand0);
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_CallY_OrY2_ForBombTorizoAttack:
            {
                if (samus is null)
                    throw new InvalidOperationException("Bomb Torizo attack selection requires Samus state.");
                ushort operand1 = ReadWord(_bus!, 0xaa0000 |
                    unchecked((ushort)(cursor + 4)));
                state.ReturnInstruction = unchecked((ushort)(cursor + 6));
                bool chooseFirst = samus.Missiles < 5 ||
                    ((nmiFrameCounter8 + (samus.XPosition & 1) + (samus.XPosition >> 1)) & 8) != 0;
                cursor = chooseFirst ? operand0 : operand1;
                return true;
            }

            case TorizoInstructionCodes.Instruction_Torizo_SpawnBombTorizosChozoOrbs:
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY:
                SpawnBombTorizoSonicBoom(torizo, operand0);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY:
                SpawnGoldenTorizoSonicBoom(torizo, operand0);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY:
                SpawnBombTorizoExplosiveSwipe(torizo, operand0);
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
                    : operand0;
                return true;

            case TorizoInstructionCodes.Instruction_Torizo_LoadGoldenTorizoPalettes:
                LoadGoldenTorizoFinalPalette();
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
                    ? operand0
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
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 & ~0x2000));
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
                    operand0);
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnEyeBeam:
                SpawnGoldenTorizoEyeBeam(torizo, operand0);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case TorizoInstructionCodes.Instruction_GT_CallY_25Chance_IfSamusMorphedInFrontOfTorizo:
                cursor = SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    samus,
                    cursor,
                    operand0);
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_25Chance_IfHealthLessThan789:
                if (torizo.Health > 0x0788 || (_nextRandom!() & 0x0102) != 0)
                    cursor = unchecked((ushort)(cursor + 4));
                else
                {
                    state.DecisionCounter = 0;
                    state.ReturnInstruction = unchecked((ushort)(cursor + 4));
                    cursor = operand0;
                }
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_IfStunHealthGreaterThan2A31:
                if (torizo.Health <= 0x2a30 || (torizo.Parameter2 & 0x2000) == 0)
                    cursor = unchecked((ushort)(cursor + 4));
                else
                {
                    state.ReturnInstruction = unchecked((ushort)(cursor + 4));
                    cursor = operand0;
                }
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_GotoY_JumpForwards_IfAtLeast70Pixel:
                cursor = SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    samus,
                    controllerInput,
                    cursor,
                    operand0);
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
                    operand0);
                return true;

            case TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack:
            {
                SamusState activeSamus = RequireGoldenTorizoSamus(samus);
                ushort operand1 = ReadWord(_bus!, 0xaa0000 |
                    unchecked((ushort)(cursor + 4)));
                state.ReturnInstruction = unchecked((ushort)(cursor + 6));
                bool chooseFirst = activeSamus.Missiles < 0x20 ||
                    ((nmiFrameCounter8 + (activeSamus.XPosition & 1) +
                        (activeSamus.XPosition >> 1)) & 8) != 0;
                cursor = chooseFirst ? operand0 : operand1;
                return true;
            }

            case TorizoInstructionCodes.Instruction_GoldenTorizo_WalkingMovement_IndexInY:
                cursor = ProcessGoldenTorizoWalkInstruction(
                    torizo,
                    state,
                    samus,
                    RequireBombTorizoLevel(level),
                    cursor,
                    operand0);
                return true;

            default:
                return false;
        }
    }

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
        if (state.SamusSpaceJumpFrames <= 0x0168 &&
            ((controllerInput & 0x0300) == 0 || (_nextRandom!() & 0x0101) == 0))
        {
            return unchecked((ushort)(cursor + 4));
        }

        state.DecisionCounter = 0;
        StartGoldenTorizoForwardJump(torizo, state);
        return target;
    }

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

    private ushort ProcessGoldenTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        ushort cursor,
        ushort tableOffset)
    {
        state.HorizontalVelocity = ReadWord(
            _bus!,
            EnemyRomTablePointers.Torizo.JumpHorizontalVelocityWords + tableOffset);
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

    private static SamusState RequireGoldenTorizoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Golden Torizo instruction selection requires the active Samus actor.");

    private void ApplyBombTorizoMapOffset(
        RoomEnemySlot torizo,
        ushort tableOffset,
        bool subtract)
    {
        int xTable = subtract ? 0xaac440 : 0xaac3ee;
        int yTable = subtract ? 0xaac460 : 0xaac40e;
        short xDelta = unchecked((short)ReadWord(_bus!, xTable + tableOffset));
        short yDelta = unchecked((short)ReadWord(_bus!, yTable + (tableOffset & 0x000f)));
        torizo.XPosition = unchecked((ushort)(torizo.XPosition + (subtract ? -xDelta : xDelta)));
        torizo.YPosition = unchecked((ushort)(torizo.YPosition + (subtract ? -yDelta : yDelta)));
    }

    private ushort ProcessBombTorizoWalkInstruction(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData level,
        ushort cursor,
        ushort tableOffset,
        int velocityTable,
        ushort collisionFacingRight,
        ushort collisionFacingLeft)
    {
        state.HorizontalVelocity = ReadWord(_bus!, velocityTable + tableOffset);
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
