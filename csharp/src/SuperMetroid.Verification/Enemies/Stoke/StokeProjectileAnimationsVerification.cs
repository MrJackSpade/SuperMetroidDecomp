using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Stoke.StokeProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>#641: the Stoke's projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class StokeProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, StokeProjectileAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(StokeProjectileFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyStokeProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Stoke projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
