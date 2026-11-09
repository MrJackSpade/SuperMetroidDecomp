namespace SuperMetroid.Core.Enemies;

/// <summary>Common-shot timing shared by ordinary and boss adapters.</summary>
public static class EnemyShotTiming
{
    /// <summary>
    /// The normal enemy shot AI stores sixteen frames of
    /// invincibility after a damaging projectile with the Plasma bit set.
    /// </summary>
    public const int PlasmaInvincibilityFrames = 16;
}
