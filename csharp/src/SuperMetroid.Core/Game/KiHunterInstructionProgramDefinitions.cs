namespace SuperMetroid.Core.Game;

internal readonly record struct KiHunterInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for KiHunter body and wing instruction programs.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KiHunterInstructionProgramDefinitions
{
    /// <summary><c>InstList_Kihunter_Idling_FacingLeft</c> at $A8:E9FA.</summary>
    internal const ushort FlyingLeft = 0xe9fa;
    /// <summary><c>InstList_Kihunter_Swiping_FacingLeft</c> at $A8:EA08.</summary>
    internal const ushort SwoopLeft = 0xea08;
    /// <summary><c>InstList_Kihunter_Idling_FacingRight</c> at $A8:EA24.</summary>
    internal const ushort FlyingRight = 0xea24;
    /// <summary><c>InstList_Kihunter_Swiping_FacingRight</c> at $A8:EA32.</summary>
    internal const ushort SwoopRight = 0xea32;
    /// <summary><c>InstList_KihunterWings_FacingLeft</c> at $A8:EA4E.</summary>
    internal const ushort WingsLeft = 0xea4e;
    /// <summary><c>InstList_KihunterWings_FacingRight</c> at $A8:EA5E.</summary>
    internal const ushort WingsRight = 0xea5e;
    /// <summary><c>InstList_KihunterWings_Falling</c> at $A8:EA7E.</summary>
    internal const ushort DetachedWings = 0xea7e;
    /// <summary><c>InstList_Kihunter_Hop_FacingLeft</c> at $A8:EA8A.</summary>
    internal const ushort JumpLeft = 0xea8a;
    /// <summary><c>InstList_Kihunter_Hop_FacingRight</c> at $A8:EAA6.</summary>
    internal const ushort JumpRight = 0xeaa6;
    /// <summary><c>InstList_Kihunter_LandedFromHop_FacingLeft</c> at $A8:EAC2.</summary>
    internal const ushort LandLeft = 0xeac2;
    /// <summary><c>InstList_Kihunter_LandedFromHop_FacingRight</c> at $A8:EADA.</summary>
    internal const ushort LandRight = 0xeada;
    /// <summary><c>InstList_Kihunter_AcidSpitAttack_FacingLeft</c> at $A8:EAF2.</summary>
    internal const ushort SpitLeft = 0xeaf2;
    /// <summary><c>InstList_Kihunter_AcidSpitAttack_FacingRight</c> at $A8:EB10.</summary>
    internal const ushort SpitRight = 0xeb10;

    private static readonly KiHunterInstructionMechanicsWord[] Words =
    [
        new(0xe9fa, 2), new(0xe9fe, 2), new(0xea02, 1),
        new(0xea06, EnemyInstructionCodePointers.Instruction_Kihunter_SetIdlingInstListsFacingForwards),

        new(0xea08, 2), new(0xea0c, 6), new(0xea10, 2),
        new(0xea14, 2), new(0xea18, 2), new(0xea1c, 0x0020),
        new(0xea20, CommonEnemyInstructionCodes.Goto), new(0xea22, FlyingLeft),

        new(0xea24, 2), new(0xea28, 2), new(0xea2c, 1),
        new(0xea30, EnemyInstructionCodePointers.Instruction_Kihunter_SetIdlingInstListsFacingForwards),

        new(0xea32, 2), new(0xea36, 6), new(0xea3a, 2),
        new(0xea3e, 2), new(0xea42, 2), new(0xea46, 0x0020),
        new(0xea4a, CommonEnemyInstructionCodes.Goto), new(0xea4c, FlyingRight),

        new(0xea4e, 2), new(0xea52, 2), new(0xea56, 1),
        new(0xea5a, CommonEnemyInstructionCodes.Goto), new(0xea5c, WingsLeft),
        new(0xea5e, 2), new(0xea62, 2), new(0xea66, 1),
        new(0xea6a, CommonEnemyInstructionCodes.Goto), new(0xea6c, WingsRight),

        new(0xea7e, 1), new(0xea82, CommonEnemyInstructionCodes.Sleep),

        new(0xea8a, 8), new(0xea8e, 8), new(0xea92, 0x000b),
        new(0xea96, 2), new(0xea9a, 2),
        new(0xea9e, EnemyInstructionCodePointers.Instruction_Kihunter_SetFunctionToHop),
        new(0xeaa0, 1), new(0xeaa4, CommonEnemyInstructionCodes.Sleep),

        new(0xeaa6, 8), new(0xeaaa, 8), new(0xeaae, 0x000b),
        new(0xeab2, 2), new(0xeab6, 2),
        new(0xeaba, EnemyInstructionCodePointers.Instruction_Kihunter_SetFunctionToHop),
        new(0xeabc, 1), new(0xeac0, CommonEnemyInstructionCodes.Sleep),

        new(0xeac2, 8), new(0xeac6, 8), new(0xeaca, 0x000b), new(0xeace, 8),
        new(0xead2, EnemyInstructionCodePointers.Instruction_Kihunter_SetFunctionTo_Wingless_Thinking),
        new(0xead4, 1), new(0xead8, CommonEnemyInstructionCodes.Sleep),

        new(0xeada, 8), new(0xeade, 8), new(0xeae2, 0x000b), new(0xeae6, 8),
        new(0xeaea, EnemyInstructionCodePointers.Instruction_Kihunter_SetFunctionTo_Wingless_Thinking),
        new(0xeaec, 1), new(0xeaf0, CommonEnemyInstructionCodes.Sleep),

        new(0xeaf2, 0x0020), new(0xeaf6, 6), new(0xeafa, 0x0010), new(0xeafe, 2),
        new(0xeb02, EnemyInstructionCodePointers.Instruction_Kihunter_FireAcidSpitLeft),
        new(0xeb04, 0x0018),
        new(0xeb08, EnemyInstructionCodePointers.Instruction_Kihunter_SetFunctionTo_Wingless_Thinking),
        new(0xeb0a, 1), new(0xeb0e, CommonEnemyInstructionCodes.Sleep),

        new(0xeb10, 0x0020), new(0xeb14, 6), new(0xeb18, 0x0010), new(0xeb1c, 2),
        new(0xeb20, EnemyInstructionCodePointers.Instruction_Kihunter_FireAcidSpitRight),
        new(0xeb22, 0x0018),
        new(0xeb26, EnemyInstructionCodePointers.Instruction_Kihunter_SetFunctionTo_Wingless_Thinking),
        new(0xeb28, 1), new(0xeb2c, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xe9fc, 0xea00, 0xea04,
        0xea0a, 0xea0e, 0xea12, 0xea16, 0xea1a, 0xea1e,
        0xea26, 0xea2a, 0xea2e,
        0xea34, 0xea38, 0xea3c, 0xea40, 0xea44, 0xea48,
        0xea50, 0xea54, 0xea58,
        0xea60, 0xea64, 0xea68,
        0xea80,
        0xea8c, 0xea90, 0xea94, 0xea98, 0xea9c, 0xeaa2,
        0xeaa8, 0xeaac, 0xeab0, 0xeab4, 0xeab8, 0xeabe,
        0xeac4, 0xeac8, 0xeacc, 0xead0, 0xead6,
        0xeadc, 0xeae0, 0xeae4, 0xeae8, 0xeaee,
        0xeaf4, 0xeaf8, 0xeafc, 0xeb00, 0xeb06, 0xeb0c,
        0xeb12, 0xeb16, 0xeb1a, 0xeb1e, 0xeb24, 0xeb2a,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KiHunterInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            KiHunterInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"KiHunter instruction mechanics pointer $A8:{address:X4} is not compiled.");
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
}
