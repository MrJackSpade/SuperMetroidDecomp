using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// The native bank-$86 lists the Mother Brain effect ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MotherBrainEffectNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MotherBrainEffectProgram>[] Entries =
    [
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.OnionRing, 0xc432),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.OnionRingHit, 0xc464),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.Bomb, 0xc76e),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.RainbowBeamCharging, 0xc829),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.Drool, 0xc8b0),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.DroolHitFloor, 0xc8e1),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.EscapeDoorParticle, 0xca22),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.PurpleBreathBig, 0xcaa4),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.PurpleBreathSmall, 0xcac8),
        new NativeProgramEntry<MotherBrainEffectProgram>(MotherBrainEffectProgram.EscapeSubtitle, 0xcb0d),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:C432-CB12, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xc432, 0xcb13);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MotherBrainEffectProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
