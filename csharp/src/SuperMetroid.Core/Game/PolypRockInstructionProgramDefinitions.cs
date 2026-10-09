namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Polyp's single-frame lava-rock animation.
/// Its sprite operand selects installed presentation artwork.
/// </summary>
internal abstract class PolypRockInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_NorfairLavaquakeRocks</c> at $86:BBD5.</summary>
    internal const ushort Initial = 0xbbd5;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> holding the lava-rock frame at $86:BBD9.
    /// </summary>
    internal const ushort Sleep = 0xbbd9;

    /// <summary>Spritemap operand at $86:BBD7.</summary>
    internal const ushort PresentationWord = 0xbbd7;

    /// <summary>EnemyProjSpritemaps_LavaquakeRocks at $8D:9340, selected by $86:BBD7.</summary>
    internal const ushort Spritemap = 0x9340;

    /// <summary>Resolves Polyp's presentation operand to the fixed lava-rock spritemap.</summary>
    /// <param name="operandAddress">Native instruction-word address of the visual selector.</param>
    /// <returns>The spritemap pointer selected by the compiled presentation operand.</returns>
    /// <exception cref="InvalidDataException">The address is not Polyp's compiled visual operand.</exception>
    internal static ushort FrameAt(ushort operandAddress) => operandAddress == PresentationWord
        ? Spritemap
        : throw new InvalidDataException($"Unknown Polyp-rock visual operand $86:{operandAddress:X4}.");

    /// <summary>Gets the number of fixed control words in Polyp's compiled projectile instruction list.</summary>
    public static int MechanicsWordCount => 2;
    /// <summary>$86:BBD5-BBD9 installs one static rock pose then sleeps.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => index switch
    {
        0 => new(Initial, 1),
        1 => new(Sleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Finds the compiled mechanics value stored at a native instruction-word address.</summary>
    /// <param name="address">Address of a mechanics-owned word in the Polyp-rock instruction list.</param>
    /// <returns>The word value emitted for that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Polyp-rock instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}
