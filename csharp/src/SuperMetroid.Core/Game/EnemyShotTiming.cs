namespace SuperMetroid.Core.Game;

/// <summary>Cartridge common-shot timing shared by ordinary and boss adapters.</summary>
public static class EnemyShotTiming
{
    /// <summary>
    /// NormalEnemyShotAI at $A0:A85C-$A0:A85F stores sixteen frames of
    /// invincibility after a damaging projectile with the Plasma bit set.
    /// </summary>
    public const ushort PlasmaInvincibilityFrames = 16;
}
