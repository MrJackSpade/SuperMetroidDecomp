using static SuperMetroid.Verification.Enemies.KiHunters.KiHunterNativeListAddresses;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>#641: the Ki-Hunter's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class KiHunterAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyKiHunterAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, KiHunterAnimations.Programs,
            KiHunterFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Ki-Hunter programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
