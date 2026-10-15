using static SuperMetroid.Verification.Enemies.Nuclear.NuclearWaffleProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>#641: the Nuclear Waffle's body typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NuclearWaffleProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNuclearWaffleProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, NuclearWaffleProjectileAnimations.Programs,
            NuclearWaffleProjectileFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Nuclear Waffle body programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
