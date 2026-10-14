namespace SuperMetroid.Core.Game;

/// <summary>Native projectile damage headers, independent of their adjacent animation pointers.</summary>
internal static class SamusProjectileDamageDefinitions
{
    /// <summary>$93:8431 ProjectileDataTable_Uncharged_Power, first beam header.</summary>
    internal const int BeamHeaderStart = 0x938431;
    /// <summary>Each beam header occupies one damage word and ten direction-pointer words.</summary>
    internal const int BeamHeaderStride = 22;

    /// <summary>
    /// Resolves damage at an exact beam or non-beam header address; adjacent animation words remain outside this compiled table.
    /// </summary>
    internal static ushort Read(int address)
    {
        int offset = address - BeamHeaderStart;
        if (offset >= 0 && offset % BeamHeaderStride == 0 && offset / BeamHeaderStride < 24)
            return BeamDamage(offset / BeamHeaderStride);

        // Match exact headers, not arbitrary bytes in this mixed mechanics/art region.
        // Unaligned, null and out-of-table addresses retain the original bus behavior.
        var header = (SamusProjectileHeader)address;
        if (!Enum.IsDefined(header))
            throw new InvalidDataException(
                $"Projectile damage header ${address:X6} is outside the compiled cartridge definitions.");
        return header switch
        {
            SamusProjectileHeader.Missile => 100,
            SamusProjectileHeader.SuperMissile or SamusProjectileHeader.SuperMissileLink => 300,
            SamusProjectileHeader.PowerBomb => 200,
            SamusProjectileHeader.Bomb => 30,
            SamusProjectileHeader.BeamExplosion or SamusProjectileHeader.MissileExplosion or
                SamusProjectileHeader.SuperMissileExplosion => 8,
            SamusProjectileHeader.BombExplosion or SamusProjectileHeader.Projectile27 => 0,
            SamusProjectileHeader.PlasmaSBA or SamusProjectileHeader.WaveSBA or SamusProjectileHeader.SpazerSBA or
                SamusProjectileHeader.SpazerSBATrail => 300,
            SamusProjectileHeader.Projectile25 => 0xf000,
            SamusProjectileHeader.ShinesparkEcho => 0x1000,
            _ => throw new InvalidOperationException($"Undefined SamusProjectileHeader {header}."),
        };
    }
    /// <summary>
    /// $93:8431..8640 ProjectileDataTable_Uncharged_* / Charged_*: twelve
    /// weapon headers per charge state, each followed by ten direction pointers.
    /// The charged half swaps Wave/Plasma and Plasma-Wave/Plasma-Ice order.
    /// </summary>
    internal static (bool Charged, SamusBeamCombination Beam) BeamIdentity(int header)
    {
        if ((uint)header >= 24)
            throw new ArgumentOutOfRangeException(nameof(header), header, "There are twenty-four beam headers.");
        bool charged = header >= 12;
        SamusBeamCombination beam = (header % 12) switch
        {
            0 => SamusBeamCombination.Power,
            1 => SamusBeamCombination.Spazer,
            2 => SamusBeamCombination.SpazerIce,
            3 => SamusBeamCombination.SpazerIceWave,
            4 => SamusBeamCombination.PlasmaIceWave,
            5 => SamusBeamCombination.Ice,
            6 => charged ? SamusBeamCombination.Plasma : SamusBeamCombination.Wave,
            7 => charged ? SamusBeamCombination.Wave : SamusBeamCombination.Plasma,
            8 => SamusBeamCombination.IceWave,
            9 => SamusBeamCombination.SpazerWave,
            10 => charged ? SamusBeamCombination.PlasmaIce : SamusBeamCombination.PlasmaWave,
            11 => charged ? SamusBeamCombination.PlasmaWave : SamusBeamCombination.PlasmaIce,
            var row => throw new InvalidOperationException($"Beam header row {row} is outside twelve."),
        };
        return (charged, beam);
    }

    /// <summary>Maps a beam header to its native base damage and triples charged shots.</summary>
    private static ushort BeamDamage(int header)
    {
        var (charged, beam) = BeamIdentity(header);
        int damage = beam switch
        {
            SamusBeamCombination.Power => 20,
            SamusBeamCombination.Ice => 30,
            SamusBeamCombination.Wave => 50,
            SamusBeamCombination.IceWave => 60,
            SamusBeamCombination.Spazer => 40,
            SamusBeamCombination.SpazerIce => 60,
            SamusBeamCombination.SpazerWave => 70,
            SamusBeamCombination.SpazerIceWave => 100,
            SamusBeamCombination.Plasma => 150,
            SamusBeamCombination.PlasmaIce => 200,
            SamusBeamCombination.PlasmaWave => 250,
            SamusBeamCombination.PlasmaIceWave => 300,
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
                throw new InvalidOperationException($"{beam} has no beam header."),
            _ => throw new ArgumentOutOfRangeException(nameof(header), beam, "Undefined beam combination."),
        };
        return (ushort)(damage * (charged ? 3 : 1));
    }
}
