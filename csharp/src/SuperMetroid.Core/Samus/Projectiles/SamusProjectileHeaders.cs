using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;
using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileHeaderDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program each projectile header starts.</summary>
internal static class SamusProjectileHeaders
{
    /// <summary>The program <paramref name="header"/>'s list field <paramref name="direction"/> selects, or null for no list.</summary>
    internal static SamusProjectileProgram? Program(SamusProjectileHeader header, int direction)
    {
        ProjectileHeaderLayout layout = header switch
        {
        // $93:8431, Power
        SamusProjectileHeader.UnchargedPower => new ProjectileHeaderLayout([P.PowerUpFacingRight, P.PowerUpRight, P.PowerRight, P.PowerDownRight, P.PowerDownFacingRight, P.PowerDownFacingRight, P.PowerDownLeft, P.PowerLeft, P.PowerUpLeft, P.PowerUpFacingRight], 0b0000000000),
        // $93:84B5, Wave
        SamusProjectileHeader.UnchargedWave => new ProjectileHeaderLayout([P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight], 0b0000000000),
        // $93:849F, Ice
        SamusProjectileHeader.UnchargedIce => new ProjectileHeaderLayout([P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight], 0b0000000000),
        // $93:84E1, IceWave
        SamusProjectileHeader.UnchargedIceWave => new ProjectileHeaderLayout([P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight], 0b0000000000),
        // $93:8447, Spazer
        SamusProjectileHeader.UnchargedSpazer => new ProjectileHeaderLayout([P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight], 0b0000000000),
        // $93:84F7, SpazerWave
        SamusProjectileHeader.UnchargedSpazerWave => new ProjectileHeaderLayout([P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight], 0b0000000000),
        // $93:845D, SpazerIce
        SamusProjectileHeader.UnchargedSpazerIce => new ProjectileHeaderLayout([P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight], 0b0000000000),
        // $93:8473, SpazerIceWave
        SamusProjectileHeader.UnchargedSpazerIceWave => new ProjectileHeaderLayout([P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight], 0b0000000000),
        // $93:84CB, Plasma
        SamusProjectileHeader.UnchargedPlasma => new ProjectileHeaderLayout([P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight], 0b0000000000),
        // $93:850D, PlasmaWave
        SamusProjectileHeader.UnchargedPlasmaWave => new ProjectileHeaderLayout([P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight], 0b0000000000),
        // $93:8523, PlasmaIce
        SamusProjectileHeader.UnchargedPlasmaIce => new ProjectileHeaderLayout([P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight], 0b0000000000),
        // $93:8489, PlasmaIceWave
        SamusProjectileHeader.UnchargedPlasmaIceWave => new ProjectileHeaderLayout([P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight], 0b0000000000),
        // $93:8539, ChargedPower
        SamusProjectileHeader.ChargedPower => new ProjectileHeaderLayout([P.ChargedPowerUpFacingRight, P.ChargedPowerUpRight, P.ChargedPowerRight, P.ChargedPowerDownRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownLeft, P.ChargedPowerLeft, P.ChargedPowerUpLeft, P.ChargedPowerUpFacingRight], 0b0000000000),
        // $93:85D3, ChargedWave
        SamusProjectileHeader.ChargedWave => new ProjectileHeaderLayout([P.ChargedWaveUpFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveDownFacingRight, P.ChargedWaveDownFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveUpFacingRight], 0b0000000000),
        // $93:85A7, ChargedIce
        SamusProjectileHeader.ChargedIce => new ProjectileHeaderLayout([P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight], 0b0000000000),
        // $93:85E9, ChargedIceWave
        SamusProjectileHeader.ChargedIceWave => new ProjectileHeaderLayout([P.ChargedIceWaveUpFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveUpFacingRight], 0b0000000000),
        // $93:854F, ChargedSpazer
        SamusProjectileHeader.ChargedSpazer => new ProjectileHeaderLayout([P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight], 0b0000000000),
        // $93:85FF, ChargedSpazerWave
        SamusProjectileHeader.ChargedSpazerWave => new ProjectileHeaderLayout([P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight], 0b0000000000),
        // $93:8565, ChargedSpazerIce
        SamusProjectileHeader.ChargedSpazerIce => new ProjectileHeaderLayout([P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight], 0b0000000000),
        // $93:857B, ChargedSpazerIceWave
        SamusProjectileHeader.ChargedSpazerIceWave => new ProjectileHeaderLayout([P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight], 0b0000000000),
        // $93:85BD, ChargedPlasma
        SamusProjectileHeader.ChargedPlasma => new ProjectileHeaderLayout([P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight], 0b0000000000),
        // $93:862B, ChargedPlasmaWave
        SamusProjectileHeader.ChargedPlasmaWave => new ProjectileHeaderLayout([P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight], 0b0000000000),
        // $93:8615, ChargedPlasmaIce
        SamusProjectileHeader.ChargedPlasmaIce => new ProjectileHeaderLayout([P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight], 0b0000000000),
        // $93:8591, ChargedPlasmaIceWave
        SamusProjectileHeader.ChargedPlasmaIceWave => new ProjectileHeaderLayout([P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight], 0b0000000000),
        // $93:8641, Missile
        SamusProjectileHeader.Missile => new ProjectileHeaderLayout([P.MissileUpFacingRight, P.MissileUpRight, P.MissileRight, P.MissileDownRight, P.MissileDownFacingRight, P.MissileDownFacingRight, P.MissileDownLeft, P.MissileLeft, P.MissileUpLeft, P.MissileUpFacingRight], 0b0000000000),
        // $93:8657, SuperMissile
        SamusProjectileHeader.SuperMissile => new ProjectileHeaderLayout([P.SuperMissileUpFacingRight, P.SuperMissileUpRight, P.SuperMissileRight, P.SuperMissileDownRight, P.SuperMissileDownFacingRight, P.SuperMissileDownFacingRight, P.SuperMissileDownLeft, P.SuperMissileLeft, P.SuperMissileUpLeft, P.SuperMissileUpFacingRight], 0b0000000000),
        // $93:866D, SuperMissileLink
        SamusProjectileHeader.SuperMissileLink => new ProjectileHeaderLayout([P.SuperMissileLink, null, P.PowerBomb, null, P.Bomb, null, P.BeamExplosion, null, P.MissileExplosion, null], 0b1000000000),
        // $93:8671, PowerBomb
        SamusProjectileHeader.PowerBomb => new ProjectileHeaderLayout([P.PowerBomb, null, P.Bomb, null, P.BeamExplosion, null, P.MissileExplosion, null, P.BombExplosion, null], 0b0010000000),
        // $93:8675, Bomb
        SamusProjectileHeader.Bomb => new ProjectileHeaderLayout([P.Bomb, null, P.BeamExplosion, null, P.MissileExplosion, null, P.BombExplosion, null, P.PlasmaSBA, null], 0b0000100000),
        // $93:8679, BeamExplosion
        SamusProjectileHeader.BeamExplosion => new ProjectileHeaderLayout([P.BeamExplosion, null, P.MissileExplosion, null, P.BombExplosion, null, P.PlasmaSBA, null, P.WaveSBA, null], 0b0000001000),
        // $93:867D, MissileExplosion
        SamusProjectileHeader.MissileExplosion => new ProjectileHeaderLayout([P.MissileExplosion, null, P.BombExplosion, null, P.PlasmaSBA, null, P.WaveSBA, null, P.SpazerUpFacingRight, null], 0b0000000010),
        // $93:8681, BombExplosion
        SamusProjectileHeader.BombExplosion => new ProjectileHeaderLayout([P.BombExplosion, null, P.PlasmaSBA, null, P.WaveSBA, null, P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null], 0b0000000000),
        // $93:8685, PlasmaSBA
        SamusProjectileHeader.PlasmaSBA => new ProjectileHeaderLayout([P.PlasmaSBA, null, P.WaveSBA, null, P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:8689, WaveSBA
        SamusProjectileHeader.WaveSBA => new ProjectileHeaderLayout([P.WaveSBA, null, P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:868D, SpazerSBA
        SamusProjectileHeader.SpazerSBA => new ProjectileHeaderLayout([P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:8691, SuperMissileExplosion
        SamusProjectileHeader.SuperMissileExplosion => new ProjectileHeaderLayout([P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:86D7, Projectile27
        SamusProjectileHeader.Projectile27 => new ProjectileHeaderLayout([P.Projectile27, null, null, null, null, null, P.PowerUpFacingRight, null, null, null], 0b0000010000),
        // $93:8695, Projectile25
        SamusProjectileHeader.Projectile25 => new ProjectileHeaderLayout([P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:86AB, SpazerSBATrail
        SamusProjectileHeader.SpazerSBATrail => new ProjectileHeaderLayout([P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight], 0b0000000000),
        // $93:86C1, ShinesparkEcho
        SamusProjectileHeader.ShinesparkEcho => new ProjectileHeaderLayout([P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight], 0b0000000000),
            _ => throw new ArgumentOutOfRangeException(nameof(header), header, "Undefined projectile header."),
        };
        if ((uint)direction >= layout.Fields.Length)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "A projectile header holds ten list fields.");
        return layout.Fields[direction] is { } program ? program
            : (layout.Empty & 1 << direction) != 0 ? null
            : throw new InvalidDataException($"Projectile header {header} field {direction} is not a list.");
    }
}
