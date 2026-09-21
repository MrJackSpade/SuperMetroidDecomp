namespace SuperMetroid.Core.Game;

/// <summary>One compiled downward-gate projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct DownwardGateProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for both downward-gate actors. Spritemap operands remain live cartridge
/// presentation data; movement continues to use the shared translated pre-instruction.
/// </summary>
internal static class DownwardGateProjectileInstructionProgramDefinitions
{
    /// <summary>Downward-moving gate instruction list at $86:E53C.</summary>
    internal const ushort Moving = 0xe53c;

    /// <summary>Closed gate instruction list at $86:E55E.</summary>
    internal const ushort Closed = 0xe55e;

    /// <summary>Initial closed-gate sleep instruction at $86:E566.</summary>
    internal const ushort ClosedSleep = 0xe566;

    private static readonly DownwardGateProjectileInstructionMechanicsWord[] Words =
    [
        new(Moving, DownwardGateEnemyProjectileRomData.SetYVelocityInstruction),
        new(0xe53e, 0x0100),
        new(0xe540, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xe542, DownwardGateEnemyProjectileRomData.MovementPreInstruction),
        new(0xe544, 0x0001),
        new(0xe548, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe54a, 0x0001),
        new(0xe54e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe550, 0x0001),
        new(0xe554, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe556, 0x0001),
        new(0xe55a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe55c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(Closed, DownwardGateEnemyProjectileRomData.SetYVelocityInstruction),
        new(0xe560, 0xff00),
        new(0xe562, 0x0001),
        new(ClosedSleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe568, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0xe56a, DownwardGateEnemyProjectileRomData.MovementPreInstruction),
        new(0xe56c, 0x0001),
        new(0xe570, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe572, 0x0001),
        new(0xe576, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe578, 0x0001),
        new(0xe57c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe57e, 0x0001),
        new(0xe582, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        new(0xe584, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    private static readonly ushort[] PresentationWords =
        [0xe546, 0xe54c, 0xe552, 0xe558, 0xe564, 0xe56e, 0xe574, 0xe57a, 0xe580];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static DownwardGateProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.DownwardGateMoving or
        RoomEnemyProjectileKind.DownwardGateClosed;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            DownwardGateProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Downward-gate projectile mechanics pointer $86:{address:X4} is not compiled.");
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
