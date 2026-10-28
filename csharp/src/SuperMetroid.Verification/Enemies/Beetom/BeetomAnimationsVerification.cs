using static SuperMetroid.Verification.Enemies.Beetom.BeetomNativeListAddresses;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>#641: the Beetom's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class BeetomAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyBeetomAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, BeetomAnimations.Programs,
            BeetomFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Beetom programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
