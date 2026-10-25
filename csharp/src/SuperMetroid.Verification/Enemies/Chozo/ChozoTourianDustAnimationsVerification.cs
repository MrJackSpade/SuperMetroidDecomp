using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoTourianDustNativeListAddresses;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>#641: the Wrecked Ship Chozo's and Tourian statue's dust typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class ChozoTourianDustAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, ChozoTourianDustAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(ChozoTourianDustFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyChozoTourianDustAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Chozo/Tourian dust programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
