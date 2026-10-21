using static SuperMetroid.Core.Samus.Projectiles.SamusProjectileDamageDefinitions;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Weapon dispatch to bank-$93 projectile headers; the headers' list fields are typed programs (SamusProjectileHeaderDefinitions).</summary>
internal static class SamusProjectileSelectionDefinitions
{
    /// <summary>$93:83C1 SamusProjectileDataPointers_UnchargedBeams: first of two twelve-word equipment dispatches.</summary>
    private const int BeamSelectors = 0x9383c1;
    /// <summary>$93:83F1 SamusProjectileDataPointers_NonBeam: nine projectile-kind cases.</summary>
    private const int NonBeamSelectors = 0x9383f1;
    /// <summary>$93:8403 SamusProjectileDataPointers_ShinesparkEcho_SpazerSBATrail: eight projectile-kind cases.</summary>
    private const int TrailSelectors = 0x938403;
    /// <summary>$93:8413 SamusProjectileDataPointers_SBA: twelve equipment cases.</summary>
    private const int SpecialSelectors = 0x938413;
    /// <summary>$93:842B SamusProjectileDataPointers_SuperMissileLink: only kind two selects the link.</summary>
    private const int LinkSelectors = 0x93842b;

    /// <summary>Resolves bank-$93 beam, non-beam, trail, special-attack, and header program selectors from compiled dispatch layouts.</summary>
    internal static ushort ReadWord(int address)
    {
        if (address < BeamSelectors || address >= (int)SamusProjectileHeader.Projectile27 + 4 || (address & 1) == 0)
            return Read(address);
        if (address < NonBeamSelectors)
        {
            int selector = (address - BeamSelectors) / 2;
            return BeamHeader(selector >= 12, SamusBeamCombinations.FromTableIndex(selector % 12));
        }
        if (address < TrailSelectors)
            return (ushort)(((address - NonBeamSelectors) / 2) switch
            {
                2 => (int)SamusProjectileHeader.SuperMissile, 3 => (int)SamusProjectileHeader.PowerBomb, 5 => (int)SamusProjectileHeader.Bomb,
                7 => (int)SamusProjectileHeader.BeamExplosion, 8 => (int)SamusProjectileHeader.MissileExplosion, _ => (int)SamusProjectileHeader.Missile,
            });
        if (address < SpecialSelectors)
            return (ushort)(((address - TrailSelectors) / 2) switch
            {
                2 or 4 => (int)SamusProjectileHeader.SpazerSBATrail, 3 => (int)SamusProjectileHeader.Projectile25,
                5 => (int)SamusProjectileHeader.Projectile27, 7 => (int)SamusProjectileHeader.ShinesparkEcho, _ => 0,
            });
        if (address < LinkSelectors)
            return (ushort)(SamusBeamCombinations.FromTableIndex((address - SpecialSelectors) / 2) switch
            {
                SamusBeamCombination.Wave => (int)SamusProjectileHeader.WaveSBA,
                SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave => (int)SamusProjectileHeader.SpazerSBA,
                SamusBeamCombination.Plasma or SamusBeamCombination.PlasmaWave => (int)SamusProjectileHeader.PlasmaSBA,
                SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.IceWave or
                    SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or
                    SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave => 0,
                SamusBeamCombination.SpazerPlasma or
                    SamusBeamCombination.SpazerPlasmaWave or
                    SamusBeamCombination.SpazerPlasmaIce or
                    SamusBeamCombination.SpazerPlasmaIceWave => throw new InvalidOperationException($"The special-attack selector table has no {SamusBeamCombinations.FromTableIndex((address - SpecialSelectors) / 2)} row."),
                var beams => throw new InvalidOperationException($"The special-attack selector table has no {beams} row."),
            });
        if (address < BeamHeaderStart)
            return address == LinkSelectors + 4 ? unchecked((ushort)(int)SamusProjectileHeader.SuperMissileLink) : (ushort)0;
        // Header list fields select typed programs through SamusProjectileHeaderDefinitions; only damage words remain here.
        return IsHeaderDamageWord(address) ? Read(address)
            : throw new InvalidDataException($"Projectile header field ${address:X6} is a list, not a selection word.");
    }

    /// <summary>Whether <paramref name="address"/> is a projectile header's leading damage word.</summary>
    private static bool IsHeaderDamageWord(int address) =>
        address < (int)SamusProjectileHeader.SuperMissileLink ? (address - BeamHeaderStart) % BeamHeaderStride == 0
        : address < (int)SamusProjectileHeader.Projectile25 ? (address - (int)SamusProjectileHeader.SuperMissileLink) % 4 == 0
        : address < (int)SamusProjectileHeader.Projectile27 ? (address - (int)SamusProjectileHeader.Projectile25) % BeamHeaderStride == 0
        : address == (int)SamusProjectileHeader.Projectile27;

    /// <summary>Maps a supported equipment combination to its native charged or uncharged projectile-header row.</summary>
    private static ushort BeamHeader(bool charged, SamusBeamCombination beam)
    {
        int row = beam switch
        {
            SamusBeamCombination.Power => 0,
            SamusBeamCombination.Spazer => 1,
            SamusBeamCombination.SpazerIce => 2,
            SamusBeamCombination.SpazerIceWave => 3,
            SamusBeamCombination.PlasmaIceWave => 4,
            SamusBeamCombination.Ice => 5,
            SamusBeamCombination.Wave => charged ? 7 : 6,
            SamusBeamCombination.Plasma => charged ? 6 : 7,
            SamusBeamCombination.IceWave => 8,
            SamusBeamCombination.SpazerWave => 9,
            SamusBeamCombination.PlasmaWave => charged ? 11 : 10,
            SamusBeamCombination.PlasmaIce => charged ? 10 : 11,
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
                throw new InvalidOperationException($"{beam} has no beam header."),
            _ => throw new ArgumentOutOfRangeException(nameof(beam), beam, "Undefined beam combination."),
        };
        return (ushort)(BeamHeaderStart + (row + (charged ? 12 : 0)) * BeamHeaderStride);
    }
}
