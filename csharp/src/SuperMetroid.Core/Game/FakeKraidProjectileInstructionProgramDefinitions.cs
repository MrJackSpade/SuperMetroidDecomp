namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Fake Kraid's spit and left/right spike projectile poses.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class FakeKraidProjectileInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpit</c> at $86:9DDA.</summary>
    internal const ushort Spit = 0x9dda;

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Left</c> at $86:9DE0.</summary>
    internal const ushort SpikeLeft = 0x9de0;

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Right</c> at $86:9DE6.</summary>
    internal const ushort SpikeRight = 0x9de6;

    /// <summary><c>Instruction_EnemyProjectile_Sleep</c> in the spit list at $86:9DDE.</summary>
    internal const ushort SpitSleep = 0x9dde;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the left-spike list at $86:9DE4.
    /// </summary>
    internal const ushort SpikeLeftSleep = 0x9de4;

    /// <summary>
    /// <c>Instruction_EnemyProjectile_Sleep</c> in the right-spike list at $86:9DEA.
    /// </summary>
    internal const ushort SpikeRightSleep = 0x9dea;

    // Each pose occupies six bytes: duration, visual operand, terminal sleep.
    public static int MechanicsWordCount => 6;
    public static int PresentationWordCount => 3;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address = (ushort)(Spit + 6 * (index / 2) + 4 * (index % 2));
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Spit + 6 * index + 2);
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.FakeKraidSpit or
        RoomEnemyProjectileKind.FakeKraidSpikeLeft or
        RoomEnemyProjectileKind.FakeKraidSpikeRight;

    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        Spit or SpikeLeft or SpikeRight => 0x7fff,
        SpitSleep or SpikeLeftSleep or SpikeRightSleep =>
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep,
        _ => throw new InvalidDataException(
            $"Fake Kraid projectile instruction mechanics pointer $86:{address:X4} " +
            "is not compiled."),
    };

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = (ushort)address - Spit;
        return (uint)offset < 18 && offset % 6 is 0 or 1 or 4 or 5;
    }
}
