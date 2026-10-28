using static SuperMetroid.Verification.Enemies.Metroids.MetroidNativeListAddresses;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>#641: the Metroid's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class MetroidAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMetroidAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MetroidAnimations.Programs,
            MetroidFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Metroid programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
