namespace SuperMetroid.Core.Game;

internal readonly record struct ShaktoolProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for the cartridge's three unused Shaktool attack-circle programs.
/// Their eight spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class ShaktoolProjectileInstructionProgramDefinitions
{
    /// <summary>Front attack-circle program at $86:BD68.</summary>
    internal const ushort Front = 0xbd68;

    /// <summary>Middle attack-circle program at $86:BD78.</summary>
    internal const ushort Middle = 0xbd78;

    /// <summary>Back attack-circle program at $86:BD8C.</summary>
    internal const ushort Back = 0xbd8c;

    private static readonly ShaktoolProjectileInstructionMechanicsWord[] Words =
    [
        new(Front, 0x0004),
        new(0xbd6c, 0x0004),
        new(0xbd70, 0x0077),
        new(0xbd74, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xbd76, 0xbd70),
        new(Middle, 0x0006),
        new(0xbd7c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xbd7e,
            EnemyProjectileCodePointers.PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving),
        new(0xbd80, 0x0004),
        new(0xbd84, 0x0077),
        new(0xbd88, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xbd8a, 0xbd84),
        new(Back, 0x000a),
        new(0xbd90, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xbd92,
            EnemyProjectileCodePointers.PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving),
        new(0xbd94, 0x0077),
        new(0xbd98, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xbd9a, 0xbd94),
    ];

    private static readonly ushort[] PresentationWords =
        [0xbd6a, 0xbd6e, 0xbd72, 0xbd7a, 0xbd82, 0xbd86, 0xbd8e, 0xbd96];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ShaktoolProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) =>
        kind is RoomEnemyProjectileKind.ShaktoolAttackFrontCircle or
            RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle or
            RoomEnemyProjectileKind.ShaktoolAttackBackCircle;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ShaktoolProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Shaktool attack-circle mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (ShaktoolProjectileInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address ||
                bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
