namespace SuperMetroid.Core.Game;

internal readonly record struct DeadTorizoInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Dead Torizo's stationary corpse program. Its
/// spritemap operand remains live cartridge presentation data.
/// </summary>
internal static class DeadTorizoInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseTorizo</c> at $A9:D6DC.</summary>
    internal const ushort Stationary = 0xd6dc;

    /// <summary>The terminal common sleep word at $A9:D6E0.</summary>
    internal const ushort SleepOpcode = 0xd6e0;

    /// <summary><c>Spritemaps_CorpseTorizo</c> begins after the program at $A9:D6E2.</summary>
    internal const ushort FirstAdjacentPresentationData = 0xd6e2;

    private static readonly DeadTorizoInstructionMechanicsWord[] Words =
    [
        new(Stationary, 1),
        new(SleepOpcode, CommonEnemyInstructionCodes.Sleep),
    ];

    /// <summary>The live <c>Spritemaps_CorpseTorizo</c> operand at $A9:D6DE.</summary>
    internal const ushort PresentationWord = 0xd6de;

    internal static int MechanicsWordCount => Words.Length;
    internal static DeadTorizoInstructionMechanicsWord MechanicsWord(int index) => Words[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Dead Torizo instruction mechanics pointer $A9:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        return bankAddress is
            Stationary or Stationary + 1 or SleepOpcode or SleepOpcode + 1;
    }
}
