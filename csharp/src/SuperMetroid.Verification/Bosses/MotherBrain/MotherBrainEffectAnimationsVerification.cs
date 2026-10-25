using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainEffectNativeListAddresses;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641: Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MotherBrainEffectAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, MotherBrainEffectAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(MotherBrainEffectFrameDefinitions.ShownProjectileFrames[frame])), MotherBrainEffectCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMotherBrainEffectAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Mother Brain effect programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
