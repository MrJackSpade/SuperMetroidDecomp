namespace SuperMetroid.Core.Game;

/// <summary>Named outcomes of the nonzero charged-shot glow branches.</summary>
public enum SamusBeamChargePaletteAction : byte
{
    /// <summary>No charge-palette effect is active.</summary>
    Inactive,
    /// <summary>Advances the ordinary charged-shot glow cycle.</summary>
    ChargeCycle,
    /// <summary>Advances the pseudo-Screw Attack glow cycle.</summary>
    PseudoScrewCycle,
    /// <summary>Applies the ordinary white charged-shot palette.</summary>
    OrdinaryWhite,
    /// <summary>Applies the next Hyper Beam palette entry.</summary>
    HyperPalette,
    /// <summary>Holds the current Hyper Beam palette.</summary>
    HyperHold,
    /// <summary>Restores the normal suit palette.</summary>
    RestoredNormalSuit,
}
