using static SuperMetroid.Verification.Enemies.MamaTurtles.MamaTurtleNativeListAddresses;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>#641: the turtles' typed programs, each matched step by step to its native bank-$A2 list.</summary>
internal static class MamaTurtleAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMamaTurtleAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MamaTurtleAnimations.Programs,
            MamaTurtleFrameToolingDefinitions.Spritemap, new NativeEnemyCueRules<MamaTurtleCue>(
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                AssertEqual(0, operands.Length, $"{at}: operands");
            },
            cue => cue.Kind is MamaTurtleCueKind.LoopOrTurnAround));
        Console.WriteLine($"  Mama Turtle programs: {Entries.Length} programs ({steps} steps) match bank $A2.");
    }
}
