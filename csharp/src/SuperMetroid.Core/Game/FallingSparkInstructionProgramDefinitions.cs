namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Falling Spark's falling and floor-impact programs. Interleaved
/// spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class FallingSparkInstructionProgramDefinitions
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

    /// <summary>Gets the 14 spritemap operands interleaved with the falling and floor-impact mechanics words.</summary>
    public static int PresentationWordCount => 14;

    /// <summary>Returns the address of a falling or floor-impact spritemap operand.</summary>
    /// <param name="index">Zero-based presentation index: the first three select falling frames and the remaining eleven select impact frames.</param>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 14 presentation operands.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 3 ? Falling + index * 4 + 2 : HitFloor + (index - 3) * 4 + 2);
    }

    /// <summary>Reads a compiled duration or projectile command at an exact mechanics-word address.</summary>
    /// <param name="address">Bank-$86 address of the mechanics word.</param>
    /// <returns>The compiled word stored at <paramref name="address"/>.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address) => TryRead(address, out ushort value)
        ? value : throw new InvalidDataException($"Falling Spark instruction mechanics pointer $86:{address:X4} is not compiled.");

    // Three falling drawings loop at three frames each. Eleven one-frame impact
    // drawings blink through the presentation operands, then delete the projectile.
    /// <summary>Resolves only compiled mechanics words, leaving interleaved spritemap operands unclaimed.</summary>
    /// <param name="address">Bank-$86 address to test.</param>
    /// <param name="value">Receives the compiled mechanics word when the address is recognized; otherwise receives zero.</param>
    /// <returns><see langword="true"/> when <paramref name="address"/> is a compiled mechanics word; otherwise, <see langword="false"/>.</returns>
    internal static bool TryRead(ushort address, out ushort value)
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
