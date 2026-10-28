using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.FallingSparks.FallingSparkNativeListAddresses;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.FallingSparks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.FallingSparks;

/// <summary>#641: the falling spark's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class FallingSparkAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, FallingSparkAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(FallingSparkFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFallingSparkAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  falling spark programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
