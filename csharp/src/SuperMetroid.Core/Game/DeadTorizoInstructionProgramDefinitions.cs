using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Dead Torizo's stationary corpse program. Its
/// spritemap operand is resolved by the compiled enemy visual catalog.
/// </summary>
internal abstract class DeadTorizoInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseTorizo</c> at $A9:D6DC.</summary>
    internal const ushort Stationary = 0xd6dc;

    /// <summary>The terminal common sleep word at $A9:D6E0.</summary>
    internal const ushort SleepOpcode = 0xd6e0;

    /// <summary>Native program bank $A9.</summary>
    internal const byte Bank = 0xa9;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xd6dc),
        Entry(Stationary),
        Op(0x0001),
        Origin(0xd6e0),
        Entry(SleepOpcode),
        Op((ushort)CommonEnemyInstruction.Sleep));

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Dead Torizo instruction mechanics pointer $A9:{address:X4} is not compiled.");
}
