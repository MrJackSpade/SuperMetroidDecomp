using static SuperMetroid.Verification.Enemies.Rios.NorfairRioNativeListAddresses;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>#641: the Norfair Rio's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class NorfairRioAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNorfairRioAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, NorfairRioAnimations.Programs,
            NorfairRioFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Norfair Rio programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
