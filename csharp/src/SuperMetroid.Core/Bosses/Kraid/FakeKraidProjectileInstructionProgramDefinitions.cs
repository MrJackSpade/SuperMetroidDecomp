namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's spit and left/right spike projectile programs. Each native list shows one pose for
/// <see cref="PoseDuration"/> passes and then sleeps; <see cref="FakeKraidProjectileAnimation"/> plays them typed.
/// </summary>
internal abstract class FakeKraidProjectileInstructionProgramDefinitions
{

    /// <summary>Projectile passes each pose shows before its list's terminal sleep.</summary>
    internal const int PoseDuration = 0x7fff;
}
