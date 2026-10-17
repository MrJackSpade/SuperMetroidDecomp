namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How one of the ordinary Pirates' collision rectangles reacts when a Samus projectile hits it.</summary>
internal enum SpacePirateHitboxShot
{
    /// <summary>The empty frame's point rectangle: the common enemy shot reaction, bypassing the gold ninja's armor tests.</summary>
    Ordinary = 1,
    /// <summary>An ordinary Pirate rectangle: the Pirate family's shot reaction.</summary>
    Pirate,
    /// <summary>A gold ninja rectangle that takes damage from beams and missiles its vulnerabilities admit.</summary>
    GoldNinjaVulnerable,
    /// <summary>A gold ninja rectangle that reflects beams and ignores heavier weapons.</summary>
    GoldNinjaInvincible,
}
