namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What one update of the charge glow showed on Samus's suit.</summary>
public enum SamusBeamChargeGlowAction
{
    /// <summary>No charge glow effect is active.</summary>
    Inactive,
    /// <summary>Advances the ordinary charged-shot glow cycle.</summary>
    ChargeCycle,
    /// <summary>Advances the pseudo-Screw Attack glow cycle.</summary>
    PseudoScrewCycle,
    /// <summary>Applies the ordinary white charged-shot glow.</summary>
    OrdinaryWhite,
    /// <summary>Applies the next Hyper Beam shade.</summary>
    HyperShade,
    /// <summary>Holds the current Hyper Beam shade.</summary>
    HyperHold,
    /// <summary>Restores the normal suit shade.</summary>
    RestoredNormalSuit,
}
