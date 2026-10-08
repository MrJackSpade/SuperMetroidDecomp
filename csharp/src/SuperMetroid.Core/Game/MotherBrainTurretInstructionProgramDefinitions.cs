namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Mother Brain's eight turret poses, direction-selected bullets, and
/// shared bullet touch/shot smoke. Spritemap operands resolve through extracted
/// presentation art.
/// </summary>
internal abstract class MotherBrainTurretInstructionProgramDefinitions
{
    /// <summary>Left-facing turret pose at $86:C101.</summary>
    internal const ushort TurretLeft = 0xc101;

    /// <summary>Down-facing turret pose at $86:C10D.</summary>
    internal const ushort TurretDown = 0xc10d;

    /// <summary>Direction-indexed turret-bullet selector at $86:C131.</summary>
    internal const ushort BulletSelector = 0xc131;

    /// <summary>Shared turret-bullet touch/shot smoke at $86:C19A.</summary>
    internal const ushort BulletTouchOrShot = 0xc19a;

    /// <summary>Left-facing bullet pose at $86:C143; eight poses occupy six bytes each.</summary>
    internal const ushort BulletLeft = 0xc143;
    public static int PresentationWordCount => 21;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(index < 8 ? TurretLeft + 6 * index + 2
            : index < 16 ? BulletLeft + 6 * (index - 8) + 2
            : BulletTouchOrShot + 6 + 4 * (index - 16));
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainRoomTurret or
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Mother Brain turret mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool TryRead(ushort address, out ushort value)
    {
        int poseOffset = address - TurretLeft;
        if (poseOffset < 0 || poseOffset >= 48)
            poseOffset = address - BulletLeft;
        if (poseOffset >= 0 && poseOffset < 48 && poseOffset % 6 is 0 or 4)
        {
            value = poseOffset % 6 == 0 ? (ushort)1 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep;
            return true;
        }
        int selectorOffset = address - BulletSelector;
        if (selectorOffset >= 0 && selectorOffset <= 16 && selectorOffset % 2 == 0)
        {
            value = selectorOffset == 0
                ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY
                : (ushort)(BulletLeft + 6 * (selectorOffset / 2 - 1));
            return true;
        }
        int smokeOffset = address - BulletTouchOrShot;
        if (smokeOffset is 0 or 2 or 24 || smokeOffset >= 4 && smokeOffset <= 20 && smokeOffset % 4 == 0)
        {
            value = smokeOffset switch
            {
                0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_UsePalette0,
                2 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction,
                20 => 32,
                24 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
                _ => 8,
            };
            return true;
        }
        value = 0;
        return false;
    }
}
