namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Powamp's looping spike animation and private delete list.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class PowampSpikeInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_PowampSpike</c> at $86:D208.</summary>
    internal const ushort Initial = 0xd208;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the spike loop at $86:D214.
    /// </summary>
    internal const ushort LoopCommand = 0xd214;

    /// <summary><c>InstList_EnemyProjectile_PowampSpike_Delete</c> at $86:D218.</summary>
    internal const ushort Delete = 0xd218;
    public static int PresentationWordCount => 3;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + index * 4 + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address) => TryRead(address, out ushort value)
        ? value : throw new InvalidDataException($"Powamp-spike instruction mechanics pointer $86:{address:X4} is not compiled.");

    /// <summary>The control words that follow the three drawings, by bank-$86 address.</summary>
    private enum ControlWord : ushort
    {
        /// <summary>$86:D214: the loop's GotoY opcode.</summary>
        LoopGoto = LoopCommand,
        /// <summary>$86:D216: the loop's target, the initial drawing.</summary>
        LoopTarget = LoopCommand + 2,
        /// <summary>$86:D218: the delete program's opcode.</summary>
        DeleteOpcode = Delete,
    }

    // Three six-frame drawings repeat until the producer chooses the separate delete program.
    internal static bool TryRead(ushort address, out ushort value)
    {
        value = 0;
        if (address >= Initial && address < LoopCommand && (address - Initial) % 4 == 0)
        { value = 6; return true; }
        if (!Enum.IsDefined((ControlWord)address)) return false;
        value = (ControlWord)address switch
        {
            ControlWord.LoopGoto => (ushort)EnemyProjectileInstruction.GotoY,
            ControlWord.LoopTarget => Initial,
            ControlWord.DeleteOpcode => (ushort)EnemyProjectileInstruction.Delete,
            var word => throw new InvalidOperationException($"Undefined {nameof(ControlWord)} {(int)word}."),
        };
        return true;
    }
}