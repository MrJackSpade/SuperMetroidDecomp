using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program each projectile header starts.</summary>
internal static class SamusProjectileHeaders
{
    /// <summary>
    /// The programs <paramref name="header"/>'s list fields name, in direction order: every list a projectile started
    /// from the header can begin with, whichever direction it is fired in.
    /// </summary>
    internal static IEnumerable<SamusProjectileProgram> Programs(SamusProjectileHeader header) =>
        Layout(header).Fields.OfType<SamusProjectileProgram>();

    /// <summary><paramref name="header"/>'s list fields.</summary>
    private static ProjectileHeaderLayout Layout(SamusProjectileHeader header) => header switch
        {
        // Power
        SamusProjectileHeader.UnchargedPower => new ProjectileHeaderLayout([P.PowerUpFacingRight, P.PowerUpRight, P.PowerRight, P.PowerDownRight, P.PowerDownFacingRight, P.PowerDownFacingRight, P.PowerDownLeft, P.PowerLeft, P.PowerUpLeft, P.PowerUpFacingRight], 0b0000000000),
        // Wave
        SamusProjectileHeader.UnchargedWave => new ProjectileHeaderLayout([P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight], 0b0000000000),
        // Ice
        SamusProjectileHeader.UnchargedIce => new ProjectileHeaderLayout([P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight], 0b0000000000),
        // IceWave
        SamusProjectileHeader.UnchargedIceWave => new ProjectileHeaderLayout([P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight], 0b0000000000),
        // Spazer
        SamusProjectileHeader.UnchargedSpazer => new ProjectileHeaderLayout([P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight], 0b0000000000),
        // SpazerWave
        SamusProjectileHeader.UnchargedSpazerWave => new ProjectileHeaderLayout([P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight], 0b0000000000),
        // SpazerIce
        SamusProjectileHeader.UnchargedSpazerIce => new ProjectileHeaderLayout([P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight], 0b0000000000),
        // SpazerIceWave
        SamusProjectileHeader.UnchargedSpazerIceWave => new ProjectileHeaderLayout([P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight], 0b0000000000),
        // Plasma
        SamusProjectileHeader.UnchargedPlasma => new ProjectileHeaderLayout([P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight], 0b0000000000),
        // PlasmaWave
        SamusProjectileHeader.UnchargedPlasmaWave => new ProjectileHeaderLayout([P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight], 0b0000000000),
        // PlasmaIce
        SamusProjectileHeader.UnchargedPlasmaIce => new ProjectileHeaderLayout([P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight], 0b0000000000),
        // PlasmaIceWave
        SamusProjectileHeader.UnchargedPlasmaIceWave => new ProjectileHeaderLayout([P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight], 0b0000000000),
        // ChargedPower
        SamusProjectileHeader.ChargedPower => new ProjectileHeaderLayout([P.ChargedPowerUpFacingRight, P.ChargedPowerUpRight, P.ChargedPowerRight, P.ChargedPowerDownRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownLeft, P.ChargedPowerLeft, P.ChargedPowerUpLeft, P.ChargedPowerUpFacingRight], 0b0000000000),
        // ChargedWave
        SamusProjectileHeader.ChargedWave => new ProjectileHeaderLayout([P.ChargedWaveUpFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveDownFacingRight, P.ChargedWaveDownFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveUpFacingRight], 0b0000000000),
        // ChargedIce
        SamusProjectileHeader.ChargedIce => new ProjectileHeaderLayout([P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight], 0b0000000000),
        // ChargedIceWave
        SamusProjectileHeader.ChargedIceWave => new ProjectileHeaderLayout([P.ChargedIceWaveUpFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveUpFacingRight], 0b0000000000),
        // ChargedSpazer
        SamusProjectileHeader.ChargedSpazer => new ProjectileHeaderLayout([P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight], 0b0000000000),
        // ChargedSpazerWave
        SamusProjectileHeader.ChargedSpazerWave => new ProjectileHeaderLayout([P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight], 0b0000000000),
        // ChargedSpazerIce
        SamusProjectileHeader.ChargedSpazerIce => new ProjectileHeaderLayout([P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight], 0b0000000000),
        // ChargedSpazerIceWave
        SamusProjectileHeader.ChargedSpazerIceWave => new ProjectileHeaderLayout([P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight], 0b0000000000),
        // ChargedPlasma
        SamusProjectileHeader.ChargedPlasma => new ProjectileHeaderLayout([P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight], 0b0000000000),
        // ChargedPlasmaWave
        SamusProjectileHeader.ChargedPlasmaWave => new ProjectileHeaderLayout([P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight], 0b0000000000),
        // ChargedPlasmaIce
        SamusProjectileHeader.ChargedPlasmaIce => new ProjectileHeaderLayout([P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight], 0b0000000000),
        // ChargedPlasmaIceWave
        SamusProjectileHeader.ChargedPlasmaIceWave => new ProjectileHeaderLayout([P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight], 0b0000000000),
        // Missile
        SamusProjectileHeader.Missile => new ProjectileHeaderLayout([P.MissileUpFacingRight, P.MissileUpRight, P.MissileRight, P.MissileDownRight, P.MissileDownFacingRight, P.MissileDownFacingRight, P.MissileDownLeft, P.MissileLeft, P.MissileUpLeft, P.MissileUpFacingRight], 0b0000000000),
        // SuperMissile
        SamusProjectileHeader.SuperMissile => new ProjectileHeaderLayout([P.SuperMissileUpFacingRight, P.SuperMissileUpRight, P.SuperMissileRight, P.SuperMissileDownRight, P.SuperMissileDownFacingRight, P.SuperMissileDownFacingRight, P.SuperMissileDownLeft, P.SuperMissileLeft, P.SuperMissileUpLeft, P.SuperMissileUpFacingRight], 0b0000000000),
        // SuperMissileLink
        SamusProjectileHeader.SuperMissileLink => new ProjectileHeaderLayout([P.SuperMissileLink], 0),
        // PowerBomb
        SamusProjectileHeader.PowerBomb => new ProjectileHeaderLayout([P.PowerBomb], 0),
        // Bomb
        SamusProjectileHeader.Bomb => new ProjectileHeaderLayout([P.Bomb], 0),
        // PlasmaSBA
        SamusProjectileHeader.PlasmaSBA => new ProjectileHeaderLayout([P.PlasmaSBA], 0),
        // WaveSBA
        SamusProjectileHeader.WaveSBA => new ProjectileHeaderLayout([P.WaveSBA], 0),
        // SpazerSBA
        SamusProjectileHeader.SpazerSBA => new ProjectileHeaderLayout([P.SpazerUpFacingRight], 0),
        // SpazerSBATrail
        SamusProjectileHeader.SpazerSBATrail => new ProjectileHeaderLayout([P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight], 0b0000000000),
        // ShinesparkEcho
        SamusProjectileHeader.ShinesparkEcho => new ProjectileHeaderLayout([P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight], 0b0000000000),
            _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
        };

    /// <summary>The program <paramref name="header"/>'s list field <paramref name="direction"/> selects, or null for no list.</summary>
    internal static SamusProjectileProgram? Program(SamusProjectileHeader header, int direction)
    {
        ProjectileHeaderLayout layout = Layout(header);
        if ((uint)direction >= layout.Fields.Length)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Projectile header {header} keeps {layout.Fields.Length} list fields; no shot reads it in another direction.");
        return layout.Fields[direction] is { } program ? program
            : (layout.Empty & 1 << direction) != 0 ? null
            : throw new InvalidDataException($"Projectile header {header} field {direction} is not a list.");
    }
}
