using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

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

    /// <summary>Includes fixed control words and one duration before each wall-explosion pose.</summary>
    public static int MechanicsWordCount => WordAddresses.Length;
    /// <summary>Pairs a selected $86 instruction address with its compiled mechanic value.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) =>
        new(WordAddresses[index], OldTourianEscapeShaftWallExplosionDefinitions.ReadMechanicsWord(WordAddresses[index]));
    /// <summary>Bounds pose spritemap operands, excluding the sound operand at $86:B451.</summary>
    public static int PresentationWordCount => OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordCount;
    /// <summary>Locates the spritemap word following a wall-explosion pose duration.</summary>
    public static ushort PresentationWordAddress(int index) => OldTourianEscapeShaftWallExplosionDefinitions.PresentationWordAddress(index);

    /// <summary>Matches both bytes of each enumerated control or duration word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
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
