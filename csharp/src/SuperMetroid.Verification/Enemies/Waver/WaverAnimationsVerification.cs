using static SuperMetroid.Verification.Enemies.Waver.WaverNativeListAddresses;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Enemies.Waver;

/// <summary>#641: Waver's typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class WaverAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyWaverAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, WaverAnimations.Programs,
            WaverFrameToolingDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Waver programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
