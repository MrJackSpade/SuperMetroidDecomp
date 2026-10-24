using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteNativeListAddresses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#641: the Magdollite's typed programs, each matched step by step to its native bank-$A8 list.</summary>
internal static class MagdolliteAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyMagdolliteAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, MagdolliteAnimations.Programs,
            MagdolliteFrameToolingDefinitions.Spritemap,
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
                }, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, operands[index]), $"{at}: operand {index}");
            });
        Console.WriteLine($"  Magdollite programs: {Entries.Length} programs ({steps} steps) match bank $A8.");
    }
}
