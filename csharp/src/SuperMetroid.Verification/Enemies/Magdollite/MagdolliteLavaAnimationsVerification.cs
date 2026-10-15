using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteLavaNativeListAddresses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#641: the Magdollite's lava typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class MagdolliteLavaAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMagdolliteLavaAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, MagdolliteLavaAnimations.Programs,
            MagdolliteLavaFrameDefinitions.PresentationKey, MagdolliteLavaCueRules.Rules);
        Console.WriteLine($"  Magdollite lava programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
