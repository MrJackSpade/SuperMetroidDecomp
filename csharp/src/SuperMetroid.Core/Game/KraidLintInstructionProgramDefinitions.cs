namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Kraid's initial and post-growth belly-lint poses. Their two
/// interleaved ordinary-spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class KraidLintInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>kKraid_Ilist_8AFE</c> at $A7:8AFE.</summary>
    internal const ushort Initial = 0x8afe;

    /// <summary><c>kKraid_Ilist_8B04</c> at $A7:8B04.</summary>
    internal const ushort PostGrowth = 0x8b04;

    /// <summary>The first adjacent Kraid fingernail program at $A7:8B0A.</summary>
    internal const ushort FirstAdjacentFootProgram = 0x8b0a;

    public static int MechanicsWordCount => 4;
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

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
