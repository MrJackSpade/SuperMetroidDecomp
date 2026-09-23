namespace SuperMetroid.Core.Game;

internal readonly record struct CeresSteamInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the four directional Ceres steam programs.
/// Extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CeresSteamInstructionProgramDefinitions
{
    /// <summary>
    /// <c>InstList_CeresSteam_Up_0</c> at $A6:F04D. Its 17 mechanics words
    /// through $F07F match the pinned NTSC J/U v1.0 ROM. The bounded program
    /// hides steam, shows its first frame for one tick, then branches back to
    /// $F04D while the activation timer remains nonzero or to $F061 when it
    /// expires. The four directional programs share this control template;
    /// presentation pointers are separate live cartridge operands.
    /// </summary>
    internal const ushort Up = 0xf04d;
    /// <summary>
    /// <c>InstList_CeresSteam_Up_1</c> at $A6:F059 hides steam for $40 ticks,
    /// then makes it tangible and visible before the active frames resume.
    /// </summary>
    internal const ushort UpHiddenHold = 0xf059;
    /// <summary>
    /// <c>InstList_CeresSteam_Up_2</c> at $A6:F061 runs seven three-tick
    /// frames and returns to the hidden hold at $F059. The interpreter thus
    /// traverses only the three local states $F04D, $F059, and $F061.
    /// </summary>
    internal const ushort UpActive = 0xf061;
    /// <summary>
    /// <c>InstList_CeresSteam_Left_0</c> at $A6:F081, selected by both left
    /// variants 1 and 4. All 17 mechanics addresses are the upward program's
    /// addresses plus $34. The pinned ROM preserves 14 operand values exactly
    /// and relocates only its three local branch targets by $34, with zero
    /// mismatches. It therefore has the same bounded activation state graph.
    /// </summary>
    internal const ushort Left = 0xf081;
    /// <summary>
    /// <c>InstList_CeresSteam_Left_1</c> at $A6:F08D; the relocated $40-tick
    /// hidden hold returns to the seven-frame active state.
    /// </summary>
    internal const ushort LeftHiddenHold = 0xf08d;
    /// <summary>
    /// <c>InstList_CeresSteam_Left_2</c> at $A6:F095; seven three-tick frames
    /// loop to the local hidden hold at $F08D.
    /// </summary>
    internal const ushort LeftActive = 0xf095;
    /// <summary>
    /// <c>InstList_CeresSteam_Down_0</c> at $A6:F0B5, selected by variant 2.
    /// All 17 mechanics addresses are the upward program's addresses plus
    /// $68. Direct pinned-ROM comparison preserves 14 values and relocates
    /// only the three local branch targets by $68, with zero mismatches.
    /// The bounded activation graph is consequently the same as upward.
    /// </summary>
    internal const ushort Down = 0xf0b5;
    /// <summary>
    /// <c>InstList_CeresSteam_Down_1</c> at $A6:F0C1; its $40-tick hidden
    /// hold returns to the local active state.
    /// </summary>
    internal const ushort DownHiddenHold = 0xf0c1;
    /// <summary>
    /// <c>InstList_CeresSteam_Down_2</c> at $A6:F0C9; seven three-tick
    /// frames loop to the hidden hold at $F0C1.
    /// </summary>
    internal const ushort DownActive = 0xf0c9;
    /// <summary><c>InstList_CeresSteam_Right_0</c> at $A6:F0E9.</summary>
    internal const ushort Right = 0xf0e9;
    /// <summary><c>InstList_CeresSteam_Right_1</c> at $A6:F0F5.</summary>
    internal const ushort RightHiddenHold = 0xf0f5;
    /// <summary><c>InstList_CeresSteam_Right_2</c> at $A6:F0FD.</summary>
    internal const ushort RightActive = 0xf0fd;

    private static readonly CeresSteamInstructionMechanicsWord[] Words =
    [
        new(0xf04d, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf04f, 1),
        new(0xf053, CeresEnemyCodePointers.StepCeresSteamActivationTimer),
        new(0xf055, Up),
        new(0xf057, UpActive),
        new(0xf059, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf05b, 0x0040),
        new(0xf05f, EnemyInstructionCodePointers.Instruction_CeresSteam_SetToTangibleAndVisible),
        new(0xf061, 3), new(0xf065, 3), new(0xf069, 3), new(0xf06d, 3),
        new(0xf071, 3), new(0xf075, 3), new(0xf079, 3),
        new(0xf07d, CommonEnemyInstructionCodes.Goto),
        new(0xf07f, UpHiddenHold),

        new(0xf081, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf083, 1),
        new(0xf087, CeresEnemyCodePointers.StepCeresSteamActivationTimer),
        new(0xf089, Left),
        new(0xf08b, LeftActive),
        new(0xf08d, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf08f, 0x0040),
        new(0xf093, EnemyInstructionCodePointers.Instruction_CeresSteam_SetToTangibleAndVisible),
        new(0xf095, 3), new(0xf099, 3), new(0xf09d, 3), new(0xf0a1, 3),
        new(0xf0a5, 3), new(0xf0a9, 3), new(0xf0ad, 3),
        new(0xf0b1, CommonEnemyInstructionCodes.Goto),
        new(0xf0b3, LeftHiddenHold),

        new(0xf0b5, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf0b7, 1),
        new(0xf0bb, CeresEnemyCodePointers.StepCeresSteamActivationTimer),
        new(0xf0bd, Down),
        new(0xf0bf, DownActive),
        new(0xf0c1, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf0c3, 0x0040),
        new(0xf0c7, EnemyInstructionCodePointers.Instruction_CeresSteam_SetToTangibleAndVisible),
        new(0xf0c9, 3), new(0xf0cd, 3), new(0xf0d1, 3), new(0xf0d5, 3),
        new(0xf0d9, 3), new(0xf0dd, 3), new(0xf0e1, 3),
        new(0xf0e5, CommonEnemyInstructionCodes.Goto),
        new(0xf0e7, DownHiddenHold),

        new(0xf0e9, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf0eb, 1),
        new(0xf0ef, CeresEnemyCodePointers.StepCeresSteamActivationTimer),
        new(0xf0f1, Right),
        new(0xf0f3, RightActive),
        new(0xf0f5, CeresEnemyCodePointers.HideCeresSteam),
        new(0xf0f7, 0x0040),
        new(0xf0fb, EnemyInstructionCodePointers.Instruction_CeresSteam_SetToTangibleAndVisible),
        new(0xf0fd, 3), new(0xf101, 3), new(0xf105, 3), new(0xf109, 3),
        new(0xf10d, 3), new(0xf111, 3), new(0xf115, 3),
        new(0xf119, CommonEnemyInstructionCodes.Goto),
        new(0xf11b, RightHiddenHold),
    ];

    /// <summary>
    /// Addresses of live extended-spritemap operands, separate from compiled
    /// mechanics. For the upward program, the operands at $F051 and $F05D
    /// reuse $F142. Its seven active operands at <c>$F063 + 4 * frame</c>,
    /// for frames 0..6, contain <c>$F142 + $0A * frame</c>. All nine words
    /// match the pinned NTSC J/U v1.0 ROM. The stride is the size of each
    /// extended-spritemap record; the authored payloads remain cartridge data.
    /// For the leftward program, $F085 and $F091 repeat $F188; its active
    /// operands at <c>$F097 + 4 * frame</c> contain
    /// <c>$F188 + $0A * frame</c> for frames 0..6. All nine pinned-ROM words
    /// match, and the interpreter reads only these reachable frame operands.
    /// For the downward program, $F0B9 and $F0C5 repeat $F1CE; the seven
    /// active operands at <c>$F0CB + 4 * frame</c> contain
    /// <c>$F1CE + $0A * frame</c> for frames 0..6. All nine pinned-ROM words
    /// match the bounded seven-record sequence.
    /// </summary>
    private static readonly ushort[] PresentationWords =
    [
        0xf051, 0xf05d, 0xf063, 0xf067, 0xf06b, 0xf06f, 0xf073, 0xf077, 0xf07b,
        0xf085, 0xf091, 0xf097, 0xf09b, 0xf09f, 0xf0a3, 0xf0a7, 0xf0ab, 0xf0af,
        0xf0b9, 0xf0c5, 0xf0cb, 0xf0cf, 0xf0d3, 0xf0d7, 0xf0db, 0xf0df, 0xf0e3,
        0xf0ed, 0xf0f9, 0xf0ff, 0xf103, 0xf107, 0xf10b, 0xf10f, 0xf113, 0xf117,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CeresSteamInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresSteamInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres steam instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
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
