using static SuperMetroid.Verification.Enemies.FallingSparks.FallingSparkNativeListAddresses;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.FallingSparks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.FallingSparks;

/// <summary>#641: the falling spark's typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class FallingSparkAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFallingSparkAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, FallingSparkAnimations.Programs,
            FallingSparkFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  falling spark programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
