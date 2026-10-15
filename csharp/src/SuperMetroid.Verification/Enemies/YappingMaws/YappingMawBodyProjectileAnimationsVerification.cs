using static SuperMetroid.Verification.Enemies.YappingMaws.YappingMawBodyProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#641: the Yapping Maw's body links typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class YappingMawBodyProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyYappingMawBodyProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, YappingMawBodyProjectileAnimations.Programs,
            YappingMawBodyProjectileFrameDefinitions.PresentationKey, NativeEnemyProjectileCommonCues.Rules);
        Console.WriteLine($"  Yapping Maw body link programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
