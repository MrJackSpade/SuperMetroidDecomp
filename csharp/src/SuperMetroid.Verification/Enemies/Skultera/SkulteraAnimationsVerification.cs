using static SuperMetroid.Verification.Enemies.Skultera.SkulteraNativeListAddresses;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>#641: Skultera's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class SkulteraAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifySkulteraAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, SkulteraAnimations.Programs,
            SkulteraFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Skultera programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
