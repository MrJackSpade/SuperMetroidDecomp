using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program each projectile definition's shots start: one for each direction a shot is fired in, or one whatever its direction.</summary>
internal static class SamusProjectileHeaderDefinitions
{
    /// <summary>The program a shot from each directional definition starts in each direction it is fired in.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileHeader, SamusProjectileDirectionMap<P>> DirectionalPrograms =
        new Dictionary<SamusProjectileHeader, SamusProjectileDirectionMap<P>>
        {
            [SamusProjectileHeader.UnchargedPower] = new(P.PowerUpFacingRight, P.PowerUpRight, P.PowerRight, P.PowerDownRight, P.PowerDownFacingRight, P.PowerDownFacingRight, P.PowerDownLeft, P.PowerLeft, P.PowerUpLeft, P.PowerUpFacingRight),
            [SamusProjectileHeader.UnchargedWave] = new(P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight),
            [SamusProjectileHeader.UnchargedIce] = new(P.IceUpFacingRight),
            [SamusProjectileHeader.UnchargedIceWave] = new(P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight),
            [SamusProjectileHeader.UnchargedSpazer] = new(P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight),
            [SamusProjectileHeader.UnchargedSpazerWave] = new(P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight),
            [SamusProjectileHeader.UnchargedSpazerIce] = new(P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight),
            [SamusProjectileHeader.UnchargedSpazerIceWave] = new(P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight),
            [SamusProjectileHeader.UnchargedPlasma] = new(P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight),
            [SamusProjectileHeader.UnchargedPlasmaWave] = new(P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight),
            [SamusProjectileHeader.UnchargedPlasmaIce] = new(P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight),
            [SamusProjectileHeader.UnchargedPlasmaIceWave] = new(P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight),
            [SamusProjectileHeader.ChargedPower] = new(P.ChargedPowerUpFacingRight, P.ChargedPowerUpRight, P.ChargedPowerRight, P.ChargedPowerDownRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownLeft, P.ChargedPowerLeft, P.ChargedPowerUpLeft, P.ChargedPowerUpFacingRight),
            [SamusProjectileHeader.ChargedWave] = new(P.ChargedWaveUpFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveDownFacingRight, P.ChargedWaveDownFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveUpFacingRight),
            [SamusProjectileHeader.ChargedIce] = new(P.ChargedIceUpFacingRight),
            [SamusProjectileHeader.ChargedIceWave] = new(P.ChargedIceWaveUpFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveUpFacingRight),
            [SamusProjectileHeader.ChargedSpazer] = new(P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight),
            [SamusProjectileHeader.ChargedSpazerWave] = new(P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight),
            [SamusProjectileHeader.ChargedSpazerIce] = new(P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight),
            [SamusProjectileHeader.ChargedSpazerIceWave] = new(P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight),
            [SamusProjectileHeader.ChargedPlasma] = new(P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight),
            [SamusProjectileHeader.ChargedPlasmaWave] = new(P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight),
            [SamusProjectileHeader.ChargedPlasmaIce] = new(P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight),
            [SamusProjectileHeader.ChargedPlasmaIceWave] = new(P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight),
            [SamusProjectileHeader.Missile] = new(P.MissileUpFacingRight, P.MissileUpRight, P.MissileRight, P.MissileDownRight, P.MissileDownFacingRight, P.MissileDownFacingRight, P.MissileDownLeft, P.MissileLeft, P.MissileUpLeft, P.MissileUpFacingRight),
            [SamusProjectileHeader.SuperMissile] = new(P.SuperMissileUpFacingRight, P.SuperMissileUpRight, P.SuperMissileRight, P.SuperMissileDownRight, P.SuperMissileDownFacingRight, P.SuperMissileDownFacingRight, P.SuperMissileDownLeft, P.SuperMissileLeft, P.SuperMissileUpLeft, P.SuperMissileUpFacingRight),
            [SamusProjectileHeader.SpazerSBATrail] = new(P.SpazerSBATrailUpFacingRight),
            [SamusProjectileHeader.ShinesparkEcho] = new(P.ShinesparkEchoUpFacingRight),
        };

    /// <summary>
    /// The one program each of the other definitions starts: the Super Missile link's, the bombs' and the special attacks'
    /// particles', which start it whatever their direction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileHeader, P> SolePrograms =
        new Dictionary<SamusProjectileHeader, P>
        {
            [SamusProjectileHeader.SuperMissileLink] = P.SuperMissileLink,
            [SamusProjectileHeader.PowerBomb] = P.PowerBomb,
            [SamusProjectileHeader.Bomb] = P.Bomb,
            [SamusProjectileHeader.PlasmaSBA] = P.PlasmaSBA,
            [SamusProjectileHeader.WaveSBA] = P.WaveSBA,
            [SamusProjectileHeader.SpazerSBA] = P.SpazerUpFacingRight,
        };
}
