namespace SuperMetroid.Core.Progression;

/// <summary>
/// The combination of beams a beam projectile carries, which selects its damage, speed, trails, sounds and enemy
/// vulnerability. Retail equipment never pairs Spazer with Plasma; the four combinations that do are reachable only
/// through glitched equipment. Its values are arbitrary and nothing reads them; <see cref="SamusBeamCombinations"/> owns
/// each combination's beam components.
/// </summary>
public enum SamusBeamCombination
{
    /// <summary>The unmodified Power Beam.</summary>
    Power,
    /// <summary>Wave.</summary>
    Wave,
    /// <summary>Ice.</summary>
    Ice,
    /// <summary>Ice and Wave.</summary>
    IceWave,
    /// <summary>Spazer.</summary>
    Spazer,
    /// <summary>Spazer and Wave.</summary>
    SpazerWave,
    /// <summary>Spazer and Ice.</summary>
    SpazerIce,
    /// <summary>Spazer, Ice and Wave.</summary>
    SpazerIceWave,
    /// <summary>Plasma.</summary>
    Plasma,
    /// <summary>Plasma and Wave.</summary>
    PlasmaWave,
    /// <summary>Plasma and Ice.</summary>
    PlasmaIce,
    /// <summary>Plasma, Ice and Wave.</summary>
    PlasmaIceWave,
    /// <summary>Glitched Spazer and Plasma.</summary>
    SpazerPlasma,
    /// <summary>Glitched Spazer, Plasma and Wave (the Chainsaw beam).</summary>
    SpazerPlasmaWave,
    /// <summary>Glitched Spazer, Plasma and Ice (the Spacetime Beam).</summary>
    SpazerPlasmaIce,
    /// <summary>Glitched Spazer, Plasma, Ice and Wave.</summary>
    SpazerPlasmaIceWave,
}
