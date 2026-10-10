using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's private bank-$A4 instruction callbacks. Movement and attack timing are
/// intentionally executed by ROM lists rather than duplicated in the once-per-frame main AI.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Dispatches the 27 named $A4 Crocomire callback identities in
    /// <see cref="CrocomireInstruction"/>. The pinned NTSC J/U v1.0
    /// body programs produce every key. Distinct movement, wall,
    /// sound, shake, fight, and dust effects retain their authored
    /// control order; the bounded dust-offset subrule is documented
    /// in the opcode catalog. Non-Crocomire slots and words outside the
    /// closed set return false so the general interpreter can report an
    /// untranslated Crocomire opcode after its other dispatchers.
    /// </summary>
    private bool TryProcessCrocomireInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort opcode,
        ref ushort cursor,
        ushort cameraX)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Crocomire)
            return false;

        if (!Enum.IsDefined((CrocomireInstruction)opcode))
            return false;

        CrocomireEnemyState state = RequireCrocomire(slot);
        ushort next = unchecked((ushort)(cursor + 2));
        CrocomireInstruction instruction = (CrocomireInstruction)opcode;
        switch (instruction)
        {
            case CrocomireInstruction.FightAI:
                cursor = RunCrocomireFightInstruction(state, samus, next);
                return true;

            case CrocomireInstruction.MaybeStartProjectileAttack:
                if (ReadCrocomireRandom() is ushort attackRandom &&
                    unchecked((short)((attackRandom & 0x0fff) - 0x0400)) < 0)
                {
                    state.FightFunction = CrocomireFightFunction.ProjectileAttack;
                    state.ProjectileCounter = 0;
                    next = CrocomireInstructionProgramDefinitions.ProjectileAttack;
                }
                cursor = next;
                return true;

            case CrocomireInstruction.QueueCrySFX:
                LastCrocomireSoundEffect = 0x0074;
                cursor = next;
                return true;
            case CrocomireInstruction.QueueBigExplosionSFX:
                LastCrocomireSoundEffect = 0x0025;
                cursor = next;
                return true;
            case CrocomireInstruction.QueueSkeletonCollapseSFX:
                LastCrocomireSoundEffect = 0x0075;
                cursor = next;
                return true;
            case CrocomireInstruction.ShakeScreen:
                EarthquakeType = 4;
                EarthquakeTimer = 5;
                LastCrocomireSoundEffect = 0x0076;
                cursor = next;
                return true;

            case CrocomireInstruction.MoveLeft4Pixels:
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case CrocomireInstruction.MoveLeft4Pixels_SpawnBigDustCloud:
            case CrocomireInstruction.MoveLeft4Pixels_SpawnBigDustCloud_dup:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if ((state.FightFlags & 0x0800) == 0)
                    MoveCrocomire(slot, level!, -4);
                cursor = next;
                return true;
            case CrocomireInstruction.MoveLeft_SpawnCloud_HandleSpikeWall:
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
            case CrocomireInstruction.MoveRight4PixelsIfOnScreen:
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.XPosition - slot.XRadius - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4Pixels:
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4PixelsIfOnScreen_SpawnCloud:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                if (unchecked((short)(
                        slot.XPosition - slot.XRadius - 260 - cameraX)) < 0)
                {
                    MoveCrocomire(slot, level!, 4);
                }
                cursor = next;
                return true;
            case CrocomireInstruction.MoveRight4Pixels_SpawnBigDustCloud:
                SpawnCrocomireRandomFootDust(state);
                RequireCrocomireLevel(level);
                MoveCrocomire(slot, level!, 4);
                cursor = next;
                return true;
            case CrocomireInstruction.SpawnBigDustCloudProjectile_Negative20:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_0:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_Negative10:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_10:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_0_dup:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_8:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_10_dup:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_18:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_20:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_28:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_30:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_38:
            case CrocomireInstruction.SpawnBigDustCloudProjectile_40:
                SpawnCrocomireDust(state, CrocomireDustOffset(instruction));
                cursor = next;
                return true;

            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireInstruction)} {opcode:X4}.");
        }
    }

    /// <summary>Signed X offsets loaded by the thirteen $A4:9A9B+5*i dust-projectile stubs.</summary>
    private static int CrocomireDustOffset(CrocomireInstruction instruction) => instruction switch
    {
        CrocomireInstruction.SpawnBigDustCloudProjectile_Negative20 => -32,
        CrocomireInstruction.SpawnBigDustCloudProjectile_0 => 0,
        CrocomireInstruction.SpawnBigDustCloudProjectile_Negative10 => -16,
        CrocomireInstruction.SpawnBigDustCloudProjectile_10 => 16,
        CrocomireInstruction.SpawnBigDustCloudProjectile_0_dup => 0,
        CrocomireInstruction.SpawnBigDustCloudProjectile_8 => 8,
        CrocomireInstruction.SpawnBigDustCloudProjectile_10_dup => 16,
        CrocomireInstruction.SpawnBigDustCloudProjectile_18 => 24,
        CrocomireInstruction.SpawnBigDustCloudProjectile_20 => 32,
        CrocomireInstruction.SpawnBigDustCloudProjectile_28 => 40,
        CrocomireInstruction.SpawnBigDustCloudProjectile_30 => 48,
        CrocomireInstruction.SpawnBigDustCloudProjectile_38 => 56,
        CrocomireInstruction.SpawnBigDustCloudProjectile_40 => 64,
        _ => throw new InvalidOperationException($"{instruction} is not a Crocomire dust-projectile instruction."),
    };

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
        slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
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
