using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SkreeMetareeParticleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkreeMetareeParticleInstructionProgramDefinitions))]
internal abstract class SkreeMetareeParticleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of duration and loop-control words across the Skree and Metaree particle loops.</summary>
    public static int MechanicsWordCount => 6;

    /// <summary>Number of spritemap operands, one for each particle loop.</summary>
    public static int PresentationWordCount => 2;
    /// <summary>Each eight-byte loop is a sixteen-frame drawing followed by goto-self.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort start = index < 3 ? SkreeMetareeParticleInstructionProgramDefinitions.Skree : SkreeMetareeParticleInstructionProgramDefinitions.Metaree;
        int field = index % 3;
        return field switch
        {
            0 => new(start, 16),
            1 => new((ushort)(start + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
            _ => new((ushort)(start + 6), start),
        };
    }

    /// <summary>Gets the bank-$86 address of the Skree or Metaree particle spritemap operand.</summary>
    /// <param name="index">Zero-based selector: zero for Skree and one for Metaree.</param>
    /// <returns>The selected loop's presentation-word address.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the two particle loops.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index == 0 ? SkreeMetareeParticleInstructionProgramDefinitions.Skree : SkreeMetareeParticleInstructionProgramDefinitions.Metaree) + 2);
    }

    /// <summary>Checks whether a full banked address is one of the bytes used by a compiled loop-duration or control word.</summary>
    /// <param name="address">24-bit SNES address to classify.</param>
    /// <returns><see langword="true"/> for either byte of one of the six mechanics words in the projectile bank.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
