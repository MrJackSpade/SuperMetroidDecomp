using static SuperMetroid.Verification.Enemies.Sparks.SparkNativeListAddresses;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Sparks;

/// <summary>#641: the Wrecked Ship Spark's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class SparkAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySparkAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, SparkAnimations.Programs,
            SparkFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Spark programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
