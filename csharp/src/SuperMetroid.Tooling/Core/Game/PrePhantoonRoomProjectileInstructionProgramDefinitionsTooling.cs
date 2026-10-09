using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PrePhantoonRoomProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PrePhantoonRoomProjectileInstructionProgramDefinitions))]
internal abstract class PrePhantoonRoomProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ISinglePresentationOperand, ICompiledMechanicsByteProbe
{
    /// <summary>$86:A3AA's $20-frame hold and $86:A3AE's delete.</summary>
    private static readonly ushort[] WordAddresses =
    [
        PrePhantoonRoomProjectileInstructionProgramDefinitions.Initial,
        PrePhantoonRoomProjectileInstructionProgramDefinitions.Initial + 4,
    ];

    /// <summary>Gets the two compiled mechanics words for the projectile's hold and delete instructions.</summary>
    public static int MechanicsWordCount => WordAddresses.Length;

    /// <summary>Returns the address and compiled value of the indexed hold or delete mechanics word.</summary>
    /// <param name="index">Zero-based position in the catalog's hold-then-delete word order.</param>
    /// <returns>The bank-local address and value used by tooling audits.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) =>
        new(WordAddresses[index], PrePhantoonRoomProjectileInstructionProgramDefinitions.ReadMechanicsWord(WordAddresses[index]));

    /// <summary>$86:A3AC, the held frame's blank visual selector.</summary>
    static ushort ISinglePresentationOperand.PresentationWord =>
        PrePhantoonRoomProjectileInstructionProgramDefinitions.Initial + 2;

    /// <summary>Tests whether a full SNES address is one of the bytes in the compiled hold or delete words.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address is in bank $86 and belongs to either mechanics word.</returns>
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
