using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteNativeListAddresses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#641, #1600: the Magdollite's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class MagdolliteAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMagdolliteAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MagdolliteAnimations.Segments, MagdolliteAnimations.StartingAt, MagdolliteFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MagdolliteCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    // The sound routine's cue is its operand decoded to the cue's sound; every other routine's cue is fixed.
                    MagdolliteCue expected = instruction.Operands switch
                    {
                        "V" => new MagdolliteCue.PlaySoundIfOnScreen(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[0])),
                        "" => instruction.Cue ?? throw new InvalidOperationException($"{at}: the routine names no cue."),
                        _ => throw new InvalidOperationException($"Undefined operand roles {instruction.Operands}."),
                    };
                    AssertEqual(expected, cue, $"{at}: cue");
                }));
        Console.WriteLine($"  Magdollite animations: {Entries.Length} segments ({beats} beats) and {MagdolliteAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
