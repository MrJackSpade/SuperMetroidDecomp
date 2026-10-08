namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled initial selectors and inert delete programs for the three entrance-statue
/// enemy slots. The visible base and boss icons are separate bank-$86 projectile actors.
/// </summary>
internal abstract class TourianEntranceStatueInstructionProgramDefinitions
{
    /// <summary><c>InstList_TourianStatue_Ridley_0</c> at $AA:D7A5.</summary>
    internal const ushort Ridley = 0xd7a5;
    /// <summary><c>InstList_TourianStatue_Phantoon_0</c> at $AA:D7AF.</summary>
    internal const ushort Phantoon = 0xd7af;
    /// <summary><c>InstList_TourianStatue_BaseDecoration_0</c> at $AA:D7B9.</summary>
    internal const ushort BaseDecoration = 0xd7b9;

    public static int MechanicsWordCount => 3;

    /// <summary>
    /// $AA:D7A5/D7AF/D7B9: each live list stops immediately. The native layout
    /// separates entries by the two-byte stop plus an unused four-byte pose
    /// and four-byte back-edge; unused presentation remains outside this catalog.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return new((ushort)(Ridley + index * (2 + 4 + 4)), CommonEnemyInstructionCodes.StopScript);
    }
    /// <summary>Returns the list selected by native even parameter zero, two, or four.</summary>
    internal static ushort GetInitialInstruction(ushort parameter)
    {
        if (parameter > 4 || (parameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(parameter));
        return parameter switch
        {
            0 => BaseDecoration,
            2 => Ridley,
            _ => Phantoon,
        };
    }

    /// <summary>Returns a live program word or rejects adjacent unused presentation data.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Tourian entrance-statue mechanics pointer $AA:{address:X4} is not compiled.");
    }
}
