using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="BombTorizoStatueInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BombTorizoStatueInstructionProgramDefinitions))]
internal abstract class BombTorizoStatueInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Drawing operands across the Bomb Torizo statue's indexed projectile programs.</summary>
    public static int PresentationWordCount => BombTorizoStatueInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a statue projectile drawing operand for the dependency audit.</summary>
    public static ushort PresentationWordAddress(int index) => BombTorizoStatueInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Each statue program contributes six timing, sound, callback, or delete words.</summary>
    public static int MechanicsWordCount => BombTorizoStatueInstructionProgramDefinitions.ProgramCount * 6;
    /// <summary>Reads one of the six compiled words in the indexed statue program.</summary>
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
    /// <summary>Accepts either byte of a statue projectile mechanics word in the projectile bank.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        return IsMechanicsWordStart(bankAddress) ||
            IsMechanicsWordStart(unchecked((ushort)(bankAddress - 1)));
    }
    /// <summary>Checks the six mechanics offsets after resolving a statue program's base.</summary>
    internal static bool IsMechanicsWordStart(ushort address) =>
        BombTorizoStatueInstructionProgramDefinitions.TryDecodeProgramOffset(address, out _, out int offset) &&
        offset is 0 or 4 or 7 or 9 or 11 or 15;
}
