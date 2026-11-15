namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Whether a bomb frame advances the Power Bomb explosion's shape.</summary>
public enum PowerBombExplosionStep
{
    /// <summary>The explosion advances.</summary>
    Advance,

    /// <summary>The blast already advanced this update, right after the liquid wave and statue descent.</summary>
    Skip,
}
