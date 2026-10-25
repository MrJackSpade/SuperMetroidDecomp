using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Dragon.DragonFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>#641: Dragon's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class DragonFireballAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, DragonFireballAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(DragonFireballFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDragonFireballAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Dragon fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
