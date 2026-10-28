using static SuperMetroid.Verification.Enemies.Corpses.DeadTourianNativeListAddresses;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>#641: the dead Sidehopper's and Tourian corpses' typed programs, each matched step by step to its native bank-$A9 list.</summary>
internal static class DeadTourianAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyDeadTourianAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, DeadTourianAnimations.Programs,
            DeadTourianFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  dead Tourian enemy programs: {Entries.Length} programs ({steps} steps) match bank $A9.");
    }
}
