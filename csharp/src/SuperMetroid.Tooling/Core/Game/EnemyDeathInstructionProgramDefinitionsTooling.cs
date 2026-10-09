using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EnemyDeathInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyDeathInstructionProgramDefinitions))]
internal abstract class EnemyDeathInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of spritemap pointer operands exposed by the runtime instruction catalog.</summary>
    public static int PresentationWordCount => EnemyDeathInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Resolves an indexed presentation operand to its bank-local instruction address.</summary>
    /// <param name="index">Zero-based operand index in the catalog's presentation sequence.</param>
    /// <returns>Address of the corresponding spritemap pointer word.</returns>
    public static ushort PresentationWordAddress(int index) => EnemyDeathInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of mechanics words included in the tooling enumeration across all death programs.</summary>
    public static int MechanicsWordCount => 66;

    /// <summary>Returns the address and decoded value for one entry in the flattened mechanics-word view.</summary>
    /// <param name="index">Zero-based entry index across the respawn, explosion, and contact-death programs.</param>
    /// <returns>The bank-local address paired with the compiled mechanics word at that location.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the mechanics-word enumeration.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 3) address = EnemyDeathInstructionProgramDefinitions.RespawnTail + (index == 0 ? 0 : 2 * (index + 1));
        else if (index < 15) address = EnemyDeathInstructionProgramDefinitions.BigExplosion + LoopMechanicsOffset(index - 3, 12);
        else if (index < 29) address = EnemyDeathInstructionProgramDefinitions.MiniKraidExplosion + LoopMechanicsOffset(index - 15, 16);
        else if (index < 38) address = EnemyDeathInstructionProgramDefinitions.NormalExplosion + FrameMechanicsOffset(index - 29, 3, 6);
        else if (index < 47) address = EnemyDeathInstructionProgramDefinitions.SmallExplosion + FrameMechanicsOffset(index - 38, 3, 6);
        else address = EnemyDeathInstructionProgramDefinitions.KilledBySamusContact + FrameMechanicsOffset(index - 47, 7, 16);
        return new((ushort)address, EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    /// <summary>Maps a loop mechanics-word ordinal to its byte offset, accounting for the extra word after the delays.</summary>
    /// <param name="index">Zero-based mechanics-word ordinal within the loop.</param>
    /// <param name="delayOffset">Byte offset boundary after the loop's delay words.</param>
    /// <returns>Byte offset of the selected word within the compiled loop.</returns>
    internal static int LoopMechanicsOffset(int index, int delayOffset) =>
        index * 2 + (index > delayOffset / 2 ? 2 : 0);

    /// <summary>Maps a logical frame-program word index around frame records and the interposed sound operand.</summary>
    /// <param name="index">Zero-based mechanics-word ordinal in the frame sequence.</param>
    /// <param name="framesBeforeSound">Number of frame entries preceding the sound operand.</param>
    /// <param name="frameCount">Total number of timed frames in the sequence.</param>
    /// <returns>Byte offset of the selected instruction or operand word.</returns>
    internal static int FrameMechanicsOffset(int index, int framesBeforeSound, int frameCount) =>
        index <= framesBeforeSound ? 4 * index
        : index <= frameCount ? 4 * (index - 1) + 2
        : 4 * frameCount + 2 + 2 * (index - frameCount - 1);
    /// <summary>Determines whether a bank-$86 byte belongs to either byte of a translated mechanics word.</summary>
    /// <param name="address">Full CPU address to classify.</param>
    /// <returns><see langword="true"/> when this byte or the preceding byte starts a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        (EnemyDeathInstructionProgramDefinitions.TryWord(unchecked((ushort)address), out _) || EnemyDeathInstructionProgramDefinitions.TryWord(unchecked((ushort)(address - 1)), out _));
}
