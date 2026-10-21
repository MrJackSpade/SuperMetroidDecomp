using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Weapon dispatch to Samus projectile headers. The bank-$93 selector tables lie one after another, uncharged beams,
/// charged beams, non-beam kinds, echo and trail kinds, special attacks and the Super Missile link, so an index past one
/// table's end selects from the next.
/// </summary>
internal static class SamusProjectileSelectionDefinitions
{
    /// <summary>Entries in all the selector tables together.</summary>
    private const int EntryCount = 56;

    /// <summary>The header <paramref name="table"/>'s entry <paramref name="index"/> selects.</summary>
    /// <exception cref="InvalidDataException">The entry holds no header, or the index reads outside the tables.</exception>
    internal static SamusProjectileHeader Select(SamusProjectileSelectorTable table, int index) =>
        Entry(table, index) ?? throw new InvalidDataException($"{table} selector {index} holds no projectile header.");

    /// <summary>The header <paramref name="table"/>'s entry <paramref name="index"/> holds, or null for an empty entry.</summary>
    /// <exception cref="InvalidDataException">The index reads outside the tables.</exception>
    internal static SamusProjectileHeader? Entry(SamusProjectileSelectorTable table, int index)
    {
        int position = Start(table) + index;
        if ((uint)position >= EntryCount)
            throw new InvalidDataException($"{table} selector {index} reads outside the projectile selector tables.");
        if (position < 24)
            return BeamHeader(position >= 12, SamusBeamCombinations.FromTableIndex(position % 12));
        position -= 24;
        if (position < 9)
            return position switch
            {
                2 => SamusProjectileHeader.SuperMissile, 3 => SamusProjectileHeader.PowerBomb, 5 => SamusProjectileHeader.Bomb,
                7 => SamusProjectileHeader.BeamExplosion, 8 => SamusProjectileHeader.MissileExplosion, _ => SamusProjectileHeader.Missile,
            };
        position -= 9;
        if (position < 8)
            return position switch
            {
                2 or 4 => SamusProjectileHeader.SpazerSBATrail, 3 => SamusProjectileHeader.Projectile25,
                5 => SamusProjectileHeader.Projectile27, 7 => SamusProjectileHeader.ShinesparkEcho, _ => null,
            };
        position -= 8;
        if (position < 12)
            return SamusBeamCombinations.FromTableIndex(position) switch
            {
                SamusBeamCombination.Wave => SamusProjectileHeader.WaveSBA,
                SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave => SamusProjectileHeader.SpazerSBA,
                SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaWave => SamusProjectileHeader.PlasmaSBA,
                SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.IceWave or
                    SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or
                    SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave => null,
                SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                    SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
                    throw new InvalidOperationException("The special-attack selector table has twelve rows."),
                var beams => throw new ArgumentOutOfRangeException(nameof(table), beams, "Undefined beam combination."),
            };
        return position - 12 == 2 ? SamusProjectileHeader.SuperMissileLink : null;
    }

    /// <summary>Where <paramref name="table"/> starts among all the selector entries.</summary>
    private static int Start(SamusProjectileSelectorTable table) => table switch
    {
        SamusProjectileSelectorTable.UnchargedBeam => 0,
        SamusProjectileSelectorTable.ChargedBeam => 12,
        SamusProjectileSelectorTable.NonBeam => 24,
        SamusProjectileSelectorTable.EchoAndTrail => 33,
        SamusProjectileSelectorTable.SpecialAttack => 41,
        SamusProjectileSelectorTable.SuperMissileLink => 53,
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Undefined projectile selector table."),
    };

    /// <summary>The charged or uncharged header for a supported equipment combination.</summary>
    private static SamusProjectileHeader BeamHeader(bool charged, SamusBeamCombination beam) => beam switch
    {
        SamusBeamCombination.Power => charged ? SamusProjectileHeader.ChargedPower : SamusProjectileHeader.UnchargedPower,
        SamusBeamCombination.Spazer => charged ? SamusProjectileHeader.ChargedSpazer : SamusProjectileHeader.UnchargedSpazer,
        SamusBeamCombination.SpazerIce => charged ? SamusProjectileHeader.ChargedSpazerIce : SamusProjectileHeader.UnchargedSpazerIce,
        SamusBeamCombination.SpazerIceWave => charged ? SamusProjectileHeader.ChargedSpazerIceWave : SamusProjectileHeader.UnchargedSpazerIceWave,
        SamusBeamCombination.PlasmaIceWave => charged ? SamusProjectileHeader.ChargedPlasmaIceWave : SamusProjectileHeader.UnchargedPlasmaIceWave,
        SamusBeamCombination.Ice => charged ? SamusProjectileHeader.ChargedIce : SamusProjectileHeader.UnchargedIce,
        SamusBeamCombination.Wave => charged ? SamusProjectileHeader.ChargedWave : SamusProjectileHeader.UnchargedWave,
        SamusBeamCombination.Plasma => charged ? SamusProjectileHeader.ChargedPlasma : SamusProjectileHeader.UnchargedPlasma,
        SamusBeamCombination.IceWave => charged ? SamusProjectileHeader.ChargedIceWave : SamusProjectileHeader.UnchargedIceWave,
        SamusBeamCombination.SpazerWave => charged ? SamusProjectileHeader.ChargedSpazerWave : SamusProjectileHeader.UnchargedSpazerWave,
        SamusBeamCombination.PlasmaWave => charged ? SamusProjectileHeader.ChargedPlasmaWave : SamusProjectileHeader.UnchargedPlasmaWave,
        SamusBeamCombination.PlasmaIce => charged ? SamusProjectileHeader.ChargedPlasmaIce : SamusProjectileHeader.UnchargedPlasmaIce,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new InvalidOperationException($"{beam} has no beam header."),
        _ => throw new ArgumentOutOfRangeException(nameof(beam), beam, "Undefined beam combination."),
    };
}
