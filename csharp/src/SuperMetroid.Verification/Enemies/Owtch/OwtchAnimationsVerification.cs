using static SuperMetroid.Verification.Enemies.Owtch.OwtchNativeListAddresses;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>#641: Owtch's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class OwtchAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyOwtchAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, OwtchAnimations.Programs,
            OwtchFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Owtch programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
