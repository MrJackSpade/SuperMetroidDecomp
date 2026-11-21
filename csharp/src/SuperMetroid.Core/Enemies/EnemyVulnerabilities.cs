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
    /// <summary>How <paramref name="definition"/>'s enemy takes every weapon's hits: the vulnerabilities its definition names.</summary>
    public static EnemyVulnerabilities Of(RoomEnemyDefinition definition) => definition.Vulnerability;

    /// <summary>How the enemy takes the beam combination <paramref name="combination"/>.</summary>
    /// <remarks>
    /// The four combinations that pair Spazer with Plasma, which only the pause screen's same-frame toggle equips, take
    /// the missile, Super Missile, bomb and Power Bomb responses (<see cref="UnauthoredBeamVulnerability"/>).
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
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or SamusBeamCombination.SpazerPlasmaIce or
            SamusBeamCombination.SpazerPlasmaIceWave => UnauthoredBeamVulnerability.Response(combination, UnauthoredResponses),
        _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
    };

    /// <summary>
    /// The ordnance response each of the four combinations that pair Spazer with Plasma reads, in the order they count:
    /// the missile, Super Missile, bomb and Power Bomb responses.
    /// </summary>
    private Dictionary<SamusBeamCombination, EnemyWeaponVulnerability> UnauthoredResponses => new()
    {
        [SamusBeamCombination.SpazerPlasma] = Ordnance.Missile,
        [SamusBeamCombination.SpazerPlasmaWave] = Ordnance.SuperMissile,
        [SamusBeamCombination.SpazerPlasmaIce] = Ordnance.Bomb,
        [SamusBeamCombination.SpazerPlasmaIceWave] = Ordnance.PowerBomb,
    };
}
