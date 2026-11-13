namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The Stoke's projectile animation (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="StokeProjectileAnimations"/>.
/// </summary>
internal enum StokeProjectileSegment
{
    /// <summary>The initial animation. No authored spawn or branch enters it.</summary>
    Initial,
}
