namespace SuperMetroid.Core.Game;

/// <summary>One compiled Yapping Maw mechanics word at its bank-$A8 address.</summary>
internal readonly record struct YappingMawInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, callback, and loop control for Yapping Maw's attack and cooldown
/// programs. Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class YappingMawInstructionProgramDefinitions
{
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUp</c> at $A8:9F6F.</summary>
    internal const ushort AttackingFacingUp = 0x9f6f;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUpRight</c> at $A8:9F85.</summary>
    internal const ushort AttackingFacingUpRight = 0x9f85;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingRight</c> at $A8:9F9B.</summary>
    internal const ushort AttackingFacingRight = 0x9f9b;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDownRight</c> at $A8:9FB1.</summary>
    internal const ushort AttackingFacingDownRight = 0x9fb1;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDown</c> at $A8:9FC7.</summary>
    internal const ushort AttackingFacingDown = 0x9fc7;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDownLeft</c> at $A8:9FDD.</summary>
    internal const ushort AttackingFacingDownLeft = 0x9fdd;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingLeft</c> at $A8:9FF3.</summary>
    internal const ushort AttackingFacingLeft = 0x9ff3;
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUpLeft</c> at $A8:A009.</summary>
    internal const ushort AttackingFacingUpLeft = 0xa009;

    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUpRight</c> at $A8:A01F.</summary>
    internal const ushort CooldownFacingUpRight = 0xa01f;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUp</c> at $A8:A025.</summary>
    internal const ushort CooldownFacingUp = 0xa025;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUpLeft</c> at $A8:A03D.</summary>
    internal const ushort CooldownFacingUpLeft = 0xa03d;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDownRight</c> at $A8:A05B.</summary>
    internal const ushort CooldownFacingDownRight = 0xa05b;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDown</c> at $A8:A061.</summary>
    internal const ushort CooldownFacingDown = 0xa061;
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDownLeft</c> at $A8:A079.</summary>
    internal const ushort CooldownFacingDownLeft = 0xa079;

    /// <summary><c>InstListPointers_YappingMaw</c>, adjacent selector data at $A8:A097.</summary>
    internal const ushort AdjacentAttackSelectorTable = 0xa097;

    private static readonly YappingMawInstructionMechanicsWord[] Words =
        BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static YappingMawInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns the attack program selected by the native clockwise octant.</summary>
    internal static ushort AttackForDirection(int direction) => direction switch
    {
        0 => AttackingFacingUp,
        1 => AttackingFacingUpRight,
        2 => AttackingFacingRight,
        3 => AttackingFacingDownRight,
        4 => AttackingFacingDown,
        5 => AttackingFacingDownLeft,
        6 => AttackingFacingLeft,
        7 => AttackingFacingUpLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction,
            "Yapping Maw direction must be zero through seven."),
    };

    /// <summary>Returns fixed Yapping Maw control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            YappingMawInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Yapping Maw instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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

    private static YappingMawInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<YappingMawInstructionMechanicsWord>(capacity: 96);
        for (int direction = 0; direction < 8; direction++)
            AddAttackLoop(words, AttackForDirection(direction));

        AddCallback(words, CooldownFacingUpRight,
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUpRight);
        AddFrame(words, unchecked((ushort)(CooldownFacingUpRight + 2)), duration: 4);
        AddCallback(words, CooldownFacingUp,
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUp);
        AddCooldownLoop(words, unchecked((ushort)(CooldownFacingUp + 2)));

        AddCallback(words, CooldownFacingUpLeft,
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUpLeft);
        AddFrame(words, unchecked((ushort)(CooldownFacingUpLeft + 2)), duration: 4);
        AddCallback(words, unchecked((ushort)(CooldownFacingUpLeft + 6)),
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusUp);
        AddCooldownLoop(words, unchecked((ushort)(CooldownFacingUpLeft + 8)));

        AddCallback(words, CooldownFacingDownRight,
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDownRight);
        AddFrame(words, unchecked((ushort)(CooldownFacingDownRight + 2)), duration: 4);
        AddCallback(words, CooldownFacingDown,
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDown);
        AddCooldownLoop(words, unchecked((ushort)(CooldownFacingDown + 2)));

        AddCallback(words, CooldownFacingDownLeft,
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDownLeft);
        AddFrame(words, unchecked((ushort)(CooldownFacingDownLeft + 2)), duration: 4);
        AddCallback(words, unchecked((ushort)(CooldownFacingDownLeft + 6)),
            EnemyInstructionCodePointers.Instruction_YappingMaw_OffsetSamusDown);
        AddCooldownLoop(words, unchecked((ushort)(CooldownFacingDownLeft + 8)));
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 52);
        for (int direction = 0; direction < 8; direction++)
        {
            ushort entry = AttackForDirection(direction);
            words.Add(unchecked((ushort)(entry + 2)));
            words.Add(unchecked((ushort)(entry + 6)));
            words.Add(unchecked((ushort)(entry + 12)));
            words.Add(unchecked((ushort)(entry + 16)));
        }

        AddCooldownPresentation(words, unchecked((ushort)(CooldownFacingUpRight + 2)));
        AddCooldownLoopPresentation(words, unchecked((ushort)(CooldownFacingUp + 2)));
        AddCooldownPresentation(words, unchecked((ushort)(CooldownFacingUpLeft + 2)));
        AddCooldownLoopPresentation(words, unchecked((ushort)(CooldownFacingUpLeft + 8)));
        AddCooldownPresentation(words, unchecked((ushort)(CooldownFacingDownRight + 2)));
        AddCooldownLoopPresentation(words, unchecked((ushort)(CooldownFacingDown + 2)));
        AddCooldownPresentation(words, unchecked((ushort)(CooldownFacingDownLeft + 2)));
        AddCooldownLoopPresentation(words, unchecked((ushort)(CooldownFacingDownLeft + 8)));
        return words.ToArray();
    }

    private static void AddAttackLoop(
        List<YappingMawInstructionMechanicsWord> words,
        ushort entry)
    {
        AddFrame(words, entry, duration: 5);
        AddFrame(words, unchecked((ushort)(entry + 4)), duration: 3);
        AddCallback(words, unchecked((ushort)(entry + 8)),
            EnemyInstructionCodePointers.Instruction_YappingMaw_QueueSFXIfOnScreen);
        AddFrame(words, unchecked((ushort)(entry + 10)), duration: 80);
        AddFrame(words, unchecked((ushort)(entry + 14)), duration: 3);
        words.Add(new(unchecked((ushort)(entry + 18)), CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(entry + 20)), entry));
    }

    private static void AddCooldownLoop(
        List<YappingMawInstructionMechanicsWord> words,
        ushort entry)
    {
        AddFrame(words, entry, duration: 80);
        AddFrame(words, unchecked((ushort)(entry + 4)), duration: 3);
        AddFrame(words, unchecked((ushort)(entry + 8)), duration: 5);
        AddFrame(words, unchecked((ushort)(entry + 12)), duration: 3);
        AddCallback(words, unchecked((ushort)(entry + 16)),
            EnemyInstructionCodePointers.Instruction_YappingMaw_QueueSFXIfOnScreen);
        words.Add(new(unchecked((ushort)(entry + 18)), CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(entry + 20)), entry));
    }

    private static void AddFrame(
        List<YappingMawInstructionMechanicsWord> words,
        ushort address,
        ushort duration) => words.Add(new(address, duration));

    private static void AddCallback(
        List<YappingMawInstructionMechanicsWord> words,
        ushort address,
        ushort callback) => words.Add(new(address, callback));

    private static void AddCooldownPresentation(List<ushort> words, ushort frame) =>
        words.Add(unchecked((ushort)(frame + 2)));

    private static void AddCooldownLoopPresentation(List<ushort> words, ushort entry)
    {
        words.Add(unchecked((ushort)(entry + 2)));
        words.Add(unchecked((ushort)(entry + 6)));
        words.Add(unchecked((ushort)(entry + 10)));
        words.Add(unchecked((ushort)(entry + 14)));
    }
}
