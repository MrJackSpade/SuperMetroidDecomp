using static SuperMetroid.Verification.Enemies.Evir.EvirNativeListAddresses;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Evir;

/// <summary>#641: the Evir's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class EvirAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEvirAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, EvirAnimations.Programs,
            EvirFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Evir programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
