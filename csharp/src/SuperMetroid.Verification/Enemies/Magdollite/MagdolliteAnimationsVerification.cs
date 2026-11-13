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
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    // A sound operand is checked by decoding the native byte to the cue's sound.
                    for (int index = 0; index < operands.Length; index++)
                        AssertEqual<SoundEffectId>(instruction.Operands[index] switch
                    {
                        'V' => cue.Sound ?? throw new InvalidOperationException($"{at}: the sound cue carries no sound."),
                        _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                    }, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[index]), $"{at}: operand {index}");
                }));
        Console.WriteLine($"  Magdollite animations: {Entries.Length} segments ({beats} beats) and {MagdolliteAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
