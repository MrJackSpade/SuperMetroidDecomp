using static SuperMetroid.Verification.Enemies.Rios.LowerNorfairRioNativeListAddresses;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>#641: the Lower Norfair Rio's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class LowerNorfairRioAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyLowerNorfairRioAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, LowerNorfairRioAnimations.Programs,
            LowerNorfairRioFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Lower Norfair Rio programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
