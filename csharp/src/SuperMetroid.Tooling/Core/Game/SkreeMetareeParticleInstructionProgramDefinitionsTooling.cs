using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SkreeMetareeParticleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkreeMetareeParticleInstructionProgramDefinitions))]
internal abstract class SkreeMetareeParticleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 6;
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
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
            1 => new((ushort)(start + 4), (ushort)EnemyProjectileInstruction.GotoY),
            _ => new((ushort)(start + 6), start),
        };
    }
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)((index == 0 ? SkreeMetareeParticleInstructionProgramDefinitions.Skree : SkreeMetareeParticleInstructionProgramDefinitions.Metaree) + 2);
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
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
