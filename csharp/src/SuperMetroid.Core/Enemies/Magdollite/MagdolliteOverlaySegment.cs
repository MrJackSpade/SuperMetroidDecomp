namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite tracking overlay's throw and pillar-cap animations (#641, #2554).
/// Their steps are <see cref="MagdolliteOverlayAnimations"/>.
/// </summary>
internal enum MagdolliteOverlaySegment
{
    /// <summary>Facing left, rearing with the throw sound and the attack's cooldown, and leaning out to throw.</summary>
    LeftThrow,
    /// <summary>Its hand facing left reaching out and flinging a gob of lava (#2439).</summary>
    LeftLavaFling,
    /// <summary>Its hand facing left throwing its last and finishing the throw (#2439).</summary>
    LeftThrowFinish,
    /// <summary>Facing right, rearing with the throw sound and the attack's cooldown, and leaning out to throw.</summary>
    RightThrow,
    /// <summary>Its hand facing right reaching out and flinging a gob of lava (#2439).</summary>
    RightLavaFling,
    /// <summary>Its hand facing right throwing its last and finishing the throw (#2439).</summary>
    RightThrowFinish,
    /// <summary>The cap on top of its pillar, held.</summary>
    PillarCap,
}
