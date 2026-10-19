using static SuperMetroid.Verification.Enemies.Skree.SkreeMetareeNativeListAddresses;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>#641: the Skree's and Metaree's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class SkreeMetareeAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySkreeMetareeAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, SkreeMetareeAnimations.Programs,
            SkreeMetareeFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Skree or Metaree programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
