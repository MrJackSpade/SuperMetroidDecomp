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
    internal static readonly (MotherBrainEffectProgram Program, ushort Entry)[] Entries =
    [
        (MotherBrainEffectProgram.OnionRing, 0xc432),
        (MotherBrainEffectProgram.OnionRingHit, 0xc464),
        (MotherBrainEffectProgram.Bomb, 0xc76e),
        (MotherBrainEffectProgram.RainbowBeamCharging, 0xc829),
        (MotherBrainEffectProgram.Drool, 0xc8b0),
        (MotherBrainEffectProgram.DroolHitFloor, 0xc8e1),
        (MotherBrainEffectProgram.EscapeDoorParticle, 0xca22),
        (MotherBrainEffectProgram.PurpleBreathBig, 0xcaa4),
        (MotherBrainEffectProgram.PurpleBreathSmall, 0xcac8),
        (MotherBrainEffectProgram.EscapeSubtitle, 0xcb0d),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:C432-CB12, which production must never read.</summary>
    internal static readonly (ushort First, ushort End) Extent = (0xc432, 0xcb13);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MotherBrainEffectProgram program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);
}
