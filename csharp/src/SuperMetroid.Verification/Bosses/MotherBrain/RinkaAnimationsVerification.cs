using static SuperMetroid.Verification.Bosses.MotherBrain.RinkaNativeListAddresses;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>#641: the Rinkas' typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class RinkaAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyRinkaAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, RinkaAnimations.Programs,
            RinkaFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Rinka programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
