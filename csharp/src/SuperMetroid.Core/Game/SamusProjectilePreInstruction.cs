namespace SuperMetroid.Core.Game;

/// <summary>Semantic identities for the bank-$90 function pointers stored per slot.</summary>
public enum SamusProjectilePreInstruction : byte
{
    /// <summary>No pre-instruction is assigned.</summary>
    None,
    /// <summary>Moves an ordinary beam that does not have the Wave Beam trail.</summary>
    NoWaveBeam,
    /// <summary>Moves a Wave Beam using its three-frame trail phase.</summary>
    WaveBeamThreeFrameTrail,
    /// <summary>Moves a Wave Beam using its four-frame trail phase.</summary>
    WaveBeamFourFrameTrail,
    /// <summary>Moves a Hyper Beam projectile.</summary>
    HyperBeam,
    /// <summary>Moves and accelerates an ordinary missile.</summary>
    Missile,
    /// <summary>Moves and accelerates a Super Missile.</summary>
    SuperMissile,
    /// <summary>Updates the linked Super Missile auxiliary projectile.</summary>
    SuperMissileLink,
    /// <summary>Updates the main Ice Beam special-attack projectile.</summary>
    IceCombo,
    /// <summary>Moves an outward Ice Beam special-attack particle.</summary>
    IceComboOutward,
    /// <summary>Updates the Wave Beam special attack.</summary>
    WaveCombo,
    /// <summary>Updates the main Spazer special-attack phase.</summary>
    SpazerCombo,
    /// <summary>Updates the Plasma Beam special attack.</summary>
    PlasmaCombo,
    /// <summary>Moves a falling Spazer special-attack particle.</summary>
    SpazerComboFalling,
    /// <summary>Moves a Shinespark echo projectile.</summary>
    ShinesparkEcho,
    /// <summary>Represents execution that reaches the spacetime palette-copy tail.</summary>
    SpacetimePaletteCopyTail,
    /// <summary>Represents the chainsaw window-store path before Power Bomb execution.</summary>
    ChainsawWindowStoreThenPowerBomb,
    /// <summary>Represents charged chainsaw execution through low WRAM.</summary>
    ChargedChainsawLowWramExecution,
    /// <summary>Represents the Murder Beam's misaligned native execution path.</summary>
    MurderBeamMisalignedExecution,
}
