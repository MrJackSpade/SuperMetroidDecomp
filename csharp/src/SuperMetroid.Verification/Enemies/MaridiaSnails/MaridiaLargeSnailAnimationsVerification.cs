using static SuperMetroid.Verification.Enemies.MaridiaSnails.MaridiaLargeSnailNativeListAddresses;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>#641: Oum's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class MaridiaLargeSnailAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMaridiaLargeSnailAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MaridiaLargeSnailAnimations.Programs,
            MaridiaLargeSnailFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Oum programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
