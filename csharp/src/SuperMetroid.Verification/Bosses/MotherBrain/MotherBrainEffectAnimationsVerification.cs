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
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMotherBrainEffectAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, MotherBrainEffectAnimations.Programs,
            MotherBrainEffectFrameDefinitions.PresentationKey, MotherBrainEffectCueRules.Rules);
        Console.WriteLine($"  Mother Brain effect programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
