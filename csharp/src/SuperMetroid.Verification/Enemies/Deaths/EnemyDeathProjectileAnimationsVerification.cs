using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Deaths.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>#641: an enemy's death explosion and drop typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EnemyDeathProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, EnemyDeathProjectileAnimations.Programs, (frame => EnemyProjectileFrames.ProgramFrameKey(EnemyDeathProjectileFrameDefinitions.ShownProjectileFrames[frame])), EnemyDeathProjectileCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEnemyDeathProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  enemy death and drop programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
