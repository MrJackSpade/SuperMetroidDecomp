namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Which glow Samus's suit shows after a charged shot (#1682). Values are arbitrary.</summary>
public enum ChargedShotGlowKind
{
    /// <summary>No glow: the suit shows the charge colours or its own.</summary>
    None,

    /// <summary>An ordinary charged shot's white flash.</summary>
    ChargedShotFlash,

    /// <summary>The Hyper Beam shot's glow through its ten authored colours.</summary>
    HyperBeamGlow,
}
