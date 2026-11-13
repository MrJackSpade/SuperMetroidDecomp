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
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            HopperAnimations.Segments, HopperAnimations.StartingAt, HopperFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<HopperCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    for (int index = 0; index < operands.Length; index++)
                    {
                        switch (instruction.Operands[index])
                        {
                            case 'V':
                                AssertEqual((SoundEffectId?)SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[index]), cue.Sound,
                                    $"{at}: library-two sound");
                                break;
                            default:
                                throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}.");
                        }
                    }
                }));
        Console.WriteLine($"  Hopper animations: {Entries.Length} segments ({beats} beats) and {HopperAnimations.StartingAt.Count} animations match bank $A3.");
    }
}
