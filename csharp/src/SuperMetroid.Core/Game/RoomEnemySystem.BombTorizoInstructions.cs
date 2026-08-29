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
        byte nmiFrameCounter8,
        out bool pauseInterpreter)
    {
        pauseInterpreter = false;
        if (torizo.EnemyDefinitionPointer != BombTorizoDefinition)
            return false;

        BombTorizoEnemyState state = RequireBombTorizoState(torizo);
        int operandAddress = 0xaa0000 | unchecked((ushort)(cursor + 2));
        ushort operand0 = ReadWord(_bus!, operandAddress);

        switch (opcode)
        {
            case 0x806b: // Enemy_SetAiPreInstr_AA
                state.PreInstruction = operand0;
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0x8074: // Enemy_ClearAiPreInstr_AA
                state.PreInstruction = TorizoPreInstructionIdle;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb09c: // Torizo_Instr_3: install main function.
                state.Function = operand0;
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0xb11d: // Torizo_Instr_31: begin six-piece body breakup.
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0x8000));
                for (int piece = 0; piece < 6; piece++)
                    SpawnBombTorizoLowHealthDrool(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb1be: // Torizo_Instr_33: latch the second death phase.
                torizo.Parameter2 = unchecked((ushort)(torizo.Parameter2 | 0x4000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb224: // Torizo_Instr_36: show body.
                torizo.Properties = torizo.Properties.Without(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb22e: // Torizo_Instr_37: hide body.
                torizo.Properties = torizo.Properties.With(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb238: // Torizo_Instr_35: clear both body palette rows.
                BlackOutBombTorizoPalette();
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb24d: // Torizo_Instr_38: boss bit, music, and item drop.
                FinishBombTorizoDeath(state);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb271: // Torizo_Instr_6: native screen-shake helper call.
                EarthquakeType = 4;
                EarthquakeTimer = 32;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb94d: // Torizo_Instr_5: restore normal Bomb Torizo palette.
                LoadBombTorizoPalette();
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xb951: // Torizo_Instr_9: post-awakening music/palette-FX request.
                LastBombTorizoMusicRequest = new BombTorizoMusicRequest(5, 8);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc2c8: // Torizo_Instr_7: explicit RTL/no-op.
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc2c9: // Torizo_Instr_2: protect statue/transition frames from shots.
                state.ShotGuard = BombTorizoShotGuardValue;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc2d1: // Torizo_Instr_8: re-enable shot damage.
                state.ShotGuard = 0;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc2d9: // Torizo_Instr_25: two-target variant/death branch.
            {
                ushort operand1 = ReadWord(_bus!, 0xaa0000 |
                    unchecked((ushort)(cursor + 4)));
                if ((torizo.Parameter2 & 0x4000) != 0)
                    cursor = operand0;
                else
                    cursor = unchecked((ushort)(cursor + 6)); // Bomb variant skips both targets.
                _ = operand1; // Retained/documented for the shared Golden Torizo list format.
                return true;
            }

            case 0xc2ed: // Torizo_Instr_22: save a later landing/list return target.
                state.ReturnInstruction = operand0;
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0xc2f7: // Torizo_Instr_19: return to saved target.
                cursor = state.ReturnInstruction;
                return true;

            case 0xc2fd: // Torizo_Instr_32: restore list interrupted at 350 health.
                cursor = state.InterruptedInstruction;
                return true;

            case 0xc303: // Torizo_Instr_30: six low-health core explosions, wait 40.
                for (int explosion = 0; explosion < 6; explosion++)
                    SpawnBombTorizoLowHealthExplosion(torizo, operand0);
                torizo.CurrentInstruction = unchecked((ushort)(cursor + 4));
                torizo.FlashTimer = 40;
                torizo.InstructionTimer = 40;
                pauseInterpreter = true;
                return true;

            case 0xc32f: // Torizo_Instr_34: one death explosion, resume next frame.
                SpawnBombTorizoDeathExplosion(torizo);
                torizo.CurrentInstruction = unchecked((ushort)(cursor + 2));
                torizo.FlashTimer = 1;
                torizo.InstructionTimer = 1;
                pauseInterpreter = true;
                return true;

            case 0xc34a: // Torizo_Instr_24: two authored landing dust actors.
                SpawnBombTorizoLandingDust(torizo, rightFoot: true);
                SpawnBombTorizoLandingDust(torizo, rightFoot: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc35b: // Torizo_Instr_12: one initial drool below 350 health.
                if (torizo.Health < BombTorizoHeadExplosionHealth)
                    SpawnBombTorizoInitialDrool(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc36d: // Torizo_Instr_10: set centered-drool/facing override.
                torizo.Parameter1 = unchecked((ushort)(torizo.Parameter1 | 0x4000));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc377: // Torizo_Instr_11: facing/pose bits = 0.
                torizo.Parameter1 = unchecked((ushort)(torizo.Parameter1 & 0x1fff));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc38a: // Torizo_Instr_29: facing/pose bits = $8000.
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0x8000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc3a0: // Torizo_Instr_1: facing/pose bits = $2000.
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0x2000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc3b6: // Torizo_Instr_28: facing/pose bits = $A000.
                torizo.Parameter1 = unchecked((ushort)(
                    (torizo.Parameter1 & 0x1fff) | 0xa000));
                state.DecisionCounter = unchecked((ushort)(state.DecisionCounter + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc3cc: // Torizo_Instr_4: authored positive body-map correction.
                ApplyBombTorizoMapOffset(torizo, operand0, subtract: false);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0xc41e: // Torizo_Instr_40: authored inverse body-map correction.
                ApplyBombTorizoMapOffset(torizo, operand0, subtract: true);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0xc470: // Torizo_Instr_16: first walking table.
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

            case 0xc4e5: // Torizo_Instr_27: second walking table.
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

            case 0xc55a: // Torizo_Instr_23: branch while vertical velocity is negative.
                cursor = unchecked((short)state.VerticalVelocity) < 0
                    ? operand0
                    : unchecked((ushort)(cursor + 4));
                return true;

            case 0xc567: // Torizo_Instr_14: close-behind branch.
                cursor = SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    samus,
                    cursor,
                    operand0);
                return true;

            case 0xc58b: // Torizo_Instr_15: close-front jump branch.
                cursor = SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    samus,
                    cursor,
                    operand0);
                return true;

            case 0xc5a4: // Torizo_Instr_26: missile-count/randomized two-way branch.
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

            case 0xc5cb: // Torizo_Instr_18: three Chozo orbs.
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                SpawnBombTorizoChozoOrb(torizo);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc5e3: // Torizo_Instr_20: one sonic boom; operand is retained init data.
                SpawnBombTorizoSonicBoom(torizo, operand0);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0xc601: // Torizo_Instr_21: one frame-positioned explosive swipe.
                SpawnBombTorizoExplosiveSwipe(torizo, operand0);
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case 0xc610: // Torizo_Instr_17: attack sound.
                LastBombTorizoSoundEffect = 0x0027;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case 0xc618: // Torizo_Instr_13: heavy movement/impact sound.
                LastBombTorizoSoundEffect = 0x004b;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

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
        BombTorizoEnemyState state,
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
        BombTorizoEnemyState state,
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
        BombTorizoEnemyState state,
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
