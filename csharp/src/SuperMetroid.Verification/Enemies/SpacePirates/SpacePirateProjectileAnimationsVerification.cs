using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.SpacePirates.SpacePirateProjectileNativeListAddresses;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>#641: the laser and claw typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class SpacePirateProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, SpacePirateProjectileAnimations.Programs,
        frame => EnemyProjectileProgramFrameSources.Operand(SpacePirateProjectileFrameDefinitions.ShownProjectileFrames[frame]), SpacePirateProjectileCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySpacePirateProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Pirate laser and claw programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
