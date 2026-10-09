using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BotwoonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BotwoonProjectileInstructionProgramDefinitions))]
internal abstract class BotwoonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of spritemap operand words selected by Botwoon's body, tail, and spit programs.</summary>
    public static int PresentationWordCount => BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Resolves an index in the compiled presentation sequence to its bank-$86 operand address.</summary>
    /// <param name="index">Zero-based position among the body, tail, and spit spritemap operands.</param>
    /// <returns>The address of the selected spritemap pointer word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation sequence.</exception>
    public static ushort PresentationWordAddress(int index) => BotwoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of address/value pairs exposed across the compiled body, tail, and spit instruction programs.</summary>
    public static int MechanicsWordCount => 73;

    /// <summary>Resolves an index in the tooling mechanics sequence to its native address and compiled value.</summary>
    /// <param name="index">Zero-based position spanning body commands, tail waits, and spit commands.</param>
    /// <returns>The instruction address and value represented at that position.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the 73 compiled mechanics words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 48)
        {
            int command = index % 6;
            address = BotwoonProjectileInstructionProgramDefinitions.BodyProgram(index / 6) + (command < 5 ? 4 * command : 18);
        }
        else if (index < 66)
            address = BotwoonProjectileInstructionProgramDefinitions.BodyProgram(8 + (index - 48) / 2) + 4 * ((index - 48) % 2);
        else
        {
            int command = index - 66;
            address = BotwoonProjectileInstructionProgramDefinitions.Spit + (command < 6 ? 4 * command : 22);
        }
        return new((ushort)address, BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    /// <summary>Tests whether a 24-bit bank-$86 address is a byte of a compiled mechanics word, excluding presentation operands.</summary>
    /// <param name="address">Absolute SNES address to classify.</param>
    /// <returns><see langword="true"/> for bytes occupied by compiled mechanics instructions or data; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & ~1);
        // Body/tail programs start at odd addresses; spit starts even.
        ushort bodyWord = unchecked((ushort)(((ushort)address - BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft & ~1) + BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft));
        if (BotwoonProjectileInstructionProgramDefinitions.TryBodyOffset(bodyWord, out int offset))
            return offset >= 16 || offset % 4 == 0;
        int sleeping = bodyWord - BotwoonProjectileInstructionProgramDefinitions.TailUpFacingRight;
        if ((uint)sleeping < 54) return sleeping % 6 != 2;
        int spit = word - BotwoonProjectileInstructionProgramDefinitions.Spit;
        return (uint)spit < 24 && (spit >= 20 || spit % 4 == 0);
    }
}
