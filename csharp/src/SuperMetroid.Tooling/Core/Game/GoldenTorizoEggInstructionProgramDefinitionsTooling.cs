using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoEggInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEggInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEggInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if (TorizoChozoOrbInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) &&
            unchecked((ushort)address) is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab40)
        {
            return true;
        }
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
