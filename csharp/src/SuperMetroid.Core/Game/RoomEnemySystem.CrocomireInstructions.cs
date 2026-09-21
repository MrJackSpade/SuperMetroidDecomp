using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's private bank-$A4 instruction callbacks. Movement and attack timing are
/// intentionally executed by ROM lists rather than duplicated in the once-per-frame main AI.
/// </summary>
public sealed partial class RoomEnemySystem
{
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
            case CrocomireCodePointers.Instruction_Crocomire_FightAI:
                cursor = RunCrocomireFightInstruction(state, samus, next);
                return true;

            case CrocomireCodePointers.Instruction_Crocomire_MaybeStartProjectileAttack:
                if (ReadCrocomireRandom() is ushort attackRandom &&
                    unchecked((short)((attackRandom & 0x0fff) - 0x0400)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.ProjectileAttack;
                    state.ProjectileCounter = 0;
                    next = CrocomireInstructionProgramDefinitions.ProjectileAttack;
                }
                cursor = next;
                return true;

            case CrocomireCodePointers.Instruction_Crocomire_QueueCrySFX:
                LastCrocomireSoundEffect = 0x0074;
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_QueueBigExplosionSFX:
                LastCrocomireSoundEffect = 0x0025;
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_QueueSkeletonCollapseSFX:
                LastCrocomireSoundEffect = 0x0075;
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_ShakeScreen:
                EarthquakeType = 4;
                EarthquakeTimer = 5;
                LastCrocomireSoundEffect = 0x0076;
                cursor = next;
                return true;

            case CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels:
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud:
            case CrocomireCodePointers.Instruction_Crocomire_MoveLeft4Pixels_SpawnBigDustCloud_dup:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_MoveLeft_SpawnCloud_HandleSpikeWall:
                RequireCrocomireLevel(level);
                if (MoveCrocomire(slot, level!, -4))
                {
                    state.FightFunction = CrocomireFightFunction.BackingOffSpikeWall;
                    next = CrocomireInstructionProgramDefinitions.BackOffFromSpikeWall;
                }
                else
                {
                    ushort random = ReadCrocomireRandom();
                    int baseOffset = unchecked((short)(random - 0x0800)) >= 0 ? -32 : 32;
                    SpawnCrocomireDust(state, baseOffset + (random & 0x000f));
                }
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen:
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.XPosition - slot.XRadius - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels:
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_MoveRight4PixelsIfOnScreen_SpawnCloud:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.XPosition - slot.XRadius - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case CrocomireCodePointers.Instruction_Crocomire_MoveRight4Pixels_SpawnBigDustCloud:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
        }

        int? dustOffset = opcode switch
        {
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative20 => -32,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0 => 0,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_Negative10 => -16,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10 => 16,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_0_dup => 0,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_8 => 8,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_10_dup => 16,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_18 => 24,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_20 => 32,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_28 => 40,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_30 => 48,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_38 => 56,
            CrocomireCodePointers.Instruction_Crocomire_SpawnBigDustCloudProjectile_40 => 64,
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
                return CrocomireInstructionProgramDefinitions.Initial;

            case CrocomireFightFunction.StepForward:
                state.FightFunction = CrocomireFightFunction.Sleeping;
                return CrocomireInstructionProgramDefinitions.StepForward;

            case CrocomireFightFunction.Sleeping:
                if (samus is not null && WrappedMagnitude(unchecked((ushort)(
                        state.Body.XPosition - samus.XPosition))) < 224)
                {
                    state.FightFlags |= 0x8000;
                    state.FightFunction = CrocomireFightFunction.WaitingForFirstDamage;
                    return CrocomireInstructionProgramDefinitions.WaitForFirstSecondDamage;
                }
                return next;

            case CrocomireFightFunction.SteppingForward:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    if (state.StepCounter != 0)
                    {
                        state.FightFunction = CrocomireFightFunction.SteppingBack;
                        return CrocomireInstructionProgramDefinitions.StepBack;
                    }
                }
                if (unchecked((short)(
                        state.Body.XPosition - CrocomireSpikeWallThreshold)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.NearSpikeWallCharge;
                    return CrocomireInstructionProgramDefinitions.NearSpikeWallCharge;
                }
                return unchecked((short)(
                        next - CrocomireInstructionProgramDefinitions.SteppingBack)) >= 0
                    ? CrocomireInstructionProgramDefinitions.StepForward
                    : next;

            case CrocomireFightFunction.ProjectileAttack:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireInstructionProgramDefinitions.StepBack;
                }
                if (unchecked((short)(state.ProjectileCounter - 18)) < 0)
                {
                    state.ProjectileCounter = unchecked((ushort)(state.ProjectileCounter + 2));
                    SpawnCrocomireProjectile(state.Body, state.ProjectileCounter);
                    LastCrocomireSoundEffect = 0x001c;
                    return next;
                }
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireInstructionProgramDefinitions.StepForwardAfterDelay;

            case CrocomireFightFunction.NearSpikeWallCharge:
                if ((state.FightFlags & 0x0800) != 0)
                {
                    state.FightFlags &= 0xf7ff;
                    state.FightFunction = CrocomireFightFunction.SteppingBack;
                    return CrocomireInstructionProgramDefinitions.StepBack;
                }
                return next;

            case CrocomireFightFunction.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                if (state.StepCounter != 0)
                    return CrocomireInstructionProgramDefinitions.SteppingBack;
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireInstructionProgramDefinitions.StepForward;

            case CrocomireFightFunction.BackingOffSpikeWall:
                if (unchecked((short)(
                        state.Body.XPosition - CrocomireSpikeWallThreshold)) >= 0)
                {
                    state.FightFunction = CrocomireFightFunction.SteppingForward;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                return next;

            case CrocomireFightFunction.RoarAndStepForwardUnused:
                state.FightFunction = CrocomireFightFunction.SteppingForward;
                return CrocomireInstructionProgramDefinitions.Roar;

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
                    return CrocomireInstructionProgramDefinitions.StepForward;
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
                return CrocomireInstructionProgramDefinitions.RoarCloseMouth;

            case CrocomireFightFunction.ResetAnimationUnused:
                state.Body.InstructionTimer = 1;
                state.FightFlags |= 0x0200;
                state.StepCounter = 32;
                state.FightFunction = CrocomireFightFunction.ChooseAttackUnused;
                return CrocomireInstructionProgramDefinitions.Initial;

            case CrocomireFightFunction.ChooseAttackUnused:
                if ((state.FightFlags & 0x0100) != 0)
                {
                    state.Body.InstructionTimer = 1;
                    state.StepCounter = 16;
                    state.FightFunction = CrocomireFightFunction.MoveUntilSamusUnused;
                    return CrocomireInstructionProgramDefinitions.Initial;
                }
                state.FightFunction = CrocomireFightFunction.StepForwardUnused;
                return CrocomireInstructionProgramDefinitions.UnusedChargeForwardOneStep;

            case CrocomireFightFunction.StepForwardUnused:
                state.Body.InstructionTimer = 1;
                if (state.StepCounter == 0)
                {
                    state.FightFlags |= 0x2000;
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                return CrocomireInstructionProgramDefinitions.Initial;

            case CrocomireFightFunction.MoveUntilSamusUnused:
                if (unchecked((short)(state.Body.XPosition - 672)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                    state.StepCounter = 3;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                if ((state.FightFlags & 0x4000) == 0)
                {
                    state.FightFunction = CrocomireFightFunction.StepForwardVariantUnused;
                    state.FightFlags &= 0xfbff;
                    return CrocomireInstructionProgramDefinitions.MovingClaws;
                }
                state.StepCounter = 5;
                state.ProjectileCounter = (ushort)state.FightFunction;
                state.FightFunction = (CrocomireFightFunction)0x2a;
                return CrocomireInstructionProgramDefinitions.MovingClaws;

            case CrocomireFightFunction.MoveClawsUnused:
                if (state.StepCounter == 0 || --state.StepCounter == 0)
                {
                    state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                    state.FightFlags &= 0xfbff;
                    return CrocomireInstructionProgramDefinitions.StepForward;
                }
                state.FightFunction = CrocomireFightFunction.MoveClawsUnused;
                if (state.Tongue is not null)
                    state.Tongue.VariableD = 0;
                state.FightFlags |= 0x0400;
                return CrocomireInstructionProgramDefinitions.MovingClaws;

            case CrocomireFightFunction.StepForwardVariantUnused:
                if ((state.FightFlags & 0x2000) == 0)
                    state.FightFlags &= 0xfcff;
                state.FightFunction = CrocomireFightFunction.MovingClawsUnused;
                return CrocomireInstructionProgramDefinitions.StepForward;

            case CrocomireFightFunction.MovingClawsUnused:
                if (state.StepCounter == 0)
                {
                    state.FightFlags &= 0xbfff;
                    state.Body.InstructionTimer = 1;
                    state.FightFunction = (CrocomireFightFunction)state.ProjectileCounter;
                    return CrocomireInstructionProgramDefinitions.MovingClaws;
                }
                if ((state.FightFlags & 0x4000) != 0)
                {
                    state.StepCounter--;
                    LastCrocomireSoundEffect = 0x003b;
                    return CrocomireInstructionProgramDefinitions.MovingClaws;
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
            return CrocomireInstructionProgramDefinitions.StepBack;
        }
        return unchecked((short)(
                next - CrocomireInstructionProgramDefinitions.RoarCloseMouthLoop)) >= 0
            ? CrocomireInstructionProgramDefinitions.Roar
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
