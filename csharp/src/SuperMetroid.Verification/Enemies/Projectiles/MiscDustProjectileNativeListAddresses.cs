using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>
/// The native bank-$86 lists the misc dust ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MiscDustProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MiscDustProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.BeamCharge, 0xe0ee),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ElbowChargeParticle, 0xe100),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ElbowChargeEnergy, 0xe11a),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.SmallExplosion, 0xe138),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.RainbowBeamExplosion, 0xe152),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.BeamTrail, 0xe168),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.DudShot, 0xe17e),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.PowerBomb, 0xe198),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ElevatorPad, 0xe1a6),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.SmallDustCloud, 0xe1b0),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.CorpseDustCloud, 0xe1c6),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.EyeDoorSweatDrop, 0xe1d8),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.Smoke, 0xe1ea),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.LoopingElevatorPad, 0xe1fc),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.BigExplosion, 0xe208),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.SmallEnergyDrop, 0xe222),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.BigEnergyDrop, 0xe234),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.Bomb, 0xe246),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.WeirdEnergyDrop, 0xe258),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.RockParticles, 0xe266),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ShortBigDustCloud, 0xe2a8),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ShortBeamCloud, 0xe2ba),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.MediumBeamCloud, 0xe2d4),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.BigDustCloud, 0xe2f2),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.LongBeam, 0xe314),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.FlickeringLongBeam, 0xe392),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.DraygonBreathBubbles, 0xe3a0),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.SaveStationElectricity, 0xe3c6),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ExpandingGate, 0xe3e8),
        new NativeProgramEntry<MiscDustProjectileProgram>(MiscDustProjectileProgram.ContractingGate, 0xe40a),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:E0EE-E42B, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xe0ee, 0xe42c);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MiscDustProjectileProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
