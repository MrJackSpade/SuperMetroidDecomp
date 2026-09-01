using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's private bank-$A4 instruction callbacks. Movement and attack timing are
/// intentionally executed by ROM lists rather than duplicated in the once-per-frame main AI.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort CrocomireStepForwardList = 0xbbce;
    private const ushort CrocomireProjectileAttackList = 0xbb36;
    private const ushort CrocomireStepForwardDelayList = 0xbbca;
    private const ushort CrocomireStepBackList = 0xbc30;
    private const ushort CrocomireSteppingBackList = 0xbc34;
    private const ushort CrocomireWaitForDamageList = 0xbc56;
    private const ushort CrocomireMovingClawsList = 0xbcd8;
    private const ushort CrocomireRoarList = 0xbd2a;
    private const ushort CrocomireRoarClosedBoundary = 0xbda2;
    private const ushort CrocomireRoarCloseList = 0xbd8e;
    private const ushort CrocomireChargeForwardUnusedList = 0xbaea;
    private const ushort CrocomireNearWallChargeList = 0xbe7e;
    private const ushort CrocomireBackOffList = 0xbf3c;

    private bool TryProcessCrocomireInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor,
        ushort cameraX)
    {
        if (slot.EnemyDefinitionPointer != CrocomireDefinition)
            return false;

        CrocomireEnemyState state = RequireCrocomire(slot);
        ushort next = unchecked((ushort)(cursor + 2));
        switch (opcode)
        {
            case 0x86a6: // Dispatch the 21-entry fight-function table.
                cursor = RunCrocomireFightInstruction(state, samus, next);
                return true;

            case 0x8752: // Randomly begin the projectile volley.
                if (ReadCrocomireRandom() is ushort attackRandom &&
                    unchecked((short)((attackRandom & 0x0fff) - 0x0400)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.ProjectileAttack;
                    state.ProjectileCounter = 0;
                    next = CrocomireProjectileAttackList;
                }
                cursor = next;
                return true;

            case 0x8cfb: // Cry.
                LastCrocomireSoundEffect = 0x0074;
                cursor = next;
                return true;
            case 0x8d07: // Footstep.
                LastCrocomireSoundEffect = 0x0025;
                cursor = next;
                return true;
            case 0x8d13: // Alternate cry.
                LastCrocomireSoundEffect = 0x0075;
                cursor = next;
                return true;
            case 0x8fc7: // Bridge/footstep quake.
                EarthquakeType = 4;
                EarthquakeTimer = 5;
                LastCrocomireSoundEffect = 0x0076;
                cursor = next;
                return true;

            case 0x8fdf: // Move left four pixels unless a mouth hit is pending.
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case 0x8ffa:
            case 0x8fff:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case 0x901d: // Charge left; collision selects the authored backing-off list.
                RequireCrocomireLevel(level);
                if (MoveCrocomire(slot, level!, -4))
                {
                    state.FightFunction = CrocomireFightFunction.BackingOffSpikeWall;
                    next = CrocomireBackOffList;
                }
                else
                {
                    ushort random = ReadCrocomireRandom();
                    int baseOffset = unchecked((short)(random - 0x0800)) >= 0 ? -32 : 32;
                    SpawnCrocomireDust(state, baseOffset + (random & 0x000f));
                }
                cursor = next;
                return true;
            case 0x905b: // Step right only while the left edge is inside camera+260.
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.XPosition - slot.XRadius - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case 0x907f:
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
            case 0x908f:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.XPosition - slot.XRadius - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case 0x9094:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
        }

        int? dustOffset = opcode switch
        {
            0x9a9b => -32,
            0x9aa0 => 0,
            0x9aa5 => -16,
            0x9aaa => 16,
            0x9aaf => 0,
            0x9ab4 => 8,
            0x9ab9 => 16,
            0x9abe => 24,
            0x9ac3 => 32,
            0x9ac8 => 40,
            0x9acd => 48,
            0x9ad2 => 56,
            0x9ad7 => 64,
            _ => null,
        };
        if (dustOffset is not int offset)
            return false;

        SpawnCrocomireDust(state, offset);
        cursor = next;
        return true;
    }

    /// <summary>Ports $A4:86B3-$8A39, including the shipped but normally unused states.</summary>
    private ushort RunCrocomireFightInstruction(
        CrocomireEnemyState state,
        SamusState? samus,
        ushort next)
    {
        switch (state.FightFunction)
        {
            case CrocomireFightFunction.ResetAnimation:
                state.Body.InstructionTimer = 1;
                return CrocomireInitialInstructionList;

            case CrocomireFightFunction.StepForward:
                state.FightFunction = CrocomireFightFunction.Sleeping;
                return CrocomireStepForwardList;

            case CrocomireFightFunction.Sleeping:
                if (samus is not null && WrappedMagnitude(unchecked((ushort)(
                        state.Body.XPosition - samus.XPosition))) < 224)
                {
                    state.FightFlags |= 0x8000;
                    state.FightFunction = CrocomireFightFunction.WaitingForFirstDamage;
                    return CrocomireWaitForDamageList;
                }
                return next;

            case CrocomireFightFunction.SteppingForward:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    if (state.StepCounter != 0)
                    {
                        state.FightFunction = CrocomireFightFunction.SteppingBack;
                        return CrocomireStepBackList;
                    }
                }
                if (unchecked((short)(
                        state.Body.XPosition - CrocomireSpikeWallThreshold)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                    return CrocomireNearWallChargeList;
                }
                return unchecked((short)(next - CrocomireSteppingBackList)) >= 0
                    ? CrocomireStepForwardList
                    : next;

            case CrocomireFightFunction.ProjectileAttack:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireStepBackList;
                }
                if (unchecked((short)(state.ProjectileCounter - 18)) < 0)
                {
                    state.ProjectileCounter = unchecked((ushort)(state.ProjectileCounter + 2));
                    SpawnCrocomireProjectile(state.Body, state.ProjectileCounter);
                    LastCrocomireSoundEffect = 0x001c;
                    return next;
                }
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireStepForwardDelayList;

            case CrocomireFightFunction.NearSpikeWallCharge:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireStepBackList;
                }
                return next;

            case CrocomireFightFunction.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                if (state.StepCounter != 0)
                    return CrocomireSteppingBackList;
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireStepForwardList;

            case CrocomireFightFunction.BackingOffSpikeWall:
                if (unchecked((short)(
                        state.Body.XPosition - CrocomireSpikeWallThreshold)) >= 0)
                {
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireStepForwardList;
                }
                return next;

            case CrocomireFightFunction.RoarAndStepForwardUnused:
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireRoarList;

            case CrocomireFightFunction.WaitingForFirstDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    next,
                    CrocomireFightFunction.WaitingForSecondDamage);

            case CrocomireFightFunction.WaitingForSecondDamage:
            case CrocomireFightFunction.WaitingForSecondDamageUnused:
                return RunCrocomireWaitingForDamage(
                    state,
                    next,
                    CrocomireFightFunction.SteppingBack);

            case CrocomireFightFunction.PowerBombCharge:
                state.StepCounter--;
                if (unchecked((short)(state.StepCounter - 2)) < 0)
                {
                    state.StepCounter = 0;
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireStepForwardList;
                }
                return next;

            case CrocomireFightFunction.NearSpikeWallChargeUnused:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags = unchecked((ushort)((state.FightFlags & 0x1f00) | 0xa000));
                    state.StepCounter = 1;
                    state.ReactionTimer = 10;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    LastCrocomireSoundEffect = 0x0054;
                    return next;
                }
                state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                return CrocomireRoarCloseList;

            case CrocomireFightFunction.ResetAnimationUnused:
                state.Body.InstructionTimer = 1;
                state.FightFlags |= 0x0200;
                state.StepCounter = 32;
                state.FightFunction = CrocomireFightFunction.ChooseAttackUnused;
                return CrocomireInitialInstructionList;

            case CrocomireFightFunction.ChooseAttackUnused:
                if ((state.FightFlags & 0x0100) != 0)
                {
                    state.Body.InstructionTimer = 1;
                    state.StepCounter = 16;
                    state.FightFunction = CrocomireFightFunction.MoveUntilSamusUnused;
                    return CrocomireInitialInstructionList;
                }
                state.FightFunction = CrocomireFightFunction.StepForwardUnused;
                return CrocomireChargeForwardUnusedList;

            case CrocomireFightFunction.StepForwardUnused:
                state.Body.InstructionTimer = 1;
                if (state.StepCounter == 0)
                {
                    state.FightFlags |= 0x2000;
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    return CrocomireStepForwardList;
                }
                return CrocomireInitialInstructionList;

            case CrocomireFightFunction.MoveUntilSamusUnused:
                if (unchecked((short)(state.Body.XPosition - 672)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    state.StepCounter = 3;
                    return CrocomireStepForwardList;
                }
                if ((state.FightFlags & 0x4000) == 0)
                {
                    state.FightFunction = CrocomireFightFunction.StepForwardVariantUnused;
                    state.FightFlags &= 0xfbff;
                    return CrocomireMovingClawsList;
                }
                state.StepCounter = 5;
                state.ProjectileCounter = (ushort)state.FightFunction;
                state.FightFunction = (CrocomireFightFunction)0x2a;
                return CrocomireMovingClawsList;

            case CrocomireFightFunction.MoveClawsUnused:
                if (state.StepCounter == 0 || --state.StepCounter == 0)
                {
                    state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                    state.FightFlags &= 0xfbff;
                    return CrocomireStepForwardList;
                }
                state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                if (state.Tongue is not null)
                    state.Tongue.VariableD = 0;
                state.FightFlags |= 0x0400;
                return CrocomireMovingClawsList;

            case CrocomireFightFunction.StepForwardVariantUnused:
                if ((state.FightFlags & 0x2000) == 0)
                    state.FightFlags &= 0xfcff;
                state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                return CrocomireStepForwardList;

            case CrocomireFightFunction.MovingClawsUnused:
                if (state.StepCounter == 0)
                {
                    state.FightFlags &= 0xbfff;
                    state.Body.InstructionTimer = 1;
                    state.FightFunction = (CrocomireFightFunction)state.ProjectileCounter;
                    return CrocomireMovingClawsList;
                }
                if ((state.FightFlags & 0x4000) != 0)
                {
                    state.StepCounter--;
                    LastCrocomireSoundEffect = 0x003b;
                    return CrocomireMovingClawsList;
                }
                state.FightFlags &= 0xbfff;
                state.FightFunction = CrocomireFightFunction.SteppingBack;
                return next;

            default:
                throw new InvalidDataException(
                    $"Crocomire fight function ${((ushort)state.FightFunction):X2} is not translated.");
        }
    }

    private static ushort RunCrocomireWaitingForDamage(
        CrocomireEnemyState state,
        ushort next,
        CrocomireFightFunction damagedDestination)
    {
        if ((state.FightFlags & 0x0800) != 0)
        {
            state.FightFlags &= 0xf7ff;
            state.FightFunction = damagedDestination;
            return CrocomireStepBackList;
        }
        return unchecked((short)(next - CrocomireRoarClosedBoundary)) >= 0
            ? CrocomireRoarList
            : next;
    }

    private bool MoveCrocomire(RoomEnemySlot slot, RoomLevelData level, int pixels) =>
        // The instruction callbacks pass INT16_SHL16(±4) into the ordinary ignore-slopes
        // enemy mover; retaining 16.16 here preserves collision-edge alignment.
        slot.EnemyDefinitionPointer == CrocomireDefinition &&
        MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, pixels << 16);

    private ushort ReadCrocomireRandom() => RequireRandomNumber();

    private void SpawnCrocomireRandomFootDust(CrocomireEnemyState state)
    {
        ushort random = ReadCrocomireRandom();
        int offset = random & 0x001f;
        if (unchecked((short)(random - 0x1000)) >= 0)
            offset = -offset;
        SpawnCrocomireDust(state, offset);
    }

    private void SpawnCrocomireDust(CrocomireEnemyState state, int xOffset)
    {
        ushort random = ReadCrocomireRandom();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(state.Body.XPosition + xOffset + (random & 7))),
            unchecked((ushort)(state.Body.YPosition + state.Body.YRadius - 16)),
            animationIndex: 0x15);
    }

    private static void RequireCrocomireLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Crocomire movement requires room level data.");
    }
}
