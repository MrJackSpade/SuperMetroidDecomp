namespace SuperMetroid.Core.Game;

/// <summary>
/// The beam-combination table index: bits 0-3 of a beam projectile's type word
/// (<c>$0C18</c>), copied from the equipped-beam word (<c>$09A6</c>). Bank-$90/$93/$9B
/// tables index projectile damage, speed, trails, sounds and enemy vulnerability by it.
/// Retail equipment never pairs Spazer with Plasma; the four values that do are reachable
/// only through glitched equipment, and their native table reads run past the twelve retail
/// rows (Chainsaw $D and SpaceTime $E read adjacent data as artwork).
/// </summary>
public enum SamusBeamCombination : byte
{
    /// <summary>$0: the unmodified Power Beam.</summary>
    Power = 0x0,
    /// <summary>$1: Wave.</summary>
    Wave = 0x1,
    /// <summary>$2: Ice.</summary>
    Ice = 0x2,
    /// <summary>$3: Ice and Wave.</summary>
    IceWave = 0x3,
    /// <summary>$4: Spazer.</summary>
    Spazer = 0x4,
    /// <summary>$5: Spazer and Wave.</summary>
    SpazerWave = 0x5,
    /// <summary>$6: Spazer and Ice.</summary>
    SpazerIce = 0x6,
    /// <summary>$7: Spazer, Ice and Wave.</summary>
    SpazerIceWave = 0x7,
    /// <summary>$8: Plasma.</summary>
    Plasma = 0x8,
    /// <summary>$9: Plasma and Wave.</summary>
    PlasmaWave = 0x9,
    /// <summary>$A: Plasma and Ice.</summary>
    PlasmaIce = 0xa,
    /// <summary>$B: Plasma, Ice and Wave.</summary>
    PlasmaIceWave = 0xb,
    /// <summary>$C: glitched Spazer and Plasma.</summary>
    SpazerPlasma = 0xc,
    /// <summary>$D: glitched Spazer, Plasma and Wave (the Chainsaw beam).</summary>
    SpazerPlasmaWave = 0xd,
    /// <summary>$E: glitched Spazer, Plasma and Ice (the SpaceTime beam).</summary>
    SpazerPlasmaIce = 0xe,
    /// <summary>$F: glitched Spazer, Plasma, Ice and Wave.</summary>
    SpazerPlasmaIceWave = 0xf,
}
