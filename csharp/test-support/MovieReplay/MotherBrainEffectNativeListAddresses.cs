using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Mother Brain effect ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class MotherBrainEffectNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<MotherBrainEffectSegment>[] Entries =
    [
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.OnionRing, 0xc432),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.OnionRingHit, 0xc464),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.Bomb, 0xc76e),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.RainbowBeamCharging, 0xc829),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.Drool, 0xc8b0),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.DroolHitFloor, 0xc8e1),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.EscapeDoorParticle, 0xca22),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.PurpleBreathBig, 0xcaa4),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.PurpleBreathSmall, 0xcac8),
        new NativeProgramEntry<MotherBrainEffectSegment>(MotherBrainEffectSegment.EscapeSubtitle, 0xcb0d),
    ];

    /// <summary>The bank-$86 bytes from the first list to the end of the last, $86:C432-CB12, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xc432, 0xcb13);

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort Entry(MotherBrainEffectSegment program) => NativeEnemyProjectileProgramOracle.EntryOf(Entries, program);

    /// <summary>The family's typed segments and animations beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MotherBrainEffectAnimations.Segments, MotherBrainEffectAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), MotherBrainEffectCueRules.Rules);
}
