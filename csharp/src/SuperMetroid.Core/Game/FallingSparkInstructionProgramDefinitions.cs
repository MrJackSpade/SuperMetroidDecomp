namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Falling Spark's falling and floor-impact programs. Interleaved
/// spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class FallingSparkInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_FallingSpark_Falling</c> at $86:F353.</summary>
    internal const ushort Falling = 0xf353;

    /// <summary><c>InstList_EnemyProjectile_FallingSpark_HitFloor</c> at $86:F363.</summary>
    internal const ushort HitFloor = 0xf363;

    /// <summary>
    /// Terminal <c>Instruction_EnemyProjectile_Delete</c> word in
    /// <c>InstList_EnemyProjectile_FallingSpark_HitFloor</c> at $86:F38F.
    /// </summary>
    internal const ushort HitFloorTerminalDelete = 0xf38f;

    public static int MechanicsWordCount => 17;
    public static int PresentationWordCount => 14;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 3 ? (ushort)(Falling + index * 4)
            : index < 5 ? (ushort)(HitFloor - 4 + (index - 3) * 2)
            : index < 16 ? (ushort)(HitFloor + (index - 5) * 4) : HitFloorTerminalDelete;
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 3 ? Falling + index * 4 + 2 : HitFloor + (index - 3) * 4 + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address) => TryRead(address, out ushort value)
        ? value : throw new InvalidDataException($"Falling Spark instruction mechanics pointer $86:{address:X4} is not compiled.");

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (address & 0xffff) - Falling;
        return offset >= 0 && TryRead((ushort)(Falling + (offset & ~1)), out _);
    }

    // Three falling drawings loop at three frames each. Eleven one-frame impact
    // drawings blink through the presentation operands, then delete the projectile.
    private static bool TryRead(ushort address, out ushort value)
    {
        value = 0;
        if (address >= Falling && address < HitFloor - 4 && (address - Falling) % 4 == 0)
        { value = 3; return true; }
        if (address == HitFloor - 4)
        { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY; return true; }
        if (address == HitFloor - 2) { value = Falling; return true; }
        if (address >= HitFloor && address < HitFloorTerminalDelete && (address - HitFloor) % 4 == 0)
        { value = 1; return true; }
        if (address == HitFloorTerminalDelete)
        { value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete; return true; }
        return false;
    }
}