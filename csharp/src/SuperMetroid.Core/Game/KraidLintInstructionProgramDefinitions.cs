namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Kraid's initial and post-growth belly-lint poses. Their two
/// interleaved ordinary-spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class KraidLintInstructionProgramDefinitions
{
    /// <summary><c>kKraid_Ilist_8AFE</c> at $A7:8AFE.</summary>
    internal const ushort Initial = 0x8afe;

    /// <summary><c>kKraid_Ilist_8B04</c> at $A7:8B04.</summary>
    internal const ushort PostGrowth = 0x8b04;

    /// <summary>Number of compiled duration and terminal-instruction words across the two pose lists.</summary>
    public static int MechanicsWordCount => 4;
    /// <summary>Number of compiled spritemap selectors, one for each pose list.</summary>
    public static int PresentationWordCount => 2;

    /// <summary>Two six-byte poses each contain duration, visual operand and Sleep.
    /// Mechanics ordinals alternate between the duration and terminal instruction.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool sleep = (index & 1) != 0;
        return new((ushort)(Initial + 6 * (index / 2) + (sleep ? 4 : 0)),
            sleep ? CommonEnemyInstructionCodes.Sleep : (ushort)0x7fff);
    }

    /// <summary>The visual operand lies two bytes into each six-byte pose.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + 6 * index + 2);
    }
    /// <summary>Reads a compiled duration or terminal instruction at its native address.</summary>
    /// <param name="address">Address of a mechanics word in the initial or post-growth pose lists.</param>
    /// <returns>The compiled instruction word stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify one of this program's mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }

        throw new InvalidDataException(
            $"Kraid lint instruction mechanics pointer $A7:{address:X4} is not compiled.");
    }
}
