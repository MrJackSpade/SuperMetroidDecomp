namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Powamp's looping spike animation and private delete list.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class PowampSpikeInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_PowampSpike</c> at $86:D208.</summary>
    internal const ushort Initial = 0xd208;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the spike loop at $86:D214.
    /// </summary>
    internal const ushort LoopCommand = 0xd214;

    /// <summary><c>InstList_EnemyProjectile_PowampSpike_Delete</c> at $86:D218.</summary>
    internal const ushort Delete = 0xd218;

    public static int MechanicsWordCount => 6;
    public static int PresentationWordCount => 3;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(index < 3 ? Initial + index * 4 : LoopCommand + (index - 3) * 2);
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Initial + index * 4 + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address) => TryRead(address, out ushort value)
        ? value : throw new InvalidDataException($"Powamp-spike instruction mechanics pointer $86:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (address & 0xffff) - Initial;
        return offset >= 0 && TryRead((ushort)(Initial + (offset & ~1)), out _);
    }

    // Three six-frame drawings repeat until the producer chooses the separate delete program.
    private static bool TryRead(ushort address, out ushort value)
    {
        value = 0;
        if (address >= Initial && address < LoopCommand && (address - Initial) % 4 == 0)
        { value = 6; return true; }
        if (address == LoopCommand)
        { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY; return true; }
        if (address == LoopCommand + 2) { value = Initial; return true; }
        if (address == Delete)
        { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete; return true; }
        return false;
    }
}