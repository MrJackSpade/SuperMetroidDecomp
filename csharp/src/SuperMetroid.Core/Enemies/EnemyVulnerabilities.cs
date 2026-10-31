using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>How an enemy takes every weapon's hits.</summary>
/// <param name="Power">The Power Beam family's beams.</param>
/// <param name="Spazer">The Spazer family's beams.</param>
/// <param name="Plasma">The Plasma family's beams.</param>
/// <param name="ChargedBeam">
/// A charged beam's multiplier, which replaces the plain beam's when the beam does not freeze the enemy.
/// </param>
/// <param name="Ordnance">Missiles and bombs.</param>
/// <param name="Contact">Samus's attacking contact.</param>
public sealed record EnemyVulnerabilities(
    EnemyBeamFamilyVulnerability Power,
    EnemyBeamFamilyVulnerability Spazer,
    EnemyBeamFamilyVulnerability Plasma,
    EnemyWeaponVulnerability ChargedBeam,
    EnemyOrdnanceVulnerability Ordnance,
    EnemyContactVulnerability Contact)
{
    /// <summary>How <paramref name="definition"/>'s enemy takes every weapon's hits: its own vulnerabilities, or the standard ones.</summary>
    public static EnemyVulnerabilities Of(RoomEnemyDefinition definition) =>
        definition.Vulnerability ?? EnemyVulnerabilityDefinitions.Standard;

    /// <summary>How the enemy takes the beam combination <paramref name="combination"/>.</summary>
    /// <remarks>
    /// The four combinations that pair Spazer with Plasma exist only through glitched equipment. The authored tables
    /// list twelve beam combinations followed by the missile, Super Missile, bomb and Power Bomb entries, and the game
    /// looks those four combinations up past the twelve, so they take the missile, Super Missile, bomb and Power Bomb
    /// responses in that order. That lookup is gameplay a glitched beam can reach.
    /// </remarks>
    internal EnemyWeaponVulnerability ForBeam(SamusBeamCombination combination) => combination switch
    {
        SamusBeamCombination.Power => Power.Plain,
        SamusBeamCombination.Wave => Power.Wave,
        SamusBeamCombination.Ice => Power.Ice,
        SamusBeamCombination.IceWave => Power.IceWave,
        SamusBeamCombination.Spazer => Spazer.Plain,
        SamusBeamCombination.SpazerWave => Spazer.Wave,
        SamusBeamCombination.SpazerIce => Spazer.Ice,
        SamusBeamCombination.SpazerIceWave => Spazer.IceWave,
        SamusBeamCombination.Plasma => Plasma.Plain,
        SamusBeamCombination.PlasmaWave => Plasma.Wave,
        SamusBeamCombination.PlasmaIce => Plasma.Ice,
        SamusBeamCombination.PlasmaIceWave => Plasma.IceWave,
        SamusBeamCombination.SpazerPlasma => Ordnance.Missile,
        SamusBeamCombination.SpazerPlasmaWave => Ordnance.SuperMissile,
        SamusBeamCombination.SpazerPlasmaIce => Ordnance.Bomb,
        SamusBeamCombination.SpazerPlasmaIceWave => Ordnance.PowerBomb,
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };
}
