namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Whether a bomb frame advances the Power Bomb explosion's shape.</summary>
public enum PowerBombExplosionStep
{
    /// <summary>The explosion advances.</summary>
    Advance,

    /// <summary>The per-line effect phase advances it this frame instead.</summary>
    Skip,
}
