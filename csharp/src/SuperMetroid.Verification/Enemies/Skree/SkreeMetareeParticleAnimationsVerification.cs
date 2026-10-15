using static SuperMetroid.Verification.Enemies.Skree.SkreeMetareeParticleNativeListAddresses;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>#641: the Skree's and Metaree's death particles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SkreeMetareeParticleAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySkreeMetareeParticleAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, SkreeMetareeParticleAnimations.Programs,
            SkreeMetareeParticleFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Skree/Metaree particle programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
