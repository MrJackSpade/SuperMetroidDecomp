namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Fake Kraid's spit and left/right spike projectile poses.
/// Their interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class FakeKraidProjectileInstructionProgramDefinitions
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

    /// <summary>Number of interleaved sprite operands retained as live presentation words.</summary>
    public static int PresentationWordCount => 3;

    /// <summary>Returns the bank-$86 address of a spit-list spritemap operand.</summary>
    /// <param name="index">Zero-based operand index, shared by the three projectile instruction lists.</param>
    /// <returns>The address of that list's corresponding presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the three compiled presentation words.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Spit + 6 * index + 2);
    }

    /// <summary>Indicates whether a projectile kind is one of Fake Kraid's compiled spit or spike projectiles.</summary>
    /// <param name="kind">Projectile kind to check.</param>
    /// <returns><see langword="true"/> for the spit, left-spike, or right-spike projectile kinds.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.FakeKraidSpit or
        RoomEnemyProjectileKind.FakeKraidSpikeLeft or
        RoomEnemyProjectileKind.FakeKraidSpikeRight;

    /// <summary>Resolves a compiled projectile-list address to its held frame duration or sleep instruction pointer.</summary>
    /// <param name="address">Bank-$86 address of a list header or sleep word.</param>
    /// <returns>The mechanics word stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not one of the compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address) => address switch
    {
        Spit or SpikeLeft or SpikeRight => 0x7fff,
        SpitSleep or SpikeLeftSleep or SpikeRightSleep =>
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep,
        _ => throw new InvalidDataException(
            $"Fake Kraid projectile instruction mechanics pointer $86:{address:X4} " +
            "is not compiled."),
    };
}
