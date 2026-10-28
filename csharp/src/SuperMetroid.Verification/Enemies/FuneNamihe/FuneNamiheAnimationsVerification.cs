using static SuperMetroid.Verification.Enemies.FuneNamihe.FuneNamiheNativeListAddresses;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#641: the Fune's and Namihe's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class FuneNamiheAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFuneNamiheAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, FuneNamiheAnimations.Programs,
            FuneNamiheFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Fune or Namihe programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
