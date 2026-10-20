using static SuperMetroid.Verification.Enemies.Skree.SkreeMetareeParticleNativeListAddresses;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>#641: the Skree's and Metaree's death particles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SkreeMetareeParticleAnimationsVerification
{
    /// <summary>The bank-$86 operand of each pose's native frame; it selects the composition the pose's frame draws.</summary>
    private static Dictionary<SkreeMetareeParticleFrame, ushort> NativeOperands { get; } = new()
    {
        [new(0)] = (ushort)SkreeMetareeParticleOperand.Skree,
        [new(1)] = (ushort)SkreeMetareeParticleOperand.Metaree,
    };

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, SkreeMetareeParticleAnimations.Programs, frame => NativeOperands[frame], NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySkreeMetareeParticleAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Skree/Metaree particle programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
