using static SuperMetroid.Verification.Enemies.Flies.FlyNativeListAddresses;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Flies;

/// <summary>#641: the Mellow, Mella and Memu typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class FlyAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFlyAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, FlyAnimations.Programs,
            FlyFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  fly-family programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
