using static SuperMetroid.Core.Game.SamusProjectileDamageDefinitions;

namespace SuperMetroid.Core.Game;

/// <summary>Weapon dispatch and directional program layout for bank-$93 projectiles.</summary>
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
    /// <summary>$93:86DB InstList_SamusProjectile_Power_Up: eight twelve-byte compass programs.</summary>
    private const ushort PowerProgram = 0x86db;
    /// <summary>$93:873B InstList_SamusProjectile_Wave_IceWave_Up: eight-byte up prefix, then four 132-byte axis programs.</summary>
    private const ushort WaveProgram = 0x873b;
    /// <summary>$93:8953 InstList_SamusProjectile_Ice: shared by all directions.</summary>
    private const ushort IceProgram = 0x8953;
    /// <summary>$93:8977 InstList_SamusProjectile_Spazer_SpazerIce_Up_0: eight 28-byte compass programs.</summary>
    private const ushort SpazerProgram = 0x8977;
    /// <summary>$93:8A57 InstList_SamusProjectile_SpazerWave_SpazerIceWave_Up: eight 84-byte compass programs.</summary>
    private const ushort SpazerWaveProgram = 0x8a57;
    /// <summary>$93:8CF7 InstList_SamusProjectile_Plasma_PlasmaIce_Down_Up_0: four twenty-byte axis programs.</summary>
    private const ushort PlasmaProgram = 0x8cf7;
    /// <summary>$93:8D47 InstList_SamusProjectile_PlasmaIceWave_Down_Up: four 76-byte axis programs, with Wave-only entry after the first frame.</summary>
    private const ushort PlasmaWaveProgram = 0x8d47;
    /// <summary>$93:8E77 InstList_SamusProjectile_Charged_Power_Up: eight twenty-byte compass programs.</summary>
    private const ushort ChargedPowerProgram = 0x8e77;
    /// <summary>$93:8F17 InstList_SamusProjectile_Charged_Wave_Up: up prefix then four 132-byte axis programs.</summary>
    private const ushort ChargedWaveProgram = 0x8f17;
    /// <summary>$93:912F InstList_SamusProjectile_Charged_Ice: shared by all directions.</summary>
    private const ushort ChargedIceProgram = 0x912f;
    /// <summary>$93:9153 InstList_SamusProjectile_Charged_IW_Up: up prefix then four 132-byte axis programs.</summary>
    private const ushort ChargedIceWaveProgram = 0x9153;
    /// <summary>$93:936B InstList_SamusProjectile_Charged_S_SI_Down_Up_0: four 84-byte axis programs.</summary>
    private const ushort ChargedSpazerProgram = 0x936b;
    /// <summary>$93:94BB InstList_SamusProjectile_Charged_SW_SIW_Up_0: eight 196-byte compass programs.</summary>
    private const ushort ChargedSpazerWaveProgram = 0x94bb;
    /// <summary>$93:9ADB InstList_SamusProjectile_Charged_P_PI_Down_Up_0: four 68-byte axis programs.</summary>
    private const ushort ChargedPlasmaProgram = 0x9adb;
    /// <summary>$93:9BEB InstList_SamusProjectile_Charged_PW_PIW_Down_Up_0: four 180-byte axis programs.</summary>
    private const ushort ChargedPlasmaWaveProgram = 0x9beb;
    /// <summary>$93:9EBB InstList_SamusProjectile_Missiles_Up: eight twelve-byte compass programs.</summary>
    private const ushort MissileProgram = 0x9ebb;
    /// <summary>$93:9F1B InstList_SamusProjectile_SuperMissiles_Up: eight twelve-byte compass programs.</summary>
    private const ushort SuperMissileProgram = 0x9f1b;
    /// <summary>$93:9F7B InstList_SamusProjectile_SuperMissileLink: non-directional link program.</summary>
    private const ushort LinkProgram = 0x9f7b;
    /// <summary>$93:9F87 InstList_SamusProjectile_PowerBomb: non-directional Power Bomb program.</summary>
    private const ushort PowerBombProgram = 0x9f87;
    /// <summary>$93:9FBF InstList_SamusProjectile_Bomb: non-directional bomb program.</summary>
    private const ushort BombProgram = 0x9fbf;
    /// <summary>$93:A007 InstList_SamusProjectile_BeamExplosion: beam impact program.</summary>
    private const ushort BeamExplosionProgram = 0xa007;
    /// <summary>$93:A039 InstList_SamusProjectile_MissileExplosion: missile impact program.</summary>
    private const ushort MissileExplosionProgram = 0xa039;
    /// <summary>$93:A06B InstList_SamusProjectile_BombExplosion: bomb explosion program.</summary>
    private const ushort BombExplosionProgram = 0xa06b;
    /// <summary>$93:A095 InstList_SamusProjectile_PlasmaSBA: Plasma special attack program.</summary>
    private const ushort PlasmaSpecialProgram = 0xa095;
    /// <summary>$93:A159 InstList_SamusProjectile_WaveSBA: Wave special attack program.</summary>
    private const ushort WaveSpecialProgram = 0xa159;
    /// <summary>$93:A0C1 InstList_SamusProjectile_SuperMissileExplosion: super missile impact program.</summary>
    private const ushort SuperMissileExplosionProgram = 0xa0c1;
    /// <summary>$93:A0F3 UNUSED_InstList_SamusProjectile_Projectile25_93A0F3: unused kind25 program.</summary>
    private const ushort Projectile25Program = 0xa0f3;
    /// <summary>$93:A13D InstList_SamusProjectile_SpazerSBATrail_0: shared Spazer trail program.</summary>
    private const ushort SpazerTrailProgram = 0xa13d;
    /// <summary>$93:A119 InstList_SamusProjectile_ShinesparkEcho: shared echo program.</summary>
    private const ushort EchoProgram = 0xa119;
    /// <summary>$93:A16D UNUSED_InstList_SamusProjectile_Projectile27_93A16D: unused kind27 program.</summary>
    private const ushort Projectile27Program = 0xa16d;

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
                var beams => throw new InvalidOperationException($"The special-attack selector table has no {beams} row."),
            });
        if (address < BeamHeaderStart)
            return address == LinkSelectors + 4 ? unchecked((ushort)(int)SamusProjectileHeader.SuperMissileLink) : (ushort)0;
        if (address < (int)SamusProjectileHeader.SuperMissileLink)
        {
            int header = (address - BeamHeaderStart) / BeamHeaderStride;
            int field = (address - BeamHeaderStart) % BeamHeaderStride / 2;
            if (field == 0) return Read(address);
            int octant = (field - 1 - (field >= 6 ? 1 : 0)) & 7;
            if (header >= 24)
                return (ushort)((header == 24 ? MissileProgram : SuperMissileProgram) + octant * 12);
            var (charged, beam) = BeamIdentity(header);
            return BeamProgram(charged, beam, octant);
        }
        if (address < (int)SamusProjectileHeader.Projectile25)
        {
            if ((address - (int)SamusProjectileHeader.SuperMissileLink) % 4 == 0) return Read(address);
            // The odd field of each four-byte header is its single program pointer.
            var fourByteHeader = (SamusProjectileHeader)(address - 2);
            if (!Enum.IsDefined(fourByteHeader))
                throw new InvalidDataException($"Projectile selector ${address:X6} is not a four-byte header's program field.");
            return fourByteHeader switch
            {
                SamusProjectileHeader.SuperMissileLink => LinkProgram,
                SamusProjectileHeader.PowerBomb => PowerBombProgram,
                SamusProjectileHeader.Bomb => BombProgram,
                SamusProjectileHeader.BeamExplosion => BeamExplosionProgram,
                SamusProjectileHeader.MissileExplosion => MissileExplosionProgram,
                SamusProjectileHeader.BombExplosion => BombExplosionProgram,
                SamusProjectileHeader.PlasmaSBA => PlasmaSpecialProgram,
                SamusProjectileHeader.WaveSBA => WaveSpecialProgram,
                SamusProjectileHeader.SpazerSBA => SpazerProgram,
                SamusProjectileHeader.SuperMissileExplosion => SuperMissileExplosionProgram,
                SamusProjectileHeader.Missile or SamusProjectileHeader.SuperMissile or
                    SamusProjectileHeader.Projectile25 or SamusProjectileHeader.SpazerSBATrail or
                    SamusProjectileHeader.ShinesparkEcho or SamusProjectileHeader.Projectile27 =>
                    throw new InvalidOperationException($"{fourByteHeader} is not a four-byte header."),
                _ => throw new InvalidOperationException($"Undefined SamusProjectileHeader {fourByteHeader}."),
            };
        }
        if (address < (int)SamusProjectileHeader.Projectile27)
        {
            int header = (address - (int)SamusProjectileHeader.Projectile25) / BeamHeaderStride;
            if ((address - (int)SamusProjectileHeader.Projectile25) % BeamHeaderStride == 0) return Read(address);
            return header switch { 0 => Projectile25Program, 1 => SpazerTrailProgram, _ => EchoProgram };
        }
        return address == (int)SamusProjectileHeader.Projectile27 ? Read(address) : Projectile27Program;
    }

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

    private static ushort BeamProgram(bool charged, SamusBeamCombination beam, int octant)
    {
        int axis = octant & 3;
        bool wave = beam.HasWave;
        bool ice = beam.HasIce;
        if (beam.HasPlasma)
        {
            if (charged)
                return (ushort)((wave ? ChargedPlasmaWaveProgram : ChargedPlasmaProgram) + axis * (wave ? 180 : 68));
            if (!wave) return (ushort)(PlasmaProgram + axis * 20);
            return (ushort)(PlasmaWaveProgram + axis * 76 + (!ice && axis != 2 ? 8 : 0));
        }
        if (beam.HasSpazer)
        {
            if (charged)
                return (ushort)((wave ? ChargedSpazerWaveProgram : ChargedSpazerProgram) + (wave ? octant * 196 : axis * 84));
            return (ushort)((wave ? SpazerWaveProgram : SpazerProgram) + octant * (wave ? 84 : 28));
        }
        if (wave)
        {
            int start = charged ? ice ? ChargedIceWaveProgram : ChargedWaveProgram : WaveProgram;
            return (ushort)(start + (octant == 0 ? 0 : 8 + axis * 132));
        }
        if (ice) return charged ? ChargedIceProgram : IceProgram;
        return (ushort)((charged ? ChargedPowerProgram : PowerProgram) + octant * (charged ? 20 : 12));
    }
}