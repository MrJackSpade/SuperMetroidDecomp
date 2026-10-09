using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GunshipDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GunshipDustInstructionProgramDefinitions))]
internal abstract class GunshipDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of timer, frame-delay, and control words compiled across the six dust programs.</summary>
    public static int MechanicsWordCount => GunshipDustInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns a compiled mechanics instruction address and value at its flattened catalog index.</summary>
    /// <param name="index">Zero-based index among the mechanics words from all six programs.</param>
    /// <returns>The instruction address and its timer, delay, pointer, or opcode value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => GunshipDustInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Total number of animated-frame spritemap operands in the six liftoff-dust lists.</summary>
    public static int PresentationWordCount => GunshipDustInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps an animated-frame ordinal to the instruction address of its spritemap operand.</summary>
    /// <param name="index">Zero-based index among the animated frames in list order.</param>
    public static ushort PresentationWordAddress(int index) => GunshipDustInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address falls within either byte of a compiled bank-$86 mechanics word.</summary>
    /// <param name="address">24-bit address to test against the six gunship dust programs.</param>
    /// <returns><see langword="true"/> when the address identifies a compiled mechanics word byte.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GunshipDustInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = GunshipDustInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
