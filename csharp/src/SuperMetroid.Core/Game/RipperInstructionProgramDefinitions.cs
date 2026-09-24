namespace SuperMetroid.Core.Game;

internal readonly record struct RipperInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the GRipper, Ripper II, and Ripper animation loops.
/// Interleaved spritemap operands are selected by the installed visual catalog.
/// </summary>
internal static class RipperInstructionProgramDefinitions
{
    /// <summary><c>InstList_GRipper_MovingLeft</c> at $A2:E19B.</summary>
    internal const ushort GRipperMovingLeft = 0xe19b;
    /// <summary><c>InstList_GRipper_MovingRight</c> at $A2:E1AF.</summary>
    internal const ushort GRipperMovingRight = 0xe1af;
    /// <summary><c>InstList_Ripper2_MovingRight</c> at $A2:E2E0.</summary>
    internal const ushort Ripper2MovingRight = 0xe2e0;
    /// <summary><c>InstList_Ripper2_MovingLeft</c> at $A2:E2F4.</summary>
    internal const ushort Ripper2MovingLeft = 0xe2f4;
    /// <summary><c>InstList_Ripper_MovingRight</c> at $A2:E477.</summary>
    internal const ushort RipperMovingRight = 0xe477;
    /// <summary><c>InstList_Ripper_MovingLeft</c> at $A2:E48B.</summary>
    internal const ushort RipperMovingLeft = 0xe48b;

    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingLeft</c> at $A2:E43F.</summary>
    internal const ushort FrozenFacingLeftSpritemap = 0xe43f;
    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingRight</c> at $A2:E44B.</summary>
    internal const ushort FrozenFacingRightSpritemap = 0xe44b;

    private static readonly RipperInstructionMechanicsWord[] Words =
    [
        new(0xe19b, 8), new(0xe19f, 7), new(0xe1a3, 8), new(0xe1a7, 7),
        new(0xe1ab, CommonEnemyInstructionCodes.Goto), new(0xe1ad, GRipperMovingLeft),
        new(0xe1af, 8), new(0xe1b3, 7), new(0xe1b7, 8), new(0xe1bb, 7),
        new(0xe1bf, CommonEnemyInstructionCodes.Goto), new(0xe1c1, GRipperMovingRight),
        new(0xe2e0, 8), new(0xe2e4, 7), new(0xe2e8, 8), new(0xe2ec, 7),
        new(0xe2f0, CommonEnemyInstructionCodes.Goto), new(0xe2f2, Ripper2MovingRight),
        new(0xe2f4, 8), new(0xe2f8, 7), new(0xe2fc, 8), new(0xe300, 7),
        new(0xe304, CommonEnemyInstructionCodes.Goto), new(0xe306, Ripper2MovingLeft),
        new(0xe477, 8), new(0xe47b, 7), new(0xe47f, 8), new(0xe483, 7),
        new(0xe487, CommonEnemyInstructionCodes.Goto), new(0xe489, RipperMovingRight),
        new(0xe48b, 8), new(0xe48f, 7), new(0xe493, 8), new(0xe497, 7),
        new(0xe49b, CommonEnemyInstructionCodes.Goto), new(0xe49d, RipperMovingLeft),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe19d, 0xe1a1, 0xe1a5, 0xe1a9,
        0xe1b1, 0xe1b5, 0xe1b9, 0xe1bd,
        0xe2e2, 0xe2e6, 0xe2ea, 0xe2ee,
        0xe2f6, 0xe2fa, 0xe2fe, 0xe302,
        0xe479, 0xe47d, 0xe481, 0xe485,
        0xe48d, 0xe491, 0xe495, 0xe499,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static RipperInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RipperInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ripper-family instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
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
