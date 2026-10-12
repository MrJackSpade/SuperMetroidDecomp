using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BombTorizoStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BombTorizoStatueInstructionProgramDefinitions))]
internal abstract class BombTorizoStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => BombTorizoStatueInstructionProgramDefinitions.ProgramCount * 6;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int programIndex = index / 6;
        ushort program = BombTorizoStatueInstructionProgramDefinitions.Program(programIndex);
        int word = index % 6;
        // First timed frame, three-byte sound command, callback plus argument,
        // second timed frame, then delete. Reuse the direct reader for values.
        int offset = word switch
        {
            0 => 0,
            1 => 4,
            2 or 3 or 4 => 7 + 2 * (word - 2),
            _ => 15,
        };
        ushort address = (ushort)(program + offset);
        return new(address, BombTorizoStatueInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        return IsMechanicsWordStart(bankAddress) ||
            IsMechanicsWordStart(unchecked((ushort)(bankAddress - 1)));
    }
    /// <summary>Checks whether mechanics word start.</summary>
    internal static bool IsMechanicsWordStart(ushort address) =>
        BombTorizoStatueInstructionProgramDefinitions.TryDecodeProgramOffset(address, out _, out int offset) &&
        offset is 0 or 4 or 7 or 9 or 11 or 15;
}
