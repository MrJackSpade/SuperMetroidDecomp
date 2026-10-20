using static SuperMetroid.Verification.Enemies.LavaJumpers.NorfairLavaJumperNativeListAddresses;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.LavaJumpers;

/// <summary>#641: the Norfair lava jumper's typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class NorfairLavaJumperAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNorfairLavaJumperAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, NorfairLavaJumperAnimations.Programs,
            NorfairLavaJumperFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            });
        Console.WriteLine($"  Norfair lava jumper programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
