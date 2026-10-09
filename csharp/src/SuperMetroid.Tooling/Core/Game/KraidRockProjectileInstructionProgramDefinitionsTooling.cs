using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KraidRockProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KraidRockProjectileInstructionProgramDefinitions))]
internal abstract class KraidRockProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled mechanics words for the shared poses and spit-rock shot program.</summary>
    public static int MechanicsWordCount => KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Returns the compiled mechanics address and value at its flattened catalog index.</summary>
    /// <param name="index">Zero-based index across the shared poses and spit-shot mechanics words.</param>
    /// <returns>The instruction address and its encoded timer, pointer, or opcode value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap operands across the shared poses and spit-rock shot frames.</summary>
    public static int PresentationWordCount => KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Maps a presentation operand ordinal to its address in a shared pose or spit-shot frame.</summary>
    /// <param name="index">Zero-based ordinal across two shared-pose operands and five shot-frame operands.</param>
    public static ushort PresentationWordAddress(int index) => KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address falls within either byte of a compiled bank-$86 mechanics word.</summary>
    /// <param name="address">24-bit address to test against the Kraid-rock projectile programs.</param>
    /// <returns><see langword="true"/> when the address identifies a compiled mechanics word byte.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
