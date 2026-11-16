using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Dragon;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Dragon fireball ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class DragonFireballNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<DragonFireballSegment>[] Entries =
    [
        new NativeProgramEntry<DragonFireballSegment>(DragonFireballSegment.RisingLeft, 0xb4bf),
        new NativeProgramEntry<DragonFireballSegment>(DragonFireballSegment.RisingRight, 0xb4cb),
        new NativeProgramEntry<DragonFireballSegment>(DragonFireballSegment.FallingLeft, 0xb4d7),
        new NativeProgramEntry<DragonFireballSegment>(DragonFireballSegment.FallingRight, 0xb4e3),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:B4BF-B4EE, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xb4bf, 0xb4ef);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, DragonFireballAnimations.Segments, DragonFireballAnimations.StartingAt, (frame => EnemyProjectileProgramFrameSources.Operand(frame)), NativeEnemyProjectileCommonCues.Rules);
}
