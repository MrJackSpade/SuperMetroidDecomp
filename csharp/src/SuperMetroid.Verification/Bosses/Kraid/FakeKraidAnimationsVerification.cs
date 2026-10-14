using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidNativeListAddresses;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>#641: Mini-Kraid's typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class FakeKraidAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFakeKraidAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, FakeKraidAnimations.Programs,
            FakeKraidFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Mini-Kraid programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }
}
