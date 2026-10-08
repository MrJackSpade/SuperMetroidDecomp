using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Dead Torizo's stationary corpse program. Its
/// spritemap operand is resolved by the compiled enemy visual catalog.
/// </summary>
internal abstract class DeadTorizoInstructionProgramDefinitions : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary><c>InstList_CorpseTorizo</c> at $A9:D6DC.</summary>
    internal const ushort Stationary = 0xd6dc;

    /// <summary>The terminal common sleep word at $A9:D6E0.</summary>
    internal const ushort SleepOpcode = 0xd6e0;

    /// <summary>The <c>Spritemaps_CorpseTorizo</c> visual operand at $A9:D6DE.</summary>
    internal const ushort PresentationWord = 0xd6de;
    static ushort ISinglePresentationOperand.PresentationWord => PresentationWord;

    /// <summary>Native program bank $A9.</summary>
    internal const byte Bank = 0xa9;
    static int IDeclaredProgramBank.Bank => Bank;

    private static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd6dc),
        Entry(Stationary),
        Op(0x0001),
        Origin(0xd6e0),
        Entry(SleepOpcode),
        Op(CommonEnemyInstructionCodes.Sleep));

    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Dead Torizo instruction mechanics pointer $A9:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address) => Layout.IsCompiledMechanicsByte(address);
}
