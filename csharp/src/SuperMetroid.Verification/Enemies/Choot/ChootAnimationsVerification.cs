using static SuperMetroid.Verification.Enemies.Choot.ChootNativeListAddresses;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Choot;

/// <summary>#641: Choot's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class ChootAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyChootAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, ChootAnimations.Programs,
            ChootFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Choot programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
