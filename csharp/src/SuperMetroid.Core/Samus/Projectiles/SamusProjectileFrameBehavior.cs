namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Semantic identities for the frame behaviour each slot runs.</summary>
public enum SamusProjectileFrameBehavior
{
    /// <summary>No frame behaviour is assigned.</summary>
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
    /// <summary>The Spacetime shot's frame behaviour, which overwrites saved progression from an animation that reaches far enough.</summary>
    SpaceTimeProgressionOverwrite,
    /// <summary>The uncharged Chainsaw shot's frame behaviour, which runs a Power Bomb's fuse over the shot.</summary>
    ChainsawPowerBomb,
    /// <summary>
    /// The charged Chainsaw shot's frame behaviour, which runs whatever Samus's changing state holds; outside the
    /// emulation scope limit (#1500), it is refused when it runs.
    /// </summary>
    ChargedChainsawRunsSamusState,
    /// <summary>
    /// The Murder Beam's frame behaviour, which starts in the middle of an unrelated part of the game; outside the
    /// emulation scope limit (#1500), it is refused when it runs.
    /// </summary>
    MurderBeamMisalignedExecution,
}
