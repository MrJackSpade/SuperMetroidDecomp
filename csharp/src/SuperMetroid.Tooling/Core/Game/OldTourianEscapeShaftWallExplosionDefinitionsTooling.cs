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

    public static int MechanicsWordCount => WordAddresses.Length;
    public static InstructionMechanicsWord MechanicsWord(int index) =>
        new(WordAddresses[index], OldTourianEscapeShaftWallExplosionDefinitions.ReadMechanicsWord(WordAddresses[index]));
    public static int PresentationWordCount => OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordAddress(index);

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
