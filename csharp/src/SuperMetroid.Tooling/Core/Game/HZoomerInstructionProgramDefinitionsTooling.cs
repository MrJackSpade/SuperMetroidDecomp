using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HZoomerInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HZoomerInstructionProgramDefinitions))]
internal abstract class HZoomerInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The orange Wrecked Ship Zoomer's four surface loops contribute 36 axis, timing, and branch words.</summary>
    public static int MechanicsWordCount => HZoomerInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes an axis setup, three-tick pose, or loop control from a Zoomer surface animation.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => HZoomerInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Each of the four wall or ceiling loops selects five installed spritemaps.</summary>
    public static int PresentationWordCount => 20;
    /// <summary>Locates the frame selector within one of four 28-byte Zoomer surface programs.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(HZoomerInstructionProgramDefinitions.UpsideRight + 28 * (index / 5) + 6 + 4 * (index % 5));
    }
    /// <summary>Matches the two bytes of a compiled Zoomer control word in bank $A3, excluding its frame selectors.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < HZoomerInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = HZoomerInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
