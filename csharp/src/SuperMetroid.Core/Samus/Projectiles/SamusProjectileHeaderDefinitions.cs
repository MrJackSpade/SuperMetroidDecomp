using P = SuperMetroid.Core.Samus.Projectiles.SamusProjectileProgram;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The program each bank-$93 projectile header's list field selects (#1301), by header and direction: the ten fields a
/// directional header holds, and for the shorter headers the words beyond them that a glitched selection reads. A
/// zero field, marked in the header's empty mask, is no list (the safe left-facing Murder Beam); any other field that is
/// not a list fails loudly.
/// </summary>
internal static class SamusProjectileHeaderDefinitions
{
    /// <summary>The program <paramref name="header"/>'s list field <paramref name="direction"/> selects, or null for no list.</summary>
    internal static SamusProjectileProgram? Program(ushort header, int direction)
    {
        ProjectileHeaderLayout layout = header switch
        {
        // $93:8431, Power
        0x8431 => new ProjectileHeaderLayout([P.PowerUpFacingRight, P.PowerUpRight, P.PowerRight, P.PowerDownRight, P.PowerDownFacingRight, P.PowerDownFacingRight, P.PowerDownLeft, P.PowerLeft, P.PowerUpLeft, P.PowerUpFacingRight], 0b0000000000),
        // $93:84B5, Wave
        0x84b5 => new ProjectileHeaderLayout([P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight], 0b0000000000),
        // $93:849F, Ice
        0x849f => new ProjectileHeaderLayout([P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight, P.IceUpFacingRight], 0b0000000000),
        // $93:84E1, IceWave
        0x84e1 => new ProjectileHeaderLayout([P.WaveUpFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveDownFacingRight, P.WaveDownFacingRight, P.WaveUpRight, P.WaveRight, P.WaveDownRight, P.WaveUpFacingRight], 0b0000000000),
        // $93:8447, Spazer
        0x8447 => new ProjectileHeaderLayout([P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight], 0b0000000000),
        // $93:84F7, SpazerWave
        0x84f7 => new ProjectileHeaderLayout([P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight], 0b0000000000),
        // $93:845D, SpazerIce
        0x845d => new ProjectileHeaderLayout([P.SpazerUpFacingRight, P.SpazerUpRight, P.SpazerRight, P.SpazerDownRight, P.SpazerDownFacingRight, P.SpazerDownFacingRight, P.SpazerDownLeft, P.SpazerLeft, P.SpazerUpLeft, P.SpazerUpFacingRight], 0b0000000000),
        // $93:8473, SpazerIceWave
        0x8473 => new ProjectileHeaderLayout([P.SpazerWaveUpFacingRight, P.SpazerWaveUpRight, P.SpazerWaveRight, P.SpazerWaveDownRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownFacingRight, P.SpazerWaveDownLeft, P.SpazerWaveLeft, P.SpazerWaveUpLeft, P.SpazerWaveUpFacingRight], 0b0000000000),
        // $93:84CB, Plasma
        0x84cb => new ProjectileHeaderLayout([P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight], 0b0000000000),
        // $93:850D, PlasmaWave
        0x850d => new ProjectileHeaderLayout([P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpFacingRight, P.PlasmaWaveUpRight, P.PlasmaWaveRight, P.PlasmaWaveDownRight, P.PlasmaWaveUpFacingRight], 0b0000000000),
        // $93:8523, PlasmaIce
        0x8523 => new ProjectileHeaderLayout([P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight, P.PlasmaUpFacingRight, P.PlasmaUpRight, P.PlasmaRight, P.PlasmaDownRight, P.PlasmaUpFacingRight], 0b0000000000),
        // $93:8489, PlasmaIceWave
        0x8489 => new ProjectileHeaderLayout([P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpFacingRight, P.PlasmaIceWaveUpRight, P.PlasmaWaveRight, P.PlasmaIceWaveDownRight, P.PlasmaIceWaveUpFacingRight], 0b0000000000),
        // $93:8539, ChargedPower
        0x8539 => new ProjectileHeaderLayout([P.ChargedPowerUpFacingRight, P.ChargedPowerUpRight, P.ChargedPowerRight, P.ChargedPowerDownRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownFacingRight, P.ChargedPowerDownLeft, P.ChargedPowerLeft, P.ChargedPowerUpLeft, P.ChargedPowerUpFacingRight], 0b0000000000),
        // $93:85D3, ChargedWave
        0x85d3 => new ProjectileHeaderLayout([P.ChargedWaveUpFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveDownFacingRight, P.ChargedWaveDownFacingRight, P.ChargedWaveUpRight, P.ChargedWaveRight, P.ChargedWaveDownRight, P.ChargedWaveUpFacingRight], 0b0000000000),
        // $93:85A7, ChargedIce
        0x85a7 => new ProjectileHeaderLayout([P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight, P.ChargedIceUpFacingRight], 0b0000000000),
        // $93:85E9, ChargedIceWave
        0x85e9 => new ProjectileHeaderLayout([P.ChargedIceWaveUpFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveDownFacingRight, P.ChargedIceWaveUpRight, P.ChargedIceWaveRight, P.ChargedIceWaveDownRight, P.ChargedIceWaveUpFacingRight], 0b0000000000),
        // $93:854F, ChargedSpazer
        0x854f => new ProjectileHeaderLayout([P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight], 0b0000000000),
        // $93:85FF, ChargedSpazerWave
        0x85ff => new ProjectileHeaderLayout([P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight], 0b0000000000),
        // $93:8565, ChargedSpazerIce
        0x8565 => new ProjectileHeaderLayout([P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpFacingRight, P.ChargedSpazerUpRight, P.ChargedSpazerRight, P.ChargedSpazerDownRight, P.ChargedSpazerUpFacingRight], 0b0000000000),
        // $93:857B, ChargedSpazerIceWave
        0x857b => new ProjectileHeaderLayout([P.ChargedSpazerWaveUpFacingRight, P.ChargedSpazerWaveUpRight, P.ChargedSpazerWaveRight, P.ChargedSpazerWaveDownRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownFacingRight, P.ChargedSpazerWaveDownLeft, P.ChargedSpazerWaveLeft, P.ChargedSpazerWaveUpLeft, P.ChargedSpazerWaveUpFacingRight], 0b0000000000),
        // $93:85BD, ChargedPlasma
        0x85bd => new ProjectileHeaderLayout([P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight], 0b0000000000),
        // $93:862B, ChargedPlasmaWave
        0x862b => new ProjectileHeaderLayout([P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight], 0b0000000000),
        // $93:8615, ChargedPlasmaIce
        0x8615 => new ProjectileHeaderLayout([P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpFacingRight, P.ChargedPlasmaUpRight, P.ChargedPlasmaRight, P.ChargedPlasmaDownRight, P.ChargedPlasmaUpFacingRight], 0b0000000000),
        // $93:8591, ChargedPlasmaIceWave
        0x8591 => new ProjectileHeaderLayout([P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpFacingRight, P.ChargedPlasmaWaveUpRight, P.ChargedPlasmaWaveRight, P.ChargedPlasmaWaveDownRight, P.ChargedPlasmaWaveUpFacingRight], 0b0000000000),
        // $93:8641, Missile
        0x8641 => new ProjectileHeaderLayout([P.MissileUpFacingRight, P.MissileUpRight, P.MissileRight, P.MissileDownRight, P.MissileDownFacingRight, P.MissileDownFacingRight, P.MissileDownLeft, P.MissileLeft, P.MissileUpLeft, P.MissileUpFacingRight], 0b0000000000),
        // $93:8657, SuperMissile
        0x8657 => new ProjectileHeaderLayout([P.SuperMissileUpFacingRight, P.SuperMissileUpRight, P.SuperMissileRight, P.SuperMissileDownRight, P.SuperMissileDownFacingRight, P.SuperMissileDownFacingRight, P.SuperMissileDownLeft, P.SuperMissileLeft, P.SuperMissileUpLeft, P.SuperMissileUpFacingRight], 0b0000000000),
        // $93:866D, SuperMissileLink
        0x866d => new ProjectileHeaderLayout([P.SuperMissileLink, null, P.PowerBomb, null, P.Bomb, null, P.BeamExplosion, null, P.MissileExplosion, null], 0b1000000000),
        // $93:8671, PowerBomb
        0x8671 => new ProjectileHeaderLayout([P.PowerBomb, null, P.Bomb, null, P.BeamExplosion, null, P.MissileExplosion, null, P.BombExplosion, null], 0b0010000000),
        // $93:8675, Bomb
        0x8675 => new ProjectileHeaderLayout([P.Bomb, null, P.BeamExplosion, null, P.MissileExplosion, null, P.BombExplosion, null, P.PlasmaSBA, null], 0b0000100000),
        // $93:8679, BeamExplosion
        0x8679 => new ProjectileHeaderLayout([P.BeamExplosion, null, P.MissileExplosion, null, P.BombExplosion, null, P.PlasmaSBA, null, P.WaveSBA, null], 0b0000001000),
        // $93:867D, MissileExplosion
        0x867d => new ProjectileHeaderLayout([P.MissileExplosion, null, P.BombExplosion, null, P.PlasmaSBA, null, P.WaveSBA, null, P.SpazerUpFacingRight, null], 0b0000000010),
        // $93:8681, BombExplosion
        0x8681 => new ProjectileHeaderLayout([P.BombExplosion, null, P.PlasmaSBA, null, P.WaveSBA, null, P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null], 0b0000000000),
        // $93:8685, PlasmaSBA
        0x8685 => new ProjectileHeaderLayout([P.PlasmaSBA, null, P.WaveSBA, null, P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:8689, WaveSBA
        0x8689 => new ProjectileHeaderLayout([P.WaveSBA, null, P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:868D, SpazerSBA
        0x868d => new ProjectileHeaderLayout([P.SpazerUpFacingRight, null, P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:8691, SuperMissileExplosion
        0x8691 => new ProjectileHeaderLayout([P.SuperMissileExplosion, null, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:86D7, Projectile27
        0x86d7 => new ProjectileHeaderLayout([P.Projectile27, null, null, null, null, null, P.PowerUpFacingRight, null, null, null], 0b0000010000),
        // $93:8695, Projectile25
        0x8695 => new ProjectileHeaderLayout([P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight, P.Projectile25UpFacingRight], 0b0000000000),
        // $93:86AB, SpazerSBATrail
        0x86ab => new ProjectileHeaderLayout([P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight, P.SpazerSBATrailUpFacingRight], 0b0000000000),
        // $93:86C1, ShinesparkEcho
        0x86c1 => new ProjectileHeaderLayout([P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight, P.ShinesparkEchoUpFacingRight], 0b0000000000),
            _ => throw new InvalidDataException($"$93:{header:X4} is not a Samus projectile header."),
        };
        if ((uint)direction >= layout.Fields.Length)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "A projectile header holds ten list fields.");
        return layout.Fields[direction] is { } program ? program
            : (layout.Empty & 1 << direction) != 0 ? null
            : throw new InvalidDataException($"Projectile header $93:{header:X4} field {direction} is not a list.");
    }
}
