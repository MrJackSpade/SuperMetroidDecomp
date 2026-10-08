using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SaveStationElectricityInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SaveStationElectricityInstructionProgramDefinitions))]
internal abstract class SaveStationElectricityInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index) => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index);
    public static int PresentationWordCount => SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(index);
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
