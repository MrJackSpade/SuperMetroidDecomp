using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>
/// The native bank-$86 lists the misc dust ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MiscDustProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (MiscDustProjectileProgram Program, ushort Entry)[] Entries =
    [
        (MiscDustProjectileProgram.BeamCharge, 0xe0ee),
        (MiscDustProjectileProgram.ElbowChargeParticle, 0xe100),
        (MiscDustProjectileProgram.ElbowChargeEnergy, 0xe11a),
        (MiscDustProjectileProgram.SmallExplosion, 0xe138),
        (MiscDustProjectileProgram.RainbowBeamExplosion, 0xe152),
        (MiscDustProjectileProgram.BeamTrail, 0xe168),
        (MiscDustProjectileProgram.DudShot, 0xe17e),
        (MiscDustProjectileProgram.PowerBomb, 0xe198),
        (MiscDustProjectileProgram.ElevatorPad, 0xe1a6),
        (MiscDustProjectileProgram.SmallDustCloud, 0xe1b0),
        (MiscDustProjectileProgram.CorpseDustCloud, 0xe1c6),
        (MiscDustProjectileProgram.EyeDoorSweatDrop, 0xe1d8),
        (MiscDustProjectileProgram.Smoke, 0xe1ea),
        (MiscDustProjectileProgram.LoopingElevatorPad, 0xe1fc),
        (MiscDustProjectileProgram.BigExplosion, 0xe208),
        (MiscDustProjectileProgram.SmallEnergyDrop, 0xe222),
        (MiscDustProjectileProgram.BigEnergyDrop, 0xe234),
        (MiscDustProjectileProgram.Bomb, 0xe246),
        (MiscDustProjectileProgram.WeirdEnergyDrop, 0xe258),
        (MiscDustProjectileProgram.RockParticles, 0xe266),
        (MiscDustProjectileProgram.ShortBigDustCloud, 0xe2a8),
        (MiscDustProjectileProgram.ShortBeamCloud, 0xe2ba),
        (MiscDustProjectileProgram.MediumBeamCloud, 0xe2d4),
        (MiscDustProjectileProgram.BigDustCloud, 0xe2f2),
        (MiscDustProjectileProgram.LongBeam, 0xe314),
        (MiscDustProjectileProgram.FlickeringLongBeam, 0xe392),
        (MiscDustProjectileProgram.DraygonBreathBubbles, 0xe3a0),
        (MiscDustProjectileProgram.SaveStationElectricity, 0xe3c6),
        (MiscDustProjectileProgram.ExpandingGate, 0xe3e8),
        (MiscDustProjectileProgram.ContractingGate, 0xe40a),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:E0EE-E42B, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xe0ee, 0xe42c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MiscDustProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
