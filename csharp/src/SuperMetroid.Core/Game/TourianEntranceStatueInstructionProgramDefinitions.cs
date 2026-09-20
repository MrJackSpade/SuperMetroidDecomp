namespace SuperMetroid.Core.Game;

/// <summary>One compiled Tourian entrance-statue mechanics word in bank $AA.</summary>
internal readonly record struct TourianEntranceStatueInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled initial selectors and inert delete programs for the three entrance-statue
/// enemy slots. The visible base and boss icons are separate bank-$86 projectile actors.
/// </summary>
internal static class TourianEntranceStatueInstructionProgramDefinitions
{
    /// <summary><c>InstList_TourianStatue_Ridley_0</c> at $AA:D7A5.</summary>
    internal const ushort Ridley = 0xd7a5;
    /// <summary><c>InstList_TourianStatue_Phantoon_0</c> at $AA:D7AF.</summary>
    internal const ushort Phantoon = 0xd7af;
    /// <summary><c>InstList_TourianStatue_BaseDecoration_0</c> at $AA:D7B9.</summary>
    internal const ushort BaseDecoration = 0xd7b9;
    /// <summary>First unused visible-loop list immediately after the live programs.</summary>
    internal const ushort AdjacentUnusedProgram = 0xd7bb;

    private static readonly ushort[] InitialPrograms =
        [BaseDecoration, Ridley, Phantoon];
    private static readonly TourianEntranceStatueInstructionMechanicsWord[] Words =
    [
        new(Ridley, CommonEnemyInstructionCodes.StopScript),
        new(Phantoon, CommonEnemyInstructionCodes.StopScript),
        new(BaseDecoration, CommonEnemyInstructionCodes.StopScript),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static TourianEntranceStatueInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];

    /// <summary>Returns the list selected by native even parameter zero, two, or four.</summary>
    internal static ushort GetInitialInstruction(ushort parameter)
    {
        if (parameter > 4 || (parameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(parameter));
        return InitialPrograms[parameter >> 1];
    }

    /// <summary>Returns a live program word or rejects adjacent unused presentation data.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Tourian entrance-statue mechanics pointer $AA:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
