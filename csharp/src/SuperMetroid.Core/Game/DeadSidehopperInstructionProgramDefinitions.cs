namespace SuperMetroid.Core.Game;

internal readonly record struct DeadSidehopperInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the sidehopper corpse's hopping, idle, and
/// corpse programs. Their eleven spritemap operands remain live cartridge data.
/// </summary>
internal static class DeadSidehopperInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseSidehopper_Alive_Hopping</c> at $A9:ECAC.</summary>
    internal const ushort AliveHopping = 0xecac;

    /// <summary><c>InstList_CorpseSidehopper_Alive_Idle</c> at $A9:ECE3.</summary>
    internal const ushort AliveIdle = 0xece3;

    /// <summary><c>InstList_CorpseSidehopper_Alive_Corpse</c> at $A9:ECE9.</summary>
    internal const ushort AliveCorpse = 0xece9;

    /// <summary><c>InstList_CorpseSidehopper_Alive_Dead</c> at $A9:ECEF.</summary>
    internal const ushort InitiallyDead = 0xecef;

    /// <summary>The embedded <c>Instruction_SidehopperCorpse_EndHop</c> word at $A9:ECCC.</summary>
    internal const ushort EndHopOpcode = 0xeccc;

    /// <summary>The terminal common sleep word of the hopping program at $A9:ECCE.</summary>
    internal const ushort HoppingSleepOpcode = 0xecce;

    /// <summary>The first following program, <c>InstList_CorpseZoomer_Param1_0</c>, at $A9:ECF5.</summary>
    internal const ushort FirstAdjacentProgram = 0xecf5;

    private static readonly DeadSidehopperInstructionMechanicsWord[] Words =
    [
        new(0xecac, 0x0002), new(0xecb0, 0x0004),
        new(0xecb4, 0x0005), new(0xecb8, 0x0030),
        new(0xecbc, 0x0005), new(0xecc0, 0x0004),
        new(0xecc4, 0x0005), new(0xecc8, 0x0004),
        new(EndHopOpcode, EnemyInstructionCodePointers.Instruction_SidehopperCorpse_EndHop),
        new(HoppingSleepOpcode, CommonEnemyInstructionCodes.Sleep),
        new(AliveIdle, 0x0001),
        new(0xece7, CommonEnemyInstructionCodes.Sleep),
        new(AliveCorpse, 0x0001),
        new(0xeced, CommonEnemyInstructionCodes.Sleep),
        new(InitiallyDead, 0x0001),
        new(0xecf3, CommonEnemyInstructionCodes.Sleep),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xecae, 0xecb2, 0xecb6, 0xecba,
        0xecbe, 0xecc2, 0xecc6, 0xecca,
        0xece5, 0xeceb, 0xecf1,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static DeadSidehopperInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Dead sidehopper instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
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
