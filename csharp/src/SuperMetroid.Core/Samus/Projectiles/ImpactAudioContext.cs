namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Who owns a frame of projectile impact audio.</summary>
public enum ImpactAudioContext
{
    /// <summary>Ordinary gameplay.</summary>
    Gameplay,

    /// <summary>A cinematic, which suppresses impact audio.</summary>
    Cinematic,
}
