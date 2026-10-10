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

    // Three six-frame drawings repeat until the producer chooses the separate delete program.
    internal static bool TryRead(ushort address, out ushort value)
    {
        value = 0;
        if (address >= Initial && address < LoopCommand && (address - Initial) % 4 == 0)
        { value = 6; return true; }
        if (address == LoopCommand)
        { value = (ushort)EnemyProjectileInstruction.GotoY; return true; }
        if (address == LoopCommand + 2) { value = Initial; return true; }
        if (address == Delete)
        { value = (ushort)EnemyProjectileInstruction.Delete; return true; }
        return false;
    }
}