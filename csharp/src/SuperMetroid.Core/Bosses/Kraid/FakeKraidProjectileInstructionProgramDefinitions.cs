namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's spit and left/right spike projectile programs. Each native list shows one pose for
/// <see cref="PoseDuration"/> passes and then sleeps; <see cref="FakeKraidProjectileAnimation"/> plays them typed.
/// </summary>
internal abstract class FakeKraidProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpit</c> at $86:9DDA.</summary>
    internal const ushort Spit = 0x9dda;

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Left</c> at $86:9DE0.</summary>
    internal const ushort SpikeLeft = 0x9de0;

    /// <summary><c>InstList_EnemyProjectile_MiniKraidSpikes_Right</c> at $86:9DE6.</summary>
    internal const ushort SpikeRight = 0x9de6;

    /// <summary>Projectile passes each pose shows before its list's terminal sleep.</summary>
    internal const int PoseDuration = 0x7fff;
}
