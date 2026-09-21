namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kago-bug projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct KagoBugProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Kago bug's landed, falling, jumping, and shot programs. The
/// shared initial Kraid-rock pose is owned by
/// <see cref="KraidRockProjectileInstructionProgramDefinitions"/>; interleaved
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KagoBugProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_KagoBug_HitFloor</c> at $86:D03C.</summary>
    internal const ushort Landed = 0xd03c;

    /// <summary><c>InstList_EnemyProjectile_KagoBug_Falling</c> at $86:D04A.</summary>
    internal const ushort Falling = 0xd04a;

    /// <summary><c>InstList_EnemyProjectile_KagoBug_Jump_0</c> at $86:D052.</summary>
    internal const ushort JumpStart = 0xd052;

    /// <summary><c>InstList_EnemyProjectile_KagoBug_Jump_1</c> at $86:D05C.</summary>
    internal const ushort JumpLoop = 0xd05c;

    /// <summary><c>InstList_EnemyProjectile_Shot_KagoBug</c> at $86:D064.</summary>
    internal const ushort Shot = 0xd064;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_KagoBug_StartJumping</c> at $86:D15C.
    /// </summary>
    internal const ushort StartJumpInstruction = 0xd15c;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_KagoBug_StartIdling</c> at $86:D1B6.
    /// </summary>
    internal const ushort StartIdleInstruction = 0xd1b6;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_UsePalette0_duplicate_again</c> at $86:D1C7.
    /// </summary>
    internal const ushort UsePaletteZeroInstruction = 0xd1c7;

    /// <summary>
    /// <c>PreInstruction_EnemyProjectile_KagoBug_SpawnDrop</c> used as an instruction
    /// callback at $86:D1CE.
    /// </summary>
    internal const ushort SpawnDropInstruction = 0xd1ce;

    private static readonly KagoBugProjectileInstructionMechanicsWord[] Words =
    [
        new(Landed, 0x0005),
        new(0xd040, StartIdleInstruction),
        new(0xd042, 0x7fff),
        new(0xd046, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xd048, Landed),
        new(Falling, 0x7fff),
        new(0xd04e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xd050, Falling),
        new(JumpStart, 0x0010),
        new(0xd056, 0x0005),
        new(0xd05a, StartJumpInstruction),
        new(JumpLoop, 0x7fff),
        new(0xd060, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xd062, JumpLoop),
        new(Shot, UsePaletteZeroInstruction),
        new(0xd066, 0x0004),
        new(0xd06a, 0x0004),
        new(0xd06e, 0x0004),
        new(0xd072, 0x0004),
        new(0xd076, 0x0004),
        new(0xd07a, SpawnDropInstruction),
        new(0xd07c, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0xd07e, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xd03e, 0xd044, 0xd04c, 0xd054, 0xd058, 0xd05e,
        0xd068, 0xd06c, 0xd070, 0xd074, 0xd078,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KagoBugProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            KagoBugProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Kago-bug projectile mechanics pointer $86:{address:X4} is not compiled.");
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
