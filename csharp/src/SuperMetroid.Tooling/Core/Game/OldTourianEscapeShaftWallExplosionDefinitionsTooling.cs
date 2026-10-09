using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="OldTourianEscapeShaftWallExplosionDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(OldTourianEscapeShaftWallExplosionDefinitions))]
internal abstract class OldTourianEscapeShaftWallExplosionDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>
    /// The control words of $86:B443-$B46F and each pose's duration. $86:B451 is the sound
    /// instruction's one-byte operand and each pose's following word is its spritemap.
    /// </summary>
    private static readonly ushort[] WordAddresses =
    [
        0xb443, 0xb445, 0xb447, OldTourianEscapeShaftWallExplosionDefinitions.Loop, 0xb44b, 0xb44d, 0xb44f,
        .. Enumerable.Range(0, OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordCount)
            .Select(index => (ushort)(OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordAddress(index) - 2)),
        0xb46a, 0xb46c, 0xb46e,
    ];

    /// <summary>Number of mechanics words compiled from the initial list and counted explosion loop.</summary>
    public static int MechanicsWordCount => WordAddresses.Length;

    /// <summary>Returns the mechanics address and encoded value at its catalog index, excluding spritemap operands.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics address sequence.</param>
    /// <returns>The instruction address and timer, pointer, or opcode value stored there.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) =>
        new(WordAddresses[index], OldTourianEscapeShaftWallExplosionDefinitions.ReadMechanicsWord(WordAddresses[index]));

    /// <summary>Number of small-explosion frame spritemap operands supplied by extracted presentation art.</summary>
    public static int PresentationWordCount => OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordCount;

    /// <summary>Maps a small-explosion frame ordinal to the address of its spritemap operand.</summary>
    /// <param name="index">Zero-based index in the six-frame explosion sequence.</param>
    public static ushort PresentationWordAddress(int index) => OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a byte address falls within either byte of a compiled bank-$86 mechanics word.</summary>
    /// <param name="address">24-bit address to test against the old Tourian fake-wall explosion program.</param>
    /// <returns><see langword="true"/> when the address identifies a mechanics word byte.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        foreach (ushort wordAddress in WordAddresses)
        {
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
