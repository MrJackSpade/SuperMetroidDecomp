namespace SuperMetroid.Core.Game;

/// <summary>One compiled Phantoon instruction mechanics word at its bank-$A7 address.</summary>
internal readonly record struct PhantoonInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, control flow, and callback operands for Phantoon's four enemy
/// records. Extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class PhantoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Phantoon_Body_Invulnerable</c> at $A7:CC41.</summary>
    public const ushort InvulnerableBody = 0xcc41;
    /// <summary><c>InstList_Phantoon_Body_FullHitbox</c> at $A7:CC47.</summary>
    public const ushort FullHitboxBody = 0xcc47;
    /// <summary><c>InstList_Phantoon_Body_EyeHitboxOnly</c> at $A7:CC4D.</summary>
    public const ushort EyeHitboxBody = 0xcc4d;
    /// <summary><c>InstList_Phantoon_Eye_Open</c> at $A7:CC53.</summary>
    public const ushort EyeOpen = 0xcc53;
    /// <summary><c>InstList_Phantoon_Eye_Closed</c> at $A7:CC7B.</summary>
    public const ushort EyeClosed = 0xcc7b;
    /// <summary><c>InstList_Phantoon_Eye_Close_PickNewPattern</c> at $A7:CC81.</summary>
    public const ushort EyeCloseAndPickNewPattern = 0xcc81;
    /// <summary><c>InstList_Phantoon_Eye_Close</c> at $A7:CC91.</summary>
    public const ushort EyeClose = 0xcc91;
    /// <summary><c>InstList_Phantoon_Eyeball_Centered</c> at $A7:CC9D.</summary>
    public const ushort EyeballCentered = 0xcc9d;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingUp</c> at $A7:CCA7.</summary>
    public const ushort EyeLookingUp = 0xcca7;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingUpRight</c> at $A7:CCAD.</summary>
    public const ushort EyeLookingUpRight = 0xccad;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingRight</c> at $A7:CCB3.</summary>
    public const ushort EyeLookingRight = 0xccb3;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingDownRight</c> at $A7:CCB9.</summary>
    public const ushort EyeLookingDownRight = 0xccb9;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingDown</c> at $A7:CCBF.</summary>
    public const ushort EyeLookingDown = 0xccbf;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingDownLeft</c> at $A7:CCC5.</summary>
    public const ushort EyeLookingDownLeft = 0xccc5;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingLeft</c> at $A7:CCCB.</summary>
    public const ushort EyeLookingLeft = 0xcccb;
    /// <summary><c>InstList_Phantoon_Eyeball_LookingUpLeft</c> at $A7:CCD1.</summary>
    public const ushort EyeLookingUpLeft = 0xccd1;
    /// <summary><c>InstList_Phantoon_Tentacles</c> at $A7:CCD7.</summary>
    public const ushort InitialTentacles = 0xccd7;
    /// <summary><c>InstList_Phantoon_Mouth_SpawnFlame</c> at $A7:CCEB.</summary>
    public const ushort MouthFollowUp = 0xcceb;
    /// <summary><c>InstList_Phantoon_Mouth_Initial</c> at $A7:CCF7.</summary>
    public const ushort InitialMouth = 0xccf7;
    /// <summary>First casual-flame timer word following the instruction block at $A7:CCFD.</summary>
    internal const ushort AdjacentCasualFlameTimers = 0xccfd;

    private static readonly PhantoonInstructionMechanicsWord[] Words =
    [
        new(0xcc41, 1), new(0xcc45, CommonEnemyInstructionCodes.Sleep),
        new(0xcc47, 1), new(0xcc4b, CommonEnemyInstructionCodes.Sleep),
        new(0xcc4d, 1), new(0xcc51, CommonEnemyInstructionCodes.Sleep),
        new(0xcc53, 10), new(0xcc57, 10), new(0xcc5b, 1),
        new(0xcc5f, EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY),
        new(0xcc61, PhantoonInstructionCodes.PlayPhantoonMaterializationSFX),
        new(0xcc63, EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY),
        new(0xcc65, PhantoonInstructionCodes.SetupEyeOpenPhantoonState),
        new(0xcc67, CommonEnemyInstructionCodes.Sleep),
        new(0xcc7b, 1), new(0xcc7f, CommonEnemyInstructionCodes.Sleep),
        new(0xcc81, 1), new(0xcc85, 10),
        new(0xcc89, EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY),
        new(0xcc8b, PhantoonInstructionCodes.PickNewPhantoonPattern),
        new(0xcc8d, CommonEnemyInstructionCodes.Goto), new(0xcc8f, EyeClosed),
        new(0xcc91, 1), new(0xcc95, 10),
        new(0xcc99, CommonEnemyInstructionCodes.Goto), new(0xcc9b, EyeClosed),
        new(0xcc9d, 1),
        new(0xcca1, EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY),
        new(0xcca3, PhantoonInstructionCodes.PlayPhantoonMaterializationSFX),
        new(0xcca5, CommonEnemyInstructionCodes.Sleep),
        new(0xcca7, 1), new(0xccab, CommonEnemyInstructionCodes.Sleep),
        new(0xccad, 1), new(0xccb1, CommonEnemyInstructionCodes.Sleep),
        new(0xccb3, 1), new(0xccb7, CommonEnemyInstructionCodes.Sleep),
        new(0xccb9, 1), new(0xccbd, CommonEnemyInstructionCodes.Sleep),
        new(0xccbf, 1), new(0xccc3, CommonEnemyInstructionCodes.Sleep),
        new(0xccc5, 1), new(0xccc9, CommonEnemyInstructionCodes.Sleep),
        new(0xcccb, 1), new(0xcccf, CommonEnemyInstructionCodes.Sleep),
        new(0xccd1, 1), new(0xccd5, CommonEnemyInstructionCodes.Sleep),
        new(0xccd7, 8), new(0xccdb, 8), new(0xccdf, 8), new(0xcce3, 8),
        new(0xcce7, CommonEnemyInstructionCodes.Goto), new(0xcce9, InitialTentacles),
        new(0xcceb, 5), new(0xccef, 5),
        new(0xccf3, EnemyInstructionCodePointers.Instruction_CommonA7_CallFunctionInY),
        new(0xccf5, PhantoonInstructionCodes.SpawnCasualFlame),
        new(0xccf7, 1), new(0xccfb, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcc43, 0xcc49, 0xcc4f,
        0xcc55, 0xcc59, 0xcc5d,
        0xcc7d, 0xcc83, 0xcc87, 0xcc93, 0xcc97, 0xcc9f,
        0xcca9, 0xccaf, 0xccb5, 0xccbb, 0xccc1, 0xccc7, 0xcccd, 0xccd3,
        0xccd9, 0xccdd, 0xcce1, 0xcce5,
        0xcced, 0xccf1, 0xccf9,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static PhantoonInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Phantoon control data or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            PhantoonInstructionMechanicsWord candidate = Words[middle];
            if (address == candidate.Address)
                return candidate.Value;
            if (address < candidate.Address)
                high = middle - 1;
            else
                low = middle + 1;
        }

        throw new InvalidDataException(
            $"Phantoon instruction mechanics pointer $A7:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
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
