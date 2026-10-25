using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Cacatac.CacatacProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>#641: the Cacatac's spike typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class CacatacProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, CacatacProjectileAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(CacatacProjectileFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyCacatacProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Cacatac spike programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
