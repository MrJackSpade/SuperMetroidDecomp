namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>How one of Kraid's arm or foot rectangles reacts when a Samus projectile hits it.</summary>
internal enum KraidHitboxShot
{
    /// <summary>Every arm rectangle: the hit raises a dust explosion where the projectile struck.</summary>
    Arm = 1,
    /// <summary>Every foot rectangle: the hit has no Kraid-specific effect.</summary>
    Inert,
}
