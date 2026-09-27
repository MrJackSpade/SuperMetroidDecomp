namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed words of the three Mother Brain head lists executed by the translated
/// rainbow/cutscene interpreter. Durations, visual selectors, opcode identities,
/// and operands are kept in native order; the interpreter still owns their effects.
/// </summary>
public static class MotherBrainHeadInstructionProgramDefinitions
{
    /// <summary>$A9:9CB9, phase-three neutral head list.</summary>
    public const ushort NeutralStart = 0x9cb9;

    /// <summary>$A9:9CE1, final accepted current pointer in the neutral list.</summary>
    public const ushort NeutralActiveEnd = 0x9ce1;

    /// <summary>$A9:9DB1, Baby attack / four-onion-ring head list.</summary>
    public const ushort BabyAttackStart = 0x9db1;

    /// <summary>$A9:9DF5, final accepted current pointer in the Baby attack list.</summary>
    public const ushort BabyAttackActiveEnd = 0x9df5;

    /// <summary>$A9:9F00, phase-three bomb head list.</summary>
    public const ushort BombStart = 0x9f00;

    /// <summary>$A9:9F32, final accepted current pointer in the bomb list.</summary>
    public const ushort BombActiveEnd = 0x9f32;

    // One following word is retained in each segment because a current pointer
    // admitted at its upper bound may still request its adjacent operand.
    private static ReadOnlySpan<ushort> NeutralWords =>
    [
        0x0004, 0xa69b, 0x0004, 0xa6d9, 0x0008, 0xa717, 0x0004, 0xa6d9,
        0x0004, 0xa69b, 0x0004, 0xa6d9, 0x0008, 0xa717, 0x0008, 0xa6d9,
        0x9d0d, 0x0004, 0xa69b, 0x9b0f, 0x9cb9, 0x0004,
    ];

    private static ReadOnlySpan<ushort> BabyAttackWords =>
    [
        0x9ea3, 0x9b20, 0x9e37, 0x9b0f, 0x9dc1, 0x9eb5, 0x9b20, 0x9e5b,
        0x0004, 0xa717, 0x0004, 0xa750, 0x9df7, 0x0008, 0xa789, 0x9e29,
        0x9b32, 0x0017, 0x0003, 0xa789, 0x9e29, 0x0003, 0xa789, 0x9e29,
        0x0003, 0xa789, 0x9e29, 0x0010, 0xa789, 0x0004, 0xa750, 0x0010,
        0xa717, 0x9b14, 0x9cb9, 0xaf5a,
    ];

    private static ReadOnlySpan<ushort> BombWords =>
    [
        0x0004, 0xa69b, 0x0004, 0xa6d9, 0x0008, 0xa717, 0x9b20, 0x0004,
        0xa717, 0x0004, 0xa750, 0x9b28, 0x006f, 0x0008, 0xa789, 0x9ebd,
        0x0001, 0x9b6d, 0x0020, 0xa789, 0x0004, 0xa750, 0x0010, 0xa717,
        0x9b14, 0x9cb9, 0x0010,
    ];

    /// <summary>The precise pointer windows executed by the translated head interpreter.</summary>
    public static bool IsActivePointer(ushort pointer) =>
        pointer is >= NeutralStart and <= NeutralActiveEnd or
            >= BabyAttackStart and <= BabyAttackActiveEnd or
            >= BombStart and <= BombActiveEnd;

    /// <summary>Reads one compiled native word, including the boundary operand word.</summary>
    public static ushort ReadWord(ushort pointer)
    {
        if (TryRead(NeutralStart, NeutralWords, pointer, out ushort value) ||
            TryRead(BabyAttackStart, BabyAttackWords, pointer, out value) ||
            TryRead(BombStart, BombWords, pointer, out value))
            return value;
        throw new InvalidDataException(
            $"Mother Brain head word $A9:{pointer:X4} is outside the compiled lists.");
    }

    private static bool TryRead(ushort start, ReadOnlySpan<ushort> words,
        ushort pointer, out ushort value)
    {
        int offset = pointer - start;
        if (offset >= 0 && (offset & 1) == 0 && offset / 2 < words.Length)
        {
            value = words[offset / 2];
            return true;
        }
        value = 0;
        return false;
    }
}
