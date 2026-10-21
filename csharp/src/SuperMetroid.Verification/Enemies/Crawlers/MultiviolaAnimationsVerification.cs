using static SuperMetroid.Verification.Enemies.Crawlers.MultiviolaNativeListAddresses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>#641: Multiviola's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class MultiviolaAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMultiviolaAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MultiviolaAnimations.Programs,
            MultiviolaFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Multiviola programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
