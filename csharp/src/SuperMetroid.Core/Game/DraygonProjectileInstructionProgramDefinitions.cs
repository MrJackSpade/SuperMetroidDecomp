namespace SuperMetroid.Core.Game;

/// <summary>One compiled Draygon projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct DraygonProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Draygon goop and wall-turret projectile instruction lists.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class DraygonProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Touch</c> at $86:8C38.</summary>
    internal const ushort GoopTouch = 0x8c38;

    /// <summary><c>InstList_EnemyProjectile_DraygonGoop</c> at $86:8C3A.</summary>
    internal const ushort Goop = 0x8c3a;

    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Shot</c> at $86:8C58.</summary>
    internal const ushort GoopShot = 0x8c58;

    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_0</c> at $86:8CA4.</summary>
    internal const ushort WallTurretBloom = 0x8ca4;

    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_1</c> at $86:8CE6.</summary>
    internal const ushort WallTurretFlight = 0x8ce6;

    private static readonly DraygonProjectileInstructionMechanicsWord[] Words =
    [
        new(GoopTouch, EnemyProjectileCodePointers.Instruction_DraygonGoop_SamusCollision),
        new(Goop, 0x000a),
        new(0x8c3e, 0x000a),
        new(0x8c42, 0x000a),
        new(0x8c46, 0x000a),
        new(0x8c4a, 0x000a),
        new(0x8c4e, 0x000a),
        new(0x8c52, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8c54, Goop),
        new(0x8c56, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(GoopShot, 0x0008),
        new(0x8c5c, 0x0008),
        new(0x8c60, EnemyProjectileCodePointers.Instruction_SpawnEnemyDropsWithDraygonEyeChances),
        new(0x8c62, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8c64, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        new(0x8c66, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        new(WallTurretBloom, 0x0005),
        new(0x8ca8, 0x0004),
        new(0x8cac, 0x0003),
        new(0x8cb0, 0x0003),
        new(0x8cb4, 0x0003),
        new(0x8cb8, 0x0003),
        new(0x8cbc, 0x0004),
        new(0x8cc0, 0x0003),
        new(0x8cc4, 0x0002),
        new(0x8cc8, 0x0002),
        new(0x8ccc, 0x0002),
        new(0x8cd0, 0x0002),
        new(0x8cd4, 0x000a),
        new(0x8cd8, 0x000a),
        new(0x8cdc, 0x000a),
        new(0x8ce0, 0x000a),
        new(0x8ce4,
            EnemyProjectileCodePointers.Instruction_SetPreInst_DraygonsWallTurretProjectile_Fired),
        new(WallTurretFlight, 0x0008),
        new(0x8cea, 0x0008),
        new(0x8cee, 0x0008),
        new(0x8cf2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x8cf4, WallTurretFlight),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0x8c3c, 0x8c40, 0x8c44, 0x8c48, 0x8c4c, 0x8c50,
        0x8c5a, 0x8c5e,
        0x8ca6, 0x8caa, 0x8cae, 0x8cb2, 0x8cb6, 0x8cba, 0x8cbe,
        0x8cc2, 0x8cc6, 0x8cca, 0x8cce, 0x8cd2, 0x8cd6, 0x8cda, 0x8cde,
        0x8ce2, 0x8ce8, 0x8cec, 0x8cf0,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static DraygonProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.DraygonGoop or
        RoomEnemyProjectileKind.DraygonWallTurret;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            DraygonProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Draygon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
