using static SuperMetroid.Verification.Enemies.PipeBugs.PipeBugNativeListAddresses;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>#641: the Pipe Bugs' typed programs, each matched step by step to its native bank-$B3 list.</summary>
internal static class PipeBugAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPipeBugAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, PipeBugAnimations.Programs,
            PipeBugFrameToolingDefinitions.Spritemap,
            word => null,
            (word, operands, cue, at) => throw new InvalidDataException($"{at}: the lists use no owner instructions."));
        Console.WriteLine($"  Pipe Bug programs: {Entries.Length} programs ({steps} steps) match bank $B3.");
    }
}
