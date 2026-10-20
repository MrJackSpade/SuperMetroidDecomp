using static SuperMetroid.Verification.Enemies.Hoppers.HopperNativeListAddresses;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>#641: the Sidehoppers' and Dessgeegas' typed programs, each matched step by step to its native bank-$A3 list.</summary>
internal static class HopperAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyHopperAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, HopperAnimations.Programs,
            HopperFrameToolingDefinitions.Spritemap,
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
                            AssertEqual((SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, operands[index]), cue.Sound,
                                $"{at}: library-two sound");
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}.");
                    }
                }
            });
        Console.WriteLine($"  Hopper programs: {Entries.Length} programs ({steps} steps) match bank $A3.");
    }
}
