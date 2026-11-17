using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileSegment;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The animation each projectile kind's shots start: one for each direction a shot is fired in, or one whatever its direction.</summary>
internal static class SamusProjectileKindDefinitions
{
    /// <summary>The animation a shot of each directional kind starts in each direction it is fired in.</summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileKind, SamusProjectileDirectionMap<P>> DirectionalAnimations =
        new Dictionary<SamusProjectileKind, SamusProjectileDirectionMap<P>>
        {
            [SamusProjectileKind.UnchargedPower] = new(P.PowerUp, P.PowerUpRight, P.PowerRight, P.PowerDownRight, P.PowerDown, P.PowerDown, P.PowerDownLeft, P.PowerLeft, P.PowerUpLeft, P.PowerUp),
            [SamusProjectileKind.UnchargedWave] = new(P.WaveUp, P.WaveRisingDiagonal, P.WaveHorizontal, P.WaveFallingDiagonal, P.WaveVertical, P.WaveVertical, P.WaveRisingDiagonal, P.WaveHorizontal, P.WaveFallingDiagonal, P.WaveUp),
            [SamusProjectileKind.UnchargedIce] = new(P.Ice),
            [SamusProjectileKind.UnchargedIceWave] = new(P.WaveUp, P.WaveRisingDiagonal, P.WaveHorizontal, P.WaveFallingDiagonal, P.WaveVertical, P.WaveVertical, P.WaveRisingDiagonal, P.WaveHorizontal, P.WaveFallingDiagonal, P.WaveUp),
            [SamusProjectileKind.UnchargedSpazer] = new(P.SpazerUp, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDown, P.SpazerDown, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUp),
            [SamusProjectileKind.UnchargedSpazerWave] = new(P.SpazerWaveUp, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDown, P.SpazerWaveDown, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUp),
            [SamusProjectileKind.UnchargedSpazerIce] = new(P.SpazerUp, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDown, P.SpazerDown, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUp),
            [SamusProjectileKind.UnchargedSpazerIceWave] = new(P.SpazerWaveUp, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDown, P.SpazerWaveDown, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUp),
            [SamusProjectileKind.UnchargedPlasma] = new(P.PlasmaVertical, P.PlasmaRisingDiagonal, P.PlasmaHorizontal, P.PlasmaFallingDiagonal, P.PlasmaVertical, P.PlasmaVertical, P.PlasmaRisingDiagonal, P.PlasmaHorizontal, P.PlasmaFallingDiagonal, P.PlasmaVertical),
            [SamusProjectileKind.UnchargedPlasmaWave] = new(P.PlasmaWaveVertical, P.PlasmaWaveRisingDiagonal, P.PlasmaWaveHorizontal, P.PlasmaWaveFallingDiagonal, P.PlasmaWaveVertical, P.PlasmaWaveVertical, P.PlasmaWaveRisingDiagonal, P.PlasmaWaveHorizontal, P.PlasmaWaveFallingDiagonal, P.PlasmaWaveVertical),
            [SamusProjectileKind.UnchargedPlasmaIce] = new(P.PlasmaVertical, P.PlasmaRisingDiagonal, P.PlasmaHorizontal, P.PlasmaFallingDiagonal, P.PlasmaVertical, P.PlasmaVertical, P.PlasmaRisingDiagonal, P.PlasmaHorizontal, P.PlasmaFallingDiagonal, P.PlasmaVertical),
            [SamusProjectileKind.UnchargedPlasmaIceWave] = new(P.PlasmaIceWaveVertical, P.PlasmaIceWaveRisingDiagonal, P.PlasmaWaveHorizontal, P.PlasmaIceWaveFallingDiagonal, P.PlasmaIceWaveVertical, P.PlasmaIceWaveVertical, P.PlasmaIceWaveRisingDiagonal, P.PlasmaWaveHorizontal, P.PlasmaIceWaveFallingDiagonal, P.PlasmaIceWaveVertical),
            [SamusProjectileKind.ChargedPower] = new(P.ChargedPowerUp, P.ChargedPowerUpRight, P.ChargedPowerRight, P.ChargedPowerDownRight, P.ChargedPowerDown, P.ChargedPowerDown, P.ChargedPowerDownLeft, P.ChargedPowerLeft, P.ChargedPowerUpLeft, P.ChargedPowerUp),
            [SamusProjectileKind.ChargedWave] = new(P.ChargedWaveUp, P.ChargedWaveRisingDiagonal, P.ChargedWaveHorizontal, P.ChargedWaveFallingDiagonal, P.ChargedWaveVertical, P.ChargedWaveVertical, P.ChargedWaveRisingDiagonal, P.ChargedWaveHorizontal, P.ChargedWaveFallingDiagonal, P.ChargedWaveUp),
            [SamusProjectileKind.ChargedIce] = new(P.ChargedIce),
            [SamusProjectileKind.ChargedIceWave] = new(P.ChargedIceWaveUp, P.ChargedIceWaveRisingDiagonal, P.ChargedIceWaveHorizontal, P.ChargedIceWaveFallingDiagonal, P.ChargedIceWaveVertical, P.ChargedIceWaveVertical, P.ChargedIceWaveRisingDiagonal, P.ChargedIceWaveHorizontal, P.ChargedIceWaveFallingDiagonal, P.ChargedIceWaveUp),
            [SamusProjectileKind.ChargedSpazer] = new(P.ChargedSpazerVertical, P.ChargedSpazerRisingDiagonal, P.ChargedSpazerHorizontal, P.ChargedSpazerFallingDiagonal, P.ChargedSpazerVertical, P.ChargedSpazerVertical, P.ChargedSpazerRisingDiagonal, P.ChargedSpazerHorizontal, P.ChargedSpazerFallingDiagonal, P.ChargedSpazerVertical),
            [SamusProjectileKind.ChargedSpazerWave] = new(P.ChargedSpazerWaveUp, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDown, P.ChargedSpazerWaveDown, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUp),
            [SamusProjectileKind.ChargedSpazerIce] = new(P.ChargedSpazerVertical, P.ChargedSpazerRisingDiagonal, P.ChargedSpazerHorizontal, P.ChargedSpazerFallingDiagonal, P.ChargedSpazerVertical, P.ChargedSpazerVertical, P.ChargedSpazerRisingDiagonal, P.ChargedSpazerHorizontal, P.ChargedSpazerFallingDiagonal, P.ChargedSpazerVertical),
            [SamusProjectileKind.ChargedSpazerIceWave] = new(P.ChargedSpazerWaveUp, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDown, P.ChargedSpazerWaveDown, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUp),
            [SamusProjectileKind.ChargedPlasma] = new(P.ChargedPlasmaVertical, P.ChargedPlasmaRisingDiagonal, P.ChargedPlasmaHorizontal, P.ChargedPlasmaFallingDiagonal, P.ChargedPlasmaVertical, P.ChargedPlasmaVertical, P.ChargedPlasmaRisingDiagonal, P.ChargedPlasmaHorizontal, P.ChargedPlasmaFallingDiagonal, P.ChargedPlasmaVertical),
            [SamusProjectileKind.ChargedPlasmaWave] = new(P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveRisingDiagonal, P.ChargedPlasmaWaveHorizontal, P.ChargedPlasmaWaveFallingDiagonal, P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveRisingDiagonal, P.ChargedPlasmaWaveHorizontal, P.ChargedPlasmaWaveFallingDiagonal, P.ChargedPlasmaWaveVertical),
            [SamusProjectileKind.ChargedPlasmaIce] = new(P.ChargedPlasmaVertical, P.ChargedPlasmaRisingDiagonal, P.ChargedPlasmaHorizontal, P.ChargedPlasmaFallingDiagonal, P.ChargedPlasmaVertical, P.ChargedPlasmaVertical, P.ChargedPlasmaRisingDiagonal, P.ChargedPlasmaHorizontal, P.ChargedPlasmaFallingDiagonal, P.ChargedPlasmaVertical),
            [SamusProjectileKind.ChargedPlasmaIceWave] = new(P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveRisingDiagonal, P.ChargedPlasmaWaveHorizontal, P.ChargedPlasmaWaveFallingDiagonal, P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveVertical, P.ChargedPlasmaWaveRisingDiagonal, P.ChargedPlasmaWaveHorizontal, P.ChargedPlasmaWaveFallingDiagonal, P.ChargedPlasmaWaveVertical),
            [SamusProjectileKind.Missile] = new(P.MissileUp, P.MissileUpRight, P.MissileRight, P.MissileDownRight, P.MissileDown, P.MissileDown, P.MissileDownLeft, P.MissileLeft, P.MissileUpLeft, P.MissileUp),
            [SamusProjectileKind.SuperMissile] = new(P.SuperMissileUp, P.SuperMissileUpRight, P.SuperMissileRight, P.SuperMissileDownRight, P.SuperMissileDown, P.SuperMissileDown, P.SuperMissileDownLeft, P.SuperMissileLeft, P.SuperMissileUpLeft, P.SuperMissileUp),
            [SamusProjectileKind.SpazerSpecialAttackTrail] = new(P.SpazerSpecialAttackTrail),
            [SamusProjectileKind.ShinesparkEcho] = new(P.ShinesparkEcho),
        };

    /// <summary>
    /// The one animation each of the other kinds starts: the Super Missile link's, the bombs' and the special attacks'
    /// particles', which start it whatever their direction.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SamusProjectileKind, P> SoleAnimations =
        new Dictionary<SamusProjectileKind, P>
        {
            [SamusProjectileKind.SuperMissileLink] = P.SuperMissileLink,
            [SamusProjectileKind.PowerBomb] = P.PowerBomb,
            [SamusProjectileKind.Bomb] = P.Bomb,
            [SamusProjectileKind.PlasmaSpecialAttack] = P.PlasmaSpecialAttack,
            [SamusProjectileKind.WaveSpecialAttack] = P.WaveSpecialAttack,
            [SamusProjectileKind.SpazerSpecialAttack] = P.SpazerUp,
        };
}
