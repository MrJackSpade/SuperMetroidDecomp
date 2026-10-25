using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteLavaNativeListAddresses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#641: the Magdollite's lava typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MagdolliteLavaAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MagdolliteLavaAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(MagdolliteLavaFrameDefinitions.ShownProjectileFrames[frame])), MagdolliteLavaCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMagdolliteLavaAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Magdollite lava programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
