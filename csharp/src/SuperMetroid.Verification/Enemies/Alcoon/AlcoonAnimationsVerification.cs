using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonNativeListAddresses;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>#641: the Alcoon's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class AlcoonAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyAlcoonAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, AlcoonAnimations.Programs,
            AlcoonFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Alcoon programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
