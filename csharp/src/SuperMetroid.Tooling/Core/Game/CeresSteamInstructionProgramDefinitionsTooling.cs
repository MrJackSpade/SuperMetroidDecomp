using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresSteamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresSteamInstructionProgramDefinitions))]
internal abstract class CeresSteamInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The four directional Ceres steam programs contribute their activation and branch control words.</summary>
    public static int MechanicsWordCount => CeresSteamInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes a Ceres steam timing or conditional branch word from the directional programs.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresSteamInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Each of four Ceres steam directions retains nine extended-spritemap operands.</summary>
    public static int PresentationWordCount => 36;
    /// <summary>Locates a direction's steam spritemap operand within its 52-byte instruction program.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        int start = CeresSteamInstructionProgramDefinitions.Up + 52 * (index / 9);
        int frame = index % 9;
        return (ushort)(start + (frame < 2 ? 4 + 12 * frame : 22 + 4 * (frame - 2)));
    }
    /// <summary>Matches compiled bank-$A6 steam control bytes while excluding their extended-spritemap operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa60000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresSteamInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresSteamInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
