namespace SuperMetroid.Core.Game;

/// <summary>One compiled Mother Brain turret mechanics word at its bank-$86 address.</summary>
internal readonly record struct MotherBrainTurretInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Mother Brain's eight turret poses, direction-selected bullets, and
/// shared bullet touch/shot smoke. Spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MotherBrainTurretInstructionProgramDefinitions
{
    /// <summary>Left-facing turret pose at $86:C101.</summary>
    internal const ushort TurretLeft = 0xc101;

    /// <summary>Down-left-facing turret pose at $86:C107.</summary>
    internal const ushort TurretDownLeft = 0xc107;

    /// <summary>Down-facing turret pose at $86:C10D.</summary>
    internal const ushort TurretDown = 0xc10d;

    /// <summary>Down-right-facing turret pose at $86:C113.</summary>
    internal const ushort TurretDownRight = 0xc113;

    /// <summary>Right-facing turret pose at $86:C119.</summary>
    internal const ushort TurretRight = 0xc119;

    /// <summary>Up-right-facing turret pose at $86:C11F.</summary>
    internal const ushort TurretUpRight = 0xc11f;

    /// <summary>Up-facing turret pose at $86:C125.</summary>
    internal const ushort TurretUp = 0xc125;

    /// <summary>Up-left-facing turret pose at $86:C12B.</summary>
    internal const ushort TurretUpLeft = 0xc12b;

    /// <summary>Direction-indexed turret-bullet selector at $86:C131.</summary>
    internal const ushort BulletSelector = 0xc131;

    /// <summary>Shared turret-bullet touch/shot smoke at $86:C19A.</summary>
    internal const ushort BulletTouchOrShot = 0xc19a;

    private static readonly ushort[] TurretPrograms =
    [
        TurretLeft, TurretDownLeft, TurretDown, TurretDownRight,
        TurretRight, TurretUpRight, TurretUp, TurretUpLeft,
    ];

    private static readonly ushort[] BulletPrograms =
        [0xc143, 0xc149, 0xc14f, 0xc155, 0xc15b, 0xc161, 0xc167, 0xc16d];

    private static readonly MotherBrainTurretInstructionMechanicsWord[] Words = BuildWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static int DirectionCount => TurretPrograms.Length;
    internal static MotherBrainTurretInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort TurretProgram(MotherBrainTurretDirection direction) =>
        DirectionProgram(TurretPrograms, direction);

    internal static ushort BulletProgram(MotherBrainTurretDirection direction) =>
        DirectionProgram(BulletPrograms, direction);

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainRoomTurret or
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MotherBrainTurretInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Mother Brain turret mechanics pointer $86:{address:X4} is not compiled.");
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

    private static ushort DirectionProgram(
        ushort[] programs,
        MotherBrainTurretDirection direction)
    {
        int index = (byte)direction;
        if ((uint)index >= programs.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction,
                "Mother Brain turret direction must be zero through seven.");
        }

        return programs[index];
    }

    private static MotherBrainTurretInstructionMechanicsWord[] BuildWords()
    {
        var words = new List<MotherBrainTurretInstructionMechanicsWord>(49);
        foreach (ushort program in TurretPrograms)
            AddPose(words, program);

        Add(words, BulletSelector,
            EnemyProjectileCodePointers
                .Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY);
        for (int index = 0; index < BulletPrograms.Length; index++)
            Add(words, BulletSelector + 2 + index * 2, BulletPrograms[index]);
        foreach (ushort program in BulletPrograms)
            AddPose(words, program);

        Add(words, BulletTouchOrShot,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_UsePalette0);
        Add(words, BulletTouchOrShot + 2,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction);
        AddTimedFrames(words, BulletTouchOrShot + 4, [8, 8, 8, 8, 32]);
        Add(words, BulletTouchOrShot + 0x18,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(21);
        foreach (ushort program in TurretPrograms)
            words.Add(unchecked((ushort)(program + 2)));
        foreach (ushort program in BulletPrograms)
            words.Add(unchecked((ushort)(program + 2)));
        AddPresentationFrames(words, BulletTouchOrShot + 4, 5);
        return words.ToArray();
    }

    private static void AddPose(
        List<MotherBrainTurretInstructionMechanicsWord> words,
        ushort program)
    {
        Add(words, program, 1);
        Add(words, program + 4,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
    }

    private static void AddTimedFrames(
        List<MotherBrainTurretInstructionMechanicsWord> words,
        int start,
        ReadOnlySpan<ushort> durations)
    {
        for (int index = 0; index < durations.Length; index++)
            Add(words, start + index * 4, durations[index]);
    }

    private static void AddPresentationFrames(List<ushort> words, int start, int count)
    {
        for (int index = 0; index < count; index++)
            words.Add(unchecked((ushort)(start + index * 4 + 2)));
    }

    private static void Add(
        List<MotherBrainTurretInstructionMechanicsWord> words,
        int address,
        ushort value) => words.Add(new(unchecked((ushort)address), value));
}
