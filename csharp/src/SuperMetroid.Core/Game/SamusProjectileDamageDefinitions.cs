using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Native projectile damage headers, independent of their adjacent animation pointers.</summary>
internal static class SamusProjectileDamageDefinitions
{
    /// <summary>
    /// $93:8431..8640 ProjectileDataTable_Uncharged_* and ProjectileDataTable_Charged_*:
    /// twenty-four headers separated by ten direction pointers. Physical table order
    /// differs between the charged and uncharged halves; this is not a beam-bit index.
    /// </summary>
    private static ReadOnlySpan<ushort> BeamDamage =>
    [
        20, 40, 60, 100, 300, 30, 50, 150, 60, 70, 250, 200,
        60, 120, 180, 300, 900, 90, 450, 150, 180, 210, 600, 750,
    ];

    /// <summary>$93:8431 ProjectileDataTable_Uncharged_Power, first beam header.</summary>
    private const int BeamHeaderStart = 0x938431;
    /// <summary>Each beam header occupies one damage word and ten direction-pointer words.</summary>
    private const int BeamHeaderStride = 22;

    /// <summary>$93:8641 ProjectileDataTable_NonBeam_Missile: damage header identity.</summary>
    private const int Missile = 0x938641;
    /// <summary>$93:8657 ProjectileDataTable_NonBeam_SuperMissile: damage header identity.</summary>
    private const int SuperMissile = 0x938657;
    /// <summary>$93:866D ProjectileDataTable_NonBeam_SuperMissileLink: damage header identity.</summary>
    private const int SuperMissileLink = 0x93866d;
    /// <summary>$93:8671 ProjectileDataTable_NonBeam_PowerBomb: damage header identity.</summary>
    private const int PowerBomb = 0x938671;
    /// <summary>$93:8675 ProjectileDataTable_NonBeam_Bomb: damage header identity.</summary>
    private const int Bomb = 0x938675;
    /// <summary>$93:8679 ProjectileDataTable_NonBeam_BeamExplosion: damage header identity.</summary>
    private const int BeamExplosion = 0x938679;
    /// <summary>$93:867D ProjectileDataTable_NonBeam_MissileExplosion: damage header identity.</summary>
    private const int MissileExplosion = 0x93867d;
    /// <summary>$93:8681 ProjectileDataTable_NonBeam_BombExplosion: damage header identity.</summary>
    private const int BombExplosion = 0x938681;
    /// <summary>$93:8685 ProjectileDataTable_NonBeam_PlasmaSBA: damage header identity.</summary>
    private const int PlasmaSBA = 0x938685;
    /// <summary>$93:8689 ProjectileDataTable_NonBeam_WaveSBA: damage header identity.</summary>
    private const int WaveSBA = 0x938689;
    /// <summary>$93:868D ProjectileDataTable_NonBeam_SpazerSBA: damage header identity.</summary>
    private const int SpazerSBA = 0x93868d;
    /// <summary>$93:8691 ProjectileDataTable_NonBeam_SuperMissileExplosion: damage header identity.</summary>
    private const int SuperMissileExplosion = 0x938691;
    /// <summary>$93:8695 ProjectileDataTable_NonBeam_Projectile25: damage header identity.</summary>
    private const int Projectile25 = 0x938695;
    /// <summary>$93:86AB ProjectileDataTable_NonBeam_SpazerSBATrail: damage header identity.</summary>
    private const int SpazerSBATrail = 0x9386ab;
    /// <summary>$93:86C1 ProjectileDataTable_NonBeam_ShinesparkEcho: damage header identity.</summary>
    private const int ShinesparkEcho = 0x9386c1;
    /// <summary>$93:86D7 ProjectileDataTable_NonBeam_Projectile27: damage header identity.</summary>
    private const int Projectile27 = 0x9386d7;

    internal static ushort Read(ISnesAddressSpace bus, int address)
    {
        int offset = address - BeamHeaderStart;
        if (offset >= 0 && offset % BeamHeaderStride == 0 && offset / BeamHeaderStride < BeamDamage.Length)
            return BeamDamage[offset / BeamHeaderStride];

        // Match exact headers, not arbitrary bytes in this mixed mechanics/art region.
        // Unaligned, null and out-of-table addresses retain the original bus behavior.
        return address switch
        {
            Missile => 100,
            SuperMissile or SuperMissileLink => 300,
            PowerBomb => 200,
            Bomb => 30,
            BeamExplosion or MissileExplosion or SuperMissileExplosion => 8,
            BombExplosion or Projectile27 => 0,
            PlasmaSBA or WaveSBA or SpazerSBA or SpazerSBATrail => 300,
            Projectile25 => 0xf000,
            ShinesparkEcho => 0x1000,
            _ => (ushort)(bus.ReadByte(address) | bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8),
        };
    }
}
