using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="GoldenTorizoEyeBeamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEyeBeamInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEyeBeamInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts beam wall/floor-impact controls and their authored timing words.</summary>
    public static int MechanicsWordCount => GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes a compiled bank-$86 eye-beam control for exact ROM comparison.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts seventeen separately extracted eye-beam spritemap operands.</summary>
    public static int PresentationWordCount => GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Finds one beam visual operand while the packed floor-impact sound remains audio data.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Checks both bytes of each bank-$86 mechanic word, excluding visual and sound operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
