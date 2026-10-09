using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DownwardGateProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DownwardGateProjectileInstructionProgramDefinitions))]
internal abstract class DownwardGateProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled behavior words in the moving and closed downward-gate instruction lists.</summary>
    public static int MechanicsWordCount => DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled behavior word and its native instruction-list address.</summary>
    /// <param name="index">Zero-based position in the compiled mechanics-word sequence.</param>
    /// <returns>The address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of spritemap pointer words that remain live presentation data.</summary>
    public static int PresentationWordCount => DownwardGateProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the native instruction-list address of a live spritemap pointer operand.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The address of the selected presentation operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index) => DownwardGateProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a ROM byte is part of a compiled mechanics word for a downward-gate projectile.</summary>
    /// <param name="address">24-bit ROM address to classify; either byte of a compiled word is accepted.</param>
    /// <returns><see langword="true"/> for a mechanics byte in the enemy-projectile bank; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DownwardGateProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DownwardGateProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
