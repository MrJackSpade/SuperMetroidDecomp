namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Named outcomes of the nonzero charged-shot glow branches.</summary>
public enum SamusBeamChargePaletteAction
{
    /// <summary>No charge-colour effect is active.</summary>
    Inactive,
    /// <summary>Advances the ordinary charged-shot glow cycle.</summary>
    ChargeCycle,
    /// <summary>Advances the pseudo-Screw Attack glow cycle.</summary>
    PseudoScrewCycle,
    /// <summary>Applies the ordinary white charged-shot colouring.</summary>
    OrdinaryWhite,
    /// <summary>Applies the next Hyper Beam colouring.</summary>
    HyperPalette,
    /// <summary>Holds the current Hyper Beam colouring.</summary>
    HyperHold,
    /// <summary>Restores the normal suit colours.</summary>
    RestoredNormalSuit,
}
