using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program each projectile header starts.</summary>
internal static class SamusProjectileHeaders
{
    /// <summary>
    /// The programs <paramref name="header"/>'s list fields name, in direction order: every list a projectile started
    /// from the header can begin with, whichever direction it is fired in.
    /// </summary>
    internal static IEnumerable<SamusProjectileProgram> Programs(SamusProjectileHeader header) => DirectionPrograms(header);

    /// <summary>
    /// The program a shot from <paramref name="header"/> starts in each firing direction, by aim index; a header whose shots
    /// start one program whatever their direction holds only that one.
    /// </summary>
    private static SamusProjectileProgram[] DirectionPrograms(SamusProjectileHeader header) => header switch
        {
        // Power
        SamusProjectileHeader.UnchargedPower => [P.PowerUpFacingRight, P.PowerUpRight, P.PowerRight, P.PowerDownRight, P.PowerDownFacingRight, P.PowerDownFacingRight, P.PowerDownLeft, P.PowerLeft, P.PowerUpLeft, P.PowerUpFacingRight],
        // Wave
        SamusProjectileHeader.UnchargedWave => [P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight],
        // Ice
        SamusProjectileHeader.UnchargedIce => [P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight],
        // IceWave
        SamusProjectileHeader.UnchargedIceWave => [P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight],
        // Spazer
        SamusProjectileHeader.UnchargedSpazer => [P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight],
        // SpazerWave
        SamusProjectileHeader.UnchargedSpazerWave => [P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight],
        // SpazerIce
        SamusProjectileHeader.UnchargedSpazerIce => [P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight],
        // SpazerIceWave
        SamusProjectileHeader.UnchargedSpazerIceWave => [P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight],
        // Plasma
        SamusProjectileHeader.UnchargedPlasma => [P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight],
        // PlasmaWave
        SamusProjectileHeader.UnchargedPlasmaWave => [P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight],
        // PlasmaIce
        SamusProjectileHeader.UnchargedPlasmaIce => [P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight],
        // PlasmaIceWave
        SamusProjectileHeader.UnchargedPlasmaIceWave => [P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight],
        // ChargedPower
        SamusProjectileHeader.ChargedPower => [P.ChargedPowerUpFacingRight, P.ChargedPowerUpRight, P.ChargedPowerRight, P.ChargedPowerDownRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownLeft, P.ChargedPowerLeft, P.ChargedPowerUpLeft, P.ChargedPowerUpFacingRight],
        // ChargedWave
        SamusProjectileHeader.ChargedWave => [P.ChargedWaveUpFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveDownFacingRight, P.ChargedWaveDownFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveUpFacingRight],
        // ChargedIce
        SamusProjectileHeader.ChargedIce => [P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight],
        // ChargedIceWave
        SamusProjectileHeader.ChargedIceWave => [P.ChargedIceWaveUpFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveUpFacingRight],
        // ChargedSpazer
        SamusProjectileHeader.ChargedSpazer => [P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight],
        // ChargedSpazerWave
        SamusProjectileHeader.ChargedSpazerWave => [P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight],
        // ChargedSpazerIce
        SamusProjectileHeader.ChargedSpazerIce => [P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight],
        // ChargedSpazerIceWave
        SamusProjectileHeader.ChargedSpazerIceWave => [P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight],
        // ChargedPlasma
        SamusProjectileHeader.ChargedPlasma => [P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight],
        // ChargedPlasmaWave
        SamusProjectileHeader.ChargedPlasmaWave => [P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight],
        // ChargedPlasmaIce
        SamusProjectileHeader.ChargedPlasmaIce => [P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight],
        // ChargedPlasmaIceWave
        SamusProjectileHeader.ChargedPlasmaIceWave => [P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight],
        // Missile
        SamusProjectileHeader.Missile => [P.MissileUpFacingRight, P.MissileUpRight, P.MissileRight, P.MissileDownRight, P.MissileDownFacingRight, P.MissileDownFacingRight, P.MissileDownLeft, P.MissileLeft, P.MissileUpLeft, P.MissileUpFacingRight],
        // SuperMissile
        SamusProjectileHeader.SuperMissile => [P.SuperMissileUpFacingRight, P.SuperMissileUpRight, P.SuperMissileRight, P.SuperMissileDownRight, P.SuperMissileDownFacingRight, P.SuperMissileDownFacingRight, P.SuperMissileDownLeft, P.SuperMissileLeft, P.SuperMissileUpLeft, P.SuperMissileUpFacingRight],
        // SuperMissileLink
        SamusProjectileHeader.SuperMissileLink => [P.SuperMissileLink],
        // PowerBomb
        SamusProjectileHeader.PowerBomb => [P.PowerBomb],
        // Bomb
        SamusProjectileHeader.Bomb => [P.Bomb],
        // PlasmaSBA
        SamusProjectileHeader.PlasmaSBA => [P.PlasmaSBA],
        // WaveSBA
        SamusProjectileHeader.WaveSBA => [P.WaveSBA],
        // SpazerSBA
        SamusProjectileHeader.SpazerSBA => [P.SpazerUpFacingRight],
        // SpazerSBATrail
        SamusProjectileHeader.SpazerSBATrail => [P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight],
        // ShinesparkEcho
        SamusProjectileHeader.ShinesparkEcho => [P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight],
            _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
        };

    /// <summary>The program a shot from <paramref name="header"/> fired in aim direction <paramref name="direction"/> starts.</summary>
    internal static SamusProjectileProgram Program(SamusProjectileHeader header, int direction)
    {
        SamusProjectileProgram[] programs = DirectionPrograms(header);
        if ((uint)direction >= programs.Length)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Projectile header {header} keeps {programs.Length} direction programs; no shot reads it in another direction.");
        return programs[direction];
    }
}
