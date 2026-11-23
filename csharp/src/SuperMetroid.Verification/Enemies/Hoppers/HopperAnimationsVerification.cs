using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Enemies.Hoppers.HopperNativeListAddresses;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>#641, #1600: the Sidehoppers' and Dessgeegas' typed animations, each segment matched to its native bank-$A3 list and each animation to the lists' control flow.</summary>
internal static class HopperAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyHopperAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, Entries,
            HopperAnimations.Segments, HopperAnimations.StartingAt, HopperFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<HopperCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    // The sound routine's cue is its operand decoded to the cue's sound; every other routine's cue is fixed.
                    HopperCue expected = instruction.Operands switch
                    {
                        "V" => new HopperCue.PlaySound(SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, operands[0])),
                        "" => instruction.Cue ?? throw new InvalidOperationException($"{at}: the routine names no cue."),
                        _ => throw new InvalidOperationException($"Undefined operand roles {instruction.Operands}."),
                    };
                    AssertEqual(expected, cue, $"{at}: cue");
                }));
        Console.WriteLine($"  Hopper animations: {Entries.Length} segments ({beats} beats) and {HopperAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
