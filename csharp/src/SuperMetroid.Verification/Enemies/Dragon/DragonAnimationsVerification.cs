using static SuperMetroid.Verification.Enemies.Dragon.DragonNativeListAddresses;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>#641: Dragon's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class DragonAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDragonAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, DragonAnimations.Programs,
            DragonFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Dragon programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
