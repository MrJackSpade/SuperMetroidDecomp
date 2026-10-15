using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Development-tool members of <see cref="SaveStationElectricityInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SaveStationElectricityInstructionProgramDefinitions))]
internal abstract class SaveStationElectricityInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The save-station discharge compiles its twenty-cycle setup, one-frame poses, loop, and deletion controls.</summary>
    public static int MechanicsWordCount => SaveStationElectricityInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes a discharge timer, frame duration, or terminal control word from the bank-$86 list.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => SaveStationElectricityInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>The electricity loop cycles through eight artwork selections.</summary>
    public static int PresentationWordCount => SaveStationElectricityInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the spritemap operand following an electricity frame's one-tick duration.</summary>
    public static ushort PresentationWordAddress(int index) => SaveStationElectricityInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches compiled discharge-control bytes in bank $86 without absorbing the eight visual operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
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
