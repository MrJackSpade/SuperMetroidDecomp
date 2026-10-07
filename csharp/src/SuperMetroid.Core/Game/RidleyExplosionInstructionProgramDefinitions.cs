namespace SuperMetroid.Core.Game;

/// <summary>Native $A6:CA47..CAEF breakup programs: one frame followed by sleep.</summary>
internal abstract class RidleyExplosionInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Bank $A6 owns the Ridley explosion enemy's ordinary spritemap programs.</summary>
    internal const byte Bank = 0xa6;
    static int IDeclaredProgramBank.Bank => Bank;
    /// <summary>$A6:CA47, the first Ridley explosion instruction list (small tail segment).</summary>
    internal const ushort First = 0xca47;
    /// <summary>The 29 native six-byte lists ending at $A6:CAEF.</summary>
    internal const int ProgramCount = 29;
    public static int PresentationWordCount => ProgramCount;
    public static ushort PresentationWordAddress(int index) => checked((ushort)(First + index * 6 + 2));
    public static int MechanicsWordCount => ProgramCount * 2;
    public static InstructionMechanicsWord MechanicsWord(int index) => new(
        checked((ushort)(First + index / 2 * 6 + (index % 2 == 0 ? 0 : 4))),
        index % 2 == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - First;
        if (offset >= 0 && offset < ProgramCount * 6)
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return CommonEnemyInstructionCodes.Sleep;
        }
        throw new InvalidDataException($"Ridley breakup mechanics pointer $A6:{address:X4} is not compiled.");
    }
}
