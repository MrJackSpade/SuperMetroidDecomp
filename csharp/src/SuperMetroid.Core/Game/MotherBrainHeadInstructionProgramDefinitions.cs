namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed bank-$A9 words of Mother Brain's head instruction lists. Only the six
/// contiguous data regions identified by the native labels are stored here;
/// the CPU routines between them are dispatched as opcodes, not copied as data.
/// Frame durations, visual selectors, commands and operands retain native order.
/// </summary>
public static class MotherBrainHeadInstructionProgramDefinitions
{
    /// <summary>$A9:9B7F, stretching, recoil, initial, decapitated and drool lists.</summary>
    public const ushort EarlyStart = 0x9b7f;

    /// <summary>$A9:9C63, last head-list word before native code at $A9:9C65.</summary>
    public const ushort EarlyEnd = 0x9c63;

    /// <summary>$A9:9C77, rainbow-beam hold and phase-two neutral lists.</summary>
    public const ushort RainbowAndNeutralPhaseTwoStart = 0x9c77;

    /// <summary>$A9:9CAB, last phase-two neutral branch operand.</summary>
    public const ushort RainbowAndNeutralPhaseTwoEnd = 0x9cab;

    /// <summary>$A9:9CB9, phase-three neutral head list.</summary>
    public const ushort NeutralStart = 0x9cb9;

    /// <summary>$A9:9CE1, final current pointer in the dedicated neutral interpreter.</summary>
    public const ushort NeutralActiveEnd = 0x9ce1;

    /// <summary>$A9:9D0B, last unused phase-three neutral list word.</summary>
    public const ushort NeutralRegionEnd = 0x9d0b;

    /// <summary>$A9:9D25, corpse and phase-two/three onion-ring attack lists.</summary>
    public const ushort CorpseAndRingsStart = 0x9d25;

    /// <summary>$A9:9DB1, Baby-targeted four-ring head list.</summary>
    public const ushort BabyAttackStart = 0x9db1;

    /// <summary>$A9:9DF5, final current pointer in the dedicated Baby interpreter.</summary>
    public const ushort BabyAttackActiveEnd = 0x9df5;

    /// <summary>$A9:9DF5, last four-ring branch operand before native code.</summary>
    public const ushort CorpseAndRingsEnd = 0x9df5;

    /// <summary>$A9:9ECC, phase-two/three bomb and laser head lists.</summary>
    public const ushort BombAndLaserStart = 0x9ecc;

    /// <summary>$A9:9F00, phase-three bomb head list.</summary>
    public const ushort BombStart = 0x9f00;

    /// <summary>$A9:9F32, final current pointer in the dedicated bomb interpreter.</summary>
    public const ushort BombActiveEnd = 0x9f32;

    /// <summary>$A9:9F44, last laser-list branch operand before native code.</summary>
    public const ushort BombAndLaserEnd = 0x9f44;

    /// <summary>$A9:9F6C, rainbow-beam charging head list.</summary>
    public const ushort RainbowChargeStart = 0x9f6c;

    /// <summary>$A9:9F82, last charging-list loop operand before native code.</summary>
    public const ushort RainbowChargeEnd = 0x9f82;

    private static ReadOnlySpan<ushort> EarlyWords =>
    [
        0x9b77, 0x0002, 0xa5f8, 0x0002, 0xa62c, 0x9b3c, 0x0002, 0xa62c,
        0x9b6d, 0x9b28, 0x007e, 0x9b3c, 0x0010, 0xa660, 0x9b3c, 0x0010,
        0xa660, 0x9b3c, 0x0020, 0xa660, 0x0004, 0xa62c, 0x0001, 0xa5f8,
        0x9b0f, 0x9bab, 0x9b77, 0x0002, 0xa717, 0x0002, 0xa750, 0x9b3c,
        0x0002, 0xa750, 0x9b6d, 0x9b28, 0x007e, 0x9b3c, 0x0010, 0xa789,
        0x9b3c, 0x0010, 0xa789, 0x9b3c, 0x0020, 0xa789, 0x0004, 0xa750,
        0x0001, 0xa717, 0x9b0f, 0x9bdf, 0x9b77, 0x0002, 0xa717, 0x0002,
        0xa750, 0x0002, 0xa750, 0x9b6d, 0x9b28, 0x007e, 0x0010, 0xa789,
        0x0010, 0xa789, 0x0020, 0xa789, 0x0004, 0xa750, 0x0001, 0xa717,
        0x9b0f, 0x9c0b, 0x0000, 0xa320, 0x812f, 0x0008, 0xa5f8, 0x0004,
        0xa5bf, 0x0004, 0xa586, 0x9b0f, 0x9c21, 0x0008, 0xa717, 0x0004,
        0xa6d9, 0x0004, 0xa69b, 0x9b0f, 0x9c31, 0x9b77, 0x0004, 0xa717,
        0x0004, 0xa750, 0x9b28, 0x007e, 0x0002, 0xa789, 0x9b3c, 0x0002,
        0xa789, 0x9b3c, 0x0002, 0xa789, 0x9b3c, 0x0002, 0xa789, 0x9b3c,
        0x0002, 0xa789, 0x9c65,
    ];

    private static ReadOnlySpan<ushort> RainbowAndNeutralPhaseTwoWords =>
    [
        0x0001, 0xa5f8, 0x9b0f, 0x9c77, 0x0001, 0xa717, 0x9b0f, 0x9c7f,
        0x0004, 0xa586, 0x0004, 0xa5bf, 0x0008, 0xa5f8, 0x0004, 0xa5bf,
        0x0004, 0xa586, 0x0004, 0xa5bf, 0x0008, 0xa5f8, 0x9cad, 0x0004,
        0xa5bf, 0x9b0f, 0x9c87,
    ];

    private static ReadOnlySpan<ushort> NeutralWords =>
    [
        0x0004, 0xa69b, 0x0004, 0xa6d9, 0x0008, 0xa717, 0x0004, 0xa6d9,
        0x0004, 0xa69b, 0x0004, 0xa6d9, 0x0008, 0xa717, 0x0008, 0xa6d9,
        0x9d0d, 0x0004, 0xa69b, 0x9b0f, 0x9cb9, 0x0004, 0xa717, 0x0004,
        0xa750, 0x0002, 0xa789, 0x9b28, 0x006f, 0x0002, 0xa789, 0x0002,
        0xa789, 0x0002, 0xa789, 0x0002, 0xa789, 0x0004, 0xa750, 0x0004,
        0xa717, 0x9d21,
    ];

    private static ReadOnlySpan<ushort> CorpseAndRingsWords =>
    [
        0x0002, 0xa69b, 0x0002, 0xa6d9, 0x0040, 0xa717, 0x0040, 0xad3e,
        0x0002, 0xad6d, 0x9b0f, 0x9d35, 0x9b20, 0x0004, 0xa5f8, 0x0004,
        0xa62c, 0x9b28, 0x006f, 0x0008, 0xa660, 0x9e5b, 0x9e29, 0x9b32,
        0x0017, 0x0003, 0xa660, 0x9e5b, 0x9e29, 0x0003, 0xa660, 0x9e5b,
        0x9e29, 0x0003, 0xa660, 0x9e5b, 0x9e29, 0x0010, 0xa660, 0x0004,
        0xa62c, 0x0010, 0xa5f8, 0x9b14, 0x9c87, 0x9b20, 0x0004, 0xa5f8,
        0x0004, 0xa62c, 0x9b28, 0x006f, 0x0008, 0xa660, 0x9e5b, 0x9e29,
        0x9b32, 0x0017, 0x0003, 0xa660, 0x9e5b, 0x9e29, 0x0010, 0xa660,
        0x0004, 0xa62c, 0x0010, 0xa5f8, 0x9b14, 0x9c87, 0x9ea3, 0x9b20,
        0x9e37, 0x9b0f, 0x9dc1, 0x9eb5, 0x9b20, 0x9e5b, 0x0004, 0xa717,
        0x0004, 0xa750, 0x9df7, 0x0008, 0xa789, 0x9e29, 0x9b32, 0x0017,
        0x0003, 0xa789, 0x9e29, 0x0003, 0xa789, 0x9e29, 0x0003, 0xa789,
        0x9e29, 0x0010, 0xa789, 0x0004, 0xa750, 0x0010, 0xa717, 0x9b14,
        0x9cb9,
    ];

    private static ReadOnlySpan<ushort> BombAndLaserWords =>
    [
        0x0004, 0xa586, 0x0004, 0xa5bf, 0x0008, 0xa5f8, 0x9b20, 0x0004,
        0xa5f8, 0x0004, 0xa62c, 0x9b28, 0x006f, 0x0008, 0xa660, 0x9ebd,
        0x0007, 0x9b6d, 0x0020, 0xa660, 0x0004, 0xa62c, 0x0010, 0xa5f8,
        0x9b14, 0x9c87, 0x0004, 0xa69b, 0x0004, 0xa6d9, 0x0008, 0xa717,
        0x9b20, 0x0004, 0xa717, 0x0004, 0xa750, 0x9b28, 0x006f, 0x0008,
        0xa789, 0x9ebd, 0x0001, 0x9b6d, 0x0020, 0xa789, 0x0004, 0xa750,
        0x0010, 0xa717, 0x9b14, 0x9cb9, 0x0010, 0xa5bf, 0x0004, 0xa5f8,
        0x9f46, 0x0020, 0xa5f8, 0x9b14, 0x9c87,
    ];

    private static ReadOnlySpan<ushort> RainbowChargeWords =>
    [
        0x9f8e, 0x0004, 0xa5f8, 0x0004, 0xa5bf, 0x0002, 0xa586, 0x9f84,
        0x001e, 0xa586, 0x9b0f, 0x9f7a,
    ];

    /// <summary>The precise pointer windows executed by the dedicated head interpreter.</summary>
    public static bool IsActivePointer(ushort pointer) =>
        pointer is >= NeutralStart and <= NeutralActiveEnd or
            >= BabyAttackStart and <= BabyAttackActiveEnd or
            >= BombStart and <= BombActiveEnd;

    /// <summary>Whether a word belongs to one of the six compiled head-list regions.</summary>
    public static bool ContainsWord(ushort pointer) =>
        Contains(EarlyStart, EarlyWords, pointer) ||
        Contains(RainbowAndNeutralPhaseTwoStart, RainbowAndNeutralPhaseTwoWords, pointer) ||
        Contains(NeutralStart, NeutralWords, pointer) ||
        Contains(CorpseAndRingsStart, CorpseAndRingsWords, pointer) ||
        Contains(BombAndLaserStart, BombAndLaserWords, pointer) ||
        Contains(RainbowChargeStart, RainbowChargeWords, pointer);

    /// <summary>Reads one compiled native word from a head-list data region.</summary>
    public static ushort ReadWord(ushort pointer)
    {
        if (TryRead(EarlyStart, EarlyWords, pointer, out ushort value) ||
            TryRead(RainbowAndNeutralPhaseTwoStart, RainbowAndNeutralPhaseTwoWords,
                pointer, out value) ||
            TryRead(NeutralStart, NeutralWords, pointer, out value) ||
            TryRead(CorpseAndRingsStart, CorpseAndRingsWords, pointer, out value) ||
            TryRead(BombAndLaserStart, BombAndLaserWords, pointer, out value) ||
            TryRead(RainbowChargeStart, RainbowChargeWords, pointer, out value))
            return value;
        throw new InvalidDataException(
            $"Mother Brain head word $A9:{pointer:X4} is outside the compiled lists.");
    }

    private static bool TryRead(ushort start, ReadOnlySpan<ushort> words,
        ushort pointer, out ushort value)
    {
        if (Contains(start, words, pointer))
        {
            value = words[(pointer - start) / 2];
            return true;
        }
        value = 0;
        return false;
    }

    private static bool Contains(ushort start, ReadOnlySpan<ushort> words, ushort pointer)
    {
        int offset = pointer - start;
        return offset >= 0 && (offset & 1) == 0 && offset / 2 < words.Length;
    }
}
