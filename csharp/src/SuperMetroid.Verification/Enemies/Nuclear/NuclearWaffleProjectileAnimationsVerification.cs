using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>#641: the Nuclear Waffle's body typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NuclearWaffleProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, NuclearWaffleProjectileAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(NuclearWaffleProjectileFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNuclearWaffleProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Nuclear Waffle body programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
