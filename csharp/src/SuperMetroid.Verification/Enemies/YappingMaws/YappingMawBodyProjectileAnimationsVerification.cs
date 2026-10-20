using static SuperMetroid.Verification.Enemies.YappingMaws.YappingMawBodyProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#641: the Yapping Maw's body links typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class YappingMawBodyProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, YappingMawBodyProjectileAnimations.Programs, (frame => EnemyProjectileFrames.ProgramFrameKey(YappingMawBodyProjectileFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyYappingMawBodyProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Yapping Maw body link programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
