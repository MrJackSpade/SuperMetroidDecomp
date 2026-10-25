using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Projectiles.MiscDustProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#641: the misc dust's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MiscDustProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MiscDustProjectileAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(MiscDustProjectileFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMiscDustProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  misc dust programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
