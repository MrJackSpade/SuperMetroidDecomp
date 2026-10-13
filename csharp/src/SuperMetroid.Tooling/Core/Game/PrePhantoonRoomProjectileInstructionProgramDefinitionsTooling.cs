using SuperMetroid.Core.Hardware;
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

    /// <summary>Two compiled words: the initial duration and terminal delete.</summary>
    public static int MechanicsWordCount => WordAddresses.Length;
    /// <summary>Returns a compiled hold or delete word by its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) =>
        new(WordAddresses[index], PrePhantoonRoomProjectileInstructionProgramDefinitions.ReadMechanicsWord(WordAddresses[index]));

    /// <summary>$86:A3AC, the held frame's blank visual selector.</summary>
    static ushort ISinglePresentationOperand.PresentationWord =>
        PrePhantoonRoomProjectileInstructionProgramDefinitions.Initial + 2;

    /// <summary>Recognizes either byte of the hold or delete word in bank $86.</summary>
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
